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
          : base("CreateDatabase", "CD",
            "Creates and initializes a database",
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

            // Retrieve input data

            if (!DA.GetData(2, ref createTable)) return;
            if (!DA.GetData(3, ref tableName)) return;
            if (!DA.GetDataList(5, columnName)) return;
            if (!DA.GetDataList(6, columnType)) return;


            // Create the table
            if (createTable)
            {
                string result = CreateTable(tableName, columnName, columnType);
                DA.SetData(0, result);
            }
        }


        // Wrapper methods

        private string CreateTable(string tableName, List<string> columnName, List<string> columnType)
        {
            var columns = new Dictionary<string, string>();
            for (int i = 0; i < columnName.Count; i++)
            {
                columns.Add(columnName[i], columnType[i]);
            }
            string result = _ghClientApi.CreateTable(tableName, columns);
            return result;
        }




        protected override System.Drawing.Bitmap Icon => null; // Add an icon if available

        public override Guid ComponentGuid => new Guid("85e34b87-35a5-4dde-a671-2af6ea21b242");
    }
}
