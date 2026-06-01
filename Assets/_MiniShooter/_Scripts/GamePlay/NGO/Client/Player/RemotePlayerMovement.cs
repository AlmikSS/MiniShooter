using System.Collections.Generic;
using Core.DIServiceLocator;
using Core.NGO.Types;
using Core.TicksSystem;
using Unity.Netcode;
using UnityEngine;

namespace NGO.Client
{
    public sealed class RemotePlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float _interpolateTime;
        
        private readonly Queue<PositionSnapshot> _snapshotQueue = new();
        private PositionSnapshot _from;
        private PositionSnapshot _to;
        private float _timer;
        private float _tickInterval;
        private bool _constructed;
        
        public void Construct()
        {
            _constructed = true;
            _tickInterval = ServiceLocator.Get<TickSystem>().TickInterval;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _constructed = false;
        }

        public void ReceiveStateSnapshot(PlayerMovementState state)
        {
            if (!_constructed)
                return;

            var snapshot = new PositionSnapshot(state.Tick, state.Position);
            
            if (_snapshotQueue.Count == 0)
            {
                _from = snapshot;
                _to = snapshot;

                transform.position = snapshot.Position;
            }
            
            _snapshotQueue.Enqueue(snapshot);
        }

        private void Update()
        {
            if (!_constructed)
                return;
            
            if (_snapshotQueue.Count == 0)
                return;
            
            _timer += Time.deltaTime / _tickInterval;
            transform.position = Vector3.Lerp(_from.Position, _to.Position, _timer);

            if (!(_timer >= 1f))
                return;
            
            if (_snapshotQueue.Count <= 0)
                return;
            
            _from = _to;
            _to = _snapshotQueue.Dequeue();
            _timer = 0f;
        }
    }

    public struct PositionSnapshot
    {
        public int Tick;
        public Vector3 Position;

        public PositionSnapshot(int tick, Vector3 position)
        {
            Tick = tick;
            Position = position;
        }
    }
}