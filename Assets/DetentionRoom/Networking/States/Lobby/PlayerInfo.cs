using Bolt;
using DetentionRoom.Networking.Inside_Session;
using UnityEngine.SceneManagement;

namespace DetentionRoom.Networking.States.Lobby
{
    public class PlayerInfo : EntityEventListener<IPlayerInfo>
    {
        private string _username;
        private Classes _team;
        
        private PlayerInfoDisplay _playerInfoDisplay;
        private MenuController _menuController;

        private IPlayerInfo _iPlayerInfo;
        
        public override void Attached()
        {
            _team = Classes.Unassigned;
            _iPlayerInfo = entity.GetState<IPlayerInfo>();
            _menuController = MenuController.GetInstance();
            
            MenuController.OnSwitchToStudentTeam += SwitchToStudentTeam;
            MenuController.OnSwitchToTeacherTeam += SwitchToTeacherTeam;
            MenuController.OnSwitchToUnassignedTeam += SwitchToUnassignedTeam;
            
            state.AddCallback("Username", UsernameChangeUpdate);
            state.AddCallback("Team", TeamChangeUpdate);
            state.AddCallback("Entity", EntityChangedUpdate);

            if (entity.IsOwner)
            {
                state.Team = Classes.Unassigned.ToString();
            }

            _playerInfoDisplay = Instantiate(_menuController.playerInfoDisplayPrefab, _menuController.unassignedTeam);
        }

        public override void ControlGained()
        {
            _username = PlayerData.UserToken.Username;
        }
        
        private void UsernameChangeUpdate()
        {
            _playerInfoDisplay.usernameText.text = _iPlayerInfo.Username;
        }
        private void TeamChangeUpdate()
        {
            if (_playerInfoDisplay == null)
            {
                return;
            }

            switch (state.Team)
            {
                case "Unassigned":
                    _playerInfoDisplay.transform.SetParent(_menuController.unassignedTeam);
                    break;
                
                case "Teacher":
                    _playerInfoDisplay.transform.SetParent(_menuController.teacherTeam);
                    break;
                
                case "Student":
                    _playerInfoDisplay.transform.SetParent(_menuController.studentTeam);
                    break;
            }
        }

        private void EntityChangedUpdate()
        {
            if (_playerInfoDisplay != null)
            {
                _playerInfoDisplay.Initialize(entity);
            }
        }
        
        private void SwitchToTeacherTeam()
        {
            if (entity.HasControl)
            {
                _team = Classes.Teacher;
            }
        }
        private void SwitchToStudentTeam()
        {
            if (entity.HasControl)
            {
                _team = Classes.Student;
            }
        }
        private void SwitchToUnassignedTeam()
        {
            if (entity.HasControl)
            {
                _team = Classes.Unassigned;
            }
        }

        private void OnDestroy()
        {
            MenuController.OnSwitchToStudentTeam -= SwitchToStudentTeam;
            MenuController.OnSwitchToTeacherTeam -= SwitchToTeacherTeam;

            if (SceneManager.GetActiveScene().name == "Menu")
            {
                DestroyImmediate(_playerInfoDisplay.gameObject);
            }
        }
        
        public override void SimulateController()
        {
            if (BoltNetwork.Frame % 5 != 0)
            {
                return;
            }

            var input = LobbyCommand.Create();
            
            input.Username = _username;
            input.Team = _team.ToString();
            input.Entity = entity;
            
            entity.QueueInput(input);
        }
        
        public override void ExecuteCommand(Command command, bool resetState)
        {
            if (!entity.IsOwner || !(command is LobbyCommand lobbyCommand))
            {
                return;
            }

            state.Team = lobbyCommand.Input.Team;
            state.Username = lobbyCommand.Input.Username;
            state.Entity = lobbyCommand.Input.Entity;
        }
    }
}