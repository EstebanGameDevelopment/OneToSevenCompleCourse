using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class Item : MonoBehaviour
    {
        [SerializeField] private ItemTypeSO ItemType;

        void Start()
        {
            ItemType.Init(this.gameObject);
        }

        public bool IsCoinItem()
        {
            return ItemType is ItemCoinSO;
        }

        void OnTriggerEnter(Collider _collision)
        {
            if (_collision != null)
            {
                if (_collision.gameObject != null)
                {
                    if (_collision.gameObject.GetComponent<ItemEffect>() != null)
                    {
                        SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_ITEM_COLLECTED, ItemType, _collision.gameObject.GetComponent<ItemEffect>());
                        GameObject.Destroy(this.gameObject);
                    }
                }
            }
        }
    }
}