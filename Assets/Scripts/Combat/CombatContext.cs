using AutoBattlerSpire.Core;

namespace AutoBattlerSpire.Combat
{
    public class CombatContext
    {
        public Rng Rng;
        public Fighter Player;
        public Fighter Enemy;

        public Slot CurrentSlot;
        public bool OverchargeThisPlay;

        public CombatContext(uint seed, Fighter player, Fighter enemy)
        {
            Rng = new Rng(seed);
            Player = player;
            Enemy = enemy;
        }

        public Fighter Self(bool playerTurn) => playerTurn ? Player : Enemy;
        public Fighter Target(bool playerTurn) => playerTurn ? Enemy : Player;
    }
}
