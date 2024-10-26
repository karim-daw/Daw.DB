using Daw.DB.Data.APIs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;

namespace Daw.DB.Data.Services {
    public static class ApiFactory {
        private static IServiceProvider _serviceProvider;

        private static IDatabaseContext _databaseContext;
        private static IGhClientApi _ghClientApi;
        private static IEventfulGhClientApi _eventDrivenGhClientApi;

        static ApiFactory() {
            // Load configuration from appsettings.json located in the same directory as the executing assembly
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) // Use the directory of the executing assembly
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .Build();

            // Configure services based on the loaded configuration
            _serviceProvider = ServiceConfiguration.ConfigureServices(configuration);
        }

        // Method to get IDatabaseContext without exposing the ServiceProvider
        public static IDatabaseContext GetDatabaseContext() {
            if (_databaseContext == null) {
                // Use GetRequiredService internally
                _databaseContext = _serviceProvider.GetRequiredService<IDatabaseContext>();
            }
            return _databaseContext;
        }

        public static IGhClientApi GetGhClientApi() {
            if (_ghClientApi == null) {
                _ghClientApi = _serviceProvider.GetRequiredService<IGhClientApi>();
            }
            return _ghClientApi;
        }


        public static IEventfulGhClientApi GetEventDrivenGhClientApi() {
            if (_eventDrivenGhClientApi == null) {
                _eventDrivenGhClientApi = _serviceProvider.GetRequiredService<IEventfulGhClientApi>();
            }
            return _eventDrivenGhClientApi;
        }

        // Optionally, add a method to set the connection string
        public static void SetConnectionString(string connectionString) {
            GetDatabaseContext().ConnectionString = connectionString;
        }

        // Remove the ServiceProvider property to prevent GH layer from accessing it directly
        // public static IServiceProvider ServiceProvider => _serviceProvider;
    }
}
