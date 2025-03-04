using System;

namespace Xeora.Web.Basics.Serialization
{
    public class Convertors
    {
        public static string BinaryToBase64Serialize(object input)
        {
            byte[] serializedBytes = Serializer.Serialize(input);

            return Base64.Serialize(serializedBytes);
        }

        public static T Base64ToBinaryDeSerialize<T>(string base64data)
        {
            byte[] serializedBytes = Base64.DeSerialize(base64data);

            object value = 
                Deserializer.Deserialize(serializedBytes);
            
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}