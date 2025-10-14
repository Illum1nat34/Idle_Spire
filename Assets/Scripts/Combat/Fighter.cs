using System.Collections.Generic;
using UnityEngine;
using AutoBattlerSpire.Core;
using AutoBattlerSpire.Data;

namespace AutoBattlerSpire.Combat
{
    public class Fighter
    {
        public string Name;
        public int MaxHp;
        public int Hp;
        public int Block;

        public Deck Deck;
        public List<CardData> StartList = new();

        public Fighter(string name, int maxHp, Rng rng)
        {
            Name = name; MaxHp = maxHp; Hp = maxHp;
            Deck = new Deck(rng);
        }

        public void LoadDeck() => Deck.LoadFromList(StartList);

        public void TakeDamage(int amount)
        {
            int dmg = Mathf.Max(0, amount - Block);
            Block = Mathf.Max(0, Block - amount);
            Hp = Mathf.Max(0, Hp - dmg);
        }

        public void AddBlock(int amount) { Block += amount; }
        public void ResetBlock() { Block = 0; }
    }
}
