namespace Xeora.Web.Basics.Context.Response
{
    public interface IHttpResponseHeader : IKeyValueCollection<string, string>
    {
        void AddOrUpdate(string key, string value);

        bool KeepAlive { get; set; }
        IHttpResponseStatus Status { get; }
        IHttpCookie Cookie { get; }
    }
}
