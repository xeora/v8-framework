using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xeora.Web.Basics.Service;

namespace Xeora.Web.Basics.Serialization
{
    public class Serializer
    {
        public const int MAX_BYTE_SIZE = 1024 * 1024 * 16; // 16 MB

        public static byte[] Serialize(object value)
        {
            if (value == null)
                return SerializeString(string.Empty);
            
            switch (value)
            {
                case string stringValue:
                    return SerializeString(stringValue);
                case string[] stringValues:
                    return SerializeStringArray(stringValues);
                case ISerializablePoolValue serializablePoolValue:
                    return SerializeSerializablePoolValue(serializablePoolValue);
                default:
                    Type valueType = value.GetType();
                    
                    if (valueType.IsValueType && !valueType.IsEnum)
                        return SerializeStruct(value);
                    
                    throw new ArgumentException("Value is not a serializable type.", nameof(value)); 
            } 
        }
        
        private static byte[] SerializeStringArray(string[] values)
        {
            const byte serializerPointer = 
                Pointers.SERIALIZER_STRING_POINTER | Pointers.SERIALIZER_ARRAY_POINTER;
            
            if (values == null || values.Length == 0) 
                return new[] { serializerPointer }
                    .Concat(BitConverter.GetBytes(0))
                    .ToArray();

            byte[] serializedValue = 
                new[] { serializerPointer }
                    .Concat(BitConverter.GetBytes(values.Length))
                    .ToArray();
            
            int totalStringSize = 0;
            foreach (string value in values)
            {
                totalStringSize += value.Length;
                
                if (totalStringSize > MAX_BYTE_SIZE) 
                    throw new ArgumentOutOfRangeException(nameof(values), $"The total size of collected Values must not be bigger than {MAX_BYTE_SIZE} characters!");
                
                byte[] stringBytes = 
                    System.Text.Encoding.UTF8.GetBytes(value);
                
                serializedValue = serializedValue
                    .Concat(BitConverter.GetBytes(stringBytes.Length))
                    .Concat(stringBytes)
                    .ToArray();
            }

            return serializedValue;
        }
        
        private static byte[] SerializeString(string value)
        {
            const byte serializerPointer = 
                Pointers.SERIALIZER_STRING_POINTER | Pointers.SERIALIZER_SINGLE_VALUE_POINTER;
            
            if (string.IsNullOrEmpty(value)) 
                return new[] { serializerPointer }
                    .Concat(BitConverter.GetBytes(0))
                    .ToArray();
            
            if (value.Length > MAX_BYTE_SIZE) 
                throw new ArgumentOutOfRangeException(nameof(value), $"Value must not be bigger than {MAX_BYTE_SIZE} characters!");

            byte[] stringBytes = 
                System.Text.Encoding.UTF8.GetBytes(value); 
            
            return new[] { serializerPointer }
                .Concat(BitConverter.GetBytes(stringBytes.Length))
                .Concat(stringBytes)
                .ToArray();
        }
        
        private static byte[] SerializeSerializablePoolValue(ISerializablePoolValue value)
        {
            const byte serializerPointer = 
                Pointers.SERIALIZER_SERIALIZABLE_POOL_VALUE_POINTER | Pointers.SERIALIZER_SINGLE_VALUE_POINTER;

            byte[] assemblyNameBytes = 
                System.Text.Encoding.UTF8.GetBytes(value.GetType().Assembly.GetName().Name!);
            byte[] typeFullNameBytes = 
                System.Text.Encoding.UTF8.GetBytes(value.GetType().FullName!);
            
            byte[] serializedSerializablePoolValue = 
                value.Serialize();
            
            return new[] { serializerPointer }
                .Concat(BitConverter.GetBytes(assemblyNameBytes.Length))
                .Concat(assemblyNameBytes)
                .Concat(BitConverter.GetBytes(typeFullNameBytes.Length))
                .Concat(typeFullNameBytes)
                .Concat(BitConverter.GetBytes(serializedSerializablePoolValue.Length))
                .Concat(serializedSerializablePoolValue)
                .ToArray();
        }
        
        private static byte[] SerializeStruct(object value)
        {
            byte[] serializedValue;
            byte serializerPointer = Pointers.SERIALIZER_STRUCT_POINTER;
            
            if (!value.GetType().IsArray)
            {
                if (!value.GetType().IsValueType)
                    throw new ArgumentException("Struct is not a serializable ValueType!", nameof(value));

                byte[] assemblyNameBytes = 
                    System.Text.Encoding.UTF8.GetBytes(value.GetType().Assembly.GetName().Name!);
                byte[] typeNameBytes = 
                    System.Text.Encoding.UTF8.GetBytes(value.GetType().FullName!);
                
                byte[] serializedStruct = 
                    SerializeStructValue(value);
                
                serializerPointer |= Pointers.SERIALIZER_SINGLE_VALUE_POINTER;
                serializedValue = new[] { serializerPointer };
                serializedValue = 
                    serializedValue
                        .Concat(BitConverter.GetBytes(assemblyNameBytes.Length))
                        .Concat(assemblyNameBytes)
                        .Concat(BitConverter.GetBytes(typeNameBytes.Length))
                        .Concat(typeNameBytes)
                        .Concat(BitConverter.GetBytes(serializedStruct.Length))
                        .Concat(serializedStruct)
                        .ToArray();

                return serializedValue;
            }
            
            serializerPointer |= Pointers.SERIALIZER_ARRAY_POINTER;
            serializedValue = new[] { serializerPointer };
            
            Array values = (Array)value;
            Type arrayType = null;
            
            serializedValue = 
                serializedValue
                    .Concat(BitConverter.GetBytes(values.Length)).ToArray();
            
            foreach (object v in values)
            {
                if (!v.GetType().IsValueType)
                    throw new ArgumentException("Struct is not a serializable ValueType!", nameof(value));
                
                if (arrayType == null) 
                    arrayType = v.GetType();
                else if (arrayType != v.GetType())
                    throw new ArgumentException("Array should contain the same type of ValueTypes!", nameof(value));
                
                byte[] assemblyNameBytes = 
                    System.Text.Encoding.UTF8.GetBytes(v.GetType().Assembly.GetName().Name!);
                byte[] typeNameBytes = 
                    System.Text.Encoding.UTF8.GetBytes(v.GetType().FullName!);
                
                byte[] serializedStruct = 
                    SerializeStructValue(v);
                
                serializedValue = 
                    serializedValue
                        .Concat(BitConverter.GetBytes(assemblyNameBytes.Length))
                        .Concat(assemblyNameBytes)
                        .Concat(BitConverter.GetBytes(typeNameBytes.Length))
                        .Concat(typeNameBytes)
                        .Concat(BitConverter.GetBytes(serializedStruct.Length))
                        .Concat(serializedStruct)
                        .ToArray();
            }
            
            return serializedValue;
        }
        
        private static byte[] SerializeStructValue(object value)
        {
            int size = Marshal.SizeOf(value);
            byte[] serializedValue = new byte[size];

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(value, ptr, true);
                Marshal.Copy(ptr, serializedValue, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return serializedValue;
        }
    }
}