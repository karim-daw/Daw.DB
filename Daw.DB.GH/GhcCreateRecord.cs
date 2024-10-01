using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Daw.DB.GH
{
    public class GhcCreateRecord : GH_Component
    {
        private readonly IEventfulGhClientApi _eventfulGhClientApi;
        private readonly IDatabaseContext _databaseContext;

        public GhcCreateRecord()
          : base("Create Record", "CreateRec",
            "Creates a record and inserts it into the database",
            "Daw.DB", "CREATE")
        {
            // Use the ApiFactory to get pre-configured instances
            _eventfulGhClientApi = ApiFactory.GetEventDrivenGhClientApi();
            _databaseContext = ApiFactory.GetDatabaseContext();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to insert the record into", GH_ParamAccess.item);
            pManager.AddBooleanParameter("AddRecord", "AR", "Boolean to trigger record addition", GH_ParamAccess.item);
            pManager.AddTextParameter("JsonRecord", "JR", "JSON representation of record key-value pairs or list of records", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool addRecord = false;
            string tableName = null;
            List<string> jsonRecords = new List<string>();

            // Retrieve input data
            if (!DA.GetData(0, ref tableName)) return;
            if (!DA.GetData(1, ref addRecord)) return;
            if (!DA.GetDataList(2, jsonRecords)) return;

            // Add records to the table
            if (addRecord)
            {
                string result = CreateRecords(tableName, jsonRecords);
                DA.SetData(0, result);
            }
        }

        // Wrapper method to handle record creation
        private string CreateRecords(string tableName, List<string> jsonRecords)
        {
            var records = new List<Dictionary<string, object>>();

            try
            {
                // Check if the connection string is set
                if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString))
                {
                    return "Connection string has not been set yet. " +
                           "You have to create a database first. Lay down a ConnectionString " +
                           "component on the canvas.";
                }

                // Deserialize JSON strings to dictionaries
                foreach (var json in jsonRecords)
                {
                    var record = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    records.Add(record);
                }

                // Use the eventful GhClientApi to add records
                string result = _eventfulGhClientApi.AddDictionaryRecordBatch(tableName, records);

                return result;
            }
            catch (Exception ex)
            {
                return $"Error adding record(s): {ex.Message}";
            }
        }

        protected override System.Drawing.Bitmap Icon => null; // Add an icon if available

        public override Guid ComponentGuid => new Guid("4703fb0f-3b90-4d88-b533-c793b3ca5522");
    }
}
