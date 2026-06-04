using System.Collections.Generic;
using Core.TicksSystem;
using NGO.Client;
using NGO.Types;
using Simulation.Player.Weapons;
using Unity.Netcode;

namespace NGO.Server
{
    public sealed class PlayerCombatServer : NetworkBehaviour, ITickable
    {
        private TickSystem _tickSystem;
        private LocalPlayerCombat _client;
        private readonly Queue<PlayerCombatRequest> _requests = new();
        private WeaponSimulation[] _weapons;
        private bool _isConstruct;
        
        public TickPhase UpdatePhase => TickPhase.ServerPhase;
        
        public void Construct(TickSystem tickSystem, WeaponSimulation[] weapons)
        {
            _tickSystem = tickSystem;
            _weapons = weapons;
            _tickSystem.Register(this);
            _isConstruct = true;
        }

        public void SetClient(LocalPlayerCombat client)
        {
            _client = client;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _isConstruct = false;
            _tickSystem.Unregister(this);
            _tickSystem = null;
        }

        public void Tick(float deltaTime)
        {
            if (!_isConstruct)
                return;
            
            while (_requests.Count > 0)
            {
                var request = _requests.Dequeue();
                var weapon = _weapons[0];
                weapon.Simulate(request.Origin, request.Direction);
            }
        }

        public void ShootRequest(PlayerCombatRequest request)
        {
            _requests.Enqueue(request);
        }
    }
}