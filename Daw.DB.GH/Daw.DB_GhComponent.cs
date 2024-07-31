using Daw.DB.Data;
using Daw.DB.Data.Interfaces;
using Grasshopper.Kernel;
using System;

namespace Daw.DB.GH
{
    public class DB_GhComponent : GH_Component
    {
        private readonly IClientApi _clientApi;

        public DB_GhComponent()
          : base("DB_GhComponent", "DB",
            "Creates and initializes a database",
            "Category", "Subcategory")
        {
            // Use the ApiFactory to get a pre-configured IClientApi instance
            _clientApi = ApiFactory.GetClientApi();  // Notice no database path is passed here
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Create", "C", "Boolean to trigger database creation", GH_ParamAccess.item);
            pManager.AddTextParameter("Database Name", "DB", "Name of the database to create", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database creation", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool createDatabase = false;
            string databaseName = null;

            // Retrieve input data
            if (!DA.GetData(0, ref createDatabase)) return;
            if (!DA.GetData(1, ref databaseName)) return;

            // Check if the user triggered the database creation
            if (createDatabase && !string.IsNullOrEmpty(databaseName))
            {
                try
                {
                    // Explicitly create the database using the user-provided name
                    _clientApi.InitializeDatabase(databaseName);
                    DA.SetData(0, $"Database '{databaseName}' created successfully.");
                }
                catch (Exception ex)
                {
                    // Handle and report any errors that occurred during database creation
                    DA.SetData(0, $"Error: {ex.Message}");
                }
            }
            else
            {
                DA.SetData(0, "Database creation not triggered or invalid database name.");
            }
        }

        protected override System.Drawing.Bitmap Icon => null; // Add an icon if available

        public override Guid ComponentGuid => new Guid("85e34b87-35a5-4dde-a671-2af6ea21b242");
    }
}
