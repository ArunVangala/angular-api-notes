using Microsoft.AspNetCore.Mvc;
using DataMasterPro.API.Services;
using DataMasterPro.API.Models;
using Newtonsoft.Json;

namespace DataMasterPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(DatabaseService dbService, ILogger<DatabaseController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        [HttpGet("projects")]
        public IActionResult GetProjects()
        {
            try
            {
                var projects = new[] { "Samast", "GTD", "G2G", "Dev" };
                return Ok(new ApiResponse<string[]>
                {
                    Success = true,
                    Message = "Projects retrieved successfully",
                    Data = projects
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting projects");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("servers/{project}")]
        public IActionResult GetServers(string project)
        {
            try
            {
                var servers = _dbService.GetServersByProject(project);
                return Ok(new ApiResponse<List<ServerConfig>>
                {
                    Success = true,
                    Message = $"Servers retrieved for {project}",
                    Data = servers,
                    RecordCount = servers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting servers for project: {Project}", project);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("databases/{serverName}")]
        public async Task<IActionResult> GetDatabases(string serverName)
        {
            try
            {
                var databases = await _dbService.GetDatabasesAsync(serverName);
                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = "Databases retrieved successfully",
                    Data = databases,
                    RecordCount = databases.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting databases for server: {Server}", serverName);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("tables/{serverName}/{databaseName}")]
        public async Task<IActionResult> GetTables(string serverName, string databaseName)
        {
            try
            {
                var tables = await _dbService.GetTablesAsync(serverName, databaseName);
                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = "Tables retrieved successfully",
                    Data = tables,
                    RecordCount = tables.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tables");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("columns/{serverName}/{databaseName}/{tableName}")]
        public async Task<IActionResult> GetColumns(string serverName, string databaseName, string tableName)
        {
            try
            {
                var columns = await _dbService.GetColumnsAsync(serverName, databaseName, tableName);
                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = "Columns retrieved successfully",
                    Data = columns,
                    RecordCount = columns.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting columns");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("procedures/{serverName}/{databaseName}")]
        public async Task<IActionResult> GetStoredProcedures(string serverName, string databaseName)
        {
            try
            {
                var procedures = await _dbService.GetStoredProceduresAsync(serverName, databaseName);
                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = "Stored procedures retrieved successfully",
                    Data = procedures,
                    RecordCount = procedures.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stored procedures");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("views/{serverName}/{databaseName}")]
        public async Task<IActionResult> GetViews(string serverName, string databaseName)
        {
            try
            {
                var views = await _dbService.GetViewsAsync(serverName, databaseName);
                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = "Views retrieved successfully",
                    Data = views,
                    RecordCount = views.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting views");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("query")]
        public async Task<IActionResult> ExecuteQuery([FromBody] QueryRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Query cannot be empty"
                    });
                }

                if (!request.Query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Only SELECT queries are allowed"
                    });
                }

                var result = await _dbService.ExecuteQueryAsync(
                    request.ServerName, 
                    request.DatabaseName, 
                    request.Query);

                var jsonResult = JsonConvert.SerializeObject(result, Formatting.Indented);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Query executed successfully",
                    Data = jsonResult,
                    RecordCount = result.Rows.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("procedure")]
        public async Task<IActionResult> ExecuteStoredProcedure([FromBody] StoredProcedureRequest request)
        {
            try
            {
                var result = await _dbService.ExecuteStoredProcedureAsync(
                    request.ServerName,
