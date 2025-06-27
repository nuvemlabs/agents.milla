using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using OpenAI;

namespace agents.milla.Services;

public class AgentRouter
{
    private readonly OpenAIConfig _config;
    private readonly ILogger<AgentRouter> _logger;
    private readonly IAgent _routerAgent;

    public AgentRouter(OpenAIConfig config, ILogger<AgentRouter> logger)
    {
        _config = config;
        _logger = logger;

        var openAIClient = new OpenAIClient(_config.ApiKey);
        
        _routerAgent = new OpenAIChatAgent(
            name: "Router",
            systemMessage: """
            You are an intelligent message router for an AI Deal Desk Assistant. Your job is to analyze incoming messages and determine which expert agents should respond.

            Available agents and their expertise:
            - PricingAgent: Pricing strategy, discounts, revenue analysis, margin calculations, payment terms
            - LegalAgent: Contract terms, compliance, risk assessment, legal implications, SLA requirements
            - FinanceAgent: Financial projections, ROI analysis, budget impact, cash flow, ARR calculations  
            - VPAgent: Strategic decisions, executive recommendations, final approvals, high-level business impact
            - SalesRepAgent: Customer relationship, deal coordination, sales process, customer requirements
            - GeneralAssistant: General questions outside the specific expertise areas above

            Analyze the user's message and respond with ONLY a JSON object in this exact format:
            {
              "relevantAgents": ["AgentName1", "AgentName2"],
              "reasoning": "Brief explanation of why these agents are relevant",
              "messageType": "question|proposal_request|analysis_request|general"
            }

            Rules:
            - If asking about pricing, discounts, or financial terms: include PricingAgent and possibly FinanceAgent
            - If asking about contracts, legal terms, or compliance: include LegalAgent
            - If asking about financial impact, ROI, or budgets: include FinanceAgent
            - If asking for executive decision or strategic advice: include VPAgent
            - If asking about sales process or customer management: include SalesRepAgent
            - If asking for a complete deal proposal: include PricingAgent, LegalAgent, FinanceAgent, VPAgent
            - If the question is outside all expertise areas: use GeneralAssistant only
            - Maximum 3 agents per response unless it's a complete proposal request
            """,
            chatClient: openAIClient.GetChatClient(_config.ModelId),
            temperature: 0.1f)
            .RegisterMessageConnector();
    }

    public async Task<AgentRoutingResult> RouteMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Routing message: {Message}", message);

            var response = await _routerAgent.GenerateReplyAsync(
                messages: new List<IMessage> { new TextMessage(Role.User, message, from: "user") },
                cancellationToken: cancellationToken);

            var responseContent = response?.GetContent() ?? "";
            
            // Parse the JSON response
            var routingResult = System.Text.Json.JsonSerializer.Deserialize<AgentRoutingResult>(responseContent, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (routingResult != null && routingResult.RelevantAgents?.Any() == true)
            {
                _logger.LogInformation("Routed to agents: {Agents}", string.Join(", ", routingResult.RelevantAgents));
                return routingResult;
            }
            else
            {
                _logger.LogWarning("Failed to parse routing response, defaulting to GeneralAssistant");
                return new AgentRoutingResult
                {
                    RelevantAgents = new[] { "GeneralAssistant" },
                    Reasoning = "Unable to determine specific expertise area",
                    MessageType = "general"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error routing message, defaulting to GeneralAssistant");
            return new AgentRoutingResult
            {
                RelevantAgents = new[] { "GeneralAssistant" },
                Reasoning = "Error in routing analysis",
                MessageType = "general"
            };
        }
    }
}

public class AgentRoutingResult
{
    public string[] RelevantAgents { get; set; } = Array.Empty<string>();
    public string Reasoning { get; set; } = "";
    public string MessageType { get; set; } = "general";
}