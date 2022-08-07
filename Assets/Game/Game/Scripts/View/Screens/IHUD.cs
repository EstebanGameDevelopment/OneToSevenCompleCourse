using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourVRExperience.Game
{
    public interface IHUD
    {
        void ActivateHUD(bool _activate);
        void UpdateDisplayScore(int _score);
        void UpdateDisplayLife(int _life);
        void UpdateDisplayTotalAmmo(int _ammo);
        void UpdateDisplayDangerInfo(string _message);
        void UpdateDisplayNPCInfo(string _message);

    }
}