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
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wave.Native;
using Wave.Essence.Hand;
using Wave.Essence.InputModule;

namespace Wave.Essence.Interaction.Mode.Demo
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Dropdown))]
	sealed class DropdownHandler : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Interaction.Demo.DropdownHandler";
		void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}

		private HandInputModule inputModule = null;
		void UpdatePinchStrength(int dropdownValue)
		{
			if (inputModule == null) { return; }
			if (dropdownValue < 0 || dropdownValue > 8)
			{
				inputModule.PinchOnThreshold = 0.7f;
				inputModule.PinchOffThreshold = 0.7f;
			}
			else
			{
				float f = Convert.ToSingle(dropdownValue);
				inputModule.PinchOnThreshold = (f + 1) / 10;
				inputModule.PinchOffThreshold = (f + 1) / 10;
			}
		}

		private Dropdown m_DropDown = null;
		private Text m_DropDownText = null;
		private string[] textStrings = new string[] {
			"0.1", "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9"
		};
		private Color m_Color = new Color(26, 7, 253, 255);

		void DropdownValueChanged(Dropdown change)
		{
			Log.d(LOG_TAG, "DropdownValueChanged(): " + change.value, true);
			UpdatePinchStrength(change.value);
		}
		void Start()
		{
			m_DropDown = GetComponent<Dropdown>();
			m_DropDown.onValueChanged.AddListener(
				delegate { DropdownValueChanged(m_DropDown); }
				);
			m_DropDownText = GetComponentInChildren<Text>();

			// clear all option item
			m_DropDown.options.Clear();

			// fill the dropdown menu OptionData
			foreach (string c in textStrings)
			{
				m_DropDown.options.Add(new Dropdown.OptionData() { text = c });
			}

			m_DropDown.value = 0;
		}
		private void FixedUpdate()
		{
			inputModule = EventSystem.current.gameObject.GetComponent<HandInputModule>();
			if (inputModule != null)
			{
				m_DropDown.value = (int)(Mathf.Round(inputModule.PinchOnThreshold * 10)) - 1;
			}
		}
		void Update()
		{
			if (m_DropDownText == null)
				return;

			m_DropDownText.text = textStrings[m_DropDown.value];

			Canvas dropdown_canvas = m_DropDown.gameObject.GetComponentInChildren<Canvas>();
			Button[] buttons = m_DropDown.gameObject.GetComponentsInChildren<Button>();
			if (dropdown_canvas != null)
			{
				foreach (Button btn in buttons)
				{
					Log.d(LOG_TAG, "set button " + btn.name + " color.", true);
					ColorBlock cb = btn.colors;
					cb.normalColor = this.m_Color;
					btn.colors = cb;
				}
			}
		}

		public void ChangeColor()
		{
			Image img = gameObject.GetComponent<Image>();
			img.color = img.color == Color.yellow ? Color.green : Color.yellow;
		}
	}
}
