using DetentionRoom.Networking.States.Player;

namespace DetentionRoom.Networking.Utility
{
    public static class WeaponUtility
    {
        public static bool GetEquippedState(int id, Player player)
        {
            switch (id)
            {
                case 0:
                    return player.weaponSystem.chalkWeaponState.weapon.IsEquipped;
                case 1:
                    return player.weaponSystem.staplerWeaponState.weapon.IsEquipped;
                case 2:
                    return player.weaponSystem.slingshotWeaponState.weapon.IsEquipped;
                case 3:
                    return player.weaponSystem.spongeWeaponState.weapon.IsEquipped;
                case 4:
                    return player.weaponSystem.handsWeaponState.weapon.IsEquipped;
            }

            return false;
        }
    }
}