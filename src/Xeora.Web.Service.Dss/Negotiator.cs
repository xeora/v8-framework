using Xeora.Web.Basics;
using Xeora.Web.Basics.Domain;
using Xeora.Web.Basics.Service;

namespace Xeora.Web.Service
{
    internal class Negotiator : INegotiator
    {
        public Negotiator()
        {
            
        }

        public IHandler GetHandler(string handlerId) => null;
        
        public void KeepHandler(string handlerId) {}
        
        public void DropHandler(string handlerId) {}

        public IVariablePool GetVariablePool(string sessionId, string keyId) => null;

        public IDomain CreateNewDomainInstance(string[] domainIdAccessTree, string domainLanguageId) => null;

        /*public void TransferVariablePool(string keyId, string fromSessionId, string toSessionId) =>
            PoolManager.Copy(keyId, fromSessionId, toSessionId);*/
        
        public IStatusTracker StatusTracker => null;

        public ITaskSchedulerEngine TaskScheduler => null;
        
        public Basics.Configuration.IXeora XeoraSettings => 
            Configuration.Manager.Current.Configuration;
        
        public void ClearCache() {}
    }
}
