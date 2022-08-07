using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence;
using Wave.Native;

public class PoseMode : MonoBehaviour
{
	const string LOG_TAG = "Controller.Model.CPoseMode";
	void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d(LOG_TAG, msg, true);
	}
	public XR_Hand WhichHand = XR_Hand.Dominant;
	private WVR_ControllerPoseMode poseMode = WVR_ControllerPoseMode.WVR_ControllerPoseMode_Panel;
	private WVR_DeviceType deviceType = WVR_DeviceType.WVR_DeviceType_Invalid;
	private Vector3 currPosOffset = Vector3.zero;
	private Quaternion currRotOffset = Quaternion.identity;

	// Start is called before the first frame update
	void Start()
    {
		if (WhichHand == XR_Hand.Dominant)
			deviceType = WVR_DeviceType.WVR_DeviceType_Controller_Right;
		else
			deviceType = WVR_DeviceType.WVR_DeviceType_Controller_Left;
			
		if (Interop.WVR_GetControllerPoseMode(deviceType, ref poseMode))
		{
			DEBUG(WhichHand + " is using " + poseMode + " on start.");
		} else
		{
			DEBUG(WhichHand + " get pose mode fail");
		}
	}

	//private void OnApplicationPause(bool pause)
	//{
	//	if (!pause)
	//	{
	//		if (Interop.WVR_GetControllerPoseMode(deviceType, ref poseMode))
	//		{
	//			DEBUG("onResume" + WhichHand + " is using " + poseMode);
	//		}
	//		else
	//		{
	//			DEBUG(WhichHand + " get pose mode fail");
	//		}
	//	}
	//}

	// Update is called once per frame
	void Update()
    {
		var vec = WaveEssence.Instance.GetCurrentControllerPositionOffset(deviceType);
		var rot = WaveEssence.Instance.GetCurrentControllerRotationOffset(deviceType);
		if (vec != currPosOffset || rot != currRotOffset)
		{
			DEBUG("Controller pose mode changed");
			if (Interop.WVR_GetControllerPoseMode(deviceType, ref poseMode))
			{
				DEBUG(WhichHand + " is changed to " + poseMode);
			}
			else
			{
				DEBUG(WhichHand + " get pose mode fail");
			}

			currPosOffset = vec;
			currRotOffset = rot;

			this.transform.localPosition = currPosOffset;
			this.transform.localRotation = currRotOffset;

			DEBUG(WhichHand + " Pos offset: " + vec.x + ", " + vec.y + ", " + vec.z + ", Rot offset: " + rot.x + ", " + rot.y + ", " + rot.z + ", " + rot.w);
			
		}
    }
}
