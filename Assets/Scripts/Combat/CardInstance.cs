using UnityEngine;
using AutoBattlerSpire.Data;
using AutoBattlerSpire.Core;

namespace AutoBattlerSpire.Combat
{
    public class CardInstance
    {
        public CardData Data;
        public int TempCostMod; // +1/-1 на ход

        public CardInstance(CardData data) { Data = data; }

        public int CostNow => Mathf.Max(0, Data.BaseCost + TempCostMod);
        public bool IsAdaptiveFor(Slot s) => Data.IsAdaptiveFor(s);

        public override string ToString() => $"{Data?.Title} (cost {CostNow})";
    }
}
