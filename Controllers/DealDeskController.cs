using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using agents.milla.Models;
using agents.milla.Services;

namespace agents.milla.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DealDeskController : ControllerBase
{
    private readonly DealDeskOrchestrator _orchestrator;
    private readonly ILogger<DealDeskController> _logger;

    public DealDeskController(DealDeskOrchestrator orchestrator, ILogger<DealDeskController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> ProcessDealChat([FromBody] DealRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
        {
            return BadRequest(new { error = "Message is required" });
        }

        _logger.LogInformation("Received deal chat request: {Message}", request.Message);

        try
        {
            // Set up Server-Sent Events response
            Response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            
            // Process the deal request and stream responses
            await foreach (var agentResponse in _orchestrator.ProcessDealRequestAsync(request))
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