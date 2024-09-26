using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

public static class ServiceConfiguration
{
    public static IServiceProvider ConfigureServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        bool usePostgres = configuration.GetValue<bool>("DatabaseSettings:UsePostgres");

        if (usePostgres)
        {
            // Get the connection string from the configuration if Postgres is used
            string connectionString = configuration.GetValue<string>("DatabaseSettings:PostgresConnectionString");

            // Register the Supabase client
            var supabaseConfig = configuration.GetSection("Supabase").Get<SupabaseConfiguration>();
            if (supabaseConfig == null || string.IsNullOrEmpty(supabaseConfig.Url) || string.IsNullOrEmpty(supabaseConfig.Key))
            {
                throw new InvalidOperationException("Supabase configuration is not set correctly in appsettings.json.");
            }

            services.AddScoped<Supabase.Client>(provider =>
                new Supabase.Client(supabaseConfig.Url, supabaseConfig.Key));
        }
        else
        {
            services.AddTransient<ISQLiteConnectionFactory, SQLiteConnectionFactory>();
            services.AddTransient<IDatabaseConnectionFactory>(provider => provider.GetRequiredService<ISQLiteConnectionFactory>());
        }

        // Register other services as needed
        services.AddSingleton<IQueryBuilderService, QueryBuilderService>();
        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<ITableChangePublisher, TableChangePublisher>();

        services.AddScoped<ISqlService, SqlService>();
        services.AddScoped<IDictionaryHandler, DictionaryHandler>();
        services.AddScoped<IJsonHandler, JsonHandler>();
        services.AddScoped(typeof(IEntityHandler<>), typeof(EntityHandler<>));

        // Register client APIs
        services.AddScoped<IGenericClientApi, GenericClientApi>();
        services.AddScoped<IGhClientApi, GhClientApi>();

        services.AddScoped<IEventfulGhClientApi>(provider =>
        {
            var ghClientApi = provider.GetRequiredService<IGhClientApi>();
            var tableChangePublisher = provider.GetRequiredService<ITableChangePublisher>();
            return new EventfulGhClientApi(ghClientApi, tableChangePublisher);
        });

        return services.BuildServiceProvider();
    }

    public class SupabaseConfiguration
    {
        public string Url { get; set; }
        public string Key { get; set; }
    }
}

