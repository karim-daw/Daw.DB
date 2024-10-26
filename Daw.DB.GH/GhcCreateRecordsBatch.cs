using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Daw.DB.GH {
    public class GhcCreateRecordsBatch : GH_Component {
        private readonly IEventfulGhClientApi _eventfulGhClientApi;
        private readonly IDatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of the GhcCreateRecordsBatch class.
        /// </summary>
        public GhcCreateRecordsBatch()
          : base("Create Records Batch", "CreateBatch",
            "Creates a batch of records and inserts them into the database",
            "Daw.DB", "CREATE") {
            // Use the ApiFactory to get pre-configured instances
            _eventfulGhClientApi = ApiFactory.GetEventDrivenGhClientApi();
            _databaseContext = ApiFactory.GetDatabaseContext();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to insert the records into", GH_ParamAccess.item);
            pManager.AddBooleanParameter("AddRecords", "AR", "Boolean to trigger record addition", GH_ParamAccess.item);
            pManager.AddTextParameter("RecordKeys", "RK", "List of keys (column names) for the records", GH_ParamAccess.list);
            pManager.AddTextParameter("RecordValues", "RV", "Flattened list of record values", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            bool addRecords = false;
            string tableName = null;
            List<string> recordKeys = new List<string>();
            List<string> recordValues = new List<string>();

            // Retrieve input data
            if (!DA.GetData(0, ref tableName))
                return;
            if (!DA.GetData(1, ref addRecords))
                return;
            if (!DA.GetDataList(2, recordKeys))
                return;
            if (!DA.GetDataList(3, recordValues))
                return;

            // Add records to the table
            if (addRecords) {
                string result = CreateRecords(tableName, recordKeys, recordValues);
                DA.SetData(0, result);
            }
        }

        // Wrapper method
        private string CreateRecords(string tableName, List<string> recordKeys, List<string> recordValues) {
            try {
                // Check if the connection string is set
                if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString)) {
                    return "Connection string has not been set yet. " +
                           "You have to create a database first. Use the Create Database component.";
                }

                if (recordKeys == null || recordKeys.Count == 0) {
                    return "Record keys (column names) are required.";
                }

                if (recordValues == null || recordValues.Count == 0) {
                    return "Record values are required.";
                }

                // Calculate the number of records
                int numRecords = recordValues.Count / recordKeys.Count;

                if (recordValues.Count % recordKeys.Count != 0) {
                    return "The number of record values must be a multiple of the number of record keys.";
                }

                // Build the list of records
                var recordList = new List<Dictionary<string, object>>();

                for (int i = 0; i < numRecords; i++) {
                    var record = new Dictionary<string, object>();
                    for (int j = 0; j < recordKeys.Count; j++) {
                        int valueIndex = i * recordKeys.Count + j;
                        record.Add(recordKeys[j], recordValues[valueIndex]);
                    }
                    recordList.Add(record);
                }

                // Use the eventful GhClientApi to add records
                string result = _eventfulGhClientApi.AddDictionaryRecordBatch(tableName, recordList);

                return result;
            }
            catch (Exception ex) {
                return $"Error adding records: {ex.Message}";
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null; // Add an icon if available

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("42B2FCED-41F4-4112-B1FE-340A6ACA5775");
    }
}
