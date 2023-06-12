using Autofac;
using InstaClient.Services;
using PubnubMessaging;
using PubnubMessaging.Model;
using PubnubMessaging.Services;
using Serilog;
using System;
using System.Configuration;
using System.Runtime.Remoting.Channels;

namespace InstaClient
{
    internal class Program
    {
        #region Fields

        private static IContainer _Container;

        #endregion Fields

        #region Operations

        static void Main()
        {
            //  get config
            var clientId = ConfigurationManager.AppSettings["ClientId"].ToString();

            var options = new MessagingOptions
            {
                PublishKey = ConfigurationManager.AppSettings["PublishKey"].ToString(),
                SubscribeKey = ConfigurationManager.AppSettings["SubscribeKey"].ToString(),
                SubscribeOnPresence = Convert.ToBoolean(ConfigurationManager.AppSettings["SubscribeOnPresence"]),
                DebugLogs = Convert.ToBoolean(ConfigurationManager.AppSettings["DebugLogs"])
            };

            var storageOptions = new StorageOptions
            {
                RootPath = Convert.ToString(ConfigurationManager.AppSettings["RootPath"])
            };

            var builder = new ContainerBuilder();
            builder.RegisterType<JsonGenericSerializer>().As<ISerializer>().SingleInstance();
            builder.RegisterInstance(storageOptions).As<IStorageOptions>().SingleInstance();
            builder.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
            builder.RegisterInstance(options).As<IMessagingOptions>().SingleInstance();
            builder.RegisterType<MessagingService>().As<IMessagingService>().SingleInstance();
            builder.RegisterType<MessageFactory>().As<IMessageFactory>().SingleInstance();
            builder.RegisterType<CommsService>().As<ICommsService>().SingleInstance();
            AddLogging(builder);
            _Container = builder.Build();

            //  get the service
            var service = _Container.Resolve<ICommsService>();
            service.Start(clientId);

            service.Stop();
            Console.WriteLine("Server stopped");
        }

        static void AddLogging(ContainerBuilder builder)
        {
            /*  Create an event source before running this
             *  PowerShell in Administrator mode
             *  -> New-EventLog -LogName Application -Source MyApp
             */

            // instantiate and configure logging. Using serilog here, to log to console and a text-file.
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
        }

        #endregion Operations
    }
}
