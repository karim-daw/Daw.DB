using Daw.DB.Data;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;

namespace Daw.DB.GH
{
    public class GhcConnectionString : GH_Component
    {
        private readonly IDatabaseContext _databaseContext;

        public GhcConnectionString()
          : base("Connection String", "ConnStr",
              "Displays the full connection string when the database is connected for the first time. This will also persist to other components.",
            "Daw.DB", "Config")
        {
            // Obtain the IDatabaseContext instance from the ApiFactory
            _databaseContext = ApiFactory.GetDatabaseContext();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Set default value to false to prevent the component from running automatically
            pManager.AddBooleanParameter("ShowConnectionString", "SCS", "Boolean to trigger the display of the connection string", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("ConnectionString", "CS", "Connection string to the database", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool showConnectionString = false;
            if (!DA.GetData(0, ref showConnectionString)) return;

            // If toggle is false, return an empty string
            if (!showConnectionString)
            {
                DA.SetData(0, "");
                return;
            }

            // Use the IDatabaseContext to get the connection string
            if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString))
            {
                DA.SetData(0, "Connection string not set.");
            }
            else
            {
                DA.SetData(0, _databaseContext.ConnectionString);
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("49AD151D-D3E4-457F-90B2-2F0CDC126874");
    }
}
