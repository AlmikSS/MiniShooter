using Core.NGO.Types;
using Unity.Netcode;
using UnityEngine;

namespace NGO.Client
{
    public sealed class RemotePlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float _interpolateTime;
        
        private PlayerMovementState _authorityState;
        private bool _constructed;
        
        public void Construct()
        {
            _constructed = true;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _constructed = false;
        }

        public void ReceiveStateSnapshot(PlayerMovementState state)
        {
            _authorityState = state;
        }

        private void Update()
        {
            if (!_constructed)
                return;
            
            if (transform.position == _authorityState.Position)
                return;
            
            transform.position = Vector3.Lerp(transform.position, _authorityState.Position, Time.deltaTime * _interpolateTime);
        }
    }
}