using Daw.DB.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Daw.DB.Tests
{
    [TestClass]
    public class SQLiteConnectionFactoryTests
    {
        private ISQLiteConnectionFactory _factory;
        private IDatabaseContext _databaseContext;

        private string _databaseFilePath;
        private string _connectionString;

        [TestInitialize]
        public void Setup()
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                // Add other configuration sources if needed
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Configure services
            var serviceProvider = ServiceConfiguration.ConfigureServices(configuration);

            // Get the ISQLiteConnectionFactory instance
            _factory = serviceProvider.GetRequiredService<ISQLiteConnectionFactory>();

            // Get the IDatabaseContext instance
            _databaseContext = serviceProvider.GetRequiredService<IDatabaseContext>();

            // Set up a temporary database file path
            _databaseFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

            // Set the connection string in the database context
            _connectionString = $"Data Source={_databaseFilePath};Version=3;";
            _databaseContext.ConnectionString = _connectionString;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _factory = null;
            _databaseContext = null;

            // Delete the temp file
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
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateConnection_NullOrEmptyConnectionString_ThrowsException()
        {
            string invalidConnectionString = null;

            // set the connection string in the database context
            _databaseContext.ConnectionString = invalidConnectionString;

            // Act
            _factory.CreateConnection();
        }


        [TestMethod]
        public void TestCreateConnection_ValidConnectionString_ReturnsSQLiteConnection()
        {
            // Act
            using (var connection = _factory.CreateConnection())
            {
                // Assert
                Assert.IsNotNull(connection);
                Assert.IsInstanceOfType(connection, typeof(IDbConnection));
                Assert.AreEqual(_connectionString, connection.ConnectionString);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateConnectionFromFilePath_NullOrEmptyPath_ThrowsException()
        {
            // Act
            _factory.CreateConnectionFromFilePath(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestCreateConnectionFromFilePath_FileNotFound_ThrowsException()
        {
            // Arrange
            string invalidFilePath = "invalid_path.db";

            // Act
            _factory.CreateConnectionFromFilePath(invalidFilePath);
        }

        [TestMethod]
        public void TestCreateConnectionFromFilePath_ValidPath_ReturnsSQLiteConnection()
        {
            // Arrange
            // Ensure the file exists
            if (!File.Exists(_databaseFilePath))
            {
                SQLiteConnection.CreateFile(_databaseFilePath);
            }

            // Act
            using (var connection = _factory.CreateConnectionFromFilePath(_databaseFilePath))
            {
                // Assert
                Assert.IsNotNull(connection);
                Assert.IsInstanceOfType(connection, typeof(IDbConnection));
                Assert.IsTrue(connection.ConnectionString.Contains(_databaseFilePath));
            }
        }

        // test ping
        [TestMethod]

        public void TestPing_ValidConnection_ReturnsTrue()
        {
            // Act
            Assert.IsTrue(_factory.Ping(_connectionString));

        }

        // assert that false is returned for any exception
        [TestMethod]
        public void TestPing_ExceptionThrown_ReturnsFalse()
        {
            // Arrange
            string invalidConnectionString = "invalid_connection_string";

            // Act
            Assert.IsFalse(_factory.Ping(invalidConnectionString));
        }

    }
}
