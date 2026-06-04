using UnityEngine;

namespace Binders
{
    public sealed class PlayerBinder : Binder
    {
        [SerializeField] private PlayerMovementBinder _playerMovementBinder;
        
        protected override void Construct()
        {
            _playerMovementBinder.Bind(IsServer, IsOwner);
        }
    }
}