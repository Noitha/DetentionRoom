using Bolt;

namespace DetentionRoom.Scripts
{
    public class MedicPickup : Pickup<IMedicPickup>
    {
        public int amount;
        
        public override void Attached()
        {
            if (!entity.IsOwner)
            {
                return;
            }
            
            state.MedicAmount = amount;
        }

        protected override void VisualizeAmount(){}

        protected override void Refill()
        {
            if (!entity.IsAttached || !entity.IsOwner)
            {
                return;
            }

            state.MedicAmount = amount;
        }

        public override void Interact(BoltEntity boltEntity)
        {
            var medicPickup = MedicPickedUp.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            medicPickup.Player = boltEntity;
            medicPickup.Amount = state.MedicAmount;
            medicPickup.ObjectEntity = entity;
            medicPickup.Send();
        }
        
        public override string Focus()
        {
            return "Pickup " + state.MedicAmount + " Health";
        }
    }
}