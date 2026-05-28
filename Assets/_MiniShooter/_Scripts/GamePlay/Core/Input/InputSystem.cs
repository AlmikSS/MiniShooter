using Core.DIServiceLocator;
using Core.TicksSystem;
using UnityEngine;

namespace Core.Input
{
    public sealed class InputSystem : MonoBehaviour, IInputSystem, ITickable
    {
        private InputActions _inputActions;
        private bool _isConstruct;
        private int _uiOpenedCount;

        private InputContext _context;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private Input _attackInput;
        private Input _jumpInput;
        private Input _crouchInput;
        private Input _sprintInput;
        private Input _interactInput;
        private Input _reloadInput;

        public TickPhase UpdatePhase => TickPhase.InputPhase;
        public InputSnapshot Snapshot { get; private set; }

        public void Construct(bool isServer)
        {
            if (isServer)
                return;
            
            ServiceLocator.Register<IInputSystem>(this);
            ServiceLocator.Get<TickSystem>().Register(this);
            
            _inputActions.Enable();
            _isConstruct = true;
        }

        private void OnDestroy()
        {
            _isConstruct = false;
            _inputActions.Disable();
            ServiceLocator.Unregister(this);
            ServiceLocator.Get<TickSystem>().Unregister(this);
        }

        public void OpenUI()
        {
            _uiOpenedCount++;
            _context = InputContext.UI;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseUI()
        {
            _uiOpenedCount--;
            
            if (_uiOpenedCount > 0)
                return;

            _context = InputContext.Gameplay;
            _uiOpenedCount = 0;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void Tick(float deltaTime)
        {
            if (!_isConstruct)
                return;
            
            Snapshot = new InputSnapshot(
                _context,
                _moveInput,
                _lookInput,
                _attackInput,
                _jumpInput,
                _crouchInput,
                _sprintInput,
                _interactInput,
                _reloadInput);
            
            _attackInput.Reset();
            _jumpInput.Reset();
            _crouchInput.Reset();
            _sprintInput.Reset();
            _interactInput.Reset();
            _reloadInput.Reset();
        }

        private void Update()
        {
            if (!_isConstruct)
                return;

            _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            _lookInput = _inputActions.Player.Look.ReadValue<Vector2>();

            _attackInput.IsStarted = _inputActions.Player.Attack.WasPressedThisFrame();
            _attackInput.IsPressed = _inputActions.Player.Attack.IsPressed();
            _attackInput.IsReleased = _inputActions.Player.Attack.WasReleasedThisFrame();
            
            _jumpInput.IsStarted = _inputActions.Player.Jump.WasPressedThisFrame();
            _jumpInput.IsPressed = _inputActions.Player.Jump.IsPressed();
            _jumpInput.IsReleased = _inputActions.Player.Jump.WasReleasedThisFrame();
            
            _crouchInput.IsStarted = _inputActions.Player.Crouch.WasPressedThisFrame();
            _crouchInput.IsPressed = _inputActions.Player.Crouch.IsPressed();
            _crouchInput.IsReleased = _inputActions.Player.Crouch.WasReleasedThisFrame();
            
            _sprintInput.IsStarted = _inputActions.Player.Sprint.WasPressedThisFrame();
            _sprintInput.IsPressed = _inputActions.Player.Sprint.IsPressed();
            _sprintInput.IsReleased = _inputActions.Player.Sprint.WasReleasedThisFrame();
            
            _interactInput.IsStarted = _inputActions.Player.Interact.WasPressedThisFrame();
            _interactInput.IsPressed = _inputActions.Player.Interact.IsPressed();
            _interactInput.IsReleased = _inputActions.Player.Interact.WasReleasedThisFrame();
            
            _reloadInput.IsStarted = _inputActions.Player.Reload.WasPressedThisFrame();
            _reloadInput.IsPressed = _inputActions.Player.Reload.IsPressed();
            _reloadInput.IsReleased = _inputActions.Player.Reload.WasReleasedThisFrame();
        }
    }
}