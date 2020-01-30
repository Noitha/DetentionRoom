using DetentionRoom.Networking.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DetentionRoom.Networking
{
    public class EnterSessionPassword : MonoBehaviour
    {
        public TMP_InputField inputPassWord;
        public Button connectButton;
        public GameObject window;
        public Button closeButton;

        private void Awake()
        {
            closeButton.onClick.AddListener(CloseWindow);
        }
        private void Start()
        {
            window.SetActive(false);
        }
        public void Initialize(DisplaySession displaySession)
        {
            connectButton.onClick.RemoveAllListeners();
            connectButton.onClick.AddListener(() => displaySession.EnterPasswordAndConnect(inputPassWord.text));
        }
        private void CloseWindow()
        {
            window.SetActive(false);
        }
        public void Display()
        {
            window.SetActive(true);
        }
        public void Hide()
        {
            window.SetActive(false);
        }
    }
}