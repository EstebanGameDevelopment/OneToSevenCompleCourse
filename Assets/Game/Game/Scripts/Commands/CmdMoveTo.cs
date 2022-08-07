using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class CmdMoveTo : Command
    {
        private Avatar m_avatar;
        private Vector3 m_target;
        private float m_speed;

        private bool m_isRunning = false;
        private bool m_animationUpdated = false;

        private NavMeshAgent m_navigation;
        private bool m_isUsingPathfinding = false;
        private bool m_destroyOnTarget = false;

        public CmdMoveTo(Avatar _avatar, Vector3 _target, float _speed, bool _destroyOnTarget = false)
        {
            m_avatar = _avatar;
            m_target = _target;
            m_speed = _speed;
            m_destroyOnTarget = _destroyOnTarget;

            m_navigation = m_avatar.gameObject.GetComponent<NavMeshAgent>();
        }

        public override void Init()
        {
            if (!m_hasBeenInited)
            {
                m_hasBeenInited = true;

                Utilities.ActivatePhysics(m_avatar.gameObject, false);
                if (m_navigation != null)
                {
                    m_navigation.enabled = false;
                    Vector3 target = new Vector3(m_target.x, m_avatar.gameObject.transform.position.y, m_target.z);
                    if (Utilities.IsThereObstacleBetweenPosition(m_avatar.gameObject.transform.position, target, GameController.LAYER_PLAYER, GameController.LAYER_NPC, GameController.LAYER_ENEMY))
                    {
                        m_isUsingPathfinding = true;
                        Utilities.ActivatePhysics(m_avatar.gameObject, false);
                        m_navigation.enabled = true;
                        m_navigation.SetDestination(m_target);
                    }
                }
            }
        }

        private void ChangeRunningState(bool _isRunning)
        {
            if (m_isRunning != _isRunning)
            {
                m_animationUpdated = false;
            }
            m_isRunning = _isRunning;
            if (!m_animationUpdated)
            {
                m_animationUpdated = true;
                if (m_isRunning)
                {
                    m_avatar.ChangeAnimationRun();
                }
                else
                {
                    m_avatar.ChangeAnimationIdle();
                }
            }
        }

        public override void Execute()
        {
            Init();

            if (!m_isUsingPathfinding)
            {
                Utilities.ActivatePhysics(m_avatar.gameObject, true);

                if (m_avatar.gameObject.GetComponent<RotateToTarget>() != null)
                {
                    m_avatar.gameObject.GetComponent<RotateToTarget>().ActivateRotation(m_target);
                }

                Vector3 direction = Utilities.GetDirection(m_target, m_avatar.gameObject.transform.position);
                m_avatar.gameObject.transform.GetComponent<Rigidbody>().MovePosition(m_avatar.gameObject.transform.position + (direction * m_speed * Time.deltaTime));
            }
            else
            {
                m_avatar.GetComponent<Avatar>().ForwardMovementPathfinding(m_navigation.velocity.normalized);
            }
        }

        public override bool IsFinished()
        {
            if (Utilities.DistanceXZ(m_avatar.gameObject.transform.position, m_target) < 1)
            {
                m_navigation.enabled = false;
                if (m_isUsingPathfinding) m_avatar.GetComponent<Avatar>().RestoreInitialForwardModel();
                Utilities.ActivatePhysics(m_avatar.gameObject, true);
                if (m_destroyOnTarget)
                {
                    SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_AVATAR_DESTROY_IT, m_avatar);
                }
                else
                {
                    ChangeRunningState(false);
                }
                return true;
            }
            else
            {
                ChangeRunningState(true);
                return false;
            }
        }

        public override bool IsBlocking()
        {
            return true;
        }

        public override void Destroy()
        {
            if (m_isUsingPathfinding) m_avatar.GetComponent<Avatar>().RestoreInitialForwardModel();
            Utilities.ActivatePhysics(m_avatar.gameObject, false);
            if (m_navigation != null)
            {
                m_navigation.enabled = false;
                m_navigation = null;
            }
            m_avatar.ReenablePhysicsDelayed();
        }
    }
}