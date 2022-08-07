// "Wave SDK
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

//#define DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;
using Wave.Essence.Extra;
using TMPro;

namespace Wave.Essence.Controller.Model
{

	public enum InputType
	{
		Trigger,
		TouchPad,
		DigitalTrigger,
		App,
		Home,
		Volume,
		VolumeUp,
		VolumeDown,
		Grip,
		DPad_Left,
		DPad_Right,
		DPad_Up,
		DPad_Down,
		Bumper,
		Thumbstick,
		ButtonA,
		ButtonB,
		ButtonX,
		ButtonY
	};

	public enum Alignment
	{
		Balance,
		Right,
		Left
	};

	public enum DisplayPlane
	{
		Normal,
		Body_Up,
		Body_Middle,
		Body_Bottom
	};

	[System.Serializable]
	public class TipInfo
	{
		public InputType inputType;
		public Alignment alignment = Alignment.Balance;
		[Range(0.0f, 0.1f)]
		public float buttonAndTextDistance = 0.035f; // Distance between SourceObject and DestObject
		[Range(0.0f, 0.1f)]
		public float buttonAndLineDistance = 0.0f; // Distance between SourceObject and Line
		[Range(-0.1f, 0.1f)]
		public float lineLengthAdjustment = -0.003f; // Distance between Line and DestObject
		public bool multiLanguage = false;
		public string inputText = "system";
	}

	[System.Serializable]
	public class TipComponent
	{
		public string name;
		public string inputText = "system";
		public string inputType = null;
		public GameObject sourceObject;
		public GameObject lineObject;
		public GameObject polylineObject;
		public GameObject destObject;
		public Alignment alignment = Alignment.Balance;
		public float buttonAndTextDistance; // Distance between SourceObject and DestObject
		public float buttonAndLineDistance; // Distance between SourceObject and Line
		public float lineLengthAdjustment; // Distance between Line and DestObject
		public bool multiLanguage = false;

		public bool leftRightFlag = false;
		public float zValue;
		public float yValue;
	}

	public class ControllerTips : MonoBehaviour
	{
		private static string LOG_TAG = "ControllerTips";

		public bool enableControllerTips = false;
		public bool autoLayout = true;
		public bool polyline = true;
		public bool fadingEffect = true;
		[Range(0, 45.0f)]
		public float displayAngle = 30.0f;
		public bool hideWhenRolling = true;
		public bool basedOnEmitter = true;
		public DisplayPlane displayPlane = DisplayPlane.Normal;

		[Header("Line Customization")]
		[Range(0.0001f, 0.001f)]
		public float lineWidth = 0.0004f;
		public Color lineColor = Color.white;

		[Header("Text Customization")]
		[Range(1, 15)]
		public int textFontSize = 8;
		public Color textColor = Color.white;

		[Header("Controller Tips List")]
		public bool useSystemConfig = true;
		[HideInInspector]
		public List<TipInfo> tipInfoList = new List<TipInfo>();

		private ResourceWrapper rw = null;
		private RenderModel wrm = null;
		private string sysLang = null;
		private string sysCountry = null;
		private int checkCount = 0;
		private GameObject textPrefab = null;
		private GameObject linePrefab = null;
		private GameObject hmd = null;
		private bool needRedraw = true;
		private GameObject emitter = null;
		private GameObject combinedLine;
		private List<TipComponent> tipComp = new List<TipComponent>();
		private List<TipComponent> rightList = new List<TipComponent>();
		private List<TipComponent> leftList = new List<TipComponent>();
		private Transform body = null;
		private int leftCount = 0;
		private int rightCount = 0;

		//for update
		private Vector3 _targetForward, _targetRight, _targetUp;
		private float zAngle, xAngle, yAngle;
		private bool tmpEnableControllerTips = false;
		private bool tmpAutoLayout = false;
		private bool tmpPolyline = false;
		private bool fadingEffectChangedFlag = false;
		private bool tmpFadingEffect = false;
		private bool textAndLineDone = false;
		private Material material;
		private Texture2D texture;
		private Color fadingColor;
		private float tmpFadingAlpha;

		private bool showTips = true;
		private bool PreShowTips = true;

		[HideInInspector]
		public bool checkInteractionMode = false;

		public void CreateControllerTips()
		{
			if (!enableControllerTips) return;
			ClearResourceAndObject();
			rw = ResourceWrapper.instance;
			wrm = this.GetComponentInChildren<RenderModel>();

			Log.i(LOG_TAG, "Create Controller Tips!");
			textPrefab = Resources.Load("ControllerTipsText") as GameObject;
			Log.i(LOG_TAG, "ControllerTipsText is loaded.");

			if (textPrefab == null)
			{
				Log.i(LOG_TAG, "TextInd is not found!");
				return;
			}
			else
			{
				Log.i(LOG_TAG, "TextInd is found!");
			}

			linePrefab = Resources.Load("ControllerTipsLine") as GameObject;

			if (linePrefab == null)
			{
				Log.i(LOG_TAG, "ControllerTipsLine is not found!");
				return;
			}
			else
			{
				Log.d(LOG_TAG, "ControllerTipsLine is found!");
			}

			if (hmd == null)
				hmd = Camera.main.gameObject;

			if (hmd == null)
			{
				Log.i(LOG_TAG, "Can't get HMD!");
				return;
			}

			var gc = transform.childCount;

			for (int i = 0; i < gc; i++)
			{
				GameObject go = transform.GetChild(i).gameObject;

				Log.i(LOG_TAG, "child name is " + go.name);
			}

			Log.i(LOG_TAG, "displayAngle: " + displayAngle + ", hideWhenRolling: " + hideWhenRolling + ", basedOnEmitter: " + basedOnEmitter + ", displayPlane: " + displayPlane);
			Log.i(LOG_TAG, "lineWidth: " + lineWidth + ", lineColor: " + lineColor);
			Log.i(LOG_TAG, "textFontSize: " + textFontSize + ", textColor: " + textColor);

			body = transform.Find("_[CM]_Body");
			if (body == null)
			{
				body = transform.Find("__CM__Body");
				if (body == null)
				{
					body = transform.Find("__CM__Body.__CM__Body");
					if (body == null)
					{
						body = transform.Find("Body");
					}
				}
			}

			if (body == null)
			{
				Log.w(LOG_TAG, "Body of the controller can't be found in the model!");
			}

			foreach (TipInfo bi in tipInfoList)
			{
				Log.i(LOG_TAG, "inputType: " + bi.inputType + ", alignment: " + bi.alignment + ", buttonAndTextDistance: " + bi.buttonAndTextDistance + ", buttonAndLineDistance: " + bi.buttonAndLineDistance + ", lineLengthAdjustment: " + bi.lineLengthAdjustment + ", multiLanguage: " + bi.multiLanguage + ", inputText: " + bi.inputText);

				Transform tmp = FindComponentByName(bi.inputType);
				string inputType = FindInputType(bi.inputType);

				if (tmp != null)
				{
					TipComponent tmpCom = new TipComponent();

					tmpCom.name = tmp.name;
					tmpCom.sourceObject = tmp.gameObject;
					tmpCom.inputType = inputType;
					tmpCom.alignment = bi.alignment;
					tmpCom.buttonAndTextDistance = bi.buttonAndTextDistance;
					tmpCom.buttonAndLineDistance = bi.buttonAndLineDistance;
					tmpCom.lineLengthAdjustment = bi.lineLengthAdjustment;
					tmpCom.multiLanguage = bi.multiLanguage;
					tmpCom.inputText = bi.inputText;

					tipComp.Add(tmpCom);
				}
			}

			Sort();
			FindDisplayPlane();
			DisplayArrangement();
			StartCoroutine(CreateLineAndText());

			emitter = null;
			if (basedOnEmitter)
			{
				if (wrm != null)
				{
					GameObject modelObj = wrm.gameObject;

					int modelchild = modelObj.transform.childCount;
					for (int j = 0; j < modelchild; j++)
					{
						GameObject childName = modelObj.transform.GetChild(j).gameObject;
						if (childName.name == "__CM__Emitter" || childName.name == "_[CM]_Emitter")
						{
							emitter = childName;
						}
					}
				}
			}

			fadingColor = lineColor;
			tmpFadingAlpha = 0;
			texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
			material = new Material(Shader.Find("Unlit/Transparent"));

			needRedraw = false;
		}

		void CombineLineMeshes()
		{
			List<CombineInstance> lineList = new List<CombineInstance>();

			for (int i = 0; i < leftList.Count; i++)
			{
				//line
				if (leftList[i].lineObject != null)
				{
					MeshFilter[] lineMeshFilters = leftList[i].lineObject.GetComponents<MeshFilter>();
					CombineInstance[] lineCombineInstance = new CombineInstance[lineMeshFilters.Length];
					leftList[i].lineObject.SetActive(false);

					for (int j = 0; j < lineMeshFilters.Length; j++)
					{
						lineCombineInstance[j].mesh = lineMeshFilters[j].sharedMesh;
						lineCombineInstance[j].transform = lineMeshFilters[j].transform.localToWorldMatrix;
						lineList.Add(lineCombineInstance[j]);
					}
				}
				if(leftList[i].polylineObject != null)
				{
					MeshFilter[] polylineMeshFilters = leftList[i].polylineObject.GetComponents<MeshFilter>();
					CombineInstance[] polylineCombineInstance = new CombineInstance[polylineMeshFilters.Length];
					leftList[i].polylineObject.SetActive(false);

					for (int j = 0; j < polylineMeshFilters.Length; j++)
					{
						polylineCombineInstance[j].mesh = polylineMeshFilters[j].sharedMesh;
						polylineCombineInstance[j].transform = polylineMeshFilters[j].transform.localToWorldMatrix;
						lineList.Add(polylineCombineInstance[j]);
					}
				}
			}

			for (int i = 0; i < rightList.Count; i++)
			{
				//line
				if (rightList[i].lineObject != null)
				{
					MeshFilter[] lineMeshFilters = rightList[i].lineObject.GetComponents<MeshFilter>();
					CombineInstance[] lineCombineInstance = new CombineInstance[lineMeshFilters.Length];
					rightList[i].lineObject.SetActive(false);

					for (int j = 0; j < lineMeshFilters.Length; j++)
					{
						lineCombineInstance[j].mesh = lineMeshFilters[j].sharedMesh;
						lineCombineInstance[j].transform = lineMeshFilters[j].transform.localToWorldMatrix;
						lineList.Add(lineCombineInstance[j]);
					}
				}
				if (rightList[i].polylineObject != null)
				{
					MeshFilter[] polylineMeshFilters = rightList[i].polylineObject.GetComponents<MeshFilter>();
					CombineInstance[] polylineCombineInstance = new CombineInstance[polylineMeshFilters.Length];
					rightList[i].polylineObject.SetActive(false);

					for (int j = 0; j < polylineMeshFilters.Length; j++)
					{
						polylineCombineInstance[j].mesh = polylineMeshFilters[j].sharedMesh;
						polylineCombineInstance[j].transform = polylineMeshFilters[j].transform.localToWorldMatrix;
						lineList.Add(polylineCombineInstance[j]);
					}
				}
			}

			for(int i = 0; i < tipComp.Count; i++)
			{
				if (tipComp[i].lineObject != null)
					tipComp[i].lineObject.SetActive(false);

				if (tipComp[i].polylineObject != null)
					tipComp[i].polylineObject.SetActive(false);
			}

			//combine line meshes
			combinedLine = new GameObject("ControllerTips_Line");
			combinedLine.transform.parent = this.transform;

			combinedLine.AddComponent<MeshFilter>();
			combinedLine.AddComponent<MeshRenderer>();
			combinedLine.GetComponent<MeshRenderer>().material = material;

			Color o = lineColor;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					texture.SetPixel(i, j, o);
				}
			}
			texture.Apply();

			combinedLine.GetComponent<MeshRenderer>().material.mainTexture = texture;
			combinedLine.GetComponent<MeshFilter>().mesh = new Mesh();
			combinedLine.GetComponent<MeshFilter>().mesh.CombineMeshes(lineList.ToArray());
			if (combinedLine != null)
				combinedLine.SetActive(false);
			Log.i(LOG_TAG, "Line meshes are combined!");
		}

		void UpdateInfo(List<TipComponent> tmpComp)
		{
			if (!enableControllerTips)
			{
				if (combinedLine != null)
					combinedLine.SetActive(false);
				foreach (TipComponent ci in tmpComp)
				{
					if (ci.destObject != null)
					{
						ci.destObject.SetActive(false);
					}
				}

				return;
			}

			if (!showTips)
			{
				if (combinedLine != null)
					combinedLine.SetActive(false);
				foreach (TipComponent ci in tmpComp)
				{
					if (ci.destObject != null)
					{
						ci.destObject.SetActive(false);
					}
				}
				return;
			}

			if ((_targetForward.y < (displayAngle / 90f)) || (zAngle < displayAngle))
			{
				if (combinedLine != null)
					combinedLine.SetActive(false);
				foreach (TipComponent ci in tmpComp)
				{
					if (ci.destObject != null)
					{
						ci.destObject.SetActive(false);
					}
				}

				return;
			}

			if (hideWhenRolling)
			{

				if (xAngle > 90.0f)
				{
					if (combinedLine != null)
						combinedLine.SetActive(false);
					foreach (TipComponent ci in tmpComp)
					{
						if (ci.destObject != null)
						{
							ci.destObject.SetActive(false);
						}
					}
					return;
				}
			}

			//fading effect
			if (tmpFadingEffect != fadingEffect)
			{
				Log.i(LOG_TAG, "fadingEffect: " + fadingEffect);
				tmpFadingEffect = fadingEffect;
				fadingEffectChangedFlag = true;
			}

			if (tmpFadingEffect)
				UpdateAlphaValue();

			if (fadingEffectChangedFlag && !tmpFadingEffect)
			{
				Log.i(LOG_TAG, "fadingEffect: " + fadingEffect);
				UpdateAlphaValue();
				fadingEffectChangedFlag = false;
			}

			if (combinedLine != null)
				combinedLine.SetActive(true);

			foreach (TipComponent ci in tmpComp)
			{
				if (ci.destObject != null)
				{
					ci.destObject.SetActive(true);
				}
			}
		}

		// reset for redraw
		public void ResetControllerTips()
		{
			Log.i(LOG_TAG, "Reset Controller Tips!");
			StopAllCoroutines();
			ClearResourceAndObject();
			rw = ResourceWrapper.instance;
			sysLang = rw.getSystemLanguage();
			sysCountry = rw.getSystemCountry();
			needRedraw = true;
		}

		void ClearResourceAndObject()
		{
			Log.i(LOG_TAG, "Clear Controller Tips!");

			textAndLineDone = false;

			if (material != null)
				Destroy(material);
			if (texture != null)
				Destroy(texture);
			if (combinedLine != null)
				Destroy(combinedLine);

			foreach (TipComponent tc in tipComp)
			{
				if (tc.destObject != null)
				{
					Destroy(tc.destObject);
				}
				if (tc.lineObject != null)
				{
					Destroy(tc.lineObject);
				}
				if (tc.polylineObject != null)
				{
					Destroy(tc.polylineObject);
				}
			}
			tipComp.Clear();

			foreach (TipComponent leftComp in leftList)
			{
				if (leftComp.destObject != null)
				{
					Destroy(leftComp.destObject);
				}
				if (leftComp.lineObject != null)
				{
					Destroy(leftComp.lineObject);
				}
				if (leftComp.polylineObject != null)
				{
					Destroy(leftComp.polylineObject);
				}
			}
			leftList.Clear();

			foreach (TipComponent rightComp in rightList)
			{
				if (rightComp.destObject != null)
				{
					Destroy(rightComp.destObject);
				}
				if (rightComp.lineObject != null)
				{
					Destroy(rightComp.lineObject);
				}
				if (rightComp.polylineObject != null)
				{
					Destroy(rightComp.polylineObject);
				}
			}
			rightList.Clear();

			Resources.UnloadUnusedAssets();
		}

		void OnAdaptiveControllerModelReady(XR_Hand hand)
		{
			needRedraw = true;
		}

		IEnumerator CreateControllerTipsCoroutine()
		{
			yield return null;
			CreateControllerTips();
		}

		Transform FindComponentByName(InputType input)
		{
			// find component by name
			string partName = null;
			string partName1 = null;
			string partName2 = null;

			switch (input)
			{
				case InputType.Trigger:
					partName = "_[CM]_TriggerKey";
					partName1 = "__CM__TriggerKey";
					partName2 = "__CM__TriggerKey.__CM__TriggerKey";
					break;
				case InputType.TouchPad:
					partName = "_[CM]_TouchPad";
					partName1 = "__CM__TouchPad";
					partName2 = "__CM__TouchPad.__CM__TouchPad";
					break;
				case InputType.Grip:
					partName = "_[CM]_Grip";
					partName1 = "__CM__Grip";
					partName2 = "__CM__Grip.__CM__Grip";
					break;
				case InputType.DPad_Left:
					partName = "_[CM]_DPad_Left";
					partName1 = "__CM__DPad_Left";
					partName2 = "__CM__DPad_Left.__CM__DPad_Left";
					break;
				case InputType.DPad_Right:
					partName = "_[CM]_DPad_Right";
					partName1 = "__CM__DPad_Right";
					partName2 = "__CM__DPad_Right.__CM__DPad_Right";
					break;
				case InputType.DPad_Up:
					partName = "_[CM]_DPad_Up";
					partName1 = "__CM__DPad_Up";
					partName2 = "__CM__DPad_Up.__CM__DPad_Up";
					break;
				case InputType.DPad_Down:
					partName = "_[CM]_DPad_Down";
					partName1 = "__CM__DPad_Down";
					partName2 = "__CM__DPad_Down.__CM__DPad_Down";
					break;
				case InputType.App:
					partName = "_[CM]_AppButton";
					partName1 = "__CM__AppButton";
					partName2 = "__CM__AppButton.__CM__AppButton";
					break;
				case InputType.Home:
					partName = "_[CM]_HomeButton";
					partName1 = "__CM__HomeButton";
					partName2 = "__CM__HomeButton.__CM__HomeButton";
					break;
				case InputType.Volume:
					partName = "_[CM]_VolumeKey";
					partName1 = "__CM__VolumeKey";
					partName2 = "__CM__VolumeKey.__CM__VolumeKey";
					break;
				case InputType.VolumeUp:
					partName = "_[CM]_VolumeUp";
					partName1 = "__CM__VolumeUp";
					partName2 = "__CM__VolumeUp.__CM__VolumeUp";
					break;
				case InputType.VolumeDown:
					partName = "_[CM]_VolumeDown";
					partName1 = "__CM__VolumeDown";
					partName2 = "__CM__VolumeDown.__CM__VolumeDown";
					break;
				case InputType.DigitalTrigger:
					partName = "_[CM]_DigitalTriggerKey";
					partName1 = "__CM__DigitalTriggerKey";
					partName2 = "__CM__DigitalTriggerKey.__CM__DigitalTriggerKey";
					break;
				case InputType.Bumper:
					partName = "_[CM]_BumperKey";
					partName1 = "__CM__BumperKey";
					partName2 = "__CM__BumperKey.__CM__BumperKey";
					break;
				case InputType.Thumbstick:
					partName = "_[CM]_Thumbstick";
					partName1 = "__CM__Thumbstick";
					partName2 = "__CM__Thumbstick.__CM__Thumbstick";
					break;
				case InputType.ButtonA:
					partName = "_[CM]_ButtonA";
					partName1 = "__CM__ButtonA";
					partName2 = "__CM__ButtonA.__CM__ButtonA";
					break;
				case InputType.ButtonB:
					partName = "_[CM]_ButtonB";
					partName1 = "__CM__ButtonB";
					partName2 = "__CM__ButtonB.__CM__ButtonB";
					break;
				case InputType.ButtonX:
					partName = "_[CM]_ButtonX";
					partName1 = "__CM__ButtonX";
					partName2 = "__CM__ButtonX.__CM__ButtonX";
					break;
				case InputType.ButtonY:
					partName = "_[CM]_ButtonY";
					partName1 = "__CM__ButtonY";
					partName2 = "__CM__ButtonY.__CM__ButtonY";
					break;
				default:
					partName = "_[CM]_unknown";
					partName1 = "__CM__unknown";
					partName2 = "__CM__unknown.__CM__unknown";
					Log.d(LOG_TAG, "Unknown input type!");
					break;
			}

			Transform tmp = transform.Find(partName);
			if (tmp == null)
			{
				tmp = transform.Find(partName1);
				if (tmp == null)
				{
					tmp = transform.Find(partName2);
				}
			}

			if (tmp == null)
			{
				Log.i(LOG_TAG, partName + ", " + partName1 + ", " + partName2 + " can't be found in the model!");
			}

			return tmp;
		}

		string FindInputType(InputType input)
		{
			switch (input)
			{
				case InputType.Trigger:
					return "TriggerKey";
				case InputType.TouchPad:
					return "TouchPad";
				case InputType.Grip:
					return "Grip";
				case InputType.DPad_Left:
					return "DPad_Left";
				case InputType.DPad_Right:
					return "DPad_Right";
				case InputType.DPad_Up:
					return "DPad_Up";
				case InputType.DPad_Down:
					return "DPad_Down";
				case InputType.App:
					return "AppKey";
				case InputType.Home:
					return "HomeKey";
				case InputType.Volume:
					return "VolumeKey";
				case InputType.VolumeUp:
					return "VolumeUp";
				case InputType.VolumeDown:
					return "VolumeDown";
				case InputType.DigitalTrigger:
					return "DigitalTriggerKey";
				case InputType.Bumper:
					return "BumperKey";
				case InputType.Thumbstick:
					return "Thumbstick";
				case InputType.ButtonA:
					return "ButtonA";
				case InputType.ButtonB:
					return "ButtonB";
				case InputType.ButtonX:
					return "ButtonX";
				case InputType.ButtonY:
					return "ButtonY";
				default:
					Log.d(LOG_TAG, "Unknown input type!");
					return "unknown";
			}
		}

		void FindDisplayPlane()
		{
			if (tipComp.Count == 0)
				return;
			float displayPlaneY = 0;

			if(displayPlane != DisplayPlane.Normal)
			{
				displayPlaneY = tipComp[0].sourceObject.transform.localPosition.y;
				if (displayPlane == DisplayPlane.Body_Up)
				{
					for (int i = 0; i < tipComp.Count; i++)
					{
						if (displayPlaneY < tipComp[i].sourceObject.transform.localPosition.y)
						{
							displayPlaneY = tipComp[i].sourceObject.transform.localPosition.y;
						}
					}
				}
				if (displayPlane == DisplayPlane.Body_Middle)
				{
					displayPlaneY = body.transform.localPosition.y;
				}
				if (displayPlane == DisplayPlane.Body_Bottom)
				{
					for (int i = 0; i < tipComp.Count; i++)
					{
						if (displayPlaneY > tipComp[i].sourceObject.transform.localPosition.y)
						{
							displayPlaneY = tipComp[i].sourceObject.transform.localPosition.y;
						}
					}
				}

				for (int i = 0; i < tipComp.Count; i++)
				{
					tipComp[i].yValue = displayPlaneY;
				}
			}
			else
			{
				for (int i = 0; i < tipComp.Count; i++)
				{
					tipComp[i].yValue = tipComp[i].sourceObject.transform.localPosition.y;
				}
			}
		}

		void Sort()
		{
			if (tipComp.Count == 0)
				return;

			leftCount = 0;
			rightCount = 0;

			leftList.Clear();
			rightList.Clear();

			if (body != null)
			{
				//Determine the priority : insertion sort by localPosition.z
				int i, j;
				float tmpZ;
				TipComponent tmpComp;

				for (i = 0; i < tipComp.Count; i++)
				{
					tmpZ = tipComp[i].sourceObject.transform.localPosition.z;
					tmpComp = tipComp[i];
					for (j = i; j > 0 && tmpZ > tipComp[j - 1].sourceObject.transform.localPosition.z; j--)
					{
						tipComp[j] = tipComp[j - 1];
					}
					tipComp[j] = tmpComp;
				}
				//Left or right : distance of localPosition.x between the body and the button
				for (i = 0; i < tipComp.Count; i++)
				{
					if (!tipComp[i].leftRightFlag)
					{
						if (System.Math.Round(tipComp[i].sourceObject.transform.localPosition.x - body.transform.localPosition.x, 3) > 0)
						{
							tipComp[i].alignment = Alignment.Right;
							tipComp[i].leftRightFlag = true;
							rightCount++;
#if DEBUG
							Log.i(LOG_TAG, "Round 1 : " + tipComp[i].name + "  " + tipComp[i].alignment);
#endif
						}
						if (System.Math.Round(tipComp[i].sourceObject.transform.localPosition.x - body.transform.localPosition.x, 3) < 0)
						{
							tipComp[i].alignment = Alignment.Left;
							tipComp[i].leftRightFlag = true;
							leftCount++;
#if DEBUG
							Log.i(LOG_TAG, "Round 1 : " + tipComp[i].name + "  " + tipComp[i].alignment);
#endif
						}
					}
				}
				//Left or right : user-defined
				for (i = 0; i < tipComp.Count; i++)
				{
					if (!tipComp[i].leftRightFlag)
					{
						if (tipComp[i].alignment == Alignment.Right)
						{
							tipComp[i].leftRightFlag = true;
							rightCount++;
#if DEBUG
							Log.i(LOG_TAG, "Round 2 : " + tipComp[i].name + "  " + tipComp[i].alignment);
#endif
						}
						if (tipComp[i].alignment == Alignment.Left)
						{
							tipComp[i].leftRightFlag = true;
							leftCount++;
#if DEBUG
							Log.i(LOG_TAG, "Round 2 : " + tipComp[i].name + "  " + tipComp[i].alignment);
#endif
						}
					}
				}
				//Left or right : auto-balance
				for (i = 0; i < tipComp.Count; i++)
				{
					if (!tipComp[i].leftRightFlag && tipComp[i].alignment == Alignment.Balance)
					{
						if (rightCount <= leftCount)
						{
							tipComp[i].alignment = Alignment.Right;
							tipComp[i].leftRightFlag = true;
							rightCount++;
#if DEBUG
							Log.i(LOG_TAG, "Round 3 : " + tipComp[i].name + "  " + tipComp[i].alignment);
#endif
						}
						else
						{
							tipComp[i].alignment = Alignment.Left;
							tipComp[i].leftRightFlag = true;
							leftCount++;
#if DEBUG
							Log.i(LOG_TAG, "Round 3 : " + tipComp[i].name + "  " + tipComp[i].alignment);
#endif
						}
					}

				}
				for (i = 0; i < tipComp.Count; i++)
				{
					if (tipComp[i].alignment == Alignment.Left)
						leftList.Add(tipComp[i]);
					if (tipComp[i].alignment == Alignment.Right)
						rightList.Add(tipComp[i]);
				}
			}
		}

		//Automatically arrange the z value of each component
		void DisplayArrangement()
		{
			if (!autoLayout)
			{
				if (rightList.Count >= 1)
				{
					for (int i = 0; i < rightList.Count; i++)
					{
						rightList[i].zValue = rightList[i].sourceObject.transform.localPosition.z;
					}
				}

				if (leftList.Count >= 1)
				{
					for (int i = 0; i < leftList.Count; i++)
					{
						leftList[i].zValue = leftList[i].sourceObject.transform.localPosition.z;
					}
				}
			}
			else
			{
				float ratio = 0.3f;

				if (rightList.Count >= 1)
				{
					if (rightList.Count == 1)
					{
						rightList[0].zValue = rightList[0].sourceObject.transform.localPosition.z;
					}
					else
					{
						float rightRange = rightList[0].sourceObject.transform.localPosition.z - rightList[rightList.Count - 1].sourceObject.transform.localPosition.z;
						float rightZoomRange = rightRange * (1 + ratio);
						float offset = (rightZoomRange - rightRange) / 2;
						float rightOffset = rightZoomRange / (rightList.Count - 1);

						for (int i = 0; i < rightList.Count; i++)
						{
							rightList[i].zValue = (rightList[0].sourceObject.transform.localPosition.z + offset) - rightOffset * i;
						}
					}
				}

				if (leftList.Count >= 1)
				{

					if (leftList.Count == 1)
					{
						leftList[0].zValue = leftList[0].sourceObject.transform.localPosition.z;
					}
					else
					{
						float leftRange = leftList[0].sourceObject.transform.localPosition.z - leftList[leftList.Count - 1].sourceObject.transform.localPosition.z;
						float leftZoomRange = leftRange * (1 + ratio);
						float offset = (leftZoomRange - leftRange) / 2;
						float leftOffset = leftZoomRange / (leftList.Count - 1);


						for (int i = 0; i < leftList.Count; i++)
						{
							leftList[i].zValue = (leftList[0].sourceObject.transform.localPosition.z + offset) - leftOffset * i;
						}
					}
				}
			}
		}

		IEnumerator CreateLineAndText()
		{
			textAndLineDone = false;
			StartCoroutine(TextAndLineCoroutine(tipComp));
			if (needRedraw)
			{
				yield break;
			}
			else
			{
				yield return new WaitUntil(() => textAndLineDone);
				CombineLineMeshes();
			}
		}

		IEnumerator TextAndLineCoroutine(List<TipComponent> tmpComp)
		{
			for(int compCount = 0; compCount < tmpComp.Count; compCount++)
			{
				if (needRedraw)
				{
					yield break;
				}

				TipComponent comp = tmpComp[compCount];
				Quaternion spawnRot = Quaternion.identity;
				spawnRot = transform.rotation;

				// destObject :  instantiate controller tip and fill in the text
				comp.destObject = null;
				Vector3 destPos = Vector3.zero;
				if (comp.alignment == Alignment.Right)
				{
					destPos = transform.TransformPoint(new Vector3(comp.buttonAndTextDistance, comp.yValue, comp.zValue));
				}
				else
				{
					destPos = transform.TransformPoint(new Vector3(-comp.buttonAndTextDistance, comp.yValue, comp.zValue));
				}

				comp.destObject = Instantiate(textPrefab, destPos, spawnRot);
				comp.destObject.name = comp.name + "_Tip";
				comp.destObject.transform.parent = this.transform;

				int childC = comp.destObject.transform.childCount;
				for (int i = 0; i < childC; i++)
				{
					GameObject c = comp.destObject.transform.GetChild(i).gameObject;
					if (comp.alignment == Alignment.Left)
					{
						float tx = c.transform.localPosition.x;
						c.transform.localPosition = new Vector3(tx * (-1), c.transform.localPosition.y, c.transform.localPosition.z);
					}

					TextMeshPro tmPro = c.GetComponent<TextMeshPro>();
					if (tmPro == null)
						Log.d(LOG_TAG, " TextMeshPro is null.");
					if (tmPro != null)
					{
						tmPro.color = textColor;
						tmPro.fontSize = textFontSize;
						if (comp.multiLanguage)
						{
							sysLang = rw.getSystemLanguage();
							sysCountry = rw.getSystemCountry();
							Log.d(LOG_TAG, " System language is " + sysLang);


							// use default string - multi-language
							if (comp.inputText == "system")
							{
								tmPro.text = rw.getString(comp.inputType);
								Log.d(LOG_TAG, " Name: " + comp.destObject.name + " uses default multi-language -> " + tmPro.text);
							}
							else
							{
								tmPro.text = rw.getString(comp.inputText);
								Log.d(LOG_TAG, " Name: " + comp.destObject.name + " uses customed multi-language -> " + tmPro.text);
							}
						}
						else
						{
							if (comp.inputText == "system")
								tmPro.text = comp.inputType;
							else
								tmPro.text = comp.inputText;

							Log.d(LOG_TAG, " Name: " + comp.destObject.name + " didn't uses multi-language -> " + tmPro.text);
						}

						if (comp.alignment == Alignment.Left)
						{
							tmPro.alignment = TextAlignmentOptions.MidlineRight;
						}
						else
						{
							tmPro.alignment = TextAlignmentOptions.MidlineLeft;
						}
					}
				}

				comp.destObject.SetActive(false);

				// lineObject : instantiate line
				comp.lineObject = null;
				comp.polylineObject = null;
				Vector3 linePos = Vector3.zero;

				// Create line
				if (polyline && autoLayout)
				{
					// polyline 1
					float tmpDistance = comp.buttonAndTextDistance / 3;
					Vector3 polyPos = Vector3.zero;

					if (comp.alignment == Alignment.Right)
					{
						polyPos = transform.TransformPoint(new Vector3(comp.buttonAndTextDistance, comp.yValue, comp.zValue) - new Vector3(tmpDistance, 0.0f, 0.0f));
					}
					else if (comp.alignment == Alignment.Left)
					{
						polyPos = transform.TransformPoint(new Vector3(-comp.buttonAndTextDistance, comp.yValue, comp.zValue) + new Vector3(tmpDistance, 0.0f, 0.0f));
					}

					comp.lineObject = Instantiate(linePrefab, polyPos, spawnRot);
					comp.lineObject.name = comp.name + "Line";
					comp.lineObject.transform.parent = this.transform;

					var pl1 = comp.lineObject.GetComponent<ControllerTipsLine>();
					pl1.lineColor = lineColor;
					pl1.lineLength = tmpDistance - 0.003f;
					pl1.startWidth = lineWidth;
					pl1.endWidth = lineWidth;
					pl1.alignment = comp.alignment;
					pl1.updateMeshSettings();

					// polyline 2
					if (comp.alignment == Alignment.Right)
					{
						linePos = transform.TransformPoint(new Vector3(comp.sourceObject.transform.localPosition.x + comp.buttonAndLineDistance, comp.sourceObject.transform.localPosition.y, comp.sourceObject.transform.localPosition.z));
					}
					else if (comp.alignment == Alignment.Left)
					{
						linePos = transform.TransformPoint(new Vector3(comp.sourceObject.transform.localPosition.x - comp.buttonAndLineDistance, comp.sourceObject.transform.localPosition.y, comp.sourceObject.transform.localPosition.z));
					}

					comp.polylineObject = Instantiate(linePrefab, linePos, spawnRot);
					comp.polylineObject.name = comp.name + "Line";
					comp.polylineObject.transform.parent = this.transform;

					float tmpLength = Vector3.Distance(comp.sourceObject.transform.position, polyPos);
					if (tmpLength < 0)
						tmpLength = 0;

					var pl2 = comp.polylineObject.GetComponent<ControllerTipsLine>();
					pl2.lineColor = lineColor;
					pl2.lineLength = tmpLength;
					pl2.startWidth = lineWidth;
					pl2.endWidth = lineWidth;
					pl2.alignment = comp.alignment;
					pl2.updateMeshSettings();

					Vector3 dir = polyPos - comp.sourceObject.transform.position;

					Quaternion tmp = Quaternion.LookRotation(dir, transform.up);
					if (comp.alignment == Alignment.Right)
						comp.polylineObject.transform.rotation = tmp * Quaternion.AngleAxis(-90, new Vector3(0, 1, 0));
					else
						comp.polylineObject.transform.rotation = tmp * Quaternion.AngleAxis(90, new Vector3(0, 1, 0));

					comp.lineObject.SetActive(false);
					comp.polylineObject.SetActive(false);
				}
				else
				{
					if (comp.alignment == Alignment.Right)
					{
						linePos = transform.TransformPoint(new Vector3(comp.sourceObject.transform.localPosition.x + comp.buttonAndLineDistance, comp.sourceObject.transform.localPosition.y, comp.sourceObject.transform.localPosition.z));
					}
					else if (comp.alignment == Alignment.Left)
					{
						linePos = transform.TransformPoint(new Vector3(comp.sourceObject.transform.localPosition.x - comp.buttonAndLineDistance, comp.sourceObject.transform.localPosition.y, comp.sourceObject.transform.localPosition.z));
					}

					comp.lineObject = Instantiate(linePrefab, linePos, spawnRot);
					comp.lineObject.name = comp.name + "_Line";
					comp.lineObject.transform.parent = this.transform;

					float tmpLength = Vector3.Distance(comp.sourceObject.transform.position, comp.destObject.transform.position) + comp.lineLengthAdjustment;
					if (tmpLength < 0)
						tmpLength = 0;

					if (tmpLength > comp.buttonAndTextDistance)
						tmpLength = Vector3.Distance(linePos, comp.destObject.transform.position);

					var li = comp.lineObject.GetComponent<ControllerTipsLine>();
					li.lineColor = lineColor;
					li.lineLength = tmpLength;
					li.startWidth = lineWidth;
					li.endWidth = lineWidth;
					li.alignment = comp.alignment;
					li.updateMeshSettings();

					Vector3 dir = comp.destObject.transform.position - comp.sourceObject.transform.position;

					Quaternion tmp = Quaternion.LookRotation(dir, transform.up);
					if (comp.alignment == Alignment.Right)
						comp.lineObject.transform.rotation = tmp * Quaternion.AngleAxis(-90, new Vector3(0, 1, 0));
					else
						comp.lineObject.transform.rotation = tmp * Quaternion.AngleAxis(90, new Vector3(0, 1, 0));

					comp.lineObject.SetActive(false);
				}
				if(wrm.mergeToOneBone)
					comp.sourceObject.SetActive(false);
				yield return null;
			}

			textAndLineDone = true;
		}

		void UpdateAlphaValue()
		{
			float fadingAngle = Vector3.Angle(_targetUp, new Vector3(0.0f, 0.1f, 0.0f));
			float min = 50;
			float max = 130;
			float tmpAlpha = 0;

			if (!fadingEffect)
			{
				tmpAlpha = 1;
			}
			else
			{
				if (fadingAngle > min && fadingAngle < max)
					tmpAlpha = 1;
				else
				{
					if (fadingAngle <= 90)
						tmpAlpha = (fadingAngle - displayAngle) / (min - displayAngle) * 0.5f;
					else
						tmpAlpha = -(fadingAngle - (180 - displayAngle)) / ((180 - displayAngle) - max) * 0.5f;
				}
			}

			if (tmpFadingAlpha != tmpAlpha)
			{
				tmpFadingAlpha = tmpAlpha;
				fadingColor.a = tmpAlpha;
				if (combinedLine != null)
				{
					for (int i = 0; i < 2; i++)
					{
						for (int j = 0; j < 2; j++)
						{
							texture.SetPixel(i, j, fadingColor);
						}
					}
					texture.Apply();

					combinedLine.GetComponent<MeshRenderer>().material.mainTexture = texture;
				}

				//destObject's alpha value
				foreach (TipComponent ci in tipComp)
				{
					var destObjectColor = textColor;
					if (ci.destObject != null)
					{
						destObjectColor.a = tmpAlpha;
						ci.destObject.GetComponentInChildren<TextMeshPro>().color = destObjectColor;
					}
				}
			}
		}

		void AddControllerTipsList()
		{
			tipInfoList.Clear();

			TipInfo home = new TipInfo();
			home.inputType = InputType.Home;
			home.alignment = Alignment.Balance;
			home.buttonAndTextDistance = 0.035f;
			home.buttonAndLineDistance = 0.0f;
			home.lineLengthAdjustment = -0.003f;
			home.multiLanguage = true;
			home.inputText = "system";

			tipInfoList.Add(home);

			TipInfo app = new TipInfo();
			app.inputType = InputType.App;
			app.alignment = Alignment.Balance;
			app.buttonAndTextDistance = 0.035f;
			app.buttonAndLineDistance = 0.0f;
			app.lineLengthAdjustment = -0.003f;
			app.multiLanguage = true;
			app.inputText = "system";

			tipInfoList.Add(app);

			TipInfo grip = new TipInfo();
			grip.inputType = InputType.Grip;
			grip.alignment = Alignment.Balance;
			grip.buttonAndTextDistance = 0.035f;
			grip.buttonAndLineDistance = 0.0f;
			grip.lineLengthAdjustment = -0.003f;
			grip.multiLanguage = true;
			grip.inputText = "system";

			tipInfoList.Add(grip);

			TipInfo trigger = new TipInfo();
			trigger.inputType = InputType.Trigger;
			trigger.alignment = Alignment.Balance;
			trigger.buttonAndTextDistance = 0.035f;
			trigger.buttonAndLineDistance = 0.0f;
			trigger.lineLengthAdjustment = -0.003f;
			trigger.multiLanguage = true;
			trigger.inputText = "system";

			tipInfoList.Add(trigger);

			TipInfo dt = new TipInfo();
			dt.inputType = InputType.DigitalTrigger;
			dt.alignment = Alignment.Balance;
			dt.buttonAndTextDistance = 0.035f;
			dt.buttonAndLineDistance = 0.0f;
			dt.lineLengthAdjustment = -0.003f;
			dt.multiLanguage = true;
			dt.inputText = "system";

			tipInfoList.Add(dt);

			TipInfo touchpad = new TipInfo();
			touchpad.inputType = InputType.TouchPad;
			touchpad.alignment = Alignment.Balance;
			touchpad.buttonAndTextDistance = 0.035f;
			touchpad.buttonAndLineDistance = 0.0f;
			touchpad.lineLengthAdjustment = -0.003f;
			touchpad.multiLanguage = true;
			touchpad.inputText = "system";

			tipInfoList.Add(touchpad);

			TipInfo vol = new TipInfo();
			vol.inputType = InputType.Volume;
			vol.alignment = Alignment.Balance;
			vol.buttonAndTextDistance = 0.035f;
			vol.buttonAndLineDistance = 0.0f;
			vol.lineLengthAdjustment = -0.003f;
			vol.multiLanguage = true;
			vol.inputText = "system";

			tipInfoList.Add(vol);

			TipInfo thumbstick = new TipInfo();
			thumbstick.inputType = InputType.Thumbstick;
			thumbstick.alignment = Alignment.Balance;
			thumbstick.buttonAndTextDistance = 0.035f;
			thumbstick.buttonAndLineDistance = 0.0f;
			thumbstick.lineLengthAdjustment = -0.003f;
			thumbstick.multiLanguage = true;
			thumbstick.inputText = "system";

			tipInfoList.Add(thumbstick);

			TipInfo buttonA = new TipInfo();
			buttonA.inputType = InputType.ButtonA;
			buttonA.alignment = Alignment.Balance;
			buttonA.buttonAndTextDistance = 0.055f;
			buttonA.buttonAndLineDistance = 0.0f;
			buttonA.lineLengthAdjustment = -0.003f;
			buttonA.multiLanguage = true;
			buttonA.inputText = "system";

			tipInfoList.Add(buttonA);

			TipInfo buttonB = new TipInfo();
			buttonB.inputType = InputType.ButtonB;
			buttonB.alignment = Alignment.Balance;
			buttonB.buttonAndTextDistance = 0.055f;
			buttonB.buttonAndLineDistance = 0.0f;
			buttonB.lineLengthAdjustment = -0.003f;
			buttonB.multiLanguage = true;
			buttonB.inputText = "system";

			tipInfoList.Add(buttonB);

			TipInfo buttonX = new TipInfo();
			buttonX.inputType = InputType.ButtonX;
			buttonX.alignment = Alignment.Balance;
			buttonX.buttonAndTextDistance = 0.055f;
			buttonX.buttonAndLineDistance = 0.0f;
			buttonX.lineLengthAdjustment = -0.003f;
			buttonX.multiLanguage = true;
			buttonX.inputText = "system";

			tipInfoList.Add(buttonX);

			TipInfo buttonY = new TipInfo();
			buttonY.inputType = InputType.ButtonY;
			buttonY.alignment = Alignment.Balance;
			buttonY.buttonAndTextDistance = 0.055f;
			buttonY.buttonAndLineDistance = 0.0f;
			buttonY.lineLengthAdjustment = -0.003f;
			buttonY.multiLanguage = true;
			buttonY.inputText = "system";

			tipInfoList.Add(buttonY);
		}

		bool isConnected()
		{
			for (int i = 0; i < tipComp.Count; i++)
			{
				if (tipComp[i].sourceObject == null)
					return false;
			}
			return true;
		}

		void OnEnable()
		{
			Log.i(LOG_TAG, "Controller Tips - OnEnable()");
			ResetControllerTips();

			RenderModel.onRenderModelReady += OnAdaptiveControllerModelReady;

			tmpEnableControllerTips = enableControllerTips;
			tmpAutoLayout = autoLayout;
			tmpPolyline = polyline;
			tmpFadingEffect = fadingEffect;

			if (enableControllerTips && TMP_Settings.instance == null)
			{
				Log.d(LOG_TAG, "TMP Essentials can't be found. Please import TMP Essentials before using Controller Tips.");
				enableControllerTips = false;
			}

			if (useSystemConfig)
				AddControllerTipsList();
		}

		void OnDisable()
		{
			Log.i(LOG_TAG, "Controller Tips - OnDisable()");
			RenderModel.onRenderModelReady -= OnAdaptiveControllerModelReady;
			ResetControllerTips();
		}

		void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus == true)
			{
				ResetControllerTips();
			}
		}

		// Use this for initialization
		void Start()
		{
		}

		// Update is called once per frame
		void Update()
		{
			if (hmd == null)
				hmd = Camera.main.gameObject;
			if (hmd == null)
			{
				Log.d(LOG_TAG, "Can't get HMD!");
				return;
			}

			if (!isConnected())
			{
				ResetControllerTips();
				Log.i(LOG_TAG, "Controller disconnects!");
				return;
			}

			if (tmpEnableControllerTips != enableControllerTips || tmpAutoLayout != autoLayout || tmpPolyline != polyline ||tmpFadingEffect != fadingEffect)
			{
				tmpEnableControllerTips = enableControllerTips;
				tmpAutoLayout = autoLayout;
				tmpPolyline = polyline;
				tmpFadingEffect = fadingEffect;
				ResetControllerTips();
			}

			if (!enableControllerTips)
			{
				UpdateInfo(tipComp);
				return;
			}

			if (checkInteractionMode)
			{
				showTips = ClientInterface.InteractionMode == XR_InteractionMode.Controller;
			}

			if (showTips != PreShowTips)
			{
				Log.d(LOG_TAG, "Show controller tips state change, new state: " + showTips);
				PreShowTips = showTips;
			}

			if (!showTips)
			{
				UpdateInfo(tipComp);
				return;
			}

			checkCount++;
			if (checkCount > 50)
			{
				checkCount = 0;
				if (rw != null)
				{
					if (rw.getSystemLanguage() != sysLang || rw.getSystemCountry() != sysCountry) ResetControllerTips();
				}
			}
			if (needRedraw == true)
			{
				StartCoroutine(CreateControllerTipsCoroutine());
			}

			if (basedOnEmitter && (emitter != null))
				_targetForward = emitter.transform.rotation * Vector3.forward;
			else
				_targetForward = transform.rotation * Vector3.forward;
			_targetRight = transform.rotation * Vector3.right;
			_targetUp = transform.rotation * Vector3.up;

			zAngle = Vector3.Angle(_targetForward, hmd.transform.forward);
			xAngle = Vector3.Angle(_targetRight, hmd.transform.right);
#if DEBUG
			yAngle = Vector3.Angle(_targetUp, hmd.transform.up);

			if (Log.gpl.Print)
				Log.d(LOG_TAG, "Z: " + _targetForward + ":" + zAngle + ", X: " + _targetRight + ":" + xAngle + ", Y: " + _targetUp + ":" + yAngle);
#endif

			UpdateInfo(tipComp);
		}
	}
}
