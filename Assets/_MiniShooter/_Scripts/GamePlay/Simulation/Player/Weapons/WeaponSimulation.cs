using Configs;
using Core.TicksSystem;
using NGO.Types;
using UnityEngine;

namespace Simulation.Player.Weapons
{
    public abstract class WeaponSimulation
    {
        public readonly string Id;
        protected readonly WeaponState _state;
        protected readonly LayerMask _attackLayerMask;
        protected readonly WeaponConfig _config;
        protected readonly TickSystem _tickSystem; 
        protected readonly int _ticksBetweenShoots;

        protected WeaponSimulation(LayerMask attackLayerMask, WeaponConfig config, TickSystem tickSystem)
        {
            _attackLayerMask = attackLayerMask;
            _config = config;
            _tickSystem = tickSystem;
            Id = _config.Id;
            _state = new WeaponState
            {
                Ammo = _config.MaxAmmo
            };
            
            _ticksBetweenShoots = CalculateTicksBetweenShoots(config.FireRate, _tickSystem.TargetTickRate);
        }

        private static int CalculateTicksBetweenShoots(float fireRate, int tickRate)
        {
            var shotsPerSecond = fireRate / 60f;
            return Mathf.Max(1, Mathf.RoundToInt(tickRate / shotsPerSecond));
        }
        
        public abstract void Simulate(Vector3 origin, Vector3 direction);
    }
}