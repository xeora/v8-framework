namespace Xeora.Web.Basics.Service
{
    public interface ISerializablePoolValue
    {
        byte[] Serialize(); 
        void Deserialize(byte[] serializedValue);
    }
}