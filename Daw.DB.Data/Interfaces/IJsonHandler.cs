namespace Daw.DB.Data.Interfaces
{
    public interface IJsonHandler
    {
        void AddRecordFromJson(string tableName, string jsonRecord);
        void AddTableFromJson(string tableName, string jsonSchema);

        void UpdateRecordFromJson(string tableName, object id, string jsonRecord);
    }
}
