using Daw.DB.Data.Interfaces;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;

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
                string result = ReadRecords(tableName);
                DA.SetData(0, result);
            }
        }

        private string ReadRecords(string tableName)
        {
            string connectionString = SQLiteConnectionFactory.ConnectionString;
            try
            {
                dynamic record = _ghClientApi.GetAllDictionaryRecords(tableName, connectionString);
                string dynamicObject = Convert.ToString(record);
                return dynamicObject;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("32240D21-C21D-4B03-9245-A8C6B87F3087");
    }
}