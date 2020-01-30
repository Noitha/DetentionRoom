using System;
using System.Collections.Generic;
using System.Globalization;
using Bolt;
using Bolt.Matchmaking;
using Bolt.Photon;
using Bolt.Tokens;
using DetentionRoom.Networking.Inside_Session;
using DetentionRoom.Networking.Lobby;
using TMPro;
using UdpKit;
using UdpKit.platform.photon;
using UnityEngine;
using UnityEngine.UI;

namespace DetentionRoom.Networking
{
    public class MenuController : GlobalEventListener
    {
        #region Variables

        private static MenuController _instance;
        
        public static int PlayerLives = 20;
        
        #region UI
        
        [Header("Main Menu")]
        public Button hostGameButton;
        public Button enterLobbyButton;
        public Button exitGameButton;
        public Button optionsButton;
        public GameObject mainMenuWindow;
        
        [Header("Host Game")]
        public Button returnFromHostGameButton;
        public Button createGameButton;
        public Toggle requirePasswordToggle;
        public TMP_InputField gameNameInputField;
        public TMP_InputField passwordInputField;
        public GameObject hostGameWindow;
        public Slider minPlayers;
        public Slider maxPlayers;
        public TextMeshProUGUI minPlayersText;
        public TextMeshProUGUI maxPlayersText;
        public Toggle enablePlayerAmountSettingsToggle;
        public GameObject playerSettings;
        
        [Header("Lobby")]
        public Button returnFromLobbyButton;
        public Button refreshAllSessionsButton;
        public Transform listOfSessions;
        public DisplaySession sessionUiPrefab;
        public EnterSessionPassword enterSessionPassword;
        public GameObject lobbyWindow;
        
        [Header("Session")]
        public Button returnFromSessionButton;
        public Button startGameButton;
        public GameObject sessionWindow;
        public Transform teacherTeam;
        public Transform studentTeam;
        public Transform unassignedTeam;
        public Button selectTeamTeacherButton;
        public Button selectTeamStudentButton;
        public Button selectUnassignedButton;
        public PlayerInfoDisplay playerInfoDisplayPrefab;
        public Transform eventLogContainer;
        public TextMeshProUGUI eventLogTextPrefab;
        public Transform chatContainer;
        public TextMeshProUGUI chatTextPrefab;
        public Button sendChatTextButton;
        public TMP_InputField chatTextInputField;

        [Header("Settings")]
        public TMP_Dropdown resolutionsDropdown;
        public TMP_Dropdown fullScreenModeDropDown;
        public GameObject settingsMenu;
        public Button returnFromSettings;

        public static event Action OnSwitchToTeacherTeam;
        public static event Action OnSwitchToStudentTeam;
        public static event Action OnSwitchToUnassignedTeam;
        
        #endregion

        #region Data

        private string _gameName;
        private bool _requirePassword;
        private string _password;
        private int _minPlayers;
        private int _maxPlayers;
        private bool _usePlayerAmountSettings;
        private readonly Dictionary<Guid, GameObject> _cachedSessions = new Dictionary<Guid, GameObject>();
        
        #endregion

        #endregion
        
        #region Unity Methods

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(this);
            }
            
            var boltConfig = BoltRuntimeSettings.instance.GetConfigCopy();
            boltConfig.serverConnectionAcceptMode = BoltConnectionAcceptMode.Manual;
            
            //Main Menu
            hostGameButton.onClick.AddListener(() => BoltLauncher.StartServer(new UdpEndPoint(UdpIPv4Address.Any, 27000), boltConfig));
            enterLobbyButton.onClick.AddListener(() => BoltLauncher.StartClient());
            optionsButton.onClick.AddListener(EnterOptionsMenu);
            exitGameButton.onClick.AddListener(Application.Quit);
            
            //Host Game
            returnFromHostGameButton.onClick.AddListener(ReturnFromHostToMenu);
            createGameButton.onClick.AddListener(CreateSession);
            gameNameInputField.onValueChanged.AddListener(ValidateGameName);
            passwordInputField.onValueChanged.AddListener(ValidatePassword);
            requirePasswordToggle.onValueChanged.AddListener(ToggleRequirePassword);
            minPlayers.onValueChanged.AddListener(ValidateMinPlayers);
            maxPlayers.onValueChanged.AddListener(ValidateMaxPlayers);
            enablePlayerAmountSettingsToggle.onValueChanged.AddListener(TogglePlayerAmountSettings);
            
            //Lobby
            returnFromLobbyButton.onClick.AddListener(ReturnFromLobbyToMenu);
            refreshAllSessionsButton.onClick.AddListener(RefreshAllSessions);
            
            //Session
            returnFromSessionButton.onClick.AddListener(ReturnFromSession);
            startGameButton.onClick.AddListener(StartGame);
            selectTeamTeacherButton.onClick.AddListener(() => { OnSwitchToTeacherTeam?.Invoke(); });
            selectTeamStudentButton.onClick.AddListener(() => { OnSwitchToStudentTeam?.Invoke(); });
            selectUnassignedButton.onClick.AddListener(() => { OnSwitchToUnassignedTeam?.Invoke(); });
            sendChatTextButton.onClick.AddListener(SendText);

            //Settings
            returnFromSettings.onClick.AddListener(ReturnFromOptions);
            resolutionsDropdown.onValueChanged.AddListener(ChangeResolution);
            fullScreenModeDropDown.onValueChanged.AddListener(ChangeScreenMode);
        }
        private void Start()
        {
            InitializeWindows();
            _minPlayers = (int) minPlayers.value;
            _maxPlayers = (int) maxPlayers.value;
            _usePlayerAmountSettings = true;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        #endregion

        #region Custom Methods

        #region Instance

        public static MenuController GetInstance()
        {
            return _instance;
        }

        #endregion

        #region UI

        private static void SwitchMenu(GameObject from, GameObject to)
        {
            to.SetActive(true);
            from.SetActive(false);
        }

        #endregion

        #region Main Menu

        private void InitializeWindows()
        {
            mainMenuWindow.SetActive(true);
            hostGameWindow.SetActive(false);
            lobbyWindow.SetActive(false);
            sessionWindow.SetActive(false);
            settingsMenu.SetActive(false);
        }

        #endregion
        
        #region Host Game
        private void ReturnFromHostToMenu()
        {
            BoltLauncher.Shutdown();
            SwitchMenu(hostGameWindow, mainMenuWindow);
        }
        private void ToggleRequirePassword(bool requirePassword)
        {
            passwordInputField.gameObject.SetActive(requirePassword);
            _requirePassword = requirePassword;
        }
        private void TogglePlayerAmountSettings(bool playerAmountSettings)
        {
            playerSettings.SetActive(playerAmountSettings);
            _usePlayerAmountSettings = playerAmountSettings;
        }
        private void ValidateMinPlayers(float playerAmount)
        {
            maxPlayers.minValue = playerAmount;
            maxPlayersText.text = maxPlayers.value.ToString(CultureInfo.CurrentCulture);
            
            minPlayersText.text = playerAmount.ToString(CultureInfo.CurrentCulture);
            _minPlayers = (int) playerAmount;
        }
        private void ValidateMaxPlayers(float playerAmount)
        {
            maxPlayersText.text = playerAmount.ToString(CultureInfo.CurrentCulture);
            _maxPlayers = (int) playerAmount;
        }
        private void ValidateGameName(string gameName)
        {
            if (gameName.Length == 0)
            {
                createGameButton.interactable = false;
                return;
            }

            var canCreate = true;
            
            foreach (var session in BoltNetwork.SessionList)
            {
                if (session.Key.ToString() == gameName)
                {
                    canCreate = false;
                }
            }

            createGameButton.interactable = canCreate;
            _gameName = gameName;
        }
        private void ValidatePassword(string password)
        {
            if (password.Length == 0)
            {
                createGameButton.interactable = false;
                return;
            }

            _password = password;
            createGameButton.interactable = true;
        }
        private void CreateSession()
        {
            if (!BoltNetwork.IsServer)
            {
                return;
            }
            
            var photonRoomProperties = new PhotonRoomProperties();

            photonRoomProperties.AddRoomProperty("Use Player Amount Settings", _usePlayerAmountSettings);
            photonRoomProperties.AddRoomProperty("Max Players", _maxPlayers);
            photonRoomProperties.AddRoomProperty("Min Players", _minPlayers);
            
            if (_requirePassword)
            {
                photonRoomProperties.AddRoomProperty("RP", true);
                BoltMatchmaking.CreateSession(_gameName, photonRoomProperties);
            }
            else
            {
                photonRoomProperties.AddRoomProperty("RP", false);
                BoltMatchmaking.CreateSession(_gameName, photonRoomProperties);
            }
        }
        private void InitializeHostGameSettings()
        {
            requirePasswordToggle.isOn = false;
            ToggleRequirePassword(false);

            var guidGeneratedGameName = Guid.NewGuid().ToString();
            gameNameInputField.text = guidGeneratedGameName;
            ValidateGameName(guidGeneratedGameName);
        }
        
        #endregion
        
        #region Lobby

        private void ReturnFromLobbyToMenu()
        {
            BoltLauncher.Shutdown();
            SwitchMenu(lobbyWindow, mainMenuWindow);
            foreach (Transform session in listOfSessions)
            {
                Destroy(session.gameObject);
            }
        }

        #endregion
        
        #region Session
        private void RefreshAllSessions()
        {
            _cachedSessions.Clear();

            foreach (Transform session in listOfSessions)
            {
                Destroy(session.gameObject);
            }

            foreach (var session in BoltNetwork.SessionList)
            {
                if (!(session.Value is PhotonSession p) || !p.Properties.TryGetValue("RP", out var requiresPassword))
                {
                    continue;
                }

                var newSession = Instantiate(sessionUiPrefab, listOfSessions);
                newSession.SetSessionData(session.Value, (bool) requiresPassword, enterSessionPassword);
                _cachedSessions.Add(session.Key, newSession.gameObject);
            }
        }
        private void ReturnFromSession()
        {
            BoltLauncher.Shutdown();
        }
        private void StartGame()
        {
            if (!BoltNetwork.IsServer)
            {
                return;
            }
            
            BoltNetwork.LoadScene("PreAlphaMap");
            BoltMatchmaking.UpdateSession(new PhotonRoomProperties
            {
                IsOpen = false, 
                IsVisible = false
            });
        }
        private void CheckMinPlayerRequirements()
        {
            if (!(BoltMatchmaking.CurrentSession is PhotonSession photonSession))
            {
                return;
            }

            if (!photonSession.Properties.TryGetValue("Use Player Amount Settings", out var usePlayerAmountSettings))
            {
                return;
            }
            
            if ((bool) usePlayerAmountSettings)
            {
                if (!photonSession.Properties.TryGetValue("Min Players", out var mp))
                {
                    return;
                }
                    
                startGameButton.interactable = BoltMatchmaking.CurrentSession.ConnectionsCurrent >= (int) mp;
            }
            else
            {
                startGameButton.interactable = true;
            }
        }
        private void SendText()
        {
            if (chatTextInputField.text.Length == 0)
            {
                return;
            }

            var sessionText = SessionChat.Create(ReliabilityModes.Unreliable);
            sessionText.Player = PlayerData.UserToken.Username;
            sessionText.Message = chatTextInputField.text;
            sessionText.Send();
            
            chatTextInputField.text = "";
        }
        
        #endregion

        #region Options

        private void EnterOptionsMenu()
        {
            SwitchMenu(mainMenuWindow, settingsMenu);
        }

        private void ChangeResolution(int value)
        {
            switch (value)
            {
                case 0:
                    Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen);
                    break;
                
                case 1:
                    Screen.SetResolution(3840, 2160, FullScreenMode.ExclusiveFullScreen);
                    break;
                
                case 2:
                    Screen.SetResolution(1280, 720, FullScreenMode.ExclusiveFullScreen);
                    break;
            }
        }

        
        private void ChangeScreenMode(int value)
        {
            var currentResolution = Screen.currentResolution;
            
            switch (fullScreenModeDropDown.value)
            {
                case 0:
                    Screen.SetResolution(currentResolution.width, currentResolution.height, FullScreenMode.Windowed);
                    break;
                
                case 1:
                    Screen.SetResolution(currentResolution.width, currentResolution.height, FullScreenMode.MaximizedWindow);
                    break;
                
                case 2:
                    Screen.SetResolution(currentResolution.width, currentResolution.height, FullScreenMode.ExclusiveFullScreen);
                    break;
                
                case 3:
                    Screen.SetResolution(currentResolution.width, currentResolution.height, FullScreenMode.FullScreenWindow);
                    break;
            }
        }

        private void ReturnFromOptions()
        {
            SwitchMenu(settingsMenu, mainMenuWindow);
        }

        #endregion
        
        #endregion
        
        #region Bolt Callbacks

        public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
        {
            if (_cachedSessions.Count == 0)
            {
                RefreshAllSessions();
                return;
            }
            
            var sessionsToDelete = new HashSet<Guid>();
            var sessionsToAdd = new Dictionary<Guid, UdpSession>();
            
            //Remove sessions that no longer exists.
            foreach (var cachedSession in _cachedSessions)
            {
                var found = false;
                
                foreach (var session in sessionList)
                {
                    if (cachedSession.Key != session.Key)
                    {
                        continue;
                    }
                    
                    found = true;
                    break;
                }

                if (!found)
                {
                    sessionsToDelete.Add(cachedSession.Key);
                }
            }
            foreach (var guid in sessionsToDelete)
            {
                var deleteAfter = false;
                var toDelete = Guid.Empty;
                
                foreach (var cachedSession in _cachedSessions)
                {
                    if (guid == cachedSession.Key)
                    {
                        Destroy(cachedSession.Value);
                        toDelete = guid;
                        deleteAfter = true;
                    }
                }

                if (deleteAfter)
                {
                    _cachedSessions.Remove(toDelete);
                }
            }
            
            //Add missing sessions
            foreach (var session in sessionList)
            {
                var found = false;
                
                foreach (var cachedSession in _cachedSessions)
                {
                    if (cachedSession.Key != session.Key)
                    {
                        continue;
                    }
                    
                    found = true;
                    break;
                }

                if (!found)
                {
                    sessionsToAdd.Add(session.Key, session.Value);
                }
            }
            foreach (var sessionToAdd in sessionsToAdd)
            {
                if (!(sessionToAdd.Value is PhotonSession p) || !p.Properties.TryGetValue("RP", out var requiresPassword))
                {
                    continue;
                }
                
                var newSession = Instantiate(sessionUiPrefab, listOfSessions);
                newSession.SetSessionData(sessionToAdd.Value, (bool)requiresPassword, enterSessionPassword);
                _cachedSessions.Add(sessionToAdd.Key, newSession.gameObject);
            }
            
            //Update...
        }

        public override void Connected(BoltConnection connection)
        {
            var userToken = (UserToken) connection.ConnectToken;
            var newEventLog = Instantiate(eventLogTextPrefab, eventLogContainer);
            newEventLog.text = userToken.Username + " has joined to the session";
            
            if (!BoltNetwork.IsServer)
            {
                return;
            }

            var playerInfo = BoltNetwork.Instantiate(BoltPrefabs.PlayerInfo);
            playerInfo.AssignControl(connection);

            CheckMinPlayerRequirements();
        }

        public override void Disconnected(BoltConnection connection)
        {
            var userToken = (UserToken) connection.ConnectToken;
            var newEventLog = Instantiate(eventLogTextPrefab, eventLogContainer);
            newEventLog.text = userToken.Username + " has left the session";
            
            if (!BoltNetwork.IsServer)
            {
                return;
            }

            foreach (var entity in BoltNetwork.Entities)
            {
                if (!entity.IsController(connection))
                {
                    continue;
                }
                
                BoltNetwork.Destroy(entity);
            }

            CheckMinPlayerRequirements();
        }
        
        public override void BoltStartBegin()
        {
            BoltNetwork.RegisterTokenClass<UserToken>();
            BoltNetwork.RegisterTokenClass<PhotonRoomProperties>();
        }

        public override void BoltStartDone()
        {
            if (BoltNetwork.IsClient)
            {
                SwitchMenu(mainMenuWindow, lobbyWindow);
                _cachedSessions.Clear();
            }
            else if (BoltNetwork.IsServer)
            {
                InitializeHostGameSettings();
                SwitchMenu(mainMenuWindow, hostGameWindow);
            }
            
            startGameButton.gameObject.SetActive(BoltNetwork.IsServer);
        }
        
        public override void SessionCreated(UdpSession session)
        {
            SwitchMenu(hostGameWindow, sessionWindow);

            var hostInfo = BoltNetwork.Instantiate(BoltPrefabs.PlayerInfo);
            hostInfo.TakeControl();
            
            CheckMinPlayerRequirements();
        }
        
        public override void SessionConnected(UdpSession session, IProtocolToken token)
        {
            SwitchMenu(lobbyWindow, sessionWindow);
        }

        public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
        {
            switch (disconnectReason)
            {
                case UdpConnectionDisconnectReason.Unknown:
                    break;
                case UdpConnectionDisconnectReason.Timeout:
                    break;
                case UdpConnectionDisconnectReason.Error:
                    break;
                case UdpConnectionDisconnectReason.Disconnected:
                    break;
                case UdpConnectionDisconnectReason.Authentication:
                    break;
                case UdpConnectionDisconnectReason.MaxCCUReached:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(disconnectReason), disconnectReason, null);
            }
            
            SwitchMenu(sessionWindow, mainMenuWindow);
        }
        
        public override void ConnectRequest(UdpEndPoint endpoint, IProtocolToken token)
        {
            var userToken = (UserToken) token;
            var newEventLog = Instantiate(eventLogTextPrefab, eventLogContainer);
            newEventLog.text = userToken.Username + " has requested a connection to the session.";
            
            if (!BoltNetwork.IsServer)
            {
                return;
            }
            
            if (!(BoltMatchmaking.CurrentSession is PhotonSession))
            {
                return;
            }

            if (_usePlayerAmountSettings)
            {
                if (BoltMatchmaking.CurrentSession.ConnectionsCurrent <= _maxPlayers)
                {
                    if (_requirePassword)
                    {
                        if (userToken.Password != _password)
                        {
                            BoltNetwork.Refuse(endpoint, new BoltDisconnectToken("Wrong Password", UdpConnectionDisconnectReason.Authentication));
                            return;
                        }
                    
                        BoltNetwork.Accept(endpoint);
                    }
                    else
                    {
                        BoltNetwork.Accept(endpoint);
                    }
                }
                else
                {
                    BoltNetwork.Refuse(endpoint, new BoltDisconnectToken("Session is Full", UdpConnectionDisconnectReason.MaxCCUReached));
                }
            }
            else
            {
                if (_requirePassword)
                {
                    if (userToken.Password != _password)
                    {
                        BoltNetwork.Refuse(endpoint, new BoltDisconnectToken("Wrong Password", UdpConnectionDisconnectReason.Authentication));
                        return;
                    }
                    
                    BoltNetwork.Accept(endpoint);
                }
                else
                {
                    BoltNetwork.Accept(endpoint);
                }
            }
        }
        
        #endregion

        public override void OnEvent(SessionChat sessionChat)
        {
            var newEventLog = Instantiate(chatTextPrefab, chatContainer);
            newEventLog.text = sessionChat.Player + ": " + sessionChat.Message;
        }
    }
}