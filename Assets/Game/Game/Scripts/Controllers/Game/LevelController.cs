#if ENABLE_PHOTON
using Photon.Pun;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YourVRExperience.Networking;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class LevelController : MonoBehaviour
    {
        private static LevelController _instance;
        public static LevelController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType<LevelController>();
                }
                return _instance;
            }
        }

        public GameObject EnemyGroup;
        public GameObject NPCGroup;

        private List<Enemy> m_enemies = new List<Enemy>();
        private List<NPC> m_NPCs = new List<NPC>();
        private List<Bullet> m_bullets = new List<Bullet>();
        private List<PatrolWaypoints> m_patrolWaypoints = new List<PatrolWaypoints>();
        private List<AreaVisionDetection> m_areaVisionDetection = new List<AreaVisionDetection>();
        private List<RotateToTarget> m_rotateToTarget = new List<RotateToTarget>();

        private EnemySpawner[] m_enemySpawner;

        private List<AvatarData> m_dataEnemies = new List<AvatarData>();
        private List<AvatarData> m_dataNPCs = new List<AvatarData>();

        private bool m_hasBeenInited = false;

        private Dictionary<string,AvatarData> m_instantiatedEnemies = new Dictionary<string, AvatarData>();
        private Dictionary<string, AvatarData> m_instantiatedNPCS = new Dictionary<string, AvatarData>();

        public List<Enemy> Enemies
        {
            get { return m_enemies; }
        }

        public List<NPC> NPCs
        {
            get { return m_NPCs; }
        }

        void Awake()
        {
            if (!GameController.Instance.IsMultiplayerGame)
            {
                EnemyGroup.SetActive(true);
                NPCGroup.SetActive(true);
            }
            else
            {
                if (!NetworkController.Instance.IsServer)
                {
                    GameObject.Destroy(EnemyGroup);
                    GameObject.Destroy(NPCGroup);
                }
                else
                {
                    if (EnemyGroup && NPCGroup)
                    {
                        EnemyGroup.SetActive(true);
                        NPCGroup.SetActive(true);

                        Enemy[] networkEnemies = GameObject.FindObjectsOfType<Enemy>();
                        NPC[] networkNPCs = GameObject.FindObjectsOfType<NPC>();

                        for (int i = 0; i < networkEnemies.Length; i++)
                        {
                            Enemy enemy = networkEnemies[i];
                            if (enemy != null)
                            {
                                m_dataEnemies.Add(new AvatarData(enemy));
                                try { GameObject.Destroy(enemy.gameObject); } catch (Exception err) { }
                            }
                        }
                        for (int i = 0; i < networkNPCs.Length; i++)
                        {
                            NPC npc = networkNPCs[i];
                            if (npc != null)
                            {
                                m_dataNPCs.Add(new AvatarData(npc));
                                try { GameObject.Destroy(npc.gameObject); } catch (Exception err) { }
                            }
                        }
                    }
                }
            }
        }

        void Start()
        {
            m_enemySpawner = GameObject.FindObjectsOfType<EnemySpawner>();

            ItemControllerSO.Instance.RegisterCurrentLevelItems();

            SystemEventController.Instance.Event += OnSystemEvent;

            if (!GameController.Instance.IsMultiplayerGame)
            {
                Enemy[] networkEnemies = GameObject.FindObjectsOfType<Enemy>();
                NPC[] networkNPC = GameObject.FindObjectsOfType<NPC>();

                for (int i = 0; i < networkEnemies.Length; i++)
                {
                    if (networkEnemies[i].GetComponent<NetworkGameID>() != null) networkEnemies[i].GetComponent<NetworkGameID>().SetEnabled(false);
                }
                for (int i = 0; i < networkNPC.Length; i++)
                {
                    if (networkNPC[i].GetComponent<NetworkGameID>() != null) networkNPC[i].GetComponent<NetworkGameID>().SetEnabled(false);
                }
            }
            else
            {
                if (NetworkController.Instance.IsServer)
                {
                    for (int i = 0; i < m_dataEnemies.Count; i++)
                    {
                        AvatarData avatarData = m_dataEnemies[i];
                        m_instantiatedEnemies.Add(NetworkController.Instance.CreateNetworkPrefab(avatarData.PrefabName, null, "Prefabs\\Avatars\\" + avatarData.PrefabName, avatarData.Position, Quaternion.identity, 0, Enemy.ENEMY_ANIMATION.ANIMATION_IDLE), avatarData);
                    }

                    for (int i = 0; i < m_dataNPCs.Count; i++)
                    {
                        AvatarData avatarData = m_dataNPCs[i];
                        m_instantiatedNPCS.Add(NetworkController.Instance.CreateNetworkPrefab(avatarData.PrefabName, null, "Prefabs\\Avatars\\" + avatarData.PrefabName, avatarData.Position, Quaternion.identity, 0, NPC.NPC_ANIMATION.ANIMATION_IDLE), avatarData);
                    }
                }
            }

            Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
            m_enemies.AddRange(enemies);

            NPC[] npcs = GameObject.FindObjectsOfType<NPC>();
            m_NPCs.AddRange(npcs);

            m_hasBeenInited = true;
        }

        public void Destroy()
        {
            if (_instance != null)
            {
                _instance = null;
                ItemControllerSO.Instance.Destroy();
                for (int i = 0; i < m_enemies.Count; i++)
                {
                    if (m_enemies[i] != null) m_enemies[i].Destroy();
                }
                for (int i = 0; i < m_NPCs.Count; i++)
                {
                    if (m_NPCs[i] != null) m_NPCs[i].Destroy();
                }
                for (int i = 0; i < m_bullets.Count; i++)
                {
                    GameObject.Destroy(m_bullets[i].gameObject);
                }
                if (m_enemySpawner != null)
                {
                    for (int i = 0; i < m_enemySpawner.Length; i++)
                    {
                        if (m_enemySpawner[i] != null) GameObject.Destroy(m_enemySpawner[i]);
                    }
                }
                GameObject.Destroy(this.gameObject);

                SystemEventController.Instance.Event -= OnSystemEvent;
            }
        }

        private void OnSystemEvent(string _nameEvent, object[] _parameters)
        {
            if (_nameEvent == PatrolWaypoints.EVENT_PATROLWAYPOINTS_HAS_STARTED)
            {
                PatrolWaypoints newPatrolWaypoints = (PatrolWaypoints)_parameters[0];
                if ((newPatrolWaypoints != null) && (!m_patrolWaypoints.Contains(newPatrolWaypoints)))
                {
                    newPatrolWaypoints.MasksToIgnore = new string[] { GameController.LAYER_PLAYER, GameController.LAYER_ENEMY, GameController.LAYER_NPC };
                    m_patrolWaypoints.Add(newPatrolWaypoints);
                }
            }
            if (_nameEvent == PatrolWaypoints.EVENT_PATROLWAYPOINTS_HAS_BEEN_DESTROYED)
            {
                if (m_patrolWaypoints.Remove((PatrolWaypoints)_parameters[0]))
                {
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_AREAVISIONDETECTED_HAS_STARTED)
            {
                AreaVisionDetection newAreaVisionDetection = (AreaVisionDetection)_parameters[0];
                if ((newAreaVisionDetection != null) && (!m_areaVisionDetection.Contains(newAreaVisionDetection)))
                {
                    m_areaVisionDetection.Add(newAreaVisionDetection);
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_AREAVISIONDETECTED_HAS_BEEN_DESTROYED)
            {
                if (m_areaVisionDetection.Remove((AreaVisionDetection)_parameters[0]))
                {
                }
            }
            if (_nameEvent == RotateToTarget.EVENT_ROTATETOTARGET_HAS_STARTED)
            {
                RotateToTarget newRotateToTarget = (RotateToTarget)_parameters[0];
                if ((newRotateToTarget != null) && (!m_rotateToTarget.Contains(newRotateToTarget)))
                {
                    m_rotateToTarget.Add(newRotateToTarget);
                }
            }
            if (_nameEvent == RotateToTarget.EVENT_ROTATETOTARGET_HAS_BEEN_DESTROYED)
            {
                if (m_rotateToTarget.Remove((RotateToTarget)_parameters[0]))
                {
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_BULLET_HAS_STARTED)
            {
                Bullet newBullet = (Bullet)_parameters[0];
                if ((newBullet != null) && (!m_bullets.Contains(newBullet)))
                {
                    m_bullets.Add(newBullet);
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_BULLET_HAS_BEEN_DESTROYED)
            {
                if (m_bullets.Remove((Bullet)_parameters[0]))
                {
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_AVATAR_DESTROY_IT)
            {
                Avatar avatar = (Avatar)_parameters[0];
                Enemy found = m_enemies.Single(s => (Avatar)s == avatar);
                if (found != null)
                {
                    found.Destroy();
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_ENEMY_HAS_BEEN_STARTED)
            {
                Enemy newEnemy = (Enemy)_parameters[0];
                if (m_instantiatedEnemies.Count > 0)
                {
                    AvatarData dataEnemy = null;
                    if (m_instantiatedEnemies.TryGetValue(newEnemy.gameObject.name, out dataEnemy))
                    {
                        dataEnemy.InitEnemy(newEnemy);
                        m_instantiatedEnemies.Remove(newEnemy.gameObject.name);
                    }
                }
                if (!m_enemies.Contains(newEnemy))
                {
                    m_enemies.Add(newEnemy);
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_NPC_HAS_BEEN_STARTED)
            {
                NPC newNPC = (NPC)_parameters[0];
                if (m_instantiatedNPCS.Count > 0)
                {
                    AvatarData dataNPC = null;
                    if (m_instantiatedNPCS.TryGetValue(newNPC.gameObject.name, out dataNPC))
                    {
                        dataNPC.InitNPC(newNPC);
                        m_instantiatedNPCS.Remove(newNPC.gameObject.name);
                    }
                }
                if (!m_NPCs.Contains(newNPC))
                {
                    m_NPCs.Add(newNPC);
                }
            }
        }

        public bool CheckVictory()
        {
            if (!m_hasBeenInited)
            {
                return false;
            }
            else
            {
                if (GameController.Instance.IsMultiplayerGame)
                {
                    if (NetworkController.Instance.IsServer)
                    {
                        return PlayerHasKilledAllEnemies() || (ItemControllerSO.Instance.HasCollectedAllCoins());
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return PlayerHasKilledAllEnemies() || (ItemControllerSO.Instance.HasCollectedAllCoins());
                }
            }
        }

        public void InitializeLogicGameElements()
        {
            for (int i = 0; i < m_enemies.Count; i++)
            {
                if (m_enemies[i] != null)
                {
                    m_enemies[i].InitLogic();
                }
            }
            for (int i = 0; i < m_NPCs.Count; i++)
            {
                if (m_NPCs[i] != null)
                {
                    m_NPCs[i].InitLogic();
                }
            }
        }

        public void StopLogicGameElements()
        {
            for (int i = 0; i < m_enemies.Count; i++)
            {
                if (m_enemies[i] != null)
                {
                    m_enemies[i].StopLogic();
                }
            }
            for (int i = 0; i < m_NPCs.Count; i++)
            {
                if (m_NPCs[i] != null)
                {
                    m_NPCs[i].StopLogic();
                }
            }
        }


        public bool PlayerHasKilledAllEnemies()
        {
            bool areAllEnemiesDead = true;

            for (int i = 0; i < m_enemies.Count; i++)
            {
                if (m_enemies[i] != null)
                {
                    areAllEnemiesDead = false;
                }
            }

            return areAllEnemiesDead;
        }


        public bool AlarmEnemyNearby(float distanceDetection)
        {
            for (int i = 0; i < m_enemies.Count; i++)
            {
                if (m_enemies[i] != null)
                {
                    float distanceToPlayer = Vector3.Distance(m_enemies[i].transform.position, GameController.Instance.LocalPlayer.GetGameObject().transform.position);
                    if (distanceToPlayer < distanceDetection)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool AlarmNPCNearby(float distanceDetection)
        {
            for (int i = 0; i < m_NPCs.Count; i++)
            {
                if (m_NPCs[i] != null)
                {
                    float distanceToPlayer = Vector3.Distance(m_NPCs[i].transform.position, GameController.Instance.LocalPlayer.GetGameObject().transform.position);
                    if (distanceToPlayer < distanceDetection)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public void UpdateLogic()
        {
            for (int i = 0; i < m_enemies.Count; i++)
            {
                if (m_enemies[i] != null) m_enemies[i].UpdateLogic();
            }
            for (int i = 0; i < m_NPCs.Count; i++)
            {
                if (m_NPCs[i] != null) m_NPCs[i].UpdateLogic();
            }
            for (int i = 0; i < m_bullets.Count; i++)
            {
                if (m_bullets[i] != null) m_bullets[i].UpdateLogic();
            }
            for (int i = 0; i < m_patrolWaypoints.Count; i++)
            {
                if (m_patrolWaypoints[i] != null) m_patrolWaypoints[i].UpdateLogic();
            }
            for (int i = 0; i < m_areaVisionDetection.Count; i++)
            {
                if (m_areaVisionDetection[i] != null) m_areaVisionDetection[i].UpdateLogic();
            }
            for (int i = 0; i < m_rotateToTarget.Count; i++)
            {
                if (m_rotateToTarget[i] != null) m_rotateToTarget[i].UpdateLogic();
            }
            bool shouldRunSpawner = false;
            if (!GameController.Instance.IsMultiplayerGame)
            {
                shouldRunSpawner = true;
            }
            else
            {
                if (NetworkController.Instance.IsServer)
                {
                    shouldRunSpawner = true;
                }
            }
            if (shouldRunSpawner)
            {
                if (m_enemySpawner != null)
                {
                    for (int i = 0; i < m_enemySpawner.Length; i++)
                    {
                        if (m_enemySpawner[i] != null) m_enemySpawner[i].UpdateLogic();
                    }
                }
            }
        }
    }
}