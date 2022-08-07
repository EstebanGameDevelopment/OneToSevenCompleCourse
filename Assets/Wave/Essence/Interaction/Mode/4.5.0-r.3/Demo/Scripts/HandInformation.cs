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
	public class HandInformation : MonoBehaviour
	{
		const string kValidPose = "Valid Pose: ";
		const string kConfidence = "Confidence: ";
		const string kHandScale = "Hand Scale" ;
		const string kHandMotion = "Hand Motion: ";
		const string kHoldRole = "Hold Role: ";
		const string kHoldObject = "Hold Object: ";
		const string kPinchOrigin = "Pinch Origin: ";
		const string kPinchDirection = "Pinch Direction: ";
		const string kPinchStrength = "Pinch Strength: ";
		const string kPinchThreshold = "Pinch Strength: ";

		public bool IsLeftHand = false;
		public Text ValidPose = null;
		public Text Confidence = null;
		public Text HandScale = null;
		Vector3 handScale = Vector3.zero;
		public Text HandMotion = null;
		public Text HoldRole = null;
		public Text HoldObject = null;
		public Text PinchOrigin = null;
		Vector3 pinchOrigin = Vector3.zero;
		public Text PinchDirection = null;
		Vector3 pinchDirection = Vector3.zero;
		public Text PinchStrength = null;
		public Text PinchThreshold = null;

		void Update()
		{
			if (HandManager.Instance == null) { return; }

			if (ValidPose != null)
				ValidPose.text = kValidPose + HandManager.Instance.IsHandPoseValid(IsLeftHand);
			if (Confidence != null)
				Confidence.text = kConfidence + HandManager.Instance.GetHandConfidence(IsLeftHand);
			if (HandScale != null)
			{
				HandManager.Instance.GetHandScale(ref handScale, IsLeftHand);
				HandScale.text = kHandScale + handScale.x + ", " + handScale.y + ", " + handScale.z;
			}
			if (HandMotion != null)
				HandMotion.text = kHandMotion + HandManager.Instance.GetHandMotion(IsLeftHand);
			if (HoldRole != null)
				HoldRole.text = kHoldRole + HandManager.Instance.GetHandHoldRole(IsLeftHand);
			if (HoldObject != null)
				HoldObject.text = kHoldObject + HandManager.Instance.GetHandHoldType(IsLeftHand);
			if (PinchOrigin != null)
			{
				HandManager.Instance.GetPinchOrigin(ref pinchOrigin, IsLeftHand);
				PinchOrigin.text = kPinchOrigin + pinchOrigin.x + ", " + pinchOrigin.y + ", " + pinchOrigin.z;
			}
			if (PinchDirection != null)
			{
				HandManager.Instance.GetPinchDirection(ref pinchDirection, IsLeftHand);
				PinchDirection.text = kPinchDirection + pinchDirection.x + ", " + pinchDirection.y + ", " + pinchDirection.z;
			}
			if (PinchStrength != null)
				PinchStrength.text = kPinchStrength + HandManager.Instance.GetPinchStrength(IsLeftHand);
			if (PinchThreshold != null)
				PinchThreshold.text = kPinchThreshold + HandManager.Instance.GetPinchThreshold();
		}
	}
}
