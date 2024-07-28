using System.Collections.Generic;

namespace Daw.DB.Interfaces
{
    public interface IDataService
    {
        void CreateDatabase(string connectionString);

        string GetConnectionString();
    }

    public interface IEntityService
    {
        void CreateEntity(string entityName, Dictionary<string, string> entityFields);
        void UpdateSchema(string entityName, Dictionary<string, string> newFields);
        bool EntityExists(string entityName);
        void DeleteEntity(string entityName);

        // relations
        void CreateOneToManyRelation(string parentEntity, string childEntity, string foreignKey);
        void CreateManyToManyRelation(string firstEntity, string secondEntity, string relationTable, Dictionary<string, string> relationTableFields);
    }


    /// <summary>
    /// Service for working with records, implement CRUD operations
    /// </summary>
    public interface IRecordService
    {
        // add record
        void AddRecord(string entityName, Dictionary<string, object> record);

        // get records
        List<Dictionary<string, object>> GetRecords(string entityName);

        // update record
        void UpdateRecord(string entityName, Dictionary<string, object> record);

        // delete record
        void DeleteRecord(string entityName, Dictionary<string, object> record);
    }

    /// <summary>
    /// Service for working with queries, implement query execution
    /// </summary>
    public interface IQueryService
    {
        List<Dictionary<string, object>> ExecuteQuery(string query);
    }
}
