using Daw.DB.Data;
using Daw.DB.Data.Interfaces;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Daw.DB.GH
{
    public class GhcCreateRecord : GH_Component
    {
        private readonly IGhClientApi _ghClientApi;

        public GhcCreateRecord()
          : base("CreateRecord", "CR",
            "Creates a record and inserts it into the database",
            "Daw.DB", "CREATE")
        {
            // Use the ApiFactory to get a pre-configured IClientApi instance to interact with the database
            _ghClientApi = ApiFactory.GetGhClientApi();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to insert the record into", GH_ParamAccess.item);
            pManager.AddBooleanParameter("AddRecord", "AR", "Boolean to trigger record addition", GH_ParamAccess.item);
            pManager.AddTextParameter("RecordKeys", "RK", "Record KEYS to add to the table", GH_ParamAccess.list);
            pManager.AddTextParameter("RecordValues", "RV", "Record VALUES to add to the table", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool addRecord = false;
            string tableName = null;
            List<string> recordKeys = new List<string>();
            List<string> recordValues = new List<string>();

            // Retrieve input data
            if (!DA.GetData(1, ref tableName)) return;
            if (!DA.GetData(2, ref addRecord)) return;
            if (!DA.GetDataList(3, recordKeys)) return;
            if (!DA.GetDataList(4, recordValues)) return;

            // Add a record to the table
            if (addRecord)
            {
                string result = CreateRecord(tableName, recordKeys, recordValues);
                DA.SetData(0, result);
            }
        }

        // Wrapper method
        private string CreateRecord(string tableName, List<string> recordKeys, List<string> recordValues)
        {
            var record = new Dictionary<string, object>();
            for (int i = 0; i < recordKeys.Count; i++)
            {
                record.Add(recordKeys[i], recordValues[i]);
            }


            string connectionString = SQLiteConnectionFactory.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return "Connection string has not been set yet. " +
                    "You have to create a database first. Lay down a ConnectionString " +
                    "component on the canvas, if a connection string is outputted";
            }
            try
            {
                _ghClientApi.AddDictionaryRecord(tableName, record, connectionString);
                return "Record added successfully.";
            }
            catch (Exception ex)
            {
                return $"Error adding record: {ex.Message}";
            }
        }

        protected override System.Drawing.Bitmap Icon => null; // Add an icon if available

        public override Guid ComponentGuid => new Guid("4703fb0f-3b90-4d88-b533-c793b3ca5522");
    }
}
