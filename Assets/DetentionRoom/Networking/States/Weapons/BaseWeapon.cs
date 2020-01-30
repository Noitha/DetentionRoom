using Bolt;
using UnityEngine;

namespace DetentionRoom.Networking.States.Weapons
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    public abstract class BaseWeapon : MonoBehaviour
    {
        public Weapon weapon;
        public bool skipUpdate;
        public AudioClip[] fireSound;
        public AudioClip reloadSound;
        public bool canSwitch;
        public AudioSource audioSource;
        protected int BeforeReload;
        public Animator animator;
        protected bool cancelReload;
        public Player.Player player;
        public Transform cameraTransform;

        private void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

        public void InterruptReload()
        {
            cancelReload = true;
            
            var reloadingWeapon = ReloadingWeapon.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            reloadingWeapon.Player = player.entity;
            reloadingWeapon.WeaponId = weapon.Id;
            reloadingWeapon.WeaponReloadState = false;
            reloadingWeapon.Send();
            
            //Send a command to set the ammo in magazine.
            var weaponReloaded = WeaponReloaded.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            weaponReloaded.Player = weapon.Player;
            weaponReloaded.WeaponId = weapon.Id;
            weaponReloaded.NewCurrentAmount = BeforeReload;
            weaponReloaded.Send();
        }
    }
}