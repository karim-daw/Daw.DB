using Daw.DB.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.IO;

namespace Daw.DB.Tests
{
    [TestClass]
    public class SQLiteConnectionFactoryTests
    {
        private ISQLiteConnectionFactory _factory;

        private string _databaseFilePath;
        private string _connectionString;

        [TestInitialize]
        public void Setup()
        {
            // _factory = new SQLiteConnectionFactory();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // configure the factory
            var serviceProvider = ServiceConfiguration.ConfigureServices(configuration);
            _factory = serviceProvider.GetRequiredService<ISQLiteConnectionFactory>();

            _databaseFilePath = Path.GetTempFileName();
            _connectionString = $"Data Source={_databaseFilePath};Version=3;";


        }

        [TestCleanup]
        public void Cleanup()
        {
            _factory = null;

            // delete the temp file
            if (File.Exists(_databaseFilePath))
            {
                try
                {
                    File.Delete(_databaseFilePath);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Failed to delete the file {_databaseFilePath}: {ex.Message}");
                }
            }

        }

        [TestMethod]
        public void TestExtractDbPath_ValidConnectionString_ReturnsCorrectPath()
        {
            // arrange
            string path = "C:\\Users\\daw\\source\\repos\\Daw.DB\\Daw.DB\\Daw.DB.Data\\Daw.DB.Data\\Data\\GhClient.db";
            string connectionString = $"Data Source={path};Version=3;";

            // act
            string dbPath = SQLiteConnectionFactory.ExtractDbPath(connectionString);

            // assert
            Assert.AreEqual(path, dbPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestExtractDbPath_NullOrEmptyConnectionString_ThrowsException()
        {
            // act
            SQLiteConnectionFactory.ExtractDbPath(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestExtractDbPath_MissingDataSource_ThrowsException()
        {
            // act
            SQLiteConnectionFactory.ExtractDbPath("Version=3;");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateConnection_NullOrEmptyConnectionString_ThrowsException()
        {
            // act
            _factory.CreateConnection(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateConnection_FileNotFound_ThrowsException()
        {
            // arrange
            string invalidConnectionString = "this_is_an_invalid_connection_string";
            // act
            _factory.CreateConnection(invalidConnectionString);
        }

        [TestMethod]
        public void TestCreateConnection_ValidConnectionString_ReturnsSQLiteConnection()
        {

            // act
            using (var connection = _factory.CreateConnection(_connectionString))
            {
                // assert
                Assert.IsNotNull(connection);
                Assert.IsInstanceOfType(connection, typeof(IDbConnection));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateConnectionFromFilePath_NullOrEmptyPath_ThrowsException()
        {
            // act
            _factory.CreateConnectionFromFilePath(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestCreateConnectionFromFilePath_FileNotFound_ThrowsException()
        {
            // arrange
            string invalidFilePath = "invalid_path.db";

            // act
            _factory.CreateConnectionFromFilePath(invalidFilePath);
        }

        [TestMethod]
        public void TestCreateConnectionFromFilePath_ValidPath_ReturnsSQLiteConnection()
        {
            // if postgress is used, skip this test
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                return;
            }

            // act
            using (var connection = _factory.CreateConnectionFromFilePath(_databaseFilePath))
            {
                // assert
                Assert.IsNotNull(connection);
                Assert.IsInstanceOfType(connection, typeof(IDbConnection));
            }
        }
    }
}
