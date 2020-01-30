using System.Collections.Generic;
using DetentionRoom.Networking.States.Player;
using DetentionRoom.Networking.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DetentionRoom.Scripts
{
    // ReSharper disable once InconsistentNaming
    public class HUD : MonoBehaviour
    {
        private static HUD _instance;
        private Player _player;
        private IPlayer _iPlayer;
        
        public Slider healthSlider;
        public TextMeshProUGUI healthText;
        
        public Image activeWeaponIcon;
        public TextMeshProUGUI activeWeaponAmmoText;
        
        public List<Sprite> weaponIcons = new List<Sprite>();
        public Sprite spongeIconWet;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 100;
        }
        
        public static HUD GetInstance()
        {
            return _instance;
        }

        public void SetPlayer(Player player)
        {
            _player = player;
            _iPlayer = player.state;
        }

        public void RefreshHealth()
        {
            if (!_player.entity.HasControl)
            {
                return;
            }

            healthSlider.value = _iPlayer.Health;
            healthText.text = _iPlayer.Health + "/" + 100;
        }

        public void RefreshWeaponDisplay()
        {
            if (!_player.entity.HasControl)
            {
                return;
            }

            var weapon = _iPlayer.Weapons[_iPlayer.ActiveWeaponIndex];
            activeWeaponIcon.sprite = weaponIcons[_iPlayer.ActiveWeaponIndex];
            activeWeaponAmmoText.text = weapon.CurrentAmmoInMagazine + "/" + weapon.EntireAmmo;

            if (weapon.IsWet)
            {
                activeWeaponIcon.sprite = spongeIconWet;
            }
        }

        public void RefreshAmmoDisplay()
        {
            var weapon = _iPlayer.Weapons[_iPlayer.ActiveWeaponIndex];
            activeWeaponAmmoText.text = weapon.CurrentAmmoInMagazine + "/" + weapon.EntireAmmo;
        }
    }
}