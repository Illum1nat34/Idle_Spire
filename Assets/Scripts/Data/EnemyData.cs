using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoBattlerSpire.Data
{
    [Serializable]
    public class EnemyDeckEntry
    {
        public CardData Card;
        [Min(1)] public int Count = 1;
    }

    [CreateAssetMenu(fileName="Enemy_", menuName="Game/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public string Title;
        [TextArea] public string Lore;

        [Header("Stats")]
        public int MaxHp = 40;

        [Header("Deck")]
        public List<EnemyDeckEntry> Deck = new();

        [Header("Curve (notes only)")]
        [Range(0,100)] public int WeightCost1 = 40;
        [Range(0,100)] public int WeightCost2 = 35;
        [Range(0,100)] public int WeightCost3 = 25;

        [Header("Features (passives/tricks)")]
        public string[] Features; // просто описания; реализация далее
    }
}
