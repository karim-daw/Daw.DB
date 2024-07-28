using Daw.DB;
using Daw.DB.Interfaces;
using Daw.DB.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace DynamicEntitiesApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IDataService, SqliteDataService>();
                    services.AddSingleton<IEntityService, EntityService>();
                    services.AddSingleton<IRecordService, RecordService>();
                    services.AddTransient<DataApi>();
                    services.AddTransient<App>();
                })
                .Build();

            var app = host.Services.GetRequiredService<App>();
            app.Run();
        }
    }

    public class App
    {
        private readonly DataApi _dataApi;

        public App(DataApi databaseApi)
        {
            _dataApi = databaseApi;
            _dataApi.InitializeDatabase("DynamicEntities");

        }

        public void Run()
        {

            _dataApi.CreateEntity("customer", new Dictionary<string, string>
            {
                { "name", "TEXT" },
                { "age", "INTEGER" }
            });

            _dataApi.CreateEntity("product", new Dictionary<string, string>
            {
                { "name", "TEXT" },
                { "price", "REAL" }
            });

            _dataApi.CreateEntity("customer_order", new Dictionary<string, string>
            {
                { "orderNumber", "TEXT" },
                { "date", "TEXT" },
                { "customer_id", "INTEGER" }
            });

            _dataApi.CreateOneToManyRelation("customer", "customer_order", "customer_id");

            _dataApi.CreateEntity("order_product", new Dictionary<string, string>
            {
                { "order_id", "INTEGER" },
                { "product_id", "INTEGER" }
            });

            _dataApi.CreateManyToManyRelation("customer_order", "product", "order_product", new Dictionary<string, string>());

            _dataApi.AddRecord("customer", new Dictionary<string, object>
            {
                { "name", "Alice" },
                { "age", 30 }
            });

            _dataApi.AddRecord("product", new Dictionary<string, object>
            {
                { "name", "Laptop" },
                { "price", 999.99m }
            });

            _dataApi.AddRecord("customer_order", new Dictionary<string, object>
            {
                { "orderNumber", "A123" },
                { "date", "2024-07-26" },
                { "customer_id", 1 }
            });

            _dataApi.AddRecord("order_product", new Dictionary<string, object>
            {
                { "order_id", 1 },
                { "product_id", 1 }
            });

            Console.WriteLine("Fetching records from 'customer' entity...");
            _dataApi.DisplayRecords("customer");

            Console.WriteLine("Fetching records from 'product' entity...");
            _dataApi.DisplayRecords("product");

            Console.WriteLine("Fetching records from 'customer_order' entity...");
            _dataApi.DisplayRecords("customer_order");

            Console.WriteLine("Fetching records from 'order_product' entity...");
            _dataApi.DisplayRecords("order_product");

            Console.WriteLine("Updating schema of 'customer_order' entity...");
            _dataApi.UpdateSchema("customer_order", new Dictionary<string, string>
            {
                { "customerName", "TEXT" }
            });

            _dataApi.AddRecord("customer_order", new Dictionary<string, object>
            {
                { "orderNumber", "B456" },
                { "date", "2024-07-27" },
                { "customerName", "Bob" },
                { "customer_id", 1 }
            });

            Console.WriteLine("Fetching updated records from 'customer_order' entity...");
            _dataApi.DisplayRecords("customer_order");

            Console.WriteLine("Done.");
        }
    }
}
