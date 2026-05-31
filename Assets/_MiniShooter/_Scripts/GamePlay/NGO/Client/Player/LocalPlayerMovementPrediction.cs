using System.Collections.Generic;
using System.Linq;
using Core.DIServiceLocator;
using Core.Input;
using Core.NGO.Types;
using Core.TicksSystem;
using NGO.Server;
using NGO.Types;
using Visual.Player;
using Unity.Netcode;
using UnityEngine;

namespace NGO.Client
{
    public sealed class LocalPlayerMovementPrediction : NetworkBehaviour, ITickable
    {
        [SerializeField] private float _positionThreshold;
        
        private PlayerMovementSimulation _simulation; 
        private PlayerMovementServer _server;
        private IInputSystem _inputSystem;
        private TickSystem _tickSystem;
        private readonly List<PlayerMovementInput> _inputHistory = new();
        private readonly Dictionary<int, PlayerMovementState> _predictedStates = new();
        private PlayerMovementState _currentState;
        private int _lastProcessedServerTick;
        private bool _constructed;

        public TickPhase UpdatePhase => TickPhase.ClientPhase;
        
        public void Construct(PlayerMovementSimulation simulation)
        {
            _simulation = simulation;
            _inputSystem = ServiceLocator.Get<IInputSystem>();
            _tickSystem = ServiceLocator.Get<TickSystem>();
            _tickSystem.Register(this);

            _currentState = new PlayerMovementState
            {
                Tick = _tickSystem.Tick,
                Position = transform.position,
            };

            _constructed = true;
        }

        public void SetServer(PlayerMovementServer server)
        {
            _server = server;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _tickSystem.Unregister(this);
            _constructed = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_constructed || _inputSystem.Snapshot.Context != InputContext.Gameplay)
                return;
            
            var input = new PlayerMovementInput
            {
                Tick = _tickSystem.Tick,
                MoveInput = _inputSystem.Snapshot.MoveInput,
                IsSprint = _inputSystem.Snapshot.SprintInput.IsPressed,
                IsJump = _inputSystem.Snapshot.JumpInput.IsStarted,
                DeltaTime = deltaTime,
            };
            
            _currentState.Tick = _tickSystem.Tick;
            _simulation.SimulateMovement(input, ref _currentState);
            
            _inputHistory.Add(input);
            _predictedStates.Add(_currentState.Tick, _currentState);
            
            SendMovementRpc(input);
        }

        public void ReceiveAuthorityState(PlayerMovementState serverState)
        {
            if (!_constructed)
                return;
            
            if (serverState.Tick <= _lastProcessedServerTick)
                return;
            
            if (!_predictedStates.TryGetValue(serverState.Tick, out var clientState))
                return;
            
            _lastProcessedServerTick = serverState.Tick;

            if (Vector3.Distance(clientState.Position, serverState.Position) <= _positionThreshold)
                return;
            
            var inputs = _inputHistory.Where(x => x.Tick > serverState.Tick).ToArray();
            _inputHistory.RemoveAll(x => x.Tick <= serverState.Tick);
            var keysToRemove = _predictedStates.Keys.Where(x => x > serverState.Tick).ToArray();

            foreach (var key in keysToRemove)
            {
                _predictedStates.Remove(key);
            }
            
            var state = serverState;
            
            foreach (var input in inputs)
            {
                state.Tick = input.Tick;
                _simulation.SimulateMovement(input, ref state);
                _predictedStates[input.Tick] = state;
            } 
            
            _currentState = state;
        }

        private void Update()
        {
            if (!_constructed)
                return;
            
            if (Vector3.Distance(transform.position, _currentState.Position) <= _positionThreshold)
                return;
            
            transform.position = _currentState.Position;
        }

        [Rpc(SendTo.Server)]
        private void SendMovementRpc(PlayerMovementInput input)
        {
            _server.CheckMovement(input);
        }
    }
}