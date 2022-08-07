using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public interface IPlayer : ICameraPlayer
    {
        int Life { get; }
        int Score { get; }
        bool UseRigidBody { set; }

        void InitLogic();
        void Destroy();
        void SetHUD(IHUD _hud);
        void ResetPlayerLife();
        void ResetPlayerPosition();
        void StopLogic();
        bool IsSelectable();
        void Select(bool _activated);
        void AddScore(int _score);
        void RunCommands();
        bool CheckInsideArea(Vector3 _origin, Vector3 _target);
        void UpdateLogic();
    }
}