namespace agents.milla.Models;

public class DealStatus
{
    public string DealId { get; set; } = Guid.NewGuid().ToString();
    public string Status { get; set; } = "processing"; // processing, approved, denied
    public decimal? TotalMargin { get; set; }
    public int? LegalRiskScore { get; set; }
    public decimal? ARRImpact { get; set; }
    public List<string> CompletedApprovals { get; set; } = new();
    public List<string> PendingApprovals { get; set; } = new() { "pricing", "legal", "finance", "vp" };
    public List<string> Blockers { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string? FinalDecision { get; set; }
    public string? DecisionReasoning { get; set; }
} 