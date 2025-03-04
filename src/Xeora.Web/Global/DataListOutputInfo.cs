using System.IO;
using Xeora.Web.Basics.Service;

namespace Xeora.Web.Global
{
    public class DataListOutputInfo : ISerializablePoolValue
    {
        public DataListOutputInfo() {}
        
        public DataListOutputInfo(string uniqueId, long count, long total, bool failed)
        {
            this.UniqueId = uniqueId;
            this.Count = count;
            this.Total = total;
            this.Failed = failed;
        }

        public string UniqueId { get; private set; }
        public long Count { get; private set; }
        public long Total { get; private set; }
        public bool Failed { get; private set; }
        
        public byte[] Serialize()
        {
            using var stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            
            writer.Write(this.UniqueId);
            writer.Write(this.Count);
            writer.Write(this.Total);
            writer.Write(this.Failed);
            writer.Flush();
            
            return stream.ToArray();
        }

        public void Deserialize(byte[] serializedValue)
        {
            using var stream = new MemoryStream(serializedValue);
            using BinaryReader reader = new BinaryReader(stream);

            this.UniqueId = reader.ReadString();
            this.Count = reader.ReadInt64();
            this.Total = reader.ReadInt64();
            this.Failed = reader.ReadBoolean();
        }
    }
}