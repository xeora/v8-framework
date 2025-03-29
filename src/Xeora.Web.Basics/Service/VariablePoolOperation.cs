using System;
using System.Reflection;
using System.Collections.Concurrent;
using System.Threading;

namespace Xeora.Web.Basics.Service
{
    public sealed class VariablePoolOperation
    {
        private static readonly object Lock = new();

        private readonly string _SessionKeyId;
        private readonly IVariablePool _VariablePool;
        
        public VariablePoolOperation(string sessionId, string keyId)
        {
            Monitor.Enter(VariablePoolOperation.Lock);
            try
            {
                this._VariablePool =
                    Helpers.Negotiator.GetVariablePool(sessionId, keyId);
            }
            catch (Exception ex)
            {
                throw new TargetInvocationException("Communication Error! Variable Pool is not accessible...", ex);
            }
            finally
            {
                Monitor.Exit(VariablePoolOperation.Lock);
            }

            this._SessionKeyId = $"{sessionId}_{keyId}";
        }
        
        public void Set(string name, string value) =>
            this.RegisterVariableToPool(name, value);
        
        public void Set(string name, string[] value) =>
            this.RegisterVariableToPool(name, value);
        
        public void Set(string name, ISerializablePoolValue value) =>
            this.RegisterVariableToPool(name, value);

        public void Set<T>(string name, T value) where T : struct =>
            this.RegisterVariableToPool(name, value);
        
        public void Set<T>(string name, T[] values) where T : struct =>
            this.RegisterVariableToPool(name, values);

        public T Get<T>(string name)
        {
            object objectValue = 
                this.GetVariableFromPool(name);

            if (objectValue is T value)
                return value;

            return default;
        }

        /*public void Transfer(string fromSessionId) =>
            this.TransferRegistrations($"{fromSessionId}_{this._KeyId}");*/
        
        private object GetVariableFromPool(string name)
        {
            object rObject = 
                VariablePoolPreCache.GetCachedVariable(this._SessionKeyId, name);

            if (rObject != null) return rObject;
            
            byte[] serializedValue = 
                this._VariablePool.Get(name);

            if (serializedValue == null || serializedValue.Length < 5) return null;
            
            return Serialization.Deserializer.Deserialize(serializedValue);
        }
        
        private void RegisterVariableToPool(string name, object value)
        {
            if (!string.IsNullOrWhiteSpace(name) && name.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(name), "Key must not be longer than 128 characters!");

            VariablePoolPreCache.CleanCachedVariables(this._SessionKeyId, name);

            byte[] serializedValue =
                Serialization.Serializer.Serialize(value);

            this._VariablePool.Set(name, serializedValue);
        }

        /*private void TransferRegistrations(string fromSessionId)
        {
            try
            {
                TypeCache.Current.Negotiator.TransferVariablePool(this._KeyId, fromSessionId, this._SessionId);
            }
            catch (Exception ex)
            {
                throw new TargetInvocationException("Communication Error! Variable Pool is not accessible...", ex);
            }
        }*/
        
        // This class required to eliminate the mass request to VariablePool.
        // VariablePool registration requires serialization...
        // Use PreCache for only read keys do not use for variable registration!
        // It is suitable for repeating requests...
        private static class VariablePoolPreCache
        {
            private static readonly object Lock = new();
            private static ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _variablePreCache;
            private static ConcurrentDictionary<string, ConcurrentDictionary<string, object>> VariablePreCache
            {
                get
                {
                    Monitor.Enter(VariablePoolPreCache.Lock);
                    try
                    {
                        return VariablePoolPreCache._variablePreCache ?? (VariablePoolPreCache._variablePreCache =
                                   new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>());
                    }
                    finally
                    {
                        Monitor.Exit(VariablePoolPreCache.Lock);
                    }
                }
            }

            public static object GetCachedVariable(string sessionKeyId, string name)
            {
                if (!VariablePoolPreCache.VariablePreCache.TryGetValue(sessionKeyId,
                    out ConcurrentDictionary<string, object> nameValuePairs)) return null;
                
                return nameValuePairs.TryGetValue(name, out object value) ? value : null;
            }

            public static void CacheVariable(string sessionKeyId, string name, object value)
            {
                if (!VariablePoolPreCache.VariablePreCache.TryGetValue(sessionKeyId, out ConcurrentDictionary<string, object> nameValuePairs))
                {
                    nameValuePairs = new ConcurrentDictionary<string, object>();

                    if (!VariablePoolPreCache.VariablePreCache.TryAdd(sessionKeyId, nameValuePairs))
                    {
                        VariablePoolPreCache.CacheVariable(sessionKeyId, name, value);

                        return;
                    }
                }

                if (value == null)
                    nameValuePairs.TryRemove(name, out value);
                else
                    nameValuePairs.AddOrUpdate(name, value, (cName, cValue) => value);
            }

            public static void CleanCachedVariables(string sessionKeyId, string name)
            {
                if (VariablePoolPreCache.VariablePreCache.TryGetValue(sessionKeyId, out ConcurrentDictionary<string, object> nameValuePairs))
                    nameValuePairs.TryRemove(name, out _);
            }
        }
    }
}