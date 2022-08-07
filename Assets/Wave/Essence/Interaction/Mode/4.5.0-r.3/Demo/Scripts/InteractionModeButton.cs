// "Wave SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC\u2019s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using Wave.Native;

namespace Wave.Essence.Interaction.Mode.Demo
{
	[DisallowMultipleComponent]
	public class InteractionModeButton : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Interaction.Mode.Demo.InteractionModeButton";
		private void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}

		public void GazeMode()
		{
			if (InteractionModeManager.Instance != null)
			{
				DEBUG("Set interaction mode to Gaze.");
				//obsolete ClientInterface.InteractionMode = XR_InteractionMode.Gaze;
			}
		}
		public void ControllerMode()
		{
			if (InteractionModeManager.Instance != null)
			{
				DEBUG("Set interact ion mode to Controller.");
				//obsolete ClientInterface.InteractionMode = XR_InteractionMode.Controller;
			}
		}
		public void HandMode()
		{
			if (InteractionModeManager.Instance != null)
			{
				DEBUG("Set interaction mode to Hand.");
				//obsolete ClientInterface.InteractionMode = XR_InteractionMode.Hand;
			}
		}
	}
}
