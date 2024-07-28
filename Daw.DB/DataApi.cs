using Daw.DB.Interfaces;
using System;
using System.Collections.Generic;

namespace Daw.DB
{
    public class DataApi(IDataService dataService, IEntityService entityService, IRecordService recordService)
    {
        private readonly IDataService _dataService = dataService;
        private readonly IEntityService _entityService = entityService;
        private readonly IRecordService _recordService = recordService;

        public void InitializeDatabase(string databaseName)
        {
            _dataService.CreateDatabase(databaseName);
        }

        public void CreateEntity(string entityName, Dictionary<string, string> fields)
        {
            if (!_entityService.EntityExists(entityName))
            {
                _entityService.CreateEntity(entityName, fields);
                Console.WriteLine($"Created entity '{entityName}'");
            }
            else
            {
                Console.WriteLine($"Entity '{entityName}' already exists");
            }
        }

        public void AddRecord(string entityName, Dictionary<string, object> record)
        {
            _recordService.AddRecord(entityName, record);
            Console.WriteLine($"Added record to '{entityName}'");
        }

        public void DisplayRecords(string entityName)
        {
            var records = _recordService.GetRecords(entityName);
            foreach (var record in records)
            {
                Console.WriteLine(string.Join(", ", record));
            }
        }

        public void UpdateSchema(string entityName, Dictionary<string, string> newFields)
        {
            _entityService.UpdateSchema(entityName, newFields);
            Console.WriteLine($"Updated schema of '{entityName}'");
        }

        public void CreateOneToManyRelation(string parentEntity, string childEntity, string foreignKey)
        {
            _entityService.CreateOneToManyRelation(parentEntity, childEntity, foreignKey);
            Console.WriteLine($"Created one-to-many relation from '{parentEntity}' to '{childEntity}'");
        }

        public void CreateManyToManyRelation(string firstEntity, string secondEntity, string relationTable, Dictionary<string, string> relationTableFields)
        {
            _entityService.CreateManyToManyRelation(firstEntity, secondEntity, relationTable, relationTableFields);
            Console.WriteLine($"Created many-to-many relation between '{firstEntity}' and '{secondEntity}' using '{relationTable}'");
        }
    }
}
