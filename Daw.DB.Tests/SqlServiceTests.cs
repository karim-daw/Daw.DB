using Daw.DB.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Daw.DB.Tests
{

    [TestClass]
    public class SqlServiceTests
    {
        private ISqlService _sqlService;

        private string _databaseFilePath;
        private string _connectionString;

        [TestInitialize]
        public void Setup()
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configure services based on the loaded configuration
            var serviceProvider = ServiceConfiguration.ConfigureServices(configuration);
            _sqlService = serviceProvider.GetRequiredService<ISqlService>();


            _databaseFilePath = Path.GetTempFileName();
            _connectionString = $"Data Source={_databaseFilePath};Version=3;";


        }


        [TestCleanup]
        public void Cleanup()
        {
            // Ensure all connections are closed before cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

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
        public void CreateTable_Success()
        {
            // arrange object
            string sql = "CREATE TABLE TestTable (Name TEXT)";
            _sqlService.ExecuteCommand(sql, _connectionString);

            // act
            var result = _sqlService.ExecuteQuery("SELECT name FROM sqlite_master WHERE type='table' AND name='TestTable'", _connectionString);


            // assert
            Assert.AreEqual(1, result.Count());
        }

        // create test method for failure of creating table
        [ExpectedException(typeof(SQLiteException))]
        [TestMethod]
        public void WrongSql_Failure()
        {
            // arrange object to fail creating table
            string sql = "CREATE TABLES TestTable (Name TEXT)";
            _sqlService.ExecuteCommand(sql, _connectionString);
        }

        [TestMethod]
        public void InsertData_Success()
        {
            // arrange object
            string sql = "CREATE TABLE TestTable (Name TEXT)";
            _sqlService.ExecuteCommand(sql, _connectionString);

            // act
            _sqlService.ExecuteCommand("INSERT INTO TestTable (Name) VALUES ('Test')", _connectionString);
            var result = _sqlService.ExecuteQuery("SELECT * FROM TestTable", _connectionString);

            // assert
            Assert.AreEqual(1, result.Count());
        }

        // test method for transactions that will succeed
        [TestMethod]
        public void ExecuteInTransaction_Success()
        {
            // arrange object
            string sql = "CREATE TABLE TestTable (Name TEXT)";
            _sqlService.ExecuteCommand(sql, _connectionString);

            IEnumerable<(string sql, object parameters)> sqlCommands = new List<(string sql, object parameters)>
            {
                ("INSERT INTO TestTable (Name) VALUES ('Test1')", null),
                ("INSERT INTO TestTable (Name) VALUES ('Test2')", null)
            };

            // act
            _sqlService.ExecuteInTransaction(sqlCommands, _connectionString);
            var result = _sqlService.ExecuteQuery("SELECT * FROM TestTable", _connectionString);

            // assert
            Assert.AreEqual(2, result.Count());
        }

        // test method for transactions that will fail if one of the commands is wrong
        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void ExecuteInTransaction_Failure()
        {
            // arrange object
            string sql = "CREATE TABLE TestTable (Name TEXT)";
            _sqlService.ExecuteCommand(sql, _connectionString);

            IEnumerable<(string sql, object parameters)> sqlCommands = null;

            // create list of sql commands to execute

            // act
            _sqlService.ExecuteInTransaction(sqlCommands, _connectionString);

        }


    }
}
