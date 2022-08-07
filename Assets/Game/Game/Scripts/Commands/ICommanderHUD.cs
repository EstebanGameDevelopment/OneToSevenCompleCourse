using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace YourVRExperience.Game
{
    public interface ICommanderHUD
    {
        void InitializeDropDown(List<string> _commandOptions, UnityAction<int> _callback);
        void SetValueDropdown(int _value);
        void SetValueNameSelected(string _name);
        void SetValueActionSelected(string _action);
        void ResetAll(int _value);
        void ResetWithUnitSelected();
        void ResettWithSetData(string _instructions);
        void ResetWithAssignAction();
        string GetValueNameSelected();
    }
}