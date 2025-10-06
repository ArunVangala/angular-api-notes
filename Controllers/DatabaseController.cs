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
                  ///show all the servers here
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
