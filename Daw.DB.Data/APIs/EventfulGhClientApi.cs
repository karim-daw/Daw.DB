using System;
using System.Collections.Generic;

namespace Daw.DB.Data.APIs
{


    /////////////////////////////////////// Table Change Publisher //////////////////////////////////////////////



    /// <summary>
    /// Defines the methods that a class must implement to be able to notify subscribers when a table is changed.
    /// </summary>
    public interface ITableChangePublisher
    {
        event EventHandler<TableChangedEventArgs> TableChanged;

        void PublishTableChanged(string tableName, string operation);
    }



    /// <summary>
    /// Event arguments for the TableChanged event.
    /// This class contains the name of the table that was changed and the operation that was performed.
    /// Used to notify subscribers when a table is changed.
    /// </summary>
    public class TableChangedEventArgs : EventArgs
    {
        public string TableName { get; }
        public string Operation { get; }

        public TableChangedEventArgs(string tableName, string operation)
        {
            TableName = tableName;
            Operation = operation;
        }
    }



    /// <summary>
    /// Table change Publisher class that implements the ITableChangePublisher interface.
    /// It raises an event when a table is changed.
    /// An event is raised when a record is added, updated, or deleted from a table.
    /// This can be used in conjunction with a stream reader to notify subscribers when a table is changed.
    /// </summary>
    public class TableChangePublisher : ITableChangePublisher
    {
        // Event that is raised when a table is changed
        public event EventHandler<TableChangedEventArgs> TableChanged;


        /// <summary>
        /// Notifies subscribers when a table is changed.
        /// This method raises the TableChanged event.
        /// So subscribers can be notified when a table is changed.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="operation"></param>
        public void PublishTableChanged(string tableName, string operation)
        {
            // Raise the event when a table is changed
            // ? is a null-conditional operator that checks if the event has any subscribers
            // If it does, the event is raised
            TableChanged?.Invoke(this, new TableChangedEventArgs(tableName, operation));
        }
    }



    /////////////////////////////////////// Eventull GH Client //////////////////////////////////////////////



    /// <summary>
    /// Interface for a Grasshopper client API that can notify subscribers when a table is changed.
    /// This interface extends the IGhClientApi interface.
    /// But includes additional methods to subscribe and unsubscribe from table change events.
    /// </summary>
    public interface IEventfulGhClientApi : IGhClientApi
    {
        void SubscribeToTableChanges(EventHandler<TableChangedEventArgs> handler);
        void UnsubscribeFromTableChanges(EventHandler<TableChangedEventArgs> handler);
    }


    /// <summary>
    /// Implementation of the IEventfulGhClientApi interface.
    /// Allows subscribers to be notified when a table is changed.
    /// And has all methods of the IGhClientApi interface.
    /// </summary>
    public class EventfulGhClientApi : IEventfulGhClientApi
    {

        private readonly IGhClientApi _ghClientApi;
        private readonly ITableChangePublisher _tableChangePublisher;

        public EventfulGhClientApi(IGhClientApi ghClientApi, ITableChangePublisher tableChangePublisher)
        {
            _ghClientApi = ghClientApi;
            _tableChangePublisher = tableChangePublisher;
        }



        /// <summary>
        /// Wrapper method for the CreateConnection method of the IGhClientApi interface.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public string CreateConnection(string connectionString)
        {
            return _ghClientApi.CreateConnection(connectionString);
        }



        /// <summary>
        /// Wrapper method for the CreateTable method of the IGhClientApi interface.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public string CreateTable(string tableName, Dictionary<string, string> columns, string connectionString)
        {
            return _ghClientApi.CreateTable(tableName, columns, connectionString);
        }



        /// <summary>
        /// Eventful wrapper method for the AddDictionaryRecord method of the IGhClientApi interface.
        /// This method notifies subscribers when a record is added to a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="record"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public string AddDictionaryRecord(string tableName, Dictionary<string, object> record, string connectionString)
        {
            var result = _ghClientApi.AddDictionaryRecord(tableName, record, connectionString);
            _tableChangePublisher.PublishTableChanged(tableName, "AddRecord");
            return result;
        }



        /// <summary>
        /// Eventful wrapper method for the AddDictionaryRecordInTransaction method of the IGhClientApi interface.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        public string AddDictionaryRecordInTransaction(string tableName, IEnumerable<Dictionary<string, object>> records, string connectionString)
        {
            var result = _ghClientApi.AddDictionaryRecordInTransaction(tableName, records, connectionString);
            _tableChangePublisher.PublishTableChanged(tableName, "AddRecordsInTransaction");
            return result;
        }


        public string AddDictionaryRecordBatch(string tableName, IEnumerable<Dictionary<string, object>> records, string connectionString)
        {
            var result = _ghClientApi.AddDictionaryRecordBatch(tableName, records, connectionString);
            _tableChangePublisher.PublishTableChanged(tableName, "AddRecordsBatch");
            return result;
        }



        /// <summary>
        /// Wrapper method for the GetAllDictionaryRecords method of the IGhClientApi interface.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> GetAllDictionaryRecords(string tableName, string connectionString)
        {
            return _ghClientApi.GetAllDictionaryRecords(tableName, connectionString);
        }



        /// <summary>
        /// Wrapper method for the GetDictionaryRecordById method of the IGhClientApi interface.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public dynamic GetDictionaryRecordById(string tableName, object id, string connectionString)
        {
            return _ghClientApi.GetDictionaryRecordById(tableName, id, connectionString);
        }



        /// <summary>
        /// Eventful wrapper method for the UpdateDictionaryRecord method of the IGhClientApi interface.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="record"></param>
        /// <param name="connectionString"></param>
        public void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record, string connectionString)
        {
            _ghClientApi.UpdateDictionaryRecord(tableName, id, record, connectionString);
            _tableChangePublisher.PublishTableChanged(tableName, "UpdateRecord");
        }


        /// <summary>
        /// Eventful wrapper method for the UpdateDictionaryRecordInTransaction method of the IGhClientApi interface.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="connectionString"></param>
        public void DeleteRecord(string tableName, object id, string connectionString)
        {
            _ghClientApi.DeleteRecord(tableName, id, connectionString);
            _tableChangePublisher.PublishTableChanged(tableName, "DeleteRecord");
        }


        /// <summary>
        /// Subscribe to table change events.
        /// </summary>
        /// <param name="handler"></param>
        public void SubscribeToTableChanges(EventHandler<TableChangedEventArgs> handler)
        {
            _tableChangePublisher.TableChanged += handler;
        }

        /// <summary>
        /// Unsubscribe from table change events.
        /// </summary>
        /// <param name="handler"></param>
        public void UnsubscribeFromTableChanges(EventHandler<TableChangedEventArgs> handler)
        {
            _tableChangePublisher.TableChanged -= handler;
        }


    }

}
