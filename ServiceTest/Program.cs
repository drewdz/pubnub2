using Autofac;
using PubnubMessaging;
using PubnubMessaging.Model;
using PubnubMessaging.Services;
using Serilog;
using ServiceTest.Services;
using System;
using System.Configuration;

namespace ServiceTest
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
                SubscribeOnPresence = Convert.ToBoolean(ConfigurationManager.AppSettings["SubscribeOnPresence"])
            };

            var builder = new ContainerBuilder();
            builder.RegisterType<JsonGenericSerializer>().As<ISerializer>().SingleInstance();
            builder.RegisterInstance(options).As<IMessagingOptions>().SingleInstance();
            builder.RegisterType<MessagingService>().As<IMessagingService>().SingleInstance();
            builder.RegisterType<MessageFactory>().As<IMessageFactory>().SingleInstance();
            builder.RegisterType<CommsService>().As<ICommsService>().SingleInstance();
            AddLogging(builder);
            _Container = builder.Build();

            var serializer = _Container.Resolve<ISerializer>();
            var hello = new ConnectMessage();
            var json = serializer.Serialize(hello);

            Console.WriteLine($"Here is a HelloMessage...\r\n\r\n{json}\r\n\r\nStarting the server.");

            var service = _Container.Resolve<ICommsService>();
            service.Start(clientId, new string[] { clientId });

            Console.WriteLine("Server running. Press enter to stop");
            var s = System.Console.ReadLine();

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
