using System;
using Bolt;
using DetentionRoom.Networking.States.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace DetentionRoom.Networking.In_Game
{
    [BoltGlobalBehaviour(BoltNetworkModes.Server)]
    public class NetworkServerInGameCallbacks : GlobalEventListener
    {
        /// <summary>
        /// Get the correct weapon-id with the weapon-name
        /// </summary>
        /// <param name="weaponName"></param>
        /// <returns></returns>
        public static int ConvertWeaponNameToId(string weaponName)
        {
            switch (weaponName)
            {
                case "Chalk":
                    return 0;
                
                case "Stapler":
                    return 1;
                
                case "Slingshot":
                    return 2;
                
                case "Sponge":
                    return 3;
                
                case "Hands":
                    return 4;
                
                default: return -1;
            }
        }
        
        /// <summary>
        /// Get the correct weapon-name with the weapon-id
        /// </summary>
        /// <param name="weaponId"></param>
        /// <returns></returns>
        public static string ConvertWeaponIdToName(int weaponId)
        {
            switch (weaponId)
            {
                case 0:
                    return "Chalk";
                
                case 1:
                    return "Stapler";
                
                case 2:
                    return "Slingshot";
                
                case 3:
                    return "Sponge";
                
                case 4:
                    return "Hands";
                
                default: return "";
            }
        }

        /// <summary>
        /// Create a player instance and assign control to the player.
        /// </summary>
        /// <param name="playerInfo">Info from the lobby.</param>
        /// <param name="boltConnection">Connection of the players.</param>
        /// <param name="isHost">Is this player the host.</param>
        private static void CreatePlayer(IPlayerInfo playerInfo, BoltConnection boltConnection, bool isHost)
        {
            //If a player has selected no team, assign one randomly
            if (playerInfo.Team == Classes.Unassigned.ToString())
            {
                var random = Random.Range(0, 2);

                playerInfo.Team = random == 1 ? Classes.Teacher.ToString() : Classes.Student.ToString();
            }
            
            //Get all the spawn positions
            var spawnPoints = FindObjectsOfType<SpawnPosition>();
            
            //Initialize value to zero.
            var spawnPosition = Vector3.zero;
            
            //Get all the spawn positions of stage one and of the correct team
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint.stage == Stage.Dunno && spawnPoint.classType.ToString() == playerInfo.Team)
                {
                    spawnPosition = spawnPoint.transform.position;
                }
            }
            
            //Instantiate the player over the network at the correct position and at 0 rotation
            var networkPlayerEntity = BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPosition, Quaternion.identity);
            
            //Get the player state
            var iPlayer = networkPlayerEntity.GetState<IPlayer>();
            
            //Assign the team and username from the lobby
            iPlayer.Team = playerInfo.Team;
            iPlayer.Username = playerInfo.Username;
            iPlayer.Entity = networkPlayerEntity;
            
            if (isHost)
            {
                networkPlayerEntity.TakeControl();
            }
            else
            {
                networkPlayerEntity.AssignControl(boltConnection);
            }
        }

        /// <summary>
        /// Gets called whenever a player is hit.
        /// </summary>
        /// <param name="playerHit"></param>
        public override void OnEvent(PlayerHit playerHit)
        {
            //Get the player-state via the bolt-entity from the playerHit-event.
            var playerState = playerHit.Target.GetState<IPlayer>();
            
            //Reduce the health.
            playerState.Health -= playerHit.Damage;

            //Check if the player died.
            if (!(playerState.Health <= 0))
            {
                return;
            }
            
            //Reset Chalk
            playerState.Weapons[0].IsEquipped = true;
            playerState.Weapons[0].CurrentAmmoInMagazine = 0;
            playerState.Weapons[0].EntireAmmo = 0;
            
            //Reset Stapler
            playerState.Weapons[1].IsEquipped = false;
            playerState.Weapons[1].CurrentAmmoInMagazine = 0;
            playerState.Weapons[1].EntireAmmo = 0;
                
            //Reset Slingshot
            playerState.Weapons[2].IsEquipped = false;
            playerState.Weapons[2].CurrentAmmoInMagazine = 0;
            playerState.Weapons[2].EntireAmmo = 0;
                
            //Reset Sponge
            playerState.Weapons[3].IsEquipped = false;
            playerState.Weapons[3].CurrentAmmoInMagazine = 0;
            playerState.Weapons[3].EntireAmmo = 0;
                
            //Reset Health
            playerState.Health = 100;
            
            var respawn = Respawn.Create(playerState.Entity, EntityTargets.Everyone);
            respawn.PlayerToRespawn = playerState.Entity;
            respawn.KilledByPlayerName = playerHit.PlayerWhoShot;
            respawn.KilledByWeaponName = playerHit.WeaponName;
            respawn.Send();
        }
        
        /// <summary>
        /// Gets called whenever a player interacts with a ammo-supply.
        /// </summary>
        /// <param name="ammoPickedUp"></param>
        public override void OnEvent(AmmoPickedUp ammoPickedUp)
        {
            //Get the player-state of the player who interacted with it.
            var playerState = ammoPickedUp.Player.GetState<IPlayer>();

            //Get the weapon the ammo is intended to.
            var targetWeapon = playerState.Weapons[ConvertWeaponNameToId(ammoPickedUp.AmmoType)];

            //Get the ammo-pickup-state.
            var ammoPickupState = ammoPickedUp.ObjectEntity.GetState<IAmmoPickup>();
            
            //Cancel if player has no such weapon equipped or if the ammo-pickup has no more ammo left to pickup.
            if (!targetWeapon.IsEquipped || ammoPickupState.AmmoAmount <= 0)
            {
                return;
            }

            //Calculate the amount that can be added.
            var canAddAmount = targetWeapon.MaxCarryAmmo - targetWeapon.EntireAmmo;

            if (ammoPickupState.AmmoAmount >= canAddAmount)
            {
                targetWeapon.EntireAmmo += canAddAmount;
                ammoPickupState.AmmoAmount -= canAddAmount;
            }
            else
            {
                targetWeapon.EntireAmmo += ammoPickupState.AmmoAmount;
                ammoPickupState.AmmoAmount = 0;
            }
        }

        /// <summary>
        /// Gets called whenever a player interacts with a weapon-pickup.
        /// </summary>
        /// <param name="weaponPickedUp"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override void OnEvent(WeaponPickedUp weaponPickedUp)
        {
            //Get the player-state who interacted with the weapon-pickup.
            var playerState = weaponPickedUp.Player.GetState<IPlayer>();
            
            //Get the weapon-pickup-state.
            var weaponPickup = weaponPickedUp.ObjectEntity.GetState<IWeaponPickup>();
            
            //Get the target-weapon
            var weapon = playerState.Weapons[ConvertWeaponNameToId(weaponPickup.WeaponName)];
            
            //Cancel if player already has weapon equipped or the stock is empty.
            if (weapon.IsEquipped || weaponPickup.WeaponAmount == 0)
            {
                return;
            }
            
            //Set the data.
            weapon.IsEquipped = true;
            weapon.EntireAmmo = weapon.MagazineSize;
            weapon.CurrentAmmoInMagazine = weapon.MagazineSize;
            
            //Reduce the stock.
            weaponPickup.WeaponAmount--;

            if (!weapon.DestroyOnPickup)
            {
                return;
            }
            
            var destroySponge = DestroyGameObject.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            destroySponge.Entity = weaponPickedUp.ObjectEntity;
            destroySponge.Send();
        }
        
        /// <summary>
        /// Gets called whenever a sponge is fully charged to change the wet-state.
        /// </summary>
        /// <param name="rechargeSponge"></param>
        public override void OnEvent(RechargeSponge rechargeSponge)
        {
            //Get the player-state.
            var playerState = rechargeSponge.Player.GetState<IPlayer>();
            
            //Get the sponge and change the wet-state to true.
            playerState.Weapons[ConvertWeaponNameToId("Sponge")].IsWet = true;
            rechargeSponge.Player.gameObject.GetComponent<Player>().hud.RefreshWeaponDisplay();
        }
        
        /// <summary>
        /// Gets called whenever medic is picked-up.
        /// </summary>
        /// <param name="medicPickedUp"></param>
        public override void OnEvent(MedicPickedUp medicPickedUp)
        {
            //Get the player-state.
            var playerState = medicPickedUp.Player.GetState<IPlayer>();
            
            //Get the medic-pickup-state.
            var health = medicPickedUp.ObjectEntity.GetState<IMedicPickup>();

            //Calculate how much health the player can heal.
            var canHeal = 100 - (int)playerState.Health;
            
            //Cancel if medic-supply is empty or player health at max.
            if (health.MedicAmount == 0 || canHeal == 0)
            {
                return;
            }

            playerState.Health += canHeal;
            health.MedicAmount -= canHeal;
        }

        /// <summary>
        /// Gets called whenever a sponge needs to be instantiated over the network.
        /// </summary>
        /// <param name="spawnSponge"></param>
        public override void OnEvent(SpawnSponge spawnSponge)
        {
            BoltNetwork.Instantiate(BoltPrefabs.Sponge_Weapon_Pickup, spawnSponge.Position, Quaternion.identity);
        }

        /// <summary>
        /// Gets called whenever a weapon has been fired.
        /// </summary>
        /// <param name="weaponFired"></param>
        public override void OnEvent(WeaponFired weaponFired)
        {
            var weaponState = weaponFired.Entity.GetState<IPlayer>();
            weaponState.Weapons[weaponFired.WeaponId].CurrentAmmoInMagazine--;
            weaponState.Weapons[weaponFired.WeaponId].EntireAmmo--;

            if (ConvertWeaponIdToName(weaponFired.WeaponId) != "Sponge")
            {
                return;
            }
            
            weaponState.Weapons[weaponFired.WeaponId].IsWet = false;
            weaponState.Weapons[weaponFired.WeaponId].IsEquipped = false;
        }

        /// <summary>
        /// Gets called whenever a punch has been punched
        /// </summary>
        /// <param name="punchFired"></param>
        public override void OnEvent(PunchFired punchFired)
        {
            var playerState = punchFired.Player.GetState<IPlayer>();
            playerState.Weapons[punchFired.WeaponId].IsPunching = punchFired.IsPunching;
        }
        
        /// <summary>
        /// Gets called whenever a weapon has finished reloading to set new amount in current-magazine.
        /// </summary>
        /// <param name="weaponReloaded"></param>
        public override void OnEvent(WeaponReloaded weaponReloaded)
        {
            var playerState = weaponReloaded.Player.GetState<IPlayer>();
            playerState.Weapons[weaponReloaded.WeaponId].CurrentAmmoInMagazine = weaponReloaded.NewCurrentAmount;
        }

        /// <summary>
        /// Gets called whenever the weapon gets reloaded or cancelled.
        /// </summary>
        /// <param name="reloadingWeapon"></param>
        public override void OnEvent(ReloadingWeapon reloadingWeapon)
        {
            var playerState = reloadingWeapon.Player.GetState<IPlayer>();
            playerState.Weapons[reloadingWeapon.WeaponId].IsReloading =reloadingWeapon.WeaponReloadState;
        }

        /// <summary>
        /// Gets called to destroy a gameObject via network for all players.
        /// </summary>
        /// <param name="destroyGameObject"></param>
        public override void OnEvent(DestroyGameObject destroyGameObject)
        {
            BoltNetwork.Destroy(destroyGameObject.Entity);
        }

        /// <summary>
        /// Gets called whenever the equip-state has been changed.
        /// </summary>
        /// <param name="weaponEquipState"></param>
        public override void OnEvent(WeaponEquipState weaponEquipState)
        {
            var playerState = weaponEquipState.Player.GetState<IPlayer>();
            playerState.Weapons[weaponEquipState.WeaponId].IsEquipped = weaponEquipState.State;
        }

        /// <summary>
        /// Gets called whenever a player interacts with a door.
        /// </summary>
        /// <param name="openDoor"></param>
        public override void OnEvent(OpenDoor openDoor)
        {
            var door = openDoor.Entity.GetState<IDoor>();

            if (!(door.Cooldown <= 0))
            {
                return;
            }
            
            door.IsOpen = !door.IsOpen;
            door.Cooldown = 3;
        }
        
        public override void SceneLoadLocalDone(string map)
        {
            if(SceneManager.GetActiveScene().name != "PreAlphaMap")
            {
                return;
            }
            
            foreach (var entity in BoltNetwork.Entities)
            {
                if (!entity.StateIs<IPlayerInfo>() || !entity.IsOwner)
                {
                    continue;
                }
                
                var playerInfo = entity.GetState<IPlayerInfo>();
                CreatePlayer(playerInfo, null, true);
                BoltNetwork.Destroy(entity);
                break;
            }
        }

        public override void SceneLoadRemoteDone(BoltConnection connection)
        {
            if(SceneManager.GetActiveScene().name != "PreAlphaMap")
            {
                return;
            }

            foreach (var entity in BoltNetwork.Entities)
            {
                if (!entity.StateIs<IPlayerInfo>() || !entity.IsController(connection))
                {
                    continue;
                }
                
                var playerInfo = entity.GetState<IPlayerInfo>();
                CreatePlayer(playerInfo, connection, false);
                BoltNetwork.Destroy(entity);
                break;
            }
        }
        
        public override void Disconnected(BoltConnection connection)
        {
            foreach (var entity in BoltNetwork.Entities)
            {
                if (!entity.StateIs<IPlayer>() || !entity.IsController(connection))
                {
                    continue;
                }
                
                BoltNetwork.Destroy(entity);
                break;
            }
        }        
        
        
    }
}