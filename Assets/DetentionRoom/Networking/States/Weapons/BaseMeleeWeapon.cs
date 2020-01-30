using System.Collections;
using Bolt;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DetentionRoom.Networking.States.Weapons
{
    public class BaseMeleeWeapon : BaseWeapon
    {
        private float _nextTimeToFire;
        
        private void Update()
        {
            if (skipUpdate)
            {
                return;
            }
            
            if (!Input.GetButtonDown("Fire1") || player.state.Weapons[weapon.Id].IsPunching || !(Time.time >= _nextTimeToFire))
            {
                return;
            }
            
            Punch();
            StartCoroutine(Punching());
        }

        private void Punch()
        {
            var weaponSound = WeaponSound.Create(GlobalTargets.Everyone, ReliabilityModes.Unreliable);
            weaponSound.Position = transform.position;
            weaponSound.Label = weapon.Label;
            weaponSound.Player = player.entity;
            weaponSound.Send();
            
            /*if (!Physics.SphereCast(player.playerCamera.transform.position, weapon.Radius,
                player.playerCamera.transform.forward, out var hit,
                weapon.MaxDistance)) return;*/

            var hits = Physics.SphereCastAll(player.playerCamera.transform.position, weapon.Radius,
                player.playerCamera.transform.forward, weapon.MaxDistance);
            
            foreach (var hit in hits)
            {
                var boltEntity = hit.transform.GetComponent<BoltEntity>();

                if (boltEntity == null || boltEntity.GetState<IPlayer>() == null || boltEntity.GetState<IPlayer>().Entity == player.entity)
                {
                    return;
                }
                
                var playerHit = PlayerHit.Create(GlobalTargets.OnlyServer);
                playerHit.Target = boltEntity;
                playerHit.PlayerWhoShot = player.state.Username;
                playerHit.WeaponName = weapon.Label;
                playerHit.Damage = weapon.Damage;
                playerHit.Send();
            }
        }


        private IEnumerator Punching()
        {
            canSwitch = false;
            var punchStart = PunchFired.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            punchStart.WeaponId = weapon.Id;
            punchStart.Player = weapon.Player;
            punchStart.IsPunching = true;
            punchStart.Send();
            
            yield return new WaitForSeconds(.4f);

            var punchFinish = PunchFired.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            punchFinish.WeaponId = weapon.Id;
            punchFinish.Player = weapon.Player;
            punchFinish.IsPunching = false;
            punchFinish.Send();
            
            _nextTimeToFire = Time.time + 1f / weapon.FireRate;
            canSwitch = true;
        }
    }
}