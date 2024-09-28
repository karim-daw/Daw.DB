using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;

namespace Daw.DB.GH
{
    public class GhcDeleteTable : GH_Component
    {

        private readonly IGhClientApi _ghClientApi;

        public GhcDeleteTable()
          : base("DeleteTable", "DT",
              "Deletes table from database given a table name",
              "Daw.DB", "DELETE")
        {
            _ghClientApi = ApiFactory.GetGhClientApi();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to delete", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Delete", "D", "Boolean to trigger table delete", GH_ParamAccess.item);
        }


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
            bool deleteTable = false;
            string tableName = null;

            if (!DA.GetData(0, ref tableName)) return;
            if (!DA.GetData(1, ref deleteTable)) return;

            if (deleteTable)
            {
                string result = DeleteTable(tableName);
                DA.SetData(0, result);
            }
        }

        private string DeleteTable(string tableName)
        {
            string connectionString = SQLiteConnectionFactory.ConnectionString;

            // print in console
            Console.WriteLine($"Deleting table {tableName}");

            try
            {
                _ghClientApi.DeleteTable(tableName, connectionString);
                return $"Table {tableName} deleted successfully";
            }
            catch (Exception e)
            {
                return $"Error deleting table {tableName}: {e.Message}";
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("26771DD7-4EEC-42B2-80C6-45C4B1EF517D");
    }
}