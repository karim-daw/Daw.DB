using System.Collections.Generic;

namespace DynamicEntitiesApp.Interfaces
{
    public interface IDataService
    {
        void CreateDatabase(string databaseName);
        void CreateEntity(string entityName, Dictionary<string, string> entityFields);
        void AddRecord(string entityName, Dictionary<string, object> record);
        List<Dictionary<string, object>> GetRecords(string entityName);
        void UpdateSchema(string entityName, Dictionary<string, string> newFields);
        bool EntityExists(string entityName);

        // Dynamic entity methods
        void CreateDynamicEntity(string entityName, Dictionary<string, string> fields);
        void AddDynamicRecord(string entityName, Dictionary<string, object> record);
        List<Dictionary<string, object>> GetDynamicRecords(string entityName);
        void UpdateDynamicSchema(string entityName, Dictionary<string, string> newFields);

        // New methods for handling relationships
        void CreateOneToManyRelation(string parentEntity, string childEntity, string foreignKey);
        void CreateManyToManyRelation(string firstEntity, string secondEntity, string relationTable, Dictionary<string, string> relationTableFields);
    }
}
