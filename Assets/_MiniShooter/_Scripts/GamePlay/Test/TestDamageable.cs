using Interfaces;
using NGO.Types;
using UnityEngine;

namespace Test
{
    public class TestDamageable : MonoBehaviour, IDamageable
    {
        public void TakeDamage(DamageInfo damageInfo)
        {
            Debug.Log($"Hit, damage: {damageInfo.Damage}");
        }
    }
}