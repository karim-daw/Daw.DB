using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;

namespace Daw.DB.GH
{
    public class GhcDeleteRecord : GH_Component
    {

        private readonly IEventfulGhClientApi _eventfulGhClientApi;
        private readonly IDatabaseContext _databaseContext;

        public GhcDeleteRecord()
          : base("Delete Record", "DR",
              "Deletes a record within an exisiting table in the database given a record id",
              "Daw.DB", "DELETE")
        {
            _eventfulGhClientApi = ApiFactory.GetEventDrivenGhClientApi();
            _databaseContext = ApiFactory.GetDatabaseContext();
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("DeleteRecord", "DR", "Boolean to trigger record deletion", GH_ParamAccess.item);
            pManager.AddTextParameter("TableName", "TN", "Name of the table to delete the record from", GH_ParamAccess.item);
            pManager.AddIntegerParameter("RecordId", "ID", "Id of the record to delete", GH_ParamAccess.item);
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
            bool deleteRecord = false;
            string tableName = null;
            int recordId = 0;

            if (!DA.GetData(0, ref deleteRecord)) return;
            if (!DA.GetData(1, ref tableName)) return;
            if (!DA.GetData(2, ref recordId)) return;

            if (deleteRecord)
            {
                string result = DeleteRecord(tableName, recordId);
                DA.SetData(0, result);
            }
        }

        private string DeleteRecord(string tableName, int recordId)
        {
            if (string.IsNullOrEmpty(_databaseContext.ConnectionString))
            {
                return "Database connection string is empty";
            }

            if (string.IsNullOrEmpty(tableName))
            {
                return "Table name cannot be empty";
            }

            if (recordId == 0)
            {
                return "Record id cannot be 0";
            }

            try
            {
                _eventfulGhClientApi.DeleteRecord(tableName, recordId);
                return "Record deleted successfully";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("E692E121-39DC-4CCF-A981-46C9156BFE10");
    }
}