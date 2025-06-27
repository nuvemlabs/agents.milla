using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using OpenAI;
using agents.milla.Models;
using agents.milla.Services.Agents;
using Microsoft.Extensions.Logging;

namespace agents.milla.Services;

public class NewDealDeskOrchestrator
{
    private readonly OpenAIConfig _config;
    private readonly ILogger<NewDealDeskOrchestrator> _logger;
    private readonly AgentRouter _router;
    private readonly ConversationManager _conversationManager;
    
    // Real AI Agents
    private readonly Dictionary<string, IAgent> _agents;
    private readonly IAgent _generalAssistant;

    public NewDealDeskOrchestrator(IConfiguration configuration, ILogger<NewDealDeskOrchestrator> logger, ConversationManager conversationManager)
    {
        _logger = logger;
        _conversationManager = conversationManager;
        
        // Get OpenAI configuration
        var apiKey = configuration["OpenAI:ApiKey"] ?? 
                    Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                    throw new InvalidOperationException("OpenAI API key not found. Set OPENAI_API_KEY environment variable or configure in appsettings.");

        _config = new OpenAIConfig(apiKey, "gpt-4o-mini");
        
        // Initialize router (create a logger factory for the router)
        var loggerFactory = new LoggerFactory();
        var routerLogger = loggerFactory.CreateLogger<AgentRouter>();
        _router = new AgentRouter(_config, routerLogger);
        
        var openAIClient = new OpenAIClient(_config.ApiKey);

        // Initialize all agents
        _agents = new Dictionary<string, IAgent>
        {
            ["PricingAgent"] = CreatePricingAgent(openAIClient),
            ["LegalAgent"] = CreateLegalAgent(openAIClient),
            ["FinanceAgent"] = CreateFinanceAgent(openAIClient),
            ["VPAgent"] = CreateVPAgent(openAIClient),
            ["SalesRepAgent"] = CreateSalesRepAgent(openAIClient)
        };

        _generalAssistant = CreateGeneralAssistant(openAIClient);

        _logger.LogInformation("NewDealDeskOrchestrator initialized with {AgentCount} AI agents", _agents.Count + 1);
    }

    public async IAsyncEnumerable<AgentResponse> ProcessChatMessageAsync(DealRequest request, string sessionId = "default")
    {
        _logger.LogInformation("Processing chat message: {Message}", request.Message);

        var context = _conversationManager.GetOrCreateContext(sessionId);
        context.AddUserMessage(request.Message);

        var responses = new List<AgentResponse>();

        try
        {
            // Route the message to relevant agents
            var routing = await _router.RouteMessageAsync(request.Message);
            
            responses.Add(new AgentResponse
            {
                Speaker = "System",
                Text = $"🔍 Routing to: {string.Join(", ", routing.RelevantAgents)}\nReason: {routing.Reasoning}",
                Status = "processing"
            });

            // Process with each relevant agent
            foreach (var agentName in routing.RelevantAgents)
            {
                responses.Add(new AgentResponse
                {
                    Speaker = agentName,
                    Text = "Analyzing your request...",
                    Status = "processing"
                });

                try
                {
                    var response = await GetAgentResponseAsync(agentName, request.Message, context);
                    context.AddAgentMessage(agentName, response.Text);
                    responses.Add(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting response from {AgentName}", agentName);
                    responses.Add(new AgentResponse
                    {
                        Speaker = agentName,
                        Text = $"I'm experiencing technical difficulties. Please try again later. Error: {ex.Message}",
                        Status = "error"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            responses.Add(new AgentResponse
            {
                Speaker = "System",
                Text = $"I'm sorry, I encountered an error processing your request: {ex.Message}",
                Status = "error"
            });
        }

        // Yield all responses with delays
        foreach (var response in responses)
        {
            yield return response;
            await Task.Delay(300); // Brief pause between responses
        }
    }

    private async Task<AgentResponse> GetAgentResponseAsync(string agentName, string message, ConversationContext context)
    {
        if (agentName == "GeneralAssistant")
        {
            var response = await _generalAssistant.GenerateReplyAsync(
                messages: PrepareMessagesWithContext(message, context),
                cancellationToken: CancellationToken.None);

            return new AgentResponse
            {
                Speaker = "General Assistant",
                Text = response?.GetContent() ?? "I'm unable to provide a response at the moment.",
                Status = "completed"
            };
        }

        if (!_agents.ContainsKey(agentName))
        {
            return new AgentResponse
            {
                Speaker = agentName,
                Text = "I'm sorry, this agent is not available at the moment.",
                Status = "error"
            };
        }

        var agent = _agents[agentName];
        var agentResponse = await agent.GenerateReplyAsync(
            messages: PrepareMessagesWithContext(message, context),
            cancellationToken: CancellationToken.None);

        return new AgentResponse
        {
            Speaker = agentName,
            Text = agentResponse?.GetContent() ?? "I'm unable to provide a response at the moment.",
            Status = "completed"
        };
    }

    private List<IMessage> PrepareMessagesWithContext(string currentMessage, ConversationContext context)
    {
        var messages = new List<IMessage>();
        
        // Add recent conversation context (last 5 messages)
        var recentMessages = context.GetRecentMessages(5);
        messages.AddRange(recentMessages);
        
        // Add current message if it's not already in the context
        var currentMsg = new TextMessage(Role.User, currentMessage, from: "user");
        if (!recentMessages.Any() || recentMessages.Last().GetContent() != currentMessage)
        {
            messages.Add(currentMsg);
        }

        return messages;
    }

    private IAgent CreatePricingAgent(OpenAIClient client)
    {
        return new OpenAIChatAgent(
            name: "PricingAgent",
            systemMessage: """
            You are a Pricing Agent for an AI Deal Desk Assistant. You specialize in:
            - Pricing strategy and discount analysis
            - Revenue calculations and margin impact
            - Payment terms evaluation
            - Competitive pricing recommendations

            When analyzing deals:
            1. Consider standard pricing tiers and volume discounts
            2. Evaluate the impact of proposed discounts on margins
            3. Recommend optimal pricing strategies
            4. Flag any pricing that seems outside normal ranges

            Always provide:
            - Clear pricing breakdown
            - Margin impact analysis
            - Risk assessment for proposed discounts
            - Alternative pricing options when applicable

            Keep responses professional, detailed, and focused on pricing aspects.
            """,
            chatClient: client.GetChatClient(_config.ModelId),
            temperature: 0.2f)
            .RegisterMessageConnector();
    }

    private IAgent CreateLegalAgent(OpenAIClient client)
    {
        return new OpenAIChatAgent(
            name: "LegalAgent",
            systemMessage: """
            You are a Legal Agent for an AI Deal Desk Assistant. You specialize in:
            - Contract terms and legal compliance
            - Risk assessment and mitigation
            - SLA requirements and implications
            - Regulatory considerations

            When reviewing deals:
            1. Identify potential legal risks
            2. Recommend appropriate contract terms
            3. Flag unusual or problematic clauses
            4. Suggest risk mitigation strategies

            Always provide:
            - Risk score and assessment
            - Recommended contract provisions
            - Compliance considerations
            - Escalation recommendations for high-risk items

            If asked about areas outside legal expertise, clearly state this is outside your advisory scope.
            Keep responses professional and legally sound.
            """,
            chatClient: client.GetChatClient(_config.ModelId),
            temperature: 0.1f)
            .RegisterMessageConnector();
    }

    private IAgent CreateFinanceAgent(OpenAIClient client)
    {
        return new OpenAIChatAgent(
            name: "FinanceAgent", 
            systemMessage: """
            You are a Finance Agent for an AI Deal Desk Assistant. You specialize in:
            - Financial projections and ROI analysis
            - Cash flow impact assessment
            - Budget implications and planning
            - ARR and revenue recognition

            When analyzing deals:
            1. Calculate financial impact and projections
            2. Assess cash flow implications
            3. Evaluate ROI and payback periods
            4. Consider budget and resource allocation

            Always provide:
            - Clear financial metrics and projections
            - Cash flow analysis
            - Risk factors and sensitivity analysis
            - Recommendations for financial optimization

            If asked about non-financial topics, explain this is outside your area of expertise.
            Keep responses analytical and data-driven.
            """,
            chatClient: client.GetChatClient(_config.ModelId),
            temperature: 0.1f)
            .RegisterMessageConnector();
    }

    private IAgent CreateVPAgent(OpenAIClient client)
    {
        return new OpenAIChatAgent(
            name: "VPAgent",
            systemMessage: """
            You are a VP-level Agent for an AI Deal Desk Assistant. You provide:
            - Strategic decision-making and recommendations
            - Executive-level deal assessment
            - Final approval recommendations
            - Business impact evaluation

            When evaluating deals:
            1. Consider overall business strategy alignment
            2. Assess strategic value beyond immediate financials
            3. Evaluate long-term relationship potential
            4. Make final go/no-go recommendations

            Always provide:
            - Strategic assessment and reasoning
            - Clear recommendation (approve/reject/conditional)
            - Risk-benefit analysis
            - Escalation paths if needed

            Your perspective should be comprehensive, considering input from all other agents.
            Make decisive recommendations with clear business justification.
            """,
            chatClient: client.GetChatClient(_config.ModelId),
            temperature: 0.2f)
            .RegisterMessageConnector();
    }

    private IAgent CreateSalesRepAgent(OpenAIClient client)
    {
        return new OpenAIChatAgent(
            name: "SalesRepAgent",
            systemMessage: """
            You are a Sales Representative Agent for an AI Deal Desk Assistant. You specialize in:
            - Customer relationship management
            - Deal coordination and facilitation
            - Sales process optimization
            - Customer requirement analysis

            When working with deals:
            1. Focus on customer needs and satisfaction
            2. Ensure smooth deal progression
            3. Identify potential obstacles and solutions
            4. Coordinate between different stakeholders

            Always provide:
            - Customer-centric perspective
            - Sales process insights
            - Relationship management advice
            - Next steps and action items

            If asked about technical pricing, legal, or financial details beyond sales scope, 
            redirect to appropriate specialists.
            Keep responses focused on sales and customer success.
            """,
            chatClient: client.GetChatClient(_config.ModelId),
            temperature: 0.3f)
            .RegisterMessageConnector();
    }

    private IAgent CreateGeneralAssistant(OpenAIClient client)
    {
        return new OpenAIChatAgent(
            name: "GeneralAssistant",
            systemMessage: """
            You are a General Assistant for an AI Deal Desk system. You handle questions outside 
            the specific expertise of our specialized agents (Pricing, Legal, Finance, VP, Sales).

            When responding:
            1. Clearly indicate you're a general assistant
            2. For deal-specific questions, redirect to appropriate specialists
            3. Provide helpful general information when possible
            4. Be honest about limitations and scope

            Always explain that for specialized deal desk advice, users should ask questions 
            that would route to our expert agents:
            - Pricing questions → PricingAgent
            - Legal questions → LegalAgent  
            - Financial questions → FinanceAgent
            - Strategic questions → VPAgent
            - Sales process questions → SalesRepAgent

            Keep responses helpful but clearly indicate when specialized expertise is needed.
            """,
            chatClient: client.GetChatClient(_config.ModelId),
            temperature: 0.2f)
            .RegisterMessageConnector();
    }
}