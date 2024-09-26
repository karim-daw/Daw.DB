using Daw.DB.Data.APIs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;

namespace Daw.DB.Data
{
    public static class ApiFactory
    {
        private static IGenericClientApi _clientApi;
        private static IGhClientApi _ghClientApi;
        private static IEventfulGhClientApi _eventDrivenGhClientApi;

        public static IEventfulGhClientApi GetEventDrivenGhClientApi()
        {
            if (_eventDrivenGhClientApi == null)
            {
                // Load configuration from appsettings.json located in the same directory as the executing assembly
                var configuration = new ConfigurationBuilder()
                                    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) // Use the directory of the executing assembly
                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                    .Build();

                // Configure services based on the loaded configuration
                var serviceProvider = ServiceConfiguration.ConfigureServices(configuration);
                _eventDrivenGhClientApi = serviceProvider.GetRequiredService<IEventfulGhClientApi>();
            }
            return _eventDrivenGhClientApi;
        }

        public static IGenericClientApi GetGenericClientApi()
        {
            if (_clientApi == null)
            {
                // Load configuration from appsettings.json located in the same directory as the executing assembly
                var configuration = new ConfigurationBuilder()
                                    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) // Use the directory of the executing assembly
                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                    .Build();

                // Configure services based on the loaded configuration
                var serviceProvider = ServiceConfiguration.ConfigureServices(configuration);
                _clientApi = serviceProvider.GetRequiredService<IGenericClientApi>();
            }
            return _clientApi;
        }

        /// <summary>
        /// Get Grasshopper client api
        /// Basic access to functionality for CRUD operations
        /// </summary>
        /// <returns></returns>
        public static IGhClientApi GetGhClientApi()
        {
            if (_ghClientApi == null)
            {
                // Load configuration from appsettings.json located in the same directory as the executing assembly
                var configuration = new ConfigurationBuilder()
                                    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) // Use the directory of the executing assembly
                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                    .Build();

                // Configure services based on the loaded configuration
                var serviceProvider = ServiceConfiguration.ConfigureServices(configuration);
                _ghClientApi = serviceProvider.GetRequiredService<IGhClientApi>();
            }
            return _ghClientApi;
        }
    }
}
