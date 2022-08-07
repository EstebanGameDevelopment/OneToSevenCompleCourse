using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
namespace Wave.Essence.InputModule.Editor
{
	[CustomEditor(typeof(GazeInputModule))]
	public class GazeInputModuleEditor : UnityEditor.Editor
	{
		SerializedProperty /*m_EyeTracking, */m_Movable, m_InputEvent, m_TimeToGaze, m_ButtonControlDevices, m_ButtonControlKeys, m_IgnoreMode;
		private void OnEnable()
		{
			//m_EyeTracking = serializedObject.FindProperty("m_EyeTracking");
			m_Movable = serializedObject.FindProperty("m_Movable");
			m_InputEvent = serializedObject.FindProperty("m_InputEvent");
			m_TimeToGaze = serializedObject.FindProperty("m_TimeToGaze");
			m_ButtonControlDevices = serializedObject.FindProperty("m_ButtonControlDevices");
			m_ButtonControlKeys = serializedObject.FindProperty("m_ButtonControlKeys");
			m_IgnoreMode = serializedObject.FindProperty("m_IgnoreMode");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			GazeInputModule myScript = target as GazeInputModule;

			/*EditorGUILayout.HelpBox(
				"To use the eye tracking data for gaze.",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_EyeTracking);*/

			EditorGUILayout.HelpBox(
				"Whether the gaze pointer is movable when gazing on an object.",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_Movable);

			EditorGUILayout.HelpBox(
				"Selects the event which will be sent when gazing on an object.",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_InputEvent);

			myScript.TimerControl = EditorGUILayout.Toggle("Timer Control", myScript.TimerControl);
			if (myScript.TimerControl)
			{
				EditorGUILayout.HelpBox(
					"Sets the timer countdown seconds.",
					MessageType.Info);
				EditorGUILayout.PropertyField(m_TimeToGaze);
			}

			myScript.ButtonControl = EditorGUILayout.Toggle("Button Control", myScript.ButtonControl);
			if (myScript.ButtonControl)
			{
				EditorGUILayout.HelpBox(
					"Selects the device and button for control.",
					MessageType.Info);
				EditorGUILayout.PropertyField(m_ButtonControlDevices);
				EditorGUILayout.PropertyField(m_ButtonControlKeys);
			}

			EditorGUILayout.PropertyField(m_IgnoreMode);

			serializedObject.ApplyModifiedProperties();

			if (GUI.changed)
				EditorUtility.SetDirty((GazeInputModule)target);
		}
	}
}
#endif
