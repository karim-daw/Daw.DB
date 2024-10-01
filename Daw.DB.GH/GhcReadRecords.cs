using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Daw.DB.GH
{
    public class GhcReadRecords : GH_Component
    {
        private readonly IGhClientApi _ghClientApi;
        private readonly IDatabaseContext _databaseContext;

        public GhcReadRecords()
          : base("Read Records", "ReadRecs",
              "Reads all records from the database given a table name",
            "Daw.DB", "READ")
        {
            // Use the ApiFactory to get pre-configured instances
            _ghClientApi = ApiFactory.GetGhClientApi();
            _databaseContext = ApiFactory.GetDatabaseContext();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to read records from", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Read", "R", "Boolean to trigger record read", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "Res", "Result of the database operation", GH_ParamAccess.item);
            pManager.AddTextParameter("Records", "Recs", "Records from the table", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool readRecord = false;
            string tableName = null;

            if (!DA.GetData(0, ref tableName)) return;
            if (!DA.GetData(1, ref readRecord)) return;

            // Output all records from the table
            List<string> records = new List<string>();
            string resultMessage = string.Empty;

            if (readRecord)
            {
                if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString))
                {
                    resultMessage = "Connection string has not been set yet. " +
                                    "You have to create a database first. Use the Create Database component.";
                    DA.SetData(0, resultMessage);
                    return;
                }

                try
                {
                    var resultBuilder = new System.Text.StringBuilder();
                    foreach (var record in ReadRecords(tableName))
                    {
                        resultBuilder.AppendLine(record);
                        records.Add(record);
                    }
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
                DA.SetData(0, "Waiting for read command...");
                DA.SetDataList(1, records);
            }
        }

        /// <summary>
        /// Read all records from the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private IEnumerable<string> ReadRecords(string tableName)
        {
            IEnumerable<dynamic> records = _ghClientApi.GetAllDictionaryRecords(tableName);
            foreach (var record in records)
            {
                // Convert the record to JSON string for better readability
                string jsonRecord = Newtonsoft.Json.JsonConvert.SerializeObject(record);
                yield return jsonRecord;
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("32240D21-C21D-4B03-9245-A8C6B87F3087");
    }
}
