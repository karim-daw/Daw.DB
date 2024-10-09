using Daw.DB.Data;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.IO;
using System.Windows.Forms;

namespace Daw.DB.GH
{
    public class GhcSetConnectionStringByFileName : GH_Component
    {

        private readonly IDatabaseContext _databaseContext;

        public GhcSetConnectionStringByFileName()
          : base("Set Connection String By Filename", "SCSBFN",
              "This component sets the connection string of a database by file name. The database connection string will" +
                "be created in the same folder as where the gh file is saved. (SO MAKE SURE TO SAVE) " +
                "Make sure to activate this component in order to allow all other components to communicate to your chosen database",
              "Daw.DB", "CONFIGURATION")
        {

            _databaseContext = ApiFactory.GetDatabaseContext();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Set Connection String", "SCS", "Set the connection string of the database", GH_ParamAccess.item);
            pManager.AddTextParameter("Database File Name", "DBFN", "Name of the database file to create", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
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
            bool setConnectionString = false;
            string databaseFileName = null;

            // Retrieve input data
            if (!DA.GetData(0, ref setConnectionString)) return;
            if (!DA.GetData(1, ref databaseFileName)) return;

            if (string.IsNullOrWhiteSpace(databaseFileName))
            {
                DA.SetData(0, "Database file name is invalid.");
                return;
            }

            if (setConnectionString)
            {
                string result = SetConnectionStringByFileName(databaseFileName);

                // Output the result of the database operation
                DA.SetData(0, result);
            }
            else
            {
                _databaseContext.ConnectionString = null;

                DA.SetData(0, "Connection string has been reset to null.");
            }
        }

        private string SetConnectionStringByFileName(string databaseName)
        {

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return "Database name is invalid.";
            }


            string databasePath = GetDatabasePath(databaseName);
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                return "Failed to get the database path.";
            }

            // Set the connection string in the database context
            _databaseContext.ConnectionString = $"Data Source={databasePath};Version=3;";

            return $"Connection string set to {databasePath}";
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

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("B47122A2-235E-4581-A3B6-F4052F569EF5");
    }
}