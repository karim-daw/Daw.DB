using Grasshopper.Kernel;
using System;
using System.IO;

namespace Daw.DB.GH
{
    public class GhcLog : GH_Component
    {
        private FileSystemWatcher _fileWatcher;
        private readonly string _logFilePath = "logs/log.txt";

        public GhcLog()
          : base("ReadLogs", "RL",
              "Reads the latest logs from the log file and automatically updates when the log file changes.",
              "Daw.DB", "UTILS")
        {
            InitializeFileWatcher();
        }

        private void InitializeFileWatcher()
        {
            _fileWatcher = new FileSystemWatcher();
            _fileWatcher.Path = Path.GetDirectoryName(_logFilePath);
            _fileWatcher.Filter = Path.GetFileName(_logFilePath);
            _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fileWatcher.Changed += OnLogFileChanged;
            _fileWatcher.EnableRaisingEvents = true;
        }

        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            // Trigger Grasshopper to re-solve the component when the log file is updated
            ExpireSolution(true);
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("ReadLogs", "RL", "Boolean to trigger log reading", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Logs", "L", "Logs read from the file", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool readLogs = false;

            if (!DA.GetData(0, ref readLogs)) return;

            if (readLogs)
            {
                try
                {
                    if (File.Exists(_logFilePath))
                    {
                        var logs = File.ReadAllLines(_logFilePath);
                        DA.SetDataList(0, logs);
                    }
                    else
                    {
                        DA.SetData(0, "Log file not found.");
                    }
                }
                catch (Exception ex)
                {
                    DA.SetData(0, $"Error reading log file: {ex.Message}");
                }
            }
        }

        protected override void BeforeSolveInstance()
        {
            // Make sure the file watcher is enabled before solving
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = true;
            }
        }

        protected override void AfterSolveInstance()
        {
            // Disable the file watcher after solving to avoid unnecessary triggers
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
            }
        }


        public override void RemovedFromDocument(GH_Document document)
        {
            // Clean up the file watcher when the component is removed from the canvas
            if (_fileWatcher != null)
            {
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }

            base.RemovedFromDocument(document);
        }


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("EA045ED0-71BB-4114-8898-4BCA01082300");
    }
}