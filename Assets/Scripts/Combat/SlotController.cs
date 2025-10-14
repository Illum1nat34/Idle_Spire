namespace AutoBattlerSpire.Combat
{
    /// <summary>
    /// Управляет одноразовым 'сдвигом слота' в ход.
    /// </summary>
    public class SlotController
    {
        public bool ShiftUsedThisTurn { get; private set; }
        public void ResetTurn() => ShiftUsedThisTurn = false;

        public bool TryShift(ref AutoBattlerSpire.Core.Slot current)
        {
            if (ShiftUsedThisTurn) return false;
            if (current == AutoBattlerSpire.Core.Slot.S3) return false;
            current = (AutoBattlerSpire.Core.Slot)((int)current + 1);
            ShiftUsedThisTurn = true;
            return true;
        }
    }
}
