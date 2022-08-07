using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace YourVRExperience.Game
{
    public abstract class Command
    {
        protected bool m_hasBeenInited = false;

        public abstract void Init();
        public abstract void Execute();

        public abstract bool IsFinished();

        public abstract bool IsBlocking();
        public abstract void Destroy();
    }
}