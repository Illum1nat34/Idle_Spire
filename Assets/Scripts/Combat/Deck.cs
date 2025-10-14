using System.Collections.Generic;
using UnityEngine;
using AutoBattlerSpire.Core;

namespace AutoBattlerSpire.Combat
{
    public class Deck
    {
        private readonly List<CardInstance> _draw = new();
        private readonly List<CardInstance> _discard = new();
        private Rng _rng;

        public int DrawCount => _draw.Count;
        public int DiscardCount => _discard.Count;

        public Deck(Rng rng) { _rng = rng; }

        public void LoadFromList(List<AutoBattlerSpire.Data.CardData> list)
        {
            _draw.Clear(); _discard.Clear();
            if (list != null)
            {
                foreach (var cd in list) if (cd != null) _draw.Add(new CardInstance(cd));
            }
            Shuffle(_draw);
        }

        public CardInstance DrawTop()
        {
            if (_draw.Count == 0) Refill();
            if (_draw.Count == 0) return null;
            var c = _draw[0];
            _draw.RemoveAt(0);
            return c;
        }

        public void Discard(CardInstance c)
        {
            if (c != null) _discard.Add(c);
        }

        private void Refill()
        {
            if (_discard.Count == 0) return;
            _draw.AddRange(_discard);
            _discard.Clear();
            Shuffle(_draw);
        }

        private void Shuffle(List<CardInstance> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
