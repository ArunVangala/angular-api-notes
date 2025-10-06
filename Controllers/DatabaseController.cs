using DatabaseAPI.Models;
using DatabaseAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(
            DatabaseService databaseService,
            ILogger<DatabaseController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API is running", timestamp = DateTime.UtcNow });
        }

        [HttpGet("config")]
        public IActionResult GetProjectConfiguration()
        {
            var config = new ProjectConfiguration
            {
                Project = "Dev",
                AllowedProjects = new List<string> { "Samast", "GTD", "G2G", "Dev" },
                Servers = new List<ServerConfig>
                {
                    new() { Name = "DB1", DisplayName = "DB 1 (172.16.13.11)", IPAddress = "172.16.13.11", Project = "Samast" },
                    new() { Name = "DB2", DisplayName = "DB 2 (172.16.13.12)", IPAddress = "172.16.13.12", Project = "Samast" },
                    new() { Name = "DB3", DisplayName = "DB 3 (172.16.11.12)", IPAddress = "172.16.11.12", Project = "Samast" },
                    new() { Name = "VM4", DisplayName = "VM4 (10.10.204.161)", IPAddress = "10.10.204.161", Project = "GTD" },
                    new() { Name = "VM5", DisplayName = "VM5 (10.10.204.162)", IPAddress = "10.10.204.162", Project = "GTD" },
                    new() { Name = "VM6", DisplayName = "VM6 (10.10.204.163)", IPAddress = "10.10.204.163", Project = "GTD" },
                    new() { Name = "VM7", DisplayName = "VM7 (10.10.204.164)", IPAddress = "10.10.204.164", Project = "GTD" },
                    new() { Name = "VM8", DisplayName = "VM8 (10.10.204.165)", IPAddress = "10.10.204.165", Project = "GTD" },
                    new() { Name = "VM9", DisplayName = "VM9 (10.10.204.166)", IPAddress = "10.10.204.166", Project = "GTD" },
                    new() { Name = "DB5", DisplayName = "DB 5 (45.114.246.176)", IPAddress = "45.114.246.176", Project = "G2G" },
                    new() { Name = "DEV1", DisplayName = "DEV 1 (192.168.0.101)", IPAddress = "192.168.0.101", Project = "Dev" },
                    new() { Name = "DEV2", DisplayName = "DEV 2 (43.243.79.150)", IPAddress = "43.243.79.150", Project = "Dev" },
                    new() { Name = "DEV3", DisplayName = "DEV 3 (192.168.0.103)", IPAddress = "192.168.0.103", Project = "Dev" },
                    new() { Name = "DEV4", DisplayName = "DEV 4 (192.168.0.104)", IPAddress = "192.168.0.104", Project = "Dev" },
                    new() { Name = "DEV5", DisplayName = "DEV 5 (192.168.0.105)", IPAddress = "192.168.0.105", Project = "Dev" },
                    new() { Name = "DEV6", DisplayName = "DEV 6 (192.168.0.106)", IPAddress = "192.168.0.106", Project = "Dev" }
                }
            };

            return Ok(config);
        }

        [HttpGet("servers/{serverName}/test")]
        public async Task<IActionResult> TestConnection(string serverName)
        {
            try
            {
                var isConnected = await _databaseService.TestConnectionAsync(serverName);
                return Ok(new { isActive = isConnected, message = isConnected ? "Connected" : "Failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection for {Server}", serverName);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("servers/{serverName}/databases")]
        public async Task<IActionResult> GetDatabases(string serverName)
        {
            try
            {
                var databases = await _databaseService.GetDatabasesAsync(serverName);
                return Ok(databases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting databases for {Server}", serverName);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("servers/{serverName}/databases/{databaseName}/tables")]
        public async Task<IActionResult> GetTables(string serverName, string databaseName)
        {
            try
            {
                var tables = await _databaseService.GetTablesAsync(serverName, databaseName);
                return Ok(tables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tables");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("servers/{serverName}/databases/{databaseName}/tables/{tableName}/columns")]
        public async Task<IActionResult> GetColumns(string serverName, string databaseName, string tableName)
        {
            try
            {
                var columns = await _databaseService.GetColumnsAsync(serverName, databaseName, tableName);
                return Ok(columns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting columns");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("servers/{serverName}/databases/{databaseName}/procedures")]
        public async Task<IActionResult> GetStoredProcedures(string serverName, string databaseName)
        {
            try
            {
                var procedures = await _databaseService.GetStoredProceduresAsync(serverName, databaseName);
                return Ok(procedures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stored procedures");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("servers/{serverName}/databases/{databaseName}/views")]
        public async Task<IActionResult> GetViews(string serverName, string databaseName)
        {
            try
            {
                var views = await _databaseService.GetViewsAsync(serverName, databaseName);
                return Ok(views);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting views");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("query/daterange")]
        public async Task<IActionResult> GetDataByDateRange([FromBody] QueryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ServerName) || 
                    string.IsNullOrEmpty(request.DatabaseName) ||
                    string.IsNullOrEmpty(request.TableName) ||
                    string.IsNullOrEmpty(request.FromDate))
                {
                    return BadRequest(new { error = "Missing required parameters" });
                }

                var result = await _databaseService.GetDataByDateRangeAsync(
                    request.ServerName,
                    request.DatabaseName,
                    request.TableName,
                    request.ColumnName ?? "Date",
                    request.FromDate,
                    request.ToDate
                );

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing date range query");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("query/custom")]
        public async Task<IActionResult> ExecuteCustomQuery([FromBody] QueryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ServerName) || 
                    string.IsNullOrEmpty(request.DatabaseName) ||
                    string.IsNullOrEmpty(request.Query))
                {
                    return BadRequest(new { error = "Missing required parameters" });
                }

                var result = await _databaseService.ExecuteCustomQueryAsync(
                    request.ServerName,
                    request.DatabaseName,
                    request.Query
                );

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing custom query");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("procedures/execute")]
        public async Task<IActionResult> ExecuteStoredProcedure([FromBody] QueryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ServerName) || 
                    string.IsNullOrEmpty(request.DatabaseName) ||
                    string.IsNullOrEmpty(request.ProcedureName))
                {
                    return BadRequest(new { error = "Missing required parameters" });
                }

                var result = await _databaseService.ExecuteStoredProcedureAsync(
                    request.ServerName,
                    request.DatabaseName,
                    request.ProcedureName,
                    request.Parameters
                );

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing stored procedure");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("views/execute")]
        public async Task<IActionResult> ExecuteView([FromBody] QueryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ServerName) || 
                    string.IsNullOrEmpty(request.DatabaseName) ||
                    string.IsNullOrEmpty(request.ViewName))
                {
                    return BadRequest(new { error = "Missing required parameters" });
                }

                var result = await _databaseService.ExecuteViewAsync(
                    request.ServerName,
                    request.DatabaseName,
                    request.ViewName
                );

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing view");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
