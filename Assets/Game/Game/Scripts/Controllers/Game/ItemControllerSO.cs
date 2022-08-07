using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class ItemControllerSO : ScriptableObject
    {
        private static ItemControllerSO _instance;
        public static ItemControllerSO Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = ScriptableObject.CreateInstance<ItemControllerSO>();
                }
                return _instance;
            }
        }

        private Item[] m_items;


        public void Initialize()
        {
            SystemEventController.Instance.Event += OnSystemEvent;
        }

        public void RegisterCurrentLevelItems()
        {
            m_items = GameObject.FindObjectsOfType<Item>();
        }

        void OnDestroy()
        {
            if (SystemEventController.Instance != null)
            {
                SystemEventController.Instance.Event -= OnSystemEvent;
            }
        }

        public void Destroy()
        {
            if (m_items != null)
            {
                for (int i = 0; i < m_items.Length; i++)
                {
                    if (m_items[i] != null) GameObject.Destroy(m_items[i].gameObject);
                }
                m_items = null;
            }
        }

        private void OnSystemEvent(string _nameEvent, object[] _parameters)
        {
            if (_nameEvent == SystemEventGameController.EVENT_ITEM_COLLECTED)
            {
                ItemTypeSO itemCollected = (ItemTypeSO)_parameters[0];
                ItemEffect avatarCollector = (ItemEffect)_parameters[1];

                if (avatarCollector != null)
                {
                    itemCollected.ApplyItem(avatarCollector);
                }
            }
        }

        public bool HasCollectedAllCoins()
        {
            int totalCoins = 0;
            for (int i = 0; i < m_items.Length; i++)
            {
                if (m_items[i] != null)
                {
                    if (m_items[i].IsCoinItem())
                    {
                        totalCoins++;
                    }
                }
            }

            return totalCoins == 0;
        }
    }
}