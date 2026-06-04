using System.Collections.Generic;
using Core.NGO.Types;
using Unity.Netcode;
using UnityEngine;

namespace NGO.Client
{
    public sealed class RemotePlayerMovement : NetworkBehaviour
    {
        [SerializeField] private int _interpolationDelayTicks = 3;
        [SerializeField] private int _maxSnapshots = 32;

        private readonly List<PositionSnapshot> _snapshots = new();

        private int _latestReceivedTick;
        private float _timer;
        private bool _constructed;
        private bool _hasSnapshots;
        
        public void Construct()
        {
            _constructed = true;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _constructed = false;
            _hasSnapshots = false;
            _timer = 0f;
            _snapshots.Clear();
        }

        public void ReceiveStateSnapshot(PlayerMovementState state)
        {
            if (!_constructed)
                return;

            var snapshot = new PositionSnapshot(state.Tick, state.Position);

            _latestReceivedTick = Mathf.Max(_latestReceivedTick, state.Tick);

            _snapshots.Add(snapshot);
            _snapshots.Sort((a, b) => a.Tick.CompareTo(b.Tick));

            while (_snapshots.Count > _maxSnapshots)
                _snapshots.RemoveAt(0);
        }

        private void Update()
        {
            if (!_constructed)
                return;

            if (_snapshots.Count < 2)
                return;

            var renderTick = _latestReceivedTick - _interpolationDelayTicks;

            PositionSnapshot? from = null;
            PositionSnapshot? to = null;

            for (var i = 0; i < _snapshots.Count - 1; i++)
            {
                if (_snapshots[i].Tick <= renderTick && _snapshots[i + 1].Tick >= renderTick)
                {
                    from = _snapshots[i];
                    to = _snapshots[i + 1];
                    break;
                }
            }

            if (!from.HasValue || !to.HasValue)
            {
                transform.position = _snapshots[^1].Position;
                return;
            }

            var fromSnapshot = from.Value;
            var toSnapshot = to.Value;

            var tickRange = Mathf.Max(1, toSnapshot.Tick - fromSnapshot.Tick);
            var t = (renderTick - fromSnapshot.Tick) / (float)tickRange;

            transform.position = Vector3.Lerp(
                fromSnapshot.Position,
                toSnapshot.Position,
                t
            );

            _snapshots.RemoveAll(x => x.Tick < renderTick - _interpolationDelayTicks);
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