using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class CmdAttack : Command
    {
        private Avatar m_avatar;
        private Avatar m_target;
        private float m_speed;
        private float m_attackRange;
        private float m_timeoutToAttack;

        private NavMeshAgent m_navigation;
        private bool m_isUsingPathfinding = false;
        private bool m_isInAttackRange = false;
        private float m_timeAttack = 0;
        private bool m_isRunning = false;
        private bool m_firstIteration = true;

        public CmdAttack(Avatar _avatar, Avatar _target, float _speed, float _attackRange, float _timeOutToAttack)
        {
            m_avatar = _avatar;
            m_target = _target;
            m_speed = _speed;
            m_attackRange = _attackRange;
            m_timeoutToAttack = _timeOutToAttack;
            m_timeAttack = m_timeoutToAttack;

            m_navigation = m_avatar.gameObject.GetComponent<NavMeshAgent>();
        }

        public override void Init()
        {
            if (!m_hasBeenInited)
            {
                m_hasBeenInited = true;

                EnableNavigation(false);
                Utilities.ActivatePhysics(m_avatar.gameObject, false);
                UpdateIsInRange();
            }
        }

        private bool UpdateIsInRange()
        {
            bool isInAttackRange = m_isInAttackRange;
            m_isUsingPathfinding = Utilities.IsThereObstacleBetweenPosition(m_avatar.transform.transform.position, m_target.transform.position, GameController.LAYER_PLAYER, GameController.LAYER_ENEMY, GameController.LAYER_NPC);
            m_isInAttackRange = false;
            if (!m_isUsingPathfinding)
            {
                EnableNavigation(false);
                if (Vector3.Distance(m_avatar.transform.transform.position, m_target.transform.position) < m_attackRange)
                {
                    m_isInAttackRange = true;
                }
            }
            else
            {
                EnableNavigation(true);
            }
            if (isInAttackRange)
            {
                if (isInAttackRange != m_isInAttackRange)
                {
                    m_firstIteration = true;
                }
            }
            if (m_isInAttackRange)
            {
                if (!m_avatar.IsAnimationIdle())
                {
                    m_avatar.ChangeAnimationIdle();
                }
            }
            else
            {
                if (!m_avatar.IsAnimationRun())
                {
                    m_avatar.ChangeAnimationRun();
                }
            }
            return m_isInAttackRange;
        }

        private void EnableNavigation(bool _enable)
        {
            if (m_navigation != null)
            {
                if (_enable)
                {
                    if (!m_navigation.enabled)
                    {
                        m_navigation.enabled = true;
                    }
                    Utilities.ActivatePhysics(m_avatar.gameObject, false);
                }
                else
                {
                    if (m_navigation.enabled)
                    {
                        m_navigation.isStopped = true;
                        m_navigation.enabled = false;
                    }
                    Utilities.ActivatePhysics(m_avatar.gameObject, true);
                }
            }
        }

        public override void Execute()
        {
            Init();

            if (m_isInAttackRange)
            {
                m_timeAttack += Time.deltaTime;
                if (m_timeAttack > m_timeoutToAttack)
                {
                    m_timeAttack = 0;
                    m_avatar.ChangeAnimationAttack();
                    m_avatar.ShootAtTarget(m_target.transform.position, true);
                }

                if (UpdateIsInRange())
                {
                    EnableNavigation(false);
                    if (m_avatar.gameObject.GetComponent<RotateToTarget>() != null)
                    {
                        m_avatar.gameObject.GetComponent<RotateToTarget>().ActivateRotation(m_target.transform.position);
                    }
                    else
                    {
                        m_avatar.gameObject.transform.rotation = Quaternion.LookRotation((m_target.transform.position - m_avatar.gameObject.transform.position).normalized, Vector3.up);
                    }
                }
            }
            else
            {
                if (m_firstIteration)
                {
                    m_firstIteration = false;
                    if (m_isUsingPathfinding)
                    {
                        EnableNavigation(true);
                    }
                    else
                    {
                        m_avatar.RestoreInitialForwardModel();
                        EnableNavigation(false);
                    }
                }

                if (!m_isUsingPathfinding)
                {
                    Utilities.ActivatePhysics(m_avatar.gameObject, true);
                    Vector3 direction = Utilities.GetDirection(m_target.transform.position, m_avatar.gameObject.transform.position);
                    m_avatar.gameObject.transform.GetComponent<Rigidbody>().MovePosition(m_avatar.gameObject.transform.position + (direction * m_speed * Time.deltaTime));
                    if (m_avatar.gameObject.GetComponent<RotateToTarget>() != null)
                    {
                        m_avatar.gameObject.GetComponent<RotateToTarget>().ActivateRotation(m_target.transform.position);
                    }
                    else
                    {
                        m_avatar.gameObject.transform.rotation = Quaternion.LookRotation((m_target.transform.position - m_avatar.gameObject.transform.position).normalized, Vector3.up);
                    }
                }
                else
                {
                    m_navigation.SetDestination(m_target.transform.position);
                    m_avatar.GetComponent<Avatar>().ForwardMovementPathfinding(m_navigation.velocity.normalized);
                }

                if (Vector3.Distance(m_avatar.transform.position, m_target.transform.position) < m_attackRange)
                {
                    UpdateIsInRange();
                }
            }
        }

        public override bool IsFinished()
        {
            if (m_target == null)
            {
                m_avatar.ChangeAnimationIdle();
                EnableNavigation(false);
                return true;
            }
            else
            {
                if (m_target.Life <= 0)
                {
                    m_avatar.ChangeAnimationIdle();
                    EnableNavigation(false);
                    return true;
                }
                else
                {
                    return false;
                }
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