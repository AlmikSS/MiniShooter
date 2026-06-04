using Configs;
using Core.Input;
using Core.TicksSystem;
using NGO.Client;
using NGO.Server;
using Simulation.Player.Weapons;
using UnityEngine;

namespace Binders
{
    public sealed class PlayerCombatBinder : SystemBinder
    {
        [SerializeField] private PlayerCombatServer _server;
        [SerializeField] private LocalPlayerCombat _localCombat;
        [SerializeField] private Transform _shotOrigin;
        [SerializeField] private LayerMask _attackLayerMask;
        [SerializeField] private WeaponConfig _config;
        
        public override void Bind(bool isServer, bool isOwner, TickSystem tickSystem, IInputSystem inputSystem)
        {
            _server.SetClient(_localCombat);
            
            if (isServer)
            {
                _server.Construct(tickSystem, new WeaponSimulation[] { new RifleWeapon(_attackLayerMask, _config, tickSystem) } );
                _localCombat.SetServer(_server);
            }
            
            if (isOwner)
            {
                _localCombat.Construct(inputSystem, tickSystem, _shotOrigin);
            }
        }
    }
}