namespace Daw.DB.Data.Interfaces
{
    public interface IDatabaseCreationFactory
    {
        /// <summary>
        /// Create a new database, relacent for SQLite
        /// </summary>
        /// <param name="dbPath"></param>
        void CreateDatabase(string dbPath);
    }
}
