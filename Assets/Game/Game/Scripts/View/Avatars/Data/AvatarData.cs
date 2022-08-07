using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class AvatarData
    {
        public Vector3 Position;
        public Vector3 Scale;

        // AVATAR
        public string PrefabName;
        int InitialLife;
        float Speed;
        bool UseNavigation;

        // ENEMY
        float DetectionDistance = 10;
        float ShootingDistance = 5;
        GameObject BulletEnemy;
        bool ActivateWait = false;

        // NPC
        float NPCDetectionDistance = 10;
        float TalkingDistance = 5;

        // WAYPOINTS
        bool ExistsWaypoints;
        List<Waypoint> Waypoints = new List<Waypoint>();
        int CurrentWaypoint;
        bool UseNavigationWay;
        bool AutoStart;

        // VISION
        public bool ExistsVision;
        public Vision AreaVisionDetection;

        // ROTATE TO TARGET
        bool ExistsRotation;
        float RotationSpeed;

        public AvatarData(NPC _npc)
        {
            NPCDetectionDistance = _npc.DistanceDetection;
            TalkingDistance = _npc.DistanceTalking;

            CopyAvatar(_npc);
        }

        public AvatarData(Enemy _enemy)
        {
            // ENEMY
            DetectionDistance = _enemy.DistanceDetection;
            ShootingDistance = _enemy.DistanceShooting;
            BulletEnemy = _enemy.BulletEnemy;
            ActivateWait = _enemy.ActivateWait;

            CopyAvatar(_enemy);
        }

        void CopyAvatar(Avatar _avatar)
        {
            Position = _avatar.transform.position;
            Scale = _avatar.transform.localScale;

            PrefabName = _avatar.PrefabName;
            InitialLife = _avatar.InitialLife;
            Speed = _avatar.Speed;

            // WAYPOINTS
            PatrolWaypoints patrolWaypoints = _avatar.gameObject.GetComponent<PatrolWaypoints>();
            ExistsWaypoints = (patrolWaypoints != null);
            if (ExistsWaypoints)
            {
                CurrentWaypoint = patrolWaypoints.CurrentWaypoint;
                AutoStart = patrolWaypoints.AutoStart;
                patrolWaypoints.CopyPatrolWaypoints(Waypoints);
            }

            // VISION
            AreaVisionDetection areaVisionDetection = _avatar.gameObject.GetComponent<AreaVisionDetection>();
            ExistsVision = (areaVisionDetection != null);
            if (ExistsVision)
            {
                AreaVisionDetection = new Vision();
                AreaVisionDetection.Set(areaVisionDetection.Vision);
            }

            // ROTATE TO TARGET
            RotateToTarget rotateToTarget = _avatar.gameObject.GetComponent<RotateToTarget>();
            ExistsRotation = (rotateToTarget != null);
            if (ExistsRotation)
            {
                RotationSpeed = rotateToTarget.RotationSpeed;
            }
        }

        public void InitEnemy(Enemy _enemy)
        {
            // ENEMY
            _enemy.DistanceDetection = DetectionDistance;
            _enemy.DistanceShooting = ShootingDistance;
            _enemy.BulletEnemy = BulletEnemy;
            _enemy.ActivateWait = ActivateWait;

            InitAvatar(_enemy);
        }

        public void InitNPC(NPC _npc)
        {
            _npc.DistanceDetection = NPCDetectionDistance;
            _npc.DistanceTalking = TalkingDistance;

            InitAvatar(_npc);
        }

        private void InitAvatar(Avatar _avatar)
        {
            _avatar.transform.position = Position;
            _avatar.transform.localScale = Scale;

            _avatar.PrefabName = PrefabName;
            _avatar.InitialLife = InitialLife;
            _avatar.Speed = Speed;

            // WAYPOINTS
            PatrolWaypoints patrolWaypoints = _avatar.GetComponent<PatrolWaypoints>();
            if (patrolWaypoints != null)
            {
                if (!ExistsWaypoints)
                {
                    patrolWaypoints.enabled = false;
                }
                else
                {
                    patrolWaypoints.CurrentWaypoint = CurrentWaypoint;
                    patrolWaypoints.AutoStart = AutoStart;
                    patrolWaypoints.SetPatrolWaypoints(Waypoints);
                }
            }

            // VISION
            AreaVisionDetection areaVisionDetection = _avatar.gameObject.GetComponent<AreaVisionDetection>();
            if (areaVisionDetection != null)
            {
                if (!ExistsVision)
                {
                    areaVisionDetection.enabled = false;
                }
                else
                {
                    areaVisionDetection.SetVision(AreaVisionDetection);
                }
            }

            // ROTATE TO TARGET
            RotateToTarget rotateToTarget = _avatar.gameObject.GetComponent<RotateToTarget>();
            if (rotateToTarget != null)
            {
                if (!ExistsRotation)
                {
                    rotateToTarget.enabled = false;
                }
                else
                {
                    rotateToTarget.RotationSpeed = RotationSpeed;
                }
            }

        }
    }
}