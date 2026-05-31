using Unity.Netcode;
using UnityEngine;

namespace NGO.Types
{
    public struct PlayerMovementInput : INetworkSerializable
    {
        public int Tick;
        public Vector2 MoveInput;
        public bool IsSprint;
        public bool IsJump;
        public float DeltaTime;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref MoveInput);
            serializer.SerializeValue(ref IsSprint);
            serializer.SerializeValue(ref IsJump);
            serializer.SerializeValue(ref DeltaTime);
        }
    }
}