using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourVRExperience.Game
{
    [System.Serializable]
    public class Vision
    {
        public Material Material;
        public float DetectionDistance = 8;
        public float DetectionAngle = 40;
        public float Orientation = 90;
        public float HeightToFloor = -0.75f;
        public AreaVisionDetection.TargetCharacters Target = AreaVisionDetection.TargetCharacters.PLAYER;
        public bool UseBehavior = true;
        public float HeightDetection = 4;

        public Vision Clone()
        {
            Vision output = new Vision();
            output.Material = Material;
            output.DetectionDistance = DetectionDistance;
            output.DetectionAngle = DetectionAngle;
            output.Orientation = Orientation;
            output.HeightToFloor = HeightToFloor;
            output.Target = Target;
            output.UseBehavior = UseBehavior;
            output.HeightDetection = HeightDetection;
            return output;
        }

        public void Set(Vision _vision)
        {
            Material = _vision.Material;
            DetectionDistance = _vision.DetectionDistance;
            DetectionAngle = _vision.DetectionAngle;
            Orientation = _vision.Orientation;
            HeightToFloor = _vision.HeightToFloor;
            Target = _vision.Target;
            UseBehavior = _vision.UseBehavior;
            HeightDetection = _vision.HeightDetection;
        }

        public void Set(float _detectionDistance,
                        float _detectionAngle,
                        float _orientation,
                        float _heightToFloor,
                        AreaVisionDetection.TargetCharacters _target,
                        bool _UseBehavior,
                        float _HeightDetection)
        {
            DetectionDistance = _detectionDistance;
            DetectionAngle = _detectionAngle;
            Orientation = _orientation;
            HeightToFloor = _heightToFloor;
            Target = _target;
            UseBehavior = _UseBehavior;
            HeightDetection = _HeightDetection;
        }
    }
}