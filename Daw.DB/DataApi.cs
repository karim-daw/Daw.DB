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

        #region Entity

        /// <summary>
        /// Create a new entity with the given fields
        /// If the entity already exists, do nothing
        /// This is equivalent to creating a table in a relational database
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="fields"></param>
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

        /// <summary>
        /// Delete the entity with the given name
        /// This is equivalent to dropping a table in a relational database
        /// </summary>
        /// <param name="entityName"></param>
        public void DeleteEntity(string entityName)
        {
            _entityService.DeleteEntity(entityName);
            Console.WriteLine($"Deleted entity '{entityName}'");
        }

        /// <summary>
        /// Update the schema of the entity with the given name
        /// This is equivalent to adding or removing columns from a table in a relational database
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="newFields"></param>
        /// <returns></returns>
        public void UpdateSchema(string entityName, Dictionary<string, string> newFields)
        {
            _entityService.UpdateSchema(entityName, newFields);
            Console.WriteLine($"Updated schema of '{entityName}'");
        }

        /// <summary>
        ///  Create a one-to-many relation between two entities
        ///  This is equivalent to adding a foreign key constraint in a relational database
        ///  The foreign key is a column in the child entity that references the primary key of the parent entity
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="childEntity"></param>
        /// <param name="foreignKey"></param>
        public void CreateOneToManyRelation(string parentEntity, string childEntity, string foreignKey)
        {
            _entityService.CreateOneToManyRelation(parentEntity, childEntity, foreignKey);
            Console.WriteLine($"Created one-to-many relation from '{parentEntity}' to '{childEntity}'");
        }

        /// <summary>
        /// Create a many-to-many relation between two entities
        /// This is equivalent to creating a junction table in a relational database
        /// The junction table contains foreign keys that reference the primary keys of the two entities
        /// The relationTableFields dictionary specifies the fields in the junction table
        /// </summary>
        /// <param name="firstEntity"></param>
        /// <param name="secondEntity"></param>
        /// <param name="relationTable"></param>
        /// <param name="relationTableFields"></param>
        public void CreateManyToManyRelation(string firstEntity, string secondEntity, string relationTable, Dictionary<string, string> relationTableFields)
        {
            _entityService.CreateManyToManyRelation(firstEntity, secondEntity, relationTable, relationTableFields);
            Console.WriteLine($"Created many-to-many relation between '{firstEntity}' and '{secondEntity}' using '{relationTable}'");
        }
        #endregion

        #region Record

        /// <summary>
        /// Add a record to the entity with the given name
        /// The record is a dictionary where the keys are the field names and the values are the field values
        /// This is equivalent to inserting a row into a table in a relational database
        /// If the entity does not exist, do nothing
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="record"></param>
        public void AddRecord(string entityName, Dictionary<string, object> record)
        {
            _recordService.AddRecord(entityName, record);
            Console.WriteLine($"Added record to '{entityName}'");
        }

        /// <summary>
        /// Display all records in the entity with the given name
        /// This is equivalent to selecting all rows from a table in a relational database
        /// Each record is a dictionary where the keys are the field names and the values are the field values
        /// If the entity does not exist, do nothing
        /// </summary>
        /// <param name="entityName"></param>
        public void DisplayRecords(string entityName)
        {
            var records = _recordService.GetRecords(entityName);
            foreach (var record in records)
            {
                Console.WriteLine(string.Join(", ", record));
            }
        }

        /// <summary>
        /// Delete a record from the entity with the given name
        /// The record is a dictionary where the keys are the field names and the values are the field values
        /// This is equivalent to deleting a row from a table in a relational database
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="record"></param>
        public void DeleteRecord(string entityName, Dictionary<string, object> record)
        {
            _recordService.DeleteRecord(entityName, record);
            Console.WriteLine($"Deleted record from '{entityName}'");
        }

        /// <summary>
        /// Update a record in the entity with the given name
        /// The record is a dictionary where the keys are the field names and the values are the field values
        /// This is equivalent to updating a row in a table in a relational database
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="record"></param>
        public void UpdateRecord(string entityName, Dictionary<string, object> record)
        {
            _recordService.UpdateRecord(entityName, record);
            Console.WriteLine($"Updated record in '{entityName}'");
        }
        #endregion


    }
}
