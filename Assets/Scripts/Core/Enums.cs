using System;

namespace AutoBattlerSpire.Core
{
    [Flags]
    public enum SlotMask { None=0, S1=1, S2=2, S3=4, S1S2=S1|S2, S2S3=S2|S3, S1S3=S1|S3, All=S1|S2|S3 }

    public enum CardType { Attack, Skill, Power, Status, Curse }
    public enum Rarity { Common, Uncommon, Rare }
    public enum Slot { S1=1, S2=2, S3=3 }

    public static class SlotUtil
    {
        public static int Cost(this Slot s) => s==Slot.S1 ? 1 : s==Slot.S2 ? 2 : 3;

        public static bool MaskContains(this SlotMask mask, Slot s)
        {
            var bit = (SlotMask)(1 << ((int)s - 1));
            return (mask & bit) != 0;
        }
    }
}
