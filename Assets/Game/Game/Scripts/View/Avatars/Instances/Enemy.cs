using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourVRExperience.Networking;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class Enemy : Avatar, ItemEffect
    {
        public enum ENEMY_STATES { PATROL_AND_CHECK = 0, GO_TO_PLAYER, SHOOT_TO_PLAYER, DIE, INITIAL, NULL, AWAIT_INSTRUCTIONS }

        public enum ENEMY_ANIMATION { ANIMATION_IDLE = 0, ANIMATION_RUN = 1, ANIMATION_DEATH = 2, ANIMATION_ATTACK = 3 }

        public float DistanceDetection = 10;
        public float DistanceShooting = 5;
        public GameObject BulletEnemy;

        private Vector3 m_initialPosition;
        private float m_timerToShoot = 0;

        private bool m_detectedPlayer = false;
        private IPlayer m_targetDetected = null;

        protected override void Start()
        {
            base.Start();
            if (GameController.Instance.LocalPlayer != null)
            {
                SetTargetLife(GameController.Instance.LocalPlayer.GetGameObject().transform);
            }

            m_initialPosition = this.transform.position;

            if (ActivateWait)
            {
                ChangeState((int)ENEMY_STATES.AWAIT_INSTRUCTIONS);
            }
            else
            {
                ChangeState((int)ENEMY_STATES.INITIAL);
            }

            SystemEventController.Instance.Event += OnSystemEvent;

            SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_ENEMY_HAS_BEEN_STARTED, this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
        }

        protected override void OnStandEvent()
        {
            ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_IDLE);
        }

        protected override void OnMoveEvent()
        {
            ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_RUN);
        }

        protected override void OnNetworkEvent(string _nameEvent, int _originNetworkID, int _targetNetworkID, object[] _parameters)
        {
            base.OnNetworkEvent(_nameEvent, _originNetworkID, _targetNetworkID, _parameters);

            if (_nameEvent == SystemEventGameController.EVENT_NETWORK_ENEMY_BULLET_SHOOT)
            {
                int networkID = (int)_parameters[0];
                if (NetworkGameIDView != null)
                {
                    if (NetworkGameIDView.GetViewID() == networkID)
                    {
                        Vector3 targetBullet = (Vector3)_parameters[1];
                        ShootLocalBullet(targetBullet);
                    }
                }
            }
        }

        private void OnSystemEvent(string _nameEvent, object[] _parameters)
        {
            if (_nameEvent == SystemEventGameController.EVENT_GAMECONTROLLER_PLAYER_HAS_BEEN_CONFIRMED)
            {
                if (GameController.Instance.LocalPlayer != null)
                {
                    SetTargetLife(GameController.Instance.LocalPlayer.GetGameObject().transform);
                }
            }
            if (_nameEvent == SystemEventGameController.EVENT_NPC_DEAD)
            {
                DistanceDetection *= 1.25f;
                if (m_areaVisionDetection != null)
                {
                    m_areaVisionDetection.ChangeDistanceArea(m_areaVisionDetection.Vision.DetectionDistance * 1.25f);
                }
            }
        }

        public override bool IsSelectable()
        {
            return (ENEMY_STATES)m_state == ENEMY_STATES.AWAIT_INSTRUCTIONS;
        }
        public override void ChangeAnimationRun()
        {
            ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_RUN);
        }
        public override void ChangeAnimationIdle()
        {
            ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_IDLE);
        }
        public override bool IsAnimationRun()
        {
            return m_currentAnimation == (int)ENEMY_ANIMATION.ANIMATION_RUN;
        }
        public override bool IsAnimationIdle()
        {
            return m_currentAnimation == (int)ENEMY_ANIMATION.ANIMATION_IDLE;
        }
        public override void ChangeAnimationAttack()
        {
            ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_ATTACK);
        }
        public override void ForwardMovementPathfinding(Vector3 _velocity)
        {
            Model3d.gameObject.transform.forward = -_velocity;
        }

        protected override void PlayerLostEvent(GameObject _objectDetected)
        {
            if (m_targetDetected == _objectDetected.GetComponent<IPlayer>())
            {
                m_detectedPlayer = false;
                m_targetDetected = null;
            }
        }

        protected override void PlayerDetectionEvent(GameObject _objectDetected)
        {
            m_detectedPlayer = true;
            m_targetDetected = _objectDetected.GetComponent<IPlayer>();
        }

        public override void InitLogic()
        {
            ChangeState((int)ENEMY_STATES.PATROL_AND_CHECK);
        }

        public override void StopLogic()
        {
            ChangeState((int)ENEMY_STATES.INITIAL);
        }

        public override void DecreaseLife(int _unitsToDecrease)
        {
            base.DecreaseLife(_unitsToDecrease);

            // Increase the player's score
            if (m_life <= 0)
            {
                if (m_state != (int)ENEMY_STATES.DIE)
                {
                    GameController.Instance.LocalPlayer.AddScore(10);
                    Debug.Log("CURRENT SCORE AFTER HITTING A Enemy = " + GameController.Instance.LocalPlayer.Score);
                    ChangeState((int)ENEMY_STATES.DIE);
                }
            }
        }

        private void GoToInitialPosition()
        {
            if (Vector3.Distance(m_initialPosition, this.transform.position) > 1)
            {
                Vector3 directionVector = Utilities.GetDirection(m_initialPosition, this.transform.position);
                MoveToPosition(directionVector * Speed * Time.deltaTime);
                ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_RUN);
            }
            else
            {
                ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_IDLE);
            }
        }

        private bool IsInDetectionRange()
        {
            if ((m_areaVisionDetection != null) && (m_areaVisionDetection.Vision.UseBehavior == true))
            {
                return m_detectedPlayer;
            }
            else
            {
                foreach (IPlayer player in GameController.Instance.Players)
                {
                    if (Vector3.Distance(player.GetGameObject().transform.position, this.transform.position) < DistanceDetection)
                    {
                        if (Mathf.Abs(player.GetGameObject().transform.position.y - this.transform.position.y) < HeightDetection)
                        {
                            m_targetDetected = player;
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private bool IsInShootingRange()
        {
            if ((m_areaVisionDetection != null) && (m_areaVisionDetection.Vision.UseBehavior == true))
            {
                if (m_detectedPlayer && (m_targetDetected != null))
                {
                    if (Vector3.Distance(m_targetDetected.GetGameObject().transform.position, this.transform.position) < DistanceShooting)
                    {
                        if (Mathf.Abs(m_targetDetected.GetGameObject().transform.position.y - this.transform.position.y) < HeightDetection)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (m_targetDetected != null)
                {
                    if (Vector3.Distance(m_targetDetected.GetGameObject().transform.position, this.transform.position) < DistanceShooting)
                    {
                        if (Mathf.Abs(m_targetDetected.GetGameObject().transform.position.y - this.transform.position.y) < HeightDetection)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }

        private void WalkToPlayer()
        {
            if (m_targetDetected != null)
            {
                Vector3 directionVector = Utilities.GetDirection(m_targetDetected.GetGameObject().transform.position, this.transform.position);
                MoveToPosition(directionVector * Speed * Time.deltaTime);
            }
        }

        public override void ShootAtTarget(Vector3 _target, bool _forceShoot)
        {
            m_timerToShoot += Time.deltaTime;
            if ((m_timerToShoot > 2) || _forceShoot)
            {
                m_timerToShoot = 0;
                if (!GameController.Instance.IsMultiplayerGame)
                {
                    ShootLocalBullet(_target);
                }
                else
                {
                    if ((NetworkGameIDView != null) && NetworkGameIDView.AmOwner())
                    {
                        NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_ENEMY_BULLET_SHOOT, -1, -1, NetworkGameIDView.GetViewID(), _target);
                    }
                }
            }
        }

        private void ShootLocalBullet(Vector3 _target)
        {
            GameObject bullet = Instantiate(BulletEnemy);
            Physics.IgnoreCollision(bullet.GetComponent<Collider>(), this.gameObject.GetComponent<Collider>());
            int type = Bullet.TYPE_BULLET_ENEMY;
            Vector3 position = this.transform.position;
            Vector3 directionToTarget = Utilities.GetDirection(_target, this.transform.position);
            bullet.GetComponent<Bullet>().Shoot(type, position, directionToTarget);
            bullet.gameObject.layer = LayerMask.NameToLayer(GameController.LAYER_ENEMY);
        }

        private void UpdateInitialPositionRandom()
        {
            m_initialPosition = new Vector3(UnityEngine.Random.Range(20, -20), m_initialPosition.y, UnityEngine.Random.Range(20, -20));
        }

        protected override void ChangeState(int newState)
        {
            int finalState = newState;
            if (ActivateWait)
            {
                if ((finalState != (int)ENEMY_STATES.DIE) && (finalState != (int)ENEMY_STATES.AWAIT_INSTRUCTIONS))
                {
                    if (m_state != (int)ENEMY_STATES.AWAIT_INSTRUCTIONS)
                    {
                        finalState = (int)ENEMY_STATES.AWAIT_INSTRUCTIONS;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            base.ChangeState(finalState);

            switch ((ENEMY_STATES)m_state)
            {
                case ENEMY_STATES.INITIAL:
                    m_targetDetected = null;
                    Debug.Log("ENEMY(" + this.gameObject.name + ") to STATE [INITIAL]");
                    break;
                case ENEMY_STATES.PATROL_AND_CHECK:
                    m_targetDetected = null;
                    if ((m_patrolComponent == null) || (m_patrolComponent.AreThereAnyWaypoints() == false))
                    {
                        if (m_previousState != (int)ENEMY_STATES.INITIAL)
                        {
                            UpdateInitialPositionRandom();
                            if (m_rotateComponent != null)
                            {
                                m_rotateComponent.ActivateRotation(m_initialPosition);
                            }
                        }
                    }
                    else
                    {
                        m_patrolComponent.ActivatePatrol(Speed);
                    }
                    Debug.Log("ENEMY(" + this.gameObject.name + ") to STATE [CHECK_FOR_PLAYER]");
                    break;
                case ENEMY_STATES.GO_TO_PLAYER:
                    ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_RUN);
                    if ((m_patrolComponent != null) && (m_patrolComponent.AreThereAnyWaypoints() == true))
                    {
                        m_patrolComponent.DeactivatePatrol();
                    }
                    Debug.Log("ENEMY(" + this.gameObject.name + ") to STATE [GO_TO_PLAYER]");
                    break;
                case ENEMY_STATES.SHOOT_TO_PLAYER:
                    ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_ATTACK);
                    Debug.Log("ENEMY(" + this.gameObject.name + ") to STATE [SHOOT_TO_PLAYER]");
                    break;
                case ENEMY_STATES.DIE:
                    m_targetDetected = null;
                    if (m_patrolComponent != null)
                    {
                        m_patrolComponent.DeactivatePatrol();
                    }
                    if (m_areaVisionDetection != null)
                    {
                        m_areaVisionDetection.StopAreaDetection();
                    }
                    ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_DEATH);
                    SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_ENEMY_DEAD);
                    SoundsGameController.Instance.PlaySoundFX(SoundsGameController.FX_DEAD_ENEMY, false, 1);
                    Debug.Log("ENEMY(" + this.gameObject.name + ") to STATE [DIE]");
                    break;

                case ENEMY_STATES.AWAIT_INSTRUCTIONS:
                    if (m_commands.Count == 0)
                    {
                        ChangeAnimation((int)ENEMY_ANIMATION.ANIMATION_IDLE);
                    }
                    break;
            }
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (!CheckShouldRun())
            {
                if (m_rotateComponent != null)
                {
                    m_rotateComponent.DeactivateRotation();
                }
                if (m_patrolComponent != null)
                {
                    m_patrolComponent.DeactivatePatrol();
                }
                return;
            }

            switch ((ENEMY_STATES)m_state)
            {
                case ENEMY_STATES.INITIAL:
                    break;

                case ENEMY_STATES.PATROL_AND_CHECK:
                    if ((m_patrolComponent == null) || (m_patrolComponent.AreThereAnyWaypoints() == false))
                    {
                        GoToInitialPosition();
                    }

                    if (IsInDetectionRange() == true)
                    {
                        ChangeState((int)ENEMY_STATES.GO_TO_PLAYER);
                    }

                    if (IsInShootingRange() == true)
                    {
                        ChangeState((int)ENEMY_STATES.SHOOT_TO_PLAYER);
                    }

                    if (m_life <= 0)
                    {
                        ChangeState((int)ENEMY_STATES.DIE);
                    }
                    break;

                case ENEMY_STATES.GO_TO_PLAYER:
                    if ((m_rotateComponent != null) && (m_targetDetected != null))
                    {
                        m_rotateComponent.ActivateRotation(m_targetDetected.GetGameObject().transform.position);
                    }
                    WalkToPlayer();

                    if (IsInDetectionRange() == false)
                    {
                        ChangeState((int)ENEMY_STATES.PATROL_AND_CHECK);
                    }

                    if (IsInShootingRange() == true)
                    {
                        ChangeState((int)ENEMY_STATES.SHOOT_TO_PLAYER);
                    }

                    if (m_life <= 0)
                    {
                        ChangeState((int)ENEMY_STATES.DIE);
                    }
                    break;

                case ENEMY_STATES.SHOOT_TO_PLAYER:
                    if ((m_rotateComponent != null) && (m_targetDetected != null))
                    {
                        m_rotateComponent.ActivateRotation(m_targetDetected.GetGameObject().transform.position);
                    }

                    if (m_targetDetected != null)
                    {
                        ShootAtTarget(m_targetDetected.GetGameObject().transform.position, false);
                    }

                    if (IsInDetectionRange() == false)
                    {
                        ChangeState((int)ENEMY_STATES.PATROL_AND_CHECK);
                    }

                    if (IsInShootingRange() == false)
                    {
                        ChangeState((int)ENEMY_STATES.GO_TO_PLAYER);
                    }

                    if (m_life <= 0)
                    {
                        ChangeState((int)ENEMY_STATES.DIE);
                    }
                    break;

                case ENEMY_STATES.DIE:
                    m_timeCounter += Time.deltaTime;
                    if (m_timeCounter > 4)
                    {
                        ChangeState((int)ENEMY_STATES.NULL);
                        Destroy();
                    }
                    break;

                case ENEMY_STATES.AWAIT_INSTRUCTIONS:
                    RunCommands();
                    break;
            }
        }
    }
}