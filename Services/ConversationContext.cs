using AutoGen.Core;

namespace agents.milla.Services;

public class ConversationContext
{
    private readonly List<IMessage> _messages = new();
    private readonly object _lock = new();
    public string SessionId { get; }

    public ConversationContext(string sessionId)
    {
        SessionId = sessionId;
    }

    public void AddMessage(IMessage message)
    {
        lock (_lock)
        {
            _messages.Add(message);
            
            // Keep only last 20 messages to manage context window
            if (_messages.Count > 20)
            {
                _messages.RemoveAt(0);
            }
        }
    }

    public void AddUserMessage(string content)
    {
        var message = new TextMessage(Role.User, content, from: "user");
        AddMessage(message);
    }

    public void AddAgentMessage(string agentName, string content)
    {
        var message = new TextMessage(Role.Assistant, content, from: agentName);
        AddMessage(message);
    }

    public List<IMessage> GetMessages()
    {
        lock (_lock)
        {
            return new List<IMessage>(_messages);
        }
    }

    public List<IMessage> GetRecentMessages(int count = 10)
    {
        lock (_lock)
        {
            return _messages.TakeLast(count).ToList();
        }
    }

    public string GetContextSummary()
    {
        lock (_lock)
        {
            if (!_messages.Any()) return "No previous conversation";

            var recentMessages = _messages.TakeLast(5);
            var summary = string.Join("\n", recentMessages.Select(m => 
                $"{m.From}: {m.GetContent()?.Substring(0, Math.Min(100, m.GetContent()?.Length ?? 0))}..."));
            
            return $"Recent conversation:\n{summary}";
        }
    }
}

public class ConversationManager
{
    private readonly Dictionary<string, ConversationContext> _contexts = new();
    private readonly object _lock = new();
    private readonly ILogger<ConversationManager> _logger;

    public ConversationManager(ILogger<ConversationManager> logger)
    {
        _logger = logger;
    }

    public ConversationContext GetOrCreateContext(string sessionId)
    {
        lock (_lock)
        {
            if (!_contexts.ContainsKey(sessionId))
            {
                _contexts[sessionId] = new ConversationContext(sessionId);
                _logger.LogInformation("Created new conversation context for session: {SessionId}", sessionId);
            }

            return _contexts[sessionId];
        }
    }

    public void ClearContext(string sessionId)
    {
        lock (_lock)
        {
            if (_contexts.ContainsKey(sessionId))
            {
                _contexts.Remove(sessionId);
                _logger.LogInformation("Cleared conversation context for session: {SessionId}", sessionId);
            }
        }
    }

    public void CleanupOldContexts(TimeSpan maxAge)
    {
        // This would be implemented with timestamp tracking for production
        // For now, keep all contexts as this is a demo
    }
}