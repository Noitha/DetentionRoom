using DetentionRoom.Networking.States.Player;
using DetentionRoom.Networking.States.Weapons;
using DetentionRoom.Networking.Utility;
using UnityEngine;

namespace DetentionRoom.Scripts.Weapon
{
    public class WeaponSystem : MonoBehaviour
    {
        private Player Player { get; set; }
        
        public GameObject[] allWeapons;
        
        public BaseRayCastWeapon chalkWeaponState;
        public BaseRayCastWeapon staplerWeaponState;
        public BaseRayCastWeapon slingshotWeaponState;
        public BaseThrowableWeapon spongeWeaponState;
        public BaseMeleeWeapon handsWeaponState;
        
        
        
        
        
        
        
        private void Awake()
        {
            Player = transform.root.GetComponent<Player>();
        }
        private void Update()
        {
            if (!Player.entity.HasControl)
            {
                return;
            }

            var currentWeapon = Player.state.Weapons[Player.state.ActiveWeaponIndex];
            
            int nextWeaponIndex = Player.state.ActiveWeaponIndex;
            
            if (Player.PlayerInput.ScrollWheel > 0f)
            {
                GetReloadingWeapon(currentWeapon)?.InterruptReload();
                
                nextWeaponIndex = FindNextWeaponIndex(nextWeaponIndex);
            }
            
            if (Player.PlayerInput.ScrollWheel < 0f)
            {
                var found = false;
                
                while (!found)
                {
                    nextWeaponIndex--;
                    
                    if (nextWeaponIndex < 0)
                    {
                        nextWeaponIndex = allWeapons.Length - 1;
                    }
                    
                    if (WeaponUtility.GetEquippedState(nextWeaponIndex, Player))
                    {
                        found = true;
                    }
                    
                    GetReloadingWeapon(currentWeapon)?.InterruptReload();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha1) && WeaponUtility.GetEquippedState(0, Player))
            {
                nextWeaponIndex = 0;
                GetReloadingWeapon(currentWeapon)?.InterruptReload();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && WeaponUtility.GetEquippedState(1, Player))
            {
                nextWeaponIndex = 1;
                GetReloadingWeapon(currentWeapon)?.InterruptReload();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) && WeaponUtility.GetEquippedState(2, Player))
            {
                nextWeaponIndex = 2;
                GetReloadingWeapon(currentWeapon)?.InterruptReload();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) && WeaponUtility.GetEquippedState(3, Player))
            {
                nextWeaponIndex = 3;
                GetReloadingWeapon(currentWeapon)?.InterruptReload();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5) && WeaponUtility.GetEquippedState(4, Player))
            {
                nextWeaponIndex = 4;
                GetReloadingWeapon(currentWeapon)?.InterruptReload();
            }

            if (nextWeaponIndex != Player.state.ActiveWeaponIndex)
            {
                Player.ChangeWeaponIndex(nextWeaponIndex);
            }
        }
        public int FindNextWeaponIndex(int nextWeaponIndex)
        {
            var found = false;

            while (!found)
            {
                nextWeaponIndex++;

                if (nextWeaponIndex >= allWeapons.Length)
                {
                    nextWeaponIndex = 0;
                }

                if (WeaponUtility.GetEquippedState(nextWeaponIndex, Player))
                {
                    found = true;
                }
            }

            return nextWeaponIndex;
        }




        private BaseWeapon GetReloadingWeapon(global::Weapon weapon)
        {
            if (!weapon.IsReloading)
            {
                return null;
            }

            switch(weapon.Id)
            {
                case 0: return chalkWeaponState;
                case 1: return staplerWeaponState;
                case 2: return slingshotWeaponState;
                default: return null;
            }
        }
    }
}