using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class HUD : MonoBehaviour, IHUD, ICommanderHUD
    {
        public TextMeshProUGUI DisplayScore;
        public TextMeshProUGUI DisplayLife;
        public TextMeshProUGUI DisplayTotalAmmo;
        public TextMeshProUGUI DisplayDangerInfo;
        public TextMeshProUGUI DisplayNPCInfo;

        public TextMeshProUGUI DisplayNameSelected;
        public TMP_Dropdown CommanderOptions;
        public TextMeshProUGUI DisplayCommandSelected;
        public TextMeshProUGUI DisplayCommandInstructions;


        void Start()
        {
            SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_HUD_HAS_STARTED, this);
#if !ENABLE_MOBILE
            this.gameObject.SetActive(false);
#endif

            SystemEventController.Instance.Event += OnSystemEvent;
        }

        void OnDestroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
        }

        private void OnSystemEvent(string _nameEvent, object[] _parameters)
        {
            if (_nameEvent == SystemEventGameController.EVENT_MOBILE_FIX_MOVEMENT)
            {
                if ((bool)_parameters[0])
                {
                    DisplayScore.gameObject.SetActive(false);
                    DisplayLife.gameObject.SetActive(false);

                    DisplayNameSelected.gameObject.SetActive(true);
                    CommanderOptions.gameObject.SetActive(true);
                    DisplayCommandSelected.gameObject.SetActive(true);
                    DisplayCommandInstructions.gameObject.SetActive(true);
                }
                else
                {
                    DisplayScore.gameObject.SetActive(true);
                    DisplayLife.gameObject.SetActive(true);

                    DisplayNameSelected.gameObject.SetActive(false);
                    CommanderOptions.gameObject.SetActive(false);
                    DisplayCommandSelected.gameObject.SetActive(false);
                    DisplayCommandInstructions.gameObject.SetActive(false);
                }
            }
        }

        public void ActivateHUD(bool _activate)
        {
            this.gameObject.SetActive(_activate);
        }

        public void UpdateDisplayScore(int _score)
        {
            if (DisplayScore != null) DisplayScore.text = "Score = " + _score.ToString();
        }

        public void UpdateDisplayLife(int _life)
        {
            if (DisplayLife != null) DisplayLife.text = "Life = " + _life.ToString();
        }

        public void UpdateDisplayTotalAmmo(int _ammo)
        {
            if (DisplayTotalAmmo != null) DisplayTotalAmmo.text = "Ammo = " + _ammo.ToString();
        }

        public void UpdateDisplayDangerInfo(string _message)
        {
            if (DisplayDangerInfo != null) DisplayDangerInfo.text = _message;
        }

        public void UpdateDisplayNPCInfo(string _message)
        {
            if (DisplayNPCInfo != null) DisplayNPCInfo.text = _message;
        }

        public void InitializeDropDown(List<string> _commandOptions, UnityAction<int> _callback)
        {
            if (CommanderOptions != null)
            {
                CommanderOptions.ClearOptions();
                CommanderOptions.AddOptions(_commandOptions);
                CommanderOptions.onValueChanged.AddListener(_callback);
            }
        }

        public void SetValueDropdown(int _value)
        {
            if (CommanderOptions != null)
            {
                CommanderOptions.value = _value;
            }
        }

        public void SetValueNameSelected(string _name)
        {
            if (DisplayNameSelected != null)
            {
                DisplayNameSelected.text = _name;
                if (_name.Length > 0)
                {
                    DisplayNameSelected.gameObject.SetActive(true);
                }
                else
                {
                    DisplayNameSelected.gameObject.SetActive(false);
                }
            }
        }

        public void SetValueActionSelected(string _action)
        {
            if (DisplayCommandSelected != null)
            {
                DisplayCommandSelected.text = _action;
                if (_action.Length > 0)
                {
                    DisplayCommandSelected.gameObject.SetActive(true);
                }
                else
                {
                    DisplayCommandSelected.gameObject.SetActive(false);
                }
            }
        }

        public void ResetAll(int _value)
        {
            CommanderOptions.value = _value;
            CommanderOptions.gameObject.SetActive(false);
            DisplayNameSelected.text = "";
            DisplayCommandSelected.text = "";
            DisplayCommandInstructions.text = "";
        }

        public void ResetWithUnitSelected()
        {
            DisplayCommandSelected.gameObject.SetActive(false);
            DisplayCommandInstructions.gameObject.SetActive(false);
            CommanderOptions.gameObject.SetActive(true);
        }


        public void ResettWithSetData(string _instructions)
        {
            DisplayCommandSelected.gameObject.SetActive(true);
            DisplayCommandInstructions.gameObject.SetActive(true);
            CommanderOptions.gameObject.SetActive(false);
            DisplayCommandInstructions.text = _instructions;
        }

        public void ResetWithAssignAction()
        {
            DisplayCommandSelected.gameObject.SetActive(true);
            DisplayCommandInstructions.gameObject.SetActive(true);
            CommanderOptions.gameObject.SetActive(false);
        }

        public string GetValueNameSelected()
        {
            return DisplayNameSelected.text;
        }

    }
}