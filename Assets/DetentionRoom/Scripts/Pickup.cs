using Bolt;
using DetentionRoom.Networking;
using DetentionRoom.Networking.Interfaces;
using UnityEngine;

namespace DetentionRoom.Scripts
{
    public abstract class Pickup<T> : EntityBehaviour<T>, IInteractable, IFocusable
    {
        private readonly Vector3 _groundCheck = new Vector3(0, -.05f, 0);
        public Transform originOfGroundCheckPosition;
        private bool _onGround;
        private Rigidbody _rigidBody;
        
        
        
        public Transform container;
        public GameObject itemToVisualize;
        
        
        public float respawnRate;

        protected abstract void VisualizeAmount();
        
        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }

        protected virtual void Start()
        {
            InvokeRepeating(nameof(Refill), 0, respawnRate);
        }

        protected abstract void Refill();

        public abstract void Interact(BoltEntity boltEntity);

        private void FixedUpdate()
        {
            var groundRay = new Ray(originOfGroundCheckPosition.position, _groundCheck);
            _onGround = Physics.Raycast(groundRay, out _, 0.25f);

            var velocity = Vector3.zero;
            velocity.y = _onGround ? 0 : -9.81f;

            _rigidBody.velocity = velocity;
        }

        public abstract string Focus();
    }
}