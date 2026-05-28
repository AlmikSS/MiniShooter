using Core.Input;
using Core.TicksSystem;
using Unity.Netcode;
using UnityEngine;

namespace Core.EntryPoints
{
    public class GamePlayNetworkEntryPoint : NetworkBehaviour
    {
        [SerializeField] private TickSystem _tickSystem;
        [SerializeField] private InputSystem _inputSystem;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            StartGame();
        }

        private void StartGame()
        {
            _tickSystem.Construct();
            _inputSystem.Construct(IsServer);
            
            _tickSystem.StartTicks();
        }
    }
}