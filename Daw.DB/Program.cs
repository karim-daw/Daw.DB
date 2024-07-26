using DynamicEntitiesApp.Interfaces;
using DynamicEntitiesApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    services.AddTransient<App>();
                })
                .Build();

            var app = host.Services.GetRequiredService<App>();
            app.Run();
        }
    }

    public class App
    {
        private readonly IDataService _dataService;

        public App(IDataService dataService)
        {
            _dataService = dataService;
        }

        public void Run()
        {
            _dataService.CreateDatabase("DynamicEntities");

            AddEntityIfNotExists("customer", new Dictionary<string, string>
            {
                { "name", "TEXT" },
                { "age", "INTEGER" }
            });

            AddEntityIfNotExists("product", new Dictionary<string, string>
            {
                { "name", "TEXT" },
                { "price", "REAL" }
            });

            AddEntityIfNotExists("customer_order", new Dictionary<string, string>
            {
                { "orderNumber", "TEXT" },
                { "date", "TEXT" }
            });

            AddDynamicRecord("customer", new Dictionary<string, object>
            {
                { "name", "Alice" },
                { "age", 30 }
            });

            AddDynamicRecord("product", new Dictionary<string, object>
            {
                { "name", "Laptop" },
                { "price", 999.99m }
            });

            AddDynamicRecord("customer_order", new Dictionary<string, object>
            {
                { "orderNumber", "A123" },
                { "date", "2024-07-26" }
            });

            Console.WriteLine("Fetching records from 'customer' entity...");
            DisplayRecords(_dataService.GetDynamicRecords("customer"));

            Console.WriteLine("Fetching records from 'product' entity...");
            DisplayRecords(_dataService.GetDynamicRecords("product"));

            Console.WriteLine("Fetching records from 'customer_order' entity...");
            DisplayRecords(_dataService.GetDynamicRecords("customer_order"));

            Console.WriteLine("Updating schema of 'customer_order' entity...");
            _dataService.UpdateDynamicSchema("customer_order", new Dictionary<string, string>
            {
                { "customerName", "TEXT" }
            });

            AddDynamicRecord("customer_order", new Dictionary<string, object>
            {
                { "orderNumber", "B456" },
                { "date", "2024-07-27" },
                { "customerName", "Bob" }
            });

            Console.WriteLine("Fetching updated records from 'customer_order' entity...");
            DisplayRecords(_dataService.GetDynamicRecords("customer_order"));

            Console.WriteLine("Done.");
        }

        private void AddEntityIfNotExists(string entityName, Dictionary<string, string> fields)
        {
            if (!_dataService.EntityExists(entityName))
            {
                _dataService.CreateDynamicEntity(entityName, fields);
                Console.WriteLine($"Created entity '{entityName}'");
            }
            else
            {
                Console.WriteLine($"Entity '{entityName}' already exists");
            }
        }

        private void AddDynamicRecord(string entityName, Dictionary<string, object> record)
        {
            Console.WriteLine($"Adding a record to '{entityName}' entity...");
            _dataService.AddDynamicRecord(entityName, record);
        }

        private void DisplayRecords(List<Dictionary<string, object>> records)
        {
            foreach (var record in records)
            {
                Console.WriteLine(string.Join(", ", record.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
            }
        }
    }
}
