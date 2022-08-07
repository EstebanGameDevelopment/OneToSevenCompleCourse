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
using Wave.Native;
using System;
using UnityEngine.XR;
using Wave.Essence.Extra;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Wave.Essence.Controller.Model
{
	[System.Serializable]
	public class MeshObject
	{
		public string MeshName;
		public bool hasEffect;
		public GameObject gameObject;
		public Vector3 originPosition;
		public Material originMat;
		public Material effectMat;
	}

	[System.Serializable]
	public class BinaryButtonObject
	{
		public string MeshName;
		public bool hasEffect;
		public bool hasAnimation;
		public bool alsoEffect;
		public GameObject gameObject;
		public Material originMat;
		public Material effectMat;
		public Vector3 originPosition;
		public Vector3 originRotation;
		public Vector3 originScale;
		public Vector3 pressPosition;
		public Vector3 pressRotation;
		public Vector3 pressScale;
		public bool lastState;

		public BinaryButtonObject()
		{
			MeshName = "";
			hasEffect = false;
			hasAnimation = false;
			lastState = false;
			alsoEffect = false;
			originPosition = Vector3.zero;
			pressPosition = Vector3.zero;
			originRotation = Vector3.zero;
			pressRotation = Vector3.zero;
			originScale = Vector3.one;
			pressScale = Vector3.one;
			originMat = null;
			effectMat = null;
		}
	}

	[System.Serializable]
	public class Travel1DObject
	{
		public string MeshName;
		public bool hasEffect;
		public bool hasAnimation;
		public bool isInAnimation;
		public GameObject gameObject;
		public Material originMat;
		public Material effectMat;
		public Vector3 originPosition;
		public Quaternion originRotation;
		public Vector3 originScale;
		public Vector3 pressPosition;
		public Quaternion pressRotation;
		public Vector3 pressScale;

		//public Vector3 scalePosition;
		//public Quaternion scaleRotation;
		//public Vector3 scaleScale;

		public Travel1DObject()
		{
			MeshName = "";
			hasEffect = false;
			hasAnimation = false;
			isInAnimation = false;
			originPosition = Vector3.zero;
			pressPosition = Vector3.zero;
			originRotation = Quaternion.identity;
			pressRotation = Quaternion.identity;
			originScale = Vector3.one;
			pressScale = Vector3.one;
			originMat = null;
			effectMat = null;
		}
	}

	[System.Serializable]
	public class TouchpadObject
	{
		public string MeshName;
		public bool hasEffect;
		public bool hasAnimation;
		public GameObject gameObject;
		public Material originMat;
		public Material effectMat;
		public Vector3 originPosition;
		public Vector3 originRotation;
		public Vector3 originScale;
		public Vector3 pressPosition;
		public Vector3 pressRotation;
		public Vector3 pressScale;

		public TouchpadObject()
		{
			MeshName = "";
			hasEffect = false;
			hasAnimation = false;
			originPosition = Vector3.zero;
			pressPosition = Vector3.zero;
			originRotation = Vector3.zero;
			pressRotation = Vector3.zero;
			originScale = Vector3.one;
			pressScale = Vector3.one;
			originMat = null;
			effectMat = null;
		}
	}

	[System.Serializable]
	public class ThumbstickObject
	{
		public string MeshName;
		public bool hasEffect;
		public bool hasAnimation;
		public GameObject gameObject;
		public Material originMat;
		public Material effectMat;

		public Vector3 centerPosition;
		public Vector3 centerRotation;
		public Vector3 centerScale;

		public Vector3 upPosition;
		public Vector3 upRotation;
		public Vector3 upScale;

		public Vector3 rightPosition;
		public Vector3 rightRotation;
		public Vector3 rightScale;

		public Vector3 ptW;
		public Vector3 ptU;
		public Vector3 ptV;
		public float raidus;

		public Vector3 rtW;
		public Vector3 rtU;
		public Vector3 rtV;

		public Vector3 stW;
		public Vector3 stU;
		public Vector3 stV;

		public Vector3 xAngle;
		public Vector3 zAngle;

		public ThumbstickObject()
		{
			MeshName = "";
			hasEffect = false;
			hasAnimation = false;
			centerPosition = Vector3.zero;
			centerRotation = Vector3.zero;
			centerScale = new Vector3(1, 1, 1);
			upPosition = Vector3.zero;
			upRotation = Vector3.zero;
			upScale = new Vector3(1, 1, 1);
			rightPosition = Vector3.zero;
			rightRotation = Vector3.zero;
			rightScale = new Vector3(1, 1, 1);

			originMat = null;
			effectMat = null;
		}
	}

	[System.Serializable]
	public class CtrlerModelAnimPoseData
	{
		public Vector3 position;
		public Vector3 rotation;
		public Vector3 scale;

		public CtrlerModelAnimPoseData()
		{
			position = Vector3.zero;
			rotation = Vector3.zero;
			scale = new Vector3(1, 1, 1);
		}
	}

	[System.Serializable]
	public class CtrlerModelAnimNodeData
	{
		public string btnName;
		public uint btnType;
		public uint alsoBlueEffect;
		public CtrlerModelAnimPoseData origin;
		public CtrlerModelAnimPoseData pressed;
		public CtrlerModelAnimPoseData minX;
		public CtrlerModelAnimPoseData maxX;
		public CtrlerModelAnimPoseData minY;
		public CtrlerModelAnimPoseData maxY;

		public CtrlerModelAnimNodeData()
		{
			btnName = "";
			btnType = 99;
			alsoBlueEffect = 99;
			origin = new CtrlerModelAnimPoseData();
			pressed = new CtrlerModelAnimPoseData();
			minX = new CtrlerModelAnimPoseData();
			maxX = new CtrlerModelAnimPoseData();
			minY = new CtrlerModelAnimPoseData();
			maxY = new CtrlerModelAnimPoseData();
		}
	}

	public class ButtonEffect : MonoBehaviour
	{
		private static string LOG_TAG = "ButtonEffect";
		public bool enableButtonEffect = true;
		public XR_Hand HandType = XR_Hand.Dominant;
		public bool useSystemConfig = true;
		public Color buttonEffectColor = new Color(0, 179, 227, 255);
		public bool collectInStart = true;
		private WVR_DeviceType deviceType = WVR_DeviceType.WVR_DeviceType_Invalid;
		private bool showAnimation = false;

		private void PrintDebugLog(string msg)
		{
			Log.d(LOG_TAG, "Hand: " + HandType + ", " + msg, true);
		}

		private void PrintInfoLog(string msg)
		{
			Log.i(LOG_TAG, "Hand: " + HandType + ", " + msg, true);
		}

		public class WVR_InputObject
		{
			public WVR_InputId destination;
			public WVR_InputId sourceId;
		}

		private Dictionary<string, CtrlerModelAnimNodeData> animTable = new Dictionary<string, CtrlerModelAnimNodeData>();

		#region binary button definition
		private static readonly string[] clickKeys = new string[] {
			"MenuButton",
			"TriggerButton",
			"TriggerButton",
			"Primary2DAxisClick",
			"PrimaryButton", //A
			"SecondaryButton",
			"PrimaryButton", //X
			"SecondaryButton",
			"TriggerButton",
			"Primary2DAxisClick",
			"GripButton"
		};

		private static readonly string[] clickMeshNames = new string[] {
			"__CM__AppButton", // WVR_InputId_Alias1_Menu
			"__CM__TriggerKey", // BumperKey in DS < 3.2
			"__CM__DigitalTriggerKey", // BumperKey in DS < 3.2
			"__CM__TouchPad", // TouchPad_Press
			"__CM__ButtonA", // ButtonA
			"__CM__ButtonB", // ButtonB
			"__CM__ButtonX", // ButtonX
			"__CM__ButtonY", // ButtonY
			"__CM__BumperKey", // BumperKey in DS >= 3.2
			"__CM__Thumbstick", // Thumbstick
			"__CM__Grip"
		};

		private BinaryButtonObject[] pressObjectArray = new BinaryButtonObject[clickKeys.Length];
		#endregion

		#region Travel1D definition
		private static readonly string[] travel1DKey = new string[] {
			"Grip",
			"Trigger"
		};

		private static readonly string[] travel1DAxis = new string[] {
			"Grip",
			"Trigger"
		};

		private static readonly WVR_InputId[] travel1DId = new WVR_InputId[]
		{
			WVR_InputId.WVR_InputId_Alias1_Grip,
			WVR_InputId.WVR_InputId_Alias1_Trigger
		};

		private static readonly string[] travel1DMeshNames = new string[] {
			"__CM__Grip", // WVR_InputId_Alias1_Grip
			"__CM__TriggerKey" // TriggerKey
		};

		private Travel1DObject[] travel1DObjectArray = new Travel1DObject[travel1DKey.Length];
		#endregion

		#region Touchpad definition
		private static readonly string[] TouchPadKeys = new string[] {
			"Primary2DAxisTouch",
			"Secondary2DAxisTouch"
		};

		private static readonly string[] TouchPadAxisNames = new string[] {
			"Primary2DAxis",
			"Secondary2DAxis"
		};

		private static readonly string[] TouchPadMeshNames = new string[] {
			"__CM__TouchPad_Touch", // TouchPad_Touch
			"__CM__TouchPad_Touch" // TouchPad_Touch
		};

		private TouchpadObject[] touchpadObjectArray = new TouchpadObject[TouchPadKeys.Length];
		#endregion

		#region Thumbstick definition
		private static readonly string[] thumbstickKeys = new string[] {
			"Primary2DAxisTouch",
			"Secondary2DAxisTouch"
		};

		private static readonly string[] thumbstickAxisNames = new string[] {
			"Primary2DAxis",
			"Secondary2DAxis"
		};

		private static readonly WVR_InputId[] thumbstickId = new WVR_InputId[]
{
			WVR_InputId.WVR_InputId_Alias1_Thumbstick,
			WVR_InputId.WVR_InputId_Alias1_Thumbstick
};

		private static readonly string[] thumbstickMeshNames = new string[] {
			"__CM__Thumbstick",
			"__CM__Thumbstick"
		};

		private ThumbstickObject[] thumbstickObjectArray = new ThumbstickObject[thumbstickKeys.Length];
		#endregion

		private GameObject touchpad = null;
		private GameObject systemButton = null;
		private Material systemBtnOrigin = null;

		private bool systemAnimation = false;

		private Vector3 systempressPosition;
		private Vector3 systempressRotation;
		private Vector3 systempressScale;

		private Vector3 systeminitPosition;
		private Vector3 systeminitRotation;
		private Vector3 systeminitScale;

		private bool systemAlsoBlueEffect = false;

		private Mesh touchpadMesh = null;
		private Mesh toucheffectMesh = null;
		private bool currentIsLeftHandMode = false;
		private XRNode node;
		private bool resetEffectIfDisconnect = true;

		string renderModelName = "";

		[HideInInspector]
		public bool checkInteractionMode = false;

		void onRenderModelReady(XR_Hand hand)
		{
			if (hand == this.HandType)
			{
				PrintInfoLog("onRenderModelReady(" + hand + ") and collect");
				resetEffectIfDisconnect = true;
				CollectEffectObjects();
			}
		}
		void onRenderModelRemoved(XR_Hand hand)
		{
			if (hand != this.HandType) { return; }
			PrintInfoLog("onRenderModelRemoved() " + hand);
			if (touchMat != null)
			{
				Destroy(touchMat);
				touchMat = null;
			}
		}

		void OnEnable()
		{
			if (HandType == XR_Hand.Dominant)
			{
				node = XRNode.RightHand;
				deviceType = WVR_DeviceType.WVR_DeviceType_Controller_Right;
			}
			else
			{
				node = XRNode.LeftHand;
				deviceType = WVR_DeviceType.WVR_DeviceType_Controller_Left;
			}
			resetButtonState();
			RenderModel.onRenderModelReady += onRenderModelReady;
			RenderModel.onRenderModelRemoved += onRenderModelRemoved;
		}

		void OnDisable()
		{
			RenderModel.onRenderModelReady -= onRenderModelReady;
			RenderModel.onRenderModelRemoved -= onRenderModelRemoved;
		}

		void OnApplicationPause(bool pauseStatus)
		{
			if (!pauseStatus) // resume
			{
				PrintInfoLog("Pause(" + pauseStatus + ") and reset button state");
				resetButtonState();
			}
		}

		void resetButtonState()
		{
			PrintDebugLog("reset button state");
			if (!enableButtonEffect)
			{
				PrintInfoLog("enable button effect : false");
				return;
			}

			for (int i = 0; i < pressObjectArray.Length; i++)
			{
				if (pressObjectArray[i] == null) continue;
				if (pressObjectArray[i].hasEffect)
				{
					if (pressObjectArray[i].gameObject != null && pressObjectArray[i].originMat != null && pressObjectArray[i].effectMat != null)
					{
						pressObjectArray[i].gameObject.GetComponent<MeshRenderer>().material = pressObjectArray[i].originMat;
						pressObjectArray[i].lastState = false;
						pressObjectArray[i].gameObject.transform.localPosition = pressObjectArray[i].originPosition;
						pressObjectArray[i].gameObject.transform.localEulerAngles = pressObjectArray[i].originRotation;
						pressObjectArray[i].gameObject.transform.localScale = pressObjectArray[i].originScale;
					}
				}
			}

			for (int i = 0; i < touchpadObjectArray.Length; i++)
			{
				if (touchpadObjectArray[i] == null) continue;
				if (touchpadObjectArray[i].hasEffect)
				{
					if (touchpadObjectArray[i].gameObject != null && touchpadObjectArray[i].originMat != null && touchpadObjectArray[i].effectMat != null)
					{
						var renderer = touchpadObjectArray[i].gameObject.GetComponent<MeshRenderer>();
						renderer.material = touchpadObjectArray[i].originMat;
						renderer.enabled = false;
					}
				}
			}

			if (systemButton != null)
			{
				systemButton.GetComponent<MeshRenderer>().material = systemBtnOrigin;
			}
		}

		// Use this for initialization
		void Start()
		{
			resetButtonState();
			if (collectInStart) CollectEffectObjects();
		}

		// Update is called once per frame
		//int touch_index = -1;
		void Update()
		{
			if (checkInteractionMode)
			{
				if (ClientInterface.InteractionMode != XR_InteractionMode.Controller)
					return;
			}

			if (!checkConnection())
			{
				if (resetEffectIfDisconnect)
				{
					resetEffectIfDisconnect = false;
					resetButtonState();
				}
				return;
			}

			if (!enableButtonEffect)
				return;

			if (WaveEssence.Instance)
			{
				if (currentIsLeftHandMode != WaveEssence.Instance.IsLeftHanded)
				{
					currentIsLeftHandMode = WaveEssence.Instance.IsLeftHanded;
					PrintInfoLog("Controller role is changed to " + (currentIsLeftHandMode ? "Left" : "Right") + " and reset button state");
					resetButtonState();
				}
			}

			#region ButtonPress
			// should have animation first
			for (int i = 0; i < clickKeys.Length; i++)
			{
				if (pressObjectArray[i] == null) continue;
				if (pressObjectArray[i].gameObject == null) continue;
				if (pressObjectArray[i].gameObject.GetComponent<MeshRenderer>() == null) continue;
				if (InputDevices.GetDeviceAtXRNode(node) != null)
				{
					bool buttonState;
					if (InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(new InputFeatureUsage<bool>(clickKeys[i]), out buttonState)) {
						if (buttonState)
						{
							if (buttonState != pressObjectArray[i].lastState)
							{
								PrintInfoLog(clickKeys[i] + " pressed");
								pressObjectArray[i].lastState = buttonState;
							}

							if (showAnimation) {
								if (pressObjectArray[i].hasAnimation)
								{
									if (pressObjectArray[i].gameObject != null)
									{
										pressObjectArray[i].gameObject.transform.localPosition = pressObjectArray[i].pressPosition;
										//pressObjectArray[i].gameObject.transform.localEulerAngles = pressObjectArray[i].pressRotation;
										//pressObjectArray[i].gameObject.transform.localScale = pressObjectArray[i].pressScale;

										if (pressObjectArray[i].alsoEffect)
										{
											//PrintInfoLog(clickKeys[i] + " also effect");
											if (pressObjectArray[i].gameObject != null && pressObjectArray[i].originMat != null && pressObjectArray[i].effectMat != null)
											{
												pressObjectArray[i].gameObject.GetComponent<MeshRenderer>().materials = new Material[] 
												{
													pressObjectArray[i].effectMat, pressObjectArray[i].originMat
												};
												//pressObjectArray[i].gameObject.GetComponent<MeshRenderer>().material = pressObjectArray[i].effectMat;
												}
										}
									}
								}
							}
							else if (pressObjectArray[i].hasEffect)
							{
								if (pressObjectArray[i].gameObject != null && pressObjectArray[i].originMat != null && pressObjectArray[i].effectMat != null)
								{
									pressObjectArray[i].gameObject.GetComponent<MeshRenderer>().materials = new Material[]
									{
										pressObjectArray[i].effectMat, pressObjectArray[i].originMat
									};
									//pressObjectArray[i].gameObject.GetComponent<MeshRenderer>().material = pressObjectArray[i].effectMat;
								}
							}
						}
						else
						{
							if (buttonState != pressObjectArray[i].lastState)
							{
								PrintInfoLog(clickKeys[i] + " released");
								pressObjectArray[i].lastState = buttonState;
							}

							if (showAnimation)
							{
								if (pressObjectArray[i].hasAnimation)
								{
									if (pressObjectArray[i].gameObject != null)
									{
										pressObjectArray[i].gameObject.transform.localPosition = pressObjectArray[i].originPosition;
										//pressObjectArray[i].gameObject.transform.localEulerAngles = pressObjectArray[i].originRotation;
										//pressObjectArray[i].gameObject.transform.localScale = pressObjectArray[i].originScale;

										if (pressObjectArray[i].gameObject != null && pressObjectArray[i].originMat != null && pressObjectArray[i].effectMat != null)
										{
											pressObjectArray[i].gameObject.GetComponent<MeshRenderer>().material = pressObjectArray[i].originMat;
										}
									}
								}
							}
							else if (pressObjectArray[i].hasEffect)
							{
								if (pressObjectArray[i].gameObject != null && pressObjectArray[i].originMat != null && pressObjectArray[i].effectMat != null)
								{
									pressObjectArray[i].gameObject.GetComponent<MeshRenderer>().material = pressObjectArray[i].originMat;
								}
							}
						}
					}
				}
			}
			#endregion

			#region Travel 1D axis
			// should have animation first
			for (int i = 0; i < travel1DKey.Length; i++)
			{
				var travel1DObject = travel1DObjectArray[i];
				if (travel1DObject == null) continue;
				if (travel1DObject.gameObject == null) continue;
				if (travel1DObject.gameObject.GetComponent<MeshRenderer>() == null) continue;
				if (InputDevices.GetDeviceAtXRNode(node) != null)
				{
					float axis;

					bool touch = WXRDevice.ButtonTouch(deviceType, travel1DId[i]);

					if (touch) PrintInfoLog(travel1DId[i] + " touch");

					bool untouch = WXRDevice.ButtonUntouch(deviceType, travel1DId[i]);

					if (untouch) PrintInfoLog(travel1DId[i] + " untouch");

					bool touching = WXRDevice.ButtonTouching(deviceType, travel1DId[i]);

					if (touch || touching || untouch)
					{
						if (InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(new InputFeatureUsage<float>(travel1DKey[i]), out axis))
						{
							if (travel1DObject.hasAnimation)
							{
								if (travel1DObject.gameObject != null)
								{
									if (axis == 0)
										travel1DObject.isInAnimation = false;
									else
										travel1DObject.isInAnimation = true;

									var transform = travel1DObject.gameObject.transform;
									axis = Mathf.Clamp01(axis);
									transform.localPosition = Vector3.Lerp(travel1DObject.originPosition, travel1DObject.pressPosition, axis);
									transform.localRotation = Quaternion.Lerp(travel1DObject.originRotation, travel1DObject.pressRotation, axis);
								}
							}
						}
					}
					else
					{
						if (travel1DObject.gameObject != null)
						{
							if (travel1DObject.originMat != null && travel1DObject.effectMat != null)
							{
								travel1DObject.gameObject.GetComponent<MeshRenderer>().material = travel1DObject.originMat;
							}
							if (travel1DObject.hasAnimation && travel1DObject.isInAnimation)
							{
								travel1DObject.isInAnimation = false;
								travel1DObject.gameObject.transform.localPosition = travel1DObject.originPosition;
								travel1DObject.gameObject.transform.localRotation = travel1DObject.originRotation;
							}
						}
					}
				}
			}
			#endregion

			#region Touchpad
			for (int i = 0; i < TouchPadKeys.Length; i++)
			{
				if (touchpadObjectArray[i] == null) continue;
				if (touchpadObjectArray[i].gameObject == null) continue;
				if (touchpadObjectArray[i].gameObject.GetComponent<MeshRenderer>() == null) continue;
				if (InputDevices.GetDeviceAtXRNode(node) != null)
				{
					bool buttonState;
					int _i = GetTouchInputMapping(i);

					if (InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(new InputFeatureUsage<bool>(TouchPadKeys[i]), out buttonState))
					{
						if (buttonState)
						{
							if (touchpadObjectArray[_i].gameObject != null && touchpadObjectArray[_i].originMat != null && touchpadObjectArray[_i].effectMat != null)
							{
								Vector2 axis;
								if (InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(new InputFeatureUsage<Vector2>(TouchPadAxisNames[i]), out axis))
								{
									//touchpadObjectArray[_i].gameObject.GetComponent<MeshRenderer>().material = touchpadObjectArray[_i].effectMat;
									var renderer = touchpadObjectArray[_i].gameObject.GetComponent<MeshRenderer>();
									renderer.materials = new Material[] {
										touchpadObjectArray[_i].effectMat, touchpadObjectArray[_i].originMat
									};
									renderer.enabled = true;

									if (isTouchPadSetting)
									{
										float xangle = touchCenter.x / 100 + (axis.x * raidus * touchPtU.x) / 100 + (axis.y * raidus * touchPtW.x) / 100 + (touchptHeight * touchPtV.x) / 100;
										float yangle = touchCenter.y / 100 + (axis.x * raidus * touchPtU.y) / 100 + (axis.y * raidus * touchPtW.y) / 100 + (touchptHeight * touchPtV.y) / 100;
										float zangle = touchCenter.z / 100 + (axis.x * raidus * touchPtU.z) / 100 + (axis.y * raidus * touchPtW.z) / 100 + (touchptHeight * touchPtV.z) / 100;

										// touchAxis
										if (Log.gpl.Print)
											Log.d(LOG_TAG, "Hand: " + HandType + ", " + "Touchpad axis x: " + axis.x + " axis.y: " + axis.y + ", xangle: " + xangle + ", yangle: " + yangle + ", zangle: " + zangle);

										Vector3 touchPos = transform.TransformPoint(xangle, yangle, zangle);

										touchpadObjectArray[_i].gameObject.transform.position = touchPos;

									}
									else
									{
										float xangle = axis.x * (touchpadMesh.bounds.size.x * touchpad.transform.localScale.x - toucheffectMesh.bounds.size.x * touchpadObjectArray[_i].gameObject.transform.localScale.x) / 2;
										float yangle = axis.y * (touchpadMesh.bounds.size.z * touchpad.transform.localScale.z - toucheffectMesh.bounds.size.z * touchpadObjectArray[_i].gameObject.transform.localScale.z) / 2;

										var height = touchpadMesh.bounds.size.y * touchpad.transform.localScale.y;

										var h = Mathf.Abs(touchpadMesh.bounds.max.y);
										if (Log.gpl.Print)
										{

											Log.d(LOG_TAG, "Hand: " + HandType + ", " + "Axis2D axis x: " + axis.x + " axis.y: " + axis.y + ", xangle: " + xangle + ", yangle: " + yangle + ", height: " + height + ",h: " + h);

#if DEBUG
											Log.d(LOG_TAG, "Hand: " + HandType + ", " + "TouchEffectMesh.bounds.size: " + toucheffectMesh.bounds.size.x + ", " + toucheffectMesh.bounds.size.y + ", " + toucheffectMesh.bounds.size.z);
											Log.d(LOG_TAG, "Hand: " + HandType + ", " + "TouchEffectMesh.scale: " + touchpadObjectArray[_i].gameObject.transform.localScale.x + ", " + touchpadObjectArray[_i].gameObject.transform.localScale.y + ", " + touchpadObjectArray[_i].gameObject.transform.localScale.z);
											Log.d(LOG_TAG, "Hand: " + HandType + ", " + "TouchpadMesh.bounds.size: " + touchpadMesh.bounds.size.x + ", " + touchpadMesh.bounds.size.y + ", " + touchpadMesh.bounds.size.z);
											Log.d(LOG_TAG, "Hand: " + HandType + ", " + "TouchpadMesh. scale: " + touchpadObjectArray[_i].gameObject.transform.localScale.x + ", " + touchpadObjectArray[_i].gameObject.transform.localScale.y + ", " + touchpadObjectArray[_i].gameObject.transform.localScale.z);
											Log.d(LOG_TAG, "Hand: " + HandType + ", " + "TouchEffect.originPosition: " + touchpadObjectArray[_i].originPosition.x + ", " + touchpadObjectArray[_i].originPosition.y + ", " + touchpadObjectArray[_i].originPosition.z);
#endif
										}
										Vector3 translateVec = Vector3.zero;
										translateVec = new Vector3(xangle, h, yangle);
										touchpadObjectArray[_i].gameObject.transform.localPosition = touchpadObjectArray[_i].originPosition + translateVec;
									}
								}
							}
						}
						else
						{
							var renderer = touchpadObjectArray[_i].gameObject.GetComponent<MeshRenderer>();
							renderer.material = touchpadObjectArray[_i].originMat;
							renderer.enabled = false;
						}
					}
				}
			}
			#endregion

			#region Thumbstick
			for (int i = 0; i < thumbstickKeys.Length; i++)
			{
				if (thumbstickObjectArray[i] == null) continue;
				if (thumbstickObjectArray[i].gameObject == null) continue;
				if (thumbstickObjectArray[i].gameObject.GetComponent<MeshRenderer>() == null) continue;
				if (InputDevices.GetDeviceAtXRNode(node) != null)
				{
					bool buttonState;
					if (InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(new InputFeatureUsage<bool>(thumbstickKeys[i]), out buttonState))
					{
						if (buttonState)
						{
							if (thumbstickObjectArray[i].gameObject != null && thumbstickObjectArray[i].hasAnimation)
							{
								Vector2 axis;
								if (InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(new InputFeatureUsage<Vector2>(thumbstickAxisNames[i]), out axis))
								{
									float xPos = thumbstickObjectArray[i].centerPosition.x + ((-1) * axis.x * thumbstickObjectArray[i].raidus * thumbstickObjectArray[i].ptU.x) + (axis.y * thumbstickObjectArray[i].raidus * thumbstickObjectArray[i].ptW.x);// + (touchptHeight * touchPtV.x) / 100;
									float yPos = thumbstickObjectArray[i].centerPosition.y + ((-1) * axis.x * thumbstickObjectArray[i].raidus * thumbstickObjectArray[i].ptU.y) + (axis.y * thumbstickObjectArray[i].raidus * thumbstickObjectArray[i].ptW.y);// + (touchptHeight * touchPtV.y) / 100;
									float zPos = thumbstickObjectArray[i].centerPosition.z + ((-1) * axis.x * thumbstickObjectArray[i].raidus * thumbstickObjectArray[i].ptU.z) + (axis.y * thumbstickObjectArray[i].raidus * thumbstickObjectArray[i].ptW.z);// + (touchptHeight * touchPtV.z) / 100;

									float xValue = thumbstickObjectArray[i].centerRotation.x + ((-1) * axis.y * thumbstickObjectArray[i].xAngle.x); // + ((-1) * axis.x * thumbstickObjectArray[i].xAngle.x)
									float yValue = thumbstickObjectArray[i].centerRotation.y;
									float zValue = thumbstickObjectArray[i].centerRotation.z + ((-1) * axis.x * thumbstickObjectArray[i].zAngle.z); // + ((-1) * axis.x * thumbstickObjectArray[i].xAngle.x)

									//if (Log.gpl.Print)
									//{
									//	Log.d(LOG_TAG, "Hand: " + HandType + ", " + "thumbstick axis x: " + axis.x + " axis.y: " + axis.y);
									//	Log.d(LOG_TAG, "Hand: " + HandType + ", " + "xPos: " + xPos + " yPos: " + yPos + " zPos: " + zPos);
									//	Log.d(LOG_TAG, "Hand: " + HandType + ", " + "xValue: " + xValue + " yValue: " + yValue + " zValue: " + zValue);
									//}

									thumbstickObjectArray[i].gameObject.transform.localEulerAngles = new Vector3(xValue, yValue, zValue);
								}
							}
						} else
						{
							if (thumbstickObjectArray[i].gameObject != null)
							{
								thumbstickObjectArray[i].gameObject.transform.localPosition = thumbstickObjectArray[i].centerPosition;
								thumbstickObjectArray[i].gameObject.transform.localEulerAngles = thumbstickObjectArray[i].centerRotation;
								thumbstickObjectArray[i].gameObject.transform.localScale = thumbstickObjectArray[i].centerScale;

								if (thumbstickObjectArray[i].gameObject != null && thumbstickObjectArray[i].originMat != null && thumbstickObjectArray[i].effectMat != null)
								{
									thumbstickObjectArray[i].gameObject.GetComponent<MeshRenderer>().material = thumbstickObjectArray[i].originMat;
								}
							}
						}
					}
				}
			}
			#endregion

			// System button
			WVR_DeviceType dt;

			if (HandType == XR_Hand.Dominant)
			{
				dt = WVR_DeviceType.WVR_DeviceType_Controller_Right;
			} else
			{
				dt = WVR_DeviceType.WVR_DeviceType_Controller_Left;
			}

			bool sPress = WXRDevice.ButtonPress(dt, WVR_InputId.WVR_InputId_Alias1_System);

			if (WXRDevice.ButtonPress(dt, WVR_InputId.WVR_InputId_Alias1_System))
			{
				if (systemButton != null)
					systemButton.GetComponent<MeshRenderer>().material = effectMat;

				if (systemAnimation)
				{
					systemButton.transform.localPosition = systempressPosition;
					systemButton.transform.localEulerAngles = systempressRotation;
					systemButton.transform.localScale = systempressScale;
				}
			}

			if (WXRDevice.ButtonRelease(dt, WVR_InputId.WVR_InputId_Alias1_System))
			{
				if (systemButton != null)
					systemButton.GetComponent<MeshRenderer>().material = systemBtnOrigin;

				if (systemAnimation)
				{
					systemButton.transform.localPosition = systeminitPosition;
					systemButton.transform.localEulerAngles = systeminitRotation;
					systemButton.transform.localScale = systeminitScale;
				}
			}
		}

		private Material effectMat = null;
		private Material touchMat = null;
		private bool mergeToOneBone = false;
		private bool isTouchPadSetting = false;
		private Vector3 touchCenter = Vector3.zero;
		private float raidus;
		private Vector3 touchPtW; //W is direction of the +y analog.
		private Vector3 touchPtU; //U is direction of the +x analog.
		private Vector3 touchPtV; //V is normal of moving plane of touchpad.
		private float touchptHeight = 0.0f;

		private bool checkConnection()
		{
			bool validPoseState;
			if (InputDevices.GetDeviceAtXRNode(node) == null) return false;
			if (!InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(new InputFeatureUsage<bool>("IsTracked"), out validPoseState))
				return false;

			return validPoseState;
		}

		private WVR_DeviceType checkDeviceType()
		{
			return (HandType == XR_Hand.Right ? WVR_DeviceType.WVR_DeviceType_Controller_Right : WVR_DeviceType.WVR_DeviceType_Controller_Left);
		}

		private bool GetTouchPadParam()
		{
			WVR_DeviceType type = checkDeviceType();
			bool _connected = checkConnection();
			if (!_connected)
			{
				PrintDebugLog("Device is disconnect: ");
				return false;
			}

            renderModelName = ClientInterface.GetCurrentRenderModelName(type);

			if (renderModelName.Equals(""))
			{
				PrintDebugLog("Get " + type + " render model name fail!");
				return false;
			}

			PrintDebugLog("current render model name: " + renderModelName);

			ModelResource modelResource = ResourceHolder.Instance.getRenderModelResource(renderModelName, HandType, mergeToOneBone);

			if ((modelResource == null) || (modelResource.TouchSetting == null))
			{
				PrintDebugLog("Get render model resource fail!");
				return false;
			}

			touchCenter = modelResource.TouchSetting.touchCenter;
			touchPtW = modelResource.TouchSetting.touchPtW;
			touchPtU = modelResource.TouchSetting.touchPtU;
			touchPtV = modelResource.TouchSetting.touchPtV;
			raidus = modelResource.TouchSetting.raidus;
			touchptHeight = modelResource.TouchSetting.touchptHeight;

			PrintDebugLog("touchCenter! x: " + touchCenter.x + " ,y: " + touchCenter.y + " ,z: " + touchCenter.z);
			PrintDebugLog("touchPtW! x: " + touchPtW.x + " ,y: " + touchPtW.y + " ,z: " + touchPtW.z);
			PrintDebugLog("touchPtU! x: " + touchPtU.x + " ,y: " + touchPtU.y + " ,z: " + touchPtU.z);
			PrintDebugLog("touchPtV! x: " + touchPtV.x + " ,y: " + touchPtV.y + " ,z: " + touchPtV.z);
			PrintDebugLog("raidus: " + raidus);
			PrintDebugLog("Floating distance : " + touchptHeight);

			return true;
		}

		private int GetPressInputMapping(int pressIds_Index)
		{
			return pressIds_Index;
		}

		private int GetTouchInputMapping(int touchIds_Index)
		{
			return touchIds_Index;
		}

		private void copyButtonValues(ref CtrlerModelAnimPoseData dest, WVR_CtrlerModelAnimPoseData src)
		{
			dest.position.x = src.position.v0;//-1f;
			dest.position.y = src.position.v1;
			dest.position.z = src.position.v2 * -1f;

			dest.rotation.x = src.rotation.v0 * -1f;
			dest.rotation.y = src.rotation.v1 * -1f;
			dest.rotation.z = src.rotation.v2;

			dest.scale.x = src.scale.v0;
			dest.scale.y = src.scale.v1;
			dest.scale.z = src.scale.v2;
		}

		CtrlerModelAnimNodeData tmpAnimData;

		private void CollectEffectObjects() // collect controller object which has effect
		{
			PrintInfoLog("CollectEffectObjects start, IntPtr size = " + IntPtr.Size);
			WVR_DeviceType type = checkDeviceType();

			IntPtr AnimData = IntPtr.Zero;
			animTable.Clear();
			showAnimation = false;

			WVR_Result r = Interop.WVR_GetCtrlerModelAnimNodeData(type, ref AnimData);

			PrintInfoLog("CollectEffectObjects, AnimData IntPtr = " + AnimData.ToInt32() + " sizeof(WVR_CtrlerModelAnimData) = " + Marshal.SizeOf(typeof(WVR_CtrlerModelAnimData)) + " r = " + r);
			int IntBits = IntPtr.Size;
			if (r == WVR_Result.WVR_Success)
			{
				if (AnimData != IntPtr.Zero)
				{
					WVR_CtrlerModelAnimData anim = (WVR_CtrlerModelAnimData)Marshal.PtrToStructure(AnimData, typeof(WVR_CtrlerModelAnimData));

					PrintInfoLog("name = " + anim.name + " , size = " + anim.size);
					if (anim.size > 0) showAnimation = true;

					int szStruct = Marshal.SizeOf(typeof(WVR_CtrlerModelAnimNodeData));

					for (int i = 0; i < anim.size; i++)
					{
						WVR_CtrlerModelAnimNodeData wcmand;

						if (IntBits == 4)
							wcmand = (WVR_CtrlerModelAnimNodeData)Marshal.PtrToStructure(new IntPtr(anim.animDatas.ToInt32() + (szStruct * i)), typeof(WVR_CtrlerModelAnimNodeData));
						else
							wcmand = (WVR_CtrlerModelAnimNodeData)Marshal.PtrToStructure(new IntPtr(anim.animDatas.ToInt64() + (szStruct * i)), typeof(WVR_CtrlerModelAnimNodeData));

						//PrintInfoLog("name=" + wcmand.name + ", type=" + wcmand.type + ", blueeffect=" + wcmand.blueEffect);
						//PrintInfoLog("Origin ----------");
						//PrintInfoLog("pos x=" + wcmand.origin.position.v0 + ", y=" + +wcmand.origin.position.v1 + ", z=" + wcmand.origin.position.v2);
						//PrintInfoLog("rot x=" + wcmand.origin.rotation.v0 + ", y=" + +wcmand.origin.rotation.v1 + ", z=" + wcmand.origin.rotation.v2);
						//PrintInfoLog("pressed ----------");
						//PrintInfoLog("pos x=" + wcmand.pressed.position.v0 + ", y=" + +wcmand.pressed.position.v1 + ", z=" + wcmand.pressed.position.v2);
						//PrintInfoLog("rot x=" + wcmand.pressed.rotation.v0 + ", y=" + +wcmand.pressed.rotation.v1 + ", z=" + wcmand.pressed.rotation.v2);
						//PrintInfoLog("minX ----------");
						//PrintInfoLog("pos x=" + wcmand.minX.position.v0 + ", y=" + +wcmand.minX.position.v1 + ", z=" + wcmand.minX.position.v2);
						//PrintInfoLog("rot x=" + wcmand.minX.rotation.v0 + ", y=" + +wcmand.minX.rotation.v1 + ", z=" + wcmand.minX.rotation.v2);
						//PrintInfoLog("maxX ----------");
						//PrintInfoLog("pos x=" + wcmand.maxX.position.v0 + ", y=" + +wcmand.maxX.position.v1 + ", z=" + wcmand.maxX.position.v2);
						//PrintInfoLog("rot x=" + wcmand.maxX.rotation.v0 + ", y=" + +wcmand.maxX.rotation.v1 + ", z=" + wcmand.maxX.rotation.v2);
						//PrintInfoLog("minY ----------");
						//PrintInfoLog("pos x=" + wcmand.minY.position.v0 + ", y=" + +wcmand.minY.position.v1 + ", z=" + wcmand.minY.position.v2);
						//PrintInfoLog("rot x=" + wcmand.minY.rotation.v0 + ", y=" + +wcmand.minY.rotation.v1 + ", z=" + wcmand.minY.rotation.v2);
						//PrintInfoLog("maxY ----------");
						//PrintInfoLog("pos x=" + wcmand.maxY.position.v0 + ", y=" + +wcmand.maxY.position.v1 + ", z=" + wcmand.maxY.position.v2);
						//PrintInfoLog("rot x=" + wcmand.maxY.rotation.v0 + ", y=" + +wcmand.maxY.rotation.v1 + ", z=" + wcmand.maxY.rotation.v2);

						tmpAnimData = new CtrlerModelAnimNodeData();

						tmpAnimData.btnName = wcmand.name;
						tmpAnimData.btnType = wcmand.type;
						tmpAnimData.alsoBlueEffect = wcmand.blueEffect;

						copyButtonValues(ref tmpAnimData.pressed, wcmand.pressed);
						copyButtonValues(ref tmpAnimData.minX, wcmand.minX);
						copyButtonValues(ref tmpAnimData.maxX, wcmand.maxX);
						copyButtonValues(ref tmpAnimData.minY, wcmand.minY);
						copyButtonValues(ref tmpAnimData.maxY, wcmand.maxY);

						animTable.Add(wcmand.name, tmpAnimData);

						PrintInfoLog("tmpAnimData name=" + tmpAnimData.btnName + ", type=" + tmpAnimData.btnType + ", blueeffect=" + tmpAnimData.alsoBlueEffect);

						PrintInfoLog("pressed pos x=" + tmpAnimData.pressed.position.x + ", y=" + tmpAnimData.pressed.position.y + ", z=" + tmpAnimData.pressed.position.z);
						PrintInfoLog("pressed rot x=" + tmpAnimData.pressed.rotation.x + ", y=" + tmpAnimData.pressed.rotation.y + ", z=" + tmpAnimData.pressed.rotation.z);
						PrintInfoLog("pressed scale x=" + tmpAnimData.pressed.scale.x + ", y=" + tmpAnimData.pressed.scale.y + ", z=" + tmpAnimData.pressed.scale.z);

						if (tmpAnimData.btnType == 3)
						{
							PrintInfoLog("minX pos x=" + tmpAnimData.minX.position.x + ", y=" + tmpAnimData.minX.position.y + ", z=" + tmpAnimData.minX.position.z);
							PrintInfoLog("minX rot x=" + tmpAnimData.minX.rotation.x + ", y=" + tmpAnimData.minX.rotation.y + ", z=" + tmpAnimData.minX.rotation.z);
							PrintInfoLog("minX scale x=" + tmpAnimData.minX.scale.x + ", y=" + tmpAnimData.minX.scale.y + ", z=" + tmpAnimData.minX.scale.z);
							PrintInfoLog("maxX pos x=" + tmpAnimData.maxX.position.x + ", y=" + tmpAnimData.maxX.position.y + ", z=" + tmpAnimData.maxX.position.z);
							PrintInfoLog("maxX rot x=" + tmpAnimData.maxX.rotation.x + ", y=" + tmpAnimData.maxX.rotation.y + ", z=" + tmpAnimData.maxX.rotation.z);
							PrintInfoLog("maxX scale x=" + tmpAnimData.maxX.scale.x + ", y=" + tmpAnimData.maxX.scale.y + ", z=" + tmpAnimData.maxX.scale.z);
							PrintInfoLog("minY pos x=" + tmpAnimData.minY.position.x + ", y=" + tmpAnimData.minY.position.y + ", z=" + tmpAnimData.minY.position.z);
							PrintInfoLog("minY rot x=" + tmpAnimData.minY.rotation.x + ", y=" + tmpAnimData.minY.rotation.y + ", z=" + tmpAnimData.minY.rotation.z);
							PrintInfoLog("minY scale x=" + tmpAnimData.minY.scale.x + ", y=" + tmpAnimData.minY.scale.y + ", z=" + tmpAnimData.minY.scale.z);
							PrintInfoLog("maxY pos x=" + tmpAnimData.maxY.position.x + ", y=" + tmpAnimData.maxY.position.y + ", z=" + tmpAnimData.maxY.position.z);
							PrintInfoLog("maxY rot x=" + tmpAnimData.maxY.rotation.x + ", y=" + tmpAnimData.maxY.rotation.y + ", z=" + tmpAnimData.maxY.rotation.z);
							PrintInfoLog("maxY scale x=" + tmpAnimData.maxY.scale.x + ", y=" + tmpAnimData.maxY.scale.y + ", z=" + tmpAnimData.maxY.scale.z);
						}
					}

					PrintInfoLog("animation table: " + animTable.Count);
				}
			}

			Interop.WVR_ReleaseCtrlerModelAnimNodeData(ref AnimData);

			if (showAnimation) {
				if (HandType == XR_Hand.Dominant)
				{
					PrintDebugLog(HandType + " load Materials/WaveOutlineMatR");
					effectMat = Resources.Load("Materials/WaveOutlineMatR") as Material;
				} else
				{
					PrintDebugLog(HandType + " load Materials/WaveOutlineMatL");
					effectMat = Resources.Load("Materials/WaveOutlineMatL") as Material;
				}
			} else
			{
				if (HandType == XR_Hand.Dominant)
				{
					PrintDebugLog(HandType + " load Materials/WaveColorOffsetMatR");
					effectMat = Resources.Load("Materials/WaveColorOffsetMatR") as Material;
				}
				else
				{
					PrintDebugLog(HandType + " load Materials/WaveColorOffsetMatL");
					effectMat = Resources.Load("Materials/WaveColorOffsetMatL") as Material;
				}
			}

			touchMat = new Material(Shader.Find("Unlit/Texture"));
			if (useSystemConfig)
			{
				PrintInfoLog("use system config in controller model!");
				ReadJsonValues();
			}
			else
			{
				Log.w(LOG_TAG, "use custom config in controller model!");
			}

			var ch = this.transform.childCount;
			PrintDebugLog("childCount: " + ch);
			effectMat.color = buttonEffectColor;

			RenderModel wrm = this.GetComponent<RenderModel>();

			if (wrm != null)
			{
				mergeToOneBone = wrm.mergeToOneBone;
			} else
			{
				Log.w(LOG_TAG, "use one bone mesh!!!!!!!!");
				mergeToOneBone = false;
			}

			isTouchPadSetting = GetTouchPadParam();

			for (int i = 0; i < ch; i++)
			{
				GameObject CM = this.transform.GetChild(i).gameObject;
				string[] t = CM.name.Split("."[0]);
				var childname = t[0];
				if (childname.StartsWith("__CM__"))
				{
					if (effectMat != null)
					{
						PrintInfoLog("Set up effect mat main texture");
						effectMat.mainTexture = CM.GetComponent<MeshRenderer>().material.mainTexture;
						break;
					}
				}
			}

			for (var j = 0; j < clickMeshNames.Length; j++)
			{
				pressObjectArray[j] = new BinaryButtonObject();
				pressObjectArray[j].MeshName = clickMeshNames[j];

				for (int i = 0; i < ch; i++)
				{
					GameObject CM = this.transform.GetChild(i).gameObject;
					string[] t = CM.name.Split("."[0]);
					var childname = t[0];
					if (pressObjectArray[j].MeshName == childname)
					{
						pressObjectArray[j].gameObject = CM;
						pressObjectArray[j].originPosition = CM.transform.localPosition;
						pressObjectArray[j].originMat = CM.GetComponent<MeshRenderer>().material;
						pressObjectArray[j].effectMat = effectMat;
						pressObjectArray[j].hasEffect = true;

						// -------------------------------------------
						pressObjectArray[j].originRotation = CM.transform.localEulerAngles;
						pressObjectArray[j].originScale = CM.transform.localScale;

						if (showAnimation)
						{
							pressObjectArray[j].hasAnimation = false;
							pressObjectArray[j].alsoEffect = false;
							if (animTable.TryGetValue(childname, out tmpAnimData))
							{
								if (tmpAnimData.btnType == 1 || tmpAnimData.btnType == 3)
								{
									pressObjectArray[j].hasAnimation = true;
									pressObjectArray[j].pressPosition = tmpAnimData.pressed.position;
									pressObjectArray[j].pressRotation = tmpAnimData.pressed.rotation;
									pressObjectArray[j].pressScale = tmpAnimData.pressed.scale;
									if (tmpAnimData.alsoBlueEffect == 1) {
										pressObjectArray[j].alsoEffect = true;
									} else
									{
										pressObjectArray[j].alsoEffect = false;
									}
									//PrintInfoLog(childname + " alsoEffect=" + pressObjectArray[j].alsoEffect);
								}
							}
						}

						// -------------------------------------------
						if (childname == "__CM__TouchPad")
						{
							touchpad = pressObjectArray[j].gameObject;
							touchpadMesh = touchpad.GetComponent<MeshFilter>().mesh;
							if (touchpadMesh != null)
							{
								PrintInfoLog("touchpad is found! ");
							}
						}

						PrintInfoLog(pressObjectArray[j].MeshName + " ----- ");
						PrintInfoLog(" localPosistion: " + pressObjectArray[j].originPosition.x + ", " + pressObjectArray[j].originPosition.y + ", " + pressObjectArray[j].originPosition.z);
						PrintInfoLog(" localRotation: " + pressObjectArray[j].originRotation.x + ", " + pressObjectArray[j].originRotation.y + ", " + pressObjectArray[j].originRotation.z);
						PrintInfoLog(" localScale: " + pressObjectArray[j].originScale.x + ", " + pressObjectArray[j].originScale.y + ", " + pressObjectArray[j].originScale.z);

						PrintInfoLog(" pressPosistion: " + pressObjectArray[j].pressPosition.x + ", " + pressObjectArray[j].pressPosition.y + ", " + pressObjectArray[j].pressPosition.z);
						PrintInfoLog(" pressRotation: " + pressObjectArray[j].pressRotation.x + ", " + pressObjectArray[j].pressRotation.y + ", " + pressObjectArray[j].pressRotation.z);
						PrintInfoLog(" pressScale: " + pressObjectArray[j].pressScale.x + ", " + pressObjectArray[j].pressScale.y + ", " + pressObjectArray[j].pressScale.z);

						break;
					}
				}

				PrintInfoLog("Press -> " + pressObjectArray[j].MeshName + " has effect: " + pressObjectArray[j].hasEffect + " has animation: " + pressObjectArray[j].hasAnimation + " also effect: " + pressObjectArray[j].alsoEffect);
			}

			for (var j = 0; j < travel1DMeshNames.Length; j++)
			{
				travel1DObjectArray[j] = new Travel1DObject();
				travel1DObjectArray[j].MeshName = travel1DMeshNames[j];

				for (int i = 0; i < ch; i++)
				{
					GameObject CM = this.transform.GetChild(i).gameObject;
					string[] t = CM.name.Split("."[0]);
					var childname = t[0];
					if (travel1DObjectArray[j].MeshName == childname)
					{
						travel1DObjectArray[j].gameObject = CM;
						travel1DObjectArray[j].originPosition = CM.transform.localPosition;
						travel1DObjectArray[j].originMat = CM.GetComponent<MeshRenderer>().material;
						travel1DObjectArray[j].effectMat = effectMat;
						travel1DObjectArray[j].hasEffect = true;

						// -------------------------------------------
						travel1DObjectArray[j].originRotation = CM.transform.localRotation;
						travel1DObjectArray[j].originScale = CM.transform.localScale;

						if (showAnimation)
						{
							travel1DObjectArray[j].hasAnimation = false;
							if (animTable.TryGetValue(childname, out tmpAnimData))
							{
								if (tmpAnimData.btnType == 2)
								{
									travel1DObjectArray[j].hasAnimation = true;

									travel1DObjectArray[j].pressPosition = tmpAnimData.pressed.position;
									travel1DObjectArray[j].pressRotation = Quaternion.Euler(tmpAnimData.pressed.rotation);
									travel1DObjectArray[j].pressScale = tmpAnimData.pressed.scale;
								}
							}
						}

						var euler = travel1DObjectArray[j].pressRotation.eulerAngles;
						PrintInfoLog(travel1DObjectArray[j].MeshName + " ----- ");
						PrintInfoLog(" localPosistion: " + travel1DObjectArray[j].originPosition.x + ", " + travel1DObjectArray[j].originPosition.y + ", " + travel1DObjectArray[j].originPosition.z);
						PrintInfoLog(" localRotation: " + travel1DObjectArray[j].originRotation.x + ", " + travel1DObjectArray[j].originRotation.y + ", " + travel1DObjectArray[j].originRotation.z);

						PrintInfoLog(" pressPosistion: " + travel1DObjectArray[j].pressPosition.x + ", " + travel1DObjectArray[j].pressPosition.y + ", " + travel1DObjectArray[j].pressPosition.z);
						PrintInfoLog(" pressRotation: " + euler.x + ", " + euler.y + ", " + euler.z);

						break;
					}
				}

				PrintInfoLog("Travel 1D -> " + travel1DObjectArray[j].MeshName + " has effect: " + travel1DObjectArray[j].hasEffect + " has animation: " + travel1DObjectArray[j].hasAnimation);
			}

			for (var j = 0; j < TouchPadMeshNames.Length; j++)
			{
				touchpadObjectArray[j] = new TouchpadObject();
				touchpadObjectArray[j].MeshName = TouchPadMeshNames[j];

				for (int i = 0; i < ch; i++)
				{
					GameObject CM = this.transform.GetChild(i).gameObject;
					string[] t = CM.name.Split("."[0]);
					var childname = t[0];

					if (touchpadObjectArray[j].MeshName == childname)
					{
						touchpadObjectArray[j].gameObject = CM;
						touchpadObjectArray[j].originPosition = CM.transform.localPosition;
						touchpadObjectArray[j].originRotation = CM.transform.localEulerAngles;
						touchpadObjectArray[j].originMat = CM.GetComponent<MeshRenderer>().material;
						touchpadObjectArray[j].effectMat = effectMat;
						touchpadObjectArray[j].hasEffect = true;

						if (childname == "__CM__TouchPad_Touch")
						{
							toucheffectMesh = touchpadObjectArray[j].gameObject.GetComponent<MeshFilter>().mesh;
							if (toucheffectMesh != null)
							{
								PrintInfoLog("toucheffectMesh is found! ");
							}
						}
						break;
					}
				}

				PrintInfoLog("Touchpad -> " + touchpadObjectArray[j].MeshName + " has effect: " + touchpadObjectArray[j].hasEffect);
			}

			for (var j = 0; j < thumbstickMeshNames.Length; j++)
			{
				thumbstickObjectArray[j] = new ThumbstickObject();
				thumbstickObjectArray[j].MeshName = thumbstickMeshNames[j];

				for (int i = 0; i < ch; i++)
				{
					GameObject CM = this.transform.GetChild(i).gameObject;
					string[] t = CM.name.Split("."[0]);
					var childname = t[0];

					if (thumbstickObjectArray[j].MeshName == childname)
					{
						thumbstickObjectArray[j].gameObject = CM;
						thumbstickObjectArray[j].centerPosition = CM.transform.localPosition;
						thumbstickObjectArray[j].centerRotation = CM.transform.localEulerAngles;
						thumbstickObjectArray[j].centerScale = CM.transform.localScale;
						thumbstickObjectArray[j].originMat = CM.GetComponent<MeshRenderer>().material;
						thumbstickObjectArray[j].effectMat = effectMat;

						if (showAnimation)
						{
							thumbstickObjectArray[j].hasAnimation = false;
							if (animTable.TryGetValue(childname, out tmpAnimData))
							{
								if (tmpAnimData.btnType == 3)
								{
									thumbstickObjectArray[j].hasAnimation = true;
									thumbstickObjectArray[j].upPosition = tmpAnimData.minY.position;
									thumbstickObjectArray[j].upRotation = tmpAnimData.minY.rotation;
									thumbstickObjectArray[j].upScale = tmpAnimData.minY.scale;

									thumbstickObjectArray[j].rightPosition = tmpAnimData.maxX.position;
									thumbstickObjectArray[j].rightRotation = tmpAnimData.maxX.rotation;
									thumbstickObjectArray[j].rightScale = tmpAnimData.maxX.scale;
								}
							}
						}

						PrintInfoLog(thumbstickObjectArray[j].MeshName + " ----- ");
						PrintInfoLog(" centerPosition: " + thumbstickObjectArray[j].centerPosition.x + ", " + thumbstickObjectArray[j].centerPosition.y + ", " + thumbstickObjectArray[j].centerPosition.z);
						PrintInfoLog(" centerRotation: " + thumbstickObjectArray[j].centerRotation.x + ", " + thumbstickObjectArray[j].centerRotation.y + ", " + thumbstickObjectArray[j].centerRotation.z);
						PrintInfoLog(" centerScale: " + thumbstickObjectArray[j].centerScale.x + ", " + thumbstickObjectArray[j].centerScale.y + ", " + thumbstickObjectArray[j].centerScale.z);

						PrintInfoLog(" upPosition: " + thumbstickObjectArray[j].upPosition.x + ", " + thumbstickObjectArray[j].upPosition.y + ", " + thumbstickObjectArray[j].upPosition.z);
						PrintInfoLog(" upRotation: " + thumbstickObjectArray[j].upRotation.x + ", " + thumbstickObjectArray[j].upRotation.y + ", " + thumbstickObjectArray[j].upRotation.z);
						PrintInfoLog(" upScale: " + thumbstickObjectArray[j].upScale.x + ", " + thumbstickObjectArray[j].upScale.y + ", " + thumbstickObjectArray[j].upScale.z);

						PrintInfoLog(" rightPosition: " + thumbstickObjectArray[j].rightPosition.x + ", " + thumbstickObjectArray[j].rightPosition.y + ", " + thumbstickObjectArray[j].rightPosition.z);
						PrintInfoLog(" rightRotation: " + thumbstickObjectArray[j].rightRotation.x + ", " + thumbstickObjectArray[j].rightRotation.y + ", " + thumbstickObjectArray[j].rightRotation.z);
						PrintInfoLog(" rightScale: " + thumbstickObjectArray[j].rightScale.x + ", " + thumbstickObjectArray[j].rightScale.y + ", " + thumbstickObjectArray[j].rightScale.z);

						thumbstickObjectArray[j].ptW = (thumbstickObjectArray[j].upPosition - thumbstickObjectArray[j].centerPosition).normalized;
						thumbstickObjectArray[j].ptU = (thumbstickObjectArray[j].rightPosition - thumbstickObjectArray[j].centerPosition).normalized;
						thumbstickObjectArray[j].ptV = Vector3.Cross(thumbstickObjectArray[j].ptU, thumbstickObjectArray[j].ptW).normalized;
						thumbstickObjectArray[j].raidus = (thumbstickObjectArray[j].upPosition - thumbstickObjectArray[j].centerPosition).magnitude;

						PrintInfoLog(" pTW: " + thumbstickObjectArray[j].ptW.x + ", " + thumbstickObjectArray[j].ptW.y + ", " + thumbstickObjectArray[j].ptW.z);
						PrintInfoLog(" pTU: " + thumbstickObjectArray[j].ptU.x + ", " + thumbstickObjectArray[j].ptU.y + ", " + thumbstickObjectArray[j].ptU.z);
						PrintInfoLog(" pTV: " + thumbstickObjectArray[j].ptV.x + ", " + thumbstickObjectArray[j].ptV.y + ", " + thumbstickObjectArray[j].ptV.z);
						PrintInfoLog(" raidus: " + thumbstickObjectArray[j].raidus);

						thumbstickObjectArray[j].rtW = (thumbstickObjectArray[j].upRotation - thumbstickObjectArray[j].centerRotation).normalized;
						thumbstickObjectArray[j].rtU = (thumbstickObjectArray[j].rightRotation - thumbstickObjectArray[j].centerRotation).normalized;
						thumbstickObjectArray[j].rtV = Vector3.Cross(thumbstickObjectArray[j].rtU, thumbstickObjectArray[j].rtW).normalized;

						PrintInfoLog(" rTW: " + thumbstickObjectArray[j].rtW.x + ", " + thumbstickObjectArray[j].rtW.y + ", " + thumbstickObjectArray[j].rtW.z);
						PrintInfoLog(" rTU: " + thumbstickObjectArray[j].rtU.x + ", " + thumbstickObjectArray[j].rtU.y + ", " + thumbstickObjectArray[j].rtU.z);
						PrintInfoLog(" rTV: " + thumbstickObjectArray[j].rtV.x + ", " + thumbstickObjectArray[j].rtV.y + ", " + thumbstickObjectArray[j].rtV.z);

						thumbstickObjectArray[j].xAngle = thumbstickObjectArray[j].upRotation - thumbstickObjectArray[j].centerRotation; // mapping to Y axis
						thumbstickObjectArray[j].zAngle = -(thumbstickObjectArray[j].rightRotation - thumbstickObjectArray[j].centerRotation); // mapping to X axis

						PrintInfoLog(" xAngle: " + thumbstickObjectArray[j].xAngle.x + ", " + thumbstickObjectArray[j].xAngle.y + ", " + thumbstickObjectArray[j].xAngle.z);
						PrintInfoLog(" zAngle: " + thumbstickObjectArray[j].zAngle.x + ", " + thumbstickObjectArray[j].zAngle.y + ", " + thumbstickObjectArray[j].zAngle.z);

						break;
					}
				}

				if (thumbstickObjectArray[j].hasEffect)
					PrintInfoLog("Thumbstick -> " + thumbstickObjectArray[j].MeshName + " has effect: " + thumbstickObjectArray[j].hasEffect + " has animation: " + thumbstickObjectArray[j].hasAnimation);
			}

			resetButtonState();

			// workaround for system button
			for (int i = 0; i < ch; i++)
			{
				GameObject CM = this.transform.GetChild(i).gameObject;
				string[] t = CM.name.Split("."[0]);
				var childname = t[0];
				if (childname.Equals("__CM__HomeButton"))
				{
					PrintInfoLog("system button found!");
					systemButton = CM;
					systemBtnOrigin = CM.GetComponent<MeshRenderer>().material;
					systemAnimation = false;
					if (showAnimation)
					{
						systemAlsoBlueEffect = false;
						if (animTable.TryGetValue(childname, out tmpAnimData))
						{
							if (tmpAnimData.btnType == 1)
							{
								systemAnimation = true;
								systempressPosition = tmpAnimData.pressed.position;
								systempressRotation = tmpAnimData.pressed.rotation;
								systempressScale = tmpAnimData.pressed.scale;
								systemAlsoBlueEffect = (tmpAnimData.alsoBlueEffect == 1);

								systeminitPosition = CM.transform.localPosition;
								systeminitRotation = CM.transform.localEulerAngles;
								systeminitScale = CM.transform.localScale;
							}
						}
					}
				}
			}
			PrintInfoLog("System has animation: " + systemAnimation + ", alsoEffect: " + systemAlsoBlueEffect);

		}

#region OEMConfig
		private Color StringToColor(string color_string)
		{
			float _color_r = (float)Convert.ToInt32(color_string.Substring(1, 2), 16);
			float _color_g = (float)Convert.ToInt32(color_string.Substring(3, 2), 16);
			float _color_b = (float)Convert.ToInt32(color_string.Substring(5, 2), 16);
			float _color_a = (float)Convert.ToInt32(color_string.Substring(7, 2), 16);

			return new Color(_color_r, _color_g, _color_b, _color_a);
		}

		private Texture2D GetTexture2D(string texture_path)
		{
			if (System.IO.File.Exists(texture_path))
			{
				var _bytes = System.IO.File.ReadAllBytes(texture_path);
				var _texture = new Texture2D(1, 1);
				_texture.LoadImage(_bytes);
				return _texture;
			}
			return null;
		}

		public void Circle(Texture2D tex, int cx, int cy, int r, Color col)
		{
			int x, y, px, nx, py, ny, d;

			for (x = 0; x <= r; x++)
			{
				d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
				for (y = 0; y <= d; y++)
				{
					px = cx + x;
					nx = cx - x;
					py = cy + y;
					ny = cy - y;

					tex.SetPixel(px, py, col);
					tex.SetPixel(nx, py, col);

					tex.SetPixel(px, ny, col);
					tex.SetPixel(nx, ny, col);

				}
			}
			tex.Apply();
		}

		private void ReadJsonValues()
		{
			JSON_ModelDesc jmd = OEMConfig.getControllerModelDesc();

			if (jmd != null)
			{
				if (jmd.touchpad_dot_use_texture)
				{
					if (System.IO.File.Exists(jmd.touchpad_dot_texture_name))
					{
						var _texture = GetTexture2D(jmd.touchpad_dot_texture_name);

						PrintInfoLog("touchpad_dot_texture_name: " + jmd.touchpad_dot_texture_name);
						touchMat.mainTexture = _texture;
						touchMat.color = buttonEffectColor;
					}
				} else
				{
					buttonEffectColor = StringToColor(jmd.touchpad_dot_color);
					var texture = new Texture2D(256, 256, TextureFormat.ARGB32, false);
					Color o = Color.clear;
					o.r = 1f;
					o.g = 1f;
					o.b = 1f;
					o.a = 0f;
					for (int i = 0; i < 256; i++)
					{
						for (int j = 0; j < 256; j++)
						{
							texture.SetPixel(i, j, o);
						}
					}
					texture.Apply();

					Circle(texture, 128, 128, 100, buttonEffectColor);

					touchMat.mainTexture = texture;
				}
			}
		}
#endregion
	}
}
