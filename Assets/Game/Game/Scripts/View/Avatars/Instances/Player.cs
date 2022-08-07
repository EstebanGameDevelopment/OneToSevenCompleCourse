using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using YourVRExperience.Networking;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class Player : Avatar, ItemEffect, IPlayer
    {
        public enum PLAYER_STATES { IDLE = 0, WALK, DIE, INITIAL, AWAIT_INSTRUCTIONS }

        public enum PLAYER_ANIMATION { ANIMATION_IDLE = 0, ANIMATION_RUN = 1, ANIMATION_DEATH = 2, ANIMATION_JUMP = 3 }

        public GameObject BulletPlayer;
        public float Sensitivity = 7F;

        private int m_score;
        public int Score
        {
            get { return m_score; }
            set
            {
                m_score = value;
                if (m_HUD != null) m_HUD.UpdateDisplayScore(m_score);
            }
        }

        private float m_rotationY = 0F;
        private int m_coins = 0;
        private int m_counterBulletsShot = 0;
        private Vector3 m_initialPosition;
        private Vector3 m_forwardCamera;
        private Vector3 m_positionCamera;
        private bool m_isTouchingFloor = false;
        private float m_timerToShoot = 0;
        private IHUD m_HUD;

        public Vector3 ForwardCamera
        {
            get { return m_forwardCamera; }
            set { this.gameObject.transform.forward = value; }
        }

        public Vector3 PositionCamera
        {
            get { return m_positionCamera; }
        }

        void Awake()
        {
            SystemEventController.Instance.Event += OnSystemEvent;
        }

        protected override void Start()
        {
            base.Start();

            m_initialPosition = this.transform.position;

            ChangeState((int)PLAYER_STATES.INITIAL);

            ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_IDLE);

            ActivateLocalModel(false);

            SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_PLAYER_HAS_STARTED, this);
            SystemEventController.Instance.DispatchSystemEvent(CameraController.EVENT_CAMERA_PLAYER_READY_FOR_CAMERA, this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
        }

        public bool IsOwner()
        {
            if (!GameController.Instance.IsMultiplayerGame)
            {
                return true;
            }
            else
            if (NetworkGameIDView != null)
            {
                if (NetworkGameIDView.IsConnected())
                {
                    if (NetworkGameIDView.AmOwner())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public GameObject GetGameObject()
        {
            return this.gameObject;
        }

        public void SetHUD(IHUD _hud)
        {
            m_HUD = _hud;

            m_HUD.UpdateDisplayScore(Score);
            m_HUD.UpdateDisplayLife(m_life);
            m_HUD.UpdateDisplayTotalAmmo(Ammunition);
            m_HUD.UpdateDisplayDangerInfo("");
            m_HUD.UpdateDisplayNPCInfo("");
        }

        public void AddScore(int _score)
        {
            Score += _score;
        }

        public override void AddCoins(int _coins)
        {
            m_coins += _coins;
        }

        protected override void OnNetworkEvent(string _nameEvent, int _originNetworkID, int _targetNetworkID, object[] _parameters)
        {
            base.OnNetworkEvent(_nameEvent, _originNetworkID, _targetNetworkID, _parameters);

            if (_nameEvent == SystemEventGameController.EVENT_NETWORK_PLAYER_BULLET_SHOOT)
            {
                int networkID = (int)_parameters[0];
                if (NetworkGameIDView != null)
                {
                    if (NetworkGameIDView.GetViewID() == networkID)
                    {
                        Vector3 positionBullet = (Vector3)_parameters[1];
                        Vector3 forwardBullet = (Vector3)_parameters[2];
                        ShootLocalBullet(positionBullet, forwardBullet);
                    }
                }
            }
        }

        private void OnSystemEvent(string _nameEvent, object[] _parameters)
        {
            if (_nameEvent == SystemEventGameController.EVENT_HUD_HAS_STARTED)
            {
                m_HUD = (IHUD)_parameters[0];
                SetHUD(m_HUD);
            }
            if (_nameEvent == CameraController.EVENT_CAMERA_SWITCHED_TO_1ST_PERSON)
            {
                ActivateLocalModel(false);
                ChangeState((int)PLAYER_STATES.IDLE);
                DestroyCommands();
            }
            if (_nameEvent == CameraController.EVENT_CAMERA_SWITCHED_TO_3RD_PERSON)
            {
                ActivateLocalModel(true);
                ChangeState((int)PLAYER_STATES.IDLE);
                DestroyCommands();
            }
            if (_nameEvent == CameraController.EVENT_CAMERA_SWITCHED_TO_FREE_CAMERA)
            {
                ActivateLocalModel(true);
                ChangeState((int)PLAYER_STATES.AWAIT_INSTRUCTIONS);
            }
        }

        public override bool IsSelectable()
        {
            return (PLAYER_STATES)m_state == PLAYER_STATES.AWAIT_INSTRUCTIONS;
        }

        public override void ChangeAnimationRun()
        {
            ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_RUN);
        }

        public override void ChangeAnimationIdle()
        {
            ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_IDLE);
        }
        public override bool IsAnimationRun()
        {
            return m_currentAnimation == (int)PLAYER_ANIMATION.ANIMATION_RUN;
        }
        public override bool IsAnimationIdle()
        {
            return m_currentAnimation == (int)PLAYER_ANIMATION.ANIMATION_IDLE;
        }
        public override void ChangeAnimationAttack()
        {
            ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_IDLE);
        }
        public override void ForwardMovementPathfinding(Vector3 _velocity)
        {
            Model3d.gameObject.transform.forward = _velocity;
        }

        public override void ShootAtTarget(Vector3 _target, bool _forceShoot)
        {
            m_timerToShoot += Time.deltaTime;
            if ((m_timerToShoot > 2) || _forceShoot)
            {
                m_timerToShoot = 0;
                GameObject bullet = Instantiate(BulletPlayer);
                Physics.IgnoreCollision(bullet.GetComponent<Collider>(), this.gameObject.GetComponent<Collider>());
                int type = Bullet.TYPE_BULLET_PLAYER;
                Vector3 position = this.transform.position;
                Vector3 directionToTarget = Utilities.GetDirection(_target, this.transform.position);
                bullet.GetComponent<Bullet>().Shoot(type, position, directionToTarget);
                bullet.gameObject.layer = LayerMask.NameToLayer(GameController.LAYER_PLAYER);
            }
        }

        public void ResetPlayerPosition()
        {
            this.transform.position = m_initialPosition;
            m_positionCamera = this.transform.position;
        }

        public void ResetPlayerLife()
        {
            m_life = InitialLife;
            if (m_HUD != null) m_HUD.UpdateDisplayLife(m_life);
        }

        public override void InitLogic()
        {
            ChangeState((int)PLAYER_STATES.IDLE);
        }

        public override void StopLogic()
        {
            if (m_life > 0)
            {
                ChangeState((int)PLAYER_STATES.INITIAL);
            }
        }

        private void Idle()
        {
            m_positionCamera = this.transform.position;
        }

        private void Move()
        {
            float axisVertical = GameController.Instance.InputControls.GetAxisVertical();
            float axisHorizontal = GameController.Instance.InputControls.GetAxisHorizontal();
            Vector3 forward = axisVertical * CameraController.Instance.GameCamera.transform.forward * Speed * Time.deltaTime;
            Vector3 lateral = axisHorizontal * CameraController.Instance.GameCamera.transform.right * Speed * Time.deltaTime;
            Vector3 increment = forward + lateral;
            increment.y = 0;
            MoveToPosition(increment);
            m_positionCamera = this.transform.position;
        }

        public override void IncreaseAmmo(int _ammo)
        {
            base.IncreaseAmmo(_ammo);

            if (m_HUD != null) m_HUD.UpdateDisplayTotalAmmo(Ammunition);
        }

        public void AddCoin()
        {
            m_coins = m_coins + 1;
            Debug.Log("Player's collected coints = " + m_coins);
        }

        private void RotateCamera()
        {
            if (!GameController.Instance.InputControls.IsVR)
            {
                float rotationX = CameraController.Instance.GameCamera.transform.localEulerAngles.y + GameController.Instance.InputControls.GetMouseAxisHorizontal() * Sensitivity;
                m_rotationY = m_rotationY + GameController.Instance.InputControls.GetMouseAxisVertical() * Sensitivity;
                m_rotationY = Mathf.Clamp(m_rotationY, -60, 60);
                Quaternion rotation = Quaternion.Euler(-m_rotationY, rotationX, 0);
                m_forwardCamera = rotation * Vector3.forward;
                this.transform.forward = new Vector3(m_forwardCamera.x, 0, m_forwardCamera.z);
            }
        }

        private void ShootBullet()
        {
            if (Ammunition > 0)
            {
                if (GameController.Instance.InputControls.ShootPressed())
                {
                    Vector3 positionBullet = Vector3.zero;
                    Vector3 forwardBullet = Vector3.zero;
                    if (GameController.Instance.InputControls.IsVR)
                    {
                        positionBullet = GameController.Instance.InputControls.RayPointerVR.position;
                        forwardBullet = GameController.Instance.InputControls.RayPointerVR.forward;
                    }
                    else
                    {
                        if (CameraController.Instance.IsFirstPersonCamera())
                        {
                            positionBullet = this.transform.position;
                            forwardBullet = CameraController.Instance.GameCamera.transform.forward;
                        }
                        else
                        {
                            positionBullet = this.transform.position;
                            forwardBullet = new Vector3(CameraController.Instance.GameCamera.transform.forward.x, 0, CameraController.Instance.GameCamera.transform.forward.z);
                        }
                    }
                    if (!GameController.Instance.IsMultiplayerGame)
                    {
                        ShootLocalBullet(positionBullet, forwardBullet);
                    }
                    else
                    {
                        if ((NetworkGameIDView != null) && NetworkGameIDView.AmOwner())
                        {
                            NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_PLAYER_BULLET_SHOOT, -1, -1, NetworkGameIDView.GetViewID(), positionBullet, forwardBullet);
                        }
                    }
                }
            }
        }

        private void ShootLocalBullet(Vector3 _position, Vector3 _forward)
        {
            GameObject newBulletGO = Instantiate(BulletPlayer);
            Bullet bullet = newBulletGO.GetComponent<Bullet>();
            bullet.Shoot(Bullet.TYPE_BULLET_PLAYER, _position, _forward);
            Physics.IgnoreCollision(this.GetComponent<Collider>(), newBulletGO.GetComponent<Collider>());
            Ammunition--;
            if (m_HUD != null) m_HUD.UpdateDisplayTotalAmmo(Ammunition);
            SoundsGameController.Instance.PlaySoundFX(SoundsGameController.FX_SHOOT, false, 1);
        }

        private void Jump()
        {
            if (m_isTouchingFloor)
            {
                if (GameController.Instance.InputControls.JumpPressed())
                {
                    base.ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_JUMP);
                    m_isTouchingFloor = false;
                    this.GetComponent<Rigidbody>().AddForce(Vector3.up * 600);
                }
            }
        }

        void OnCollisionEnter(Collision _collision)
        {
            if ((PLAYER_STATES)m_state != PLAYER_STATES.AWAIT_INSTRUCTIONS)
            {
                if (_collision.gameObject.tag == GameController.TAG_FLOOR)
                {
                    m_isTouchingFloor = true;
                    if (ArrowKeyPressed())
                    {
                        ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_RUN);
                    }
                    else
                    {
                        ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_IDLE);
                    }
                }
            }
        }

        public override void IncreaseLife(int _unitsToIncrease)
        {
            base.IncreaseLife(_unitsToIncrease);

            if (m_HUD != null) m_HUD.UpdateDisplayLife(m_life);
        }

        public override void DecreaseLife(int _unitsToDecrease)
        {
            base.DecreaseLife(_unitsToDecrease);

            if (m_HUD != null) m_HUD.UpdateDisplayLife(m_life);
        }

        private bool ArrowKeyPressed()
        {
            if (GameController.Instance.InputControls.IsPressedAnyKeyToMove())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CheckToDisplayDanger()
        {
            if (m_HUD != null)
            {
                if (LevelController.Instance.AlarmEnemyNearby(6) == true)
                {
                    m_HUD.UpdateDisplayDangerInfo("Danger, Enemy nearby!!!");
                }
                else
                {
                    m_HUD.UpdateDisplayDangerInfo("");
                }
            }
        }

        private void CheckToDisplayNPC()
        {
            if (m_HUD != null)
            {
                if (LevelController.Instance.AlarmNPCNearby(6) == true)
                {
                    m_HUD.UpdateDisplayNPCInfo("A friendly NPC appeared!!!");
                }
                else
                {
                    m_HUD.UpdateDisplayNPCInfo("");
                }
            }
        }

        protected override void ChangeAnimation(int _animationID)
        {
            if (m_isTouchingFloor)
            {
                base.ChangeAnimation(_animationID);
            }
        }


        protected override void ChangeState(int newState)
        {
            base.ChangeState(newState);


            switch ((PLAYER_STATES)m_state)
            {
                case PLAYER_STATES.INITIAL:
                    Utilities.ActivatePhysics(this.gameObject, false);
                    if (m_navigationComponent != null) m_navigationComponent.enabled = false;
                    ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_IDLE);
                    Debug.Log("Player STATE CHANGED TO INITIAL");
                    break;
                case PLAYER_STATES.IDLE:
                    Utilities.ActivatePhysics(this.gameObject, true);
                    if (m_navigationComponent != null) m_navigationComponent.enabled = false;
                    ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_IDLE);
                    Debug.Log("Player STATE CHANGED TO IDLE");
                    break;
                case PLAYER_STATES.WALK:
                    Utilities.ActivatePhysics(this.gameObject, true);
                    if (m_navigationComponent != null) m_navigationComponent.enabled = false;
                    ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_RUN);
                    Debug.Log("Player STATE CHANGED TO WALK");
                    break;
                case PLAYER_STATES.DIE:
                    Utilities.ActivatePhysics(this.gameObject, true);
                    if (m_navigationComponent != null) m_navigationComponent.enabled = false;
                    ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_DEATH);
                    Debug.Log("Player STATE CHANGED TO DIE");
                    break;
                case PLAYER_STATES.AWAIT_INSTRUCTIONS:
                    Utilities.ActivatePhysics(this.gameObject, false);
                    if (m_navigationComponent != null) m_navigationComponent.enabled = true;
                    ChangeAnimation((int)PLAYER_ANIMATION.ANIMATION_IDLE);
                    Debug.Log("Player STATE CHANGED TO AWAIT_INSTRUCTIONS");
                    break;
            }
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (!CheckShouldRun()) return;

            if (m_rotateComponent != null) m_rotateComponent.UpdateLogic();

            switch ((PLAYER_STATES)m_state)
            {
                case PLAYER_STATES.INITIAL:
                    break;

                case PLAYER_STATES.IDLE:
                    // Debug.Log("STATE IDLE");
                    Idle();
                    Jump();
                    RotateCamera();
                    ShootBullet();
                    CheckToDisplayDanger();
                    CheckToDisplayNPC();
                    if (m_life <= 0)
                    {
                        ChangeState((int)PLAYER_STATES.DIE);
                    }
                    else
                    if (ArrowKeyPressed() == true)
                    {
                        ChangeState((int)PLAYER_STATES.WALK);
                    }
                    break;

                case PLAYER_STATES.WALK:
                    // Debug.Log("STATE WALK");
                    Jump();
                    Move();
                    RotateCamera();
                    ShootBullet();
                    CheckToDisplayDanger();
                    CheckToDisplayNPC();
                    if (m_life <= 0)
                    {
                        ChangeState((int)PLAYER_STATES.DIE);
                    }
                    else
                    if (ArrowKeyPressed() == false)
                    {
                        ChangeState((int)PLAYER_STATES.IDLE);
                    }
                    break;

                case PLAYER_STATES.DIE:
                    // Debug.Log("THE PLAYER IS DEAD");
                    break;

                case PLAYER_STATES.AWAIT_INSTRUCTIONS:
                    RunCommands();
                    if (m_life <= 0)
                    {
                        ChangeState((int)PLAYER_STATES.DIE);
                    }
                    break;
            }
        }
    }
}