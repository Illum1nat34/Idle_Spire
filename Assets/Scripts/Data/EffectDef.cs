using System;
using UnityEngine;

namespace AutoBattlerSpire.Data
{
    public enum EffectKind
    {
        Damage, Block, ApplyStatus,
        Scry, PutToBottom,
        PromoteCost, DemoteCost,
        ShiftSlot, AddToBank,
        CopyLast, AddEchoSlot,
        ModifyCostTemp,
        OverchargeBonus,
        ScryManual
    }

    /// <summary>Табличный атом эффекта. Логика выполнения будет позже.</summary>
    [Serializable]
    public class EffectDef
    {
        public EffectKind Kind;
        public int V1;         // числовой параметр 1 (например урон/блок/X)
        public int V2;         // числовой параметр 2 (если нужен)
        public float F1;       // множитель
        public string RefId;   // строковая ссылка (id статуса и т.п.)
    }
}
