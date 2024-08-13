using System.Collections.Generic;

namespace Daw.DB.Data.Interfaces
{
    public interface ISqlService
    {
        IEnumerable<dynamic> ExecuteQuery(string sql, string connectionString, object parameters = null);
        void ExecuteCommand(string sql, string connectionString, object parameters = null);
    }
}
