using Core.DIServiceLocator;
using Core.Input;
using Simulation.Player;
using TriInspector;
using Unity.Netcode;
using UnityEngine;

namespace Visual.Player
{
    public class PlayerCameraController : NetworkBehaviour
    {
        [Title("Dependencies")]
        [SerializeField] private Transform _orientationTransform;
        [SerializeField] private Transform _lookRoot;
        [SerializeField] private Transform _effectsRoot;
        
        [Title("Base options")]
        [SerializeField, Slider(0f, 100f)] private float _sensitivity;
        [SerializeField, Slider(0, 90f)] private float _xRotationClamp;
        [SerializeField, Slider(0, 1f)] private float _movementVelocityStopThreshold;
        
        [Title("Camera bob options")]
        [SerializeField, Slider(0, 0.1f)] private float _bobAmplitudeX;
        [SerializeField, Slider(0, 0.1f)] private float _bobAmplitudeY;
        [SerializeField, Slider(0, 5)] private float _bobFrequency;
        
        [Title("Movement tilt options")]
        [SerializeField, Slider(0, 15)] private float _movementTiltAmount;
        [SerializeField, Slider(0, 15)] private float _movementTiltSmoothness;
        [SerializeField, Slider(0, 30)] private float _movementTiltClamp;
        
        private PlayerMovementSimulation _playerMovement;
        private IInputSystem _inputSystem;
        private Vector3 _lookRotation;
        private Vector3 _movementTiltRotation;
        private Vector3 _bobPosition;
        private float _bobCycle;
        private float _currentMovementTilt;
        private bool _initialized;
        
        public void Construct(PlayerMovementSimulation simulation)
        {
            _playerMovement = simulation;
            _lookRoot.gameObject.SetActive(true);
            
            _inputSystem = ServiceLocator.Get<IInputSystem>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _initialized = true;
        }

        private void OnDestroy()
        {
            _inputSystem = null;
            _initialized = false;
        }

        private void LateUpdate()
        {
            if (!_initialized || _inputSystem == null || _inputSystem.Snapshot.Context != InputContext.Gameplay)
                return;
            
            var deltaTime = Time.deltaTime;
            var lookInput = _inputSystem.Snapshot.LookInput;
            
            CalculateBaseMouseLook(lookInput);
            CalculateCameraBob(deltaTime);
            CalculateMovementTilt(deltaTime);
            
            var effectsRotation = _movementTiltRotation;
            var effectsPosition = _bobPosition;
            
            RotateOrientationRpc(_lookRotation.y);
            _lookRoot.localRotation = Quaternion.Euler(_lookRotation);
            _effectsRoot.localRotation = Quaternion.Euler(effectsRotation);
            _effectsRoot.localPosition = effectsPosition;
        }

        [Rpc(SendTo.Everyone)]
        private void RotateOrientationRpc(float yRotation)
        {
            _orientationTransform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        private void CalculateBaseMouseLook(Vector2 lookInput)
        {
            var x = lookInput.x * _sensitivity * 0.01f;
            var y = lookInput.y * _sensitivity * 0.01f;
            
            _lookRotation.x = Mathf.Clamp(_lookRotation.x - y, -_xRotationClamp, _xRotationClamp);
            _lookRotation.y += x;
        }

        private void CalculateCameraBob(float deltaTime)
        {
            var velocity = _playerMovement.Velocity;
            var speed = velocity.magnitude;

            if (speed < _movementVelocityStopThreshold)
                return;
            
            _bobCycle += deltaTime * speed * _bobFrequency;
            
            var bobX = Mathf.Cos(_bobCycle * 0.5f) * _bobAmplitudeX;
            var bobY = Mathf.Sin(_bobCycle) * _bobAmplitudeY;

            _bobPosition = new Vector3(bobX, bobY, 0);
        }

        private void CalculateMovementTilt(float deltaTime)
        {
            var localVelocity = _orientationTransform.InverseTransformDirection(_playerMovement.Velocity);
            var targetTilt = -localVelocity.x * _movementTiltAmount;
            targetTilt = Mathf.Clamp(targetTilt, -_movementTiltClamp, _movementTiltClamp);
            _currentMovementTilt = Mathf.Lerp(_currentMovementTilt, targetTilt, deltaTime * _movementTiltSmoothness);
            _movementTiltRotation = new Vector3(0f, 0f, _currentMovementTilt);
        }
    }
}