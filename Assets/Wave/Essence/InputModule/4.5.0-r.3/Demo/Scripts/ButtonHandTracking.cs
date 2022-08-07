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
using Wave.Essence.Events;
using Wave.Essence.Hand;

namespace Wave.Essence.InputModule.Demo
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Button))]
	sealed class ButtonHandTracking : MonoBehaviour
	{
		[SerializeField]
		private HandManager.TrackerType m_Tracker = HandManager.TrackerType.Natural;
		public HandManager.TrackerType Tracker { get { return m_Tracker; } set { m_Tracker = value; } }

		private Button m_Button = null;
		private Text m_ButtonText = null;

		// Use this for initialization
		void Start()
		{
			m_Button = gameObject.GetComponent<Button>();
			m_ButtonText = gameObject.GetComponentInChildren<Text>();
		}

		/*HandManager.TrackerStatus hand_tracker_status = HandManager.TrackerStatus.NotStart;
		void Update()
		{
			hand_tracker_status = HandManager.Instance.GetHandTrackerStatus(m_Tracker);
			if (m_ButtonText != null && m_Button != null)
			{
				if (hand_tracker_status == HandManager.TrackerStatus.Available)
				{
					m_Button.interactable = true;
					m_ButtonText.text = "Disable Tracker " + m_Tracker.ToString();
				}
				else if (hand_tracker_status == HandManager.TrackerStatus.NotStart || hand_tracker_status == HandManager.TrackerStatus.StartFailure)
				{
					m_Button.interactable = true;
					m_ButtonText.text = "Enable Tracker " + m_Tracker.ToString();
				}
				else
				{
					m_Button.interactable = false;
					m_ButtonText.text = "Processing Tracker " + m_Tracker.ToString();
				}
			}
		}*/

		void OnEnable()
		{
			GeneralEvent.Listen(HandManager.HAND_TRACKER_STATUS, OnTrackerStatus);
		}

		void OnDisable()
		{
			GeneralEvent.Remove(HandManager.HAND_TRACKER_STATUS, OnTrackerStatus);
		}

		private void OnTrackerStatus(params object[] args)
		{
			HandManager.TrackerType tracker = (HandManager.TrackerType)args[0];
			HandManager.TrackerStatus status = (HandManager.TrackerStatus)args[1];
			Log.d("ButtonHandTracking", "Hand tracker: " + tracker + " status: " + status, true);
		}

		public void EnableHandTracker()
		{
			HandManager.Instance.StartHandTracker(m_Tracker);
		}

		public void DisableHandTracker()
		{
			HandManager.Instance.StopHandTracker(m_Tracker);
		}
	}
}
