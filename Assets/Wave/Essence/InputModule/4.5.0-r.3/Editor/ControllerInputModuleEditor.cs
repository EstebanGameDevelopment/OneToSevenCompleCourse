using UnityEngine;
using UnityEngine.SpatialTracking;

#if UNITY_EDITOR
using UnityEditor;
namespace Wave.Essence.InputModule.Editor
{
	[CustomEditor(typeof(ControllerInputModule))]
	public class ControllerInputModuleEditor : UnityEditor.Editor
	{
		SerializedProperty m_DominantEvent, m_DominantRaycastMask, m_NonDominantEvent, m_NonDominantRaycastMask, m_ButtonToTrigger, m_FixedBeamLength, m_IgnoreMode;
		SerializedProperty DominantController, NonDominantController;
		private void OnEnable()
		{
			m_DominantEvent = serializedObject.FindProperty("m_DominantEvent");
			m_DominantRaycastMask = serializedObject.FindProperty("m_DominantRaycastMask");
			m_NonDominantEvent = serializedObject.FindProperty("m_NonDominantEvent");
			m_NonDominantRaycastMask = serializedObject.FindProperty("m_NonDominantRaycastMask");
			m_ButtonToTrigger = serializedObject.FindProperty("m_ButtonToTrigger");
			m_FixedBeamLength = serializedObject.FindProperty("m_FixedBeamLength");
			m_IgnoreMode = serializedObject.FindProperty("m_IgnoreMode");

			DominantController = serializedObject.FindProperty("DominantController");
			NonDominantController = serializedObject.FindProperty("NonDominantController");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			ControllerInputModule myScript = target as ControllerInputModule;

			EditorGUILayout.HelpBox(
				"Creates the dominant event controller.",
				MessageType.Info);
			if (GUILayout.Button("Creates Dominant Controller."))
				CreateController(target as ControllerInputModule, XR_Hand.Dominant);
			EditorGUILayout.PropertyField(DominantController);

			GUILayout.Space(5);
			EditorGUILayout.HelpBox(
				"Creates the non-dominant event controller.",
				MessageType.Info);
			if (GUILayout.Button("Creates NonDominant Controller."))
				CreateController(target as ControllerInputModule, XR_Hand.NonDominant);
			EditorGUILayout.PropertyField(NonDominantController);

			EditorGUILayout.HelpBox(
				"There are three beam modes: Mouse(default), fixed and flexible.",
				MessageType.Info);
			myScript.BeamMode = (ControllerInputModule.BeamModes)EditorGUILayout.EnumPopup("Beam Mode", myScript.BeamMode);
			if (myScript.BeamMode == ControllerInputModule.BeamModes.Fixed)
				EditorGUILayout.PropertyField(m_FixedBeamLength);

			EditorGUILayout.PropertyField(m_DominantEvent);
			EditorGUILayout.PropertyField(m_DominantRaycastMask);
			EditorGUILayout.PropertyField(m_NonDominantEvent);
			EditorGUILayout.PropertyField(m_NonDominantRaycastMask);
			EditorGUILayout.HelpBox(
				"You have to choose the button for triggering events.",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_ButtonToTrigger);
			EditorGUILayout.PropertyField(m_IgnoreMode);

			serializedObject.ApplyModifiedProperties();

			if (GUI.changed)
				EditorUtility.SetDirty((ControllerInputModule)target);
		}

		[SerializeField]
		GameObject m_DominantController = null, m_NonDominantController = null;
		private void CreateController(ControllerInputModule target, XR_Hand hand)
		{
			if (hand == XR_Hand.Dominant)
			{
				if (m_DominantController == null)
				{
					Debug.Log("CreateController() Restores m_DominantController.");
					m_DominantController = target.DominantController;
				}
				if (m_DominantController == null)
				{
					Debug.Log("CreateController() Finds m_DominantController.");
					m_DominantController = GameObject.Find("DominantController");
				}
				if (m_DominantController == null)
				{
					Debug.Log("CreateController() Creates m_DominantController.");
					m_DominantController = new GameObject("DominantController");

					m_DominantController.SetActive(false);
					TrackedPoseDriver pose = m_DominantController.AddComponent<TrackedPoseDriver>();
					pose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);
					EventControllerSetter setter = m_DominantController.AddComponent<EventControllerSetter>();
					setter.ControllerType = hand;
					m_DominantController.SetActive(true);
				}
				target.DominantController = m_DominantController;
			}
			else
			{
				if (m_NonDominantController == null)
				{
					Debug.Log("CreateController() Restores m_NonDominantController.");
					m_NonDominantController = target.NonDominantController;
				}
				if (m_NonDominantController == null)
				{
					Debug.Log("CreateController() Finds m_NonDominantController.");
					m_NonDominantController = GameObject.Find("NonDominantController");
				}
				if (m_NonDominantController == null)
				{
					Debug.Log("CreateController() Creates m_NonDominantController.");
					m_NonDominantController = new GameObject("NonDominantController");

					m_NonDominantController.SetActive(false);
					TrackedPoseDriver pose = m_NonDominantController.AddComponent<TrackedPoseDriver>();
					pose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.LeftPose);
					EventControllerSetter setter = m_NonDominantController.AddComponent<EventControllerSetter>();
					setter.ControllerType = hand;
					m_NonDominantController.SetActive(true);
				}
				target.NonDominantController = m_NonDominantController;
			}
		}
	}
}
#endif
