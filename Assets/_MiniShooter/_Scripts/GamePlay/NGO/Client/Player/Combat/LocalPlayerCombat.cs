using Core.Input;
using Core.TicksSystem;
using NGO.Server;
using NGO.Types;
using Unity.Netcode;
using UnityEngine;

namespace NGO.Client
{
    public sealed class LocalPlayerCombat : NetworkBehaviour, ITickable
    {
        private IInputSystem _inputSystem;
        private Transform _shotOrigin;
        private TickSystem _tickSystem;
        private PlayerCombatServer _server;
        private bool _isConstruct;
        
        public TickPhase UpdatePhase => TickPhase.ClientPhase;

        public void Construct(IInputSystem inputSystem, TickSystem tickSystem, Transform shotOrigin)
        {
            _inputSystem = inputSystem;
            _tickSystem = tickSystem;
            _shotOrigin = shotOrigin;
            
            _tickSystem.Register(this);
            _isConstruct = true;
        }

        public void SetServer(PlayerCombatServer server)
        {
            _server = server;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _isConstruct = false;
            _tickSystem.Unregister(this);
            _inputSystem = null;
            _tickSystem = null;
        }

        public void Tick(float deltaTime)
        {
            if (!_isConstruct)
                return;

            var snapshot = _inputSystem.Snapshot;
            if (snapshot.Context != InputContext.Gameplay || !snapshot.AttackInput.IsPressed)
                return;
            
            SendShootRequestRpc(new PlayerCombatRequest
            {
                Tick = _tickSystem.Tick,
                WeaponId = "rifle",
                Origin = _shotOrigin.position,
                Direction = _shotOrigin.forward,
            });
        }

        [Rpc(SendTo.Server)]
        private void SendShootRequestRpc(PlayerCombatRequest request)
        {
            _server.ShootRequest(request);
        }
    }
}