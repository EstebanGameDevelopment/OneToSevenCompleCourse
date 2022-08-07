// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wave.Native;
using Wave.Essence.Hand;
using Wave.Essence.InputModule;
using UnityEngine.XR;

namespace Wave.Essence.Interaction.Mode.Demo
{
	[DisallowMultipleComponent]
	public class NaturalHandInputModule : BaseInputModule
	{
		private const string LOG_TAG = "Wave.Essence.Interaction.Mode.Demo.NaturalHandInputModule";
		private void INFO(string msg) { Log.i(LOG_TAG, msg, true); }
		private void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}

		#region Inspector
		[Tooltip("Sets the right hand selector used to point objects in a scene when the hand gesture is pinch.")]
		[SerializeField]
		private GameObject m_RightHandSelector = null;
		public GameObject RightHandSelector { get { return m_RightHandSelector; } set { m_RightHandSelector = value; } }

		[Tooltip("Sets the left hand selector used to point objects in a scene when the hand gesture is pinch.")]
		[SerializeField]
		private GameObject m_LeftHandSelector = null;
		public GameObject LeftHandSelector { get { return m_LeftHandSelector; } set { m_LeftHandSelector = value; } }

		[Tooltip("The threshold of pinch on.")]
		[SerializeField]
		[Range(0.5f, 1)]
		private float m_PinchOnThreshold = 0.7f;
		public float PinchOnThreshold { get { return m_PinchOnThreshold; } set { m_PinchOnThreshold = value; } }

		[SerializeField]
		[Tooltip("Starts dragging when pinching over this duration of time in seconds.")]
		private float m_PinchTimeToDrag = 1.0f;
		public float PinchTimeToDrag { get { return m_PinchTimeToDrag; } set { m_PinchTimeToDrag = value; } }

		[SerializeField]
		[Range(0.5f, 1)]
		[Tooltip("The threshold of pinch off.")]
		private float m_PinchOffThreshold = 0.7f;
		public float PinchOffThreshold { get { return m_PinchOffThreshold; } set { m_PinchOffThreshold = value; } }

		[Tooltip("Ignore the interaction mode.")]
		[SerializeField]
		private bool m_IgnoreMode = false;
		public bool IgnoreMode { get { return m_IgnoreMode; } set { m_IgnoreMode = value; } }
		#endregion

		private void ValidateParameters()
		{
			if (m_PinchOffThreshold > m_PinchOnThreshold)
				m_PinchOffThreshold = m_PinchOnThreshold;
		}

		private bool m_SingleInput = true;
		public bool SingleInput { get { return m_SingleInput; } set { m_SingleInput = value; } }

		private readonly HandManager.HandType[] s_Hands = new HandManager.HandType[] {
			HandManager.HandType.Left,
			HandManager.HandType.Right
		};

		private Dictionary<HandManager.HandType, HandManager.HandMotion> m_HandMotion = new Dictionary<HandManager.HandType, HandManager.HandMotion>() {
			{ HandManager.HandType.Left, HandManager.HandMotion.None },
			{ HandManager.HandType.Right, HandManager.HandMotion.None }
		};
		private Dictionary<HandManager.HandType, Vector3> m_PinchOrigin = new Dictionary<HandManager.HandType, Vector3>() {
			{ HandManager.HandType.Left, Vector3.zero },
			{ HandManager.HandType.Right, Vector3.zero }
		};
		private Dictionary<HandManager.HandType, Vector3> m_PinchDirection = new Dictionary<HandManager.HandType, Vector3>() {
			{ HandManager.HandType.Left, Vector3.zero },
			{ HandManager.HandType.Right, Vector3.zero }
		};
		private Dictionary<HandManager.HandType, float> m_PinchStrength = new Dictionary<HandManager.HandType, float>() {
			{ HandManager.HandType.Left, 0 },
			{ HandManager.HandType.Right, 0 }
		};

		/// NaturalHandPointerTracker is used to track the HandPointer
		private Dictionary<HandManager.HandType, NaturalHandPointerTracker> s_HandPointerTracker = new Dictionary<HandManager.HandType, NaturalHandPointerTracker>() {
			{ HandManager.HandType.Left, null },
			{ HandManager.HandType.Right, null }
		};

		#region BaseInputModule Overrides
		private bool mEnabled = false;
		protected override void OnEnable()
		{
			if (!mEnabled)
			{
				base.OnEnable();

				// 0. Disable the existed StandaloneInputModule.
				Destroy(GetComponent<StandaloneInputModule>());

				// 1. Set up necessary components for Hand input.
				if (Camera.main != null)
				{
					NaturalHandPointerTracker[] trackers = Camera.main.gameObject.GetComponentsInChildren<NaturalHandPointerTracker>();
					foreach (NaturalHandPointerTracker tracker in trackers)
						s_HandPointerTracker[tracker.Hand] = tracker;

					if (s_HandPointerTracker[HandManager.HandType.Left] == null)
						CreatePointerTracker(HandManager.HandType.Left);
					Log.i(LOG_TAG, "OnEnable() Left pointer tracker: " + s_HandPointerTracker[HandManager.HandType.Left].gameObject.name, true);

					if (s_HandPointerTracker[HandManager.HandType.Right] == null)
						CreatePointerTracker(HandManager.HandType.Right);
					Log.i(LOG_TAG, "OnEnable() Right pointer tracker: " + s_HandPointerTracker[HandManager.HandType.Right].gameObject.name, true);
				}
				else
				{
					Log.w(LOG_TAG, "OnEable() Please set up the Main Camera", true);
				}

				INFO("OnEnable() m_RightHandSelector: " + (m_RightHandSelector == null ? "null" : m_RightHandSelector.name)
					+ ", m_LeftHandSelector: " + (m_LeftHandSelector == null ? "null" : m_LeftHandSelector.name));

				mEnabled = true;
			}
		}
		protected override void OnDisable()
		{
			if (mEnabled)
			{
				base.OnDisable();
				DEBUG("OnDisable()");

				ActivateBeamPointer(HandManager.HandType.Left, false);
				ActivateBeamPointer(HandManager.HandType.Right, false);
				mEnabled = false;
			}
		}
		public override void Process()
		{
			ValidateParameters();

			for (int i = 0; i < s_Hands.Length; i++)
			{
				HandManager.HandType hand = s_Hands[i];

				/// 1. Update the beam, pointer, event camera and physics raycaster.
				UpdateComponents(hand);

				/// 2. Save previous raycasted object.
				prevRaycastedObject = GetRaycastedObject(hand);

				/// 3. Updates hand pose related data.
				if (HandManager.Instance != null)
				{
					m_HandMotion[hand] = HandManager.Instance.GetHandMotion(hand == HandManager.HandType.Left ? true : false);

					//UpdatePinchOriginDirection(hand);
					UpdatePinchOriginDirectionVIU(hand);

					m_PinchStrength[hand] = HandManager.Instance.GetPinchStrength(hand == HandManager.HandType.Left ? true : false);
				}

				/// 4. Updates the selector pose with hand pose data.
				if (m_LeftHandSelector != null)
				{
					m_LeftHandSelector.transform.localPosition = m_PinchOrigin[HandManager.HandType.Left];
					if (!m_PinchDirection[HandManager.HandType.Left].Equals(Vector3.zero))
						m_LeftHandSelector.transform.localRotation = Quaternion.LookRotation(m_PinchDirection[HandManager.HandType.Left]);
				}

				if (m_RightHandSelector != null)
				{
					m_RightHandSelector.transform.localPosition = m_PinchOrigin[HandManager.HandType.Right];
					if (!m_PinchDirection[HandManager.HandType.Right].Equals(Vector3.zero))
						m_RightHandSelector.transform.localRotation = Quaternion.LookRotation(m_PinchDirection[HandManager.HandType.Right]);
				}

				/// 5. Shows the interactable beam and pointer. Hides the uninteractable beam and pointer.
				if (!IsHandInteractable(hand))
					continue;

				/// 6. The beam and pointer will become effective when pinching and uneffective when not pinching.
				/// isPinch is updated here.
				LegalizeBeamPointerOnPinch(hand);

				/// 7. Raycasts when not dragging.
				if ((mPointerEventData[hand] == null) ||
					(mPointerEventData[hand] != null && !mPointerEventData[hand].dragging))
				{
					ResetPointerEventData(hand);
					GraphicRaycast(hand);
					PhysicsRaycast(hand);
				}

				/// 8. Shows the pointer when casting to an object. Hides the pointer when not casting to any object.
				Vector3 intersection_position = GetIntersectionPosition(hand, mPointerEventData[hand].pointerCurrentRaycast);

				if (m_HandSpotPointer[hand] != null)
				{
					GameObject curr_raycasted_object = GetRaycastedObject(hand);
					if (curr_raycasted_object != null)
						m_HandSpotPointer[hand].OnPointerEnter(curr_raycasted_object, intersection_position, true);
					else
						m_HandSpotPointer[hand].OnPointerExit(prevRaycastedObject);
				}

				/// 9. If the pinch origin is invalid, do NOT send event at this frame.
				///     If dragging before, will keep dragging.
				bool send_event = !m_PinchOrigin[hand].Equals(Vector3.zero);
				if (send_event)
				{
					OnGraphicPointerEnterExit(hand);
					OnPhysicsPointerEnterExit(hand);

					OnPointerHover(hand);

					if (!mPointerEventData[hand].eligibleForClick)
					{
						if (isPinch[hand])
							OnPointerDown(hand);
					}
					else if (mPointerEventData[hand].eligibleForClick)
					{
						if (isPinch[hand])
						{
							// Down before, and receives the selected gesture continuously.
							OnPointerDrag(hand);

						}
						else
						{
							DEBUG("Focus hand: " + HandInputSwitch.Instance.PrimaryInput
								+ ", right strength: " + m_PinchStrength[HandManager.HandType.Right]
								+ ", left strength: " + m_PinchStrength[HandManager.HandType.Left]);
							// Down before, but not receive the selected gesture.
							OnPointerUp(hand);
						}
					}
				}
			}
		}
		#endregion

		#region Major Standalone Functions
		private bool IsHandInteractable(HandManager.HandType hand)
		{
			bool interactable = false;

			bool focused = ClientInterface.IsFocused;
			bool is_tracked = (HandManager.Instance != null ? HandManager.Instance.IsHandPoseValid(hand) : false);
			bool hand_mode = (m_IgnoreMode || (ClientInterface.InteractionMode == XR_InteractionMode.Hand));
			bool primary_input = ((!m_SingleInput) || (hand == HandInputSwitch.Instance.PrimaryInput));
			bool valid_motion = (m_HandMotion[hand] != HandManager.HandMotion.None);
			// Ignore the Main Camera case.

			interactable = focused && is_tracked && hand_mode && primary_input && valid_motion;

			if (Log.gpl.Print)
			{
				DEBUG("IsHandInteractable() " + hand +
					", interactable: " + interactable +
					", focused: " + focused +
					", is_tracked: " + is_tracked +
					", m_IgnoreMode: " + m_IgnoreMode +
					", interaction mode: " + ClientInterface.InteractionMode +
					", primary_input: " + primary_input
					);
			}

			ActivateBeamPointer(hand, interactable);

			return interactable;
		}

		private void CreatePointerTracker(HandManager.HandType hand)
		{
			if (Camera.main == null)
				return;

			// 1. Create a pointer tracker gameObject and attach to the head.
			var pt = new GameObject(hand + "HandTracker");
			pt.transform.SetParent(Camera.main.gameObject.transform, false);
			pt.transform.localPosition = Vector3.zero;
			DEBUG("CreatePointerTracker() " + hand + " sets pointer tracker parent to " + pt.transform.parent.name);

			// 2. Add the ControllerPointerTracker component.
			pt.SetActive(false);
			s_HandPointerTracker[hand] = pt.AddComponent<NaturalHandPointerTracker>();
			s_HandPointerTracker[hand].Hand = hand;
			pt.SetActive(true);
			DEBUG("CreatePointerTracker() " + hand + " sets pointer tracker type to " + s_HandPointerTracker[hand].Hand);

			// 3. Set the pointer tracker PhysicsRaycaster eventMask.
			//PhysicsRaycaster phy_raycaster = pt.GetComponent<PhysicsRaycaster>();
		}

		/// HandBeam and HandPointer
		private Dictionary<HandManager.HandType, GameObject> beamObject = new Dictionary<HandManager.HandType, GameObject>() {
			{ HandManager.HandType.Left, null },
			{ HandManager.HandType.Right, null }
		};
		private Dictionary<HandManager.HandType, HandBeam> m_HandBeam = new Dictionary<HandManager.HandType, HandBeam>() {
			{ HandManager.HandType.Left, null },
			{ HandManager.HandType.Right, null }
		};
		private Dictionary<HandManager.HandType, GameObject> pointerObject = new Dictionary<HandManager.HandType, GameObject>() {
			{ HandManager.HandType.Left, null },
			{ HandManager.HandType.Right, null }
		};
		private Dictionary<HandManager.HandType, NaturalHandSpotPointer> m_HandSpotPointer = new Dictionary<HandManager.HandType, NaturalHandSpotPointer>() {
			{ HandManager.HandType.Left, null },
			{ HandManager.HandType.Right, null }
		};

		private void ActivateBeamPointer(HandManager.HandType hand, bool active)
		{
			if (m_HandBeam[hand] != null)
				m_HandBeam[hand].ShowBeam = active;

			if (m_HandSpotPointer[hand] != null)
				m_HandSpotPointer[hand].ShowPointer = active;
		}

		/// Camera and PhysicsRaycaster from NaturalHandPointerTracker
		private Dictionary<HandManager.HandType, Camera> m_Camera = new Dictionary<HandManager.HandType, Camera>() {
			{ HandManager.HandType.Left, null },
			{ HandManager.HandType.Right, null }
		};
		private Dictionary<HandManager.HandType, PhysicsRaycaster> m_PhysicsRaycaster = new Dictionary<HandManager.HandType, PhysicsRaycaster>() {
			{ HandManager.HandType.Left, null },
			{ HandManager.HandType.Right, null }
		};

		private void UpdateComponents(HandManager.HandType hand)
		{
			/// 1. Updates the Hand beam and pointer.
			GameObject new_beam = HandBeamProvider.Instance.GetHandBeam(hand);
			if (new_beam != null && !ReferenceEquals(beamObject[hand], new_beam))
			{
				beamObject[hand] = new_beam;
				m_HandBeam[hand] = beamObject[hand].GetComponent<HandBeam>();
			}
			if (beamObject[hand] == null)
				m_HandBeam[hand] = null;

			GameObject new_pointer = HandPointerProvider.Instance.GetHandPointer(hand);
			if (new_pointer != null && !ReferenceEquals(pointerObject[hand], new_pointer))
			{
				pointerObject[hand] = new_pointer;
				m_HandSpotPointer[hand] = pointerObject[hand].GetComponent<NaturalHandSpotPointer>();
			}
			if (pointerObject[hand] == null)
				m_HandSpotPointer[hand] = null;

			if (m_HandBeam[hand] == null || m_HandSpotPointer[hand] == null)
			{
				if (Log.gpl.Print)
				{
					if (m_HandBeam[hand] == null)
						Log.i(LOG_TAG, "ValidateParameters() No beam of " + hand, true);
					if (m_HandSpotPointer[hand] == null)
						Log.i(LOG_TAG, "ValidateParameters() No pointer of " + hand, true);
				}
			}

			/// 2. Updates the event camera and physics raycaster.
			if (s_HandPointerTracker[hand] != null)
			{
				if (m_Camera[hand] == null)
					m_Camera[hand] = s_HandPointerTracker[hand].GetComponent<Camera>();
				if (m_PhysicsRaycaster[hand] == null)
					m_PhysicsRaycaster[hand] = s_HandPointerTracker[hand].GetComponent<PhysicsRaycaster>();
			}

			if (m_Camera[hand] == null)
			{
				if (Log.gpl.Print)
					Log.e(LOG_TAG, "ValidateParameters() Forget to put Main Camera??");
			}
		}
		#endregion

		#region Update Beam and Pointer
		private Dictionary<HandManager.HandType, bool> isPinch = new Dictionary<HandManager.HandType, bool>() {
			{ HandManager.HandType.Left, false },
			{ HandManager.HandType.Right, false }
		};
		private const uint PINCH_FRAME_COUNT = 10;
		private Dictionary<HandManager.HandType, uint> pinchFrame = new Dictionary<HandManager.HandType, uint>() {
			{ HandManager.HandType.Left, 0 },
			{ HandManager.HandType.Right, 0 }
		};
		private Dictionary<HandManager.HandType, uint> unpinchFrame = new Dictionary<HandManager.HandType, uint>()
		{
			{ HandManager.HandType.Left, 0 },
			{ HandManager.HandType.Right, 0 }
		};
		private void LegalizeBeamPointerOnPinch(HandManager.HandType hand)
		{
			bool effective = false;
			/**
			 * Set the beam and pointer to effective when
			 * Not pinch currently and, 1 or 2 happens.
			 * 1. Focused hand is right and right pinch strength is enough.
			 * 2. Focused hand is left and left pinch strength is enough.
			 **/
			if (!isPinch[hand])
			{
				if (((hand == HandManager.HandType.Right) &&
					 ((m_HandMotion[HandManager.HandType.Right] == HandManager.HandMotion.Pinch) && (m_PinchStrength[HandManager.HandType.Right] >= m_PinchOnThreshold))
					) ||
					((hand == HandManager.HandType.Left) &&
					 ((m_HandMotion[HandManager.HandType.Left] == HandManager.HandMotion.Pinch) && (m_PinchStrength[HandManager.HandType.Left] >= m_PinchOnThreshold))
					)
				)
				{
					effective = true;
				}
			}
			if (effective)
			{
				pinchFrame[hand]++;
				if (pinchFrame[hand] > PINCH_FRAME_COUNT)
				{
					isPinch[hand] = true;
					if (m_HandBeam[hand] != null)
						m_HandBeam[hand].SetEffectiveBeam(true);
					if (m_HandSpotPointer[hand] != null)
						m_HandSpotPointer[hand].SetEffectivePointer(true);
					unpinchFrame[hand] = 0;
				}
			}

			bool uneffective = false;
			/**
			 * Set the beam and pointer to uneffective when
			 * Is pinching currently and, 1 or 2 happens.
			 * 1. Focused hand is right and, right gesture is not pinch or right pinch strength is not enough.
			 * 2. Focused hand is left and, left gesture is not pinch or left pinch strength is not enough.
			 **/
			if (isPinch[hand])
			{
				if (((hand == HandManager.HandType.Right) &&
					 ((m_HandMotion[HandManager.HandType.Right] != HandManager.HandMotion.Pinch) || (m_PinchStrength[HandManager.HandType.Right] < m_PinchOffThreshold))
					) ||
					((hand == HandManager.HandType.Left) &&
					 ((m_HandMotion[HandManager.HandType.Left] != HandManager.HandMotion.Pinch) || (m_PinchStrength[HandManager.HandType.Left] < m_PinchOffThreshold))
					)
				)
				{
					uneffective = true;
				}
			}
			if (uneffective)
			{
				unpinchFrame[hand]++;
				if (unpinchFrame[hand] > PINCH_FRAME_COUNT)
				{
					isPinch[hand] = false;
					if (m_HandBeam[hand] != null)
						m_HandBeam[hand].SetEffectiveBeam(false);
					if (m_HandSpotPointer[hand] != null)
						m_HandSpotPointer[hand].SetEffectivePointer(false);
					pinchFrame[hand] = 0;
				}
			}
		}
		#endregion

		#region Pinch Selector Control
		private Quaternion toRotation = Quaternion.identity;
		private void RotateSelector(HandManager.HandType hand, GameObject selector, Quaternion fromRotation)
		{
			if (HandManager.Instance == null)
				return;

			Quaternion rot = Quaternion.identity;
			if (hand == HandManager.HandType.Right)
			{
				if (HandManager.Instance.GetJointRotation(HandManager.HandJoint.Wrist, ref rot, false))
					selector.transform.rotation *= (rot * Quaternion.Inverse(fromRotation)); // *= toRotation
			}
			if (hand == HandManager.HandType.Left)
			{
				if (HandManager.Instance.GetJointRotation(HandManager.HandJoint.Wrist, ref rot, true))
					selector.transform.rotation *= (rot * Quaternion.Inverse(fromRotation)); // *= toRotation
			}
		}

		Vector3 headPos = Vector3.zero;
		Vector3 wristPos = Vector3.zero, thumbTipPos = Vector3.zero;

		[SerializeField]
		private float m_PinchOffsetX = 0.12f;
		public float PinchOffsetX { get { return m_PinchOffsetX; } set { m_PinchOffsetX = value; } }
		[SerializeField]
		private float m_PinchOffsetY = 0.08f;
		public float PinchOffsetY { get { return m_PinchOffsetY; } set { m_PinchOffsetY = value; } }
		private void UpdatePinchOriginDirectionVIU(HandManager.HandType hand)
		{
			if (HandManager.Instance == null) return;

			HandManager.Instance.GetJointPosition(HandManager.HandJoint.Wrist,			ref wristPos,			hand);
			HandManager.Instance.GetJointPosition(HandManager.HandJoint.Thumb_Tip,		ref thumbTipPos,		hand);

			Vector3 pinchOrigin = wristPos;
			pinchOrigin.y += m_PinchOffsetY;
			if (hand == HandManager.HandType.Right)
				pinchOrigin.x -= m_PinchOffsetX;
			else
				pinchOrigin.x += m_PinchOffsetX;
			m_PinchOrigin[hand] = pinchOrigin;

			headPos = Camera.main != null ? Camera.main.gameObject.transform.position : Vector3.zero;
			m_PinchDirection[hand] = m_PinchOrigin[hand] - headPos;
		}
		private void UpdatePinchOriginDirection(HandManager.HandType hand)
		{
			Vector3 origin = m_PinchOrigin[hand];
			HandManager.Instance.GetPinchOrigin(ref origin, hand == HandManager.HandType.Left ? true : false);
			m_PinchOrigin[hand] = origin;

			Vector3 direction = m_PinchDirection[hand];
			HandManager.Instance.GetPinchDirection(ref direction, hand == HandManager.HandType.Left ? true : false);
			m_PinchDirection[hand] = direction;
		}
		#endregion

		#region Raycast
		private Dictionary<HandManager.HandType, PointerEventData> mPointerEventData = new Dictionary<HandManager.HandType, PointerEventData>() {
			{ HandManager.HandType.Left, null },
			{ HandManager.HandType.Right, null },
		};
		private void ResetPointerEventData(HandManager.HandType hand)
		{
			if (m_Camera[hand] == null)
				return;

			if (mPointerEventData[hand] == null)
			{
				mPointerEventData[hand] = new PointerEventData(eventSystem);
				mPointerEventData[hand].pointerCurrentRaycast = new RaycastResult();
			}

			mPointerEventData[hand].Reset();
			mPointerEventData[hand].position = new Vector2(0.5f * m_Camera[hand].pixelWidth, 0.5f * m_Camera[hand].pixelHeight); // center of screen
			firstRaycastResult.Clear();
			mPointerEventData[hand].pointerCurrentRaycast = firstRaycastResult;
		}

		private GameObject prevRaycastedObject = null;
		private GameObject GetRaycastedObject(HandManager.HandType hand)
		{
			if (mPointerEventData[hand] == null)
				return null;

			return mPointerEventData[hand].pointerCurrentRaycast.gameObject;
		}

		private Vector3 GetIntersectionPosition(HandManager.HandType hand, RaycastResult raycastResult)
		{
			if (m_Camera[hand] == null)
				return Vector3.zero;

			float intersectionDistance = raycastResult.distance + m_Camera[hand].nearClipPlane;
			Vector3 intersectionPosition = m_Camera[hand].transform.forward * intersectionDistance + m_Camera[hand].transform.position;
			return intersectionPosition;
		}

		private List<RaycastResult> GetResultList(List<RaycastResult> originList)
		{
			List<RaycastResult> result_list = new List<RaycastResult>();
			for (int i = 0; i < originList.Count; i++)
			{
				if (originList[i].gameObject != null)
					result_list.Add(originList[i]);
			}
			return result_list;
		}

		private RaycastResult SelectRaycastResult(HandManager.HandType hand, RaycastResult currResult, RaycastResult nextResult)
		{
			if (currResult.gameObject == null)
				return nextResult;
			if (nextResult.gameObject == null)
				return currResult;

			if (currResult.worldPosition == Vector3.zero)
				currResult.worldPosition = GetIntersectionPosition(hand, currResult);

			float curr_distance = (float)Math.Round(Mathf.Abs(currResult.worldPosition.z - currResult.module.eventCamera.transform.position.z), 3);

			if (nextResult.worldPosition == Vector3.zero)
				nextResult.worldPosition = GetIntersectionPosition(hand, nextResult);

			float next_distance = (float)Math.Round(Mathf.Abs(nextResult.worldPosition.z - currResult.module.eventCamera.transform.position.z), 3);

			// 1. Check the "Order in Layer" of the Canvas.
			if (nextResult.sortingOrder > currResult.sortingOrder)
				return nextResult;

			// 2. Check the distance.
			if (next_distance > curr_distance)
				return currResult;

			if (next_distance < curr_distance)
			{
				/*DEBUG("SelectRaycastResult() "
					+ nextResult.gameObject.name + ", position: " + nextResult.worldPosition
					+ ", distance: " + next_distance
					+ " is smaller than "
					+ currResult.gameObject.name + ", position: " + currResult.worldPosition
					+ ", distance: " + curr_distance
					);*/

				return nextResult;
			}

			return currResult;
		}

		private RaycastResult m_Result = new RaycastResult();
		private RaycastResult FindFirstResult(HandManager.HandType hand, List<RaycastResult> resultList)
		{
			m_Result = resultList[0];
			for (int i = 1; i < resultList.Count; i++)
				m_Result = SelectRaycastResult(hand, m_Result, resultList[i]);
			return m_Result;
		}

		private RaycastResult firstRaycastResult = new RaycastResult();
		private GraphicRaycaster[] graphic_raycasters;
		private Dictionary<HandManager.HandType, List<RaycastResult>> graphicRaycastResults = new Dictionary<HandManager.HandType, List<RaycastResult>>() {
			{ HandManager.HandType.Left, new List<RaycastResult>() },
			{ HandManager.HandType.Right, new List<RaycastResult>() },
		};
		private Dictionary<HandManager.HandType, List<GameObject>> graphicRaycastObjects = new Dictionary<HandManager.HandType, List<GameObject>>() {
			{ HandManager.HandType.Left, new List<GameObject>() },
			{ HandManager.HandType.Right, new List<GameObject>() },
		};
		private Dictionary<HandManager.HandType, List<GameObject>> preGraphicRaycastObjects = new Dictionary<HandManager.HandType, List<GameObject>>() {
			{ HandManager.HandType.Left, new List<GameObject>() },
			{ HandManager.HandType.Right, new List<GameObject>() },
		};
		private GameObject raycastTarget = null;
		private void GraphicRaycast(HandManager.HandType hand)
		{
			if (m_Camera[hand] == null)
				return;

			// Find GraphicRaycaster
			graphic_raycasters = FindObjectsOfType<GraphicRaycaster>();

			graphicRaycastResults[hand].Clear();
			graphicRaycastObjects[hand].Clear();

			for (int i = 0; i < graphic_raycasters.Length; i++)
			{
				// Ignore the Blocker of Dropdown.
				if (graphic_raycasters[i].gameObject.name.Equals("Blocker"))
					continue;

				// Change the Canvas' event camera.
				if (graphic_raycasters[i].gameObject.GetComponent<Canvas>() != null)
					graphic_raycasters[i].gameObject.GetComponent<Canvas>().worldCamera = m_Camera[hand];
				else
					continue;

				// Raycasting.
				graphic_raycasters[i].Raycast(mPointerEventData[hand], graphicRaycastResults[hand]);
				graphicRaycastResults[hand] = GetResultList(graphicRaycastResults[hand]);
				if (graphicRaycastResults[hand].Count == 0)
					continue;

				// Get the results.
				firstRaycastResult = FindFirstResult(hand, graphicRaycastResults[hand]);

				//DEBUG ("GraphicRaycast() device: " + event_controller.device + ", camera: " + firstRaycastResult.module.eventCamera + ", first result = " + firstRaycastResult);
				mPointerEventData[hand].pointerCurrentRaycast = SelectRaycastResult(hand, mPointerEventData[hand].pointerCurrentRaycast, firstRaycastResult);
				graphicRaycastResults[hand].Clear();
			} // for (int i = 0; i < graphic_raycasters.Length; i++)

			raycastTarget = mPointerEventData[hand].pointerCurrentRaycast.gameObject;
			while (raycastTarget != null)
			{
				graphicRaycastObjects[hand].Add(raycastTarget);
				raycastTarget = (raycastTarget.transform.parent != null ? raycastTarget.transform.parent.gameObject : null);
			}
		}

		private Dictionary<HandManager.HandType, List<RaycastResult>> physicsRaycastResults = new Dictionary<HandManager.HandType, List<RaycastResult>>() {
			{ HandManager.HandType.Left, new List<RaycastResult>() },
			{ HandManager.HandType.Right, new List<RaycastResult>() },
		};
		private Dictionary<HandManager.HandType, List<GameObject>> physicsRaycastObjects = new Dictionary<HandManager.HandType, List<GameObject>>() {
			{ HandManager.HandType.Left, new List<GameObject>() },
			{ HandManager.HandType.Right, new List<GameObject>() },
		};
		private Dictionary<HandManager.HandType, List<GameObject>> prePhysicsRaycastObjects = new Dictionary<HandManager.HandType, List<GameObject>>() {
			{ HandManager.HandType.Left, new List<GameObject>() },
			{ HandManager.HandType.Right, new List<GameObject>() },
		};
		private void PhysicsRaycast(HandManager.HandType hand)
		{
			if (m_Camera[hand] == null || m_PhysicsRaycaster[hand] == null)
				return;

			// Clear cache values.
			physicsRaycastResults[hand].Clear();
			physicsRaycastObjects[hand].Clear();

			// Raycasting.
			m_PhysicsRaycaster[hand].Raycast(mPointerEventData[hand], physicsRaycastResults[hand]);
			if (physicsRaycastResults[hand].Count == 0)
				return;

			for (int i = 0; i < physicsRaycastResults[hand].Count; i++)
			{
				// Ignore the GameObject with JointPose component.
				if (physicsRaycastResults[hand][i].gameObject.GetComponent<JointPose>() != null)
					continue;

				physicsRaycastObjects[hand].Add(physicsRaycastResults[hand][i].gameObject);
			}

			firstRaycastResult = FindFirstRaycast(physicsRaycastResults[hand]);

			//DEBUG ("PhysicsRaycast() device: " + event_controller.device + ", camera: " + firstRaycastResult.module.eventCamera + ", first result = " + firstRaycastResult);
			mPointerEventData[hand].pointerCurrentRaycast = SelectRaycastResult(hand, mPointerEventData[hand].pointerCurrentRaycast, firstRaycastResult);
		}
		#endregion

		#region Event Handling
		private void OnGraphicPointerEnterExit(HandManager.HandType hand)
		{
			if (graphicRaycastObjects[hand].Count != 0)
			{
				for (int i = 0; i < graphicRaycastObjects[hand].Count; i++)
				{
					if (graphicRaycastObjects[hand][i] != null && !preGraphicRaycastObjects[hand].Contains(graphicRaycastObjects[hand][i]))
					{
						ExecuteEvents.Execute(graphicRaycastObjects[hand][i], mPointerEventData[hand], ExecuteEvents.pointerEnterHandler);
						DEBUG("OnGraphicPointerEnterExit() enter: " + graphicRaycastObjects[hand][i]);
					}
				}
			}

			if (preGraphicRaycastObjects[hand].Count != 0)
			{
				for (int i = 0; i < preGraphicRaycastObjects[hand].Count; i++)
				{
					if (preGraphicRaycastObjects[hand][i] != null && !graphicRaycastObjects[hand].Contains(preGraphicRaycastObjects[hand][i]))
					{
						ExecuteEvents.Execute(preGraphicRaycastObjects[hand][i], mPointerEventData[hand], ExecuteEvents.pointerExitHandler);
						DEBUG("OnGraphicPointerEnterExit() exit: " + preGraphicRaycastObjects[hand][i]);
					}
				}
			}

			CopyList(graphicRaycastObjects[hand], preGraphicRaycastObjects[hand]);
		}

		private void OnPhysicsPointerEnterExit(HandManager.HandType hand)
		{
			if (physicsRaycastObjects[hand].Count != 0)
			{
				for (int i = 0; i < physicsRaycastObjects[hand].Count; i++)
				{
					if (physicsRaycastObjects[hand][i] != null && !prePhysicsRaycastObjects[hand].Contains(physicsRaycastObjects[hand][i]))
					{
						ExecuteEvents.Execute(physicsRaycastObjects[hand][i], mPointerEventData[hand], ExecuteEvents.pointerEnterHandler);
						DEBUG("OnPhysicsPointerEnterExit() enter: " + physicsRaycastObjects[hand][i]);
					}
				}
			}

			if (prePhysicsRaycastObjects[hand].Count != 0)
			{
				for (int i = 0; i < prePhysicsRaycastObjects[hand].Count; i++)
				{
					if (prePhysicsRaycastObjects[hand][i] != null && !physicsRaycastObjects[hand].Contains(prePhysicsRaycastObjects[hand][i]))
					{
						ExecuteEvents.Execute(prePhysicsRaycastObjects[hand][i], mPointerEventData[hand], ExecuteEvents.pointerExitHandler);
						DEBUG("OnPhysicsPointerEnterExit() exit: " + prePhysicsRaycastObjects[hand][i]);
					}
				}
			}

			CopyList(physicsRaycastObjects[hand], prePhysicsRaycastObjects[hand]);
		}

		private void OnPointerHover(HandManager.HandType hand)
		{
			GameObject go = GetRaycastedObject(hand);
			if (go != null && prevRaycastedObject == go)
				ExecuteEvents.ExecuteHierarchy(go, mPointerEventData[hand], PointerEvents.pointerHoverHandler);
		}

		private void OnPointerDown(HandManager.HandType hand)
		{
			GameObject go = GetRaycastedObject(hand);
			if (go == null) return;

			// Send a Pointer Down event. If not received, get handler of Pointer Click.
			mPointerEventData[hand].pressPosition = mPointerEventData[hand].position;
			mPointerEventData[hand].pointerPressRaycast = mPointerEventData[hand].pointerCurrentRaycast;
			mPointerEventData[hand].pointerPress =
				ExecuteEvents.ExecuteHierarchy(go, mPointerEventData[hand], ExecuteEvents.pointerDownHandler)
				?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

			DEBUG("OnPointerDown() send Pointer Down to " + mPointerEventData[hand].pointerPress + ", current GameObject is " + go);

			// If Drag Handler exists, send initializePotentialDrag event.
			mPointerEventData[hand].pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
			if (mPointerEventData[hand].pointerDrag != null)
			{
				DEBUG("OnPointerDown() send initializePotentialDrag to " + mPointerEventData[hand].pointerDrag + ", current GameObject is " + go);
				ExecuteEvents.Execute(mPointerEventData[hand].pointerDrag, mPointerEventData[hand], ExecuteEvents.initializePotentialDrag);
			}

			// Press happened (even not handled) object.
			mPointerEventData[hand].rawPointerPress = go;
			// Allow to send Pointer Click event
			mPointerEventData[hand].eligibleForClick = true;
			// Reset the screen position of press, can be used to estimate move distance
			mPointerEventData[hand].delta = Vector2.zero;
			// Current Down, reset drag state
			mPointerEventData[hand].dragging = false;
			mPointerEventData[hand].useDragThreshold = true;
			// Record the count of Pointer Click should be processed, clean when Click event is sent.
			mPointerEventData[hand].clickCount = 1;
			// Set clickTime to current time of Pointer Down instead of Pointer Click
			// since Down & Up event should not be sent too closely. (< CLICK_TIME)
			mPointerEventData[hand].clickTime = Time.unscaledTime;
		}

		private void OnPointerDrag(HandManager.HandType hand)
		{
			if (Time.unscaledTime - mPointerEventData[hand].clickTime < m_PinchTimeToDrag)
				return;
			if (mPointerEventData[hand].pointerDrag == null)
				return;

			if (!mPointerEventData[hand].dragging)
			{
				DEBUG("OnPointerDrag() send BeginDrag to " + mPointerEventData[hand].pointerDrag);
				ExecuteEvents.Execute(mPointerEventData[hand].pointerDrag, mPointerEventData[hand], ExecuteEvents.beginDragHandler);
				mPointerEventData[hand].dragging = true;
			}
			else
			{
				ExecuteEvents.Execute(mPointerEventData[hand].pointerDrag, mPointerEventData[hand], ExecuteEvents.dragHandler);
			}
		}

		private void OnPointerUp(HandManager.HandType hand)
		{
			GameObject go = GetRaycastedObject(hand);
			// The "go" may be different with mPointerEventData.pointerDrag so we don't check null.

			if (mPointerEventData[hand].pointerPress != null)
			{
				// In the frame of button is pressed -> unpressed, send Pointer Up
				DEBUG("OnPointerUp() send Pointer Up to " + mPointerEventData[hand].pointerPress);
				ExecuteEvents.Execute(mPointerEventData[hand].pointerPress, mPointerEventData[hand], ExecuteEvents.pointerUpHandler);
			}

			if (mPointerEventData[hand].eligibleForClick)
			{
				GameObject click_object = ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);
				if (click_object != null)
				{
					if (click_object == mPointerEventData[hand].pointerPress)
					{
						// In the frame of button from being pressed to unpressed, send Pointer Click if Click is pending.
						DEBUG("OnPointerUp() send Pointer Click to " + mPointerEventData[hand].pointerPress);
						ExecuteEvents.Execute(mPointerEventData[hand].pointerPress, mPointerEventData[hand], ExecuteEvents.pointerClickHandler);
					}
					else
					{
						DEBUG("OnTriggerUpMouse() pointer down object " + mPointerEventData[hand].pointerPress + " is different with click object " + click_object);
					}
				}

				if (mPointerEventData[hand].dragging)
				{
					GameObject drop_object = ExecuteEvents.GetEventHandler<IDropHandler>(go);
					if (drop_object == mPointerEventData[hand].pointerDrag)
					{
						// In the frame of button from being pressed to unpressed, send Drop and EndDrag if dragging.
						DEBUG("OnPointerUp() send Pointer Drop to " + mPointerEventData[hand].pointerDrag);
						ExecuteEvents.Execute(mPointerEventData[hand].pointerDrag, mPointerEventData[hand], ExecuteEvents.dropHandler);
					}

					DEBUG("OnPointerUp() send Pointer endDrag to " + mPointerEventData[hand].pointerDrag);
					ExecuteEvents.Execute(mPointerEventData[hand].pointerDrag, mPointerEventData[hand], ExecuteEvents.endDragHandler);

					mPointerEventData[hand].pointerDrag = null;
					mPointerEventData[hand].dragging = false;
				}
			}

			// Down object.
			mPointerEventData[hand].pointerPress = null;
			// Press happened (even not handled) object.
			mPointerEventData[hand].rawPointerPress = null;
			// Clear pending state.
			mPointerEventData[hand].eligibleForClick = false;
			// Click event is sent, clear count.
			mPointerEventData[hand].clickCount = 0;
			// Up event is sent, clear the time limitation of Down event.
			mPointerEventData[hand].clickTime = 0;
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
