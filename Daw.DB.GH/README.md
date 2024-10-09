
# Daw.DB.GH Plugin Suite

## Overview

This repository contains a comprehensive suite of Grasshopper components designed to interact with databases using the Daw.DB APIs. The components cover a wide range of functionalities, from basic database management to advanced event-driven interactions.

## Components

### 1. Database Management Components
- [x] **GhcCreateDatabase**: Creates a new SQLite database file.
- [x] **GhcConnectionString**: Generates and manages connection strings.
- [x] **GhcCreateTable**: Creates tables with specified columns and data types.
- [x] **GhcDropTable**: Drops (deletes) an existing table.
- [ ] **GhcListTables**: Lists all tables in the database.


### 2. Record Management Components
- [x] **GhcCreateRecord**: Inserts new records into a table.
- [x] **GhcReadRecords**: Reads all records from a specified table.
- [ ] **GhcDeleteRecord**: Deletes records from a table based on a primary key or condition.
- [ ] **GhcUpdateRecord**: Updates existing records in a table based on a primary key or condition.
- [ ] **GhcFilterRecords**: Filters records based on specified conditions.
- [ ] **GhcSortRecords**: Sorts records based on specified columns and order.
- [ ] **GhcOneToOneJoin**: Joins records from two tables based on a common key.
- [ ] **GhcOneToManyJoin**: Joins records from two tables based on a common key, allowing multiple matches.
- [ ] **GhcManyToManyJoin**: Joins records from two tables based on a common key, allowing multiple matches on both sides.
- [ ] **GhcAggregateRecords**: Performs aggregation functions on records, such as `SUM`, `AVG`, `COUNT`, etc.
- [ ] **GhcGroupRecords**: Groups records based on specified columns.
- [ ] **GhcPivotRecords**: Pivots records based on specified columns. This means converting rows into columns.
- [ ] **GhcUnpivotRecords**: Unpivots records based on specified columns. This means converting columns into rows.
- [ ] **GhcSelectColumns**: Selects specific columns from a table.
- [ ] **GhcSelectDistinct**: Selects distinct records from a table.
- [ ] **GhcLimitRecords**: Limits the number of records returned.


- 
### 3. Event-Driven Components
- [x] GhcEventfulReadRecords**: Reads records from a table and updates automatically when the table changes.
- **GhcEventfulUpdateRecord**: Listens for changes in a table and automatically updates specific records.
- **GhcSubscribeToTableEvents**: Allows users to subscribe to table changes and trigger actions based on those events.

### 4. Utility Components
- **GhcValidateRecord**: Validates data before inserting or updating records in the database.
- **GhcBackupDatabase**: Backs up the current database to a specified file location.
- **GhcRestoreDatabase**: Restores a database from a backup file.
- **GhcTransactionManager**: Manages database transactions, allowing multiple operations to be executed as a single unit.
- **GhcQueryBuilder**: Builds custom SQL queries dynamically within Grasshopper.
- **GhcExecuteCustomQuery**: Executes a custom SQL query on the database and retrieves results.
- **GhcDatabaseInfo**: Retrieves metadata about the database, such as the number of tables, total records, etc.
- **GhcExportToCSV**: Exports data from a table to a CSV file.
- **GhcImportFromCSV**: Imports data into a table from a CSV file.

### 5. Connection and Configuration Components
- [x] GhcSetConnectionString**: Sets the active connection string for other components to use.
- [x] GhcGetConnectionStatus**: Checks the current connection status and any active transactions.
- **GhcConfigureDatabase**: Configures database settings, such as timeout, encryption, and other SQLite-specific settings.

### 6. Advanced and Specialized Components
- **GhcExecuteStoredProcedure**: Executes stored procedures in the database.
- **GhcSchemaManager**: Manages database schema changes, such as adding or removing columns.
- **GhcDataMigrationTool**: Helps with migrating data from one database to another, possibly with transformations.

### 7. Event Handlers and Triggers
- **GhcTableChangeTrigger**: Triggers specific actions or workflows in Grasshopper when a table change event occurs.
- **GhcRecordChangeHandler**: Listens for changes to specific records and triggers updates or notifications.

### 8. Visualization Components
- **GhcDatabaseViewer**: Allows users to explore the database structure and content within a Grasshopper interface.
- **GhcQueryResultVisualizer**: Visualizes the result of a SQL query in a table format or graph.

### 9. Performance Monitoring Components
- **GhcPerformanceMonitor**: Monitors database performance metrics, such as query execution time, transaction times, etc.
- **GhcLogViewer**: Views the logs of all database operations performed within Grasshopper.

## Integration Considerations
- **Consistency**: Ensure consistent naming conventions across all components for ease of use.
- **Error Handling**: Implement robust error handling in all components, with meaningful feedback to the user.
- **Documentation**: Provide tooltips, descriptions, and example use cases within each component to help users understand how to use them effectively.

## Getting Started
To get started, clone the repository and open it in your preferred IDE. You can build the project and start using the components within Grasshopper.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributions
Contributions are welcome! Please read the [CONTRIBUTING](CONTRIBUTING.md) guidelines before submitting a pull request.
