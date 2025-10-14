using System.Collections.Generic;
using UnityEngine;
using AutoBattlerSpire.Data;
using AutoBattlerSpire.Core;
using AutoBattlerSpire.UI;

namespace AutoBattlerSpire.Combat
{
    /// <summary>
    /// CombatRunner читает HP и стартовые колоды из HeroData/EnemyData (если заданы).
    /// Иначе использует fallback: PlayerMaxHp/EnemyMaxHp и массивы CardData из инспектора.
    /// </summary>
    public class CombatRunner : MonoBehaviour
    {
        [Header("Seed")] public uint RunSeed = 12345;

        [Header("Use SO (optional)")]
        public HeroData PlayerHero;     // если задан — берём MaxHp/StartDeck/Title
        public EnemyData EnemyDef;      // если задан — берём MaxHp/Deck/Title

        [Header("Fallback if SO not set")]
        public int PlayerMaxHp = 60;
        public int EnemyMaxHp  = 45;
        public CardData[] PlayerCards;
        public CardData[] EnemyCards;

        [Header("UI")] public CombatUI UI;

        private CombatContext _ctx;
        private bool _playerTurn = true;
        private SlotController _slotCtrl;
        private Scanner _playerScanner;

        private bool _isTurnRunning = false;
        private int _slotIndex = 0;
        private CardInstance _currentCard;

        void Start()
        {
            var rng = new Rng(RunSeed);

            // Build Player
            string pName = "Player";
            int pMax = Mathf.Max(1, PlayerMaxHp);
            List<CardData> pList = new List<CardData>();

            if (PlayerHero != null)
            {
                pName = string.IsNullOrEmpty(PlayerHero.Title) ? pName : PlayerHero.Title;
                if (PlayerHero.MaxHp > 0) pMax = PlayerHero.MaxHp;
                if (PlayerHero.StartDeck != null) pList.AddRange(PlayerHero.StartDeck);
            }
            else if (PlayerCards != null) { pList.AddRange(PlayerCards); }

            var p = new Fighter(pName, pMax, rng);
            p.StartList.AddRange(pList);
            p.LoadDeck();

            // Build Enemy
            string eName = "Enemy";
            int eMax = Mathf.Max(1, EnemyMaxHp);
            List<CardData> eList = new List<CardData>();

            if (EnemyDef != null)
            {
                eName = string.IsNullOrEmpty(EnemyDef.Title) ? eName : EnemyDef.Title;
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
            else if (EnemyCards != null) { eList.AddRange(EnemyCards); }

            var e = new Fighter(eName, eMax, rng);
            e.StartList.AddRange(eList);
            e.LoadDeck();

            _ctx = new CombatContext(RunSeed, p, e);
            _slotCtrl = new SlotController();
            _playerScanner = new Scanner(p.Deck);

            if (UI != null)
            {
                UI.Bind(this);
                UI.SetNames(p.Name, e.Name);
                UI.UpdateBars(p, e);
                UI.Log("Combat start (HP from SO if set)");
            }
        }

        public void OnEndTurnClicked()
        {
            if (_isTurnRunning) return;
            StartCoroutine(RunTurnRoutine());
        }

        public void OnScryClicked(int x)
        {
            if (!_isTurnRunning || !_playerTurn) return;
            var top = _playerScanner.PeekTop(x);
            if (top.Count == 0) return;
            var indexesToBottom = new List<int>();
            var orderedTop = new List<int>();
            var nextSlot = (Slot)(_slotIndex + 1);
            for (int i = 0; i < top.Count; i++)
            {
                bool good = top[i].IsAdaptiveFor(nextSlot) || top[i].CostNow <= nextSlot.Cost();
                if (!good) indexesToBottom.Add(i); else orderedTop.Add(i);
            }
            _playerScanner.ApplyScryResult(indexesToBottom, orderedTop);
            UI?.Log($"Просмотр {x}: вниз {indexesToBottom.Count}, наверху {orderedTop.Count}");
        }

        public void OnShiftClicked()
        {
            if (!_isTurnRunning || !_playerTurn) return;
            var slot = (Slot)(_slotIndex + 1);
            if (_slotCtrl.TryShift(ref slot))
            {
                _slotIndex = (int)slot - 1;
                _ctx.CurrentSlot = slot;
                UI?.Log($"Сдвиг слота: теперь {slot}");
            }
        }

        System.Collections.IEnumerator RunTurnRoutine()
        {
            _isTurnRunning = true;
            _slotCtrl.ResetTurn();

            var self = _ctx.Self(_playerTurn);
            var target = _ctx.Target(_playerTurn);
            self.ResetBlock();

            for (_slotIndex = 0; _slotIndex < 3; _slotIndex++)
            {
                var slot = (Slot)(_slotIndex + 1);
                _ctx.CurrentSlot = slot;

                _currentCard = self.Deck.DrawTop();
                if (_currentCard == null) { UI?.Log($"{self.Name} [{slot}] NO CARD"); continue; }

                float timer = 0.2f;
                while (timer > 0f) { timer -= Time.deltaTime; yield return null; }

                bool canPlay = _currentCard.IsAdaptiveFor(slot) || _currentCard.CostNow <= slot.Cost();
                if (canPlay)
                {
                    _ctx.OverchargeThisPlay = (slot.Cost() > _currentCard.CostNow);
                    if (_currentCard.Data.Effects != null)
                    {
                        foreach (var eff in _currentCard.Data.Effects)
                            EffectExecutor.Execute(eff, _ctx, self, target, _currentCard);
                    }
                    self.Deck.Discard(_currentCard);
                    UI?.Log($"{self.Name} [{slot}] PLAY {_currentCard}");
                }
                else
                {
                    self.Deck.Discard(_currentCard);
                    UI?.Log($"{self.Name} [{slot}] FOLD {_currentCard}");
                }

                UI?.UpdateBars(_ctx.Player, _ctx.Enemy);
                yield return new UnityEngine.WaitForSeconds(0.15f);
            }

            _playerTurn = !_playerTurn;
            _isTurnRunning = false;

            if (_ctx.Player.Hp <= 0) UI?.Log("Enemy wins.");
            if (_ctx.Enemy .Hp <= 0) UI?.Log("Player wins.");
        }
    }
}
