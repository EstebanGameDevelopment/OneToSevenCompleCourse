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
using Wave.Native;

namespace Wave.Essence.InputModule
{
	public class RaycastResultProvider
	{
		private const string LOG_TAG = "Wave.RaycastResultProvider";
		private void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg);
		}

		class RaycastResultStorage
		{
			public XR_Device hand { get; set; }
			public GameObject resultObject { get; set; }
			public Vector3 worldPosition { get; set; }

			public RaycastResultStorage(XR_Device type, GameObject target, Vector3 position)
			{
				this.resultObject = null;
				this.worldPosition = Vector3.zero;
			}
		}

		private List<RaycastResultStorage> raycastResults = new List<RaycastResultStorage>();
		private XR_Device[] resultDeviceList = new XR_Device[] {
			XR_Device.Head,
			XR_Device.Dominant,
			XR_Device.NonDominant
		};

		private static RaycastResultProvider m_Instance = null;
		public static RaycastResultProvider Instance
		{
			get
			{
				if (m_Instance == null)
					m_Instance = new RaycastResultProvider();
				return m_Instance;
			}
		}

		private RaycastResultProvider()
		{
			for (int i = 0; i < resultDeviceList.Length; i++)
				raycastResults.Add(new RaycastResultStorage(resultDeviceList[i], null, Vector3.zero));
		}

		public void SetRaycastResult(XR_Hand hand, GameObject resultObject, Vector3 worldPosition)
		{
			SetRaycastResult((XR_Device)hand, resultObject, worldPosition);
		}
		public void SetRaycastResult(XR_Device device, GameObject resultObject, Vector3 worldPosition)
		{
			for (int i = 0; i < resultDeviceList.Length; i++)
			{
				if (resultDeviceList[i] == device)
				{
					raycastResults[i].resultObject = resultObject;
					raycastResults[i].worldPosition = worldPosition;
					break;
				}
			}
		}

		public GameObject GetRaycastResultObject(XR_Hand hand)
		{
			return GetRaycastResultObject((XR_Device)hand);
		}
		public GameObject GetRaycastResultObject(XR_Device device)
		{
			int index = 0;
			for (int i = 0; i < resultDeviceList.Length; i++)
			{
				if (resultDeviceList[i] == device)
				{
					index = i;
					break;
				}
			}

			return raycastResults[index].resultObject;
		}

		public Vector3 GetRaycastResultWorldPosition(XR_Hand hand)
		{
			return GetRaycastResultWorldPosition((XR_Device)hand);
		}
		public Vector3 GetRaycastResultWorldPosition(XR_Device device)
		{
			int index = 0;
			for (int i = 0; i < resultDeviceList.Length; i++)
			{
				if (resultDeviceList[i] == device)
				{
					index = i;
					break;
				}
			}

			return raycastResults[index].worldPosition;
		}
	}
}
