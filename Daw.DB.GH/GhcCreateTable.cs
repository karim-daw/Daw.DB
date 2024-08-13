using Daw.DB.Data;
using Daw.DB.Data.Interfaces;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Daw.DB.GH
{
    public class GhcCreateTable : GH_Component
    {
        private readonly IGhClientApi _ghClientApi;

        public GhcCreateTable()
          : base("CreateTable", "CT",
            "Creates a table in the database",
            "Category", "Subcategory")
        {
            // Use the ApiFactory to get a pre-configured IClientApi instance to interact with the database
            _ghClientApi = ApiFactory.GetGhClientApi();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("CreateTable", "CT", "Boolean to trigger table creation", GH_ParamAccess.item);
            pManager.AddTextParameter("TableName", "T", "Name of the table to create", GH_ParamAccess.item);
            pManager.AddTextParameter("ColumnName", "CN", "Column names to be used for the table", GH_ParamAccess.list);
            pManager.AddTextParameter("ColumnType", "CT",
                "Column type (such as TEXT, TEXT NOT NULL, TEXT NOT NULL UNIQUE, INTEGER, INTEGER PRIMARY KEY AUTOINCREMENT, REAL, etc.). " +
                "This allows you to dynamically define tables with various column names and types.", GH_ParamAccess.list);
            pManager.AddTextParameter("DatabasePath", "DBP", "Path to the database file", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool createTable = false;
            string tableName = null;
            List<string> columnName = new List<string>();
            List<string> columnType = new List<string>();
            string databasePath = null;

            // Retrieve input data
            if (!DA.GetData(0, ref createTable)) return;
            if (!DA.GetData(1, ref tableName)) return;
            if (!DA.GetDataList(2, columnName)) return;
            if (!DA.GetDataList(3, columnType)) return;
            if (!DA.GetData(4, ref databasePath)) return;

            if (createTable)
            {
                string result = CreateTable(tableName, columnName, columnType, databasePath);
                DA.SetData(0, result);
            }
        }

        // Wrapper method
        private string CreateTable(string tableName, List<string> columnNames, List<string> columnTypes, string databasePath)
        {
            var columns = new Dictionary<string, string>();
            for (int i = 0; i < columnNames.Count; i++)
            {
                columns.Add(columnNames[i], columnTypes[i]);
            }

            string connectionString = $"Data Source={databasePath};Version=3;";
            try
            {
                _ghClientApi.CreateTable(tableName, columns, connectionString);
            }
            catch (Exception ex)
            {
                return $"Error creating table in GH component: {ex.Message}";
            }
            return "Table created successfully.";

        }

        protected override System.Drawing.Bitmap Icon => null; // Add an icon if available

        public override Guid ComponentGuid => new Guid("0e5c2e9d-9489-4872-ae9b-5d8349ba2459");
    }
}
