using System.Collections.Generic;
using Core.DIServiceLocator;
using Core.Input;
using Core.TicksSystem;
using Unity.Netcode;
using UnityEngine;

namespace Binders
{
    public class Binder : NetworkBehaviour
    {
        [SerializeField] private List<SystemBinder> _systems = new();
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Construct();
        }

        private void Construct()
        {
            var tickSystem = ServiceLocator.Get<TickSystem>();
            var inputSystem = ServiceLocator.Get<IInputSystem>();
            
            foreach (var system in _systems)
            {
                system.Bind(IsServer, IsOwner, tickSystem, inputSystem);
            }
        }
    }
}