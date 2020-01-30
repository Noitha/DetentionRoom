using Bolt;
using DetentionRoom.Networking;

namespace DetentionRoom.Scripts
{
    public class WeaponPickup : Pickup<IWeaponPickup>
    {
        public WeaponType weaponType;
        public int amount;
        
        /// <summary>
        /// Initialize when object is recognized as a network-object-
        /// </summary>
        public override void Attached()
        {
            if (!entity.IsOwner)
            {
                return;
            }

            state.WeaponName = weaponType.ToString();
            state.WeaponAmount = amount;
        }

        protected override void VisualizeAmount(){}

        protected override void Refill()
        {
            if (!entity.IsAttached || !entity.IsOwner)
            {
                return;
            }

            state.WeaponAmount = 1;
        }

        public override void Interact(BoltEntity boltEntity)
        {
            var weaponPickedUp = WeaponPickedUp.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            weaponPickedUp.Player = boltEntity;
            weaponPickedUp.WeaponType = weaponType.ToString();
            weaponPickedUp.ObjectEntity = entity;
            weaponPickedUp.Send();
        }

        public override string Focus()
        {
            return state.WeaponAmount == 1 ? "Pickup " + state.WeaponName : "Empty";
        }
    }
}