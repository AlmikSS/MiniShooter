using Unity.Netcode;
using UnityEngine;

namespace NGO.Types
{
    public struct PlayerCombatRequest : INetworkSerializable
    {
        public int Tick;
        public string WeaponId;
        public Vector3 Origin;
        public Vector3 Direction;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref WeaponId);
            serializer.SerializeValue(ref Origin);
            serializer.SerializeValue(ref Direction);
        }
    }
}