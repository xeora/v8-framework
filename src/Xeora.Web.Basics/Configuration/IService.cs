using System.Net;

namespace Xeora.Web.Basics.Configuration
{
    public interface IService
    {
        IPAddress Address { get; }
        short Port { get; }
        ITimeout Timeout { get; }
        bool Ssl { get; }
        string CertificatePassword { get; }
        IParallelism Parallelism { get; }
        bool Logging { get; }
        LoggingFormats LoggingFormat { get; }
        LoggingTypes LoggingLevel { get; }
    }
}
