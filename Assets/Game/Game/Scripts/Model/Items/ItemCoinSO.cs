using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourVRExperience.Game
{
    [CreateAssetMenu(menuName = "Game/Item Coin")]
    public class ItemCoinSO : ItemTypeSO
    {
        public int Coins;
        public override void ApplyItem(ItemEffect _actor)
        {
            _actor.AddCoins(Coins);
        }
    }
}