#if ENABLE_MIRROR
using Mirror;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YourVRExperience.Networking;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
	public class GameController : StateMachine
	{
		public const string TAG_FLOOR = "Floor";
		public const string LAYER_PLAYER = "Player";
		public const string LAYER_ENEMY = "Enemy";
		public const string LAYER_NPC = "NPC";

		private static GameController _instance;
		public static GameController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType<GameController>();
				}
				return _instance;
			}
		}

		public enum GAME_STATES { MAIN_MENU = 0, CONNECTING, LOAD_GAME, GAME_RUNNING, WIN, LOSE, PAUSE, NULL }

		public GameObject[] Controllers;
		public LevelController[] Levels;
		public int CurrentLevel = 0;
		public GameObject PlayerPrefab;
		public GameObject GameUI;

		public List<IPlayer> Players = new List<IPlayer>();

		public IPlayer LocalPlayer;

		private bool m_hasPlayerPressedButton = false;

		private float m_timeLoading = 0;
		private bool m_hasCompletedLoadingResources = false;
		private bool m_hasPressedGoToTheNextLevel = false;

		private bool m_pressedToReturnGame = false;
		private bool m_pressedToReloadGame = false;

		private int m_counterDeadEnemies = 0;
		private int m_counterDeadNPCs = 0;
		private int m_counterCollectedCoins = 0;

		private IHUD m_HUD;

		private bool m_isMultiplayerGame = false;
		private bool m_changeStateRequested = false;

		private IInputController m_inputControls;

		public int CounterDeadEnemies
		{
			get { return m_counterDeadEnemies; }
		}
		public int CounterCollectedCoins
		{
			get { return m_counterCollectedCoins; }
		}
		public bool IsMultiplayerGame
		{
			get { return m_isMultiplayerGame; }
		}
		public IInputController InputControls
        {
			get { return m_inputControls; }
        }

		void Awake()
		{
			foreach (GameObject controller in Controllers)
			{
				Instantiate(controller);
			}

			SystemEventController.Instance.Event += OnSystemEvent;
		}

		void Start()
		{
			ItemControllerSO.Instance.Initialize();

			m_state = -1;

			ChangeState((int)GAME_STATES.MAIN_MENU);
		}

		void OnDestroy()
		{
			m_inputControls = null;

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			if (m_isMultiplayerGame)
			{
				if (NetworkController.Instance != null) NetworkController.Instance.NetworkEvent -= OnNetworkEvent;
			}
		}

		private void OnNetworkEvent(string _nameEvent, int _originNetworkID, int _targetNetworkID, object[] _parameters)
		{
			if (_nameEvent == SystemEventGameController.EVENT_NETWORK_REPORT_NETWORK_ID)
			{
				int networkID = (int)_parameters[0];
				Utilities.DebugLogColor("RECEIVED THE EVENT MESSAGE WITH THE NETWORK ID [" + networkID + "]", Color.yellow);
			}
			if (_nameEvent == SystemEventGameController.EVENT_NETWORK_CHANGE_STATE)
			{
				int newState = (int)_parameters[0];
				ChangeLocalState(newState);
			}
		}

		private void OnSystemEvent(string _nameEvent, object[] _parameters)
		{
			if (_nameEvent == InputController.EVENT_INPUTCONTROLLER_HAS_STARTED)
            {
				m_inputControls = ((GameObject)_parameters[0]).GetComponent<IInputController>();
				m_inputControls.Initialize();
			}
			if (_nameEvent == NetworkController.EVENT_NETWORK_CONNECTION_WITH_ROOM)
			{
				ChangeState((int)GAME_STATES.LOAD_GAME);
			}
			if (_nameEvent == SystemEventGameController.EVENT_HUD_HAS_STARTED)
			{
				m_HUD = (IHUD)_parameters[0];
			}
			if (_nameEvent == SystemEventGameController.EVENT_PLAYER_HAS_STARTED)
			{
				IPlayer newPlayer = (IPlayer)_parameters[0];
				if (newPlayer.IsOwner())
				{
					if (LocalPlayer == null)
					{
						LocalPlayer = newPlayer;

						StartCoroutine(ConfirmationPlayerHasStarted());
#if ENABLE_MIRROR
						if (m_isMultiplayerGame)
						{
							MirrorController.Instance.Connection.CmdAssignNetworkAuthority(LocalPlayer.GetGameObject().GetComponent<NetworkIdentity>(), MirrorController.Instance.Connection.netIdentity);
						}
#endif
					}
				}
				if (!Players.Contains(newPlayer))
				{
					Players.Add(newPlayer);
				}
			}
			if (_nameEvent == SystemEventGameController.EVENT_ENEMY_DEAD)
			{
				m_counterDeadEnemies++;
				Debug.Log("<color=red>GameController has received the event of Enemy Dead!!! Total Enemies Dead=" + m_counterDeadEnemies + "</color>");
			}
			if (_nameEvent == SystemEventGameController.EVENT_NPC_DEAD)
			{
				m_counterDeadNPCs++;
				Debug.Log("<color=red>GameController has received the event of NPC Dead!!! Total NPCs Dead=" + m_counterDeadNPCs + "</color>");
			}
			if (_nameEvent == SystemEventGameController.EVENT_COIN_COLLECTED)
			{
				m_counterCollectedCoins++;
			}
		}

		public void PlaySinglePlayer()
		{
			m_hasPlayerPressedButton = true;
		}

		public void PlayMultiplayer()
		{
			m_isMultiplayerGame = true;
			NetworkController.Instance.IsMultiplayer = true;
#if ENABLE_MIRROR
			NetworkController.Instance.Initialize();
#endif
			ChangeState((int)GAME_STATES.CONNECTING);
		}

		private void ResetGameControllerState()
		{
			m_hasPlayerPressedButton = false;
			m_timeLoading = 0;
			m_hasCompletedLoadingResources = false;
			m_hasPressedGoToTheNextLevel = false;
			m_pressedToReturnGame = false;
			m_pressedToReloadGame = false;
		}

		private void RenderMenu()
		{

		}

		private bool UserPressedPlayButton()
		{
			return m_hasPlayerPressedButton;
		}

		private void LoadGame()
		{
			m_timeLoading += Time.deltaTime;
			if (m_timeLoading > 0.5f)
			{
				m_hasCompletedLoadingResources = true;
			}
		}

		private bool HasLoadedGame()
		{
			return m_hasCompletedLoadingResources;
		}

		private void RunGame()
		{
			try
            {
				foreach (IPlayer player in Players)
				{
					if (player != null) player.UpdateLogic();
				}

				LevelController.Instance.UpdateLogic();
			}
			catch (Exception err) { }
		}

		private bool PlayerIsDead()
		{
			if (LocalPlayer.Life <= 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private void ShowWinScreen()
		{

		}

		public void PressedGoToNextLevel()
		{
			m_hasPressedGoToTheNextLevel = true;
		}

		private bool PressedNextButton()
		{
			return m_hasPressedGoToTheNextLevel;
		}

		private void ShowLoseScreen()
		{

		}

		public void UserHasPressedReturnGame()
		{
			m_pressedToReturnGame = true;
		}

		private bool ButtonPressedReturnGame()
		{
			return m_pressedToReturnGame;
		}

		public void UserHasPressedReloadGame()
		{
			m_pressedToReloadGame = true;
		}

		private bool ButtonPressedReloadGame()
		{
			return m_pressedToReloadGame;
		}

		private void ActivationScreenMenuMain()
		{
			ScreenController.Instance.CreateScreen("MainMenu", true);
		}

		private void ActivationScreenConnecting()
		{
			ScreenController.Instance.CreateScreen("ConnectingScreen", true);
		}

		private void ActivationScreenLoadGame()
		{
			ScreenController.Instance.CreateScreen("LoadScreen", true);
		}

		private void ActivationScreenWinGame()
		{
			ScreenController.Instance.CreateScreen("WinScreen", true);
		}

		private void ActivationScreenLoseGame()
		{
			ScreenController.Instance.CreateScreen("LoseScreen", true);
		}

		private void ActivationGameHUD(bool _activate)
		{
			if (m_HUD != null) m_HUD.ActivateHUD(_activate);
		}

		private void ActivationScreenPauseGame()
		{
			ScreenController.Instance.CreateScreen("PauseScreen", true);
		}

		private void DisableAllScreens()
		{
			if (m_HUD != null) m_HUD.ActivateHUD(false);
		}

		private void ResetPauseVariables()
		{
			m_pressedToReturnGame = false;
			m_pressedToReloadGame = false;
		}

		private void InitializeLogicGameElements()
		{
			foreach (IPlayer player in Players)
			{
				if (player != null) player.InitLogic();
			}
			LevelController.Instance.InitializeLogicGameElements();
		}

		private void StopLogicGameElements()
		{
			foreach (IPlayer player in Players)
			{
				if (player != null) player.StopLogic();
			}
			LevelController.Instance.StopLogicGameElements();
		}

		IEnumerator ConfirmationPlayerHasStarted()
        {
			yield return new WaitForSeconds(0.1f);

#if !ENABLE_MOBILE
			IHUD iHUD = GameObject.FindObjectOfType<HUD>();
			if (iHUD != null)
			{
				LocalPlayer.SetHUD(iHUD);
			}
			else
			{
				LocalPlayer.SetHUD(Instantiate(GameUI).GetComponentInChildren<IHUD>());
			}
#endif
			LocalPlayer.ResetPlayerLife();
			LocalPlayer.ResetPlayerPosition();
			m_hasPressedGoToTheNextLevel = false;
			Instantiate(Levels[CurrentLevel]);
			SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_GAMECONTROLLER_PLAYER_HAS_BEEN_CONFIRMED);
			if (m_isMultiplayerGame)
            {
				NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_HAS_LOADED_THE_GAME, -1, -1);
			}
		}

		protected override void ChangeState(int newState)
		{
			if (m_state == newState) return;

			if (m_isMultiplayerGame)
			{
				switch ((GAME_STATES)newState)
				{
					case GAME_STATES.MAIN_MENU:
					case GAME_STATES.LOAD_GAME:
					case GAME_STATES.CONNECTING:
						ChangeLocalState(newState);
						break;

					default:
						if (!m_changeStateRequested)
						{
							m_changeStateRequested = true;
							NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_CHANGE_STATE, NetworkController.Instance.UniqueNetworkID, -1, newState);
						}
						break;
				}
			}
			else
			{
				ChangeLocalState(newState);
			}
		}

		protected void ChangeLocalState(int newState)
		{
			m_changeStateRequested = false;

			base.ChangeState(newState);

			switch ((GAME_STATES)m_state)
			{
				case GAME_STATES.MAIN_MENU:
					SystemEventController.Instance.DispatchSystemEvent(InputController.EVENT_INPUTCONTROLLER_ENABLE_MOBILE_HUD, false);
					Cursor.lockState = CursorLockMode.None;
					ResetGameControllerState();
					DisableAllScreens();
					ActivationScreenMenuMain();
					SoundsGameController.Instance.PlaySoundBackground(SoundsGameController.MELODY_MAIN_MENU, true, 1);
					Debug.Log("GAME CONTROLLER IS IN STATE MAIN_MENU");
					break;
				case GAME_STATES.CONNECTING:
					SystemEventController.Instance.DispatchSystemEvent(InputController.EVENT_INPUTCONTROLLER_ENABLE_MOBILE_HUD, false);
					ActivationScreenConnecting();
					NetworkController.Instance.NetworkEvent += OnNetworkEvent;
					NetworkController.Instance.Connect();
					break;
				case GAME_STATES.LOAD_GAME:
					SystemEventController.Instance.DispatchSystemEvent(InputController.EVENT_INPUTCONTROLLER_ENABLE_MOBILE_HUD, false);
					if (LevelController.Instance != null) LevelController.Instance.Destroy();
					Players.Clear();
					if (LocalPlayer != null)
					{
						LocalPlayer.Destroy();
						LocalPlayer = null;
					}
					if (m_isMultiplayerGame == false)
					{
						Instantiate(PlayerPrefab);
					}
					else
					{
						NetworkController.Instance.CreateNetworkPrefab(PlayerPrefab.name, PlayerPrefab.gameObject, "Prefabs\\Avatars\\" + PlayerPrefab.name, new Vector3(0, 1.04f, 0), Quaternion.identity, 0);
					}
					DisableAllScreens();
					SoundsGameController.Instance.StopSoundBackground();
					ActivationScreenLoadGame();
					Debug.Log("GAME CONTROLLER IS IN STATE LOAD_GAME");
					break;
				case GAME_STATES.GAME_RUNNING:
					SystemEventController.Instance.DispatchSystemEvent(InputController.EVENT_INPUTCONTROLLER_ENABLE_MOBILE_HUD, true);
					if (m_isMultiplayerGame)
					{
						NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_REPORT_NETWORK_ID, NetworkController.Instance.UniqueNetworkID, -1, NetworkController.Instance.UniqueNetworkID);
					}
#if !ENABLE_MOBILE
					Cursor.lockState = CursorLockMode.Locked;
#endif
					ScreenController.Instance.DestroyScreens();
					foreach (IPlayer player in Players)
					{
						if (player != null)
						{
							if (player.GetGameObject() != null)
							{
								player.GetGameObject().GetComponent<Rigidbody>().useGravity = true;
							}
						}
					}
					SoundsGameController.Instance.PlaySoundBackground(SoundsGameController.MELODY_INGAME, true, 1);
					switch ((GAME_STATES)m_previousState)
					{
						case GAME_STATES.PAUSE:
							CameraController.Instance.FreezeCamera(false);
							ResetPauseVariables();
							InitializeLogicGameElements();
							break;

						case GAME_STATES.LOAD_GAME:
							InitializeLogicGameElements();
							break;
					}
					ActivationGameHUD(true);
					Debug.Log("GAME CONTROLLER IS IN STATE GAME_RUNNING");
					break;
				case GAME_STATES.WIN:
					SystemEventController.Instance.DispatchSystemEvent(InputController.EVENT_INPUTCONTROLLER_ENABLE_MOBILE_HUD, false);
					Cursor.lockState = CursorLockMode.None;
					CurrentLevel++;
					if (CurrentLevel > 2) CurrentLevel = 0;
					foreach (IPlayer player in Players)
					{
						if (player != null) player.ResetPlayerLife();
					}
					StopLogicGameElements();
					ActivationGameHUD(false);
					ActivationScreenWinGame();
					SoundsGameController.Instance.StopSoundBackground();
					SoundsGameController.Instance.PlaySoundBackground(SoundsGameController.MELODY_WIN, false, 1);
					Debug.Log("GAME CONTROLLER IS IN STATE WIN");
					Players.Clear();
					break;
				case GAME_STATES.LOSE:
					SystemEventController.Instance.DispatchSystemEvent(InputController.EVENT_INPUTCONTROLLER_ENABLE_MOBILE_HUD, false);
					Cursor.lockState = CursorLockMode.None;
					StopLogicGameElements();
					ActivationGameHUD(false);
					ActivationScreenLoseGame();
					SoundsGameController.Instance.StopSoundBackground();
					SoundsGameController.Instance.PlaySoundBackground(SoundsGameController.MELODY_LOSE, false, 1);
					Debug.Log("GAME CONTROLLER IS IN STATE LOSE");
					Players.Clear();
					break;
				case GAME_STATES.PAUSE:
					SystemEventController.Instance.DispatchSystemEvent(InputController.EVENT_INPUTCONTROLLER_ENABLE_MOBILE_HUD, false);
					CameraController.Instance.FreezeCamera(true);
					Cursor.lockState = CursorLockMode.None;
					ActivationScreenPauseGame();
					ActivationGameHUD(false);
					StopLogicGameElements();
					break;
			}
		}

		void Update()
		{
			switch ((GAME_STATES)m_state)
			{
				case GAME_STATES.MAIN_MENU:

					RenderMenu();

					if (UserPressedPlayButton() == true)
					{
						ChangeState((int)GAME_STATES.LOAD_GAME);
					}
					break;

				case GAME_STATES.CONNECTING:
					break;

				case GAME_STATES.LOAD_GAME:

					LoadGame();

					if ((HasLoadedGame() == true) && (LocalPlayer != null) && (LevelController.Instance != null))
					{
						m_timeCounter += Time.deltaTime;
						if ((m_timeCounter > 0.5f) || (!m_isMultiplayerGame))
                        {
							ChangeState((int)GAME_STATES.GAME_RUNNING);
						}						
					}

					break;

				case GAME_STATES.GAME_RUNNING:

					RunGame();

					if ((LevelController.Instance != null) && (LevelController.Instance.CheckVictory() == true))
					{
						ChangeState((int)GAME_STATES.WIN);
					}

					if (PlayerIsDead() == true)
					{
						ChangeState((int)GAME_STATES.LOSE);
					}

					if (Input.GetKeyDown(KeyCode.P))
					{
						ChangeState((int)GAME_STATES.PAUSE);
					}

					if (Input.GetKeyDown(KeyCode.N))
					{
						ChangeState((int)GAME_STATES.WIN);
					}

					break;

				case GAME_STATES.WIN:

					ShowWinScreen();


					if (PressedNextButton() == true)
					{
						if (IsMultiplayerGame)
						{
							ChangeLocalState((int)GAME_STATES.NULL);
							NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_CHANGE_STATE, NetworkController.Instance.UniqueNetworkID, -1, (int)GAME_STATES.LOAD_GAME);
						}
						else
						{
							ChangeState((int)GAME_STATES.LOAD_GAME);
						}
					}

					if (ButtonPressedReloadGame() == true)
					{
						SceneManager.LoadScene("Game");
					}
					break;


				case GAME_STATES.LOSE:

					ShowLoseScreen();

					if (PressedNextButton() == true)
					{
						if (IsMultiplayerGame)
						{
							ChangeLocalState((int)GAME_STATES.NULL);
							NetworkController.Instance.DispatchNetworkEvent(SystemEventGameController.EVENT_NETWORK_CHANGE_STATE, NetworkController.Instance.UniqueNetworkID, -1, (int)GAME_STATES.LOAD_GAME);
						}
						else
						{
							CameraController.Instance.SetCameraTo1stPerson();
							ChangeState((int)GAME_STATES.LOAD_GAME);
						}
					}

					if (ButtonPressedReloadGame() == true)
					{
						SceneManager.LoadScene("Game");
					}
					break;

				case GAME_STATES.PAUSE:

					if (ButtonPressedReturnGame() == true)
					{
						ChangeState((int)GAME_STATES.GAME_RUNNING);
					}

					if (ButtonPressedReloadGame() == true)
					{
						SceneManager.LoadScene("Game");
					}
					break;
			}
		}
	}
}