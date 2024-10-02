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
        private readonly IDatabaseContext _dataBaseContext;
        private bool _eventTriggered;


        public GhcEventfulReadRecords()
          : base("ReadRecordWithEvents", "RRW",
              "Read all records from the database given a table name, and automatically updates when the table changes if live listening is enabled",
              "Daw.DB", "READ")
        {
            _eventDrivenGhClientApi = ApiFactory.GetEventDrivenGhClientApi();
            _dataBaseContext = ApiFactory.GetDatabaseContext();
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
            pManager.AddTextParameter("Keys", "K", "Keys from the table", GH_ParamAccess.list);
            pManager.AddTextParameter("Values", "V", "Values from the table", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool readRecord = false;
            string tableName = null;
            bool liveListen = false;

            // Get input data
            if (!DA.GetData(0, ref tableName)) return;
            if (!DA.GetData(1, ref readRecord)) return;
            if (!DA.GetData(2, ref liveListen)) return;

            // Manage live listening
            if (liveListen)
            {
                UnsubscribeFromTableChanges(tableName);
                SubscribeToTableChanges(tableName);
            }
            else
            {
                UnsubscribeFromTableChanges(tableName);
            }

            List<Grasshopper.Kernel.Types.GH_String> allKeys = new List<Grasshopper.Kernel.Types.GH_String>();
            List<object> allValues = new List<object>();

            // Always read records when "Read" is true
            if (readRecord)
            {
                _eventTriggered = false; // Reset event flag (because we're manually reading)
                foreach (var record in ReadRecords(tableName))
                {
                    // Convert the DapperRow record to separate key and value lists
                    List<Grasshopper.Kernel.Types.GH_String> keys;
                    List<object> values;
                    ConvertRecordToKeyValueLists(record, out keys, out values);

                    // Add the keys and values to the full lists to be outputted
                    allKeys.AddRange(keys);
                    allValues.AddRange(values);
                }
            }

            // Output the keys and values to Grasshopper
            DA.SetDataList(0, allKeys);   // Output the keys (column names)
            DA.SetDataList(1, allValues); // Output the values (data)
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
        /// Unsubscribe from table changes.
        /// </summary>
        /// <param name="tableName"></param>
        private void UnsubscribeFromTableChanges(string tableName)
        {
            _eventDrivenGhClientApi.UnsubscribeFromTableChanges((sender, args) =>
            {
                if (args.TableName == tableName)
                {
                    _eventTriggered = false;
                }
            });
        }


        /// <summary>
        /// Read all records from the table with the given name.
        /// This is a generator method that yields each record as a string.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private IEnumerable<dynamic> ReadRecords(string tableName)
        {
            IEnumerable<dynamic> records = _eventDrivenGhClientApi.GetAllDictionaryRecords(tableName);
            foreach (var record in records)
            {
                yield return record;
            }
        }

        /// <summary>
        /// Converts the dynamic record into a formatted string for easy viewing in Grasshopper.
        /// </summary>
        private string ConvertRecordToString(dynamic record)
        {
            return record.ToString(); // Customize this based on your record format, can use string interpolation.
        }

        /// <summary>
        /// Converts dynamic DapperRow objects to two separate lists: one for keys and one for values.
        /// </summary>
        private void ConvertRecordToKeyValueLists(dynamic record, out List<Grasshopper.Kernel.Types.GH_String> keys, out List<object> values)
        {
            keys = new List<Grasshopper.Kernel.Types.GH_String>();
            values = new List<object>();

            // Use reflection to access properties of the DapperRow
            var recordProperties = record.GetType().GetProperties();

            foreach (var prop in recordProperties)
            {
                // Add the key (property name) to the keys list
                var propName = prop.Name;
                keys.Add(new Grasshopper.Kernel.Types.GH_String(propName));

                // Convert the value to Grasshopper-compatible types and add to the values list
                var propValue = prop.GetValue(record);
                if (propValue is int)
                    values.Add(new Grasshopper.Kernel.Types.GH_Integer((int)propValue));
                else if (propValue is double)
                    values.Add(new Grasshopper.Kernel.Types.GH_Number((double)propValue));
                else if (propValue is string)
                    values.Add(new Grasshopper.Kernel.Types.GH_String((string)propValue));
                else
                    values.Add(new Grasshopper.Kernel.Types.GH_ObjectWrapper(propValue)); // Default for other types
            }
        }


        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("C53B71A4-2B1B-4C50-9ED6-34975CA6B5D7");
    }
}