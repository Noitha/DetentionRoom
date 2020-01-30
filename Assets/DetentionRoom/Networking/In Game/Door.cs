using System;
using Bolt;
using DetentionRoom.Networking.Interfaces;
using UnityEngine;

namespace DetentionRoom.Networking.In_Game
{
    public class Door : EntityEventListener<IDoor>, IInteractable, IFocusable
    {
        public enum Rotation
        {
            Zero,
            Ninety,
            OneHundredEighty,
            TwoHundredSeventy,
            MinusNinety,
            MinusOneHundredEighty,
            MinusTwoHundredSeventy
        }

        public Rotation openRotation;
        public Rotation closeRotation;
        
        public Vector3 openPosition;
        public Vector3 closePosition;

        public override void Attached()
        {
            if (entity.IsOwner)
            {
                state.Cooldown = 0;
                state.IsOpen = false;
            }
            
            state.AddCallback("IsOpen", StateChanged);
        }
        
        public void Interact(BoltEntity boltEntity)
        {
            var openDoor = OpenDoor.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
            openDoor.Entity = entity;
            openDoor.Send();
        }

        public override void SimulateOwner()
        {
            if (state.Cooldown > 0)
            {
                state.Cooldown -= BoltNetwork.FrameDeltaTime;
            }
        }
        
        private void StateChanged()
        {
            if (state.IsOpen)
            {
                switch (openRotation)
                {
                    case Rotation.Zero:
                        transform.localEulerAngles = new Vector3(0,0,0);
                        break;
                    case Rotation.Ninety:
                        transform.localEulerAngles = new Vector3(0,90,0);
                        break;
                    case Rotation.OneHundredEighty:
                        transform.localEulerAngles = new Vector3(0,180,0);
                        break;
                    case Rotation.TwoHundredSeventy:
                        transform.localEulerAngles = new Vector3(0,270,0);
                        break;
                    case Rotation.MinusNinety:
                        transform.localEulerAngles = new Vector3(0,-90,0);
                        break;
                    case Rotation.MinusOneHundredEighty:
                        transform.localEulerAngles = new Vector3(0,-180,0);
                        break;
                    case Rotation.MinusTwoHundredSeventy:
                        transform.localEulerAngles = new Vector3(0,-270,0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (closeRotation)
                {
                    case Rotation.Zero:
                        transform.localEulerAngles = new Vector3(0,0,0);
                        break;
                    case Rotation.Ninety:
                        transform.localEulerAngles = new Vector3(0,90,0);
                        break;
                    case Rotation.OneHundredEighty:
                        transform.localEulerAngles = new Vector3(0,180,0);
                        break;
                    case Rotation.TwoHundredSeventy:
                        transform.localEulerAngles = new Vector3(0,270,0);
                        break;
                    case Rotation.MinusNinety:
                        transform.localEulerAngles = new Vector3(0,-90,0);
                        break;
                    case Rotation.MinusOneHundredEighty:
                        transform.localEulerAngles = new Vector3(0,-180,0);
                        break;
                    case Rotation.MinusTwoHundredSeventy:
                        transform.localEulerAngles = new Vector3(0,-270,0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            transform.localPosition = state.IsOpen ? openPosition : closePosition;
        }

        public string Focus()
        {
            return state.IsOpen ? "Press [e] to close door" : "Press [e] to open door";
        }
    }
}