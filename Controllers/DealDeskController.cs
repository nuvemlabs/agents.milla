using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using agents.milla.Models;
using agents.milla.Services;

namespace agents.milla.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DealDeskController : ControllerBase
{
    private readonly NewDealDeskOrchestrator _orchestrator;
    private readonly ILogger<DealDeskController> _logger;
    private readonly IConfiguration _configuration;

    public DealDeskController(NewDealDeskOrchestrator orchestrator, ILogger<DealDeskController> logger, IConfiguration configuration)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> ProcessDealChat([FromBody] DealRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
        {
            return BadRequest(new { error = "Message is required" });
        }

        if (string.IsNullOrWhiteSpace(request?.AccessCode))
        {
            return BadRequest(new { error = "Access code is required" });
        }

        // Validate access code
        var requiredAccessCode = _configuration["DEMO_ACCESS_CODE"] ?? Environment.GetEnvironmentVariable("DEMO_ACCESS_CODE");
        if (string.IsNullOrWhiteSpace(requiredAccessCode))
        {
            _logger.LogError("DEMO_ACCESS_CODE environment variable not configured");
            return StatusCode(500, new { error = "Demo access not configured" });
        }

        if (request.AccessCode != requiredAccessCode)
        {
            _logger.LogWarning("Invalid access code attempt: {AccessCode}", request.AccessCode);
            return Unauthorized(new { error = "Invalid access code" });
        }

        _logger.LogInformation("Received deal chat request: {Message}", request.Message);

        try
        {
            // Set up Server-Sent Events response
            Response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            
            // Generate a session ID for this conversation (you could use user ID in production)
            var sessionId = HttpContext.Connection.Id ?? "default";
            
            // Process the deal request and stream responses
            await foreach (var agentResponse in _orchestrator.ProcessChatMessageAsync(request, sessionId))
            {
                var jsonResponse = JsonSerializer.Serialize(agentResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                // Write as Server-Sent Event
                await Response.WriteAsync($"data: {jsonResponse}\n\n");
                await Response.Body.FlushAsync();

                // Small delay to prevent overwhelming the client
                await Task.Delay(100);
            }

            // Send final completion event
            await Response.WriteAsync("data: {\"type\":\"complete\",\"message\":\"Deal processing complete\"}\n\n");
            await Response.Body.FlushAsync();

            return new EmptyResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing deal chat request");
            
            var errorResponse = new AgentResponse
            {
                Speaker = "System",
                Text = $"Error: {ex.Message}",
                Status = "error"
            };

            var errorJson = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await Response.WriteAsync($"data: {errorJson}\n\n");
            await Response.Body.FlushAsync();

            return new EmptyResult();
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
} 