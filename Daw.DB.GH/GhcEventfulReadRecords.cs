using Daw.DB.Data;
using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Daw.DB.GH
{
    public class GhcEventfulReadRecords : GH_Component
    {
        private readonly IEventfulGhClientApi _eventDrivenGhClientApi;
        private bool _eventTriggered;


        public GhcEventfulReadRecords()
          : base("ReadRecordWithEvents", "RRW",
              "Read all records from the database given a table name, and automatically updates when the table changes if live listening is enabled",
              "Daw.DB", "READ")
        {
            _eventDrivenGhClientApi = ApiFactory.GetEventDrivenGhClientApi();
            _eventTriggered = false;
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TableName", "TN", "Name of the table to read the records from", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Read", "R", "Boolean to trigger record read", GH_ParamAccess.item);
            pManager.AddBooleanParameter("LiveListen", "LL", "Enable or disable live listening for table changes", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of the database operation", GH_ParamAccess.item);
            pManager.AddTextParameter("Records", "R", "Records from the table", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool readRecord = false;
            string tableName = null;
            bool liveListen = false;

            if (!DA.GetData(0, ref tableName)) return;
            if (!DA.GetData(1, ref readRecord)) return;
            if (!DA.GetData(2, ref liveListen)) return;

            if (liveListen)
            {
                SubscribeToTableChanges(tableName);
            }

            // output all records from the table
            List<string> records = new List<string>();

            if (readRecord || _eventTriggered)
            {
                _eventTriggered = false; // Reset event flag
                var resultBuilder = new System.Text.StringBuilder();
                foreach (var record in ReadRecords(tableName))
                {
                    resultBuilder.AppendLine(record);
                    records.Add(record);
                }
                DA.SetData(0, resultBuilder.ToString());
            }

            DA.SetDataList(1, records);
        }

        /// <summary>
        /// Subscribe to table changes and trigger component update.
        /// </summary>
        /// <param name="tableName"></param>
        private void SubscribeToTableChanges(string tableName)
        {
            _eventDrivenGhClientApi.SubscribeToTableChanges((sender, args) =>
            {
                if (args.TableName == tableName)
                {
                    _eventTriggered = true;
                    ExpireSolution(true); // Trigger Grasshopper to re-solve the component
                }
            });
        }

        /// <summary>
        /// Read all records from the table with the given name.
        /// This is a generator method that yields each record as a string.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private IEnumerable<string> ReadRecords(string tableName)
        {
            string connectionString = SQLiteConnectionFactory.ConnectionString;
            IEnumerable<dynamic> records = _eventDrivenGhClientApi.GetAllDictionaryRecords(tableName, connectionString);
            foreach (var record in records)
            {
                yield return Convert.ToString(record);
            }
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("C53B71A4-2B1B-4C50-9ED6-34975CA6B5D7");
    }
}