using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Documentation;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Daw.DB.GH {
    public class GhcGetTables : GH_Component {
        public override Guid ComponentGuid => new Guid("A4766E04-7B87-4880-8C75-6A37A3B574E8");

        private readonly IGhClientApi _ghClientApi;
        private readonly IDatabaseContext _databaseContext;

        public GhcGetTables()
            : base("Get Tables", "GetTables",
                "Retrieves the tables in the database",
                "Daw.DB", "GET") {

            _ghClientApi = ApiFactory.GetGhClientApi();
            _databaseContext = ApiFactory.GetDatabaseContext();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager) {

            pManager.AddBooleanParameter("Get Tables", "GT", "Boolean to trigger table retrieval", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
            pManager.AddTextParameter("Tables", "T", "Tables in the database", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            bool getTables = false;

            // Retrieve input data
            if (!DA.GetData(0, ref getTables))
                return;

            if (getTables) {
                IEnumerable<GH_Text> tables = GetTables();

                // Output the result of the database operation
                DA.SetDataList(0, tables);
            }
        }

        private IEnumerable<GH_Text> GetTables() {

            try {
                // Use the GhClientApi to create the connection
                var tables = _ghClientApi.GetTables();

                // Convert each table name to GH_Text from dynamic explicitly
                return tables.Select(t => GH_Text.Create((string)t));
            }
            catch (Exception ex) {
                return new List<GH_Text> { GH_Text.Create($"Error retrieving tables: {ex.Message}") };
            }

        }
    }
}
