using Unity.Netcode;
using UnityEngine;

namespace Core.NGO.Types
{
    public struct PlayerMovementState : INetworkSerializable
    {
        public int Tick;
        public Vector3 Position;
        public Vector3 OrientationRotation;
        public Vector3 Velocity;
        public bool IsGrounded;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref OrientationRotation);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref IsGrounded);
        }
    }
}