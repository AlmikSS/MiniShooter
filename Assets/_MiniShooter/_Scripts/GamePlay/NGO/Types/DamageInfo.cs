using Unity.Netcode;

namespace NGO.Types
{
    public struct DamageInfo : INetworkSerializable
    {
        public float Damage;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Damage);
        }
    }
}