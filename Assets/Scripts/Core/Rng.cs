using UnityEngine;

namespace AutoBattlerSpire.Core
{
    /// <summary>Детерминированный RNG (xorshift32) с сериализуемым состоянием.</summary>
    public struct Rng
    {
        private uint _state;
        public Rng(uint seed) { _state = seed == 0 ? 2463534242u : seed; }

        private uint NextU()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            uint span = (uint)(maxExclusive - minInclusive);
            return (int)(NextU() % span) + minInclusive;
        }

        public float Value01() => (NextU() & 0xFFFFFF) / (float)0x1000000;
        public uint State => _state;
    }
}
