using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class CmdLookAt : Command
    {
        private Avatar m_avatar;
        private Vector3 m_target;
        private float m_timeToLook;

        private float m_timeCounter = 0;

        public CmdLookAt(Avatar _avatar, Vector3 _target, float _timeToLook)
        {
            m_avatar = _avatar;
            m_target = _target;
            m_timeToLook = _timeToLook;
        }

        public override void Init()
        {
        }

        public override void Execute()
        {
            if (m_avatar.gameObject.GetComponent<RotateToTarget>() != null)
            {
                m_avatar.gameObject.GetComponent<RotateToTarget>().ActivateRotation(m_target);
            }
            else
            {
                m_avatar.gameObject.transform.rotation = Quaternion.LookRotation((m_target - m_avatar.gameObject.transform.position).normalized, Vector3.up);
            }
        }

        public override bool IsFinished()
        {
            m_timeCounter += Time.deltaTime;
            if (m_timeCounter > m_timeToLook)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool IsBlocking()
        {
            return true;
        }

        public override void Destroy()
        {
        }
    }
}