using System.Collections.Generic;
using Core.DIServiceLocator;
using Core.NGO.Types;
using Core.TicksSystem;
using NGO.Client;
using NGO.Types;
using Simulation.Player;
using Unity.Netcode;
using UnityEngine;

namespace NGO.Server
{
    public class PlayerMovementServer : NetworkBehaviour, ITickable
    {
        private Transform _orientationTransform;
        private PlayerMovementSimulation _simulation;
        private LocalPlayerMovementPrediction _prediction;
        private RemotePlayerMovement _remotePlayerMovement;
        private TickSystem _tickSystem;
        private readonly Queue<PlayerMovementInput> _inputQueue = new();
        private PlayerMovementState _authorityState;
        
        public TickPhase UpdatePhase => TickPhase.ServerPhase;
        
        public void Construct(PlayerMovementSimulation simulation, Transform orientationTransform, TickSystem tickSystem)
        {
            _orientationTransform = orientationTransform;
            _simulation = simulation;
            _tickSystem = tickSystem;
            _tickSystem.Register(this);
            
            _authorityState = new PlayerMovementState
            {
                Tick = _tickSystem.Tick,
                Position = transform.position,
                OrientationRotation = _orientationTransform.eulerAngles,
            };
        }

        public void SetClient(LocalPlayerMovementPrediction prediction, RemotePlayerMovement remotePlayerMovement)
        {
            _prediction = prediction;
            _remotePlayerMovement = remotePlayerMovement;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _tickSystem.Unregister(this);
        }

        public void Tick(float deltaTime)
        {
            var processedAnyInput = false;
            
            while (_inputQueue.Count > 0)
            {
                var input = _inputQueue.Dequeue();
                _authorityState.Tick = input.Tick;
                _authorityState.OrientationRotation = _orientationTransform.eulerAngles;
                input.DeltaTime = deltaTime;
                _simulation.SimulateMovement(input, ref _authorityState);
                processedAnyInput = true;
            }

            if (!processedAnyInput)
            {
                _authorityState.Tick = _tickSystem.Tick;
                _authorityState.OrientationRotation = _orientationTransform.eulerAngles;

                _simulation.SimulateMovement(
                    new PlayerMovementInput
                    {
                        Tick = _tickSystem.Tick,
                        DeltaTime = deltaTime,
                    },
                    ref _authorityState
                );
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
    }
}