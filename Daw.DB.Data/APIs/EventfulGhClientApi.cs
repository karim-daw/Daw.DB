using System;
using System.Collections.Generic;

namespace Daw.DB.Data.APIs
{

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


    public interface ITableChangeNotifier
    {
        event EventHandler<TableChangedEventArgs> TableChanged;

        void NotifyTableChanged(string tableName, string operation);
    }

    public class TableChangeNotifier : ITableChangeNotifier
    {
        public event EventHandler<TableChangedEventArgs> TableChanged;

        public void NotifyTableChanged(string tableName, string operation)
        {
            TableChanged?.Invoke(this, new TableChangedEventArgs(tableName, operation));
        }
    }


    public interface IEventfulGhClientApi : IGhClientApi
    {
        void SubscribeToTableChanges(EventHandler<TableChangedEventArgs> handler);
        void UnsubscribeFromTableChanges(EventHandler<TableChangedEventArgs> handler);
    }

    public class EventfulGhClientApi : IEventfulGhClientApi
    {
        private readonly IGhClientApi _ghClientApi;
        private readonly ITableChangeNotifier _tableChangeNotifier;

        public EventfulGhClientApi(IGhClientApi ghClientApi, ITableChangeNotifier tableChangeNotifier)
        {
            _ghClientApi = ghClientApi;
            _tableChangeNotifier = tableChangeNotifier;
        }

        // IGhClientApi methods, delegated to the wrapped instance
        public string CreateConnection(string connectionString)
        {
            return _ghClientApi.CreateConnection(connectionString);
        }

        public string CreateTable(string tableName, Dictionary<string, string> columns, string connectionString)
        {
            return _ghClientApi.CreateTable(tableName, columns, connectionString);
        }

        public string AddDictionaryRecord(string tableName, Dictionary<string, object> record, string connectionString)
        {
            var result = _ghClientApi.AddDictionaryRecord(tableName, record, connectionString);
            _tableChangeNotifier.NotifyTableChanged(tableName, "AddRecord");
            return result;
        }

        public string AddDictionaryRecordInTransaction(string tableName, string connectionString, params Dictionary<string, object>[] records)
        {
            var result = _ghClientApi.AddDictionaryRecordInTransaction(tableName, connectionString, records);
            _tableChangeNotifier.NotifyTableChanged(tableName, "AddRecordsInTransaction");
            return result;
        }

        public IEnumerable<dynamic> GetAllDictionaryRecords(string tableName, string connectionString)
        {
            return _ghClientApi.GetAllDictionaryRecords(tableName, connectionString);
        }

        public dynamic GetDictionaryRecordById(string tableName, object id, string connectionString)
        {
            return _ghClientApi.GetDictionaryRecordById(tableName, id, connectionString);
        }

        public void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record, string connectionString)
        {
            _ghClientApi.UpdateDictionaryRecord(tableName, id, record, connectionString);
            _tableChangeNotifier.NotifyTableChanged(tableName, "UpdateRecord");
        }

        public void DeleteRecord(string tableName, object id, string connectionString)
        {
            _ghClientApi.DeleteRecord(tableName, id, connectionString);
            _tableChangeNotifier.NotifyTableChanged(tableName, "DeleteRecord");
        }

        // IEventfulGhClientApi methods
        public void SubscribeToTableChanges(EventHandler<TableChangedEventArgs> handler)
        {
            _tableChangeNotifier.TableChanged += handler;
        }

        public void UnsubscribeFromTableChanges(EventHandler<TableChangedEventArgs> handler)
        {
            _tableChangeNotifier.TableChanged -= handler;
        }
    }

}
