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
using Wave.Essence.Hand;

#if UNITY_EDITOR
using UnityEditor;
namespace Wave.Essence.InputModule.Editor
{
	[CustomEditor(typeof(HandInputModule))]
	public class HandInputModuleEditor : UnityEditor.Editor
	{
		SerializedProperty m_RightHandSelector, m_LeftHandSelector, m_UseDefaultPinch, m_PinchOnThreshold, m_PinchTimeToDrag, m_PinchOffThreshold, m_IgnoreMode;
		private void OnEnable()
		{
			m_RightHandSelector = serializedObject.FindProperty("m_RightHandSelector");
			m_LeftHandSelector = serializedObject.FindProperty("m_LeftHandSelector");
			m_UseDefaultPinch = serializedObject.FindProperty("m_UseDefaultPinch");
			m_PinchOnThreshold = serializedObject.FindProperty("m_PinchOnThreshold");
			m_PinchOffThreshold = serializedObject.FindProperty("m_PinchOffThreshold");
			m_PinchTimeToDrag = serializedObject.FindProperty("m_PinchTimeToDrag");
			m_IgnoreMode = serializedObject.FindProperty("m_IgnoreMode");
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			HandInputModule myScript = target as HandInputModule;

			GUILayout.Space(5);
			EditorGUILayout.HelpBox(
				"Chooses a selector for the right hand. A seletor should contain:\n" +
				"- A HandBeam object.\n" +
				"- A HandSpotPointer object.\n" +
				"You can click the following button to create and set a right selector automatically.",
				MessageType.Info);
			if (GUILayout.Button("Creates a right selector."))
				CreateHand(target as HandInputModule, HandManager.HandType.Right);
			EditorGUILayout.PropertyField(m_RightHandSelector);

			GUILayout.Space(5);
			EditorGUILayout.HelpBox(
				"Chooses a selector for the left hand. A seletor should contain:\n" +
				"- A HandBeam object.\n" +
				"- A HandSpotPointer object.\n" +
				"You can click the following button to create and set a left selector automatically.",
				MessageType.Info);
			if (GUILayout.Button("Creates a left selector."))
				CreateHand(target as HandInputModule, HandManager.HandType.Left);
			EditorGUILayout.PropertyField(m_LeftHandSelector);

			EditorGUILayout.HelpBox(
				"Use the system default pinch threshold.",
				MessageType.Info);
			myScript.UseDefaultPinch = EditorGUILayout.Toggle("Use Default Pinch", myScript.UseDefaultPinch);

			if (!myScript.UseDefaultPinch)
			{
				EditorGUILayout.HelpBox(
					"When the pinch strength is over threshold, the HandInputModule will start sending events",
					MessageType.Info);
				EditorGUILayout.PropertyField(m_PinchOnThreshold);

				EditorGUILayout.HelpBox(
					"The HandInputModule will keep sending events until the Pinch strength is smaller than the Pinch Off Threshold.",
					MessageType.Info);
				EditorGUILayout.PropertyField(m_PinchOffThreshold);
			}

			EditorGUILayout.HelpBox(
				"The HandInputModule will start sending drag events when it is pointing to the same object over this duration.",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_PinchTimeToDrag);

			EditorGUILayout.HelpBox(
				"The HandInputModule will send events ignoring the interaction mode.",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_IgnoreMode);

			serializedObject.ApplyModifiedProperties();

			if (GUI.changed)
				EditorUtility.SetDirty((HandInputModule)target);
		}

		[SerializeField]
		GameObject m_RightSelector = null, m_LeftSelector = null;
		private void CreateHand(HandInputModule target, HandManager.HandType hand)
		{
			GameObject selector_beam = null, selector_pointer = null;

			if (hand == HandManager.HandType.Right)
			{
				if (m_RightSelector == null)
				{
					Debug.Log("CreateHand() Restores RightHandSelector.");
					m_RightSelector = target.RightHandSelector;
				}
				if (m_RightSelector == null)
				{
					Debug.Log("CreateHand() Finding RightHandSelector.");
					m_RightSelector = GameObject.Find("RightHandSelector");
				}
				if (m_RightSelector == null)
				{
					GameObject RightHand = new GameObject("RightHand");

					Debug.Log("CreateHand() Creates m_RightSelector.");
					m_RightSelector = new GameObject("RightHandSelector");
					m_RightSelector.transform.SetParent(RightHand.transform, false);

					selector_beam = new GameObject("RightBeam");
					selector_beam.transform.SetParent(m_RightSelector.transform, false);
					selector_beam.SetActive(false);
					HandBeam beam = selector_beam.AddComponent<HandBeam>();
					beam.BeamType = hand;
					selector_beam.SetActive(true);

					selector_pointer = new GameObject("RightPointer");
					selector_pointer.transform.SetParent(m_RightSelector.transform, false);
					selector_pointer.SetActive(false);
					HandSpotPointer pointer = selector_pointer.AddComponent<HandSpotPointer>();
					pointer.PointerType = hand;
					selector_pointer.SetActive(true);
				}
				target.RightHandSelector = m_RightSelector;
			}
			else
			{
				if (m_LeftSelector == null)
				{
					Debug.Log("CreateHand() Restores LeftHandSelector.");
					m_LeftSelector = target.LeftHandSelector;
				}
				if (m_LeftSelector == null)
				{
					Debug.Log("CreateHand() Finding LeftHandSelector.");
					m_LeftSelector = GameObject.Find("LeftHandSelector");
				}
				if (m_LeftSelector == null)
				{
					GameObject LeftHand = new GameObject("LeftHand");

					Debug.Log("CreateHand() Creates m_LeftSelector.");
					m_LeftSelector = new GameObject("LeftHandSelector");
					m_LeftSelector.transform.SetParent(LeftHand.transform, false);

					selector_beam = new GameObject("LeftBeam");
					selector_beam.transform.SetParent(m_LeftSelector.transform, false);
					selector_beam.SetActive(false);
					HandBeam beam = selector_beam.AddComponent<HandBeam>();
					beam.BeamType = hand;
					selector_beam.SetActive(true);

					selector_pointer = new GameObject("LeftPointer");
					selector_pointer.transform.SetParent(m_LeftSelector.transform, false);
					selector_pointer.SetActive(false);
					HandSpotPointer pointer = selector_pointer.AddComponent<HandSpotPointer>();
					pointer.PointerType = hand;
					selector_pointer.SetActive(true);
				}
				target.LeftHandSelector = m_LeftSelector;
			}
		}
	}
}
#endif
