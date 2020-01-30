using Bolt;
using DetentionRoom.Networking.States.Player;
using DetentionRoom.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetentionRoom.Networking.In_Game
{
    public class NetworkCallbacks : GlobalEventListener
    {
        public GameObject impactEffect;
        public GameObject splashEffect;
        
        public ApplyDamageIndicator applyDamageIndicator;

        public SoundSpawner soundSpawner;
        
        public SpongeProjectile spongeProjectile;

 
        public override void OnEvent(ImpactEffect ie)
        {
            var impact = Instantiate(impactEffect, ie.Position, Quaternion.identity);
            Destroy(impact, 1f);
        }

        public override void OnEvent(ApplyHitIndicator applyHitIndicator)
        {
            var damageIndicator = Instantiate(applyDamageIndicator, applyHitIndicator.Position, Quaternion.identity);
            damageIndicator.Display(applyHitIndicator.Message);
        }

        public override void OnEvent(WeaponSound weaponSound)
        {
            var newWeaponSound = Instantiate(soundSpawner);
            newWeaponSound.Play(weaponSound.Label, weaponSound.Player);
        }

        public override void OnEvent(CreateSplash createSplash)
        {
            var splash = Instantiate(splashEffect, createSplash.Position, Quaternion.identity);
            Destroy(splash, 2f);

            var hits = Physics.SphereCastAll(createSplash.Position, createSplash.Radius, Vector3.one);

            foreach (var hit in hits)
            {
                var player = hit.transform.GetComponent<Player>();
                if (player == null)
                {
                    continue;
                }

                if (player.entity.HasControl)
                {
                    var playerHit = PlayerHit.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
                    playerHit.Damage = createSplash.Damage;
                    playerHit.Target = player.entity;
                    playerHit.WeaponName = "Sponge";
                    playerHit.PlayerWhoShot = createSplash.PlayerWhoShot.GetState<IPlayer>().Username;
                    playerHit.Send();
                    
                    break;
                }
            }
        }
        
        public override void OnEvent(ThrowSponge throwSponge)
        {
            var projectile = Instantiate(spongeProjectile, throwSponge.Position, Quaternion.identity);
            projectile.Launch(throwSponge.Direction * throwSponge.Force, throwSponge.SpongeId, throwSponge.Player);
        }
        
        public override void OnEvent(DestroySpongeOnHit destroySpongeOnHit)
        {
            foreach (var projectile in FindObjectsOfType<SpongeProjectile>())
            {
                if (projectile.guid != destroySpongeOnHit.SpongeId)
                {
                    continue;
                }
                
                Destroy(projectile.gameObject);
                break;
            }
        }
        
        public override void Disconnected(BoltConnection connection)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}