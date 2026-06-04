using TriInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "Configs/WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [SerializeField] private string _id;
        
        [Title("Damage")]
        [SerializeField] private float _damage;
        [SerializeField] private float _headshotMultiplier;
        
        [Title("Options")]
        [SerializeField] private float _maxDistance;
        [SerializeField] private float _fireRate;
        [SerializeField] private float _spread;
        [SerializeField] private float _verticalRecoil;
        
        [Title("Ammo")]
        [SerializeField] private int _maxAmmo;
        [SerializeField] private int _magazineCapacity;
        [SerializeField] private float _reloadTime;
        
        [Title("Penetration")]
        [SerializeField] private bool _canPenetrate;
        [SerializeField, ShowIf(nameof(_canPenetrate))] private int _maxPenetrationCount;
        [SerializeField, ShowIf(nameof(_canPenetrate))] private float _maxPenetrationPower;

        public string Id => _id;
        public float Damage => _damage;
        public float HeadshotMultiplier => _headshotMultiplier;
        public float MaxDistance => _maxDistance;
        public float FireRate => _fireRate;
        public float Spread => _spread;
        public float VerticalRecoil => _verticalRecoil;
        public int MaxAmmo => _maxAmmo;
        public int MagazineCapacity => _magazineCapacity;
        public float ReloadTime => _reloadTime;
        public bool CanPenetrate => _canPenetrate;
        public int MaxPenetrationCount => _maxPenetrationCount;
        public float MaxPenetrationPower => _maxPenetrationPower;
    }
}