using Bolt;
using DetentionRoom.Networking.States.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Time;

namespace DetentionRoom.Networking
{
    public class SpongeRecharge : GlobalEventListener
    {
        public Slider wetProgressSlider;

        public TextMeshProUGUI wetProgressText;

        private float _currentProgress;
        private BoltEntity _player;

        private bool _filled;
        private bool _hasDrySpongeInHand;
        private bool _error;

        private AudioSource _audioSource;
        public AudioClip tapSound;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            ResetProgress();

            if (other.GetComponent<Player>() == null)
            {
                return;
            }
            
            _player = other.GetComponent<Player>().entity;

            var playerState = _player.GetState<IPlayer>();
            var weapon = playerState.Weapons[playerState.ActiveWeaponIndex];
                
            if (weapon.Label == "Sponge" && weapon.IsEquipped && !weapon.IsWet)
            {
                _hasDrySpongeInHand = true;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (_filled)
            {
                wetProgressSlider.value = 100;
                wetProgressText.text = "100%";
                _error = true;
            }
            
            if (!_hasDrySpongeInHand)
            {
                _error = true;
            }

            if (_error)
            {
                return;
            }
            
            _currentProgress += deltaTime * 140 / 5;
            
            wetProgressSlider.value = (int) _currentProgress;

            wetProgressText.text = (int) _currentProgress + "%";

            if (_currentProgress >= 100)
            {
                RechargeSponge rechargeSponge = RechargeSponge.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
                rechargeSponge.Player = _player;
                rechargeSponge.Send();

                _filled = true;
                
                var playerSoundForOthers = PlaySoundForEveryone.Create(GlobalTargets.Everyone, ReliabilityModes.Unreliable);
                playerSoundForOthers.Position = transform.position;
                playerSoundForOthers.SoundName = "Tap-Water";
                playerSoundForOthers.State = false;
                playerSoundForOthers.Send();
            }
            else
            {
                var playerSoundForOthers = PlaySoundForEveryone.Create(GlobalTargets.Everyone, ReliabilityModes.Unreliable);
                playerSoundForOthers.Position = transform.position;
                playerSoundForOthers.SoundName = "Tap-Water";
                playerSoundForOthers.State = true;
                playerSoundForOthers.Send();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            ResetProgress();

            var playerSoundForOthers = PlaySoundForEveryone.Create(GlobalTargets.Everyone, ReliabilityModes.Unreliable);
            playerSoundForOthers.Position = transform.position;
            playerSoundForOthers.SoundName = "Tap-Water";
            playerSoundForOthers.State = false;
            playerSoundForOthers.Send();
        }
        
        private void ResetProgress()
        {
            _hasDrySpongeInHand = false;
            _filled = false;
            _error = false;
            
            _currentProgress = 0;
            wetProgressText.text = "0%";
            
            wetProgressSlider.minValue = 0;
            wetProgressSlider.maxValue = 100;
            wetProgressSlider.value = 0;
        }

        public override void OnEvent(PlaySoundForEveryone playSoundForEveryone)
        {
            if (playSoundForEveryone.SoundName != "Tap-Water")
            {
                return;
            }
            
            if (!playSoundForEveryone.State)
            {
                _audioSource.Stop();
                return;
            }
            
            if (_audioSource.isPlaying)
            {
                return;
            }
            
            _audioSource.PlayOneShot(tapSound);
        }
    }
}