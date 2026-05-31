using Binders;
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
        [SerializeField] private NetworkObject _playerPrefab;
        
        private bool _initialized;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                StartGame(0);
                NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
            }
            else
            {
                RequestCurrentTickRpc();
            }
        }

        private void StartGame(int startTick)
        {
            if (_initialized)
                return;

            _initialized = true;

            _tickSystem.Construct(startTick);
            _inputSystem.Construct(IsServer);

            Debug.Log($"Start tick: {startTick}");

            _tickSystem.StartTicks();
        }

        private void SpawnPlayer(ulong id)
        {
            var player = Instantiate(_playerPrefab, new Vector3(0, 5, 0), Quaternion.identity);
            player.SpawnAsPlayerObject(id);
        }

        [Rpc(SendTo.Server)]
        private void RequestCurrentTickRpc(RpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;

            SendCurrentTickRpc(_tickSystem.Tick, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SendCurrentTickRpc(int tick, RpcParams rpcParams = default)
        {
            StartGame(tick);
        }
    }
}