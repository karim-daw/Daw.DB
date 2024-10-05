namespace Daw.DB.Data.Services
{

    public interface IDatabaseContext
    {
        string ConnectionString { get; set; }
    }

    public class DatabaseContext : IDatabaseContext
    {
        public string ConnectionString { get; set; }
    }

}
