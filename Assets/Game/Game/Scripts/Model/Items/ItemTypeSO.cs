using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourVRExperience.Game
{
    public abstract class ItemTypeSO : ScriptableObject
    {
        public string Name;
        public Vector3 Size;

        public virtual void Init(GameObject _go)
        {
            _go.name = Name;
            _go.transform.localScale = Size;
        }

        public abstract void ApplyItem(ItemEffect _actor);
    }
}