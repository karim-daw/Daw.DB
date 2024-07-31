using System.Data;

namespace Daw.DB.Data.Interfaces
{
    public interface IDatabaseConnectionFactory
    {
        /// <summary>
        /// Create a new connection to the database
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();
    }
}
