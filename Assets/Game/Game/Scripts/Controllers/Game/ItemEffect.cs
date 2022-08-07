using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourVRExperience.Game
{
    public interface ItemEffect
    {
        void IncreaseLife(int _unitsToIncrease);
        void IncreaseAmmo(int _ammo);
        void AddCoins(int _coins);
    }
}