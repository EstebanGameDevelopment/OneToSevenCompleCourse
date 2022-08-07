// "Wave SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;

namespace Wave.Essence.Interaction.Mode.Demo
{
	[DisallowMultipleComponent]
	sealed class HandPanelCanvas : MonoBehaviour
	{
		private Canvas m_Canvas = null;
		private void Start()
		{
			m_Canvas = GetComponent<Canvas>();
		}
		void Update()
		{
			if (m_Canvas != null)
				m_Canvas.enabled = (ClientInterface.InteractionMode == XR_InteractionMode.Hand);
		}
	}
}
