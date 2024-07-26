using DynamicEntitiesApp.Interfaces;
using System;
using System.Collections.Generic;

namespace DynamicEntitiesApp
{
    public class DatabaseApi
    {
        private readonly IDataService _dataService;

        public DatabaseApi(IDataService dataService)
        {
            _dataService = dataService;
        }

        public void InitializeDatabase(string databaseName)
        {
            _dataService.CreateDatabase(databaseName);
        }

        public void CreateEntity(string entityName, Dictionary<string, string> fields)
        {
            if (!_dataService.EntityExists(entityName))
            {
                _dataService.CreateEntity(entityName, fields);
                Console.WriteLine($"Created entity '{entityName}'");
            }
            else
            {
                Console.WriteLine($"Entity '{entityName}' already exists");
            }
        }

        public void AddRecord(string entityName, Dictionary<string, object> record)
        {
            _dataService.AddRecord(entityName, record);
            Console.WriteLine($"Added record to '{entityName}'");
        }

        public void DisplayRecords(string entityName)
        {
            var records = _dataService.GetRecords(entityName);
            foreach (var record in records)
            {
                Console.WriteLine(string.Join(", ", record));
            }
        }

        public void UpdateSchema(string entityName, Dictionary<string, string> newFields)
        {
            _dataService.UpdateSchema(entityName, newFields);
            Console.WriteLine($"Updated schema of '{entityName}'");
        }

        public void CreateOneToManyRelation(string parentEntity, string childEntity, string foreignKey)
        {
            _dataService.CreateOneToManyRelation(parentEntity, childEntity, foreignKey);
            Console.WriteLine($"Created one-to-many relation from '{parentEntity}' to '{childEntity}'");
        }

        public void CreateManyToManyRelation(string firstEntity, string secondEntity, string relationTable, Dictionary<string, string> relationTableFields)
        {
            _dataService.CreateManyToManyRelation(firstEntity, secondEntity, relationTable, relationTableFields);
            Console.WriteLine($"Created many-to-many relation between '{firstEntity}' and '{secondEntity}' using '{relationTable}'");
        }
    }
}
