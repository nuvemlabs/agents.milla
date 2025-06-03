using AutoGen;
using AutoGen.OpenAI;

namespace agents.milla.Services.Agents;

public class SalesRepProxyAgent : UserProxyAgent
{
    public SalesRepProxyAgent() : base(
        name: "SalesRep",
        humanInputMode: HumanInputMode.NEVER)
    {
        // Agent is ready to use
    }

    public static SalesRepProxyAgent Create()
    {
        var agent = new SalesRepProxyAgent();
        return agent;
    }
} 