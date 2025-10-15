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
        private bool _combatEnded = false;

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
            UI?.SetEndTurnEnabled(true);
            UiLog("Combat start (clean UI)");
        }

        public void OnEndTurnClicked()
        {
            if (_isTurnRunning || _combatEnded) return;
            StartCoroutine(RunTurnRoutine());
        }

        System.Collections.IEnumerator RunTurnRoutine()
        {
            _isTurnRunning = true;

            if (_combatEnded)
            {
                _isTurnRunning = false;
                yield break;
            }

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
                        {
                            if (eff == null) continue;
                            if (eff.Kind == EffectKind.ScryManual && _playerTurn && UI != null && UI.ScryModal != null)
                            {
                                bool done = false;
                                UI.ScryModal.Open(self.Deck, Mathf.Max(1, eff.V1), ok => { done = true; });
                                yield return new WaitUntil(() => done);
                                continue;
                            }

                            EffectExecutor.Execute(eff, _ctx, self, target, card);
                            if (CheckCombatEnd())
                            {
                                UI?.UpdateBars(_ctx.Player, _ctx.Enemy);
                                _isTurnRunning = false;
                                yield break;
                            }
                        }
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
                if (CheckCombatEnd())
                {
                    _isTurnRunning = false;
                    yield break;
                }
                
                yield return new UnityEngine.WaitForSeconds(0.2f);
            }

            _playerTurn = !_playerTurn;
            _isTurnRunning = false;

            if (CheckCombatEnd()) yield break;
        }

        private void UiLog(string line)
        {
            UI?.Log(line);
            Debug.Log(line);
        }

        private bool CheckCombatEnd()
        {
            if (_combatEnded) return true;

            if (_ctx.Player.Hp <= 0)
            {
                FinishCombat($"{_ctx.Enemy.Name} wins.");
                return true;
            }

            if (_ctx.Enemy.Hp <= 0)
            {
                FinishCombat($"{_ctx.Player.Name} wins.");
                return true;
            }

            return false;
        }

        private void FinishCombat(string message)
        {
            if (_combatEnded) return;
            _combatEnded = true;
            UI?.SetEndTurnEnabled(false);
            UiLog(message);
        }
    }
}
