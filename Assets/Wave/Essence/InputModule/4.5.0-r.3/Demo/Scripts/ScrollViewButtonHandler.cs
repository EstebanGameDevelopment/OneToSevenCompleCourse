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

namespace Wave.Essence.InputModule.Demo
{
	[DisallowMultipleComponent]
	sealed class ScrollViewButtonHandler : MonoBehaviour
	{
		// Controller mode buttons
		public GameObject FlexibleModeButton = null;
		public GameObject FixedModeButton = null;
		public GameObject MouseModeButton = null;

		// Gaze mode buttons
		public GameObject TimerGazeButton = null;
		public GameObject ButtonGazeButton = null;
		public GameObject TimerButtonGazeButton = null;

		private ControllerInputModule m_ControllerInputModule = null;
		private GazeInputModule m_GazeInputModule = null;
		private void Update()
		{
			if (m_ControllerInputModule == null && EventSystem.current != null)
				m_ControllerInputModule = EventSystem.current.gameObject.GetComponent<ControllerInputModule>();
			if (m_GazeInputModule == null && EventSystem.current != null)
				m_GazeInputModule = EventSystem.current.gameObject.GetComponent<GazeInputModule>();

			if (ClientInterface.InteractionMode == XR_InteractionMode.Controller)
			{
				if (FlexibleModeButton != null)
					FlexibleModeButton.SetActive(true);
				if (FixedModeButton != null)
					FixedModeButton.SetActive(true);
				if (MouseModeButton != null)
					MouseModeButton.SetActive(true);
				if (TimerGazeButton != null)
					TimerGazeButton.SetActive(false);
				if (ButtonGazeButton != null)
					ButtonGazeButton.SetActive(false);
				if (TimerButtonGazeButton != null)
					TimerButtonGazeButton.SetActive(false);
			}
			else if (ClientInterface.InteractionMode == XR_InteractionMode.Gaze)
			{
				if (FlexibleModeButton != null)
					FlexibleModeButton.SetActive(false);
				if (FixedModeButton != null)
					FixedModeButton.SetActive(false);
				if (MouseModeButton != null)
					MouseModeButton.SetActive(false);
				if (TimerGazeButton != null)
					TimerGazeButton.SetActive(true);
				if (ButtonGazeButton != null)
					ButtonGazeButton.SetActive(true);
				if (TimerButtonGazeButton != null)
					TimerButtonGazeButton.SetActive(true);
			}
			else
			{
				if (FlexibleModeButton != null)
					FlexibleModeButton.SetActive(false);
				if (FixedModeButton != null)
					FixedModeButton.SetActive(false);
				if (MouseModeButton != null)
					MouseModeButton.SetActive(false);
				if (TimerGazeButton != null)
					TimerGazeButton.SetActive(false);
				if (ButtonGazeButton != null)
					ButtonGazeButton.SetActive(false);
				if (TimerButtonGazeButton != null)
					TimerButtonGazeButton.SetActive(false);
			}
		}

		public void OnFlexibleMode()
		{
			if (m_ControllerInputModule != null)
				m_ControllerInputModule.BeamMode = ControllerInputModule.BeamModes.Flexible;
		}

		public void OnFixedMode()
		{
			if (m_ControllerInputModule != null)
				m_ControllerInputModule.BeamMode = ControllerInputModule.BeamModes.Fixed;
		}

		public void OnMouseMode()
		{
			if (m_ControllerInputModule != null)
				m_ControllerInputModule.BeamMode = ControllerInputModule.BeamModes.Mouse;
		}

		public void OnTimerOnly()
		{
			if (m_GazeInputModule != null)
			{
				m_GazeInputModule.TimerControl = true;
				m_GazeInputModule.ButtonControl = false;
			}
		}

		public void OnButtonOnly()
		{
			if (m_GazeInputModule != null)
			{
				m_GazeInputModule.TimerControl = false;
				m_GazeInputModule.ButtonControl = true;
			}
		}

		public void OnTimerAndButton()
		{
			if (m_GazeInputModule != null)
			{
				m_GazeInputModule.TimerControl = true;
				m_GazeInputModule.ButtonControl = true;
			}
		}
	}
}
