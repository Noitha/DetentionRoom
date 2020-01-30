using DetentionRoom.Scripts;
using TMPro;
using UdpKit;
using UdpKit.platform.photon;
using UnityEngine;
using UnityEngine.UI;

namespace DetentionRoom.Networking.Lobby
{
    public class DisplaySession : MonoBehaviour
    {
        public Button joinRoomButton;
        public TextMeshProUGUI joinButtonText;
        public TextMeshProUGUI sessionNameText;
        public TextMeshProUGUI connectedPlayersText;
        private UdpSession _udpSession;
        private bool _requiresPassword;
        public EnterSessionPassword enterSessionPassword;
        
        
        private void Awake()
        {
            joinRoomButton.onClick.AddListener(ConnectToRoom);
        }
        
        public void SetSessionData(UdpSession udpSession,  bool requiresPassword, EnterSessionPassword enterSessionPasswordPanel)
        {
            if (udpSession is PhotonSession photonSession)
            {
                if (photonSession.Properties.TryGetValue("Min Players", out var minPlayers))
                {
                    
                }
                
                if (photonSession.Properties.TryGetValue("Max Players", out var maxPlayers))
                {
                    connectedPlayersText.text = udpSession.ConnectionsCurrent + "/" + (int) maxPlayers;
                }
            }
            
            sessionNameText.text = udpSession.HostName;
            _udpSession = udpSession;
            joinButtonText.text = requiresPassword ? "Enter Password" : "Join";
            _requiresPassword = requiresPassword;
            enterSessionPassword = enterSessionPasswordPanel;
        }

        private void ConnectToRoom()
        {
            if (_requiresPassword)
            {
                enterSessionPassword.Initialize(this);
                enterSessionPassword.Display();
            }
            else
            {
                enterSessionPassword.Hide();
                BoltNetwork.Connect(_udpSession, PlayerData.UserToken);
            }
        }
        
        public void EnterPasswordAndConnect(string password)
        {
            BoltNetwork.Connect(_udpSession, new UserToken(PlayerData.UserToken.Username, password));
        }
    }
}