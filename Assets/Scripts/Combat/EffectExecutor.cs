using UnityEngine;
using AutoBattlerSpire.Data;

namespace AutoBattlerSpire.Combat
{
    public static class EffectExecutor
    {
        public static void Execute(EffectDef e, CombatContext ctx, Fighter caster, Fighter target, CardInstance source)
        {
            switch (e.Kind)
            {
                case EffectKind.Damage:
                {
                    int val = ScaleBySlot(e.V1, source, ctx.CurrentSlot);
                    target.TakeDamage(val);
                    break;
                }
                case EffectKind.Block:
                {
                    int val = ScaleBySlot(e.V1, source, ctx.CurrentSlot);
                    caster.AddBlock(val);
                    break;
                }
                default:
                    Debug.Log($"[EffectExecutor] '{e.Kind}' is not implemented in Sprint 2.");
                    break;
            }
        }

        static int ScaleBySlot(int baseVal, CardInstance c, AutoBattlerSpire.Core.Slot slot)
        {
            float m = c.Data.SlotMult(slot);
            return Mathf.RoundToInt(baseVal * m);
        }
    }
}
