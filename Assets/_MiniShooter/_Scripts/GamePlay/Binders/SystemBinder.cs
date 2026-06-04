using Core.Input;
using Core.TicksSystem;
using UnityEngine;

namespace Binders
{
    public abstract class SystemBinder : MonoBehaviour
    {
        public abstract void Bind(bool isServer, bool isOwner, TickSystem tickSystem, IInputSystem inputSystem);
    }
}