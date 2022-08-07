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
using Wave.Essence.Hand;

namespace Wave.Essence.Interaction.Mode.Demo
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Text))]
	sealed class PoseValidText : MonoBehaviour
	{
		public HandManager.HandType Hand = HandManager.HandType.Right;
		private Text m_Text = null;
		void Start()
		{
			m_Text = GetComponent<Text>();
		}

		void Update()
		{
			if (m_Text == null || HandManager.Instance == null) { return; }

			m_Text.text = Hand + " Pose: "
				+ (HandManager.Instance.IsHandPoseValid(Hand) ? "valid" : "INVALID");
		}
	}
}
