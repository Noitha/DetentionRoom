using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DetentionRoom.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DetentionRoom.Networking
{
    public class ChangeUserName : MonoBehaviour
    {
        public TMP_InputField inputUsername;
        public Button applyUserName;
        public Button enterLobbyMenuButton;
        public Button enterHostMenuButton;
        private string _username;

        private UserToken _userToken;
        
        private void Awake()
        {
            inputUsername.onValueChanged.AddListener(ValidateUsername);
            applyUserName.onClick.AddListener(SaveUsername);
        }
        private void Start()
        {
            LoadUsername();
            inputUsername.text = _username;
            ValidateUsername(_username);
        }
        private void LoadUsername()
        {
            var filePath = Application.persistentDataPath + "/save/" + "Username.dat";
            
            if (!File.Exists(filePath))
            {
                _username = "";
                enterHostMenuButton.interactable = false;
                enterLobbyMenuButton.interactable = false;
                PlayerData.UserToken = null;
                return;
            }
            
            var fileStream = File.Open(filePath, FileMode.Open);
            _username = (string) new BinaryFormatter().Deserialize(fileStream);
            PlayerData.UserToken = new UserToken(_username, "");
            fileStream.Close();
        }
        private void SaveUsername()
        {
            var filePath = Application.persistentDataPath + "/save/" + "Username.dat";
            
            if (!File.Exists(filePath))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            var fileStream = File.Open(filePath, FileMode.OpenOrCreate);

            new BinaryFormatter().Serialize(fileStream, _username);
            fileStream.Close();
            
            enterHostMenuButton.interactable = true;
            enterLobbyMenuButton.interactable = true;
            PlayerData.UserToken = new UserToken(_username, "");
        }
        private void ValidateUsername(string username)
        {
            if (username.Length == 0 || username.Length > 15)
            {
                applyUserName.interactable = false;
                return;
            }
            
            applyUserName.interactable = true;
            _username = username;
        }
    }
}