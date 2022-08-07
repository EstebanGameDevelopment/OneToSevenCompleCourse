using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class Bullet : Avatar
    {
        public const int TYPE_BULLET_PLAYER = 0;
        public const int TYPE_BULLET_ENEMY = 1;

        public int Damage;
        public int Type;

        public void Shoot(int _type, Vector3 _position, Vector3 _direction)
        {
            Type = _type;
            this.transform.position = _position;
            this.transform.forward = _direction;

            SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_BULLET_HAS_STARTED, this);
        }

        private void DestroyBullet()
        {
            SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_BULLET_HAS_BEEN_DESTROYED, this);
            GameObject.Destroy(this.gameObject);
        }

        public override void InitLogic()
        {
        }

        public override void StopLogic()
        {
        }

        public override bool IsAnimationRun()
        {
            return false;
        }

        public override bool IsAnimationIdle()
        {
            return false;
        }

        public override bool IsSelectable()
        {
            return false;
        }

        public override void ChangeAnimationRun()
        {
        }

        public override void ChangeAnimationIdle()
        {
        }

        public override void ChangeAnimationAttack()
        {
        }
        public override void ForwardMovementPathfinding(Vector3 _velocity)
        {
        }

        public override void ShootAtTarget(Vector3 _target, bool _forceShoot)
        {
        }

        protected override void OnTriggerEnter(Collider _collision)
        {
            Debug.Log("Bullet=" + this.gameObject.name + " HAS DETECTED A COLLISION AGAINST " + _collision.gameObject.name + "!!!!!");
        }

        void OnCollisionEnter(Collision _collision)
        {
            if (Type == TYPE_BULLET_PLAYER)
            {
                if (_collision.gameObject.GetComponent<Enemy>() != null)
                {
                    Debug.Log(this.gameObject.name + " COLLISION AGAINST ENEMY");
                    _collision.gameObject.GetComponent<Enemy>().DecreaseLife(Damage);
                    DestroyBullet();
                }
                if (_collision.gameObject.GetComponent<NPC>() != null)
                {
                    Debug.Log(this.gameObject.name + " COLLISION AGAINST NPC");
                    _collision.gameObject.GetComponent<NPC>().DecreaseLife(Damage);
                    DestroyBullet();
                }

            }
            if (Type == TYPE_BULLET_ENEMY)
            {
                if (_collision.gameObject.GetComponent<Player>() != null)
                {
                    Debug.Log(this.gameObject.name + " COLLISION AGAINST PLAYER");
                    _collision.gameObject.GetComponent<Player>().DecreaseLife(Damage);
                    DestroyBullet();
                }
                if (_collision.gameObject.GetComponent<NPC>() != null)
                {
                    Debug.Log(this.gameObject.name + " COLLISION AGAINST NPC");
                    _collision.gameObject.GetComponent<NPC>().DecreaseLife(Damage);
                    DestroyBullet();
                }
                if (_collision.gameObject.GetComponent<Enemy>() != null)
                {
                    Debug.Log(this.gameObject.name + " COLLISION AGAINST ENEMY");
                    _collision.gameObject.GetComponent<Enemy>().DecreaseLife(Damage);
                    DestroyBullet();
                }
            }
        }

        public override void UpdateLogic()
        {
            m_timeCounter += Time.deltaTime;
            if (m_timeCounter > 6)
            {
                DestroyBullet();
            }
            else
            {
                MoveToPosition(this.gameObject.transform.forward * Speed * Time.deltaTime);
            }
        }

    }
}