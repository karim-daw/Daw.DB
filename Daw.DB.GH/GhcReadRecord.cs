using Daw.DB.Data;
using Daw.DB.Data.Interfaces;
using Grasshopper.Kernel;
using System;

namespace Daw.DB.GH
{
    public class GhcReadRecord : GH_Component
    {
        private readonly IGhClientApi _ghClientApi;

        public GhcReadRecord()
          : base("ReadRecord", "RR",
              "Reads a record of several records from the database",
            "Daw.DB", "READ")
        {
            _ghClientApi = ApiFactory.GetGhClientApi();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("DatabasePath", "DBP", "Path to the database file", GH_ParamAccess.item);
            pManager.AddTextParameter("TableName", "TN", "Name of the table to insert the record into", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Read", "R", "Boolean to trigger record read", GH_ParamAccess.item);
            pManager.AddIntegerParameter("RecordId", "RID", "Record ID to read from the table", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool readRecord = false;
            string databasePath = null;
            string tableName = null;
            int recordId = 0;

            if (!DA.GetData(0, ref databasePath)) return;
            if (!DA.GetData(1, ref tableName)) return;
            if (!DA.GetData(2, ref readRecord)) return;
            if (!DA.GetData(3, ref recordId)) return;

            if (readRecord)
            {
                string result = ReadRecord(databasePath, tableName, recordId);
                DA.SetData(0, result);
            }
        }

        private string ReadRecord(string databasePath, string tableName, int recordId)
        {

            string connectionString = $"Data Source={databasePath};Version=3;";
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