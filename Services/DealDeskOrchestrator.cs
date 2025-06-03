using AutoGen;
using AutoGen.OpenAI;
using agents.milla.Models;
using agents.milla.Services.Agents;

namespace agents.milla.Services;

public class DealDeskOrchestrator
{
    private readonly OpenAIConfig _config;
    private readonly ILogger<DealDeskOrchestrator> _logger;

    // Agents
    private readonly SalesRepProxyAgent _salesRepAgent;
    private readonly PricingAgent _pricingAgent;
    private readonly LegalAgent _legalAgent;
    private readonly FinanceAgent _financeAgent;
    private readonly VPApprovalAgent _vpAgent;

    public DealDeskOrchestrator(IConfiguration configuration, ILogger<DealDeskOrchestrator> logger)
    {
        _logger = logger;
        
        // Get OpenAI configuration
        var apiKey = configuration["OpenAI:ApiKey"] ?? 
                    Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                    throw new InvalidOperationException("OpenAI API key not found. Set OPENAI_API_KEY environment variable or configure in appsettings.");

        _config = new OpenAIConfig(apiKey, "gpt-4");

        // Initialize agents
        _salesRepAgent = SalesRepProxyAgent.Create();
        _pricingAgent = PricingAgent.Create(_config);
        _legalAgent = LegalAgent.Create(_config);
        _financeAgent = FinanceAgent.Create(_config);
        _vpAgent = VPApprovalAgent.Create(_config);

        _logger.LogInformation("DealDeskOrchestrator initialized with {AgentCount} agents", 5);
    }

    public async IAsyncEnumerable<AgentResponse> ProcessDealRequestAsync(DealRequest request)
    {
        _logger.LogInformation("Processing deal request: {Message}", request.Message);

        var dealStatus = new DealStatus();
        var responses = new List<AgentResponse>();

        // Process all responses
        try
        {
            // Start with sales rep agent
            responses.Add(new AgentResponse
            {
                Speaker = "SalesRep",
                Text = $"Processing deal request: {request.Message}",
                Status = "processing"
            });

            // Pricing Agent Analysis
            _logger.LogInformation("Starting pricing analysis");
            responses.Add(new AgentResponse
            {
                Speaker = "PricingAgent",
                Text = "Analyzing pricing and discount policy...",
                Status = "processing"
            });

            await Task.Delay(1000); // Simulate processing time
            var pricingResponse = await GetSimulatedPricingResponse(request.Message);
            UpdateDealStatus(dealStatus, pricingResponse);
            responses.Add(pricingResponse);

            // Legal Agent Analysis
            _logger.LogInformation("Starting legal analysis");
            responses.Add(new AgentResponse
            {
                Speaker = "LegalAgent", 
                Text = "Reviewing legal implications and risk factors...",
                Status = "processing"
            });

            await Task.Delay(1000);
            var legalResponse = await GetSimulatedLegalResponse(request.Message);
            UpdateDealStatus(dealStatus, legalResponse);
            responses.Add(legalResponse);

            // Finance Agent Analysis
            _logger.LogInformation("Starting finance analysis");
            responses.Add(new AgentResponse
            {
                Speaker = "FinanceAgent",
                Text = "Analyzing financial impact and profitability...",
                Status = "processing"
            });

            await Task.Delay(1000);
            var financeResponse = await GetSimulatedFinanceResponse(request.Message);
            UpdateDealStatus(dealStatus, financeResponse);
            responses.Add(financeResponse);

            // VP Approval Agent Final Decision
            _logger.LogInformation("Getting VP approval decision");
            responses.Add(new AgentResponse
            {
                Speaker = "VPApprovalAgent",
                Text = "Making final approval decision based on all analysis...",
                Status = "processing"
            });

            await Task.Delay(1500);
            var vpResponse = await GetSimulatedVPResponse(dealStatus);
            UpdateDealStatus(dealStatus, vpResponse);
            responses.Add(vpResponse);

            _logger.LogInformation("Final decision reached: {Decision}", dealStatus.FinalDecision);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing deal request");
            responses.Add(new AgentResponse
            {
                Speaker = "System",
                Text = $"Error processing request: {ex.Message}",
                Status = "error"
            });
        }

        // Yield all responses
        foreach (var response in responses)
        {
            yield return response;
        }
    }

    private Task<AgentResponse> GetSimulatedPricingResponse(string dealRequest)
    {
        // Extract information from deal request for simulation
        var text = $@"
Based on the deal request analysis:

For this deal, I've calculated the following:
- Base pricing: $120 per seat per year
- Volume discount applied: 10% (for 250+ seats)
- Term discount: 5% (for 2-year contract)
- Requested discount: 20%
- Final margin after all discounts: 68%

The requested 20% discount is within our standard policy limits.
Customer payment terms of 30 days are acceptable.

MARGIN: 68% | RECOMMENDATION: approve";

        return Task.FromResult(new AgentResponse
        {
            Speaker = "PricingAgent",
            Text = text,
            Status = "completed",
            Data = new AgentData
            {
                Margin = 68m,
                Approvals = new List<string> { "pricing" }
            }
        });
    }

    private Task<AgentResponse> GetSimulatedLegalResponse(string dealRequest)
    {
        var text = $@"
Legal review completed:

Risk factors identified:
- Standard payment terms (30 days): Low risk (+1 point)
- No unusual indemnification clauses
- Standard SLA requirements
- Jurisdiction: Delaware (acceptable)

Total risk assessment: 2/10 (Low risk)
No legal blockers identified.

RISK_SCORE: 2 | BLOCKERS: no | RECOMMENDATION: approve";

        return Task.FromResult(new AgentResponse
        {
            Speaker = "LegalAgent",
            Text = text,
            Status = "completed",
            Data = new AgentData
            {
                LegalRiskScore = 2,
                Approvals = new List<string> { "legal" }
            }
        });
    }

    private Task<AgentResponse> GetSimulatedFinanceResponse(string dealRequest)
    {
        var text = $@"
Financial analysis complete:

Contract details:
- Total contract value: $600,000 (250 seats × $120 × 2 years)
- Annual recurring revenue: $300,000
- Gross margin after COGS (25%): 68%
- Payment terms: 30 days (positive cash flow impact)

Financial metrics exceed our minimum thresholds:
- Margin 68% > 60% minimum requirement
- ARR impact: +$300K annually
- Cash flow: Positive

ARR_IMPACT: $300,000 | MARGIN: 68% | CASH_FLOW: positive | RECOMMENDATION: approve";

        return Task.FromResult(new AgentResponse
        {
            Speaker = "FinanceAgent", 
            Text = text,
            Status = "completed",
            Data = new AgentData
            {
                Margin = 68m,
                ARRImpact = 300000m,
                Approvals = new List<string> { "finance" }
            }
        });
    }

    private Task<AgentResponse> GetSimulatedVPResponse(DealStatus dealStatus)
    {
        var text = $@"
Executive Review Summary:

Input from all departments:
✓ Pricing: 68% margin, discount within policy
✓ Legal: Low risk score (2/10), no blockers  
✓ Finance: Strong ARR impact (+$300K), positive cash flow

Decision factors:
- All departments recommend approval
- Margin well above 60% minimum threshold
- Strategic customer with growth potential
- Terms align with company policies

This deal meets all approval criteria and represents good strategic value.

FINAL_DECISION: APPROVED | CONDITIONS: Standard monitoring | REASONING: Strong financial metrics with low risk";

        return Task.FromResult(new AgentResponse
        {
            Speaker = "VPApprovalAgent",
            Text = text,
            Status = "completed",
            Data = new AgentData
            {
                DealStatus = "approved",
                Approvals = new List<string> { "vp" }
            }
        });
    }

    private void UpdateDealStatus(DealStatus dealStatus, AgentResponse response)
    {
        if (response.Data == null) return;

        // Update margin
        if (response.Data.Margin.HasValue)
        {
            dealStatus.TotalMargin = response.Data.Margin.Value;
        }

        // Update legal risk score
        if (response.Data.LegalRiskScore.HasValue)
        {
            dealStatus.LegalRiskScore = response.Data.LegalRiskScore.Value;
        }

        // Update ARR impact
        if (response.Data.ARRImpact.HasValue)
        {
            dealStatus.ARRImpact = response.Data.ARRImpact.Value;
        }

        // Update approvals
        foreach (var approval in response.Data.Approvals)
        {
            if (!dealStatus.CompletedApprovals.Contains(approval))
            {
                dealStatus.CompletedApprovals.Add(approval);
                dealStatus.PendingApprovals.Remove(approval);
            }
        }

        // Update blockers
        dealStatus.Blockers.AddRange(response.Data.Blockers);

        // Update final status
        if (!string.IsNullOrEmpty(response.Data.DealStatus))
        {
            dealStatus.Status = response.Data.DealStatus;
            dealStatus.FinalDecision = response.Data.DealStatus;
        }

        dealStatus.LastUpdated = DateTime.UtcNow;
    }
} 