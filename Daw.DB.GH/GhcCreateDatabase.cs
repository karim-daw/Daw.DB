using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;

namespace Daw.DB.GH {
    public class GhcCreateDatabase : GH_Component {
        private readonly IGhClientApi _ghClientApi;
        private readonly IDatabaseContext _databaseContext;

        public GhcCreateDatabase()
            : base("Create Database", "CreateDB",
                "Creates and initializes a database",
                "Daw.DB", "CREATE") {
            // Use the ApiFactory to get pre-configured instances
            _ghClientApi = ApiFactory.GetGhClientApi();
            _databaseContext = ApiFactory.GetDatabaseContext();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBooleanParameter("Create", "C", "Boolean to trigger database creation", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            bool createDatabase = false;

            // Retrieve input data
            if (!DA.GetData(0, ref createDatabase))
                return;


            if (createDatabase) {
                string result = CreateAndInitializeDatabase();

                // Output the result of the database operation
                DA.SetData(0, result);
            }
        }

        // Wrapper method
        private string CreateAndInitializeDatabase() {

            try {
                // Use the GhClientApi to create the connection
                string creationResult = _ghClientApi.CreateConnection();

                if (!creationResult.Contains("successfully")) {
                    return creationResult;
                }

                return $"Database created and initialized at {_databaseContext.ConnectionString}";
            }
            catch (Exception ex) {
                return $"Error creating and initializing database: {ex.Message}";
            }
        }


        protected override System.Drawing.Bitmap Icon => null; // Add an icon if available

        public override Guid ComponentGuid => new Guid("85e34b87-35a5-4dde-a671-2af6ea21b242");
    }
}
