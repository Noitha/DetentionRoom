using Bolt;
using DetentionRoom.Networking;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DetentionRoom.Scripts
{
    public class AmmoPickup : Pickup<IAmmoPickup>
    {
        public AmmoType ammoType;
        public int amount;
  
        public override void Attached()
        {
            if (entity.IsOwner)
            {
                state.AmmoType = ammoType.ToString();
                state.AmmoAmount = amount;
            }

            state.AddCallback("AmmoAmount", VisualizeAmount);
        }
        
        protected override void VisualizeAmount()
        {
            foreach (Transform item in container)
            {
                Destroy(item.gameObject);   
            }

            for (var i = 0; i < state.AmmoAmount; i++)
            {
                var chalk = Instantiate(itemToVisualize, container);
                chalk.transform.localPosition = new Vector3(Random.Range(-.5f, .5f),.1f,Random.Range(-.5f, .5f));
                chalk.transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            }
        }

        protected override void Refill()
        {
            if (!entity.IsAttached || !entity.IsOwner)
            {
                return;
            }

            state.AmmoAmount = amount;
        }

        public override void Interact(BoltEntity boltEntity)
        {
            var ammoPickedUp = AmmoPickedUp.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            ammoPickedUp.Player = boltEntity;
            ammoPickedUp.AmmoType = state.AmmoType;
            ammoPickedUp.AmmoAmount = state.AmmoAmount;
            ammoPickedUp.ObjectEntity = entity;
            ammoPickedUp.Send();
        }
        
        public override string Focus()
        {
            return "Pickup " + state.AmmoAmount + " " + state.AmmoType + " Ammo";
        }
    }
}