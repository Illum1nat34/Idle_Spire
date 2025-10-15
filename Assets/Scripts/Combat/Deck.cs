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

        public List<CardInstance> PeekTop(int x)
        {
            int count = Mathf.Min(Mathf.Max(0, x), _draw.Count);
            var result = new List<CardInstance>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(_draw[i]);
            }
            return result;
        }

        public void ApplyScry(List<int> toBottom, List<int> orderedTop)
        {
            int requestedTop = orderedTop != null ? orderedTop.Count : 0;
            int requestedBottom = toBottom != null ? toBottom.Count : 0;
            int total = Mathf.Min(requestedTop + requestedBottom, _draw.Count);

            if (total <= 0)
            {
                return;
            }

            var slice = new List<CardInstance>(total);
            for (int i = 0; i < total; i++)
            {
                var card = _draw[0];
                _draw.RemoveAt(0);
                slice.Add(card);
            }

            var used = new bool[slice.Count];
            var topList = new List<CardInstance>();

            if (orderedTop != null)
            {
                foreach (var idx in orderedTop)
                {
                    if (idx < 0 || idx >= slice.Count) continue;
                    if (used[idx]) continue;
                    topList.Add(slice[idx]);
                    used[idx] = true;
                }
            }

            var bottomList = new List<CardInstance>();

            if (toBottom != null)
            {
                foreach (var idx in toBottom)
                {
                    if (idx < 0 || idx >= slice.Count) continue;
                    if (used[idx]) continue;
                    bottomList.Add(slice[idx]);
                    used[idx] = true;
                }
            }

            for (int i = 0; i < slice.Count; i++)
            {
                if (!used[i])
                {
                    topList.Add(slice[i]);
                }
            }

            if (topList.Count > 0)
            {
                _draw.InsertRange(0, topList);
            }

            if (bottomList.Count > 0)
            {
                _draw.AddRange(bottomList);
            }
        }
    }
}
