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
using UnityEngine.EventSystems;
using UnityEngine.SpatialTracking;
using Wave.Native;
using Wave.Essence.Controller.Model;
using Wave.Essence.InputModule;
using Wave.Essence.Hand;
using Wave.Essence.Hand.Model;
using Wave.Essence.Events;

namespace Wave.Essence.Interaction.Mode
{
	[DisallowMultipleComponent]
	public sealed class InteractionModeManager : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Interaction.Mode.InteractionModeManager";
		private void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}
		private void INFO(string msg) { Log.i(LOG_TAG, msg, true); }

		#region Global Declaration
		public enum InputTypes
		{
			Gaze = 0,
			Controller = 1,
			Hand = 2,
		}
		#endregion

		#region InteractionModeManager Settings
		[HideInInspector]
		public GazeInputModule InputModuleGaze = null;
		[HideInInspector]
		public ControllerInputModule InputModuleController = null;
		[HideInInspector]
		public HandInputModule InputModuleHand = null;
		#endregion

		#region GazeInputModule Settings
		[Tooltip("To use the eye tracking data for gaze.")]
		[SerializeField]
		private bool m_EyeTracking = false;
		public bool EyeTracking { get { return m_EyeTracking; } set { m_EyeTracking = value; } }

		[Tooltip("Whether the gaze pointer is movable when gazing on an object.")]
		[SerializeField]
		private bool m_Movable = true;
		public bool Movable { get { return m_Movable; } set { m_Movable = value; } }

		[Tooltip("Set the event sent if gazed.")]
		[SerializeField]
		private GazeInputModule.GazeEvent m_InputEvent = GazeInputModule.GazeEvent.Down;
		public GazeInputModule.GazeEvent InputEvent { get { return m_InputEvent; } set { m_InputEvent = value; } }

		[Tooltip("A timer will be enabled to trigger gaze events if sets this value.")]
		[SerializeField]
		private bool m_TimerControl = true;
		public bool TimerControl { get { return m_TimerControl; } set { m_TimerControl = value; } }

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
		private GazeInputModule.DeviceOption m_ButtonControlDevices = new GazeInputModule.DeviceOption();
		public GazeInputModule.DeviceOption ButtonControlDevices { get { return m_ButtonControlDevices; } set { m_ButtonControlDevices = value; } }

		[Tooltip("Set the buttons to trigger gaze events.")]
		[SerializeField]
		private GazeInputModule.ButtonOption m_ButtonControlKeys = new GazeInputModule.ButtonOption();
		public GazeInputModule.ButtonOption ButtonControlKeys { get { return m_ButtonControlKeys; } set { m_ButtonControlKeys = value; } }
		#endregion

		#region ControllerInputModule Settings
		[Tooltip("There are 3 modes of different beam types.")]
		[SerializeField]
		private ControllerInputModule.BeamModes m_BeamMode = ControllerInputModule.BeamModes.Mouse;
		public ControllerInputModule.BeamModes BeamMode { get { return m_BeamMode; } set { m_BeamMode = value; } }

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
		private ControllerInputModule.ButtonOption m_ButtonToTrigger = new ControllerInputModule.ButtonOption();
		public ControllerInputModule.ButtonOption ButtonToTrigger { get { return m_ButtonToTrigger; } set { m_ButtonToTrigger = value; } }

		[Tooltip("Set the beam length in Fixed Beam Mode.")]
		[SerializeField]
		private float m_FixedBeamLength = 50;
		public float FixedBeamLength { get { return m_FixedBeamLength; } set { m_FixedBeamLength = value; } }

		[Tooltip("Right event controller.")]
		[SerializeField]
		private GameObject m_DominantController = null;
		public GameObject DominantController { get { return m_DominantController; } set { m_DominantController = value; } }

		[Tooltip("Left event controller.")]
		[SerializeField]
		private GameObject m_NonDominantController = null;
		public GameObject NonDominantController { get { return m_NonDominantController; } set { m_NonDominantController = value; } }

		[Tooltip("Does Dominant controller have electronic hand.")]
		[SerializeField]
		public bool m_DominantElectronicHand = false;

		[Tooltip("Does NonDominant controller have electronic hand.")]
		[SerializeField]
		public bool m_NonDominantElectronicHand = false;

		[HideInInspector]
		public GameObject DominantElectronicHand = null;

		[HideInInspector]
		public GameObject NonDominantElectronicHand = null;
		#endregion

		[Tooltip("Whether to show the Hand model when the electronic hand is supported in the Controller Mode or not.")]
		[SerializeField]
		private bool m_ShowElectronicHandIfSupported = false;
		public bool ShowElectronicHandIfSupported { get { return m_ShowElectronicHandIfSupported; } set { m_ShowElectronicHandIfSupported = value; } }

		[Tooltip("Start the natural Hand Tracking by default.")]
		[SerializeField]
		private bool m_InitialStartNaturalHand = false;
		public bool InitialStartNaturalHand { get { return m_InitialStartNaturalHand; } set { m_InitialStartNaturalHand = value; } }

		#region HandInputModule Settings
		[Tooltip("Sets the right hand selector used to point objects in a scene when the hand gesture is pinch.")]
		[SerializeField]
		private GameObject m_RightHandSelector = null;
		public GameObject RightHandSelector { get { return m_RightHandSelector; } set { m_RightHandSelector = value; } }

		private GameObject m_RightHandModel = null;

		[Tooltip("Sets the left hand selector used to point objects in a scene when the hand gesture is pinch.")]
		[SerializeField]
		private GameObject m_LeftHandSelector = null;
		public GameObject LeftHandSelector { get { return m_LeftHandSelector; } set { m_LeftHandSelector = value; } }

		private GameObject m_LeftHandModel = null;

		[Tooltip("Use default pinch threshold.")]
		[SerializeField]
		private bool m_UseDefaultPinch = false;
		public bool UseDefaultPinch { get { return m_UseDefaultPinch; } set { m_UseDefaultPinch = value; } }

		[Tooltip("The threshold of pinch on.")]
		[SerializeField]
		[Range(0.5f, 1)]
		private float m_PinchOnThreshold = 0.7f;
		public float PinchOnThreshold { get { return m_PinchOnThreshold; } set { m_PinchOnThreshold = value; } }

		[SerializeField]
		[Range(0.5f, 1)]
		[Tooltip("The threshold of pinch off.")]
		private float m_PinchOffThreshold = 0.7f;
		public float PinchOffThreshold { get { return m_PinchOffThreshold; } set { m_PinchOffThreshold = value; } }

		[SerializeField]
		[Tooltip("Starts dragging when pinching over this duration of time in seconds.")]
		private float m_PinchTimeToDrag = 1.0f;
		public float PinchTimeToDrag { get { return m_PinchTimeToDrag; } set { m_PinchTimeToDrag = value; } }
		#endregion

		private static InteractionModeManager m_Instance = null;
		public static InteractionModeManager Instance { get { return m_Instance; } }

		private InputTypes m_InputTypeEx = InputTypes.Controller;
		private InputTypes m_InputType = InputTypes.Controller;

		const string kHandManager = "HandManager";
		private GameObject m_HandManagerObject = null;
		private HandManager m_HandManager = null;

		#region MonoBehaviour Overrides
		private GameObject eventSystem = null;

		void Awake()
		{
			INFO("Awake()");
			m_Instance = this;
			CheckEventSystem();
			if (ClientInterface.InteractionMode == XR_InteractionMode.Default)
			{
				if (InputModuleGaze != null)
					InputModuleGaze.enabled = false;
				if (InputModuleController != null)
					InputModuleController.enabled = false;
				if (InputModuleHand != null)
					InputModuleHand.enabled = false;
			}
		}

		private bool mEnabled = false;
		private void OnEnable()
		{
			if (!mEnabled)
			{
				// Adds the HandManager for the hand data.
				if (HandManager.Instance == null)
				{
					DEBUG("OnEnable() Adds a HandManager.");
					m_HandManagerObject = new GameObject(kHandManager);
					m_HandManager = m_HandManagerObject.AddComponent<HandManager>();
					m_HandManager.enabled = false;
					m_HandManager.TrackerOptions.Natural.InitialStart = m_InitialStartNaturalHand;
					m_HandManager.enabled = true;
				}

				UpdateCurrentMode();
				m_InputTypeEx = m_InputType;
				INFO("OnEnable() Input type: " + m_InputType);
				AddInputModule(m_InputType);

				NotifyInteractionManagerExistToAll();

				GeneralEvent.Send(GeneralEvent.INTERACTION_MODE_MANAGER_READY);
				mEnabled = true;
			}
		}

		private void OnDisable()
		{
			if (mEnabled)
			{
				mEnabled = false;
			}
		}

		private void Update()
		{
			UpdateCurrentMode();
			if (m_InputTypeEx != m_InputType)
			{
				m_InputTypeEx = m_InputType;
				DEBUG("Update() Input type is changed to " + m_InputType);
				AddInputModule(m_InputType);
			}
		}
		#endregion

		#region Major Standalone Functions
		private void NotifyInteractionManagerExistToAll()
		{
			if (m_NonDominantController != null)
			{
				RenderModel model = m_NonDominantController.GetComponentInChildren<RenderModel>();

				if (model != null)
				{
					DEBUG("Notify NonDominantController RenderModel");
					model.checkInteractionMode = true;
				}

				ButtonEffect effect = m_NonDominantController.GetComponentInChildren<ButtonEffect>();

				if (effect != null)
				{
					DEBUG("Notify NonDominantController ButtonEffect");
					effect.checkInteractionMode = true;
				}

				ControllerTips tips = m_NonDominantController.GetComponentInChildren<ControllerTips>();

				if (tips != null)
				{
					DEBUG("Notify NonDominantController ControllerTips");
					tips.checkInteractionMode = true;
				}
			}

			if (m_DominantController != null)
			{
				RenderModel model = m_DominantController.GetComponentInChildren<RenderModel>();

				if (model != null)
				{
					DEBUG("Notify DominantController RenderModel");
					model.checkInteractionMode = true;
				}

				ButtonEffect effect = m_DominantController.GetComponentInChildren<ButtonEffect>();

				if (effect != null)
				{
					DEBUG("Notify DominantController ButtonEffect");
					effect.checkInteractionMode = true;
				}

				ControllerTips tips = m_DominantController.GetComponentInChildren<ControllerTips>();

				if (tips != null)
				{
					DEBUG("Notify DominantController ControllerTips");
					tips.checkInteractionMode = true;
				}
			}

			if (DominantElectronicHand != null)
			{
				HandMeshRenderer hmr = DominantElectronicHand.GetComponent<HandMeshRenderer>();

				if (hmr != null)
				{
					DEBUG("Notify DominantElectronicHand HandMeshRenderer");
					hmr.checkInteractionMode = true;
				}
			}

			if (NonDominantElectronicHand != null)
			{
				HandMeshRenderer hmr = NonDominantElectronicHand.GetComponent<HandMeshRenderer>();

				if (hmr != null)
				{
					DEBUG("Notify NonDominantElectronicHand HandMeshRenderer");
					hmr.checkInteractionMode = true;
				}
			}

			if (RightHandSelector != null)
			{
				HandMeshRenderer hmr = RightHandSelector.GetComponentInChildren<HandMeshRenderer>();

				if (hmr != null)
				{
					DEBUG("Notify RightHandSelector HandMeshRenderer");
					hmr.checkInteractionMode = true;
				}
			}

			if (LeftHandSelector != null)
			{
				HandMeshRenderer hmr = LeftHandSelector.GetComponentInChildren<HandMeshRenderer>();

				if (hmr != null)
				{
					DEBUG("Notify NonDominantElectronicHand HandMeshRenderer");
					hmr.checkInteractionMode = true;
				}
			}
		}

		private void CheckEventSystem()
		{
			if (EventSystem.current == null)
			{
				EventSystem event_system = FindObjectOfType<EventSystem>();
				if (event_system != null)
				{
					eventSystem = event_system.gameObject;
					INFO("CheckEventSystem() find current EventSystem: " + eventSystem.name);
				}

				if (eventSystem == null)
				{
					INFO("CheckEventSystem() could not find EventSystem, create new one.");
					eventSystem = new GameObject("EventSystem", typeof(EventSystem));
				}
			}
			else
			{
				eventSystem = EventSystem.current.gameObject;
			}

			if (eventSystem != null)
			{
				if (InputModuleGaze == null)
					InputModuleGaze = eventSystem.GetComponent<GazeInputModule>();
				if (InputModuleController == null)
					InputModuleController = eventSystem.GetComponent<ControllerInputModule>();
				if (InputModuleHand == null)
					InputModuleHand = eventSystem.GetComponent<HandInputModule>();
			}
		}
		private void UpdateCurrentMode()
		{
			switch (ClientInterface.InteractionMode)
			{
				case XR_InteractionMode.Gaze:
					m_InputType = InputTypes.Gaze;
					break;
				case XR_InteractionMode.Controller:
					m_InputType = InputTypes.Controller;
					break;
				case XR_InteractionMode.Hand:
					m_InputType = InputTypes.Hand;
					break;
				default:
					break;
			}
		}
		private void AddInputModule(InputTypes mode)
		{
			CheckEventSystem();
			if (mode == InputTypes.Gaze)
			{
				DisableControllerInputModule();
				DisableHandInputModule();
				EnableGazeInputModule();
			}
			if (mode == InputTypes.Controller)
			{
				DisableGazeInputModule();
				DisableHandInputModule();
				EnableControllerInputModule();
			}
			if (mode == InputTypes.Hand)
			{
				DisableGazeInputModule();
				DisableControllerInputModule();
				EnableHandInputModule();
			}
		}

		private void EnableGazeInputModule()
		{
			if (InputModuleGaze == null)
				InputModuleGaze = eventSystem.GetComponent<GazeInputModule>();
			if (InputModuleGaze == null)
			{
				eventSystem.SetActive(false);
				InputModuleGaze = eventSystem.AddComponent<GazeInputModule>();
				InputModuleGaze.EyeTracking = m_EyeTracking;
				InputModuleGaze.Movable = m_Movable;
				InputModuleGaze.InputEvent = m_InputEvent;
				InputModuleGaze.TimerControl = m_TimerControl;
				InputModuleGaze.TimeToGaze = m_TimeToGaze;
				InputModuleGaze.ButtonControl = m_ButtonControl;
				InputModuleGaze.ButtonControlDevices = m_ButtonControlDevices;
				InputModuleGaze.ButtonControlKeys = m_ButtonControlKeys;
				eventSystem.SetActive(true);
			}
			if (InputModuleGaze != null)
			{
				DEBUG("EnableGazeInputModule()");
				InputModuleGaze.enabled = true;
			}
		}
		private void DisableGazeInputModule()
		{
			if (InputModuleGaze != null)
			{
				DEBUG("DisableGazeInputModule()");
				InputModuleGaze.enabled = false;
			}
		}

		private void EnableControllerInputModule()
		{
			if (InputModuleController == null)
			{
				DEBUG("EnableControllerInputModule() Finding ControllerInputModule.");
				InputModuleController = eventSystem.GetComponent<ControllerInputModule>();
			}
			if (InputModuleController == null)
			{
				DEBUG("EnableControllerInputModule() Creates ControllerInputModule.");
				eventSystem.SetActive(false);
				InputModuleController = eventSystem.AddComponent<ControllerInputModule>();
				InputModuleController.BeamMode = m_BeamMode;
				InputModuleController.DominantEvent = m_DominantEvent;
				InputModuleController.DominantRaycastMask = m_DominantRaycastMask;
				InputModuleController.NonDominantEvent = m_NonDominantEvent;
				InputModuleController.NonDominantRaycastMask = m_NonDominantRaycastMask;
				InputModuleController.ButtonToTrigger = m_ButtonToTrigger;
				InputModuleController.FixedBeamLength = m_FixedBeamLength;

				if (m_DominantController == null)
					CreateController(XR_Hand.Dominant);
				InputModuleController.DominantController = m_DominantController;

				if (m_NonDominantController == null)
					CreateController(XR_Hand.NonDominant);
				InputModuleController.NonDominantController = m_NonDominantController;

				eventSystem.SetActive(true);
			}
			else
			{
				DEBUG("EnableControllerInputModule() found ControllerInputModule.");
				InputModuleController.enabled = true;

				m_DominantController = InputModuleController.DominantController;
				if (m_DominantController == null)
				{
					CreateController(XR_Hand.Dominant);
					InputModuleController.DominantController = m_DominantController;
				}

				m_NonDominantController = InputModuleController.NonDominantController;
				if (m_NonDominantController == null)
				{
					CreateController(XR_Hand.NonDominant);
					InputModuleController.NonDominantController = m_NonDominantController;
				}
			}
		}

		private void CreateController(XR_Hand hand)
		{
			GameObject controller_model = null;
			if (hand == XR_Hand.Dominant)
			{
				if (m_DominantController == null)
				{
					DEBUG("CreateController() Finding DominantController.");
					m_DominantController = GameObject.Find("DominantController");
				}
				if (m_DominantController == null)
				{
					DEBUG("CreateController() Creates DominantController.");
					m_DominantController = new GameObject("DominantController");
					m_DominantController.transform.SetParent(gameObject.transform, false);

					m_DominantController.SetActive(false);
					TrackedPoseDriver pose = m_DominantController.AddComponent<TrackedPoseDriver>();
					pose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);
					EventControllerSetter setter = m_DominantController.AddComponent<EventControllerSetter>();
					setter.ControllerType = hand;
					m_DominantController.SetActive(true);

					controller_model = new GameObject("DominantControllerModel");
					controller_model.transform.SetParent(m_DominantController.transform, false);
					controller_model.SetActive(false);
					PoseMode pm = controller_model.AddComponent<PoseMode>();
					pm.WhichHand = hand;

					GameObject controller_body = new GameObject("ControllerModelBody");
					controller_body.transform.SetParent(controller_model.transform, false);

					controller_body.SetActive(false);
					RenderModel model = controller_body.AddComponent<RenderModel>();
					model.WhichHand = hand;
					ButtonEffect effect = controller_body.AddComponent<ButtonEffect>();
					effect.HandType = hand;
					ControllerTips tips = controller_body.AddComponent<ControllerTips>();
					controller_body.SetActive(true);
					controller_model.SetActive(true);
				}
			}
			else
			{
				if (m_NonDominantController == null)
				{
					DEBUG("CreateController() Finding NonDominantController.");
					m_NonDominantController = GameObject.Find("NonDominantController");
				}
				if (m_NonDominantController == null)
				{
					DEBUG("CreateController() Creates NonDominantController.");
					m_NonDominantController = new GameObject("NonDominantController");
					m_NonDominantController.transform.SetParent(gameObject.transform, false);

					m_NonDominantController.SetActive(false);
					TrackedPoseDriver pose = m_NonDominantController.AddComponent<TrackedPoseDriver>();
					pose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.LeftPose);
					EventControllerSetter setter = m_NonDominantController.AddComponent<EventControllerSetter>();
					setter.ControllerType = hand;
					m_NonDominantController.SetActive(true);

					controller_model = new GameObject("NonDominantControllerModel");
					controller_model.transform.SetParent(m_NonDominantController.transform, false);
					controller_model.SetActive(false);
					PoseMode pm = controller_model.AddComponent<PoseMode>();
					pm.WhichHand = hand;

					GameObject controller_body = new GameObject("ControllerModelBody");
					controller_body.transform.SetParent(controller_model.transform, false);

					controller_body.SetActive(false);
					RenderModel model = controller_body.AddComponent<RenderModel>();
					model.WhichHand = hand;
					ButtonEffect effect = controller_body.AddComponent<ButtonEffect>();
					effect.HandType = hand;
					ControllerTips tips = controller_body.AddComponent<ControllerTips>();
					controller_body.SetActive(true);
					controller_model.SetActive(true);
				}
			}
		}

		private void DisableControllerInputModule()
		{
			if (InputModuleController != null)
			{
				DEBUG("DisableControllerInputModule()");
				InputModuleController.enabled = false;
			}
		}

		private void EnableHandInputModule()
		{
			if (InputModuleHand == null)
			{
				DEBUG("EnableHandInputModule() Finding HandInputModule.");
				InputModuleHand = eventSystem.GetComponent<HandInputModule>();
			}
			if (InputModuleHand == null)
			{
				DEBUG("EnableHandInputModule() Creates HandInputModule.");
				eventSystem.SetActive(false);
				InputModuleHand = eventSystem.AddComponent<HandInputModule>();
				InputModuleHand.UseDefaultPinch = m_UseDefaultPinch;
				InputModuleHand.PinchOnThreshold = m_PinchOnThreshold;
				InputModuleHand.PinchOffThreshold = m_PinchOffThreshold;

				if (m_RightHandSelector == null)
					CreateHand(HandManager.HandType.Right);
				InputModuleHand.RightHandSelector = m_RightHandSelector;

				if (m_LeftHandSelector == null)
					CreateHand(HandManager.HandType.Left);
				InputModuleHand.LeftHandSelector = m_LeftHandSelector;

				eventSystem.SetActive(true);
			}
			else
			{
				DEBUG("EnableHandInputModule() Found HandInputModule.");
				InputModuleHand.enabled = true;

				m_RightHandSelector = InputModuleHand.RightHandSelector;
				if (m_RightHandSelector == null)
				{
					CreateHand(HandManager.HandType.Right);
					InputModuleHand.RightHandSelector = m_RightHandSelector;
				}

				m_LeftHandSelector = InputModuleHand.LeftHandSelector;
				if (m_LeftHandSelector == null)
				{
					CreateHand(HandManager.HandType.Left);
					InputModuleHand.LeftHandSelector = m_LeftHandSelector;
				}
			}
		}
		private void CreateHand(HandManager.HandType hand)
		{
			GameObject selector_beam = null, selector_pointer = null, hand_prefab = null;

			if (hand == HandManager.HandType.Right)
			{
				if (m_RightHandSelector == null)
				{
					Debug.Log("CreateHand() Finding RightHandSelector.");
					m_RightHandSelector = GameObject.Find("RightHandSelector");
				}
				if (m_RightHandSelector == null)
				{
					GameObject RightHand = new GameObject("RightHand");
					RightHand.transform.SetParent(gameObject.transform, false);

					m_RightHandSelector = new GameObject("RightHandSelector");
					m_RightHandSelector.transform.SetParent(RightHand.transform, false);

					selector_beam = new GameObject("RightBeam");
					selector_beam.transform.SetParent(m_RightHandSelector.transform, false);
					selector_beam.SetActive(false);
					HandBeam beam = selector_beam.AddComponent<HandBeam>();
					beam.BeamType = hand;
					selector_beam.SetActive(true);

					selector_pointer = new GameObject("RightPointer");
					selector_pointer.transform.SetParent(m_RightHandSelector.transform, false);
					selector_pointer.SetActive(false);
					HandSpotPointer pointer = selector_pointer.AddComponent<HandSpotPointer>();
					pointer.PointerType = hand;
					selector_pointer.SetActive(true);

					DEBUG("Create WaveHandRight");
					hand_prefab = Resources.Load("Prefabs/WaveHandRight") as GameObject;
					m_RightHandModel = Instantiate(hand_prefab, this.transform);
					m_RightHandModel.transform.SetParent(RightHand.transform, false);
					HandMeshRenderer hmr = m_RightHandModel.GetComponent<HandMeshRenderer>();
					m_RightHandModel.SetActive(false);
					if (hmr != null)
					{
						DEBUG("Set WaveHandRight checkInteractionMode to true, ShowElectronicHandIfSupported = " + ShowElectronicHandIfSupported);
						hmr.checkInteractionMode = true;
						hmr.showElectronicHandInControllerMode = ShowElectronicHandIfSupported;
					}
					m_RightHandModel.SetActive(true);
				}
			}
			if (hand == HandManager.HandType.Left)
			{
				if (m_LeftHandSelector == null)
				{
					Debug.Log("CreateHand() Finding LeftHandSelector.");
					m_LeftHandSelector = GameObject.Find("LeftHandSelector");
				}
				if (m_LeftHandSelector == null)
				{
					GameObject LeftHand = new GameObject("LeftHand");
					LeftHand.transform.SetParent(gameObject.transform, false);

					m_LeftHandSelector = new GameObject("LeftHandSelector");
					m_LeftHandSelector.transform.SetParent(LeftHand.transform, false);

					selector_beam = new GameObject("LeftBeam");
					selector_beam.transform.SetParent(m_LeftHandSelector.transform, false);
					selector_beam.SetActive(false);
					HandBeam beam = selector_beam.AddComponent<HandBeam>();
					beam.BeamType = hand;
					selector_beam.SetActive(true);

					selector_pointer = new GameObject("LeftPointer");
					selector_pointer.transform.SetParent(m_LeftHandSelector.transform, false);
					selector_pointer.SetActive(false);
					HandSpotPointer pointer = selector_pointer.AddComponent<HandSpotPointer>();
					pointer.PointerType = hand;
					selector_pointer.SetActive(true);

					DEBUG("Create WaveHandLeft");
					hand_prefab = Resources.Load("Prefabs/WaveHandLeft") as GameObject;
					m_LeftHandModel = Instantiate(hand_prefab, this.transform);
					m_LeftHandModel.transform.SetParent(LeftHand.transform, false);
					HandMeshRenderer hmr = m_LeftHandModel.GetComponent<HandMeshRenderer>();
					m_LeftHandModel.SetActive(false);
					if (hmr != null)
					{
						DEBUG("Set WaveHandLeft checkInteractionMode to true, ShowElectronicHandIfSupported = " + ShowElectronicHandIfSupported);
						hmr.checkInteractionMode = true;
						hmr.showElectronicHandInControllerMode = ShowElectronicHandIfSupported;
					}
					m_LeftHandModel.SetActive(true);
				}
			}
		}
		private void DisableHandInputModule()
		{
			if (InputModuleHand != null)
			{
				DEBUG("DisableHandInputModule()");
				InputModuleHand.enabled = false;
			}
		}
		#endregion
	}
}
