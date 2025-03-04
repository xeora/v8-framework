using System;
using System.IO;
using System.Threading;

namespace Xeora.Web.Manager
{
    public class Loader
    {
        private const string EXECUTABLES = "Executables";
        private const string ADDONS = "Addons";
        
        private readonly string _CacheRootLocation;
        private readonly string _DomainRootLocation;
        private readonly Watcher _Watcher;

        private Loader(Basics.Configuration.IXeora configuration, Action<string, string> reloadHandler)
        {
            this._CacheRootLocation =
                System.IO.Path.Combine(
                    configuration.Application.Main.TemporaryRoot,
                    configuration.Application.Main.WorkingPath.WorkingPathId
                );

            if (!Directory.Exists(this._CacheRootLocation))
                Directory.CreateDirectory(this._CacheRootLocation);

            this._DomainRootLocation =
                System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(
                        configuration.Application.Main.PhysicalRoot,
                        configuration.Application.Main.ApplicationRoot.FileSystemImplementation,
                        "Domains"
                    )
                );
            
            this._Watcher = 
                new Watcher(this._DomainRootLocation, () =>
                {
                    Basics.Logging.Current
                        .Information("Detected changes in Xeora Libraries!")
                        .Flush();
                    
                    // Reload
                    this.Load();
                    
                    // Notify
                    reloadHandler?.Invoke(this.Id, this.Path);
                });
        }

        public string Id { get; private set; }
        public string Path =>
            System.IO.Path.Combine(this._CacheRootLocation, this.Id);

        public static Loader Current { get; private set; }
        public static void Initialize(Basics.Configuration.IXeora configuration, Action<string, string> reloadHandler)
        {
            Loader.Current ??= new Loader(configuration, reloadHandler);
            Loader.Current.Load();
        }
        
        private void Load()
        {
            try
            {
                this.Id = Guid.NewGuid().ToString();

                if (!Directory.Exists(this.Path))
                    Directory.CreateDirectory(this.Path);

                DirectoryInfo domains =
                    new DirectoryInfo(this._DomainRootLocation);
                if (!domains.Exists)
                {
                    Basics.Logging.Current
                        .Warning("No application has been found to prepare!");
                    return;
                }

                this.LoadExecutables(domains);
                this._Watcher.Start();

                Basics.Logging.Current
                    .Information("Application is prepared successfully!");
            }
            catch (Exception)
            {
                Basics.Logging.Current
                    .Error("Application preparation has been FAILED!");

                return;
            }
            finally
            {
                Basics.Logging.Current.Flush();
            }

            // Do not block Application load
            ThreadPool.QueueUserWorkItem(_ => this.Cleanup());
        }

        private void LoadExecutables(DirectoryInfo domains)
        {
            foreach (DirectoryInfo domain in domains.GetDirectories())
            {
                string domainExecutablesLocation =
                    System.IO.Path.Combine(domain.FullName, Loader.EXECUTABLES);

                DirectoryInfo domainExecutables =
                    new DirectoryInfo(domainExecutablesLocation);
                if (domainExecutables.Exists)
                    this.CopyToTarget(domainExecutables, this.Path);

                DirectoryInfo domainChildren =
                    new DirectoryInfo(System.IO.Path.Combine(domain.FullName, Loader.ADDONS));
                if (domainChildren.Exists)
                    this.LoadExecutables(domainChildren);
            }
        }

        private void CopyToTarget(DirectoryInfo sourceRoot, string target)
        {
            foreach (FileInfo fI in sourceRoot.GetFiles())
            {
                FileInfo applicationLocation =
                    new FileInfo(
                        System.IO.Path.Combine(target, fI.Name));
                if (applicationLocation.Exists) continue;

                fI.CopyTo(applicationLocation.FullName, true);
            }

            foreach (DirectoryInfo dI in sourceRoot.GetDirectories())
            {
                DirectoryInfo applicationLocation =
                    new DirectoryInfo(
                        System.IO.Path.Combine(target, dI.Name));
                if (applicationLocation.Exists) continue;

                applicationLocation.Create();
                this.CopyToTarget(dI, applicationLocation.FullName);
            }
        }

        private static bool AvailableToDelete(DirectoryInfo application)
        {
            foreach (FileInfo fI in application.GetFiles())
            {
                Stream checkStream = null;
                try
                {
                    checkStream = fI.OpenRead();
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    checkStream?.Dispose();
                }
            }

            return true;
        }

        private void Cleanup()
        {
            DirectoryInfo cacheRoot =
                new DirectoryInfo(this._CacheRootLocation);
            if (!cacheRoot.Exists)
                return;

            foreach (DirectoryInfo application in cacheRoot.GetDirectories())
            {
                if (application.Name.Equals("PoolSessions") ||
                    application.Name.Equals(this.Id))
                    continue;

                if (!Loader.AvailableToDelete(application)) continue;
                
                try
                {
                    application.Delete(true);
                }
                catch (Exception)
                {
                    // Just Handle Exceptions
                }
            }

            Basics.Logging.Current
                .Information("Cache is cleaned up!")
                .Flush();
        }
    }
}
