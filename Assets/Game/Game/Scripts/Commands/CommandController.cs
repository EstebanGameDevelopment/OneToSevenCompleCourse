using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
    public class CommandController : StateMachine
    {
        public enum STATE_COMMANDER { SELECT_UNIT, SELECT_ACTION, SET_DATA, ASSIGN_ACTION }

        public enum COMMAND_TYPES { NONE = 0, MOVE_TO, LOOK_AT, ATTACK }

        private static CommandController _instance;
        public static CommandController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType<CommandController>();
                }
                return _instance;
            }
        }

        public Material MaterialSelection;

        private List<Avatar> m_selectedUnits = new List<Avatar>();
        private Avatar m_targetAvatar;
        private COMMAND_TYPES m_selectedCommandType;
        private Vector3 m_targetPosition;
        private bool m_isActive = false;
        private Vector3 m_anchorMouse;
        private Vector3 m_finalMouse;
        private GameObject m_planeAreaDetection;
        private ICommanderHUD m_commanderHUD;

#if ENABLE_MOBILE
    private bool m_isReadyForCommand = false;
#else
        private bool m_isReadyForCommand = true;
#endif

        void Awake()
        {
            SystemEventController.Instance.Event += OnSystemEvent;
        }

        void Start()
        {
            ChangeState((int)STATE_COMMANDER.SELECT_UNIT);
        }

        void OnDestroy()
        {
            if (SystemEventController.Instance != null)
            {
                SystemEventController.Instance.Event -= OnSystemEvent;
            }
        }

        private void OnSystemEvent(string _nameEvent, object[] _parameters)
        {
            if (_nameEvent == SystemEventGameController.EVENT_HUD_HAS_STARTED)
            {
                m_commanderHUD = (ICommanderHUD)_parameters[0];
                m_commanderHUD.InitializeDropDown(new List<string>() { "None", "Move_To", "Look_At", "Attack" }, OnCommandChanged);
                m_commanderHUD.ResetAll((int)COMMAND_TYPES.NONE);
            }

            if ((_nameEvent == CameraController.EVENT_CAMERA_SWITCHED_TO_1ST_PERSON) || (_nameEvent == CameraController.EVENT_CAMERA_SWITCHED_TO_3RD_PERSON))
            {
                m_isActive = false;
                UnselectAvatars();
                if (m_commanderHUD != null) m_commanderHUD.ResetAll((int)COMMAND_TYPES.NONE);
                ResetState();
            }
            if (_nameEvent == CameraController.EVENT_CAMERA_SWITCHED_TO_FREE_CAMERA)
            {
                m_isActive = true;
            }
            if (_nameEvent == SystemEventGameController.EVENT_MOBILE_FIX_MOVEMENT)
            {
                m_isReadyForCommand = (bool)_parameters[0];
                UnselectAvatars();
                if (m_commanderHUD != null) m_commanderHUD.ResetAll((int)COMMAND_TYPES.NONE);
            }
        }

        private void UnselectAvatars(bool _clear = false)
        {
            if (m_selectedUnits != null)
            {
                foreach (Avatar avatar in m_selectedUnits)
                {
                    if (avatar != null) avatar.Select(false);
                }
                if (_clear) m_selectedUnits.Clear();
            }
        }

        private void OnCommandChanged(int selectedCommand)
        {
            m_selectedCommandType = (COMMAND_TYPES)selectedCommand;
            Debug.Log("<color=red>Index of the selected command=" + m_selectedCommandType.ToString() + "</color>");
            if (m_selectedCommandType != COMMAND_TYPES.NONE)
            {
                ChangeState((int)STATE_COMMANDER.SET_DATA);
            }
        }

        private void ResetState()
        {
            if (m_planeAreaDetection != null)
            {
                GameObject.Destroy(m_planeAreaDetection);
            }
            foreach (Avatar unit in m_selectedUnits)
            {
                if (unit != null)
                {
                    unit.GetComponent<Avatar>().Select(false);
                }
            }
            m_commanderHUD.ResetAll((int)COMMAND_TYPES.NONE);
            m_selectedUnits.Clear();
            m_anchorMouse = Vector3.zero;
            m_finalMouse = Vector3.zero;
            ChangeState((int)STATE_COMMANDER.SELECT_UNIT);
        }

        private bool SelectSingleUnitByMouseRay()
        {
            GameObject avatarCollided = Utilities.GetCollisionMouseWithLayers(GameController.LAYER_PLAYER, GameController.LAYER_ENEMY, GameController.LAYER_NPC);
            if (avatarCollided != null)
            {
                Avatar selectedAvatar = avatarCollided.GetComponent<Avatar>();
                if (selectedAvatar != null)
                {
                    if (selectedAvatar.IsSelectable())
                    {
                        m_commanderHUD.SetValueNameSelected(selectedAvatar.gameObject.name);
                        selectedAvatar.Select(true);
                        m_selectedUnits.Add(selectedAvatar);
                        return true;
                    }
                }
            }
            return false;
        }

        private void CreatePlane(Vector3 _origin, Vector3 _target)
        {
            if (m_planeAreaDetection == null)
            {
                m_planeAreaDetection = GameObject.CreatePrimitive(PrimitiveType.Plane);
                m_planeAreaDetection.GetComponent<MeshCollider>().enabled = false;
                m_planeAreaDetection.AddComponent<PlaneFromPoly>();
                m_planeAreaDetection.transform.parent = this.transform;
            }

            m_planeAreaDetection.GetComponent<PlaneFromPoly>().Init(Utilities.GetBoundaryPoints(_origin, _target).ToArray(), MaterialSelection);
            m_planeAreaDetection.GetComponent<PlaneFromPoly>().LogicCentered(_origin, _target);
            Vector3 currentPos = m_planeAreaDetection.gameObject.transform.position;
            m_planeAreaDetection.gameObject.transform.position = new Vector3(currentPos.x, currentPos.y + 0.1f, currentPos.z);
        }

        private void CheckSelectedAvatars(Vector3 _origin, Vector3 _target)
        {
            m_commanderHUD.SetValueNameSelected("");

            foreach (IPlayer player in GameController.Instance.Players)
            {
                if (player != null)
                {
                    if (player.IsSelectable())
                    {
                        if (player.CheckInsideArea(_origin, _target))
                        {
                            player.Select(true);
                            m_commanderHUD.SetValueNameSelected(m_commanderHUD.GetValueNameSelected() + ((m_selectedUnits.Count > 0 ? "," : "") + player.GetGameObject().name));
                            m_selectedUnits.Add(player.GetGameObject().GetComponent<Avatar>());
                        }
                    }
                }
            }

            foreach (Enemy enemy in LevelController.Instance.Enemies)
            {
                if (enemy != null)
                {
                    if (enemy.IsSelectable())
                    {
                        if (enemy.CheckInsideArea(_origin, _target))
                        {
                            enemy.Select(true);
                            m_commanderHUD.SetValueNameSelected(m_commanderHUD.GetValueNameSelected() + ((m_selectedUnits.Count > 0 ? "," : "") + enemy.name));
                            m_selectedUnits.Add(enemy);
                        }
                    }
                }
            }

            foreach (NPC npc in LevelController.Instance.NPCs)
            {
                if (npc != null)
                {
                    if (npc.IsSelectable())
                    {
                        if (npc.CheckInsideArea(_origin, _target))
                        {
                            npc.Select(true);
                            m_commanderHUD.SetValueNameSelected(m_commanderHUD.GetValueNameSelected() + ((m_selectedUnits.Count > 0 ? "," : "") + npc.name));
                            m_selectedUnits.Add(npc);
                        }
                    }
                }
            }

        }

        protected override void ChangeState(int newState)
        {
            base.ChangeState(newState);

            switch ((STATE_COMMANDER)m_state)
            {
                case STATE_COMMANDER.SELECT_UNIT:
                    if (m_commanderHUD != null) m_commanderHUD.ResetAll((int)COMMAND_TYPES.NONE);
                    break;
                case STATE_COMMANDER.SELECT_ACTION:
                    if (m_planeAreaDetection != null) GameObject.Destroy(m_planeAreaDetection);
                    m_commanderHUD.ResetWithUnitSelected();
                    break;
                case STATE_COMMANDER.SET_DATA:
                    m_commanderHUD.SetValueActionSelected(m_selectedCommandType.ToString());
                    switch (m_selectedCommandType)
                    {
                        case COMMAND_TYPES.MOVE_TO:
                            m_commanderHUD.ResettWithSetData("You should click the position to go");
                            break;
                        case COMMAND_TYPES.LOOK_AT:
                            m_commanderHUD.ResettWithSetData("Provide the position to look");
                            break;
                        case COMMAND_TYPES.ATTACK:
                            m_commanderHUD.ResettWithSetData("Select the avatar to attack");
                            break;
                    }
                    break;
                case STATE_COMMANDER.ASSIGN_ACTION:
                    foreach (Avatar unit in m_selectedUnits)
                    {
                        unit.Select(false);
                        switch (m_selectedCommandType)
                        {
                            case COMMAND_TYPES.MOVE_TO:
                                CmdMoveTo cmdMoveTo = new CmdMoveTo(unit.GetComponent<Avatar>(), m_targetPosition, 20);
                                unit.AddCommand(cmdMoveTo);
                                break;
                            case COMMAND_TYPES.LOOK_AT:
                                CmdLookAt cmdLookAt = new CmdLookAt(unit.GetComponent<Avatar>(), m_targetPosition, 5);
                                unit.AddCommand(cmdLookAt);
                                break;
                            case COMMAND_TYPES.ATTACK:
                                CmdAttack cmdAttack = new CmdAttack(unit.GetComponent<Avatar>(), m_targetAvatar.GetComponent<Avatar>(), 20, 5, 2);
                                unit.AddCommand(cmdAttack);
                                break;
                        }
                    }
                    ResetState();
                    break;
            }
        }

        void Update()
        {
            if (!m_isReadyForCommand) return;
            if (!m_isActive) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetState();
            }

            switch ((STATE_COMMANDER)m_state)
            {
                case STATE_COMMANDER.SELECT_UNIT:
                    if (m_anchorMouse == Vector3.zero)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (!SelectSingleUnitByMouseRay())
                            {
                                m_anchorMouse = Utilities.GetCollisionMouse(GameController.LAYER_ENEMY, GameController.LAYER_PLAYER, GameController.LAYER_NPC);
                            }
                        }
                    }
                    else
                    {
                        Vector3 currentMouse = Utilities.GetCollisionMouse(GameController.LAYER_ENEMY, GameController.LAYER_PLAYER, GameController.LAYER_NPC);
                        CreatePlane(m_anchorMouse, currentMouse);
                        if (Input.GetMouseButtonUp(0))
                        {
                            m_finalMouse = Utilities.GetCollisionMouse(GameController.LAYER_ENEMY, GameController.LAYER_PLAYER, GameController.LAYER_NPC);
                            CheckSelectedAvatars(m_anchorMouse, m_finalMouse);
                        }
                    }

                    if (m_selectedUnits.Count > 0)
                    {
                        ChangeState((int)STATE_COMMANDER.SELECT_ACTION);
                    }
                    break;
                case STATE_COMMANDER.SELECT_ACTION:
                    break;
                case STATE_COMMANDER.SET_DATA:
                    switch (m_selectedCommandType)
                    {
                        case COMMAND_TYPES.MOVE_TO:
                            if (Input.GetMouseButtonDown(0))
                            {
                                Vector3 collidedPosition = Utilities.GetCollisionMouse();
                                if (collidedPosition != Vector3.zero)
                                {
                                    m_targetPosition = collidedPosition;
                                    ChangeState((int)STATE_COMMANDER.ASSIGN_ACTION);
                                }
                            }
                            break;

                        case COMMAND_TYPES.LOOK_AT:
                            if (Input.GetMouseButtonDown(0))
                            {
                                Vector3 collidedPosition = Utilities.GetCollisionMouse();
                                if (collidedPosition != Vector3.zero)
                                {
                                    m_targetPosition = collidedPosition;
                                    ChangeState((int)STATE_COMMANDER.ASSIGN_ACTION);
                                }
                            }
                            break;

                        case COMMAND_TYPES.ATTACK:
                            if (Input.GetMouseButtonDown(0))
                            {
                                GameObject targetAvatar = Utilities.GetCollisionMouseWithLayers(GameController.LAYER_ENEMY, GameController.LAYER_PLAYER, GameController.LAYER_NPC);
                                if (targetAvatar != null)
                                {
                                    m_targetAvatar = targetAvatar.GetComponent<Avatar>();
                                    if (m_targetAvatar != null)
                                    {
                                        Debug.Log("<color=red>TARGET AVATAR WITH NAME[" + m_targetAvatar.gameObject.name + "]</color>");
                                        ChangeState((int)STATE_COMMANDER.ASSIGN_ACTION);
                                    }
                                }
                            }
                            break;
                    }
                    break;
                case STATE_COMMANDER.ASSIGN_ACTION:
                    break;
            }

        }
    }
}