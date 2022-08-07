// "Wave SDK
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

namespace Wave.Essence.Controller.Model.Editor
{
	[CustomEditor(typeof(ControllerTips))]
	public class ControllerTipsEditor : UnityEditor.Editor
	{
		ControllerTips ctScript;

		private bool tmpShowIndicator = false;
		private bool _buttonList = false;
		private bool _element = false;

		public override void OnInspectorGUI()
		{
			ctScript = (ControllerTips)target;

			ctScript.enableControllerTips = EditorGUILayout.Toggle("Enable Controller Tips", ctScript.enableControllerTips);

			if (tmpShowIndicator != ctScript.enableControllerTips)
			{
				if (ctScript.enableControllerTips)
				{
					if (TMP_Settings.instance == null)
					{
						Debug.LogWarning("TMP Essentials can't be found. Please import TMP Essentials before using Controller Tips.");
					}
				}
				tmpShowIndicator = ctScript.enableControllerTips;
			}

			if (ctScript.enableControllerTips)
			{
				ctScript.autoLayout = EditorGUILayout.Toggle("Auto Layout", ctScript.autoLayout);
				if (ctScript.autoLayout)
				{
					ctScript.polyline = EditorGUILayout.Toggle("Polyline", ctScript.polyline);
				}
				ctScript.fadingEffect = EditorGUILayout.Toggle("Fading Effect", ctScript.fadingEffect);
				ctScript.displayAngle = EditorGUILayout.Slider("Display Angle", ctScript.displayAngle, 0.0f, 45.0f);
				ctScript.hideWhenRolling = EditorGUILayout.Toggle("Hide When Rolling", ctScript.hideWhenRolling);
				ctScript.basedOnEmitter = EditorGUILayout.Toggle("Based On Emitter", ctScript.basedOnEmitter);
				ctScript.displayPlane = (DisplayPlane)EditorGUILayout.EnumPopup("Display Plane", ctScript.displayPlane);
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Line customization", EditorStyles.boldLabel);
				ctScript.lineWidth = EditorGUILayout.Slider("Line Width", ctScript.lineWidth, 0.0001f, 0.001f);
				ctScript.lineColor = EditorGUILayout.ColorField("Line Color", ctScript.lineColor);
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Text customization", EditorStyles.boldLabel);
				ctScript.textFontSize = EditorGUILayout.IntSlider("Text Font Size", ctScript.textFontSize, 1, 15);
				ctScript.textColor = EditorGUILayout.ColorField("Text Color", ctScript.textColor);
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Controller Tips Information", EditorStyles.boldLabel);
				ctScript.useSystemConfig = EditorGUILayout.Toggle("  Use system config", ctScript.useSystemConfig);
				if (false == ctScript.useSystemConfig)
				{
					_buttonList = EditorGUILayout.Foldout(_buttonList, "Controller Tips List");
					if (_buttonList)
					{
						var list = ctScript.tipInfoList;

						int newCount = Mathf.Max(0, EditorGUILayout.IntField("Size", list.Count));

						while (newCount < list.Count)
							list.RemoveAt(list.Count - 1);
						while (newCount > list.Count)
							list.Add(new TipInfo());

						for (int i = 0; i < list.Count; i++)
						{
							_element = EditorGUILayout.Foldout(_element, "  Element " + i);
							if (_element)
							{
								ctScript.tipInfoList[i].inputType = (InputType)EditorGUILayout.EnumPopup("    Input Type", ctScript.tipInfoList[i].inputType);
								ctScript.tipInfoList[i].alignment = (Alignment)EditorGUILayout.EnumPopup("    Alignment", ctScript.tipInfoList[i].alignment);
								ctScript.tipInfoList[i].buttonAndTextDistance = EditorGUILayout.Slider("    Button And Text Distance", ctScript.tipInfoList[i].buttonAndTextDistance, 0.0f, 0.1f);
								ctScript.tipInfoList[i].buttonAndLineDistance = EditorGUILayout.Slider("    Button And Line Distance", ctScript.tipInfoList[i].buttonAndLineDistance, 0.0f, 0.1f);
								ctScript.tipInfoList[i].lineLengthAdjustment = EditorGUILayout.Slider("    Line Length Adjustment", ctScript.tipInfoList[i].lineLengthAdjustment, -0.1f, 0.1f);
								ctScript.tipInfoList[i].multiLanguage = EditorGUILayout.Toggle("    Multi-language", ctScript.tipInfoList[i].multiLanguage);
								ctScript.tipInfoList[i].inputText = EditorGUILayout.TextField("    Input text", ctScript.tipInfoList[i].inputText);
								EditorGUILayout.Space();
							}
						}
					}
				}

				EditorGUILayout.Space();
				ctScript.checkInteractionMode = EditorGUILayout.Toggle("Controller Mode Only", ctScript.checkInteractionMode);
			}

			if (GUI.changed)
				EditorUtility.SetDirty((ControllerTips)target);
		}
	}
}
#endif
