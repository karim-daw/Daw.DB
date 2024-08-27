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

        public GhcReadRecord()
          : base("ReadRecord", "RR",
              "Read record from the database given a table name and id",
            "Daw.DB", "READ")
        {
            _ghClientApi = ApiFactory.GetGhClientApi();
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to insert the record into", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Read", "R", "Boolean to trigger record read", GH_ParamAccess.item);
            pManager.AddIntegerParameter("RecordId", "RID", "Record ID to read from the table", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
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

        private string ReadRecord(string tableName, int? recordId)
        {
            string connectionString = SQLiteConnectionFactory.ConnectionString;

            // print in console
            Console.WriteLine($"Reading record from table {tableName} with id {recordId}");

            try
            {
                dynamic record = _ghClientApi.GetDictionaryRecordById(tableName, recordId, connectionString);
                string dynamicObject = Convert.ToString(record);
                return dynamicObject;
            }
            catch (Exception ex)
            {
                return $"Error adding record: {ex.Message}";
            }
        }


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("BBF82622-2D63-4D52-892B-269C69C7F6D3");
    }
}