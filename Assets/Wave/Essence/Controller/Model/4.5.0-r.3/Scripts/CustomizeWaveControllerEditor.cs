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
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

namespace Wave.Essence.Controller.Model.Editor
{
	[CustomEditor(typeof(CustomizeWaveController))]
	public class CustomizeWaveControllerEditor : UnityEditor.Editor
	{
		//For Controller Tips
		private bool tmpEnableControllerTips = false;
		private bool _ctList = false;
		private bool _element = false;

		public override void OnInspectorGUI()
		{
			CustomizeWaveController myScript = target as CustomizeWaveController;

			EditorGUILayout.LabelField("Render Model", EditorStyles.objectField);

			myScript.mergeToOne = EditorGUILayout.Toggle("Merge to one bone", myScript.mergeToOne);
			myScript.updateDynamical = EditorGUILayout.Toggle("Update if render model changed", myScript.updateDynamical);
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Button Effect", EditorStyles.objectField);

			myScript.enableButtonEffect = EditorGUILayout.Toggle("Enable button effect", myScript.enableButtonEffect);
			if (true == myScript.enableButtonEffect)
			{
				myScript.useSystemDefinedColor = EditorGUILayout.Toggle("Apply system config", myScript.useSystemDefinedColor);
				if (true != myScript.useSystemDefinedColor)
				{
					myScript.buttonEffectColor = EditorGUILayout.ColorField("  Button effect color", myScript.buttonEffectColor);
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Controller Tips", EditorStyles.objectField);
			myScript.enableControllerTips = EditorGUILayout.Toggle("Enable Controller Tips", myScript.enableControllerTips);

			if (tmpEnableControllerTips != myScript.enableControllerTips)
			{
				if (myScript.enableControllerTips)
				{
					if (TMP_Settings.instance == null)
					{
						Debug.LogWarning("TMP Essentials can't be found. Please import TMP Essentials before using Controller Tips.");
					}
				}
				tmpEnableControllerTips = myScript.enableControllerTips;
			}

			if (myScript.enableControllerTips)
			{
				myScript.autoLayout = EditorGUILayout.Toggle("Auto Layout", myScript.autoLayout);
				if (myScript.autoLayout)
				{
					myScript.polyline = EditorGUILayout.Toggle("Polyline", myScript.polyline);
				}
				myScript.fadingEffect = EditorGUILayout.Toggle("Fading Effect", myScript.fadingEffect);
				myScript.displayAngle = EditorGUILayout.Slider("Display Angle", myScript.displayAngle, 0.0f, 45.0f);
				myScript.hideWhenRolling = EditorGUILayout.Toggle("Hide When Rolling", myScript.hideWhenRolling);
				myScript.basedOnEmitter = EditorGUILayout.Toggle("Based On Emitter", myScript.basedOnEmitter);
				myScript.displayPlane = (DisplayPlane)EditorGUILayout.EnumPopup("Display Plane", myScript.displayPlane);
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Line customization", EditorStyles.boldLabel);
				myScript.lineWidth = EditorGUILayout.Slider("  Line Width", myScript.lineWidth, 0.0001f, 0.001f);
				myScript.lineColor = EditorGUILayout.ColorField("  Line Color", myScript.lineColor);
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Text customization", EditorStyles.boldLabel);
				myScript.textFontSize = EditorGUILayout.IntSlider("  Text Font Size", myScript.textFontSize, 1, 15);
				myScript.textColor = EditorGUILayout.ColorField("  Text Color", myScript.textColor);
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Controller Tips Information", EditorStyles.boldLabel);
				myScript.useSystemConfig = EditorGUILayout.Toggle("  Use system config", myScript.useSystemConfig);
				if (false == myScript.useSystemConfig)
				{
					EditorGUILayout.Space();
					_ctList = EditorGUILayout.Foldout(_ctList, "  Controller Tips List");
					if (_ctList)
					{
						var list = myScript.tipInfoList;

						int newCount = Mathf.Max(0, EditorGUILayout.IntField("      Size", list.Count));

						while (newCount < list.Count)
							list.RemoveAt(list.Count - 1);
						while (newCount > list.Count)
							list.Add(new TipInfo());

						for (int i = 0; i < list.Count; i++)
						{
							_element = EditorGUILayout.Foldout(_element, "      Element " + i);
							if (_element)
							{
								myScript.tipInfoList[i].inputType = (InputType)EditorGUILayout.EnumPopup("        Input Type", myScript.tipInfoList[i].inputType);
								myScript.tipInfoList[i].alignment = (Alignment)EditorGUILayout.EnumPopup("        Alignment", myScript.tipInfoList[i].alignment);
								myScript.tipInfoList[i].buttonAndTextDistance = EditorGUILayout.Slider("        Button And Text Distance", myScript.tipInfoList[i].buttonAndTextDistance, 0.0f, 0.1f);
								myScript.tipInfoList[i].buttonAndLineDistance = EditorGUILayout.Slider("        Button And Line Distance", myScript.tipInfoList[i].buttonAndLineDistance, 0.0f, 0.1f);
								myScript.tipInfoList[i].lineLengthAdjustment = EditorGUILayout.Slider("        Line Length Adjustment", myScript.tipInfoList[i].lineLengthAdjustment, -0.1f, 0.1f);
								myScript.tipInfoList[i].multiLanguage = EditorGUILayout.Toggle("        Multi-language", myScript.tipInfoList[i].multiLanguage);
								myScript.tipInfoList[i].inputText = EditorGUILayout.TextField("        Input text", myScript.tipInfoList[i].inputText);
								EditorGUILayout.Space();
							}
						}
					}
				}
			}

			if (GUI.changed)
				EditorUtility.SetDirty((CustomizeWaveController)target);
		}
	}
}
#endif
