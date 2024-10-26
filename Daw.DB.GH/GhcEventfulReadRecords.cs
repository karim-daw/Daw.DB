using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Daw.DB.GH {
    public class GhcEventfulReadRecords : GH_Component {
        private readonly IEventfulGhClientApi _eventDrivenGhClientApi;
        private readonly IDatabaseContext _dataBaseContext;
        private bool _eventTriggered;
        private bool _liveListening;
        private bool _subscribedToTable;

        public GhcEventfulReadRecords()
          : base("ReadRecordWithEvents", "RRW",
              "Read all records from the database given a table name, and automatically updates when the table changes if live listening is enabled",
              "Daw.DB", "READ") {
            _eventDrivenGhClientApi = ApiFactory.GetEventDrivenGhClientApi();
            _dataBaseContext = ApiFactory.GetDatabaseContext();
            _eventTriggered = false;
            _liveListening = false;
            _subscribedToTable = false;
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to read the records from", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Read", "R", "Boolean to trigger record read", GH_ParamAccess.item);
            pManager.AddBooleanParameter("LiveListen", "LL", "Enable or disable live listening for table changes", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddTextParameter("Records", "R", "List of JSON formatted records from the table", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            bool readRecord = false;
            string tableName = null;
            bool liveListen = false;

            // Get input data
            if (!DA.GetData(0, ref tableName))
                return;
            if (!DA.GetData(1, ref readRecord))
                return;
            if (!DA.GetData(2, ref liveListen))
                return;

            // Manage live listening state
            if (liveListen != _liveListening) {
                _liveListening = liveListen;
                if (_liveListening && !_subscribedToTable) {
                    SubscribeToTableChanges(tableName);
                    _subscribedToTable = true;
                }
                else if (!_liveListening && _subscribedToTable) {
                    UnsubscribeFromTableChanges(tableName);
                    _subscribedToTable = false;
                }
            }

            // Perform manual read when 'Read' is true, independent of live listening
            if (readRecord) {
                _eventTriggered = false; // Reset event flag (because we're manually reading)
                var jsonRecordsList = new List<string>();

                // ensure indented JSON output
                JsonSerializerOptions options = SetJsonIndentationOptions();

                foreach (var record in ReadRecords(tableName)) {
                    var recordDict = ConvertRecordToDictionary(record);
                    string jsonRecord = JsonSerializer.Serialize(recordDict, options);
                    jsonRecordsList.Add(jsonRecord);
                }

                // Output the list of JSON records to Grasshopper
                DA.SetDataList(0, jsonRecordsList);
            }
        }

        private static JsonSerializerOptions SetJsonIndentationOptions() {
            return new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        /// <summary>
        /// Subscribe to table changes and trigger component update.
        /// </summary>
        /// <param name="tableName"></param>
        private void SubscribeToTableChanges(string tableName) {
            _eventDrivenGhClientApi.SubscribeToTableChanges((sender, args) =>
            {
                if (_liveListening && args.TableName == tableName) {
                    _eventTriggered = true;
                    ExpireSolution(true); // Trigger Grasshopper to re-solve the component
                }
            });
        }

        /// <summary>
        /// Unsubscribe from table changes.
        /// </summary>
        /// <param name="tableName"></param>
        private void UnsubscribeFromTableChanges(string tableName) {
            _eventDrivenGhClientApi.UnsubscribeFromTableChanges((sender, args) =>
            {
                if (args.TableName == tableName) {
                    _eventTriggered = false;
                }
            });
        }

        /// <summary>
        /// Read all records from the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private IEnumerable<dynamic> ReadRecords(string tableName) {
            IEnumerable<dynamic> records = _eventDrivenGhClientApi.GetAllDictionaryRecords(tableName);
            foreach (var record in records) {
                yield return record;
            }
        }

        /// <summary>
        /// Converts dynamic DapperRow objects to a dictionary of keys and values.
        /// </summary>
        private Dictionary<string, object> ConvertRecordToDictionary(dynamic record) {
            var recordDict = new Dictionary<string, object>();

            // Cast the dynamic record to IDictionary to access keys and values
            if (record is IDictionary<string, object> dict) {
                foreach (var kvp in dict) {
                    recordDict[kvp.Key] = kvp.Value;
                }
            }

            return recordDict;
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("C53B71A4-2B1B-4C50-9ED6-34975CA6B5D7");
    }
}
