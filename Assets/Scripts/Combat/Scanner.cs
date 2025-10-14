using System.Collections.Generic;

namespace AutoBattlerSpire.Combat
{
    /// <summary>
    /// Просмотр (Scry): позволяет заглядывать на верх колоды и перекладывать часть вниз.
    /// Версия Sprint 3 — используется кнопкой 'Просмотр 2' между слотами.
    /// </summary>
    public class Scanner
    {
        private Deck _deck;
        public Scanner(Deck deck) { _deck = deck; }

        public System.Collections.Generic.List<CardInstance> PeekTop(int x)
        {
            return _deck.PeekTop(x);
        }

        public void ApplyScryResult(System.Collections.Generic.List<int> indexesToBottom, System.Collections.Generic.List<int> orderedTopIndexes)
        {
            _deck.ApplyScry(indexesToBottom, orderedTopIndexes);
        }
    }
}
