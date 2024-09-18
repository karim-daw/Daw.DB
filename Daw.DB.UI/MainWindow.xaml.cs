using Daw.DB.Data;
using Daw.DB.Data.APIs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace Daw.DB.UI
{
    public partial class MainWindow : Window
    {
        private bool isDataVisible = false;
        private readonly IGhClientApi _ghClientApi;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize GhClientApi (assuming ApiFactory is handling dependencies correctly)
            _ghClientApi = ApiFactory.GetGhClientApi();
        }

        private void ShowDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isDataVisible)
            {
                // Load data and display it
                var dataTable = GetDataFromDatabase();
                DataGridView.ItemsSource = dataTable.DefaultView;
                DataGridView.Visibility = Visibility.Visible;
                ShowDataButton.Content = "Hide Data";
                isDataVisible = true;
            }
            else
            {
                // Hide the data
                DataGridView.Visibility = Visibility.Collapsed;
                ShowDataButton.Content = "Show Data";
                isDataVisible = false;
            }
        }

        private DataTable GetDataFromDatabase()
        {
            string connectionString = "Data Source=C:\\Users\\43310\\OneDrive - Gensler\\Desktop\\mySuperCoolDb.db;Version=3;";
            string tableName = "MyTable"; // Change this to your actual table name

            try
            {
                // Fetch records from the table using GhClientApi
                var records = _ghClientApi.GetAllDictionaryRecords(tableName, connectionString);

                // Convert the dynamic records to a DataTable for display
                DataTable dataTable = records.ToDataTable();
                return dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return new DataTable();
            }
        }
    }

    // Helper class to convert dynamic results to DataTable
    public static class DapperExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<dynamic> items)
        {
            var dataTable = new DataTable();

            if (items == null || !items.Any()) return dataTable;

            // Get the first item and create the columns in the DataTable
            var firstItem = (IDictionary<string, object>)items.First();
            foreach (var key in firstItem.Keys)
            {
                dataTable.Columns.Add(key);
            }

            // Add rows to the DataTable
            foreach (var item in items)
            {
                var row = dataTable.NewRow();
                var dictionary = (IDictionary<string, object>)item;

                foreach (var key in dictionary.Keys)
                {
                    row[key] = dictionary[key] ?? DBNull.Value;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
