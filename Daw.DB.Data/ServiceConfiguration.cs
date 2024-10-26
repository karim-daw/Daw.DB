using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

public static class ServiceConfiguration {
    public static IServiceProvider ConfigureServices(IConfiguration configuration) {
        var services = new ServiceCollection();

        bool usePostgres = configuration.GetValue<bool>("DatabaseSettings:UsePostgres");

        services.AddTransient<ISQLiteConnectionFactory, SQLiteConnectionFactory>();
        services.AddTransient<IDatabaseConnectionFactory>(provider => provider.GetRequiredService<ISQLiteConnectionFactory>());


        // Register other services as needed
        services.AddSingleton<IDatabaseContext, DatabaseContext>();
        services.AddSingleton<IQueryBuilderService, QueryBuilderService>();
        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<ITableChangePublisher, TableChangePublisher>();

        services.AddScoped<ISqlService, SqlService>();
        services.AddScoped<IDictionaryHandler, DictionaryHandler>();
        services.AddScoped<IJsonHandler, JsonHandler>();
        services.AddScoped(typeof(IEntityHandler<>), typeof(EntityHandler<>));

        // Register client APIs
        services.AddScoped<IGhClientApi, GhClientApi>();

        services.AddScoped<IEventfulGhClientApi>(provider =>
        {
            var ghClientApi = provider.GetRequiredService<IGhClientApi>();
            var tableChangePublisher = provider.GetRequiredService<ITableChangePublisher>();
            return new EventfulGhClientApi(ghClientApi, tableChangePublisher);
        });

        return services.BuildServiceProvider();
    }

}

