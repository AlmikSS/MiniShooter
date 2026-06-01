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
        [SerializeField] private float _skinWidth;
        [SerializeField] private float _minMoveDistance;
        [SerializeField] private int _maxMoveIterations;
        [SerializeField] private float _slopeAngle;
        [SerializeField] private float _maxStepHeight;

        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float Acceleration => _acceleration;
        public float JumpHeight => _jumpHeight;
        public float GravityScale => _gravityScale;
        public float SkinWidth => _skinWidth;
        public float MinMoveDistance => _minMoveDistance;
        public int MaxMoveIterations => _maxMoveIterations;
        public float SlopeAngle => _slopeAngle;
        public float MaxStepHeight => _maxStepHeight;
    }
}