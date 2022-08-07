using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourVRExperience.Game
{
    [CreateAssetMenu(menuName = "Game/Item Ammo")]
    public class ItemAmmoSO : ItemTypeSO
    {
        public int Ammo;
        public override void ApplyItem(ItemEffect _actor)
        {
            _actor.IncreaseAmmo(Ammo);
        }
    }
}