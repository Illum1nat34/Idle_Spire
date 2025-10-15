using System.Collections.Generic;
using UnityEngine;
using AutoBattlerSpire.Data;
using AutoBattlerSpire.Core;
using AutoBattlerSpire.UI;

namespace AutoBattlerSpire.Combat
{
    public class CombatRunner : MonoBehaviour
    {
        [Header("Seed")] public uint RunSeed = 12345;

        [Header("Use SO (optional)")]
        public HeroData PlayerHero;
        public EnemyData EnemyDef;

        [Header("Fallback if SO not set")]
        public int PlayerMaxHp = 60;
        public int EnemyMaxHp  = 45;
        public CardData[] PlayerCards;
        public CardData[] EnemyCards;

        [Header("UI")] public CombatUI UI;

        private CombatContext _ctx;
        private bool _playerTurn = true;
        private bool _isTurnRunning = false;

        void Start()
        {
            var rng = new Rng(RunSeed);

            string pName = "Player";
            int pMax = Mathf.Max(1, PlayerMaxHp);
            var pList = new List<CardData>();
            if (PlayerHero != null)
            {
                if (!string.IsNullOrEmpty(PlayerHero.Title)) pName = PlayerHero.Title;
                if (PlayerHero.MaxHp > 0) pMax = PlayerHero.MaxHp;
                if (PlayerHero.StartDeck != null) pList.AddRange(PlayerHero.StartDeck);
            }
            else if (PlayerCards != null) pList.AddRange(PlayerCards);

            var p = new Fighter(pName, pMax, rng);
            p.StartList.AddRange(pList);
            p.LoadDeck();

            string eName = "Enemy";
            int eMax = Mathf.Max(1, EnemyMaxHp);
            var eList = new List<CardData>();
            if (EnemyDef != null)
            {
                if (!string.IsNullOrEmpty(EnemyDef.Title)) eName = EnemyDef.Title;
                if (EnemyDef.MaxHp > 0) eMax = EnemyDef.MaxHp;
                if (EnemyDef.Deck != null)
                {
                    foreach (var entry in EnemyDef.Deck)
                    {
                        if (entry == null || entry.Card == null) continue;
                        int count = Mathf.Max(1, entry.Count);
                        for (int i = 0; i < count; i++) eList.Add(entry.Card);
                    }
                }
            }
            else if (EnemyCards != null) eList.AddRange(EnemyCards);

            var e = new Fighter(eName, eMax, rng);
            e.StartList.AddRange(eList);
            e.LoadDeck();

            _ctx = new CombatContext(RunSeed, p, e);

            UI?.Bind(this);
            UI?.SetNames(p.Name, e.Name);
            UI?.UpdateBars(p, e);
            UiLog("Combat start (clean UI)");
        }

        public void OnEndTurnClicked()
        {
            if (_isTurnRunning) return;
            StartCoroutine(RunTurnRoutine());
        }

        System.Collections.IEnumerator RunTurnRoutine()
        {
            _isTurnRunning = true;

            var self = _ctx.Self(_playerTurn);
            var target = _ctx.Target(_playerTurn);
            self.ResetBlock();

            for (int slotIndex = 0; slotIndex < 3; slotIndex++)
            {
                var slot = (Slot)(slotIndex + 1);
                _ctx.CurrentSlot = slot;

                var card = self.Deck.DrawTop();
                if (card == null) { UiLog($"{self.Name} [{slot}] NO CARD"); continue; }

                bool canPlay = card.IsAdaptiveFor(slot) || card.CostNow <= slot.Cost();
                if (canPlay)
                {
                    _ctx.OverchargeThisPlay = (slot.Cost() > card.CostNow);
                    if (card.Data.Effects != null)
                    {
                        foreach (var eff in card.Data.Effects)
                            EffectExecutor.Execute(eff, _ctx, self, target, card);
                    }
                    self.Deck.Discard(card);
                    UiLog($"{self.Name} [{slot}] PLAY {card}");
                }
                else
                {
                    self.Deck.Discard(card);
                    UiLog($"{self.Name} [{slot}] FOLD {card}");
                }

                UI?.UpdateBars(_ctx.Player, _ctx.Enemy);
                yield return new UnityEngine.WaitForSeconds(0.2f);
            }

            _playerTurn = !_playerTurn;
            _isTurnRunning = false;

            if (_ctx.Player.Hp <= 0) UiLog("Enemy wins.");
            if (_ctx.Enemy .Hp <= 0) UiLog("Player wins.");
        }

        private void UiLog(string line)
        {
            UI?.Log(line);
            Debug.Log(line);
        }
    }
}
