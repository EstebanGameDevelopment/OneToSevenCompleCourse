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
using UnityEngine.XR;
#if UNITY_EDITOR
using Wave.Essence.Editor;
#endif

namespace Wave.Essence.InputModule
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(EventSystem))]
	public sealed class ControllerInputModule : PointerInputModule
	{
		const string LOG_TAG = "Wave.Essence.InputModule.ControllerInputModule";
		private void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}
		private void INFO(string msg) { Log.i(LOG_TAG, msg, true); }

		#region Public Declaration
		public enum BeamModes { Flexible, Fixed, Mouse }

		[System.Serializable]
		public class ButtonOption
		{
			public bool primary2DAxisClick = true;
			public bool triggerButton = true;
			public bool secondary2DAxisClick = false;

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
		private BeamModes m_BeamModeEx = BeamModes.Mouse;
		[Tooltip("There are 3 modes of different beam types.")]
		[SerializeField]
		private BeamModes m_BeamMode = BeamModes.Mouse;
		public BeamModes BeamMode { get { return m_BeamMode; } set { m_BeamMode = value; } }

		// If drag is prior, the click event will NOT be sent after dragging.
		private bool m_PriorDrag = false;
		public bool PriorDrag { get { return m_PriorDrag; } set { m_PriorDrag = value; } }

		[Tooltip("Select to enable events of Dominant controller.")]
		[SerializeField]
		private bool m_DominantEvent = true;
		public bool DominantEvent { get { return m_DominantEvent; } set { m_DominantEvent = value; } }

		[Tooltip("Set the PhysicsRaycaster eventMask of Dominant controller.")]
		[SerializeField]
		private LayerMask m_DominantRaycastMask = ~0;
		public LayerMask DominantRaycastMask { get { return m_DominantRaycastMask; } set { m_DominantRaycastMask = value; } }

		[Tooltip("Select to enable events of NonDominant controller.")]
		[SerializeField]
		private bool m_NonDominantEvent = true;
		public bool NonDominantEvent { get { return m_NonDominantEvent; } set { m_NonDominantEvent = value; } }

		[Tooltip("Set the PhysicsRaycaster eventMask of NonDominant controller.")]
		[SerializeField]
		private LayerMask m_NonDominantRaycastMask = ~0;
		public LayerMask NonDominantRaycastMask { get { return m_NonDominantRaycastMask; } set { m_NonDominantRaycastMask = value; } }

		[Tooltip("Choose the buttons to trigger events.")]
		[SerializeField]
		private ButtonOption m_ButtonToTrigger = new ButtonOption();
		public ButtonOption ButtonToTrigger { get { return m_ButtonToTrigger; } set { m_ButtonToTrigger = value; } }
		private List<bool> buttonState = new List<bool>(), preButtonState = new List<bool>();

		[Tooltip("Set the beam length in Fixed Beam Mode.")]
		[SerializeField]
		private float m_FixedBeamLength = 50;
		public float FixedBeamLength { get { return m_FixedBeamLength; } set { m_FixedBeamLength = value; } }

		[Tooltip("Ignore the interaction mode.")]
		[SerializeField]
		private bool m_IgnoreMode = false;
		public bool IgnoreMode { get { return m_IgnoreMode; } set { m_IgnoreMode = value; } }

		[HideInInspector]
		public GameObject DominantController = null;

		[HideInInspector]
		public GameObject NonDominantController = null;
		#endregion

		private GameObject head = null;

		// Do NOT allow event DOWN being sent multiple times during kClickInterval
		// since UI element of Unity needs time to perform transitions.
		const float kClickInterval = 0.2f;
		// After selecting an object over this duration, the drag action will be taken.
		const float kTimeToDrag = 0.2f;
		// The beam end offset + this distance = the pointer distance.
		const float kBeamToPointerDistance = 0.5f;

		private Dictionary<XR_Hand, ControllerPointerTracker> s_PointerTrackerMouse = new Dictionary<XR_Hand, ControllerPointerTracker>() {
			{ XR_Hand.Dominant, null },
			{ XR_Hand.NonDominant, null },
		};

		private bool toUpdateBeam = true;
		private bool toUpdatePointer = true;

		#region Beam Configuration
		[System.Serializable]
		class BeamConfig
		{
			public float StartWidth;
			public float EndWidth;
			public float StartOffset;
			public float EndOffset;
			public Color32 StartColor;
			public Color32 EndColor;

			public BeamConfig() { }
			public BeamConfig(BeamConfig src)
			{
				StartWidth = src.StartWidth;
				EndWidth = src.EndWidth;
				StartOffset = src.StartOffset;
				EndOffset = src.EndOffset;
				StartColor = src.StartColor;
				EndColor = src.EndColor;
			}
			public BeamConfig(float startWidth, float endWidth, float startOffset, float endOffset, Color32 startColor, Color32 endColor)
			{
				StartWidth = startWidth;
				EndWidth = endWidth;
				StartOffset = startOffset;
				EndOffset = endOffset;
				StartColor = startColor;
				EndColor = endColor;
			}

			public void copyFrom(BeamConfig src)
			{
				StartWidth = src.StartWidth;
				EndWidth = src.EndWidth;
				StartOffset = src.StartOffset;
				EndOffset = src.EndOffset;
				StartColor = src.StartColor;
				EndColor = src.EndColor;
			}
		}

		private static BeamConfig flexibleBeamConfig = new BeamConfig
		{
			StartWidth = 0.000625f,
			EndWidth = 0.00125f,
			StartOffset = 0.015f,
			EndOffset = 1.2f,
			StartColor = new Color32(255, 255, 255, 255),
			EndColor = new Color32(255, 255, 255, 0)
		};
		private static BeamConfig fixedBeamConfig = new BeamConfig
		{
			StartWidth = 0.000625f,
			EndWidth = 0.00125f,
			StartOffset = 0.015f,
			EndOffset = 50,
			StartColor = new Color32(255, 255, 255, 255),
			EndColor = new Color32(255, 255, 255, 255)
		};
		private static BeamConfig mouseBeamConfig = new BeamConfig
		{
			StartWidth = 0.000625f,
			EndWidth = 0.00125f,
			StartOffset = 0.015f,
			EndOffset = 1.2f,
			StartColor = new Color32(255, 255, 255, 255),
			EndColor = new Color32(255, 255, 255, 77)
		};

		class BeamModeSetting
		{
			public BeamModes Mode { get; set; }
			public BeamConfig Config { get; set; }

			public BeamModeSetting(BeamModes mode, BeamConfig config)
			{
				this.Mode = mode;
				this.Config = new BeamConfig(config);
			}
		}
		#endregion

		#region Event Controller Handling
		private List<EventController> m_EventControllers = new List<EventController>();
		class EventController
		{
			public XR_Hand device;
			public GameObject model;
			public GameObject raycastObjectEx;
			public PointerEventData eventData;
			public ControllerPointer pointer;
			public bool pointerEnabled;
			public ControllerBeam beam;
			public bool beamEnabled;

			private List<BeamModeSetting> raycastModeSettings;
			public void SetBeamConfig(BeamModes mode, BeamConfig config)
			{
				for (int i = 0; i < raycastModeSettings.Count; i++)
				{
					if (raycastModeSettings[i].Mode == mode)
					{
						raycastModeSettings[i].Config.copyFrom(config);
					}
				}
			}
			public BeamConfig GetBeamConfig(BeamModes mode)
			{
				for (int i = 0; i < raycastModeSettings.Count; i++)
				{
					if (raycastModeSettings[i].Mode == mode)
						return raycastModeSettings[i].Config;
				}
				return null;
			}

			public EventController(XR_Hand type)
			{
				device = type;
				model = null;
				raycastObjectEx = null;
				eventData = new PointerEventData(EventSystem.current);
				beam = null;
				beamEnabled = false;
				pointer = null;
				pointerEnabled = false;
				raycastModeSettings = new List<BeamModeSetting>();
				raycastModeSettings.Add(new BeamModeSetting(BeamModes.Flexible, flexibleBeamConfig));
				raycastModeSettings.Add(new BeamModeSetting(BeamModes.Fixed, fixedBeamConfig));
				raycastModeSettings.Add(new BeamModeSetting(BeamModes.Mouse, mouseBeamConfig));
			}
		}

		private EventController GetEventController(XR_Hand dt)
		{
			for (int i = 0; i < m_EventControllers.Count; i++)
			{
				if (m_EventControllers[i].device == dt)
					return m_EventControllers[i];
			}
			return null;
		}
		#endregion

		#region Event Controller Components Update
		private void UpdateControllerModelInProcess()
		{
			for (int i = 0; i < EventControllerProvider.ControllerTypes.Length; i++)
			{
				XR_Hand dev_type = EventControllerProvider.ControllerTypes[i];
				EventController event_controller = GetEventController(dev_type);

				GameObject origin_model = event_controller.model;
				GameObject new_model = EventControllerProvider.Instance.GetEventController(dev_type);
				LayerMask layer_mask = ~0;
				if (dev_type == XR_Hand.Dominant)
					layer_mask = m_DominantRaycastMask;
				if (dev_type == XR_Hand.NonDominant)
					layer_mask = m_NonDominantRaycastMask;

				if (origin_model == null)
				{
					if (new_model != null)
					{
						// replace with new controller instance.
						DEBUG("UpdateControllerModelInProcess() " + dev_type + ", replace null with new controller instance.");
						SetupEventController(event_controller, new_model, layer_mask);
					}
				}
				else
				{
					if (new_model == null)
					{
						DEBUG("UpdateControllerModelInProcess() " + dev_type + ", clear controller instance.");
						SetupEventController(event_controller, null, layer_mask);
					}
					else
					{
						if (!ReferenceEquals(origin_model, new_model))
						{
							// replace with new controller instance.
							DEBUG("UpdateControllerModelInProcess() " + dev_type + ", set new controller instance.");
							SetupEventController(event_controller, new_model, layer_mask);
						}
					}
				}
			}
		}

		private void SetupEventController(EventController eventController, GameObject controller_model, LayerMask mask)
		{
			// Deactivate the old model.
			if (eventController.model != null)
			{
				DEBUG("SetupEventController() deactivate " + eventController.model.name);
				eventController.model.SetActive(false);
			}

			// Replace with a new model.
			eventController.model = controller_model;

			// Activate the new model.
			// Note: must setup beam first.
			if (eventController.model != null)
			{
				DEBUG("SetupEventController() activate " + eventController.model.name);
				eventController.model.SetActive(true);

				// Set up PhysicsRaycaster.
				PhysicsRaycaster phy_raycaster = eventController.model.GetComponent<PhysicsRaycaster>();
				if (phy_raycaster == null)
					phy_raycaster = eventController.model.AddComponent<PhysicsRaycaster>();
				phy_raycaster.eventMask = mask;
				DEBUG("SetupEventController() PhysicsRaycaster eventMask: " + phy_raycaster.eventMask.value);

				// Get the model beam.
				eventController.beam = eventController.model.GetComponentInChildren<ControllerBeam>(true);
				if (eventController.beam != null)
				{
					DEBUG("SetupEventController() set up ControllerBeam: " + eventController.beam.gameObject.name + " of " + eventController.device);
					SetupEventControllerBeam(eventController, Vector3.zero, true);
				}

				// Get the model pointer.
				eventController.pointer = eventController.model.GetComponentInChildren<ControllerPointer>(true);
				if (eventController.pointer != null)
				{
					DEBUG("SetupEventController() set up ControllerPointer: " + eventController.pointer.gameObject.name + " of " + eventController.device);
					SetupEventControllerPointer(eventController);
				}
			}
		}

		private void SetupEventControllerBeam(EventController eventController, Vector3 intersectionPosition, bool updateRaycastConfig = false)
		{
			if (eventController == null || eventController.beam == null)
				return;

			BeamConfig beam_config = eventController.GetBeamConfig(m_BeamMode);
			if (updateRaycastConfig)
			{
				beam_config.StartWidth = eventController.beam.StartWidth;
				beam_config.EndWidth = eventController.beam.EndWidth;
				beam_config.StartOffset = eventController.beam.StartOffset;
				beam_config.StartColor = eventController.beam.StartColor;
				beam_config.EndColor = eventController.beam.EndColor;

				switch (m_BeamMode)
				{
					case BeamModes.Flexible:
					case BeamModes.Mouse:
						beam_config.EndOffset = eventController.beam.EndOffset;
						break;
					case BeamModes.Fixed:
						beam_config.EndOffset = m_FixedBeamLength;
						break;
					default:
						break;
				}
				eventController.SetBeamConfig(m_BeamMode, beam_config);

				DEBUG("SetupEventControllerBeam() " + eventController.device + ", " + m_BeamMode + " mode config - "
					+ "StartWidth: " + beam_config.StartWidth
					+ ", EndWidth: " + beam_config.EndWidth
					+ ", StartOffset: " + beam_config.StartOffset
					+ ", EndOffset: " + beam_config.EndOffset
					+ ", StartColor: " + beam_config.StartColor.ToString()
					+ ", EndColor: " + beam_config.EndColor.ToString()
				);
			}

			if (m_BeamMode != m_BeamModeEx || toUpdateBeam)
			{
				eventController.beam.StartWidth = beam_config.StartWidth;
				eventController.beam.EndWidth = beam_config.EndWidth;
				eventController.beam.StartOffset = beam_config.StartOffset;
				eventController.beam.EndOffset = beam_config.EndOffset;
				eventController.beam.StartColor = beam_config.StartColor;
				eventController.beam.EndColor = beam_config.EndColor;

				toUpdateBeam = false;

				DEBUG("SetupEventControllerBeam() " + eventController.device + ", " + m_BeamMode + " mode"
					+ ", StartWidth: " + eventController.beam.StartWidth
					+ ", EndWidth: " + eventController.beam.EndWidth
					+ ", StartOffset: " + eventController.beam.StartOffset
					+ ", length: " + eventController.beam.EndOffset
					+ ", StartColor: " + eventController.beam.StartColor.ToString()
					+ ", EndColor: " + eventController.beam.EndColor.ToString());
			}

			if (m_BeamMode == BeamModes.Flexible)
			{
				GameObject curr_raycasted_obj = GetRaycastedObject(eventController.device);
				if (curr_raycasted_obj != null)
					eventController.beam.OnPointerEnter(curr_raycasted_obj, intersectionPosition, true);
				else
				{
					if (curr_raycasted_obj != eventController.raycastObjectEx)
						eventController.beam.OnPointerExit(eventController.raycastObjectEx);
				}
			}
		}

		private void SetupEventControllerPointer(EventController eventController)
		{
			if (eventController.pointer == null)
				return;

			SetupEventControllerPointer(eventController, Vector3.zero);
		}

		private void SetupEventControllerPointer(EventController eventController, Vector3 intersectionPosition)
		{
			if (eventController == null || eventController.pointer == null)
				return;

			float pointerDistanceInMeters = 0;
			if (m_BeamMode != m_BeamModeEx || toUpdatePointer)
			{
				switch (m_BeamMode)
				{
					case BeamModes.Flexible:
					case BeamModes.Mouse:
						if (eventController.beam != null)
							pointerDistanceInMeters = eventController.beam.EndOffset + kBeamToPointerDistance;// eventController.beam.endOffsetMin;
						else
							pointerDistanceInMeters = mouseBeamConfig.EndOffset + kBeamToPointerDistance;

						eventController.pointer.PointerDistanceInMeters = pointerDistanceInMeters;
						break;
					default:
						break;
				}

				toUpdatePointer = false;

				DEBUG("SetupEventControllerPointer() " + eventController.device + ", " + m_BeamMode + " mode"
					+ ", pointerDistanceInMeters: " + pointerDistanceInMeters);
			}

			if (m_BeamMode != BeamModes.Fixed)
			{
				GameObject curr_raycasted_obj = GetRaycastedObject(eventController.device);
				if (curr_raycasted_obj != null)
					eventController.pointer.OnPointerEnter(curr_raycasted_obj, intersectionPosition, (m_BeamMode == BeamModes.Flexible));
				else
				{
					if (curr_raycasted_obj != eventController.raycastObjectEx)
						eventController.pointer.OnPointerExit(eventController.raycastObjectEx);
				}
			}
		}

		private void CheckBeamPointerActive(EventController eventController)
		{
			if (eventController == null)
				return;

			if (eventController.beam != null)
			{
				bool enabled = eventController.beam.gameObject.activeSelf && eventController.beam.ShowBeam;
				if (eventController.beamEnabled != enabled)
				{
					eventController.beamEnabled = enabled;
					toUpdateBeam = eventController.beamEnabled;
					DEBUG("CheckBeamPointerActive() " + eventController.device + ", beam is " + (eventController.beamEnabled ? "active." : "inactive."));
				}
			}
			else
			{
				eventController.beamEnabled = false;
			}

			if (eventController.pointer != null)
			{
				bool enabled = eventController.pointer.gameObject.activeSelf && eventController.pointer.ShowPointer;
				if (eventController.pointerEnabled != enabled)
				{
					eventController.pointerEnabled = enabled;
					toUpdatePointer = eventController.pointerEnabled;
					DEBUG("CheckBeamPointerActive() " + eventController.device + ", pointer is " + (eventController.pointerEnabled ? "active." : "inactive."));
				}
			}
			else
			{
				eventController.pointerEnabled = false;
			}
		}

		public void ChangeBeamLength(XR_Hand dt, float length)
		{
			EventController event_controller = GetEventController(dt);
			if (event_controller == null)
				return;

			if (event_controller.beam != null)
			{
				if (m_BeamMode == BeamModes.Fixed || m_BeamMode == BeamModes.Mouse)
					event_controller.beam.EndOffset = length;
			}

			toUpdateBeam = true;
			toUpdatePointer = true;
			SetupEventControllerBeam(event_controller, Vector3.zero, true);
			SetupEventControllerPointer(event_controller);
		}
		#endregion

		#region BaseInputModule Overrides
		private bool mEnabled = false;
		protected override void OnEnable()
		{
			if (!mEnabled)
			{
				base.OnEnable();
				INFO("OnEnable()");

				// 0. Disable the existed StandaloneInputModule.
				Destroy(eventSystem.GetComponent<StandaloneInputModule>());

				// 1. Initialize the necessary components.
				for (int i = 0; i < EventControllerProvider.ControllerTypes.Length; i++)
				{
					m_EventControllers.Add(new EventController(EventControllerProvider.ControllerTypes[i]));
				}

				head = Camera.main.gameObject;
				if (head != null)
				{
					ControllerPointerTracker[] trackers = head.GetComponentsInChildren<ControllerPointerTracker>();
					foreach (ControllerPointerTracker tracker in trackers)
						s_PointerTrackerMouse[tracker.TrackerType] = tracker;

					if (s_PointerTrackerMouse[XR_Hand.Dominant] == null)
						CreatePointerTracker(XR_Hand.Dominant);
					Log.i(LOG_TAG, "OnEnable() Dominant pointer tracker: " + s_PointerTrackerMouse[XR_Hand.Dominant].gameObject.name, true);

					if (s_PointerTrackerMouse[XR_Hand.NonDominant] == null)
						CreatePointerTracker(XR_Hand.NonDominant);
					Log.i(LOG_TAG, "OnEnable() NonDominant pointer tracker: " + s_PointerTrackerMouse[XR_Hand.NonDominant].gameObject.name, true);
				}
				else
				{
					Log.w(LOG_TAG, "OnEnable() Please set up the Main Camera.", true);
				}

				// 2. Initialize the button states.
				ResetButtonStates();

				// 3. Record the initial Controller raycast mode.
				m_BeamModeEx = m_BeamMode;

				// 4. Check the ControllerInputSwitch.
				if (ControllerInputSwitch.Instance != null)
					Log.i(LOG_TAG, "OnEnable() Loaded ControllerInputSwitch.");

				mEnabled = true;
			}
		}

		protected override void OnDisable()
		{
			if (mEnabled)
			{
				base.OnDisable();
				DEBUG("OnDisable()");

				for (int i = 0; i < EventControllerProvider.ControllerTypes.Length; i++)
				{
					XR_Hand dev_type = EventControllerProvider.ControllerTypes[i];
					EventController event_controller = GetEventController(dev_type);
					if (event_controller != null)
					{
						ExitAllObjects(event_controller);
					}

					ActivateBeamPointer(dev_type, false);
				}

				m_EventControllers.Clear();

				mEnabled = false;
			}
		}

		public override void Process()
		{
			if (!mEnabled)
				return;

			UpdateControllerModelInProcess();

			// Handle the raycast before updating raycast mode.
			HandleRaycast();
			m_BeamModeEx = m_BeamMode;
		}
		#endregion

		#region Major Standalone Functions
		private bool IsControllerInteractable(XR_Hand hand)
		{
			XR_Hand another_hand = (hand == XR_Hand.Dominant ? XR_Hand.NonDominant : XR_Hand.Dominant);

			bool interactable = false;

			bool enable_event = (hand == XR_Hand.Dominant ? m_DominantEvent : m_NonDominantEvent);
			bool focused = ClientInterface.IsFocused;
			bool is_tracked = WXRDevice.IsTracked((XR_Device)hand);
			bool controller_mode = (m_IgnoreMode || (ClientInterface.InteractionMode == XR_InteractionMode.Controller));
			bool primary_input = (
				(!ControllerInputSwitch.Instance.SingleInput) ||
				(hand == ControllerInputSwitch.Instance.PrimaryInput) ||
				(EventControllerProvider.Instance.GetEventController(another_hand) == null));
			// Ignore the Main Camera case.

			if (Log.gpl.Print)
			{
				DEBUG("IsControllerInteractable() " + hand +
					", enable_event: " + enable_event +
					", focused: " + focused +
					", is_tracked: " + is_tracked +
					", m_IgnoreMode: " + m_IgnoreMode +
					", interaction mode: " + ClientInterface.InteractionMode +
					", primary_input: " + primary_input
					);
			}

			interactable = enable_event && focused && is_tracked && controller_mode && primary_input;

			ActivateBeamPointer(hand, interactable);

			return interactable;
		}

		private void ActivateBeamPointer(XR_Hand hand, bool active)
		{
			EventController event_controller = GetEventController(hand);
			if (event_controller == null)
				return;

			if (!active)
			{
				if (event_controller.beam != null)
					event_controller.beam.ShowBeam = false;
				if (event_controller.pointer != null)
					event_controller.pointer.ShowPointer = false;
			}
			else
			{
				if (event_controller.beam != null)
					event_controller.beam.ShowBeam = true;
				if (event_controller.pointer != null)
					event_controller.pointer.ShowPointer = (m_BeamMode != BeamModes.Fixed);
			}
		}

		/// <summary> Get the intersection position in world space </summary>
		private Vector3 GetIntersectionPosition(Camera cam, RaycastResult raycastResult)
		{
			if (cam == null)
				return Vector3.zero;

			float intersectionDistance = raycastResult.distance + cam.nearClipPlane;
			Vector3 intersectionPosition = cam.transform.forward * intersectionDistance + cam.transform.position;
			return intersectionPosition;
		}

		private GameObject GetRaycastedObject(XR_Hand type)
		{
			EventController event_controller = GetEventController(type);
			if (event_controller != null && event_controller.eventData != null)
				return event_controller.eventData.pointerCurrentRaycast.gameObject;
			return null;
		}

		private void CreatePointerTracker(XR_Hand hand)
		{
			if (head == null)
			{
				DEBUG("CreatePointerTracker() no head!!");
				return;
			}

			// 1. Create a pointer tracker gameObject and attach to the head.
			var pt = new GameObject(hand + "PointerCamera");
			pt.transform.SetParent(head.transform, false);
			pt.transform.localPosition = Vector3.zero;
			DEBUG("CreatePointerTracker() " + hand + " sets pointer tracker parent to " + pt.transform.parent.name);

			// 2. Add the ControllerPointerTracker component.
			pt.SetActive(false);
			s_PointerTrackerMouse[hand] = pt.AddComponent<ControllerPointerTracker>();
			s_PointerTrackerMouse[hand].TrackerType = hand;
			pt.SetActive(true);
			DEBUG("CreatePointerTracker() " + hand + " sets pointer tracker type to " + s_PointerTrackerMouse[hand].TrackerType);

			// 3. Set the pointer tracker PhysicsRaycaster eventMask.
			PhysicsRaycaster phy_raycaster = pt.GetComponent<PhysicsRaycaster>();
			if (phy_raycaster != null)
			{
				phy_raycaster.eventMask = (hand == XR_Hand.Dominant ? m_DominantRaycastMask : m_NonDominantRaycastMask);
				DEBUG("CreatePointerTracker() " + hand + " sets physics raycast mask to " + phy_raycaster.eventMask.value);
			}
		}

		private void ResetButtonStates()
		{
			buttonState.Clear();
			preButtonState.Clear();
			for (int i = 0; i < m_ButtonToTrigger.OptionList.Count; i++)
			{
				buttonState.Add(false);
				preButtonState.Add(false);
			}
		}

		bool btnPressDown = false, btnPressed = false;
		private void UpdateButtonStates(XR_Device device)
		{
			btnPressDown = false;
			btnPressed = false;

			if (buttonState.Count != m_ButtonToTrigger.OptionList.Count)
			{
				ResetButtonStates();
				return;
			}

#if UNITY_EDITOR
			if (Application.isEditor)
			{
				for (int b = 0; b < m_ButtonToTrigger.OptionList.Count; b++)
				{
					btnPressDown |= WXRDevice.ButtonPress((WVR_DeviceType)device, WvrButton(m_ButtonToTrigger.OptionList[b]));
					btnPressed |= WXRDevice.ButtonHold((WVR_DeviceType)device, WvrButton(m_ButtonToTrigger.OptionList[b]));
				}
			}
			else
#endif
			{
				for (int i = 0; i < m_ButtonToTrigger.OptionList.Count; i++)
				{
					preButtonState[i] = buttonState[i];
					buttonState[i] = WXRDevice.KeyDown(device, m_ButtonToTrigger.OptionList[i]);

					if (!preButtonState[i] && buttonState[i])
						btnPressDown = true;
					if (buttonState[i])
						btnPressed = true;
				}
			}
		}
		#endregion

		#region Raycast
		private void HandleRaycast()
		{
			for (int i = 0; i < EventControllerProvider.ControllerTypes.Length; i++)
			{
				XR_Hand dev_type = EventControllerProvider.ControllerTypes[i];
				// -------------------- Conditions for running loop begins -----------------
				// 1.Do nothing if no event controller.
				EventController event_controller = GetEventController(dev_type);
				if (event_controller == null)
					continue;

				// 2. Exit the objects "entered" previously if not interactable.
				if (!IsControllerInteractable(dev_type))
				{
					if (dev_type == XR_Hand.Dominant)
					{
						ExitObjects(event_controller, ref preGraphicRaycastObjectsDominant);
						ExitObjects(event_controller, ref prePhysicsRaycastObjectsDominant);
					}
					if (dev_type == XR_Hand.NonDominant)
					{
						ExitObjects(event_controller, ref preGraphicRaycastObjectsNoDomint);
						ExitObjects(event_controller, ref prePhysicsRaycastObjectsNoDomint);
					}
					continue;
				}
				// -------------------- Conditions for running loop ends -----------------

				// -------------------- Set up the event camera begins -------------------
				Camera event_camera = null;
				if (m_BeamMode == BeamModes.Mouse)
					event_camera = (s_PointerTrackerMouse[dev_type] != null ? s_PointerTrackerMouse[dev_type].GetComponent<Camera>() : null);
				else
					event_camera = (event_controller.model != null ? event_controller.model.GetComponentInChildren<Camera>() : null);
				// -------------------- Set up the event camera ends ---------------------

				event_controller.raycastObjectEx = GetRaycastedObject(dev_type);
				ResetPointerEventDataHybrid(dev_type, event_camera);

				// -------------------- Raycast begins -------------------
				// 1. Get the nearest graphic raycast object.
				// Also, all raycasted graphic objects are stored in graphicRaycastObjects<device type>.
				if (event_camera != null)
				{
					if (dev_type == XR_Hand.Dominant)
						GraphicRaycast(event_controller, event_camera, ref graphicRaycastResultsDominant, ref graphicRaycastObjectsDominant);
					if (dev_type == XR_Hand.NonDominant)
						GraphicRaycast(event_controller, event_camera, ref graphicRaycastResultsNoDomint, ref graphicRaycastObjectsNoDomint);
				}

				// 2. Get the physical raycast object.
				// If the physical object is nearer than the graphic object, pointerCurrentRaycast will be set to the physical object.
				// Also, all raycasted physical objects are stored in physicsRaycastObjects<device type>.
				PhysicsRaycaster phy_raycaster = null;
				if (m_BeamMode == BeamModes.Mouse)
					phy_raycaster = event_camera.GetComponent<PhysicsRaycaster>();
				else
					phy_raycaster = (event_controller.model != null ? event_controller.model.GetComponentInChildren<PhysicsRaycaster>() : null);

				if (phy_raycaster != null)
				{
					// Issue: GC.Alloc 40 bytes.
					if (dev_type == XR_Hand.Dominant)
						PhysicsRaycast(event_controller, phy_raycaster, ref physicsRaycastResultsDominant, ref physicsRaycastObjectsDominant);
					if (dev_type == XR_Hand.NonDominant)
						PhysicsRaycast(event_controller, phy_raycaster, ref physicsRaycastResultsNoDomint, ref physicsRaycastObjectsNoDomint);
				}
				// -------------------- Raycast ends -------------------

				// Get the pointerCurrentRaycast object.
				GameObject curr_raycasted_obj = GetRaycastedObject(dev_type);

				// -------------------- Send Events begins -------------------
				// 1. Exit previous object, enter new object.
				if (dev_type == XR_Hand.Dominant)
				{
					EnterExitObjects(event_controller, graphicRaycastObjectsDominant, ref preGraphicRaycastObjectsDominant);
					EnterExitObjects(event_controller, physicsRaycastObjectsDominant, ref prePhysicsRaycastObjectsDominant);
				}
				if (dev_type == XR_Hand.NonDominant)
				{
					EnterExitObjects(event_controller, graphicRaycastObjectsNoDomint, ref preGraphicRaycastObjectsNoDomint);
					EnterExitObjects(event_controller, physicsRaycastObjectsNoDomint, ref prePhysicsRaycastObjectsNoDomint);
				}


				// 2. Hover object.
				if (curr_raycasted_obj != null && curr_raycasted_obj == event_controller.raycastObjectEx)
				{
					OnTriggerHover(dev_type, event_controller.eventData);
				}

				// 3. Get button states, some events are triggered by the button.
				UpdateButtonStates((XR_Device)dev_type);

				if (!btnPressDown && btnPressed)
				{
					// button hold means to drag.
					OnDrag(dev_type, event_controller.eventData);
				}
				else if (Time.unscaledTime - event_controller.eventData.clickTime < kClickInterval)
				{
					// Delay new events until kClickInterval has passed.
				}
				else if (btnPressDown && !event_controller.eventData.eligibleForClick)
				{
					// 1. button not pressed -> pressed.
					// 2. no pending Click should be procced.
					OnTriggerDown(dev_type, event_controller.eventData);
				}
				else if (!btnPressed)
				{
					// 1. If Down before, send Up event and clear Down state.
					// 2. If Dragging, send Drop & EndDrag event and clear Dragging state.
					// 3. If no Down or Dragging state, do NOTHING.
					OnTriggerUp(dev_type, event_controller.eventData);
				}
				// -------------------- Send Events ends -------------------

				PointerEventData event_data = event_controller.eventData;
				Vector3 intersec_pos = GetIntersectionPosition(event_data.enterEventCamera, event_data.pointerCurrentRaycast);

				RaycastResultProvider.Instance.SetRaycastResult(
					dev_type,
					event_controller.eventData.pointerCurrentRaycast.gameObject,
					intersec_pos
				);

				CheckBeamPointerActive(event_controller);
				SetupEventControllerBeam(event_controller, intersec_pos, false);
				SetupEventControllerPointer(event_controller, intersec_pos);
			} // for (int i = 0; i < EventControllerProvider.ControllerTypes.Length; i++)
		}

		private Vector2 eventDataPosition = Vector2.zero;
		private RaycastResult firstRaycastResult = new RaycastResult();
		private void ResetPointerEventDataHybrid(XR_Hand type, Camera eventCam)
		{
			EventController event_controller = GetEventController(type);
			if (event_controller != null)
			{
				if (event_controller.eventData == null)
					event_controller.eventData = new PointerEventData(EventSystem.current);

				if (m_BeamMode == BeamModes.Mouse && eventCam != null)
				{
					eventDataPosition.x = 0.5f * eventCam.pixelWidth;
					eventDataPosition.y = 0.5f * eventCam.pixelHeight;
				}
				else
				{
					eventDataPosition.x = 0.5f * Screen.width;
					eventDataPosition.y = 0.5f * Screen.height;
				}

				event_controller.eventData.Reset();
				event_controller.eventData.position = eventDataPosition;
				firstRaycastResult.Clear();
				event_controller.eventData.pointerCurrentRaycast = firstRaycastResult;
			}
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

			// 1. Check the "Order in Layer" of the Canvas.
			if (nextResult.sortingOrder > currResult.sortingOrder)
				return nextResult;

			// 2. Check the distance.
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

		List<RaycastResult> physicsRaycastResultsDominant = new List<RaycastResult>();
		List<RaycastResult> physicsRaycastResultsNoDomint = new List<RaycastResult>();
		List<RaycastResult> physicsResultList = new List<RaycastResult>();
		private void PhysicsRaycast(EventController event_controller, PhysicsRaycaster raycaster, ref List<RaycastResult> raycastResults, ref List<GameObject> raycastObjects)
		{
			raycastResults.Clear();
			raycastObjects.Clear();

			Profiler.BeginSample("PhysicsRaycaster.Raycast() dominant.");
			raycaster.Raycast(event_controller.eventData, raycastResults);
			Profiler.EndSample();

			GetResultList(raycastResults, physicsResultList);
			if (physicsResultList.Count == 0)
				return;

			firstRaycastResult = FindFirstResult(raycastResults);

			//if (firstRaycastResult.module != null)
				//DEBUG ("PhysicsRaycast() device: " + event_controller.device + ", camera: " + firstRaycastResult.module.eventCamera + ", first result = " + firstRaycastResult);
			event_controller.eventData.pointerCurrentRaycast = SelectRaycastResult(event_controller.eventData.pointerCurrentRaycast, firstRaycastResult);

			raycastTarget = event_controller.eventData.pointerCurrentRaycast.gameObject;
			while (raycastTarget != null)
			{
				raycastObjects.Add(raycastTarget);
				raycastTarget = (raycastTarget.transform.parent != null ? raycastTarget.transform.parent.gameObject : null);
			}
		}

		List<RaycastResult> graphicRaycastResultsDominant = new List<RaycastResult>();
		List<RaycastResult> graphicRaycastResultsNoDomint = new List<RaycastResult>();
		List<RaycastResult> graphicResultList = new List<RaycastResult>();
		private GameObject raycastTarget = null;
		private void GraphicRaycast(EventController event_controller, Camera event_camera, ref List<RaycastResult> raycastResults, ref List<GameObject> raycastObjects)
		{
			Profiler.BeginSample("Find GraphicRaycaster.");
			GraphicRaycaster[] graphic_raycasters = FindObjectsOfType<GraphicRaycaster>();
			Profiler.EndSample();

			raycastResults.Clear();
			raycastObjects.Clear();

			for (int i = 0; i < graphic_raycasters.Length; i++)
			{
				if (graphic_raycasters[i].gameObject != null && graphic_raycasters[i].gameObject.GetComponent<Canvas>() != null)
					graphic_raycasters[i].gameObject.GetComponent<Canvas>().worldCamera = event_camera;
				else
					continue;

				// 1. Get the dominant raycast results list.
				graphic_raycasters[i].Raycast(event_controller.eventData, raycastResults);
				GetResultList(raycastResults, graphicResultList);
				if (graphicResultList.Count == 0)
					continue;

				// 2. Get the dominant raycast objects list.
				firstRaycastResult = FindFirstResult(graphicResultList);

				//DEBUG ("GraphicRaycast() device: " + event_controller.device + ", camera: " + firstRaycastResult.module.eventCamera + ", first result = " + firstRaycastResult);
				event_controller.eventData.pointerCurrentRaycast = SelectRaycastResult(event_controller.eventData.pointerCurrentRaycast, firstRaycastResult);
				raycastResults.Clear();
			}

			raycastTarget = event_controller.eventData.pointerCurrentRaycast.gameObject;
			while (raycastTarget != null)
			{
				raycastObjects.Add(raycastTarget);
				raycastTarget = (raycastTarget.transform.parent != null ? raycastTarget.transform.parent.gameObject : null);
			}
		}
		#endregion

		#region Send Events
		List<GameObject> graphicRaycastObjectsDominant = new List<GameObject>(), preGraphicRaycastObjectsDominant = new List<GameObject>();
		List<GameObject> graphicRaycastObjectsNoDomint = new List<GameObject>(), preGraphicRaycastObjectsNoDomint = new List<GameObject>();
		List<GameObject> physicsRaycastObjectsDominant = new List<GameObject>(), prePhysicsRaycastObjectsDominant = new List<GameObject>();
		List<GameObject> physicsRaycastObjectsNoDomint = new List<GameObject>(), prePhysicsRaycastObjectsNoDomint = new List<GameObject>();
		private void EnterExitObjects(EventController eventController, List<GameObject> enterObjects, ref List<GameObject> exitObjects)
		{
			if (exitObjects.Count > 0)
			{
				for (int i = 0; i < exitObjects.Count; i++)
				{
					if (exitObjects[i] != null && !enterObjects.Contains(exitObjects[i]))
					{
						ExecuteEvents.Execute(exitObjects[i], eventController.eventData, ExecuteEvents.pointerExitHandler);
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
						ExecuteEvents.Execute(enterObjects[i], eventController.eventData, ExecuteEvents.pointerEnterHandler);
						DEBUG("EnterExitObjects() enter: " + enterObjects[i]);
					}
				}
			}

			CopyList(enterObjects, exitObjects);
		}

		private void ExitObjects(EventController event_controller, ref List<GameObject> exitObjects)
		{
			if (exitObjects.Count > 0)
			{
				for (int i = 0; i < exitObjects.Count; i++)
				{
					if (exitObjects[i] != null)
					{
						ExecuteEvents.Execute(exitObjects[i], event_controller.eventData, ExecuteEvents.pointerExitHandler);
						DEBUG("ExitObjects() exit: " + exitObjects[i]);
					}
				}
			}

			exitObjects.Clear();
		}

		private void ExitAllObjects(EventController event_controller)
		{
			ExitObjects(event_controller, ref preGraphicRaycastObjectsDominant);
			ExitObjects(event_controller, ref preGraphicRaycastObjectsNoDomint);
			ExitObjects(event_controller, ref prePhysicsRaycastObjectsDominant);
			ExitObjects(event_controller, ref prePhysicsRaycastObjectsNoDomint);
		}

		private void OnTriggerDown(XR_Hand type, PointerEventData eventData)
		{
			GameObject curr_raycasted_obj = GetRaycastedObject(type);
			if (curr_raycasted_obj == null)
				return;

			// Send Pointer Down. If not received, get handler of Pointer Click.
			eventData.pressPosition = eventData.position;
			eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;
			eventData.pointerPress =
				ExecuteEvents.ExecuteHierarchy(curr_raycasted_obj, eventData, ExecuteEvents.pointerDownHandler)
				?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(curr_raycasted_obj);

			DEBUG("OnTriggerDown() device: " + type + " send Pointer Down to " + eventData.pointerPress + ", current GameObject is " + curr_raycasted_obj);

			// If Drag Handler exists, send initializePotentialDrag event.
			eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(curr_raycasted_obj);
			if (eventData.pointerDrag != null)
			{
				DEBUG("OnTriggerDown() device: " + type + " send initializePotentialDrag to " + eventData.pointerDrag + ", current GameObject is " + curr_raycasted_obj);
				ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.initializePotentialDrag);
			}

			// press happened (even not handled) object.
			eventData.rawPointerPress = curr_raycasted_obj;
			// allow to send Pointer Click event
			eventData.eligibleForClick = true;
			// reset the screen position of press, can be used to estimate move distance
			eventData.delta = Vector2.zero;
			// current Down, reset drag state
			eventData.dragging = false;
			eventData.useDragThreshold = true;
			// record the count of Pointer Click should be processed, clean when Click event is sent.
			eventData.clickCount = 1;
			// set clickTime to current time of Pointer Down instead of Pointer Click.
			// since Down & Up event should not be sent too closely. (< kClickInterval)
			eventData.clickTime = Time.unscaledTime;
		}

		private void OnTriggerUp(XR_Hand type, PointerEventData eventData)
		{
			if (!eventData.eligibleForClick && !eventData.dragging)
			{
				// 1. no pending click
				// 2. no dragging
				// Mean user has finished all actions and do NOTHING in current frame.
				return;
			}

			GameObject curr_raycasted_obj = GetRaycastedObject(type);
			// curr_raycasted_obj may be different with eventData.pointerDrag so we don't check null

			if (eventData.pointerPress != null)
			{
				// In the frame of button is pressed -> unpressed, send Pointer Up
				DEBUG("OnTriggerUp() type: " + type + " send Pointer Up to " + eventData.pointerPress);
				ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);
			}

			if (eventData.eligibleForClick)
			{
				GameObject click_obj = ExecuteEvents.GetEventHandler<IPointerClickHandler>(curr_raycasted_obj);
				if (!m_PriorDrag)
				{
					if (click_obj != null)
					{
						if (click_obj == eventData.pointerPress)
						{
							// In the frame of button from being pressed to unpressed, send Pointer Click if Click is pending.
							DEBUG("OnTriggerUp() type: " + type + " send Pointer Click to " + eventData.pointerPress);
							ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerClickHandler);
						}
						else
						{
							DEBUG("OnTriggerUp() type: " + type
								+ " pointer down object " + eventData.pointerPress
								+ " is different with click object " + click_obj);
						}
					}
					else
					{
						if (eventData.dragging)
						{
							GameObject _pointerDrop = ExecuteEvents.GetEventHandler<IDropHandler>(curr_raycasted_obj);
							if (_pointerDrop == eventData.pointerDrag)
							{
								// In next frame of button from being pressed to unpressed, send Drop and EndDrag if dragging.
								DEBUG("OnTriggerUp() type: " + type + " send Pointer Drop to " + eventData.pointerDrag);
								ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dropHandler);
							}
							DEBUG("OnTriggerUp() type: " + type + " send Pointer endDrag to " + eventData.pointerDrag);
							ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);

							eventData.pointerDrag = null;
							eventData.dragging = false;
						}
					}
				}
				else
				{
					if (eventData.dragging)
					{
						GameObject _pointerDrop = ExecuteEvents.GetEventHandler<IDropHandler>(curr_raycasted_obj);
						if (_pointerDrop == eventData.pointerDrag)
						{
							// In next frame of button from being pressed to unpressed, send Drop and EndDrag if dragging.
							DEBUG("OnTriggerUp() type: " + type + " send Pointer Drop to " + eventData.pointerDrag);
							ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dropHandler);
						}
						DEBUG("OnTriggerUp() type: " + type + " send Pointer endDrag to " + eventData.pointerDrag);
						ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);

						eventData.pointerDrag = null;
						eventData.dragging = false;
					}
					else
					{
						if (click_obj != null)
						{
							if (click_obj == eventData.pointerPress)
							{
								// In the frame of button from being pressed to unpressed, send Pointer Click if Click is pending.
								DEBUG("OnTriggerUp() type: " + type + " send Pointer Click to " + eventData.pointerPress);
								ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerClickHandler);
							}
							else
							{
								DEBUG("OnTriggerUp() type: " + type
								+ " pointer down object " + eventData.pointerPress
								+ " is different with click object " + click_obj);
							}
						}
					}
				}
			}

			// Down of pending Click object.
			eventData.pointerPress = null;
			// press happened (even not handled) object.
			eventData.rawPointerPress = null;
			// clear pending state.
			eventData.eligibleForClick = false;
			// Click is processed, clearcount.
			eventData.clickCount = 0;
			// Up is processed thus clear the time limitation of Down event.
			eventData.clickTime = 0;
		}

		private void OnDrag(XR_Hand type, PointerEventData eventData)
		{
			if (Time.unscaledTime - eventData.clickTime < kTimeToDrag)
				return;
			if (eventData.pointerDrag == null)
				return;

			if (!eventData.dragging)
			{
				DEBUG("OnDrag() device: " + type + " send BeginDrag to " + eventData.pointerDrag);
				ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);
				eventData.dragging = true;
			}
			else
			{
				ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
			}
		}

		private void OnTriggerHover(XR_Hand type, PointerEventData eventData)
		{
			GameObject curr_raycasted_obj = GetRaycastedObject(type);
			ExecuteEvents.ExecuteHierarchy(curr_raycasted_obj, eventData, PointerEvents.pointerHoverHandler);
		}

		private void OnTriggerEnterAndExit(XR_Hand type, PointerEventData eventData)
		{
			GameObject curr_raycasted_obj = GetRaycastedObject(type);

			if (eventData.pointerEnter != curr_raycasted_obj)
			{
				DEBUG("OnTriggerEnterAndExit() " + type + ", enter: " + curr_raycasted_obj + ", exit: " + eventData.pointerEnter);

				HandlePointerExitAndEnter(eventData, curr_raycasted_obj);

				DEBUG("OnTriggerEnterAndExit() " + type + ", pointerEnter: " + eventData.pointerEnter + ", camera: " + eventData.enterEventCamera);
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
