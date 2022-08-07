using System;
using System.Collections.Generic;
using UnityEngine;
using YourVRExperience.Networking;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class EnemySpawner : MonoBehaviour
    {
        public Enemy EnemyPrefab;
        public GameObject Origin;
        public GameObject Target;
        public float Frequency = 5;

        private float m_timeAppear = 0;
        private Vector3 m_origin;
        private Vector3 m_target;

        private Enemy m_enemy = null;

        private List<string> m_enemiesNames = new List<string>();

        void Start()
        {
            m_origin = Origin.transform.position;
            m_target = Target.transform.position;
            GameObject.Destroy(Origin);
            GameObject.Destroy(Target);
            m_timeAppear = Frequency;

            SystemEventController.Instance.Event += OnSystemEvent;
        }

        void OnDestroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
        }

        private void OnSystemEvent(string _nameEvent, object[] _parameters)
        {
            if (_nameEvent == SystemEventGameController.EVENT_ENEMY_HAS_BEEN_STARTED)
            {
                Enemy enemy = (Enemy)_parameters[0];
                if (m_enemiesNames.Remove(enemy.gameObject.name))
                {
                    m_enemy = enemy;
                    InitEnemy();
                }
            }
        }

        private void InitEnemy()
        {
            m_enemy.ActivateWait = true;
            m_enemy.transform.position = m_origin;
            CmdMoveTo moveTo = new CmdMoveTo(m_enemy, m_target, 20, true);
            m_enemy.AddCommand(moveTo);
        }

        public void UpdateLogic()
        {
            m_timeAppear += Time.deltaTime;
            if (m_timeAppear > Frequency)
            {
                m_timeAppear = 0;
                if (!GameController.Instance.IsMultiplayerGame)
                {
                    m_enemy = Instantiate(EnemyPrefab) as Enemy;
                }
                else
                {
                    m_enemiesNames.Add(NetworkController.Instance.CreateNetworkPrefab(EnemyPrefab.PrefabName, EnemyPrefab.gameObject, "Prefabs\\Avatars\\" + EnemyPrefab.name, m_origin, Quaternion.identity, 0, Enemy.ENEMY_ANIMATION.ANIMATION_RUN));
                }
            }
        }
    }
}