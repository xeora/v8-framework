namespace Xeora.Web.Basics.Serialization
{
    internal class Pointers
    {
        internal const byte SERIALIZER_STRING_POINTER = 1;
        internal const byte SERIALIZER_STRUCT_POINTER = 1 << 2;
        internal const byte SERIALIZER_SERIALIZABLE_POOL_VALUE_POINTER = 1 << 3;
        
        internal const byte SERIALIZER_SINGLE_VALUE_POINTER = 0;
        internal const byte SERIALIZER_ARRAY_POINTER = 1 << 4;
    }
}