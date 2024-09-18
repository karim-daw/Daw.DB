using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;

namespace Daw.DB.GH
{
    public class GhcConnectionString : GH_Component
    {

        public GhcConnectionString()
          : base("ConnectionString", "CS",
              "Shows the full connection string when Database is connected for the first time, will also persist to other components",
            "Daw.DB", "Config")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // set toggle to false to prevent the component from running
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

            // if toggle is false, return an empty string
            if (!showConnectionString)
            {
                DA.SetData(0, "");
                return;
            }

            // if the connection string is not set, return an empty string
            if (string.IsNullOrWhiteSpace(SQLiteConnectionFactory.ConnectionString))
            {
                DA.SetData(0, "Connection string not set.");
            }
            else
            {
                DA.SetData(0, SQLiteConnectionFactory.ConnectionString);
            }
        }


        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("49AD151D-D3E4-457F-90B2-2F0CDC126874");
    }
}