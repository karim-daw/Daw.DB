using Daw.DB.Data;
using Daw.DB.Data.Interfaces;
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
            "Creates a record and puts it in the database",
            "Category", "Subcategory")
        {
            // Use the ApiFactory to get a pre-configured IClientApi instance to interact with the database
            _ghClientApi = ApiFactory.GetGhClientApi();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Database Name", "DB", "Name of the database to create", GH_ParamAccess.item);
            pManager.AddTextParameter("Table Name", "T", "Name of the table to create", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Add Record", "AR", "Boolean to trigger record addition", GH_ParamAccess.item);
            pManager.AddTextParameter("RecordKeys", "RK", "Record KEYS to add to the table", GH_ParamAccess.list);
            pManager.AddTextParameter("RecordValues", "RV", "Record VALUES to add to the table", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool createRecord = false;
            string databaseName = null;
            string tableName = null;
            List<string> recordKeys = new List<string>();
            List<string> recordValues = new List<string>();

            // Retrieve input data
            if (!DA.GetData(1, ref databaseName)) return;
            if (!DA.GetData(3, ref tableName)) return;
            if (!DA.GetData(4, ref createRecord)) return;
            if (!DA.GetDataList(5, recordKeys)) return;
            if (!DA.GetDataList(6, recordValues)) return;



            // Add a record to the table
            if (createRecord)
            {
                string result = CreateRecord(tableName, recordKeys, recordValues);
                DA.SetData(0, result);
            }
        }

        // Wrapper methods

        private string CreateRecord(string tableName, List<string> recordKeys, List<string> recordValues)
        {
            var record = new Dictionary<string, object>();
            for (int i = 0; i < recordKeys.Count; i++)
            {
                record.Add(recordKeys[i], recordValues[i]);
            }
            string result = _ghClientApi.AddDictionaryRecord(tableName, record);
            return result;
        }




        protected override System.Drawing.Bitmap Icon => null; // Add an icon if available

        public override Guid ComponentGuid => new Guid("85e34b87-35a5-4dde-a671-2af6ea21b242");
    }
}
