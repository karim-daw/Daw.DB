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


        public GhcReadRecords()
          : base("ReadRecord", "RR",
              "Read all records from the database given a table name",
            "Daw.DB", "READ")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to insert the record into", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Read", "R", "Boolean to trigger record read", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool readRecord = false;
            string tableName = null;

            if (!DA.GetData(0, ref tableName)) return;
            if (!DA.GetData(1, ref readRecord)) return;

            if (readRecord)
            {
                var resultBuilder = new System.Text.StringBuilder();
                foreach (var record in ReadRecords(tableName))
                {
                    resultBuilder.AppendLine(record);
                }
                DA.SetData(0, resultBuilder.ToString());
            }
        }


        /// <summary>
        /// Read all records from the table with the given name.
        /// This is a generator method that yields each record as a string.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private IEnumerable<string> ReadRecords(string tableName)
        {
            string connectionString = SQLiteConnectionFactory.ConnectionString;
            IEnumerable<dynamic> records = _ghClientApi.GetAllDictionaryRecords(tableName, connectionString);
            foreach (var record in records)
            {
                yield return Convert.ToString(record);
            }
        }


        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("32240D21-C21D-4B03-9245-A8C6B87F3087");
    }
}