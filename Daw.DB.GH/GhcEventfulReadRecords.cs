using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Daw.DB.GH
{
    public class GhcEventfulReadRecords : GH_Component
    {
        private readonly IEventfulGhClientApi _eventDrivenGhClientApi;
        private readonly IDatabaseContext _databaseContext;
        private bool _eventTriggered;
        private bool _isSubscribed;

        public GhcEventfulReadRecords()
          : base("Read Records With Events", "RRWE",
              "Reads all records from the database given a table name, and automatically updates when the table changes if live listening is enabled",
              "Daw.DB", "READ")
        {
            _eventDrivenGhClientApi = ApiFactory.GetEventDrivenGhClientApi();
            _databaseContext = ApiFactory.GetDatabaseContext();
            _eventTriggered = false;
            _isSubscribed = false;
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to read the records from", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Read", "R", "Boolean to trigger record read", GH_ParamAccess.item);
            pManager.AddBooleanParameter("LiveListen", "LL", "Enable or disable live listening for table changes", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "Res", "Result of the database operation", GH_ParamAccess.item);
            pManager.AddTextParameter("Records", "Rec", "Records from the table", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool readRecord = false;
            string tableName = null;
            bool liveListen = false;

            if (!DA.GetData(0, ref tableName)) return;
            if (!DA.GetData(1, ref readRecord)) return;
            if (!DA.GetData(2, ref liveListen)) return;

            if (string.IsNullOrWhiteSpace(tableName))
            {
                DA.SetData(0, "Table name is invalid.");
                return;
            }

            if (liveListen && !_isSubscribed)
            {
                SubscribeToTableChanges();
                _isSubscribed = true;
            }
            else if (!liveListen && _isSubscribed)
            {
                UnsubscribeFromTableChanges();
                _isSubscribed = false;
            }

            // Output all records from the table
            List<string> records = new List<string>();
            string resultMessage = string.Empty;

            if (readRecord || _eventTriggered)
            {
                _eventTriggered = false; // Reset event flag

                if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString))
                {
                    resultMessage = "Connection string has not been set yet. " +
                                    "You have to create a database first. Use the Create Database component.";
                    DA.SetData(0, resultMessage);
                    return;
                }

                try
                {
                    IEnumerable<string> recordsEnumerable = ReadRecords(tableName);
                    records.AddRange(recordsEnumerable);
                    resultMessage = "Records read successfully.";
                }
                catch (Exception ex)
                {
                    resultMessage = $"Error reading records: {ex.Message}";
                }

                DA.SetData(0, resultMessage);
                DA.SetDataList(1, records);
            }
            else
            {
                DA.SetData(0, "Waiting for read command or table changes...");
                DA.SetDataList(1, records);
            }
        }

        /// <summary>
        /// Subscribe to table changes using the IEventfulGhClientApi methods.
        /// </summary>
        private void SubscribeToTableChanges()
        {
            _eventDrivenGhClientApi.SubscribeToTableChanges(OnTableChanged);
        }

        /// <summary>
        /// Unsubscribe from table changes using the IEventfulGhClientApi methods.
        /// </summary>
        private void UnsubscribeFromTableChanges()
        {
            _eventDrivenGhClientApi.UnsubscribeFromTableChanges(OnTableChanged);
        }

        private void OnTableChanged(object sender, TableChangedEventArgs args)
        {
            // Check if the table name matches
            string currentTableName = GetCurrentTableName();
            if (args.TableName == null || args.TableName != currentTableName)
            {
                return;
            }

            _eventTriggered = true;
            ExpireSolution(true); // Trigger Grasshopper to re-solve the component
        }

        /// <summary>
        /// Gets the current table name from the input parameter.
        /// </summary>
        /// <returns></returns>
        private string GetCurrentTableName()
        {
            string tableName = null;
            if (!Params.Input[0].VolatileData.IsEmpty)
            {
                tableName = Params.Input[0].VolatileData.get_Branch(0)[0] as string;
            }
            return tableName;
        }

        /// <summary>
        /// Read all records from the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private IEnumerable<string> ReadRecords(string tableName)
        {
            IEnumerable<dynamic> records = _eventDrivenGhClientApi.GetAllDictionaryRecords(tableName);
            foreach (var record in records)
            {
                // Convert the record to JSON string for better readability
                string jsonRecord = Newtonsoft.Json.JsonConvert.SerializeObject(record);
                yield return jsonRecord;
            }
        }

        protected override void BeforeSolveInstance()
        {
            _eventTriggered = false; // Reset event flag before each solution
        }

        //protected override void ExpireDownStreamObjects()
        //{
        //    // Override this method to prevent automatic expiration of downstream objects
        //    // when the component expires due to event triggering
        //}

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("C53B71A4-2B1B-4C50-9ED6-34975CA6B5D7");
    }
}
