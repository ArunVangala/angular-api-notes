using Microsoft.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;

namespace DatabaseAPI.Services
{
    public class DatabaseService
    {
        private readonly ConnectionStringService _connectionStringService;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(
            ConnectionStringService connectionStringService,
            ILogger<DatabaseService> logger)
        {
            _connectionStringService = connectionStringService;
            _logger = logger;
        }

        public async Task<bool> TestConnectionAsync(string serverName)
        {
            try
            {
                var connectionString = _connectionStringService.GetConnectionString(serverName, "master");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed for {Server}", serverName);
                return false;
            }
        }

        public async Task<List<string>> GetDatabasesAsync(string serverName)
        {
            var databases = new List<string>();
            var connectionString = _connectionStringService.GetConnectionString(serverName, "master");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"SELECT name FROM sys.databases 
                         WHERE database_id > 4 AND state = 0 
                         ORDER BY name";

            using var command = new SqlCommand(query, connection);
            command.CommandTimeout = 15;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                databases.Add(reader.GetString(0));
            }

            return databases;
        }

        public async Task<List<string>> GetTablesAsync(string serverName, string databaseName)
        {
            var tables = new List<string>();
            var connectionString = _connectionStringService.GetConnectionString(serverName, databaseName);

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
                         WHERE TABLE_TYPE = 'BASE TABLE' 
                         ORDER BY TABLE_NAME";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }

        public async Task<List<string>> GetColumnsAsync(string serverName, string databaseName, string tableName)
        {
            var columns = new List<string>();
            var connectionString = _connectionStringService.GetConnectionString(serverName, databaseName);

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
                         WHERE TABLE_NAME = @TableName 
                         ORDER BY ORDINAL_POSITION";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }

            return columns;
        }

        public async Task<List<string>> GetStoredProceduresAsync(string serverName, string databaseName)
        {
            var procedures = new List<string>();
            var connectionString = _connectionStringService.GetConnectionString(serverName, databaseName);

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES 
                         WHERE ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_SCHEMA = 'dbo'
                         ORDER BY ROUTINE_NAME";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                procedures.Add(reader.GetString(0));
            }

            return procedures;
        }

        public async Task<List<string>> GetViewsAsync(string serverName, string databaseName)
        {
            var views = new List<string>();
            var connectionString = _connectionStringService.GetConnectionString(serverName, databaseName);

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS 
                         WHERE TABLE_SCHEMA = 'dbo'
                         ORDER BY TABLE_NAME";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                views.Add(reader.GetString(0));
            }

            return views;
        }

        public async Task<string> GetDataByDateRangeAsync(
            string serverName, 
            string databaseName, 
            string tableName, 
            string columnName, 
            string fromDate, 
            string? toDate = null)
        {
            var connectionString = _connectionStringService.GetConnectionString(serverName, databaseName);
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string query;
            if (string.IsNullOrEmpty(toDate))
            {
                query = $"SELECT * FROM [{tableName}] WHERE CAST([{columnName}] AS DATE) = @FromDate";
            }
            else
            {
                query = $"SELECT * FROM [{tableName}] WHERE CAST([{columnName}] AS DATE) BETWEEN @FromDate AND @ToDate";
            }

            using var command = new SqlCommand(query, connection);
            command.CommandTimeout = 300;
            command.Parameters.Add("@FromDate", SqlDbType.Date).Value = DateTime.Parse(fromDate);
            
            if (!string.IsNullOrEmpty(toDate))
            {
                command.Parameters.Add("@ToDate", SqlDbType.Date).Value = DateTime.Parse(toDate);
            }

            var dataTable = new DataTable();
            using var adapter = new SqlDataAdapter(command);
            await Task.Run(() => adapter.Fill(dataTable));

            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }

        public async Task<string> ExecuteCustomQueryAsync(
            string serverName, 
            string databaseName, 
            string query)
        {
            if (!IsSelectQuery(query))
            {
                throw new InvalidOperationException("Only SELECT queries are allowed");
            }

            var connectionString = _connectionStringService.GetConnectionString(serverName, databaseName);
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.CommandTimeout = 300;

            var dataTable = new DataTable();
            using var adapter = new SqlDataAdapter(command);
            await Task.Run(() => adapter.Fill(dataTable));

            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }

        public async Task<string> ExecuteStoredProcedureAsync(
            string serverName,
            string databaseName,
            string procedureName,
            Dictionary<string, object>? parameters = null)
        {
            var connectionString = _connectionStringService.GetConnectionString(serverName, databaseName);
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(procedureName, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 300;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var paramName = param.Key.StartsWith("@") ? param.Key : "@" + param.Key;
                    command.Parameters.AddWithValue(paramName, param.Value ?? DBNull.Value);
                }
            }

            var dataTable = new DataTable();
            using var adapter = new SqlDataAdapter(command);
            await Task.Run(() => adapter.Fill(dataTable));

            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }

        public async Task<string> ExecuteViewAsync(
            string serverName,
            string databaseName,
            string viewName)
        {
            var connectionString = _connectionStringService.GetConnectionString(serverName, databaseName);
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = $"SELECT * FROM [{viewName}]";
            using var command = new SqlCommand(query, connection);
            command.CommandTimeout = 300;

            var dataTable = new DataTable();
            using var adapter = new SqlDataAdapter(command);
            await Task.Run(() => adapter.Fill(dataTable));

            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }

        private bool IsSelectQuery(string query)
        {
            var lowerQuery = query.ToLower().Trim();
            if (!lowerQuery.StartsWith("select") && !lowerQuery.StartsWith("with"))
                return false;

            string[] forbidden = { "drop ", "delete ", "truncate ", "alter ", 
                                  "insert ", "update ", "create ", "exec ", 
                                  "execute ", "sp_", "xp_" };
            
            return !forbidden.Any(keyword => lowerQuery.Contains(keyword));
        }
    }
}
