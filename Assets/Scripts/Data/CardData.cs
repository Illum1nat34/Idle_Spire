using UnityEngine;
using AutoBattlerSpire.Core;

namespace AutoBattlerSpire.Data
{
    [CreateAssetMenu(fileName="Card_", menuName="Game/Card")]
    public class CardData : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public string Title;
        [TextArea] public string Description;

        [Header("Meta")]
        public CardType Type = CardType.Attack;
        public Rarity Rarity = Rarity.Common;

        [Header("Cost & Adaptive")]
        [Range(1,3)] public int BaseCost = 1;
        public SlotMask AdaptiveMask = SlotMask.None; 
        public float S1Mult = 1f, S2Mult = 1f, S3Mult = 1f; 

        [Header("Effects")]
        public EffectDef[] Effects;

        [Header("Upgrade")]
        public CardData UpgradeTo;

        public bool IsAdaptiveFor(Slot slot) => AdaptiveMask.MaskContains(slot);
        public float SlotMult(Slot slot) => slot==Slot.S1? S1Mult : slot==Slot.S2? S2Mult : S3Mult;
    }
}
