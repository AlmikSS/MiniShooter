using Configs;
using Core.NGO.Types;
using NGO.Types;
using UnityEngine;

namespace Visual.Player
{
    public sealed class PlayerMovementSimulation
    {
        private readonly Transform _orientationTransform;
        private readonly Transform _groundCheckOrigin;
        private readonly float _groundCheckRadius;
        private readonly PlayerMovementConfig _config;
        private readonly LayerMask _groundLayerMask;
        private readonly LayerMask _obstacleLayerMask;
        private readonly float _obstacleCheckRadius;
        
        public Vector3 Velocity { get; private set; }
        
        public PlayerMovementSimulation(Transform orientationTransform, Transform groundCheckOrigin, float groundCheckRadius, PlayerMovementConfig config, LayerMask groundLayerMask, LayerMask obstacleLayerMask, float obstacleCheckRadius)
        {
            _orientationTransform = orientationTransform;
            _groundCheckOrigin = groundCheckOrigin;
            _groundCheckRadius = groundCheckRadius;
            _config = config;
            _groundLayerMask = groundLayerMask;
            _obstacleLayerMask = obstacleLayerMask;
            _obstacleCheckRadius = obstacleCheckRadius;
        }
        
        public void SimulateMovement(PlayerMovementInput input, ref PlayerMovementState state)
        {
            var moveDirection = CalculateDirection(new Vector3(input.MoveInput.x, 0, input.MoveInput.y).normalized);
            var speed = input.IsSprint ? _config.RunSpeed : _config.WalkSpeed;
            var deltaTime = input.DeltaTime;
            var position = state.Position;
            
            var horizontalVelocity = new Vector3(state.Velocity.x, 0f, state.Velocity.z);
            var verticalVelocity = state.Velocity.y;
            var targetVelocity = moveDirection * speed;
            
            horizontalVelocity = CanMove(moveDirection) ? Vector3.Lerp(horizontalVelocity, targetVelocity, _config.Acceleration * deltaTime) : Vector3.zero;

            if (IsGrounded())
                verticalVelocity = 0f;
            else
                verticalVelocity += _config.GravityScale * deltaTime;

            if (input.IsJump && IsGrounded())
                verticalVelocity = _config.JumpHeight;
            
            Velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            position += Velocity * deltaTime;
            
            state.Position = position;
            state.Velocity = Velocity;
            state.IsGrounded = IsGrounded();
        }
        
        private Vector3 CalculateDirection(Vector3 inputDirection)
        {
            var direction = _orientationTransform.TransformDirection(inputDirection);
            Physics.Raycast(_groundCheckOrigin.position, Vector3.down, out var hit, _groundCheckRadius, _groundLayerMask);
            var normal = hit.normal;
            return direction - Vector3.Dot(direction, normal) * normal;
        }

        private bool IsGrounded()
        {
            return Physics.CheckSphere(_groundCheckOrigin.position, _groundCheckRadius, _groundLayerMask, QueryTriggerInteraction.Ignore);
        }

        private bool CanMove(Vector3 moveDirection)
        {
            return !Physics.CheckSphere(_orientationTransform.position + moveDirection * 0.5f, _obstacleCheckRadius, _obstacleLayerMask, QueryTriggerInteraction.Ignore);
        }
    }
}