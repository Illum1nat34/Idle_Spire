using UnityEngine;
using AutoBattlerSpire.Data;
using AutoBattlerSpire.Core;

namespace AutoBattlerSpire.Combat
{
    public class CombatRunner : MonoBehaviour
    {
        [Header("Seed")]
        public uint RunSeed = 12345;

        [Header("Test Cards (assign in Inspector)")]
        public CardData[] PlayerCards;
        public CardData[] EnemyCards;

        private CombatContext _ctx;
        private bool _playerTurn = true;

        void Start()
        {
            var rng = new Rng(RunSeed);
            var p = new Fighter("Player", 60, rng);
            var e = new Fighter("Enemy", 45, rng);

            if (PlayerCards != null) p.StartList.AddRange(PlayerCards);
            if (EnemyCards != null) e.StartList.AddRange(EnemyCards);

            p.LoadDeck();
            e.LoadDeck();

            _ctx = new CombatContext(RunSeed, p, e);

            Debug.Log("[CombatRunner] Combat start (Sprint 2).");
            InvokeRepeating(nameof(RunTurn), 0.5f, 1.0f);
        }

        void RunTurn()
        {
            if (IsEnded())
            {
                CancelInvoke();
                return;
            }

            var self = _ctx.Self(_playerTurn);
            var target = _ctx.Target(_playerTurn);
            self.ResetBlock();

            for (int i = 0; i < 3; i++)
            {
                var slot = (Slot)(i + 1);
                _ctx.CurrentSlot = slot;

                var card = self.Deck.DrawTop();
                if (card == null) continue;

                bool canPlay = card.IsAdaptiveFor(slot) || card.CostNow <= slot.Cost();
                if (canPlay)
                {
                    _ctx.OverchargeThisPlay = (slot.Cost() > card.CostNow);
                    if (card.Data.Effects != null)
                    {
                        foreach (var eff in card.Data.Effects)
                        {
                            EffectExecutor.Execute(eff, _ctx, self, target, card);
                        }
                    }
                    Debug.Log($"{self.Name} [{slot}] PLAY {card}  | HP: P{_ctx.Player.Hp} E{_ctx.Enemy.Hp}  Block: P{_ctx.Player.Block} E{_ctx.Enemy.Block}");
                }
                else
                {
                    self.Deck.Discard(card);
                    Debug.Log($"{self.Name} [{slot}] FOLD {card}  | HP: P{_ctx.Player.Hp} E{_ctx.Enemy.Hp}");
                }
            }

            _playerTurn = !_playerTurn;

            if (IsEnded())
            {
                CancelInvoke();
                return;
            }
        }

        bool IsEnded()
        {
            if (_ctx.Player.Hp <= 0) { Debug.Log("[CombatRunner] Enemy wins."); return true; }
            if (_ctx.Enemy.Hp <= 0) { Debug.Log("[CombatRunner] Player wins."); return true; }
            return false;
        }
    }
}
