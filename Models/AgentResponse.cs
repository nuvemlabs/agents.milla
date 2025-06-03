using System.Text.Json.Serialization;

namespace agents.milla.Models;

public class AgentResponse
{
    public required string Speaker { get; set; }
    public required string Text { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "processing";
    public AgentData? Data { get; set; }
}

public class AgentData
{
    public decimal? Margin { get; set; }
    public List<string> Approvals { get; set; } = new();
    public List<string> Blockers { get; set; } = new();
    public int? LegalRiskScore { get; set; }
    public decimal? ARRImpact { get; set; }
    public string? DealStatus { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
} 