using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourVRExperience.Game
{
    [CreateAssetMenu(menuName = "Game/Item Life")]
    public class ItemLifeSO : ItemTypeSO
    {
        public int Life;

        public override void ApplyItem(ItemEffect _actor)
        {
            _actor.IncreaseLife(Life);
        }
    }
}