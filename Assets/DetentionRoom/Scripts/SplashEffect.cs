using UnityEngine;

namespace DetentionRoom.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class SplashEffect : MonoBehaviour
    {
        private AudioSource _audioSource;
        public AudioClip spongeHit;
        
        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.PlayOneShot(spongeHit);
        }
    }
}