using Daw.DB.Data.APIs;
using Daw.DB.Data.Interfaces;
using Daw.DB.Data.Services;
using Daw.DB.Data.Services.Daw.DB.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Daw.DB.Data
{
    public static class ServiceConfiguration
    {
        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services with a placeholder connection factory
            services.AddSingleton<IDatabaseConnectionFactory, SQLiteConnectionFactory>();
            services.AddSingleton<IDatabaseCreationFactory, SQLiteConnectionFactory>();
            services.AddSingleton<ISqlService, SqlService>();
            services.AddSingleton<IDictionaryHandler, DictionaryHandler>();
            services.AddSingleton<IJsonHandler, JsonHandler>();

            // using a generic handler for entities to avoid having to register each entity type
            // this is usefull if the user wants to add new entities without having to modify the API
            services.AddSingleton(typeof(IEntityHandler<>), typeof(EntityHandler<>));

            // Register the client APIs
            services.AddSingleton<IClientApi, ClientApi>();
            services.AddSingleton<IGhClientApi, GhClientApi>();

            // Build and return the ServiceProvider
            return services.BuildServiceProvider();
        }
    }
}
