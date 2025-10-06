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
                //  Servers  names and passwords
               
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
