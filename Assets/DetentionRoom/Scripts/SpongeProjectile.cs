using System;
using Bolt;
using DetentionRoom.Networking.States.Player;
using UnityEngine;

namespace DetentionRoom.Scripts
{
    public class SpongeProjectile : MonoBehaviour
    {
        private readonly Vector3 _groundCheck = new Vector3(0, -.05f, 0);
        private bool _grounded;

        private Rigidbody _rigidBody;
        public Guid guid;
        private bool _launch;

        public Player player;
    
        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _launch = false;
        }

        public void Launch(Vector3 force, Guid guid, BoltEntity entity)
        {
            player = entity.gameObject.GetComponent<Player>();
            this.guid = guid;
            _rigidBody.AddForce(force);
            _launch = true;
        }
    
        private void FixedUpdate()
        {
            if (_grounded || !_launch) return;

            if (transform.position.y <= 0)
            {
                _grounded = true;
                var t = transform.position;
                t.y = 0.2f;
                transform.position = t;
            }
        
            var groundRay = new Ray(transform.position, _groundCheck);
            
            if (!Physics.Raycast(groundRay, out _, 0.25f, 9))
            {
                return;
            }

            _grounded = true;


            var spawnSponge = SpawnSponge.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            spawnSponge.Position = transform.position;
            spawnSponge.Type = "Weapon-Sponge";
            spawnSponge.Send();

            var createSplash = CreateSplash.Create(GlobalTargets.Everyone, ReliabilityModes.ReliableOrdered);
            createSplash.Position = transform.position;
            createSplash.Radius = player.weaponSystem.spongeWeaponState.weapon.Radius;
            createSplash.Damage = player.weaponSystem.spongeWeaponState.weapon.Damage;
            createSplash.PlayerWhoShot = player.entity;
            createSplash.Send();

            var destroySpongeOnHit = DestroySpongeOnHit.Create(GlobalTargets.Everyone, ReliabilityModes.ReliableOrdered);
            destroySpongeOnHit.SpongeId = guid;
            destroySpongeOnHit.Send();
        }
    }
}