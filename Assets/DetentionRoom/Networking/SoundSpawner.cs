using DetentionRoom.Networking.States.Player;
using UnityEngine;

namespace DetentionRoom.Networking
{
    public class SoundSpawner : MonoBehaviour
    {
        public AudioSource audioSource;

        public void Play(string weaponType, BoltEntity boltEntity)
        {
            var iPlayer = boltEntity.GetComponent<Player>();

            if (audioSource.isPlaying)
            {
                return;
            }
            
            switch (weaponType)
            {
                case "Chalk":
                    audioSource.maxDistance = 15;
                    audioSource.PlayOneShot(iPlayer.weaponSystem.chalkWeaponState.fireSound[Random.Range(0, iPlayer.weaponSystem.chalkWeaponState.fireSound.Length)]);
                    break;
                
                case "Stapler":
                    audioSource.maxDistance = 10;
                    audioSource.PlayOneShot(iPlayer.weaponSystem.staplerWeaponState.fireSound[Random.Range(0, iPlayer.weaponSystem.staplerWeaponState.fireSound.Length)]);
                    break;
                
                case "Slingshot":
                    audioSource.maxDistance = 25;
                    audioSource.PlayOneShot(iPlayer.weaponSystem.slingshotWeaponState.fireSound[Random.Range(0, iPlayer.weaponSystem.slingshotWeaponState.fireSound.Length)]);
                    break;

                case "Sponge":
                    audioSource.maxDistance = 30;
                    audioSource.PlayOneShot(iPlayer.weaponSystem.spongeWeaponState.fireSound[Random.Range(0, iPlayer.weaponSystem.spongeWeaponState.fireSound.Length)]);
                    break;
                
                case "Hand":
                    audioSource.maxDistance = 1;
                    audioSource.PlayOneShot(iPlayer.weaponSystem.handsWeaponState.fireSound[Random.Range(0, iPlayer.weaponSystem.handsWeaponState.fireSound.Length)]);
                    break;
            }
            
            Destroy(gameObject, 3f);
        }
    }
}