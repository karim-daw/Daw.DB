using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Daw.DB.GH
{
    public class GhcCreateRecordsBatch : GH_Component
    {


        private readonly IEventfulGhClientApi _eventfulGhClientApi;

        /// <summary>
        /// Initializes a new instance of the GhcCreateRecordsBatch class.
        /// </summary>
        public GhcCreateRecordsBatch()
          : base("GhcCreateRecordsBatch", "CRB",
            "Create a batch of records and insert all it into the database",
            "Daw.DB", "CREATE")
        {
            // Use the ApiFactory to get a pre-configured IClientApi instance to interact with the database
            _eventfulGhClientApi = ApiFactory.GetEventDrivenGhClientApi();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to insert the record into", GH_ParamAccess.item);
            pManager.AddBooleanParameter("AddRecord", "AR", "Boolean to trigger record addition", GH_ParamAccess.item);
            pManager.AddTextParameter("RecordKeys", "RK", "Record KEYS to add to the table", GH_ParamAccess.list);
            pManager.AddTextParameter("RecordValues", "RV", "Record VALUES to add to the table", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool addRecord = false;
            string tableName = null;
            List<string> recordKeys = new List<string>();
            List<object> recordValues = new List<object>();



            // Retrieve input data
            if (!DA.GetData(0, ref tableName)) return;
            if (!DA.GetData(1, ref addRecord)) return;
            if (!DA.GetDataList(2, recordKeys)) return;
            if (!DA.GetDataList(3, recordValues)) return;


            // Add a record to the table
            if (addRecord)
            {
                string result = CreateRecords(tableName, recordKeys, recordValues);
                DA.SetData(0, result);
            }
        }

        // say if we have a table named "Buildings" with columns "Name", "Height", "Floors", "Location"
        // and we want to add a record with values "Empire State Building", "443.2", "102", "New York"
        // the recordKeys would be ["Name", "Height", "Floors", "Location"]
        // the recordValues would be ["Empire State Building", "443.2", "102", "New York"]
        // and i had a list of 10 recordValues, i would have 10 records added to the table

        // [Empire State Building, 443.2, 102, New York]
        // [Empire State Building, 443.2, 102, New York]
        // [Empire State Building, 443.2, 102, New York]
        // [Empire State Building, 443.2, 102, New York]
        // [Empire State Building, 443.2, 102, New York]


        // Wrapper method
        private string CreateRecords(string tableName, List<string> recordKeys, List<object> recordValues)
        {


            string connectionString = SQLiteConnectionFactory.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return "Connection string has not been set yet. " +
                    "You have to create a database first. Lay down a ConnectionString " +
                    "component on the canvas, if a connection string is outputted";
            }
            // create a list of dictionaries
            var recordValuesList = new List<Dictionary<string, object>>();
            foreach (string key in recordKeys)
            {
                var record = new Dictionary<string, object>();
                for (int i = 0; i < recordKeys.Count; i++)
                {
                    record.Add(recordKeys[i], recordValues[i]);
                }
                recordValuesList.Add(record);
            }
            try
            {
                // _ghClientApi.AddDictionaryRecord(tableName, record, connectionString);
                _eventfulGhClientApi.AddDictionaryRecordBatch(tableName, recordValuesList, connectionString);
                return "Record added successfully.";
            }
            catch (Exception ex)
            {
                return $"Error adding record: {ex.Message}";
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("42B2FCED-41F4-4112-B1FE-340A6ACA5775"); }
        }
    }
}