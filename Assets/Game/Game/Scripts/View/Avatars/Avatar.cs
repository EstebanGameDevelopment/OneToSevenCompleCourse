#if ENABLE_PHOTON
using Photon.Pun;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using YourVRExperience.Networking;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public abstract class Avatar : StateMachine
    {
        public string PrefabName;
        public int InitialLife = 100;
        public float Speed = 2;
        public Animator Model3d;
        public float HeightDetection = 4;
        public TextMeshProUGUI InGameLife;
        public GameObject IsSelected;
        public int Ammunition = 20;
        public bool ActivateWait = false;

        protected int m_life = 100;

        protected PatrolWaypoints m_patrolComponent;
        protected RotateToTarget m_rotateComponent;
        protected AreaVisionDetection m_areaVisionDetection;

        protected int m_currentAnimation = -1;
        protected int m_networkAnimation = -1;
        protected bool m_useRigidBody = true;

        protected NavMeshAgent m_navigationComponent;
        protected Vector3 m_targetPositionToGo;

        protected List<Command> m_commands = new List<Command>();
        protected Vector3 m_initialForwardModel;
        protected int m_animationInitial = -1;

        private bool m_networkSetInitialValues = false;

        public int Life
        {
            get { return m_life; }
        }

        public bool UseRigidBody
        {
            get { return m_useRigidBody; }
            set { m_useRigidBody = value; }
        }

        private NetworkGameID m_networkGameID;
        public NetworkGameID NetworkGameIDView
        {
            get
            {
                if (m_networkGameID == null)
                {
                    if (this != null)
                    {
                        m_networkGameID = GetComponent<NetworkGameID>();
                    }
                }
                return m_networkGameID;
            }
        }

        protected virtual void Start()
        {
            m_life = InitialLife;
            if (InGameLife != null)
            {
                InGameLife.text = m_life.ToString();
            }

            m_patrolComponent = this.GetComponent<PatrolWaypoints>();
            if (m_patrolComponent != null)
            {
                m_patrolComponent.MoveEvent += OnMoveEvent;
                m_patrolComponent.StandEvent += OnStandEvent;
            }
            m_rotateComponent = this.GetComponent<RotateToTarget>();

            m_areaVisionDetection = this.GetComponent<AreaVisionDetection>();
            if (m_areaVisionDetection != null)
            {
                m_areaVisionDetection.DetectionEvent += PlayerDetectionEvent;
                m_areaVisionDetection.LostEvent += PlayerLostEvent;
            }

            m_navigationComponent = this.GetComponent<NavMeshAgent>();
            if (m_navigationComponent != null)
            {
                m_navigationComponent.enabled = false;
            }

            if (Model3d != null)
            {
                m_initialForwardModel = Model3d.transform.localEulerAngles;
            }

            if (IsSelected != null)
            {
                IsSelected.SetActive(false);
            }

            if (GameController.Instance.IsMultiplayerGame)
            {
                NetworkController.Instance.NetworkEvent += OnNetworkEvent;

                if (NetworkGameIDView != null)
                {
                    NetworkGameIDView.Initialize(SystemEventGameController.EVENT_AVATAR_INITIAL_ANIMATION);
                    if (!NetworkGameIDView.IsMine())
                    {
                        StartCoroutine(RequestAnimationState());
                    }
                }
            }
        }

        IEnumerator RequestAnimationState()
        {
            yield return new WaitForSeconds(0.2f);

            NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_AVATAR_REQUEST_ANIMATION, -1, -1, NetworkGameIDView.GetViewID());
        }

        protected virtual void OnDestroy()
        {
            if (NetworkController.Instance != null)
            {
                NetworkController.Instance.NetworkEvent -= OnNetworkEvent;
            }
        }

        public virtual void Destroy()
        {
            if (GameController.Instance.IsMultiplayerGame)
            {
                if ((NetworkGameIDView != null) && NetworkGameIDView.IsMine())
                {
                    NetworkGameIDView.Destroy();
                }
            }
            else
            {
                GameObject.Destroy(this.gameObject);
            }
        }

        protected virtual void OnNetworkEvent(string _nameEvent, int _originNetworkID, int _targetNetworkID, object[] _parameters)
        {
            if (_nameEvent == SystemEventGameController.EVENT_NETWORK_AVATAR_REQUEST_ANIMATION)
            {
                if (NetworkGameIDView != null)
                {
                    if (NetworkGameIDView.GetViewID() == (int)_parameters[0])
                    {
                        NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_AVATAR_NEW_ANIMATION, -1, -1, NetworkGameIDView.GetViewID(), m_currentAnimation);
                    }
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_AVATAR_INITIAL_ANIMATION)
            {
                if (this.gameObject == (GameObject)_parameters[0])
                {
                    m_animationInitial = (int)_parameters[1];
                }                
            }
            if (_nameEvent == SystemEventGameController.EVENT_NETWORK_AVATAR_NEW_ANIMATION)
            {
                if (NetworkGameIDView != null)
                {
                    if (NetworkGameIDView.GetViewID() == (int)_parameters[0])
                    {
                        ChangeLocalAnimation((int)_parameters[1]);
                    }
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_NETWORK_REPORT_NETWORK_ID)
            {
                int networkID = (int)_parameters[0];
                if (networkID != NetworkController.Instance.UniqueNetworkID)
                {
                    if (NetworkController.Instance.IsServer)
                    {
                        if (NetworkGameIDView != null)
                        {
                            if (m_areaVisionDetection != null)
                            {
                                NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_AVATAR_INITIAL_VALUES, -1, -1,
                                                                                NetworkGameIDView.GetViewID(), this.transform.localScale,
                                                                                m_areaVisionDetection.Vision.DetectionDistance,
                                                                                m_areaVisionDetection.Vision.DetectionAngle,
                                                                                m_areaVisionDetection.Vision.Orientation,
                                                                                m_areaVisionDetection.Vision.HeightToFloor,
                                                                                (int)m_areaVisionDetection.Vision.Target,
                                                                                m_areaVisionDetection.Vision.UseBehavior,
                                                                                m_areaVisionDetection.Vision.HeightDetection);
                            }
                            else
                            {
                                NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_AVATAR_INITIAL_VALUES, -1, -1, NetworkGameIDView.GetViewID(), this.transform.localScale);
                            }
                        }
                    }
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_NETWORK_AVATAR_INITIAL_VALUES)
            {
                if (NetworkGameIDView != null)
                {
                    if (NetworkGameIDView.GetViewID() == (int)_parameters[0])
                    {
                        if (!m_networkSetInitialValues && !NetworkController.Instance.IsServer)
                        {
                            m_networkSetInitialValues = true;
                            this.transform.localScale = (Vector3)_parameters[1];
                            if (m_areaVisionDetection != null)
                            {
                                m_areaVisionDetection.Vision.DetectionDistance = (float)_parameters[2];
                                m_areaVisionDetection.Vision.DetectionAngle = (float)_parameters[3];
                                m_areaVisionDetection.Vision.Orientation = (float)_parameters[4];
                                m_areaVisionDetection.Vision.HeightToFloor = (float)_parameters[5];
                                m_areaVisionDetection.Vision.Target = (AreaVisionDetection.TargetCharacters)((int)_parameters[6]);
                                m_areaVisionDetection.Vision.UseBehavior = (bool)_parameters[7];
                                m_areaVisionDetection.Vision.HeightDetection = (float)_parameters[8];
                                if (m_areaVisionDetection.Vision.UseBehavior)
                                {
                                    m_areaVisionDetection.CreateAreaVisionDetection();
                                }
                                else
                                {
                                    m_areaVisionDetection.StopAreaDetection();
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void ActivateLocalModel(bool _enable)
        {
            if (GameController.Instance.IsMultiplayerGame)
            {
                if (NetworkGameIDView.AmOwner())
                {
                    Model3d.gameObject.SetActive(_enable);
                }
                else
                {
                    Model3d.gameObject.SetActive(true);
                }
            }
            else
            {
                Model3d.gameObject.SetActive(_enable);
            }
        }

        public abstract bool IsSelectable();
        public abstract void ChangeAnimationRun();
        public abstract void ChangeAnimationIdle();
        public abstract bool IsAnimationRun();
        public abstract bool IsAnimationIdle();
        public abstract void ChangeAnimationAttack();
        public abstract void ForwardMovementPathfinding(Vector3 _velocity);

        public abstract void ShootAtTarget(Vector3 _target, bool _forceShoot);
        public virtual void UpdateLogic()
        {
            if (m_animationInitial != -1)
            {
                ChangeLocalAnimation(m_animationInitial);
                m_animationInitial = -1;
            }
        }

        public virtual void AddCoins(int _coins)
        {

        }

        public void Select(bool _active)
        {
            if (IsSelected != null)
            {
                IsSelected.SetActive(_active);
            }
        }

        public bool CheckInsideArea(Vector3 _origin, Vector3 _target)
        {
            float topX = ((_origin.x < _target.x) ? _target.x : _origin.x);
            float topY = ((_origin.z < _target.z) ? _target.z : _origin.z);

            float bottomX = ((_origin.x > _target.x) ? _target.x : _origin.x);
            float bottomY = ((_origin.z > _target.z) ? _target.z : _origin.z);

            Vector3 pos = this.gameObject.transform.position;
            return ((bottomX < pos.x) && (pos.x < topX) && (bottomY < pos.z) && (pos.z < topY));
        }

        public void RestoreInitialForwardModel()
        {
            Model3d.transform.localEulerAngles = m_initialForwardModel;
        }

        protected void SetTargetLife(Transform _target)
        {
            if (InGameLife != null)
            {
                if (InGameLife.GetComponent<LookAtTarget>() != null)
                {
                    InGameLife.GetComponent<LookAtTarget>().SetTarget(_target);
                }
            }
        }

        public void AddCommand(Command _command)
        {
            m_commands.Add(_command);
        }

        public void RunCommands()
        {
            for (int i = 0; i < m_commands.Count; i++)
            {
                m_commands[i].Execute();
                if (m_commands[i].IsFinished())
                {
                    m_commands.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (m_commands[i].IsBlocking())
                    {
                        break;
                    }
                }
            }
        }

        protected void DestroyCommands()
        {
            for (int i = 0; i < m_commands.Count; i++)
            {
                m_commands[i].Destroy();
            }
            m_commands.Clear();
        }

        public void ReenablePhysicsDelayed()
        {
            StartCoroutine(ReenablePhysics(1));
        }

        IEnumerator ReenablePhysics(float _time)
        {
            yield return new WaitForSeconds(_time);

            Utilities.ActivatePhysics(this.gameObject, true);
        }

        protected virtual void OnStandEvent()
        {

        }
        protected virtual void OnMoveEvent()
        {
        }

        protected virtual void ChangeAnimation(int _animationID)
        {
            if (Model3d != null)
            {
                if (GameController.Instance.IsMultiplayerGame)
                {
                    if ((m_currentAnimation != _animationID) && (m_networkAnimation == -1))
                    {
                        if ((NetworkGameIDView != null) && NetworkGameIDView.IsMine())
                        {
                            m_networkAnimation = _animationID;
                            NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_AVATAR_NEW_ANIMATION, -1, -1, NetworkGameIDView.GetViewID(), _animationID);
                        }
                    }
                }
                else
                {
                    ChangeLocalAnimation(_animationID);
                }
            }
        }

        private void ChangeLocalAnimation(int _animationID)
        {
            if (Model3d != null)
            {
                if (m_currentAnimation != _animationID)
                {
                    m_currentAnimation = _animationID;
                    Model3d.SetInteger("animationID", _animationID);
                }
                m_networkAnimation = -1;
            }
        }

        protected virtual void PlayerLostEvent(GameObject _objectDetected)
        {
        }

        protected virtual void PlayerDetectionEvent(GameObject _objectDetected)
        {
        }

        public abstract void InitLogic();

        public abstract void StopLogic();

        protected void MoveToPosition(Vector3 _increment)
        {
            if (m_useRigidBody)
            {
                transform.GetComponent<Rigidbody>().MovePosition(transform.position + _increment);
            }
            else
            {
                transform.position += _increment;
            }
        }

        public virtual void IncreaseAmmo(int _ammo)
        {
            Ammunition += _ammo;
        }

        public virtual void IncreaseLife(int _unitsToIncrease)
        {
            m_life = m_life + _unitsToIncrease;
            if (m_life > 100)
            {
                m_life = 100;
            }
            if (InGameLife != null)
            {
                InGameLife.text = m_life.ToString();
            }
            Debug.Log(this.gameObject.name + " INCREASED LIFE TO=" + m_life);
        }

        public virtual void DecreaseLife(int _unitsToDecrease)
        {
            m_life = m_life - _unitsToDecrease;
            if (m_life < 0)
            {
                m_life = 0;
            }
            if (InGameLife != null)
            {
                InGameLife.text = m_life.ToString();
            }
            Debug.Log(this.gameObject.name + " DECREASED LIFE TO=" + m_life);
        }


        protected virtual void OnTriggerEnter(Collider _collision)
        {
            if (_collision.gameObject.GetComponent<Spikes>() != null)
            {
                Debug.Log(this.gameObject.name + " HAS COLLIDED WITH A SPIKES INSTANCE");
                DecreaseLife(_collision.gameObject.GetComponent<Spikes>().DamageLife);
            }
            if (_collision.gameObject.GetComponent<Portal>() != null)
            {
                Debug.Log(this.gameObject.name + " HAS COLLIDED WITH A PORTAL INSTANCE");
                IncreaseLife(_collision.gameObject.GetComponent<Portal>().MoreLife);
            }
        }

        protected bool CheckShouldRun()
        {
            bool shouldRun = true;
            if (GameController.Instance.IsMultiplayerGame)
            {
                if (NetworkGameIDView != null)
                {
                    if (!NetworkGameIDView.IsMine()) shouldRun = false;
                }
            }
            return shouldRun;
        }
    }

}