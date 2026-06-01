using Configs;
using Core.NGO.Types;
using NGO.Types;
using UnityEngine;

namespace Simulation.Player
{
    public sealed class PlayerMovementSimulation
    {
        private readonly PlayerMovementConfig _config;
        private readonly LayerMask _groundLayerMask;
        private readonly LayerMask _obstacleLayerMask;
        private readonly float _groundCheckRadius;
        private readonly float _groundCheckMaxDistance;
        private readonly float _height;
        private readonly float _radius;
        
        public Vector3 Velocity { get; private set; }
        
        public PlayerMovementSimulation(PlayerMovementConfig config, LayerMask groundLayerMask, LayerMask obstacleLayerMask, float groundCheckRadius, float groundCheckMaxDistance, float height, float radius)
        {
            _config = config;
            _groundLayerMask = groundLayerMask;
            _obstacleLayerMask = obstacleLayerMask;
            _groundCheckRadius = groundCheckRadius;
            _groundCheckMaxDistance = groundCheckMaxDistance;
            _height = height;
            _radius = radius;
        }
        
        public void SimulateMovement(PlayerMovementInput input, ref PlayerMovementState state)
        {
            var inputDirection = new Vector3(input.MoveInput.x, 0f, input.MoveInput.y).normalized;
            var speed = input.IsSprint ? _config.RunSpeed : _config.WalkSpeed;
            var deltaTime = input.DeltaTime;
            var position = state.Position;
            var orientation = state.OrientationRotation;
            var grounded = IsGrounded(position);
            
            var horizontalVelocity = new Vector3(state.Velocity.x, 0f, state.Velocity.z);
            var verticalVelocity = state.Velocity.y;
            var targetVelocity = CalculateMoveDirection(inputDirection, orientation, position) * speed;
            
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, deltaTime * _config.Acceleration);

            if (grounded)
                verticalVelocity = 0f;
            else
                verticalVelocity += _config.GravityScale * deltaTime;
            
            if (input.IsJump && grounded)
                verticalVelocity = _config.JumpHeight;
            
            Velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            var displacement = Velocity * deltaTime;
            
            position = MoveCapsule(position, displacement);
            
            state.Velocity = Velocity;
            state.Position = position;
            state.IsGrounded = IsGrounded(position);
            state.OrientationRotation = orientation;
        }

        private Vector3 MoveCapsule(Vector3 position, Vector3 displacement)
        {
            var remaining = displacement;

            for (var i = 0; i < _config.MaxMoveIterations; i++)
            {
                var distance = remaining.magnitude;
                
                if (distance < _config.MinMoveDistance)
                    break;
                
                var direction = remaining / distance;
                GetCapsulePoints(position, out var top, out var bottom);
                var hitSomething = Physics.CapsuleCast(top, bottom, _radius,
                    direction, out var hit, distance + _config.SkinWidth,
                    _obstacleLayerMask, QueryTriggerInteraction.Ignore);

                if (!hitSomething)
                {
                    position += remaining;
                    break;
                }
                
                var safeDistance = Mathf.Max(0f, hit.distance - _config.SkinWidth);
                position += direction * safeDistance;
                
                remaining -= direction * safeDistance;
                remaining = Vector3.ProjectOnPlane(remaining, hit.normal);
            }

            return position;
        }

        private void GetCapsulePoints(Vector3 position, out Vector3 top, out Vector3 bottom)
        {
            var halfHeight = _height * 0.5f;
            var sphereOffset = halfHeight - _radius;
            
            top = position + Vector3.up * sphereOffset;
            bottom = position - Vector3.up * sphereOffset;
        }
        
        private Vector3 CalculateMoveDirection(Vector3 inputDirection, Vector3 orientation, Vector3 position)
        {
            var rotation = Quaternion.Euler(orientation);
            var direction = rotation * inputDirection;
            var distance = _height * 0.5f + 0.1f;
            
            if (!Physics.Raycast(position, Vector3.down, out var hit, distance, _groundLayerMask))
                return direction;
            
            return Vector3.ProjectOnPlane(direction, hit.normal).normalized;
        }

        private bool IsGrounded(Vector3 position)
        {
            GetCapsulePoints(position, out _, out var bottom);
            
            if (!Physics.SphereCast(bottom, _groundCheckRadius, Vector3.down, out var hit, _groundCheckMaxDistance, _groundLayerMask))
                return false;
            
            var angle = Vector3.Angle(hit.normal, Vector3.up);
            return angle <= _config.SlopeAngle;
        }
    }
}