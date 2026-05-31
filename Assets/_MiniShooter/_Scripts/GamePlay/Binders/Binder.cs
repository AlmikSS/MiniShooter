using Unity.Netcode;

namespace Binders
{
    public abstract class Binder : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Construct();
        }

        protected abstract void Construct();
    }
}