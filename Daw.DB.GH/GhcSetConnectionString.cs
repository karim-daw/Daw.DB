using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;

namespace Daw.DB.GH
{
    public class GhcSetConnectionString : GH_Component
    {

        private readonly IDatabaseContext _databaseContext;
        private readonly IGhClientApi _ghClientApi;

        public GhcSetConnectionString()
          : base("Set Connection String", "SCS",
              "This component sets the connection string of the database, Make sure" +
                "to activate this component in order to allow all other componets to communicate" +
                "with the database",
              "Daw.DB", "CONFIGURATION")
        {

            _databaseContext = ApiFactory.GetDatabaseContext();
            _ghClientApi = ApiFactory.GetGhClientApi();
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Set Connection String", "SCS", "Set the connection string of the database", GH_ParamAccess.item);
            pManager.AddTextParameter("Connection String", "CS", "Connection string to the database", GH_ParamAccess.item);
        }


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
            string connectionString = null;

            // Retrieve input data
            if (!DA.GetData(0, ref setConnectionString)) return;
            if (!DA.GetData(1, ref connectionString)) return;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                DA.SetData(0, "Database connection string name is invalid.");
                return;
            }

            // check if the connection string is pointing to a valid database
            if (setConnectionString)
            {
                string result = SetConnectionString(connectionString);

                // Output the result of the database operation
                DA.SetData(0, result);
            }


        }

        private string SetConnectionString(string connectionString)
        {
            // check if the connection string is pointing to a valid database
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return "Database connection string is invalid.";
            }

            // quickly test the connection string by pingin the database
            var ghClientApi = ApiFactory.GetGhClientApi();
            bool dbExists = ghClientApi.Ping(connectionString);

            if (!dbExists)
            {
                return "Database does not exist. Please create a database first.";
            }

            // set the connection string
            _databaseContext.ConnectionString = connectionString;

            return "Connection string set to: " + connectionString;



        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("61CCC9CC-B7E1-4F8A-96DF-CD47C5E40BDE");
    }
}