using System.Collections.Generic;
using UnityEngine;

namespace AutoBattlerSpire.Combat
{
    /// <summary>
    /// Дополняет Deck методами для Просмотра/Scry.
    /// </summary>
    public static class DeckExtensions
    {
        public static List<CardInstance> PeekTop(this Deck deck, int x)
        {
            var field = typeof(Deck).GetField("_draw", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = field.GetValue(deck) as System.Collections.IList;
            var res = new List<CardInstance>();
            int count = Mathf.Min(x, list.Count);
            for (int i = 0; i < count; i++) res.Add((CardInstance)list[i]);
            return res;
        }

        public static void ApplyScry(this Deck deck, System.Collections.Generic.List<int> indexesToBottom, System.Collections.Generic.List<int> orderedTopIndexes)
        {
            var field = typeof(Deck).GetField("_draw", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var draw = field.GetValue(deck) as System.Collections.IList;

            int n = indexesToBottom.Count + orderedTopIndexes.Count;
            var buffer = new System.Collections.Generic.List<CardInstance>();
            for (int i = 0; i < n && draw.Count > 0; i++)
            {
                buffer.Add((CardInstance)draw[0]);
                draw.RemoveAt(0);
            }

            // вниз колоды
            for (int i = 0; i < indexesToBottom.Count; i++)
            {
                int idx = indexesToBottom[i];
                if (idx >= 0 && idx < buffer.Count) draw.Add(buffer[idx]);
            }

            // обратно на верх в указанном порядке
            for (int i = 0; i < orderedTopIndexes.Count; i++)
            {
                int idx = orderedTopIndexes[i];
                if (idx >= 0 && idx < buffer.Count) draw.Insert(i, buffer[idx]);
            }
        }
    }
}
