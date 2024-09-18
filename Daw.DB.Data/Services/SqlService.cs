using Dapper;
using System.Collections.Generic;

namespace Daw.DB.Data.Services
{
    public interface ISqlService
    {
        IEnumerable<dynamic> ExecuteQuery(string sql, string connectionString, object parameters = null);
        IEnumerable<T> ExecuteQuery<T>(string sql, string connectionString, object parameters = null);
        void ExecuteCommand(string sql, string connectionString, object parameters = null);
        void ExecuteInTransaction(IEnumerable<(string sql, object parameters)> sqlCommands, string connectionString);

        // New method to retrieve column metadata
        Dictionary<string, string> GetColumnMetadata(string tableName, string connectionString);
    }

    public class SqlService : ISqlService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public SqlService(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<dynamic> ExecuteQuery(string sql, string connectionString, object parameters = null)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                return db.Query(sql, parameters);
            }
        }

        public IEnumerable<T> ExecuteQuery<T>(string sql, string connectionString, object parameters = null)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                return db.Query<T>(sql, parameters);
            }
        }

        public void ExecuteCommand(string sql, string connectionString, object parameters = null)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                db.Execute(sql, parameters);
            }
        }

        public void ExecuteInTransaction(IEnumerable<(string sql, object parameters)> sqlCommands, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        foreach (var command in sqlCommands)
                        {
                            db.Execute(command.sql, command.parameters, transaction);
                        }
                        transaction.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        transaction.Rollback();
                        throw new System.Exception("Transaction failed", ex);
                    }
                }
            }
        }

        // New method to retrieve column metadata
        public Dictionary<string, string> GetColumnMetadata(string tableName, string connectionString)
        {
            string query = $"PRAGMA table_info({tableName});";
            var columnMetadata = new Dictionary<string, string>();

            var result = ExecuteQuery(query, connectionString);
            foreach (var row in result)
            {
                columnMetadata.Add(row.name, row.type); // Assuming 'name' and 'type' are fields in the PRAGMA result
            }

            return columnMetadata;
        }
    }
}
