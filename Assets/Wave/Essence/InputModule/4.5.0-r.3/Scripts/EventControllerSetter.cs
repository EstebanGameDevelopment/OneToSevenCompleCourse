using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Wave.Native;

namespace Wave.Essence.InputModule
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera), typeof(PhysicsRaycaster))]
	public sealed class EventControllerSetter : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.InputModule.EventControllerSetter";
		private void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, m_ControllerType + " " + msg, true);
		}

		[SerializeField]
		private XR_Hand m_ControllerType = XR_Hand.Dominant;
		public XR_Hand ControllerType { get { return m_ControllerType; } set { m_ControllerType = value; } }

		private GameObject beamObject = null;
		private ControllerBeam m_Beam = null;
		private GameObject pointerObject = null;
		private ControllerPointer m_Pointer = null;

		private List<GameObject> children = new List<GameObject>();
		private int childrenCount = 0;
		private List<bool> childrenStates = new List<bool>();
		private void CheckChildrenObjects()
		{
			if (childrenCount != transform.childCount)
			{
				DEBUG("CheckChildrenObjects() Children count old: " + childrenCount + ", new: " + transform.childCount);
				childrenCount = transform.childCount;
				children.Clear();
				childrenStates.Clear();
				for (int i = 0; i < childrenCount; i++)
				{
					children.Add(transform.GetChild(i).gameObject);
					childrenStates.Add(transform.GetChild(i).gameObject.activeSelf);
					DEBUG("CheckChildrenObjects() " + gameObject.name + " has child: " + children[i].name + ", active? " + childrenStates[i]);
				}
			}
		}
		private void ForceActivateTargetObjects(bool active)
		{
			for (int i = 0; i < children.Count; i++)
			{
				if (children[i] == null)
					continue;

				if (childrenStates[i])
				{
					DEBUG("ForceActivateTargetObjects() " + (active ? "Activate" : "Deactivate") + " " + children[i].name);
					children[i].SetActive(active);
				}
			}
		}

		private bool hasFocus = false;
		//private bool m_ControllerActive = true;

		private bool mEnabled = false;
		void OnEnable()
		{
			if (!mEnabled)
			{
				// Add a beam.
				beamObject = new GameObject(m_ControllerType.ToString() + "Beam");
				beamObject.transform.SetParent(transform, false);
				beamObject.transform.localPosition = Vector3.zero;
				beamObject.transform.localRotation = Quaternion.identity;
				beamObject.SetActive(false);
				m_Beam = beamObject.AddComponent<ControllerBeam>();
				m_Beam.BeamType = m_ControllerType;
				beamObject.SetActive(true);

				// Add a pointer.
				pointerObject = new GameObject(m_ControllerType.ToString() + "Pointer");
				pointerObject.transform.SetParent(transform, false);
				pointerObject.transform.localPosition = Vector3.zero;
				pointerObject.transform.localRotation = Quaternion.identity;
				pointerObject.SetActive(false);
				m_Pointer = pointerObject.AddComponent<ControllerPointer>();
				m_Pointer.PointerType = m_ControllerType;
				pointerObject.SetActive(true);

				hasFocus = ClientInterface.IsFocused;

				if (ControllerInputSwitch.Instance != null)
					Log.i(LOG_TAG, "OnEnable() Loaded ControllerInputSwitch.");

				EventControllerProvider.Instance.SetEventController(m_ControllerType, gameObject);

				mEnabled = true;
			}
		}

		void Start()
		{
			GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
			GetComponent<Camera>().enabled = false;
			DEBUG("Start() " + gameObject.name);
		}
	}
}
