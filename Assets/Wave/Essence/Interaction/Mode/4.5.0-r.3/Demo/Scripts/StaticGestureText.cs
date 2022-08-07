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
using Wave.Essence.Hand;
using Wave.Essence.Hand.StaticGesture;

namespace Wave.Essence.Interaction.Mode.Demo
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Text))]
	sealed class StaticGestureText : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Interaction.Mode.Demo.StaticGestureText";
		private void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, m_Hand + ", " + msg, true);
		}

		[SerializeField]
		private HandManager.HandType m_Hand = HandManager.HandType.Right;
		public HandManager.HandType Hand { get { return m_Hand; } set { m_Hand = value; } }

		private Text m_Text = null;

		#region MonoBehaviour Overrides
		void Start()
		{
			m_Text = gameObject.GetComponent<Text>();
		}

		void Update()
		{
			if (m_Text == null) { return; }

			string gesture = WXRGestureHand.GetSingleHandGesture(m_Hand == HandManager.HandType.Left ? true : false);
			HandState hs = WXRGestureHand.GetState(m_Hand == HandManager.HandType.Left ? true : false);

			m_Text.text = m_Hand + " Gesture: " + gesture +
				(hs != null ?
					("\n" + hs.thumb + ", " + hs.index + ", " + hs.middle + ", " + hs.ring + ", " + hs.pinky) : ""
				);
		}
		#endregion

		private GestureType m_HandGesture = GestureType.Unknown;
		public void OnStaticGesture(GestureType gesture)
		{
			DEBUG("OnStaticGesture() " + gesture);
			m_HandGesture = gesture;
		}
	}
}
