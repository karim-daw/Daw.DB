using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Daw.DB.GH {
    public class GhcCreateTable : GH_Component {
        private readonly IGhClientApi _ghClientApi;
        private readonly IDatabaseContext _databaseContext;

        public GhcCreateTable()
          : base("Create Table", "CreateTbl",
            "Creates a table in the database",
            "Daw.DB", "CREATE") {
            // Use the ApiFactory to get pre-configured instances
            _ghClientApi = ApiFactory.GetGhClientApi();
            _databaseContext = ApiFactory.GetDatabaseContext();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBooleanParameter("CreateTable", "CT", "Boolean to trigger table creation", GH_ParamAccess.item);
            pManager.AddTextParameter("TableName", "T", "Name of the table to create", GH_ParamAccess.item);
            pManager.AddTextParameter("ColumnName", "CN", "Column names to be used for the table", GH_ParamAccess.list);
            pManager.AddTextParameter("ColumnType", "CT",
                "Column type (such as TEXT, TEXT NOT NULL, TEXT NOT NULL UNIQUE, INTEGER, INTEGER PRIMARY KEY AUTOINCREMENT, REAL, etc.). " +
                "This allows you to dynamically define tables with various column names and types.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            bool createTable = false;
            string tableName = null;
            List<string> columnName = new List<string>();
            List<string> columnType = new List<string>();

            // Retrieve input data
            if (!DA.GetData(0, ref createTable))
                return;
            if (!DA.GetData(1, ref tableName))
                return;
            if (!DA.GetDataList(2, columnName))
                return;
            if (!DA.GetDataList(3, columnType))
                return;

            if (createTable) {
                string result = CreateTable(tableName, columnName, columnType);
                DA.SetData(0, result);
            }
        }

        // Wrapper method
        private string CreateTable(string tableName, List<string> columnNames, List<string> columnTypes) {
            if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString)) {
                return "Connection string has not been set yet. " +
                       "You have to create a database first. Lay down a ConnectionString " +
                       "component on the canvas.";
            }

            if (columnNames.Count != columnTypes.Count) {
                return "The number of column names and column types must be the same.";
            }

            var columns = new Dictionary<string, string>();
            for (int i = 0; i < columnNames.Count; i++) {
                columns.Add(columnNames[i], columnTypes[i]);
            }

            try {
                string result = _ghClientApi.CreateTable(tableName, columns);
                return result;
            }
            catch (Exception ex) {
                return $"Error creating table in GH component: {ex.Message}";
            }
        }

        protected override System.Drawing.Bitmap Icon => null; // Add an icon if available

        public override Guid ComponentGuid => new Guid("0e5c2e9d-9489-4872-ae9b-5d8349ba2459");
    }
}
