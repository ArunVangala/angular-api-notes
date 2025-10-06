namespace DatabaseAPI.Models
{
    public class ServerConfig
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
    }

    public class ProjectConfiguration
    {
        public string Project { get; set; } = string.Empty;
        public List<string> AllowedProjects { get; set; } = new List<string>();
        public List<ServerConfig> Servers { get; set; } = new List<ServerConfig>();
    }

    public class QueryRequest
    {
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string? TableName { get; set; }
        public string? ColumnName { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Query { get; set; }
        public string? ProcedureName { get; set; }
        public string? ViewName { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
