using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Daw.DB.GH
{
    public class GhcUpdateRecord : GH_Component
    {

        private readonly IEventfulGhClientApi _eventfulGhClientApi;
        private readonly IDatabaseContext _databaseContext;

        public GhcUpdateRecord()
          : base("Update Record", "UR",
              "Updates a record within an existing table in the database given a valid record and id",
              "Category", "Subcategory")
        {
            _eventfulGhClientApi = ApiFactory.GetEventDrivenGhClientApi();
            _databaseContext = ApiFactory.GetDatabaseContext();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("UpdateRecord", "UR", "Boolean to trigger record update", GH_ParamAccess.item);
            pManager.AddTextParameter("TableName", "TN", "Name of the table to insert the record into", GH_ParamAccess.item);
            pManager.AddIntegerParameter("RecordId", "ID", "Id of the record to update", GH_ParamAccess.item);
            pManager.AddTextParameter("JsonRecord", "JR", "JSON representation of record key-value pair", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool updateRecord = false;
            string tableName = null;
            int recordId = 0;
            string jsonRecord = null;

            if (!DA.GetData(0, ref updateRecord)) return;
            if (!DA.GetData(1, ref tableName)) return;
            if (!DA.GetData(2, ref recordId)) return;
            if (!DA.GetData(3, ref jsonRecord)) return;


            if (updateRecord)
            {
                string result = UpdateRecord(tableName, jsonRecord, recordId);
                DA.SetData(0, result);
            }
        }

        private string UpdateRecord(string tableName, string jsonRecord, int recordId)
        {
            // check if connection string is set
            if (string.IsNullOrEmpty(_databaseContext.ConnectionString))
            {
                return "Connection string is not set";
            }

            // check if table value is valid
            if (string.IsNullOrEmpty(tableName))
            {
                return "Table name is not set";
            }

            // check if record id is valid
            if (recordId <= 0)
            {
                return "Record id is not set";
            }

            if (jsonRecord == null)
            {
                return "Record is not set";
            }

            // check if json record is valid
            if (string.IsNullOrEmpty(jsonRecord))
            {
                return "Record is not set";
            }

            // check if json record is a valid json
            if (!jsonRecord.StartsWith("{"))
            {
                return "Record is not a valid json";
            }


            // DO THE ACTUAL WORK
            try
            {
                var record = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonRecord);
                _eventfulGhClientApi.UpdateDictionaryRecord(tableName, recordId, record);
                return "Following record has been updated: " + jsonRecord;
            }
            catch (Exception ex)
            {
                return $"Error updating record: {ex.Message}";
            }


        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("4E2E006A-C6A1-40F8-94D9-B4590E29E30E");
    }
}