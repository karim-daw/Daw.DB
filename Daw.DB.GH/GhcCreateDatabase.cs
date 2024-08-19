using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.IO;
using System.Windows.Forms;

namespace Daw.DB.GH
{
    public class GhcCreateDatabase : GH_Component
    {
        private readonly IGhClientApi _ghClientApi;

        // TODO: update the component name and description and categories
        public GhcCreateDatabase()
          : base("CreateDatabase", "CD",
            "Creates and initializes a database",
            "Daw.DB", "CREATE")
        {
            // Use the ApiFactory to get a pre-configured IClientApi instance to interact with the database
            _ghClientApi = ApiFactory.GetGhClientApi();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Create", "C", "Boolean to trigger database creation", GH_ParamAccess.item);
            pManager.AddTextParameter("Database Name", "DB", "Name of the database to create", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool createDatabase = false;
            string databaseName = null;

            // Retrieve input data
            if (!DA.GetData(0, ref createDatabase)) return;
            if (!DA.GetData(1, ref databaseName)) return;

            string databasePath = GetDatabasePath(databaseName);
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                DA.SetData(0, "Database name is invalid.");
                return;
            }

            if (createDatabase)
            {
                string result = CreateAndInitializeDatabase(databasePath);

                // Output the result of the database operation
                DA.SetData(0, result);
            }
        }

        // Wrapper method
        private string CreateAndInitializeDatabase(string databasePath)
        {
            try
            {
                // Check if the database file already exists
                if (File.Exists(databasePath))
                {
                    return $"Database already exists at {databasePath}, delete it if you want to overwrite the data";
                }

                // Initialize the database using the connection string
                string connectionString = $"Data Source={databasePath};Version=3;";

                _ghClientApi.CreateConnection(connectionString);

                // save out the connection string for later use
                SQLiteConnectionFactory.ConnectionString = connectionString;

                return $"Database created and initialized at {databasePath}";
            }
            catch (Exception ex)
            {
                return $"Error creating and initializing database: {ex.Message}";
            }
        }

        // Helper method
        private string GetDatabasePath(string databaseName)
        {
            // Access the current Grasshopper document
            var ghDoc = Grasshopper.Instances.ActiveCanvas.Document;

            // Check if the document is saved
            if (ghDoc.IsFilePathDefined)
            {
                // Get the directory of the GH file
                string ghDirectory = Path.GetDirectoryName(ghDoc.FilePath);
                return Path.Combine(ghDirectory, $"{databaseName}.db");
            }
            else
            {
                // Prompt the user to save the Grasshopper file
                MessageBox.Show("Please save the Grasshopper file first.", "Save Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        protected override System.Drawing.Bitmap Icon => null; // Add an icon if available

        public override Guid ComponentGuid => new Guid("85e34b87-35a5-4dde-a671-2af6ea21b242");
    }
}
