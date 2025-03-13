namespace Xeora.Web.Basics
{
    public class PermissionResult
    {
        public enum Results
        {
            Allowed,
            Forbidden
        }

        public PermissionResult(Results result) =>
            this.Result = result;

        public PermissionResult(bool allowed) =>
            this.Result = allowed ? Results.Allowed : Results.Forbidden;

        public Results Result { get; }
    }
}
