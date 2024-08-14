using Daw.DB.Data.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.IO;

namespace Daw.DB.Tests
{
    [TestClass]
    public class SQLiteConnectionFactoryTests
    {
        private SQLiteConnectionFactory _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new SQLiteConnectionFactory();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _factory = null;
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
            // arrange
            string tempFilePath = Path.GetTempFileName();
            string connectionString = $"Data Source={tempFilePath};Version=3;";

            // act
            using (var connection = _factory.CreateConnection(connectionString))
            {
                // assert
                Assert.IsNotNull(connection);
                Assert.IsInstanceOfType(connection, typeof(IDbConnection));
            }

            // cleanup
            File.Delete(tempFilePath);
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
            // arrange
            string tempFilePath = Path.GetTempFileName();

            // act
            using (var connection = _factory.CreateConnectionFromFilePath(tempFilePath))
            {
                // assert
                Assert.IsNotNull(connection);
                Assert.IsInstanceOfType(connection, typeof(IDbConnection));
            }

            // cleanup
            File.Delete(tempFilePath);
        }
    }
}
