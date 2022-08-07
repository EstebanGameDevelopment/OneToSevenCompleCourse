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
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Wave.Native;
using System;
using System.Collections;
using UnityEngine.XR;
#if UNITY_EDITOR
using Wave.Essence.Editor;
#endif

namespace Wave.Essence.Controller.Model.Demo
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(EventSystem))]
	public sealed class DemoCMGazeInput : PointerInputModule
	{
		const string LOG_TAG = "Wave.Essence.Sample.DemoCMGazeInput";
		private void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}

		#region Public Declaration
		public enum GazeEvent
		{
			Down = 0,
			Submit = 1
		}

		[System.Serializable]
		public class DeviceOption
		{
			public bool HMD = true;
			public bool DominantController = true;
			public bool NonDominantController = true;

			private List<XR_Device> m_OptionList = new List<XR_Device>();
			public List<XR_Device> OptionList
			{
				get
				{
					m_OptionList.Clear();
					if (HMD)
						m_OptionList.Add(XR_Device.Head);
					if (DominantController)
						m_OptionList.Add(XR_Device.Dominant);
					if (NonDominantController)
						m_OptionList.Add(XR_Device.NonDominant);
					return m_OptionList;
				}
			}
		}

		[System.Serializable]
		public class ButtonOption
		{
			public bool primary2DAxisClick = true;
			public bool triggerButton = true;
			public bool secondary2DAxisClick = true;

			private List<InputFeatureUsage<bool>> m_OptionList = new List<InputFeatureUsage<bool>>();
			public List<InputFeatureUsage<bool>> OptionList
			{
				get
				{
					m_OptionList.Clear();
					if (primary2DAxisClick)
						m_OptionList.Add(XR_BinaryButton.primary2DAxisClick);
					if (triggerButton)
						m_OptionList.Add(XR_BinaryButton.triggerButton);
					if (secondary2DAxisClick)
						m_OptionList.Add(XR_BinaryButton.secondary2DAxisClick);
					return m_OptionList;
				}
			}
		}

#if UNITY_EDITOR
		private WVR_InputId WvrButton(InputFeatureUsage<bool> event_button)
		{
			switch (event_button.name)
			{
				case XR_BinaryButton.menuButtonName:
					return WVR_InputId.WVR_InputId_Alias1_Menu;
				case XR_BinaryButton.gripButtonName:
					return WVR_InputId.WVR_InputId_Alias1_Grip;
				case XR_BinaryButton.primary2DAxisClickName:
				case XR_BinaryButton.primary2DAxisTouchName:
					return WVR_InputId.WVR_InputId_Alias1_Touchpad;
				case XR_BinaryButton.triggerButtonName:
					return WVR_InputId.WVR_InputId_Alias1_Trigger;
				case XR_BinaryButton.secondary2DAxisClickName:
				case XR_BinaryButton.secondary2DAxisTouchName:
					return WVR_InputId.WVR_InputId_Alias1_Thumbstick;
				default:
					break;
			}

			return WVR_InputId.WVR_InputId_Alias1_Touchpad;
		}
#endif
		#endregion

		#region Customized Settings
		private bool m_EnableGazeEx = true;
		[Tooltip("Enable the gaze events or not.")]
		[SerializeField]
		private bool m_EnableGaze = true;
		public bool EnableGaze { get { return m_EnableGaze; } set { m_EnableGaze = value; } }

		[Tooltip("Set the event sent if gazed.")]
		[SerializeField]
		private GazeEvent m_InputEvent = GazeEvent.Down;
		public GazeEvent InputEvent { get { return m_InputEvent; } set { m_InputEvent = value; } }

		private bool m_TimerControlDefault = false;
		[Tooltip("A timer will be enabled to trigger gaze events if sets this value.")]
		[SerializeField]
		private bool m_TimerControl = true;
		public bool TimerControl {
			get { return m_TimerControl;
			}
			set {
				if (Log.gpl.Print)
					DEBUG("TimerControl() " + value);
				m_TimerControl = value;
				m_TimerControlDefault = value;
			}
		}

		[Tooltip("Set the timer countdown seconds.")]
		[SerializeField]
		private float m_TimeToGaze = 2.0f;
		public float TimeToGaze { get { return m_TimeToGaze; } set { m_TimeToGaze = value; } }

		[Tooltip("Set to trigger gaze events by buttons.")]
		[SerializeField]
		private bool m_ButtonControl = true;
		public bool ButtonControl { get { return m_ButtonControl; } set { m_ButtonControl = value; } }

		[Tooltip("Set the device type of buttons.")]
		[SerializeField]
		private DeviceOption m_ButtonControlDevices = new DeviceOption();
		public DeviceOption ButtonControlDevices { get { return m_ButtonControlDevices; } set { m_ButtonControlDevices = value; } }

		[Tooltip("Set the buttons to trigger gaze events.")]
		[SerializeField]
		private ButtonOption m_ButtonControlKeys = new ButtonOption();
		public ButtonOption ButtonControlKeys { get { return m_ButtonControlKeys; } set { m_ButtonControlKeys = value; } }

		private List<List<bool>> buttonState = new List<List<bool>>(), preButtonState = new List<List<bool>>();
		#endregion

		private GameObject head = null;
		private Camera m_Camera = null;
		private PhysicsRaycaster physicsRaycaster = null;

		private bool btnPressDown = false;
		private float currUnscaledTime = 0;

		private DemoCMGazePointer gazePointer = null;

		private WVR_GazeTriggerType m_GazeTypeEx = WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout;
		private WVR_GazeTriggerType m_GazeType = WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout;

		#region PointerInputModule overrides. 
		private bool mEnabled = false;
		protected override void OnEnable()
		{
			if (!mEnabled)
			{
				base.OnEnable();

				// 0. Remove the existed StandaloneInputModule.
				Destroy(eventSystem.GetComponent<StandaloneInputModule>());

				// 1. Set up necessary components for Gaze input.
				head = Camera.main.gameObject;
				if (head != null)
				{
					gazePointer = head.GetComponentInChildren<DemoCMGazePointer>();
					if (gazePointer == null)
					{
						GameObject pointer_object = new GameObject("Gaze Pointer");
						pointer_object.transform.SetParent(head.transform, false);
						pointer_object.SetActive(false);
						gazePointer = pointer_object.AddComponent<DemoCMGazePointer>();
						pointer_object.SetActive(true);
						DEBUG("OnEnable() Added gazePointer " + (gazePointer != null ? gazePointer.gameObject.name : "null"));
					}

					m_Camera = gazePointer.gameObject.GetComponent<Camera>();
					DEBUG("OnEnable() Found event camera " + (m_Camera != null ? m_Camera.gameObject.name : "null"));
					physicsRaycaster = gazePointer.gameObject.GetComponent<PhysicsRaycaster>();
					DEBUG("OnEnable() Found physicsRaycaster " + (physicsRaycaster != null ? physicsRaycaster.gameObject.name : "null"));
				}

				// 2. Show the Gaze gazePointer.
				m_EnableGazeEx = m_EnableGaze;
				if (m_EnableGaze)
					ActivateMeshDrawer(true);

				// 3. Initialize the button states.
				ResetButtonStates();

				// 4. Record the initial Gaze control method.
				m_TimerControlDefault = m_TimerControl;
				UpdateGazeType();
				m_GazeTypeEx = m_GazeType;

				mEnabled = true;
			}
		}

		protected override void OnDisable()
		{
			if (mEnabled)
			{
				DEBUG("OnDisable()");
				base.OnDisable();

				ActivateMeshDrawer(false);
				gazePointer = null;

				ExitAllObjects();

				mEnabled = false;
			}
		}

		private bool ValidateParameters()
		{
			if (head == null || m_Camera == null)
				return false;

			if (!ClientInterface.IsFocused)
			{
				ExitAllObjects();
				return false;
			}

			return true;
		}

		private GameObject raycastObject = null, raycastObjectEx = null;
		public override void Process()
		{
			if (m_EnableGazeEx != m_EnableGaze)
			{
				m_EnableGazeEx = m_EnableGaze;
				ActivateMeshDrawer(m_EnableGaze);
			}

			if (m_EnableGaze)
			{
				if (!ValidateParameters())
				{
					gazeTime = Time.unscaledTime;
					return;
				}

				// 1. Timer control or button control.
				GazeControl();

				// 2. Graphic raycast and physics raycast.
				raycastObjectEx = GetRaycastedObject();
				HandleRaycast();
				raycastObject = GetRaycastedObject();
				//if (raycastObject != null && raycastObject == raycastObjectEx)
					//ExecuteEvents.ExecuteHierarchy(raycastObject, pointerData, PointerEvents.pointerHoverHandler);

				// 3. Update the timer & gazePointer state. Send the Gaze event.
				GazeHandling();
			}
		}
		#endregion

		#region Major Standalone Functions
		private void ActivateMeshDrawer(bool active)
		{
			if (gazePointer == null)
				return;

			MeshRenderer mr = gazePointer.gameObject.GetComponentInChildren<MeshRenderer>();
			if (mr != null && mr.enabled != active)
			{
				DEBUG("ActivateMeshDrawer() " + (active ? "Enable " : "Disable ") + "the ring mesh.");
				mr.enabled = active;
			}
			else
			{
				if (Log.gpl.Print)
					Log.e(LOG_TAG, "ActivateMeshDrawer() Oooooooooooops! No MeshRenderer of " + gazePointer.gameObject.name);
			}
		}
		// 1. Timer control or button control.
		private void UpdateGazeType()
		{
			if (m_TimerControl && m_ButtonControl)
				m_GazeType = WVR_GazeTriggerType.WVR_GazeTriggerType_TimeoutButton;
			else if (m_ButtonControl)
				m_GazeType = WVR_GazeTriggerType.WVR_GazeTriggerType_Button;
			else
				m_GazeType = WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout;
		}
		private void GazeControl()
		{
			m_TimerControl = m_TimerControlDefault;
			if (!WXRDevice.IsTracked(XR_Device.Dominant) && !WXRDevice.IsTracked(XR_Device.NonDominant))
				m_TimerControl = true;

			UpdateGazeType();
			if (m_GazeTypeEx != m_GazeType)
			{
				m_GazeTypeEx = m_GazeType;
				DEBUG("GazeControl() m_GazeType: " + m_GazeType);
				Interop.WVR_SetGazeTriggerType(m_GazeType);
			}
		}

		// 2. Graphic raycast and physics raycast.
		private GameObject GetRaycastedObject()
		{
			if (pointerData != null)
				return pointerData.pointerCurrentRaycast.gameObject;

			return null;
		}
		private Vector3 gazeScreenPos2D = Vector2.zero;
		private void HandleRaycast()
		{
			// center of screen
			gazeScreenPos2D.x = 0.5f * Screen.width;
			gazeScreenPos2D.y = 0.5f * Screen.height;
			ResetPointerEventData();

			GraphicRaycast(ref graphicRaycastResults, ref graphicRaycastTargets);
			PhysicsRaycast(ref physicsRaycastResults, ref physicsRaycastTargets);

			EnterExitObjects(graphicRaycastTargets, ref preGraphicRaycastTargets);
			EnterExitObjects(physicsRaycastTargets, ref prePhysicsRaycastTargets);
		}

		// 3. Update the timer & gazePointer state. Send the Gaze event.
		private float gazeTime = 0.0f;
		private void GazeHandling()
		{
			// The gameobject to which raycast positions
			bool interactable = (raycastObject != null);//pointerData.pointerPress != null || ExecuteEvents.GetEventHandler<IPointerClickHandler>(raycastObject) != null;

			bool sendEvent = false;

			currUnscaledTime = Time.unscaledTime;
			if (raycastObjectEx != raycastObject)
			{
				DEBUG("GazeHandling() raycastObjectEx: "
					+ (raycastObjectEx != null ? raycastObjectEx.name : "null")
					+ ", raycastObject: "
					+ (raycastObject != null ? raycastObject.name : "null"));
				if (raycastObject != null)
					gazeTime = currUnscaledTime;
			}
			else
			{
				if (raycastObject != null)
				{
					if (m_TimerControl)
					{
						if (currUnscaledTime - gazeTime > m_TimeToGaze)
						{
							sendEvent = true;
							gazeTime = currUnscaledTime;
						}
						float rate = ((currUnscaledTime - gazeTime) / m_TimeToGaze) * 100;
						if (gazePointer != null)
						{
							gazePointer.RingPercent = interactable ? (int)rate : 0;
						}
					}

					if (m_ButtonControl)
					{
						if (!m_TimerControl)
						{
							if (gazePointer != null)
								gazePointer.RingPercent = 0;
						}

						UpdateButtonStates();
						if (btnPressDown)
						{
							sendEvent = true;
							this.gazeTime = currUnscaledTime;
						}
					}
				}
				else
				{
					if (gazePointer != null)
						gazePointer.RingPercent = 0;
				}
			}

			// Standalone Input Module information
			pointerData.delta = Vector2.zero;
			pointerData.dragging = false;

			DeselectIfSelectionChanged(raycastObject, pointerData);

			if (sendEvent)
			{
				SendPointerEvent(raycastObject);
			}
		} // GazeHandling()
		private void ResetButtonStates()
		{
			buttonState.Clear();
			for (int d = 0; d < m_ButtonControlDevices.OptionList.Count; d++)
			{
				List<bool> dev_list = new List<bool>();
				for (int k = 0; k < m_ButtonControlKeys.OptionList.Count; k++)
				{
					dev_list.Add(false);
				}
				buttonState.Add(dev_list);
			}
			preButtonState.Clear();
			for (int d = 0; d < m_ButtonControlDevices.OptionList.Count; d++)
			{
				List<bool> dev_list = new List<bool>();
				for (int k = 0; k < m_ButtonControlKeys.OptionList.Count; k++)
				{
					dev_list.Add(false);
				}
				preButtonState.Add(dev_list);
			}
		} // ResetButtonStates
		private void UpdateButtonStates()
		{
			btnPressDown = false;

			bool option_changed = false;
			if (buttonState.Count != m_ButtonControlDevices.OptionList.Count)
				option_changed = true;
			else
			{
				for (int i = 0; i < buttonState.Count; i++)
				{
					if (buttonState[i].Count != m_ButtonControlKeys.OptionList.Count)
						option_changed = true;
				}
			}
			if (option_changed)
			{
				DEBUG("UpdateButtonStates() Resets the button states due to button options changed.");
				ResetButtonStates();
				return;
			}

#if UNITY_EDITOR
			if (Application.isEditor)
			{
				for (int d = 0; d < m_ButtonControlDevices.OptionList.Count; d++)
				{
					for (int k = 0; k < m_ButtonControlKeys.OptionList.Count; k++)
					{
						if (WXRDevice.ButtonPress((WVR_DeviceType)m_ButtonControlDevices.OptionList[d],WvrButton(m_ButtonControlKeys.OptionList[k])))
						{
							btnPressDown = true;
							DEBUG("device " + m_ButtonControlDevices.OptionList[d] + ", button " + m_ButtonControlKeys.OptionList[k].name + " pressed.");
							return;
						}
					}
				}
			}
			else
#endif
			{
				for (int d = 0; d < m_ButtonControlDevices.OptionList.Count; d++)
				{
					for (int k = 0; k < m_ButtonControlKeys.OptionList.Count; k++)
					{
						preButtonState[d][k] = buttonState[d][k];
						buttonState[d][k] = WXRDevice.KeyDown(m_ButtonControlDevices.OptionList[d], m_ButtonControlKeys.OptionList[k]);

						if (preButtonState[d][k] == false && buttonState[d][k] == true)
						{
							btnPressDown = true;
							DEBUG("device " + m_ButtonControlDevices.OptionList[d] + ", button " + m_ButtonControlKeys.OptionList[k].name + " pressed.");
							return;
						}
					}
				}
			}
		} // UpdateButtonStates()
		#endregion

		/**
		 * @brief get intersection position in world space
		 **/
		private Vector3 GetIntersectionPosition(Camera cam, RaycastResult raycastResult)
		{
			// Check for camera
			if (cam == null)
			{
				return Vector3.zero;
			}

			float intersectionDistance = raycastResult.distance + cam.nearClipPlane;
			Vector3 intersectionPosition = cam.transform.position + cam.transform.forward * intersectionDistance;
			return intersectionPosition;
		}

		#region Send Events
		private void InvokeButtonClick(GameObject target)
		{
			GameObject click_obj = ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);
			if (click_obj != null)
			{
				if (click_obj.GetComponent<Button>() != null)
				{
					DEBUG("InvokeButtonClick() on " + click_obj.name);
					click_obj.GetComponent<Button>().OnSubmit(pointerData);
				}
			}
		}

		private void SendPointerEvent(GameObject target)
		{
			// PointerClick is equivalent to Button Click.
			//InvokeButtonClick(target);

			if (m_InputEvent == GazeEvent.Down)
			{
				// like "mouse" action, press->release soon, do NOT keep the pointerPressRaycast cause do NOT need to controll "down" object while not gazing.
				pointerData.pressPosition = pointerData.position;
				pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
				pointerData.pointerPress =
					ExecuteEvents.ExecuteHierarchy(target, pointerData, ExecuteEvents.pointerDownHandler)
					?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);

				DEBUG("SendPointerEvent() Send a down event to " + target.name);
				StartCoroutine(PointerUpCoroutine(pointerData.pointerPress));
			}
			else if (m_InputEvent == GazeEvent.Submit)
			{
				DEBUG("SendPointerEvent() Send a submit event to " + target.name);
				ExecuteEvents.ExecuteHierarchy(target, pointerData, ExecuteEvents.submitHandler);
			}
		}

		private IEnumerator PointerUpCoroutine(GameObject target)
		{
			yield return new WaitForSeconds(0.1f);
			if (target != null)
			{
				DEBUG("PointerUpCoroutine() Send up and click events to " + target.name);
				ExecuteEvents.ExecuteHierarchy(target, pointerData, ExecuteEvents.pointerUpHandler);
				ExecuteEvents.ExecuteHierarchy(target, pointerData, ExecuteEvents.pointerClickHandler);
			}
		}

		private void EnterExitObjects(List<GameObject> enterObjects, ref List<GameObject> exitObjects)
		{
			if (exitObjects.Count > 0)
			{
				for (int i = 0; i < exitObjects.Count; i++)
				{
					if (exitObjects[i] != null && !enterObjects.Contains(exitObjects[i]))
					{
						ExecuteEvents.Execute(exitObjects[i], pointerData, ExecuteEvents.pointerExitHandler);
						DEBUG("EnterExitObjects() exit: " + exitObjects[i]);
					}
				}
			}

			if (enterObjects.Count > 0)
			{
				for (int i = 0; i < enterObjects.Count; i++)
				{
					if (enterObjects[i] != null && !exitObjects.Contains(enterObjects[i]))
					{
						ExecuteEvents.Execute(enterObjects[i], pointerData, ExecuteEvents.pointerEnterHandler);
						DEBUG("EnterExitObjects() enter: " + enterObjects[i]);
					}
				}
			}

			CopyList(enterObjects, exitObjects);
		}

		private void ExitAllObjects()
		{
			for (int i = 0; i < prePhysicsRaycastTargets.Count; i++)
			{
				if (prePhysicsRaycastTargets[i] != null)
				{
					ExecuteEvents.Execute(prePhysicsRaycastTargets[i], pointerData, ExecuteEvents.pointerExitHandler);
					DEBUG("ExitAllObjects() exit: " + prePhysicsRaycastTargets[i]);
				}
			}

			prePhysicsRaycastTargets.Clear();

			for (int i = 0; i < preGraphicRaycastTargets.Count; i++)
			{
				if (preGraphicRaycastTargets[i] != null)
				{
					ExecuteEvents.Execute(preGraphicRaycastTargets[i], pointerData, ExecuteEvents.pointerExitHandler);
					DEBUG("ExitAllObjects() exit: " + preGraphicRaycastTargets[i]);
				}
			}

			preGraphicRaycastTargets.Clear();
		}
		#endregion

		#region Raycast
		private PointerEventData pointerData = null;
		private RaycastResult firstRaycastResult = new RaycastResult();
		private void ResetPointerEventData()
		{
			if (pointerData == null)
			{
				pointerData = new PointerEventData(eventSystem);
				pointerData.pointerCurrentRaycast = new RaycastResult();
			}

			pointerData.Reset();
			pointerData.position = gazeScreenPos2D;
			firstRaycastResult.Clear();
			pointerData.pointerCurrentRaycast = firstRaycastResult;
		}

		private void GetResultList(List<RaycastResult> originList, List<RaycastResult> targetList)
		{
			targetList.Clear();
			for (int i = 0; i < originList.Count; i++)
			{
				if (originList[i].gameObject != null)
					targetList.Add(originList[i]);
			}
		}

		private RaycastResult SelectRaycastResult(RaycastResult currResult, RaycastResult nextResult)
		{
			if (currResult.gameObject == null)
				return nextResult;
			if (nextResult.gameObject == null)
				return currResult;

			if (currResult.worldPosition == Vector3.zero)
				currResult.worldPosition = GetIntersectionPosition(currResult.module.eventCamera, currResult);

			float curr_distance = (float)Math.Round(Mathf.Abs(currResult.worldPosition.z - currResult.module.eventCamera.transform.position.z), 3);

			if (nextResult.worldPosition == Vector3.zero)
				nextResult.worldPosition = GetIntersectionPosition(nextResult.module.eventCamera, nextResult);

			float next_distance = (float)Math.Round(Mathf.Abs(nextResult.worldPosition.z - currResult.module.eventCamera.transform.position.z), 3);

			// 1. Check the distance.
			if (next_distance > curr_distance)
				return currResult;

			if (next_distance < curr_distance)
			{
				DEBUG("SelectRaycastResult() "
					+ nextResult.gameObject.name + ", position: " + nextResult.worldPosition
					+ ", distance: " + next_distance
					+ " is smaller than "
					+ currResult.gameObject.name + ", position: " + currResult.worldPosition
					+ ", distance: " + curr_distance
					);

				return nextResult;
			}

			// 2. Check the "Order in Layer" of the Canvas.
			if (nextResult.sortingOrder > currResult.sortingOrder)
				return nextResult;

			return currResult;
		}

		private RaycastResult m_Result = new RaycastResult();
		private RaycastResult FindFirstResult(List<RaycastResult> resultList)
		{
			m_Result = resultList[0];
			for (int i = 1; i < resultList.Count; i++)
				m_Result = SelectRaycastResult(m_Result, resultList[i]);
			return m_Result;
		}

		List<RaycastResult> graphicRaycastResults = new List<RaycastResult>();
		List<GameObject> graphicRaycastTargets = new List<GameObject>(), preGraphicRaycastTargets = new List<GameObject>();
		List<RaycastResult> graphicResultList = new List<RaycastResult>();
		private GameObject raycastTarget = null;
		private void GraphicRaycast(ref List<RaycastResult> raycastResults, ref List<GameObject> raycastObjects)
		{
			Profiler.BeginSample("Find GraphicRaycaster for Gaze.");
			GraphicRaycaster[] graphic_raycasters = GameObject.FindObjectsOfType<GraphicRaycaster>();
			Profiler.EndSample();

			raycastResults.Clear();
			raycastObjects.Clear();

			for (int i = 0; i < graphic_raycasters.Length; i++)
			{
				if (graphic_raycasters[i].gameObject != null && graphic_raycasters[i].gameObject.GetComponent<Canvas>() != null)
					graphic_raycasters[i].gameObject.GetComponent<Canvas>().worldCamera = m_Camera;
				else
					continue;

				// 1. Get the raycast results list.
				graphic_raycasters[i].Raycast(pointerData, raycastResults);
				GetResultList(raycastResults, graphicResultList);
				if (graphicResultList.Count == 0)
					continue;

				// 2. Get the raycast objects list.
				firstRaycastResult = FindFirstResult(graphicResultList);

				//DEBUG ("GraphicRaycast() device: " + event_controller.device + ", camera: " + firstRaycastResult.module.m_Camera + ", first result = " + firstRaycastResult);
				pointerData.pointerCurrentRaycast = SelectRaycastResult(pointerData.pointerCurrentRaycast, firstRaycastResult);
				raycastResults.Clear();
			} // for (int i = 0; i < graphic_raycasters.Length; i++)

			raycastTarget = pointerData.pointerCurrentRaycast.gameObject;
			while (raycastTarget != null)
			{
				raycastObjects.Add(raycastTarget);
				raycastTarget = (raycastTarget.transform.parent != null ? raycastTarget.transform.parent.gameObject : null);
			}
		}

		List<RaycastResult> physicsRaycastResults = new List<RaycastResult>();
		List<GameObject> physicsRaycastTargets = new List<GameObject>(), prePhysicsRaycastTargets = new List<GameObject>();
		List<RaycastResult> physicsResultList = new List<RaycastResult>();
		private void PhysicsRaycast(ref List<RaycastResult> raycastResults, ref List<GameObject> raycastObjects)
		{
			raycastResults.Clear();
			raycastObjects.Clear();

			Profiler.BeginSample("PhysicsRaycaster.Raycast() Gaze.");
			physicsRaycaster.Raycast(pointerData, raycastResults);
			Profiler.EndSample();

			GetResultList(raycastResults, physicsResultList);
			if (physicsResultList.Count == 0)
				return;

			firstRaycastResult = FindFirstResult(physicsResultList);

			//if (firstRaycastResult.module != null)
				//DEBUG ("PhysicsRaycast() camera: " + firstRaycastResult.module.m_Camera + ", first result = " + firstRaycastResult);
			pointerData.pointerCurrentRaycast = SelectRaycastResult(pointerData.pointerCurrentRaycast, firstRaycastResult);

			raycastTarget = pointerData.pointerCurrentRaycast.gameObject;
			while (raycastTarget != null)
			{
				raycastObjects.Add(raycastTarget);
				raycastTarget = (raycastTarget.transform.parent != null ? raycastTarget.transform.parent.gameObject : null);
			}
		}
		#endregion

		private void CopyList(List<GameObject> src, List<GameObject> dst)
		{
			dst.Clear();
			for (int i = 0; i < src.Count; i++)
				dst.Add(src[i]);
		}
	}
}
