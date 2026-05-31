using System.Collections.Generic;
using Core.DIServiceLocator;
using Core.NGO.Types;
using Core.TicksSystem;
using NGO.Client;
using NGO.Types;
using Visual.Player;
using Unity.Netcode;

namespace NGO.Server
{
    public class PlayerMovementServer : NetworkBehaviour, ITickable
    {
        private PlayerMovementSimulation _simulation;
        private LocalPlayerMovementPrediction _prediction;
        private RemotePlayerMovement _remotePlayerMovement;
        private TickSystem _tickSystem;
        private readonly Queue<PlayerMovementInput> _inputQueue = new();
        private PlayerMovementState _authorityState;
        
        public TickPhase UpdatePhase => TickPhase.ServerPhase;
        
        public void Construct(PlayerMovementSimulation simulation, LocalPlayerMovementPrediction prediction, RemotePlayerMovement remotePlayerMovement)
        {
            _simulation = simulation;
            _prediction = prediction;
            _remotePlayerMovement = remotePlayerMovement;
            
            _tickSystem = ServiceLocator.Get<TickSystem>();
            _tickSystem.Register(this);
            
            _authorityState = new PlayerMovementState
            {
                Tick = _tickSystem.Tick,
                Position = transform.position,
            };
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _tickSystem.Unregister(this);
        }

        public void Tick(float deltaTime)
        {
            if (_inputQueue.Count <= 0)
            {
                UpdateSimulate(deltaTime);
                return;
            }
            
            while (_inputQueue.Count > 0)
            {
                var input = _inputQueue.Dequeue();
                _authorityState.Tick = _tickSystem.Tick;
                
                input.Tick = _tickSystem.Tick;
                _simulation.SimulateMovement(input, ref _authorityState);
            }

            transform.position = _authorityState.Position;
            SendAuthorityStateRpc(_authorityState);
        }
        
        public void CheckMovement(PlayerMovementInput input)
        {
            _inputQueue.Enqueue(input);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SendAuthorityStateRpc(PlayerMovementState state)
        {
            _prediction.ReceiveAuthorityState(state);
            _remotePlayerMovement.ReceiveStateSnapshot(state);
        }

        private void UpdateSimulate(float deltaTime)
        {
            _authorityState.Tick = _tickSystem.Tick;

            _simulation.SimulateMovement(
                new PlayerMovementInput { Tick = _tickSystem.Tick, DeltaTime = deltaTime },
                ref _authorityState
            );

            transform.position = _authorityState.Position;
        }
    }
}