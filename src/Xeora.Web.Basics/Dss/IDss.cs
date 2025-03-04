using System;

namespace Xeora.Web.Basics.Dss
{
    public interface IDss
    {
        string UniqueId { get; }
        bool Reusing { get; }
        DateTime Expires { get; }
        string[] Keys { get; }
        byte[] Get(string key);
        void Set(string key, byte[] value, string lockCode = null);
        string Lock(string key);
        void Release(string key, string lockCode);
    }
}
