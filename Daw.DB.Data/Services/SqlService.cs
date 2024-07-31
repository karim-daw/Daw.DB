using Dapper;
using Daw.DB.Data.Interfaces;
using System.Collections.Generic;

namespace Daw.DB.Data.Services
{
    public class SqlService : ISqlService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public SqlService(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<dynamic> ExecuteQuery(string sql, object parameters = null)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                db.Open();
                return db.Query(sql, parameters);
            }
        }

        public void ExecuteCommand(string sql, object parameters = null)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                db.Open();
                db.Execute(sql, parameters);
            }
        }
    }
}
