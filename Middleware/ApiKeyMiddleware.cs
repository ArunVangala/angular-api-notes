using DatabaseAPI.Models;
using Microsoft.Extensions.Options;

namespace DatabaseAPI.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiKeySettings _apiKeySettings;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(
            RequestDelegate next, 
            IOptions<ApiKeySettings> apiKeySettings, 
            ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _apiKeySettings = apiKeySettings.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string requestPath = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            if (!requestPath.StartsWith("/api/"))
            {
                await _next(context);
                return;
            }

            string? providedApiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(providedApiKey))
            {
                providedApiKey = context.Request.Query["apiKey"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(providedApiKey))
            {
                _logger.LogWarning("Missing API key for request: {Path}", requestPath);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = "API Key is required", 
                    message = "Provide API key in 'X-API-Key' header or 'apiKey' query parameter" 
                });
                return;
            }

            if (!IsValidApiKey(providedApiKey))
            {
                _logger.LogWarning("Invalid API key attempted: {Key}", providedApiKey);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = "Invalid API Key", 
                    message = "The provided API key is not valid" 
                });
                return;
            }

            _logger.LogInformation("Valid API key for: {Path}", requestPath);
            await _next(context);
        }

        private bool IsValidApiKey(string apiKey)
        {
            return _apiKeySettings?.ValidApiKeys?.Contains(apiKey) ?? false;
        }
    }
}
