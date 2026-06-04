using Configs;
using Core.TicksSystem;
using Interfaces;
using NGO.Types;
using UnityEngine;

namespace Simulation.Player.Weapons
{
    public class RifleWeapon : WeaponSimulation
    {
        public RifleWeapon(LayerMask attackLayerMask, WeaponConfig config, TickSystem tickSystem) : base(attackLayerMask, config, tickSystem)
        {
        }

        public override void Simulate(Vector3 origin, Vector3 direction)
        {
            var currentTick = _tickSystem.Tick;
            if (currentTick - _state.LastFireTick < _ticksBetweenShoots || _state.Ammo <= 0)
                return;

            _state.LastFireTick = currentTick;
            _state.Ammo -= 1;

            if (!Physics.Raycast(origin, direction, out var hit, _config.MaxDistance, _attackLayerMask))
                return;

            if (!hit.transform.TryGetComponent(out IDamageable damageable))
                return;
            
            damageable.TakeDamage(new DamageInfo
            {
                Damage = _config.Damage,
            });
        }
    }
}