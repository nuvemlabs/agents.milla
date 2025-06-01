using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using OpenAI;

namespace agents.milla.Agents;

public class MillaAIChatAgent 
{
    public string Name => "Milla";
    public string Description => "Milla is a chat agent that can answer questions and help with tasks.";
    public string Version => "1.0.0";

    public async Task<string> GenerateResponseAsync(string input, CancellationToken cancellationToken = default)
    {
        try
        {
            var openAIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("Please set OPENAI_API_KEY environment variable.");
            var openAIClient = new OpenAIClient(openAIKey);
            var model = "gpt-4o-mini";

            var assistantAgent = new OpenAIChatAgent(
                name: "assistant",
                systemMessage: "You are an assistant that helps user with tasks.",
                chatClient: openAIClient.GetChatClient(model))
                .RegisterMessageConnector()
                .RegisterPrintMessage(); // register a hook to print message nicely to console

            // Create a simple response for now
            var response = await assistantAgent.GenerateReplyAsync(
                messages: new List<IMessage> { new TextMessage(Role.User, input, from: "user") },
                cancellationToken: cancellationToken);

            return response?.GetContent() ?? "I'm sorry, I couldn't process your request.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}