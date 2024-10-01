using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;

namespace Daw.DB.GH
{
    public class GhcReadRecord : GH_Component
    {
        private readonly IGhClientApi _ghClientApi;
        private readonly IDatabaseContext _databaseContext;

        public GhcReadRecord()
          : base("Read Record", "ReadRec",
              "Reads a record from the database given a table name and record ID",
            "Daw.DB", "READ")
        {
            _ghClientApi = ApiFactory.GetGhClientApi();
            _databaseContext = ApiFactory.GetDatabaseContext();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to read the record from", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Read", "R", "Boolean to trigger record read", GH_ParamAccess.item);
            pManager.AddIntegerParameter("RecordId", "RID", "Record ID to read from the table", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "Res", "Result of the database operation", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool readRecord = false;
            string tableName = null;
            int recordId = 0;

            if (!DA.GetData(0, ref tableName)) return;
            if (!DA.GetData(1, ref readRecord)) return;
            if (!DA.GetData(2, ref recordId)) return;

            if (readRecord)
            {
                string result = ReadRecord(tableName, recordId);
                DA.SetData(0, result);
            }
        }

        private string ReadRecord(string tableName, int recordId)
        {
            // Check if the connection string is set
            if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString))
            {
                return "Connection string has not been set yet. " +
                       "You have to create a database first. Use the Create Database component.";
            }

            // Print to console (optional)
            Console.WriteLine($"Reading record from table {tableName} with ID {recordId}");

            try
            {
                dynamic record = _ghClientApi.GetDictionaryRecordById(tableName, recordId);
                if (record == null)
                {
                    return $"No record found with ID {recordId} in table '{tableName}'.";
                }
                else
                {
                    // Convert the record to JSON string for better readability
                    string jsonRecord = Newtonsoft.Json.JsonConvert.SerializeObject(record);
                    return jsonRecord;
                }
            }
            catch (Exception ex)
            {
                return $"Error reading record: {ex.Message}";
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("BBF82622-2D63-4D52-892B-269C69C7F6D3");
    }
}
