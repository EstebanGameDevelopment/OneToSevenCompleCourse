using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class MobileInputGameManager : MobileInputManager
    {
        public enum JOYSTICK_MODE { NORMAL = 0, COMMANDER }

        public Button CommanderButton;
        public GameObject IconCommander;
        public GameObject IconOrders;

        private bool m_commanderActive = false;

        protected override void Start()
        {
            base.Start();

            if (CommanderButton != null) CommanderButton.onClick.AddListener(OnCommanderButton);

            SystemEventController.Instance.Event += OnSystemEvent;

            ChangeState((int)JOYSTICK_MODE.NORMAL);

            this.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
        }

        private void OnSystemEvent(string _nameEvent, object[] _parameters)
        {
            if ((_nameEvent == CameraController.EVENT_CAMERA_SWITCHED_TO_1ST_PERSON) || (_nameEvent == CameraController.EVENT_CAMERA_SWITCHED_TO_3RD_PERSON))
            {
                ChangeState((int)JOYSTICK_MODE.NORMAL);
            }
            if (_nameEvent == CameraController.EVENT_CAMERA_SWITCHED_TO_FREE_CAMERA)
            {
                ChangeState((int)JOYSTICK_MODE.COMMANDER);
            }
            if (_nameEvent == SystemEventGameController.EVENT_HUD_HAS_STARTED)
            {
                StartCoroutine(FixMobileOnHUDStarted());
            }
        }

        IEnumerator FixMobileOnHUDStarted()
        {
            yield return new WaitForSeconds(0.2f);

            SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_MOBILE_FIX_MOVEMENT, false);
        }

        private void OnCommanderButton()
        {
            m_commanderActive = !m_commanderActive;
            SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_MOBILE_FIX_MOVEMENT, m_commanderActive);
            if (m_commanderActive)
            {
                MoveJoystick.gameObject.SetActive(false);
                RotateJoystick.gameObject.SetActive(false);

                if (IconCommander != null) IconCommander.gameObject.SetActive(false);
                if (IconOrders != null) IconOrders.gameObject.SetActive(true);
            }
            else
            {
                MoveJoystick.gameObject.SetActive(true);
                RotateJoystick.gameObject.SetActive(true);

                if (IconCommander != null) IconCommander.gameObject.SetActive(true);
                if (IconOrders != null) IconOrders.gameObject.SetActive(false);
            }
        }

        protected override void ChangeState(int newState)
        {
            base.ChangeState(newState);

            switch ((JOYSTICK_MODE)m_state)
            {
                case JOYSTICK_MODE.NORMAL:
                    MoveJoystick.gameObject.SetActive(true);
                    RotateJoystick.gameObject.SetActive(true);
                    if (CommanderButton != null) CommanderButton.gameObject.SetActive(false);
                    ActionButton.gameObject.SetActive(true);
                    JumpButton.gameObject.SetActive(true);
                    SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_MOBILE_FIX_MOVEMENT, false);
                    break;

                case JOYSTICK_MODE.COMMANDER:
                    ActionButton.gameObject.SetActive(false);
                    JumpButton.gameObject.SetActive(false);
                    if (CommanderButton != null) CommanderButton.gameObject.SetActive(true);
                    if (IconCommander != null) IconCommander.gameObject.SetActive(true);
                    if (IconOrders != null) IconOrders.gameObject.SetActive(false);
                    break;
            }
        }
    }
}