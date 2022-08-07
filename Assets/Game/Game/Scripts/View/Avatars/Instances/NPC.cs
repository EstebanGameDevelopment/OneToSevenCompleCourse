using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class NPC : Avatar, ItemEffect
    {
        public enum NPC_STATES { CHECK_FOR_PLAYER = 0, GO_TO_PLAYER, TALK_TO_PLAYER, DIE, INITIAL, NULL, AWAIT_INSTRUCTIONS }

        public enum NPC_ANIMATION { ANIMATION_IDLE = 0, ANIMATION_RUN = 1, ANIMATION_DEATH = 2, ANIMATION_TALK = 3 }


        public float DistanceDetection = 10;
        public float DistanceTalking = 5;
        public string MessageToPlayer = "Hello Player";

        private Vector3 m_initialPosition;
        private float m_timerToTalk = 0;

        private bool m_detectedPlayer = false;
        private IPlayer m_targetDetected = null;

        private int m_totalEnemiesDead = 0;

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
                ChangeState((int)NPC_STATES.AWAIT_INSTRUCTIONS);
            }
            else
            {
                ChangeState((int)NPC_STATES.INITIAL);
            }

            SystemEventController.Instance.Event += OnSystemEvent;

            SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_NPC_HAS_BEEN_STARTED, this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
        }

        public override bool IsSelectable()
        {
            return (NPC_STATES)m_state == NPC_STATES.AWAIT_INSTRUCTIONS;
        }

        public override void ChangeAnimationRun()
        {
            ChangeAnimation((int)NPC_ANIMATION.ANIMATION_RUN);
        }

        public override void ChangeAnimationIdle()
        {
            ChangeAnimation((int)NPC_ANIMATION.ANIMATION_IDLE);
        }

        public override bool IsAnimationRun()
        {
            return m_currentAnimation == (int)NPC_ANIMATION.ANIMATION_RUN;
        }

        public override bool IsAnimationIdle()
        {
            return m_currentAnimation == (int)NPC_ANIMATION.ANIMATION_IDLE;
        }

        public override void ChangeAnimationAttack()
        {
            ChangeAnimation((int)NPC_ANIMATION.ANIMATION_TALK);
        }
        public override void ForwardMovementPathfinding(Vector3 _velocity)
        {
            Model3d.gameObject.transform.forward = -_velocity;
        }

        public override void ShootAtTarget(Vector3 _target, bool _forceShoot)
        {
        }

        protected override void OnStandEvent()
        {
            ChangeAnimation((int)NPC_ANIMATION.ANIMATION_IDLE);
        }

        protected override void OnMoveEvent()
        {
            ChangeAnimation((int)NPC_ANIMATION.ANIMATION_RUN);
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
            if (_nameEvent == SystemEventGameController.EVENT_ENEMY_DEAD)
            {
                m_totalEnemiesDead++;
                Debug.Log("<color=green>NPC has received the event of Enemy Dead!!! Total Enemies Dead=" + m_totalEnemiesDead + "</color>");
            }
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
            ChangeState((int)NPC_STATES.CHECK_FOR_PLAYER);
        }

        public override void StopLogic()
        {
            ChangeState((int)NPC_STATES.INITIAL);
        }

        public override void DecreaseLife(int _unitsToDecrease)
        {
            base.DecreaseLife(_unitsToDecrease);

            // Decrease the player's score
            if (m_life <= 0)
            {
                if (m_state != (int)NPC_STATES.DIE)
                {
                    GameController.Instance.LocalPlayer.AddScore(-10);
                    Debug.Log("CURRENT SCORE AFTER HITTING A NPC = " + GameController.Instance.LocalPlayer.Score);
                    ChangeState((int)NPC_STATES.DIE);
                }
            }
        }

        private void GoToInitialPosition()
        {
            if (Vector3.Distance(m_initialPosition, this.transform.position) > 1)
            {
                Vector3 directionVector = Utilities.GetDirection(m_initialPosition, this.transform.position);
                MoveToPosition(directionVector * Speed * Time.deltaTime);
                ChangeAnimation((int)NPC_ANIMATION.ANIMATION_RUN);
            }
            else
            {
                ChangeAnimation((int)NPC_ANIMATION.ANIMATION_IDLE);
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

        private bool IsInTalkingRange()
        {
            if ((m_areaVisionDetection != null) && (m_areaVisionDetection.Vision.UseBehavior == true))
            {
                if (m_detectedPlayer && (m_targetDetected != null))
                {
                    if (Vector3.Distance(m_targetDetected.GetGameObject().transform.position, this.transform.position) < DistanceTalking)
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
                    if (Vector3.Distance(m_targetDetected.GetGameObject().transform.position, this.transform.position) < DistanceTalking)
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

        private void TalkToPlayer()
        {
            m_timerToTalk += Time.deltaTime;
            if (m_timerToTalk > 2)
            {
                m_timerToTalk = 0;
                switch (m_totalEnemiesDead)
                {
                    case 0:
                        Debug.Log(MessageToPlayer);
                        break;

                    case 1:
                        Debug.Log("Thanks for saving us but there are enemies left");
                        break;

                    case 2:
                        Debug.Log("You are doing a great job!!!");
                        break;

                    default:
                        Debug.Log("You are doing the best!!!");
                        break;
                }
            }
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
                if ((finalState != (int)NPC_STATES.DIE) && (finalState != (int)NPC_STATES.AWAIT_INSTRUCTIONS))
                {
                    if (m_state != (int)NPC_STATES.AWAIT_INSTRUCTIONS)
                    {
                        finalState = (int)NPC_STATES.AWAIT_INSTRUCTIONS;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            base.ChangeState(finalState);

            switch ((NPC_STATES)m_state)
            {
                case NPC_STATES.INITIAL:
                    Debug.Log("NPC(" + this.gameObject.name + ") to STATE [INITIAL]");
                    break;
                case NPC_STATES.CHECK_FOR_PLAYER:
                    if ((m_patrolComponent == null) || (m_patrolComponent.AreThereAnyWaypoints() == false))
                    {
                        if (m_previousState != (int)NPC_STATES.INITIAL)
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
                    Debug.Log("NPC(" + this.gameObject.name + ") to STATE [CHECK_FOR_PLAYER]");
                    break;
                case NPC_STATES.GO_TO_PLAYER:
                    ChangeAnimation((int)NPC_ANIMATION.ANIMATION_RUN);
                    if ((m_patrolComponent != null) && (m_patrolComponent.AreThereAnyWaypoints() == true))
                    {
                        m_patrolComponent.DeactivatePatrol();
                    }
                    Debug.Log("NPC(" + this.gameObject.name + ") to STATE [GO_TO_PLAYER]");
                    break;
                case NPC_STATES.TALK_TO_PLAYER:
                    ChangeAnimation((int)NPC_ANIMATION.ANIMATION_TALK);
                    Debug.Log("NPC(" + this.gameObject.name + ") to STATE [TALK_TO_PLAYER]");
                    break;
                case NPC_STATES.DIE:
                    if (m_patrolComponent != null)
                    {
                        m_patrolComponent.DeactivatePatrol();
                    }
                    if (m_areaVisionDetection != null)
                    {
                        m_areaVisionDetection.StopAreaDetection();
                    }
                    ChangeAnimation((int)NPC_ANIMATION.ANIMATION_DEATH);
                    SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_NPC_DEAD);
                    SoundsGameController.Instance.PlaySoundFX(SoundsGameController.FX_DEAD_NPC, false, 1);
                    Debug.Log("NPC(" + this.gameObject.name + ") to STATE [DIE]");
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

            switch ((NPC_STATES)m_state)
            {
                case NPC_STATES.INITIAL:
                    break;

                case NPC_STATES.CHECK_FOR_PLAYER:
                    if ((m_patrolComponent == null) || (m_patrolComponent.AreThereAnyWaypoints() == false))
                    {
                        GoToInitialPosition();
                    }

                    if (IsInDetectionRange() == true)
                    {
                        ChangeState((int)NPC_STATES.GO_TO_PLAYER);
                    }

                    if (IsInTalkingRange() == true)
                    {
                        ChangeState((int)NPC_STATES.TALK_TO_PLAYER);
                    }

                    if (m_life <= 0)
                    {
                        ChangeState((int)NPC_STATES.DIE);
                    }
                    break;

                case NPC_STATES.GO_TO_PLAYER:
                    if ((m_rotateComponent != null) && (m_targetDetected != null))
                    {
                        m_rotateComponent.ActivateRotation(m_targetDetected.GetGameObject().transform.position);
                    }
                    WalkToPlayer();

                    if (IsInDetectionRange() == false)
                    {
                        ChangeState((int)NPC_STATES.CHECK_FOR_PLAYER);
                    }

                    if (IsInTalkingRange() == true)
                    {
                        ChangeState((int)NPC_STATES.TALK_TO_PLAYER);
                    }

                    if (m_life <= 0)
                    {
                        ChangeState((int)NPC_STATES.DIE);
                    }
                    break;

                case NPC_STATES.TALK_TO_PLAYER:
                    if ((m_rotateComponent != null) && (m_targetDetected != null))
                    {
                        m_rotateComponent.ActivateRotation(m_targetDetected.GetGameObject().transform.position);
                    }
                    TalkToPlayer();

                    if (IsInDetectionRange() == false)
                    {
                        ChangeState((int)NPC_STATES.CHECK_FOR_PLAYER);
                    }

                    if (IsInTalkingRange() == false)
                    {
                        ChangeState((int)NPC_STATES.GO_TO_PLAYER);
                    }

                    if (m_life <= 0)
                    {
                        ChangeState((int)NPC_STATES.DIE);
                    }
                    break;

                case NPC_STATES.DIE:
                    m_timeCounter += Time.deltaTime;
                    if (m_timeCounter > 4)
                    {
                        ChangeState((int)NPC_STATES.NULL);
                        Destroy();
                    }
                    break;

                case NPC_STATES.AWAIT_INSTRUCTIONS:
                    RunCommands();
                    break;
            }
        }
    }
}