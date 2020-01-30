using System;
using Bolt;
using UnityEngine;

namespace DetentionRoom.Networking.States.Weapons
{
    public class BaseThrowableWeapon : BaseWeapon
    {
        private void Update()
        {
            if (skipUpdate)
            {
                return;
            }
            
            if (!Input.GetButtonDown("Fire1") || !weapon.IsWet) return;
            Shoot();
        }

        private void Shoot()
        {
            //spawn projectile
            var throwSponge = ThrowSponge.Create(GlobalTargets.Everyone, ReliabilityModes.ReliableOrdered);
            throwSponge.Position = player.playerCamera.transform.position + player.playerCamera.transform.forward * .5f;
            throwSponge.Direction = player.playerCamera.transform.forward;
            throwSponge.Force = weapon.Force;
            throwSponge.SpongeId = new Guid();
            throwSponge.Player = player.entity;
            throwSponge.Send();
            
            
            var weaponFired = WeaponFired.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            weaponFired.WeaponId = weapon.Id;
            weaponFired.Entity = weapon.Player;
            weaponFired.Send();
            
            player.ChangeWeaponIndex(player.weaponSystem.FindNextWeaponIndex(player.state.ActiveWeaponIndex));
        }
    }
}