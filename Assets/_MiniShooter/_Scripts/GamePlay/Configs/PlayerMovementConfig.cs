using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "Configs/PlayerMovementConfig")]
    public sealed class PlayerMovementConfig : ScriptableObject
    {
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _runSpeed;
        [SerializeField] private float _acceleration;
        [SerializeField] private float _jumpHeight;
        [SerializeField] private float _gravityScale;

        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float Acceleration => _acceleration;
        public float JumpHeight => _jumpHeight;
        public float GravityScale => _gravityScale;
    }
}