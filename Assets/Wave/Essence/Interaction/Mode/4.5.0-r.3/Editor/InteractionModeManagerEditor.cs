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
using UnityEngine.SpatialTracking;
using Wave.XR;
using Wave.XR.Settings;
using Wave.Essence.Controller.Model;
using Wave.Essence.InputModule;
using Wave.Essence.Hand;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.SceneManagement;
using Wave.Essence.Hand.Model;


#if UNITY_EDITOR
using UnityEditor;

namespace Wave.Essence.Interaction.Mode.Editor
{
	[InitializeOnLoad]
	public static class InteractionModeManagerSettings
	{
		#region Public interface
		public const string kMenuAutoAddInteractionModeManager = "Wave/GameObject/Auto Add Interaction Mode Manager";
		public static bool AutoAddManager
		{
			get { return EditorPrefs.GetBool(kMenuAutoAddInteractionModeManager, true); }
			set {
				Debug.Log((value ? "Enable" : "Disable") + " " + kMenuAutoAddInteractionModeManager);
				EditorPrefs.SetBool(kMenuAutoAddInteractionModeManager, value);
			}
		}
		public const string kMenuAddInteractionModeManager = "Wave/GameObject/Add Interaction Mode Manager";

		const string kInteractionModeManager = "InteractionModeManager";
		public static void ForceAddManager(string sceneName = "")
		{
			var imm = new GameObject(kInteractionModeManager);
			Debug.Log("Add an InteractionModeManager to " + sceneName);
			imm.AddComponent<InteractionModeManager>();
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}
		#endregion


		static InteractionModeManagerSettings()
		{
			Debug.Log("InteractionModeManagerSettings() registered EditorSceneManager.sceneOpened.");

			UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneOpened;
		}

		private static void OnSceneOpened(Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
		{
			Debug.Log("Scene opened: " + scene.name);
			if (!ShouldAutoAddManager())
			{
				Debug.Log("OnSceneOpened() Not add manager automatically.");
				return;
			}

			if (!IsManagerOnceAdded(scene))
			{
				// Creates an InteractionModeManager gameObject.
				if (Object.FindObjectOfType<InteractionModeManager>() == null)
				{
					if (EditorUtility.DisplayDialog(
						"Add Interaction Mode Manager?",
						"Add the InteractionModeManager in your scene.\n\n" +
						"You can switch off this option from menu\n" +
						"Wave -> GameObject -> Auto Add Interaction Mode Manager",
						"OK",
						"Cancel"))
					{
						Debug.Log("The scene " + scene.name + " does NOT have InteractionModeManager and never imported before.");
						ForceAddManager(scene.name);
					}
				}

				// Records "addedManager" in asset.
				UpdateManagerAssetByScene(scene, true);
			}
		}


		#region InteractionModeManager.asset
		/// autoAddManager saved in Assets/Wave/Essence/Interaction/Mode/InteractionModeManager.asset
		static string WaveEssencePath = "Assets/Wave/Essence";
		const string kInteractionModeManagerAsset = "/Interaction/Mode/InteractionModeManager.asset";
		public static void UpdateManagerAsset(bool autoAddManager)
		{
			WaveXRSettings settings;
			EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out settings);
			if (settings != null)
				WaveEssencePath = settings.waveEssenceFolder;

			InteractionModeManagerAsset asset = null;
			if (File.Exists(WaveEssencePath + kInteractionModeManagerAsset))
			{
				asset = AssetDatabase.LoadAssetAtPath(WaveEssencePath + kInteractionModeManagerAsset, typeof(InteractionModeManagerAsset)) as InteractionModeManagerAsset;
				asset.autoAddManager = autoAddManager;
			}
			else
			{
				asset = ScriptableObject.CreateInstance(typeof(InteractionModeManagerAsset)) as InteractionModeManagerAsset;
				asset.autoAddManager = autoAddManager;
				AssetDatabase.CreateAsset(asset, WaveEssencePath + kInteractionModeManagerAsset);
			}
			AssetDatabase.SaveAssets();
			Debug.Log("UpdateManagerAsset() " + WaveEssencePath + kInteractionModeManagerAsset + ", autoAddManager: " + asset.autoAddManager);
		}
		public static bool ShouldAutoAddManager()
		{
			UpdateManagerAsset(AutoAddManager);
			InteractionModeManagerAsset asset = AssetDatabase.LoadAssetAtPath(WaveEssencePath + kInteractionModeManagerAsset, typeof(InteractionModeManagerAsset)) as InteractionModeManagerAsset;

			Debug.Log("ShouldAutoAddManager() InteractionModeManager will " + (asset.autoAddManager ? "be" : "NOT be") + " added automatically.");
			return asset.autoAddManager;
		}
		#endregion

		#region Scene Interaction Mode Manager asset.
		/// addedManager saved following the active scene location.
		private static void UpdateManagerAssetByScene(Scene scene, bool addedManager)
		{
			string asssetPath = Path.GetDirectoryName(SceneManager.GetActiveScene().path).Replace("\\", "/") + "/" + scene.name + "_InteractionMode.asset";
			InteractionModeManagerAsset asset = null;
			if (File.Exists(asssetPath))
			{
				asset = AssetDatabase.LoadAssetAtPath(asssetPath, typeof(InteractionModeManagerAsset)) as InteractionModeManagerAsset;
				asset.addedManager = addedManager;
			}
			else
			{
				asset = ScriptableObject.CreateInstance(typeof(InteractionModeManagerAsset)) as InteractionModeManagerAsset;
				asset.addedManager = addedManager;
				AssetDatabase.CreateAsset(asset, asssetPath);
			}
			AssetDatabase.SaveAssets();
			Debug.Log("UpdateInteractionModeAsset() " + asssetPath + ", addedManager: " + asset.addedManager);
		}
		private static bool IsManagerOnceAdded(Scene scene)
		{
			string asssetPath = Path.GetDirectoryName(SceneManager.GetActiveScene().path).Replace("\\", "/") + "/" + scene.name + "_InteractionMode.asset";
			if (!File.Exists(asssetPath))
				return false;

			InteractionModeManagerAsset asset = AssetDatabase.LoadAssetAtPath(asssetPath, typeof(InteractionModeManagerAsset)) as InteractionModeManagerAsset;
			return asset.addedManager;
		}
		#endregion
	}

	[CustomEditor(typeof(InteractionModeManager))]
	public class InteractionModeManagerEditor : UnityEditor.Editor
	{
		#region Menu Item
		// Item 1: Checkmark of auto add Interaction Mode Manager
		[MenuItem(InteractionModeManagerSettings.kMenuAutoAddInteractionModeManager, priority = 201)]
		public static void AutoAddInteractionModeManager()
		{
			InteractionModeManagerSettings.AutoAddManager = !InteractionModeManagerSettings.AutoAddManager;
			InteractionModeManagerSettings.UpdateManagerAsset(InteractionModeManagerSettings.AutoAddManager);
		}
		[MenuItem(InteractionModeManagerSettings.kMenuAutoAddInteractionModeManager, priority = 201, validate = true)]
		public static bool AutoAddInteractionModeManagerValidate()
		{
			Menu.SetChecked(InteractionModeManagerSettings.kMenuAutoAddInteractionModeManager, InteractionModeManagerSettings.AutoAddManager);
			return true;
		}

		// Item 2: Option of add Interaction Mode Manager
		[SerializeField]
		[MenuItem(InteractionModeManagerSettings.kMenuAddInteractionModeManager, priority = 202)]
		public static void AddInteractionModeManager()
		{
			InteractionModeManagerSettings.ForceAddManager(SceneManager.GetActiveScene().name);
		}
		[MenuItem(InteractionModeManagerSettings.kMenuAddInteractionModeManager, priority = 202, validate = true)]
		public static bool AddInteractionModeManagerValidate()
		{
			return (FindObjectOfType<InteractionModeManager>() == null);
		}
		#endregion


		SerializedProperty InputModuleGaze, InputModuleController, InputModuleHand;
		// Gaze property
		SerializedProperty /*m_EyeTracking, */m_Movable, m_InputEvent, m_TimeToGaze, m_ButtonControlDevices, m_ButtonControlKeys;
		// Controller property
		SerializedProperty m_DominantEvent, m_DominantRaycastMask, m_NonDominantEvent, m_NonDominantRaycastMask, m_ButtonToTrigger, m_FixedBeamLength;
		SerializedProperty DominantController, NonDominantController;
		//SerializedProperty m_ShowElectronicHandIfSupported; // Hide becuase NOT support electronic hand currently.
		// Hand property
		SerializedProperty m_InitialStartNaturalHand;
		SerializedProperty m_RightHandSelector, m_LeftHandSelector, m_UseDefaultPinch, m_PinchOnThreshold, m_PinchTimeToDrag, m_PinchOffThreshold;
		private void OnEnable()
		{
			InputModuleGaze = serializedObject.FindProperty("InputModuleGaze");
			InputModuleController = serializedObject.FindProperty("InputModuleController");
			InputModuleHand = serializedObject.FindProperty("InputModuleHand");

			// Gaze property
			//m_EyeTracking = serializedObject.FindProperty("m_EyeTracking");
			m_Movable = serializedObject.FindProperty("m_Movable");
			m_InputEvent = serializedObject.FindProperty("m_InputEvent");
			m_TimeToGaze = serializedObject.FindProperty("m_TimeToGaze");
			m_ButtonControlDevices = serializedObject.FindProperty("m_ButtonControlDevices");
			m_ButtonControlKeys = serializedObject.FindProperty("m_ButtonControlKeys");

			// Controller property
			m_DominantEvent = serializedObject.FindProperty("m_DominantEvent");
			m_DominantRaycastMask = serializedObject.FindProperty("m_DominantRaycastMask");
			m_NonDominantEvent = serializedObject.FindProperty("m_NonDominantEvent");
			m_NonDominantRaycastMask = serializedObject.FindProperty("m_NonDominantRaycastMask");
			m_ButtonToTrigger = serializedObject.FindProperty("m_ButtonToTrigger");
			m_FixedBeamLength = serializedObject.FindProperty("m_FixedBeamLength");

			DominantController = serializedObject.FindProperty("m_DominantController");
			NonDominantController = serializedObject.FindProperty("m_NonDominantController");

			//m_ShowElectronicHandIfSupported = serializedObject.FindProperty("m_ShowElectronicHandIfSupported");

			// Hand property
			m_InitialStartNaturalHand = serializedObject.FindProperty("m_InitialStartNaturalHand");
			m_RightHandSelector = serializedObject.FindProperty("m_RightHandSelector");
			m_LeftHandSelector = serializedObject.FindProperty("m_LeftHandSelector");
			m_UseDefaultPinch = serializedObject.FindProperty("m_UseDefaultPinch");
			m_PinchOnThreshold = serializedObject.FindProperty("m_PinchOnThreshold");
			m_PinchOffThreshold = serializedObject.FindProperty("m_PinchOffThreshold");
			m_PinchTimeToDrag = serializedObject.FindProperty("m_PinchTimeToDrag");
		}

		bool gazeOptions = true, controllerOptions = true, handOptions = true;
		bool gazeInputModuleOptions = false, controllerInputModuleOptions = false, handInputModuleOptions = false;
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			InteractionModeManager myScript = target as InteractionModeManager;

			GUIStyle foldoutStyle = EditorStyles.foldout;
			foldoutStyle.fontSize = 15;
			foldoutStyle.fontStyle = FontStyle.Bold;

			// Gaze options
			gazeOptions = EditorGUILayout.Foldout(gazeOptions, "Gaze Mode Options");

			foldoutStyle.fontSize = 12;
			foldoutStyle.fontStyle = FontStyle.Normal;

			if (gazeOptions)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(10);
				gazeInputModuleOptions = EditorGUILayout.Foldout(gazeInputModuleOptions, "Input Module Options");
				GUILayout.EndHorizontal();

				if (gazeInputModuleOptions)
				{
					GUILayout.Space(5);
					EditorGUILayout.HelpBox(
						"Creates an EventSystem GameObject with the GazeInputModule for Gaze Mode.",
						MessageType.Info);
					if (GUILayout.Button("Creates an Input Module."))
					{
						CreateInputModule(target as InteractionModeManager, XR_InteractionMode.Gaze);
					}

					EditorGUILayout.PropertyField(InputModuleGaze);

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
						GUILayout.BeginHorizontal();
						GUILayout.Space(10);
						EditorGUILayout.PropertyField(m_ButtonControlDevices);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						GUILayout.Space(10);
						EditorGUILayout.PropertyField(m_ButtonControlKeys);
						GUILayout.EndHorizontal();
					}
				}
			}

			// Controller options
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			foldoutStyle.fontSize = 15;
			foldoutStyle.fontStyle = FontStyle.Bold;

			controllerOptions = EditorGUILayout.Foldout(controllerOptions, "Controller Mode Options");

			foldoutStyle.fontSize = 12;
			foldoutStyle.fontStyle = FontStyle.Normal;

			if (controllerOptions)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(10);
				controllerInputModuleOptions = EditorGUILayout.Foldout(controllerInputModuleOptions, "Input Module Options");
				GUILayout.EndHorizontal();

				if (controllerInputModuleOptions)
				{
					GUILayout.Space(5);
					EditorGUILayout.HelpBox(
						"Creates an EventSystem GameObject with the ControllerInputModule for Controller Mode.",
						MessageType.Info);
					if (GUILayout.Button("Creates an Input Module."))
					{
						CreateInputModule(target as InteractionModeManager, XR_InteractionMode.Controller);
					}

					EditorGUILayout.PropertyField(InputModuleController);

					EditorGUILayout.HelpBox(
						"Creates the dominant event controller.",
						MessageType.Info);
					if (GUILayout.Button("Creates Dominant Controller."))
						CreateController(target as InteractionModeManager, XR_Hand.Dominant);
					EditorGUILayout.PropertyField(DominantController);

					GUILayout.Space(5);
					EditorGUILayout.HelpBox(
						"Creates the non-dominant event controller.",
						MessageType.Info);
					if (GUILayout.Button("Creates NonDominant Controller."))
						CreateController(target as InteractionModeManager, XR_Hand.NonDominant);
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
					GUILayout.BeginHorizontal();
					GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_ButtonToTrigger);
					GUILayout.EndHorizontal();
				}

				/*GUILayout.Space(10);
				EditorGUILayout.HelpBox(
					"Selects to show the Hand model in the controller mode when supported.",
					MessageType.Info);
				EditorGUILayout.PropertyField(m_ShowElectronicHandIfSupported);*/
			}

			// Hand options
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			foldoutStyle.fontSize = 15;
			foldoutStyle.fontStyle = FontStyle.Bold;

			handOptions = EditorGUILayout.Foldout(handOptions, "Hand Mode Options");

			foldoutStyle.fontSize = 12;
			foldoutStyle.fontStyle = FontStyle.Normal;

			GUILayout.Space(5);
			EditorGUILayout.HelpBox(
				"Note: You have to check the menu item\n" +
				"Wave > HandTracking > EnableHandTracking",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_InitialStartNaturalHand);
			if (handOptions)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(10);
				handInputModuleOptions = EditorGUILayout.Foldout(handInputModuleOptions, "Input Module Options");
				GUILayout.EndHorizontal();

				if (handInputModuleOptions)
				{
					GUILayout.Space(5);
					EditorGUILayout.HelpBox(
						"Creates an EventSystem GameObject with the HandInputModule for Hand Mode.",
						MessageType.Info);
					if (GUILayout.Button("Creates an Input Module."))
					{
						CreateInputModule(target as InteractionModeManager, XR_InteractionMode.Hand);
					}

					EditorGUILayout.PropertyField(InputModuleHand);

					GUILayout.Space(5);
					EditorGUILayout.HelpBox(
						"Chooses a selector for the right hand. A seletor should contain:\n" +
						"- A HandBeam object.\n" +
						"- A HandSpotPointer object.\n" +
						"You can click the following button to create and set a right selector automatically.",
						MessageType.Info);
					if (GUILayout.Button("Creates a right selector."))
						CreateHand(target as InteractionModeManager, HandManager.HandType.Right);
					EditorGUILayout.PropertyField(m_RightHandSelector);

					GUILayout.Space(5);
					EditorGUILayout.HelpBox(
						"Chooses a selector for the left hand. A seletor should contain:\n" +
						"- A HandBeam object.\n" +
						"- A HandSpotPointer object.\n" +
						"You can click the following button to create and set a left selector automatically.",
						MessageType.Info);
					if (GUILayout.Button("Creates a left selector."))
						CreateHand(target as InteractionModeManager, HandManager.HandType.Left);
					EditorGUILayout.PropertyField(m_LeftHandSelector);

					EditorGUILayout.HelpBox(
						"Use the system default pinch threshold.",
						MessageType.Info);
					myScript.UseDefaultPinch = EditorGUILayout.Toggle("Use Default Pinch", myScript.UseDefaultPinch);

					if (!myScript.UseDefaultPinch)
					{
						EditorGUILayout.HelpBox(
						"When the pinch strength is over threshold, the HandInputModule will start sending events",
						MessageType.Info);
						EditorGUILayout.PropertyField(m_PinchOnThreshold);

						EditorGUILayout.HelpBox(
							"The HandInputModule will keep sending events until the Pinch strength is smaller than the Pinch Off Threshold.",
							MessageType.Info);
						EditorGUILayout.PropertyField(m_PinchOffThreshold);
					}

					EditorGUILayout.HelpBox(
						"The HandInputModule will start sending drag events when it is pointing to the same object over this duration.",
						MessageType.Info);
					EditorGUILayout.PropertyField(m_PinchTimeToDrag);
				}
			}

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed)
				EditorUtility.SetDirty((InteractionModeManager)target);
		}

		[SerializeField]
		GazeInputModule m_GazeInputModule = null;
		[SerializeField]
		ControllerInputModule m_ControllerInputModule = null;
		[SerializeField]
		HandInputModule m_HandInputModule = null;
		private void CreateInputModule(InteractionModeManager target, XR_InteractionMode mode)
		{
			EventSystem event_system = FindObjectOfType<EventSystem>();
			if (event_system == null)
			{
				GameObject event_system_object = new GameObject("EventSystem");
				event_system = event_system_object.AddComponent<EventSystem>();
			}

			if (mode == XR_InteractionMode.Gaze)
			{
				m_GazeInputModule = FindObjectOfType<GazeInputModule>();
				if (m_GazeInputModule == null)
					m_GazeInputModule = event_system.gameObject.AddComponent<GazeInputModule>();
				target.InputModuleGaze = m_GazeInputModule;
			}
			if (mode == XR_InteractionMode.Controller)
			{
				m_ControllerInputModule = FindObjectOfType<ControllerInputModule>();
				if (m_ControllerInputModule == null)
					m_ControllerInputModule = event_system.gameObject.AddComponent<ControllerInputModule>();

				CreateController(target, XR_Hand.Dominant);
				CreateController(target, XR_Hand.NonDominant);

				target.InputModuleController = m_ControllerInputModule;
			}
			if (mode == XR_InteractionMode.Hand)
			{
				m_HandInputModule = FindObjectOfType<HandInputModule>();
				if (m_HandInputModule == null)
					m_HandInputModule = event_system.gameObject.AddComponent<HandInputModule>();

				target.InputModuleHand = m_HandInputModule;
			}
			if (mode == XR_InteractionMode.Controller || mode == XR_InteractionMode.Hand)
			{
				CreateHand(target, HandManager.HandType.Left);
				CreateHand(target, HandManager.HandType.Right);
			}
		}

		[SerializeField]
		GameObject m_DominantController = null, m_NonDominantController = null;
		private void CreateController(InteractionModeManager target, XR_Hand hand)
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
					Debug.Log("CreateController() Finding m_DominantController.");
					m_DominantController = GameObject.Find("DominantController");
				}
				if (m_DominantController == null)
				{
					/**
					 *    Root
					 *      |- DominantPoseOffset (PoseMode)
					 **/
					GameObject pose_offset = new GameObject("DominantPoseOffset");

					pose_offset.SetActive(false);
					PoseMode pm = pose_offset.AddComponent<PoseMode>();
					pm.WhichHand = hand;
					pose_offset.SetActive(true);

					/**
					 *    Root
					 *      |- DominantPoseOffset (PoseMode)
					 *      |    |- DominantController (TrackedPoseDriver, EventControllerSetter)
					 **/
					Debug.Log("CreateController() Creates m_DominantController.");
					m_DominantController = new GameObject("DominantController");
					m_DominantController.transform.SetParent(pose_offset.transform, false);

					m_DominantController.SetActive(false);
					TrackedPoseDriver pose = m_DominantController.AddComponent<TrackedPoseDriver>();
					pose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);
					EventControllerSetter setter = m_DominantController.AddComponent<EventControllerSetter>();
					setter.ControllerType = hand;
					m_DominantController.SetActive(true);

					/**
					 *    Root
					 *      |- DominantPoseOffset (PoseMode)
					 *      |    |- DominantController (TrackedPoseDriver, EventControllerSetter)
					 *      |        |- DominantModel
					 **/
					GameObject controller_body = new GameObject("DominantModel");
					controller_body.transform.SetParent(m_DominantController.transform, false);

					controller_body.SetActive(false);
					RenderModel model = controller_body.AddComponent<RenderModel>();
					model.WhichHand = hand;
					ButtonEffect effect = controller_body.AddComponent<ButtonEffect>();
					effect.HandType = hand;
					ControllerTips tips = controller_body.AddComponent<ControllerTips>();
					controller_body.SetActive(true);
				}
				target.DominantController = m_DominantController;

				// For InteractionModeManager
				if (m_ControllerInputModule == null)
					m_ControllerInputModule = FindObjectOfType<ControllerInputModule>();
				if (m_ControllerInputModule != null)
					m_ControllerInputModule.DominantController = m_DominantController;
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
					Debug.Log("CreateController() Finding m_NonDominantController.");
					m_NonDominantController = GameObject.Find("NonDominantController");
				}
				if (m_NonDominantController == null)
				{
					/**
					 *    Root
					 *      |- NonDominantPoseOffset (PoseMode)
					 **/
					GameObject pose_offset = new GameObject("NonDominantPoseOffset");

					pose_offset.SetActive(false);
					PoseMode pm = pose_offset.AddComponent<PoseMode>();
					pm.WhichHand = hand;

					/**
					 *    Root
					 *      |- NonDominantPoseOffset (PoseMode)
					 *      |    |- NonDominantController (TrackedPoseDriver, EventControllerSetter)
					 **/
					Debug.Log("CreateController() Creates m_NonDominantController.");
					m_NonDominantController = new GameObject("NonDominantController");
					m_NonDominantController.transform.SetParent(pose_offset.transform, false);

					m_NonDominantController.SetActive(false);
					TrackedPoseDriver pose = m_NonDominantController.AddComponent<TrackedPoseDriver>();
					pose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.LeftPose);
					EventControllerSetter setter = m_NonDominantController.AddComponent<EventControllerSetter>();
					setter.ControllerType = hand;
					m_NonDominantController.SetActive(true);

					/**
					 *    Root
					 *      |- NonDominantPoseOffset (PoseMode)
					 *      |    |- NonDominantController (TrackedPoseDriver, EventControllerSetter)
					 *      |        |- NonDominantModel
					 **/
					GameObject controller_body = new GameObject("NonDominantModel");
					controller_body.transform.SetParent(m_NonDominantController.transform, false);

					controller_body.SetActive(false);
					RenderModel model = controller_body.AddComponent<RenderModel>();
					model.WhichHand = hand;
					ButtonEffect effect = controller_body.AddComponent<ButtonEffect>();
					effect.HandType = hand;
					ControllerTips tips = controller_body.AddComponent<ControllerTips>();
					controller_body.SetActive(true);
					pose_offset.SetActive(true);
				}
				target.NonDominantController = m_NonDominantController;

				// For InteractionModeManager
				if (m_ControllerInputModule == null)
					m_ControllerInputModule = FindObjectOfType<ControllerInputModule>();
				if (m_ControllerInputModule != null)
					m_ControllerInputModule.NonDominantController = m_NonDominantController;
			}
		}

		[SerializeField]
		GameObject m_RightSelector = null, m_LeftSelector = null;
		private void CreateHand(InteractionModeManager target, HandManager.HandType hand)
		{
			GameObject selector_beam = null, selector_pointer = null;

			if (hand == HandManager.HandType.Right)
			{
				if (m_RightSelector == null)
				{
					Debug.Log("CreateHand() Restores RightHandSelector.");
					m_RightSelector = target.RightHandSelector;
				}
				if (m_RightSelector == null)
				{
					Debug.Log("CreateHand() Finding RightHandSelector.");
					m_RightSelector = GameObject.Find("RightHandSelector");
				}
				if (m_RightSelector == null)
				{
					GameObject RightHand = new GameObject("RightHand");

					Debug.Log("CreateHand() Creates m_RightSelector.");
					m_RightSelector = new GameObject("RightHandSelector");
					m_RightSelector.transform.SetParent(RightHand.transform, false);

					selector_beam = new GameObject("RightBeam");
					selector_beam.transform.SetParent(m_RightSelector.transform, false);
					selector_beam.SetActive(false);
					HandBeam beam = selector_beam.AddComponent<HandBeam>();
					beam.BeamType = hand;
					selector_beam.SetActive(true);

					selector_pointer = new GameObject("RightPointer");
					selector_pointer.transform.SetParent(m_RightSelector.transform, false);
					selector_pointer.SetActive(false);
					HandSpotPointer pointer = selector_pointer.AddComponent<HandSpotPointer>();
					pointer.PointerType = hand;
					selector_pointer.SetActive(true);

					var o = InstantiateHandModel(hand);
					HandMeshRenderer hmr = o.GetComponent<HandMeshRenderer>();
					if (hmr != null)
					{
						Debug.Log("CreateRightHand() by interaction manager.");
						hmr.checkInteractionMode = true;
						hmr.showElectronicHandInControllerMode = target.ShowElectronicHandIfSupported;
					}

					o.transform.SetParent(RightHand.transform, false);
				}
				target.RightHandSelector = m_RightSelector;

				// For InteractionModeManager
				if (m_HandInputModule == null)
					m_HandInputModule = FindObjectOfType<HandInputModule>();
				if (m_HandInputModule != null)
					m_HandInputModule.RightHandSelector = m_RightSelector;
			}
			else
			{
				if (m_LeftSelector == null)
				{
					Debug.Log("CreateHand() Restores LeftHandSelector.");
					m_LeftSelector = target.LeftHandSelector;
				}
				if (m_LeftSelector == null)
				{
					Debug.Log("CreateHand() Finding LeftHandSelector.");
					m_LeftSelector = GameObject.Find("LeftHandSelector");
				}
				if (m_LeftSelector == null)
				{
					GameObject LeftHand = new GameObject("LeftHand");

					Debug.Log("CreateHand() Creates m_LeftSelector.");
					m_LeftSelector = new GameObject("LeftHandSelector");
					m_LeftSelector.transform.SetParent(LeftHand.transform, false);

					selector_beam = new GameObject("LeftBeam");
					selector_beam.transform.SetParent(m_LeftSelector.transform, false);
					selector_beam.SetActive(false);
					HandBeam beam = selector_beam.AddComponent<HandBeam>();
					beam.BeamType = hand;
					selector_beam.SetActive(true);

					selector_pointer = new GameObject("LeftPointer");
					selector_pointer.transform.SetParent(m_LeftSelector.transform, false);
					selector_pointer.SetActive(false);
					HandSpotPointer pointer = selector_pointer.AddComponent<HandSpotPointer>();
					pointer.PointerType = hand;
					selector_pointer.SetActive(true);

					var o = InstantiateHandModel(hand);

					HandMeshRenderer hmr = o.GetComponent<HandMeshRenderer>();
					if (hmr != null)
					{
						Debug.Log("CreateLeftHand() by interaction manager.");
						hmr.checkInteractionMode = true;
						hmr.showElectronicHandInControllerMode = target.ShowElectronicHandIfSupported;
					}

					o.transform.SetParent(LeftHand.transform, false);
				}
				target.LeftHandSelector = m_LeftSelector;

				// For InteractionModeManager
				if (m_HandInputModule == null)
					m_HandInputModule = FindObjectOfType<HandInputModule>();
				if (m_HandInputModule != null)
					m_HandInputModule.LeftHandSelector = m_LeftSelector;
			}
		}

		private GameObject InstantiateHandModel(HandManager.HandType hand)
		{
			GameObject go = null;
			string[] results;

			string modelName = (hand == HandManager.HandType.Left) ? "WaveHandLeft" : "WaveHandRight";
			results = AssetDatabase.FindAssets(modelName);
			foreach (string guid in results)
			{
				string p = AssetDatabase.GUIDToAssetPath(guid);
				Debug.Log(modelName + " path: " + p);
				if (p.Contains(".prefab"))
				{
					var o = AssetDatabase.LoadAssetAtPath(p, typeof(GameObject)) as GameObject;

					go = PrefabUtility.InstantiatePrefab(o) as GameObject;
				}
			}
			return go;
		}
	}
}
#endif
