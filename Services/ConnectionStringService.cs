namespace DatabaseAPI.Services
{
    public class ConnectionStringService
    {
        private readonly ILogger<ConnectionStringService> _logger;
        private readonly Dictionary<string, ServerCredentials> _serverCredentials;

        public ConnectionStringService(ILogger<ConnectionStringService> logger)
        {
            _logger = logger;
            _serverCredentials = InitializeServerCredentials();
        }

        private Dictionary<string, ServerCredentials> InitializeServerCredentials()
        {
            return new Dictionary<string, ServerCredentials>(StringComparer.OrdinalIgnoreCase)
            {
                // SAMAST Servers
                ["DB1"] = new("172.16.13.11", "sa", "TsDs!1$"),
                ["DB2"] = new("172.16.13.12", "sa", "Tvd@123"),
                ["DB3"] = new("172.16.11.12", "sa", "Smst@7h64"),
                
                // GTD Servers
                ["VM4"] = new("10.10.204.161", "sa", "hoeur#q8"),
                ["VM5"] = new("10.10.204.162", "sa", "Gtdtvd@2"),
                ["VM6"] = new("10.10.204.163", "sa", "Gtdtvd@3"),
                ["VM7"] = new("10.10.204.164", "sa", "juS$$qq62"),
                ["VM8"] = new("10.10.204.165", "sa", "Ywz^&mr0"),
                ["VM9"] = new("10.10.204.166", "sa", "O0tc%lvX"),
                
                // G2G Servers
                ["DB5"] = new("45.114.246.176", "sa", "CpWQ6#uDntM7f4c$"),
                
                // Dev Servers
                ["DEV1"] = new("192.168.0.101", "sa", "Tvd1234!"),
                ["DEV2"] = new("43.243.79.150", "sa", "hoeur#q8"),
                ["DEV3"] = new("192.168.0.103", "sa", "Dev3Pass!"),
                ["DEV4"] = new("192.168.0.104", "sa", "Dev4Pass!"),
                ["DEV5"] = new("192.168.0.105", "sa", "Dev5Pass!"),
                ["DEV6"] = new("192.168.0.106", "sa", "Dev6Pass!")
            };
        }

        public string GetConnectionString(string serverName, string databaseName)
        {
            var cleanServerName = CleanServerName(serverName);
            
            if (!_serverCredentials.TryGetValue(cleanServerName, out var credentials))
            {
                throw new InvalidOperationException($"Server '{serverName}' is not configured");
            }

            var isDevServer = credentials.Server.Contains("192.168") || 
                            credentials.Server.Contains("localhost");
            
            var connectTimeout = isDevServer ? 30 : 15;
            var commandTimeout = isDevServer ? 60 : 30;

            return $"Data Source={credentials.Server};" +
                   $"Initial Catalog={databaseName};" +
                   $"User ID={credentials.Username};" +
                   $"Password={credentials.Password};" +
                   $"Connect Timeout={connectTimeout};" +
                   $"Command Timeout={commandTimeout};" +
                   $"Pooling=true;" +
                   $"Max Pool Size=100;" +
                   $"Min Pool Size=5;" +
                   $"MultipleActiveResultSets=true;" +
                   $"TrustServerCertificate=true;" +
                   $"Encrypt=false;";
        }

        private string CleanServerName(string serverName)
        {
            var cleaned = serverName.Contains('(') 
                ? serverName.Substring(0, serverName.IndexOf('(')).Trim()
                : serverName.Trim();
            
            return cleaned.ToUpper();
        }

        private record ServerCredentials(string Server, string Username, string Password);
    }
}
