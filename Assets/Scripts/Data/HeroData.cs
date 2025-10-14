using System.Collections.Generic;
using UnityEngine;

namespace AutoBattlerSpire.Data
{
    [CreateAssetMenu(fileName="Hero_", menuName="Game/Hero")]
    public class HeroData : ScriptableObject
    {
        public string Id;
        public string Title;
        [TextArea] public string Description;

        [Header("Start Deck")]
        public List<CardData> StartDeck = new();

        [Header("Start Relics")]
        public List<RelicData> StartRelics = new();
    }
}
