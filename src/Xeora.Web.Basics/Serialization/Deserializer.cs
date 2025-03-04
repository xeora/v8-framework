using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Xeora.Web.Basics.Service;

namespace Xeora.Web.Basics.Serialization
{
    public class Deserializer
    {
        public static object Deserialize(byte[] serializedValue)
        {
            if (serializedValue == null || serializedValue.Length < 5) 
                return null;
            
            return serializedValue[0] switch
            {
                Pointers.SERIALIZER_STRING_POINTER | Pointers.SERIALIZER_SINGLE_VALUE_POINTER => 
                    DeserializeString(serializedValue),
                Pointers.SERIALIZER_STRING_POINTER | Pointers.SERIALIZER_ARRAY_POINTER => 
                    DeserializeStringArray(serializedValue),
                Pointers.SERIALIZER_SERIALIZABLE_POOL_VALUE_POINTER | Pointers.SERIALIZER_SINGLE_VALUE_POINTER =>
                    DeserializeSerializablePoolValue(serializedValue),
                Pointers.SERIALIZER_STRUCT_POINTER | Pointers.SERIALIZER_SINGLE_VALUE_POINTER => 
                    DeserializeStruct(serializedValue),
                Pointers.SERIALIZER_STRUCT_POINTER | Pointers.SERIALIZER_ARRAY_POINTER => 
                    DeserializeStructArray(serializedValue),
                _ => null
            };
        }

        private static string[] DeserializeStringArray(byte[] serializedValue)
        {
            using Stream serializedValueStream = 
                new MemoryStream(serializedValue);
            
            using BinaryReader binaryReader = 
                new BinaryReader(serializedValueStream);
            
            // Skip Pointer
            binaryReader.ReadByte();
            
            int arrayLength = 
                binaryReader.ReadInt32();
            string[] stringArray = new string[arrayLength];

            for (int i = 0; i < stringArray.Length; i++)
            {
                int stringLength = 
                    binaryReader.ReadInt32();
                stringArray[i] = System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(stringLength));
            }

            return stringArray;
        }
        
        private static string DeserializeString(byte[] serializedValue)
        {
            using Stream serializedValueStream = 
                new MemoryStream(serializedValue);
            
            using BinaryReader binaryReader = 
                new BinaryReader(serializedValueStream);
            
            // Skip Pointer
            binaryReader.ReadByte();
            
            int stringLength = 
                binaryReader.ReadInt32();
            return System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(stringLength));
        }
        
        private static ISerializablePoolValue DeserializeSerializablePoolValue(byte[] serializedValue)
        {
            using Stream serializedValueStream = 
                new MemoryStream(serializedValue);
            
            using BinaryReader binaryReader = 
                new BinaryReader(serializedValueStream);

            // Skip Pointer
            binaryReader.ReadByte();
            
            int assemblyNameLength = binaryReader.ReadInt32();
            string assemblyName = 
                System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(assemblyNameLength));
            
            int typeNameLength = binaryReader.ReadInt32();
            string typeName = 
                System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(typeNameLength));

            Assembly assembly = GetAssembly(assemblyName);
            if (assembly == null) return null;
            
            Type type = assembly.GetType(typeName);
            if (type == null) return null;
            
            ISerializablePoolValue serializedPoolValue =
                (ISerializablePoolValue)Activator.CreateInstance(type);

            if (serializedPoolValue == null) return null;
            
            int objectLength = 
                binaryReader.ReadInt32();
            serializedPoolValue.Deserialize(binaryReader.ReadBytes(objectLength));

            return serializedPoolValue;
        }

        private static Array DeserializeStructArray(byte[] serializedValue)
        {
            using Stream serializedValueStream = 
                new MemoryStream(serializedValue);
            
            using BinaryReader binaryReader = 
                new BinaryReader(serializedValueStream);
            
            // Skip Pointer
            binaryReader.ReadByte();
            
            int arrayLength = 
                binaryReader.ReadInt32();
            Array structArray = null;

            for (int i = 0; i < arrayLength; i++)
            {
                int assemblyNameLength = binaryReader.ReadInt32();
                string assemblyName =
                    System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(assemblyNameLength));

                int typeNameLength = binaryReader.ReadInt32();
                string typeName =
                    System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(typeNameLength));

                Assembly assembly = GetAssembly(assemblyName);
                if (assembly == null) return null;
            
                Type type = assembly.GetType(typeName);
                if (type == null) return null;
            
                object serializedStruct =
                    Activator.CreateInstance(type);
                
                if (serializedStruct == null) return null;

                int objectLength =
                    binaryReader.ReadInt32();
                DeserializeStructValueInto(binaryReader.ReadBytes(objectLength), ref serializedStruct);
                
                structArray ??= Array.CreateInstance(serializedStruct.GetType(), arrayLength);
                structArray.SetValue(serializedStruct, i);
            }

            return structArray;
        }
        
        private static object DeserializeStruct(byte[] serializedValue)
        {
            using Stream serializedValueStream = 
                new MemoryStream(serializedValue);
            
            using BinaryReader binaryReader = 
                new BinaryReader(serializedValueStream);
            
            // Skip Pointer
            binaryReader.ReadByte();
            
            int assemblyNameLength = binaryReader.ReadInt32();
            string assemblyName = 
                System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(assemblyNameLength));
            
            int typeNameLength = binaryReader.ReadInt32();
            string typeName = 
                System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(typeNameLength));

            Assembly assembly = GetAssembly(assemblyName);
            if (assembly == null) return null;
            
            Type type = assembly.GetType(typeName);
            if (type == null) return null;
            
            object serializedStruct =
                Activator.CreateInstance(type);
            
            if (serializedStruct == null) return null;
            
            int objectLength = 
                binaryReader.ReadInt32();
            DeserializeStructValueInto(binaryReader.ReadBytes(objectLength), ref serializedStruct);

            return serializedStruct;
        }
        
        private static void DeserializeStructValueInto(byte[] serializedValue, ref object value)
        {
            int size = Marshal.SizeOf(value);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(serializedValue, 0, ptr, size);
                value = Marshal.PtrToStructure(ptr, value.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        
        private static Assembly GetAssembly(string assemblyName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name != assemblyName) continue;
                return assembly;
            }
            
            return null;
        }
    }
}