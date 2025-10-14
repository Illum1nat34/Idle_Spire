using UnityEngine;
using AutoBattlerSpire.Data;

namespace AutoBattlerSpire.Core
{
    /// <summary>
    /// Мини-бутафор: проверяет, что проект собран правильно, и логирует результаты.
    /// Никакой боёвки — только smoke-test данных.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Optional Test Refs (assign in Inspector)")]
        public HeroData TestHero;
        public EnemyData TestEnemy;

        [Header("Seed")]
        public uint RunSeed = 12345;

        void Start()
        {
            Debug.Log($"[Bootstrap] Unity {Application.unityVersion}, URP Project OK.");
            var rng = new Rng(RunSeed);
            Debug.Log($"[Bootstrap] RNG test: {rng.Range(0, 10)}, {rng.Range(0, 10)}, {rng.Range(0, 10)}");

            if (TestHero != null)
            {
                Debug.Log($"[Bootstrap] Hero: {TestHero.Title}, start deck: {TestHero.StartDeck.Count}, relics: {TestHero.StartRelics.Count}");
                foreach (var c in TestHero.StartDeck)
                    Debug.Log($"  - Card: {c?.Title} cost {c?.BaseCost} adaptive {c?.AdaptiveMask}");
            }

            if (TestEnemy != null)
            {
                Debug.Log($"[Bootstrap] Enemy: {TestEnemy.Title}, MaxHp {TestEnemy.MaxHp}");
                int total = 0;
                foreach (var e in TestEnemy.Deck) { total += (e?.Count ?? 0); }
                Debug.Log($"  - Enemy deck entries: {TestEnemy.Deck.Count}, total cards: {total}");
            }

            Debug.Log("[Bootstrap] Project OK. (Sprint 1 complete)");
        }
    }
}
