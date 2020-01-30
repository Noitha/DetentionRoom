using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DetentionRoom.Networking.Inside_Session
{
    public class PlayerInfoDisplay : MonoBehaviour
    {
        public TextMeshProUGUI usernameText;
        public Button kickButton;
        public BoltEntity boltEntity;
        
        public void Initialize(BoltEntity be)
        {
            boltEntity = be;
            transform.SetAsLastSibling();
            
            kickButton.onClick.AddListener(Kick);
            
            kickButton.gameObject.SetActive(BoltNetwork.IsServer && boltEntity.Controller != null);
        }

        private void Kick()
        {
            if (BoltNetwork.IsServer)
            {
                boltEntity.Controller?.Disconnect();
            }
        }
    }
}