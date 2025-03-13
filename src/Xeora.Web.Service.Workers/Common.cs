namespace Xeora.Web.Service.Workers
{
    internal static class Common
    {
        internal const int JOIN_WAIT_TIMEOUT = 5000;
        
        private static bool _ToggleReporting;
        
        public static bool PrintReport => Common._ToggleReporting;
        public static void ToggleReporting()
        {
            Common._ToggleReporting = !Common._ToggleReporting;
            
            string toggleStatus = 
                Common._ToggleReporting ? "activate" : "deactivated";
            
            Basics.Logging.Current
                .Information($"Worker Reporting output is {toggleStatus}")
                .Flush();
        }
    }
}