namespace Daw.DB.Data.Interfaces
{
    public interface IJsonHandler
    {
        void AddRecordFromJson(string tableName, string jsonRecord, string connectionString);
        void AddTableFromJson(string tableName, string jsonSchema, string connectionString);

        void UpdateRecordFromJson(string tableName, object id, string jsonRecord, string connectionString);
    }
}
