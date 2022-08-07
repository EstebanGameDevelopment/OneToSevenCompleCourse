// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using UnityEngine.UI;
using Wave.Native;
using Wave.Essence.Eye;

namespace Wave.Essence.InputModule.Demo
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Text))]
	public sealed class EyeTrackingButton : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Eye.Demo.EyeTrackingButton";
		private void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}

		private Text m_Text = null;

		void OnEnable()
		{
			m_Text = GetComponentInChildren<Text>();
		}

		void Update()
		{
			if (m_Text == null || EyeManager.Instance == null)
				return;

			if (EyeManager.Instance)
			{
				if (EyeManager.Instance.IsEyeTrackingAvailable())
					m_Text.text = "Disable Eye Tracking";
				else
					m_Text.text = "Enable Eye Tracking";
			}
			else
			{
				m_Text.text = "No Eye Manager exists.";
			}
		}

		public void EnableEyeTracking()
		{
			if (EyeManager.Instance == null)
				return;

			if (EyeManager.Instance.IsEyeTrackingAvailable())
			{
				DEBUG("EnableEyeTracking() Stop eye tracking.");
				EyeManager.Instance.EnableEyeTracking = false;
			}
			else
			{
				DEBUG("EnableEyeTracking() Start eye tracking.");
				EyeManager.Instance.EnableEyeTracking = true;
			}
		}
	}
}
