using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

public static class ServiceConfiguration
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register singleton services that do not depend on other services and are thread-safe and stateless
        services.AddSingleton<IDatabaseConnectionFactory, SQLiteConnectionFactory>(); // Singleton
        services.AddSingleton<IQueryBuilderService, QueryBuilderService>(); // Singleton (could be Transient)
        services.AddSingleton<IValidationService, ValidationService>(); // Singleton (could be Transient)
        services.AddSingleton<ITableChangeNotifier, TableChangeNotifier>(); // Singleton

        // Register scoped services that depend on other services and should be created once per request
        services.AddScoped<ISqlService, SqlService>(); // Scoped
        services.AddScoped<IDictionaryHandler, DictionaryHandler>(); // Scoped
        services.AddScoped<IJsonHandler, JsonHandler>(); // Scoped
        services.AddScoped(typeof(IEntityHandler<>), typeof(EntityHandler<>)); // Scoped


        // Register client APIs that depend on other services and should be created once per request
        services.AddScoped<IGenericClientApi, GenericClientApi>(); // Scoped
        services.AddScoped<IGhClientApi, GhClientApi>(); // Scoped

        services.AddScoped<IEventfulGhClientApi>(provider =>
        {
            var ghClientApi = provider.GetRequiredService<IGhClientApi>();
            var tableChangeNotifier = provider.GetRequiredService<ITableChangeNotifier>();
            return new EventfulGhClientApi(ghClientApi, tableChangeNotifier);
        }); // Scoped

        // Build and return the IServiceProvider
        return services.BuildServiceProvider();
    }
}
