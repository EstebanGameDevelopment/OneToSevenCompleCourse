using UnityEngine;
using Pvr_UnitySDKAPI;

public class PicoDevice : MonoBehaviour
{
    public enum DeviceType
    {
        HMD,
        LeftController,
        RightController
    }

    public DeviceType deviceType;

    private Vector3 devicePos;
    private Quaternion deviceRot;

    private void Awake()
    {
        if (deviceType == DeviceType.HMD)
        {
            gameObject.AddComponent<Camera>();

            GameObject leftEye = new GameObject();
            leftEye.name = "LeftEye";
            leftEye.transform.parent = transform;
            leftEye.AddComponent<Pvr_UnitySDKEye>().eyeSide = Eye.LeftEye;

            GameObject rightEye = new GameObject();
            rightEye.name = "RightEye";
            rightEye.transform.parent = transform;
            rightEye.AddComponent<Pvr_UnitySDKEye>().eyeSide = Eye.RightEye;

            GameObject bothEye = new GameObject();
            bothEye.name = "BothEye";
            bothEye.transform.parent = transform;
            bothEye.AddComponent<Pvr_UnitySDKEye>().eyeSide = Eye.BothEye;

            gameObject.AddComponent<Pvr_UnitySDKEyeManager>();
        }
    }

    private void Update()
    {
        switch (deviceType)
        {
            case DeviceType.HMD:
                devicePos = Pvr_UnitySDKSensor.Instance.HeadPose.Position;
                deviceRot = Pvr_UnitySDKSensor.Instance.HeadPose.Orientation;
                break;
            case DeviceType.LeftController:
                devicePos = Pvr_ControllerManager.controllerlink.Controller0.Position;
                deviceRot = Pvr_ControllerManager.controllerlink.Controller0.Rotation;
                break;
            case DeviceType.RightController:
                devicePos = Pvr_ControllerManager.controllerlink.Controller1.Position;
                deviceRot = Pvr_ControllerManager.controllerlink.Controller1.Rotation;
                break;
            default:
                break;
        }
        transform.localPosition = devicePos;
        transform.localRotation = deviceRot;
    }

}
