// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

namespace Wave.Essence.Interaction.Mode.Demo
{
	public class ControllerPoseModeTest : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Controller.Model.Demo.ControllerPoseModeTest";
		void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, m_Controller + " " + msg, true);
		}

		[SerializeField]
		private XR_Hand m_Controller = XR_Hand.Dominant;
		public XR_Hand Controller { get { return m_Controller; } set { m_Controller = value; } }

		[SerializeField]
		private Text m_Text = null;
		public Text ModeText { get { return m_Text; } set { m_Text = value; } }

		private void Start()
		{
			if (WXRDevice.GetRoleDevice(XR_Device.Head).TryGetFeatureValue(XR_Feature.userPresence, out bool value))
				DEBUG("Start() userPresence: " + value);
			else
				DEBUG("Start() Not support InputFeature - userPresence.");
		}

		WVR_ControllerPoseMode wvrPoseMode = WVR_ControllerPoseMode.WVR_ControllerPoseMode_Raw;
		void Update()
		{
			if (m_Text == null)
				return;
			if (Interop.WVR_GetControllerPoseMode((WVR_DeviceType)m_Controller, ref wvrPoseMode))
			{
				switch(wvrPoseMode)
				{
					case WVR_ControllerPoseMode.WVR_ControllerPoseMode_Raw:
						m_Text.text = (m_Controller == XR_Hand.Dominant ? "Right" : "Left") + ": Raw";
						break;
					case WVR_ControllerPoseMode.WVR_ControllerPoseMode_Trigger:
						m_Text.text = (m_Controller == XR_Hand.Dominant ? "Right" : "Left") + ": Trigger";
						break;
					case WVR_ControllerPoseMode.WVR_ControllerPoseMode_Panel:
						m_Text.text = (m_Controller == XR_Hand.Dominant ? "Right" : "Left") + ": Panel";
						break;
					case WVR_ControllerPoseMode.WVR_ControllerPoseMode_Handle:
						m_Text.text = (m_Controller == XR_Hand.Dominant ? "Right" : "Left") + ": Handle";
						break;
					default:
						break;
				}
			}
		}

		public void SetTriggerMode()
		{
			if (Interop.WVR_SetControllerPoseMode((WVR_DeviceType)m_Controller, WVR_ControllerPoseMode.WVR_ControllerPoseMode_Trigger))
				DEBUG("SetTriggerMode() succeeded.");
			else
				DEBUG("SetTriggerMode() failed.");
		}

		public void SetPanelMode()
		{
			if (Interop.WVR_SetControllerPoseMode((WVR_DeviceType)m_Controller, WVR_ControllerPoseMode.WVR_ControllerPoseMode_Panel))
				DEBUG("SetPanelMode() succeeded.");
			else
				DEBUG("SetPanelMode() failed.");
		}

		public void SetHandleMode()
		{
			if (Interop.WVR_SetControllerPoseMode((WVR_DeviceType)m_Controller, WVR_ControllerPoseMode.WVR_ControllerPoseMode_Handle))
				DEBUG("SetHandleMode() succeeded.");
			else
				DEBUG("SetHandleMode() failed.");
		}
	}
}
