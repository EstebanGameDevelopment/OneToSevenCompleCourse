// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;

namespace Wave.Essence.Hand.Model
{
#if UNITY_EDITOR
	using UnityEditor;

	[CustomEditor(typeof(HandMeshRenderer))]
	[CanEditMultipleObjects]
	class HandMeshRendererEditor : UnityEditor.Editor
	{
		HandMeshRenderer handMeshRenderer;

		public override void OnInspectorGUI()
		{
			handMeshRenderer = (HandMeshRenderer)target;

			EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

			//EditorGUILayout.HelpBox("Please select what is your preferred tracker type",
			//MessageType.None);
			//handMeshRenderer.tracker = (HandManager.TrackerType)EditorGUILayout.EnumPopup("Tracker type", handMeshRenderer.tracker);
			//EditorGUILayout.HelpBox("Please be noticed, the tracker type will be controlled by interaction mode, if interaction mode manager is in this scene.",
			//MessageType.Warning);

			EditorGUILayout.HelpBox("Please check if your model is used to bind left hand poses", MessageType.None);
			handMeshRenderer.IsLeft = EditorGUILayout.Toggle("Is left", handMeshRenderer.IsLeft);

			//EditorGUILayout.HelpBox("Please check if you want to show electronic hand in supported cases like controller mode",
			//MessageType.None);
			//handMeshRenderer.showElectronicHandInControllerMode = EditorGUILayout.Toggle("Show electronic hand if supportted", handMeshRenderer.showElectronicHandInControllerMode);

			EditorGUILayout.HelpBox("Please check if you want to update model's alpha by hand tracking's confidence, Lower confidence will make model become more transparent",
			MessageType.None);
			handMeshRenderer.showConfidenceAsAlpha = EditorGUILayout.Toggle("Show Confidence As Alpha", handMeshRenderer.showConfidenceAsAlpha);

			EditorGUILayout.HelpBox("Use skeleton, mesh and pose from runtime",
			MessageType.None);
			handMeshRenderer.useRuntimeModel = EditorGUILayout.Toggle("Use Runtime Model", handMeshRenderer.useRuntimeModel);

			if (!handMeshRenderer.useRuntimeModel)
			{
				EditorGUILayout.HelpBox("Use scale from runtime.", MessageType.None);
				handMeshRenderer.useScale = EditorGUILayout.Toggle("Use Scale", handMeshRenderer.useScale);
			}

			EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("The customized hand model in child will be used", MessageType.None);
			GUILayout.Space(5);
			EditorGUILayout.HelpBox("Please change rotation to make sure your model should palm faces forward and fingers points up in global axis.",
					MessageType.Info);

			EditorGUILayout.LabelField("Bones", EditorStyles.boldLabel);
			if (handMeshRenderer.alreadyDetect)
			{
				EditorGUI.BeginDisabledGroup(false);
				for (int i = 0; i < handMeshRenderer.BonePoses.Length; i++)
				{
					handMeshRenderer.customizedBonePoses[i] = EditorGUILayout.ObjectField("  " + handMeshRenderer.boneMap[i].DisplayName, handMeshRenderer.customizedBonePoses[i], typeof(Transform), true) as Transform;
				}
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.HelpBox("Please manually check if auto detect bones and update fields correctly.",
						MessageType.Warning);

				if (GUILayout.Button("Clean Bones"))
					handMeshRenderer.ClearDetect();
			}
			else
			{
				GUILayout.Space(5);

				if (GUILayout.Button("Auto Detect Bones"))
					handMeshRenderer.AutoDetect();

				GUILayout.Space(5);
			}
			if (GUI.changed)
				EditorUtility.SetDirty((HandMeshRenderer)target);
		}
	}
#endif
}
