using System;
using System.Collections;
using Bolt;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DetentionRoom.Networking.States.Weapons
{
    public class BaseRayCastWeapon : BaseWeapon
    {
        private float _nextTimeToFire;
        

        private void OnEnable()
        {
            if (!player.entity.IsAttached)
            {
                return;
            }

            var reloadingWeapon = ReloadingWeapon.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            reloadingWeapon.Player = player.entity;
            reloadingWeapon.WeaponId = weapon.Id;
            reloadingWeapon.WeaponReloadState = false;
            reloadingWeapon.Send();
        }

        private void Update()
        {
            if (skipUpdate)
            {
                return;
            }
            
            if (weapon.EntireAmmo == 0 || weapon.IsReloading)
            {
                return;
            }
            
            //Reload weapon
            if (weapon.CurrentAmmoInMagazine <= 0 && weapon.EntireAmmo > 0)
            {
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(Reload());
                }
                
                return;
            }

            if (!Input.GetButton("Fire1") || !(Time.time >= _nextTimeToFire) || weapon.EntireAmmo <= 0)
            {
                return;
            }
            
            _nextTimeToFire = Time.time + 1f / weapon.FireRate;

            if ((int)player.GetRigidbody().velocity.x != 0 || (int)player.GetRigidbody().velocity.z != 0)
            {
                Shoot(5);
            }
            else
            {
                Shoot(1);
            }
        }
        private void Shoot(int spreadFactor)
        {
            var weaponFired = WeaponFired.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            weaponFired.WeaponId = weapon.Id;
            weaponFired.Entity = weapon.Player;
            weaponFired.Send();
            
            var weaponSound = WeaponSound.Create(GlobalTargets.Everyone, ReliabilityModes.Unreliable);
            weaponSound.Position = transform.position;
            weaponSound.Label = weapon.Label;
            weaponSound.Player = player.entity;
            weaponSound.Send();
            
            var dir = Spread(spreadFactor);
            
            if (!Physics.Raycast(cameraTransform.transform.position, dir, out var hit, 100)) return;
            
            if (hit.transform.CompareTag("Stalin"))
            {
                AudioSource soundSource = hit.transform.GetComponent<AudioSource>();
                if (!soundSource.isPlaying)
                {
                    soundSource.Play();
                }
            }
            
            var boltEntity = hit.transform.GetComponent<BoltEntity>();
            
            if (boltEntity != null && boltEntity.GetState<IPlayer>() != null)
            {
                var playerHit = PlayerHit.Create(GlobalTargets.OnlyServer);
                playerHit.Target = boltEntity;
                playerHit.PlayerWhoShot = player.state.Username;
                playerHit.WeaponName = weapon.Label;
                playerHit.Damage = weapon.Damage;
                playerHit.Send();


                var applyHitIndicator = ApplyHitIndicator.Create(ReliabilityModes.Unreliable);
                applyHitIndicator.Message = "Damage: " + weapon.Damage;
                applyHitIndicator.Position = hit.point;
                applyHitIndicator.Send();
            }
            else
            {
                var impactEffect = ImpactEffect.Create(ReliabilityModes.Unreliable);
                impactEffect.Position = hit.point;
                impactEffect.Send();
            }
        }

        public IEnumerator Reload()
        {
            if (weapon.CurrentAmmoInMagazine == weapon.MagazineSize || weapon.EntireAmmo <= 0 || weapon.IsReloading)
            {
                yield break;
            }

            BeforeReload = weapon.CurrentAmmoInMagazine;
            cancelReload = false;
            
            if (cancelReload)
            {
                yield break;
            }
            
            //Send a command to notify that this weapon is reloading.
            var reloadingWeapon = ReloadingWeapon.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            reloadingWeapon.Player = weapon.Player;
            reloadingWeapon.WeaponReloadState = true;
            reloadingWeapon.WeaponId = weapon.Id;
            reloadingWeapon.Send();

            if (cancelReload)
            {
                yield break;
            }
            
            //Wait
            yield return new WaitForSeconds(weapon.ReloadTime - .25f);
            
            if (cancelReload)
            {
                yield break;
            }
            
            //Send a command to notify that this weapon has reloaded.
            reloadingWeapon = ReloadingWeapon.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            reloadingWeapon.Player = weapon.Player;
            reloadingWeapon.WeaponReloadState = false;
            reloadingWeapon.WeaponId = weapon.Id;
            reloadingWeapon.Send();
            
            if (cancelReload)
            {
                yield break;
            }
            
            //Wait
            yield return new WaitForSeconds(.25f);

            if (cancelReload)
            {
                yield break;
            }
            
            //Send a command to set the ammo in magazine.
            var weaponReloaded = WeaponReloaded.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            weaponReloaded.Player = weapon.Player;
            weaponReloaded.WeaponId = weapon.Id;
            weaponReloaded.NewCurrentAmount = weapon.EntireAmmo >= weapon.MagazineSize ? weapon.MagazineSize : weapon.EntireAmmo;
            weaponReloaded.Send();
        }
        
        private Vector3 Spread(int spreadFactor)
        {
            var direction = cameraTransform.forward;
            var sf = weapon.SpreadFactor * spreadFactor;
            
            direction.x += Random.Range(-sf, sf);
            direction.y += Random.Range(-sf, sf);
            direction.z += Random.Range(-sf, sf);

            return direction;
        }
    }
}