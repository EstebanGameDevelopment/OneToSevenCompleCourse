using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class Spikes : MonoBehaviour
    {
        public int DamageLife = 10;

        private PatrolWaypoints m_patrolWaypoints;

        void Start()
        {
            m_patrolWaypoints = this.GetComponent<PatrolWaypoints>();

            if (m_patrolWaypoints != null)
            {
                m_patrolWaypoints.ActivatePatrol(5);
            }

            SystemEventController.Instance.Event += ProcessSystemEvent;
        }

        void OnDestroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= ProcessSystemEvent;
        }

        private void ProcessSystemEvent(string _nameEvent, object[] _parameters)
        {
            if (_nameEvent == SystemEventGameController.EVENT_ENEMY_DEAD)
            {
                Debug.Log("<color=blue>Spikes has received the event of Enemy Dead!!!</color>");
                this.gameObject.transform.localScale += new Vector3(1f, 1f, 1f);
            }
            if (_nameEvent == SystemEventGameController.EVENT_NPC_DEAD)
            {
                Debug.Log("<color=blue>Spikes has received the event of NPC Dead!!!</color>");
                this.gameObject.transform.localScale -= new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
    }
}