using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xeora.Web.Basics;

namespace Xeora.Web.Tools.Serialization
{
    public static class Binary
    {
        public static byte[] Serialize(object value)
        {
            if (value == null) return null;
            
            Stream forStream = null;
            try
            {
                forStream = new MemoryStream();
                
                BinaryFormatter binFormatter = 
                    new BinaryFormatter {Binder = new Binder(Helpers.Name)};
                binFormatter.Serialize(forStream, value);

                return ((MemoryStream)forStream).ToArray();
            }
            catch (Exception e)
            {
                Logging.Error(
                    "Bin. Serializer Exception...",
                    new Dictionary<string, object>
                    {
                        { "message", e.Message },
                        { "trace", e.ToString() }
                    }
                );
                
                return null;
            }
            finally
            {
                forStream?.Dispose();
            }
        }

        [Obsolete("Obsolete")]
        public static object DeSerialize(byte[] value)
        {
            if (value == null || value.Length == 0) 
                return null;
            
            Stream forStream = null;
            try
            {
                forStream = new MemoryStream(value);

                BinaryFormatter binFormatter = 
                    new BinaryFormatter {Binder = new Binder(Helpers.Name)};
                return binFormatter.Deserialize(forStream);
            }
            catch (Exception e)
            {
                Logging.Error(
                    "Bin. Deserializer Exception...",
                    new Dictionary<string, object>
                    {
                        { "message", e.Message },
                        { "trace", e.ToString() }
                    }
                );
                
                return default;
            }
            finally
            {
                forStream?.Dispose();
            }
        }
        
        [Obsolete("Obsolete")]
        public static T DeSerialize<T>(byte[] value)
        {
            if (value == null || value.Length == 0) 
                return default;
            
            Stream forStream = null;
            try
            {
                forStream = new MemoryStream(value);

                BinaryFormatter binFormatter = 
                    new BinaryFormatter {Binder = new Binder(Helpers.Name)};
                return (T)Convert.ChangeType(binFormatter.Deserialize(forStream), typeof(T));
            }
            catch (Exception e)
            {
                Logging.Error(
                    "Bin. Deserializer Exception...",
                    new Dictionary<string, object>
                    {
                        { "message", e.Message },
                        { "trace", e.ToString() }
                    }
                );
                
                return default;
            }
            finally
            {
                forStream?.Dispose();
            }
        }
    }
}
