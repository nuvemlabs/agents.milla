🏢 Cursor AI Prompt – "AI Deal Desk Assistant" Demo using AutoGen for .NET + React

Paste this entire block into Cursor AI. It will scaffold a multi‑agent "deal‑desk" workflow inside your existing React + ASP.NET Core solution.

🎯 Goal

Transform the current project into an internal AI Deal Desk Assistant that helps sales reps generate quotes, vet discount requests, and obtain fast approvals, showcasing the orchestration power of AutoGen for .NET.

🧩 Context

Repo already contains:

/YourCompany.Api – ASP.NET Core 8 Web API

/ClientApp – React 18 front‑end (Vite or CRA)

You can install NuGet   &   npm packages.

AutoGen docs: https://microsoft.github.io/autogen-for-net/index.html

📝 You will

Study the AutoGen docs & the TravelPlanner sample for multi‑agent composition patterns.

Install packages

dotnet add YourCompany.Api package AutoGen
dotnet add YourCompany.Api package AutoGen.OpenAI

Create agents under YourCompany.Api/Services/Agents:

File

Inherits

Role

SalesRepProxyAgent.cs

UserProxyAgent

Forwards chat from the human rep

PricingAgent.cs

AssistantAgent

Calculates list price, applies discount policy

LegalAgent.cs

AssistantAgent

Flags risky terms (e.g., indemnity, SLA)

FinanceAgent.cs

AssistantAgent

Checks margin & ARR impact

VPApprovalAgent.cs

AssistantAgent

Grants / denies final approval based on inputs

Tip: Keep each agent’s SystemMessage concise & use the new ToolCallBehaviors feature for synchronous JSON calls.

Compose the group chat in DealDeskOrchestrator.cs:

var chat = new GroupChat(
    agents: new IAgent[]
    {
        salesRepProxy,
        pricingAgent,
        legalAgent,
        financeAgent,
        vpApprovalAgent
    },
    maxRounds: 6);

Exposed API

POST /api/dealdesk/chat  →  { "message": "Need 15% discount for ACME, 3‑year term" }

Streams JSON chunks: { speaker, text, approvals, blockers? }

React UI (/ClientApp/src/components/DealDeskChat.tsx)

Chat timeline grouped by agent

Sticky Status Panel summarising margin %, legal risk score, approval state

Toast success once VP approval is granted

Configuration

Read OPENAI_API_KEY + optional AZURE_OPENAI_ENDPOINT from env or user‑secrets.

Seed example (for quick demo)

{
  "message": "Customer Globex wants 20% discount on 250 seats, 2‑year term with 30‑day payment."  
}

README section → local run steps (dotnet run; npm run dev).

✅ Acceptance checklist



🤖 Cursor AI Tips

Use async streams for chat streaming (await foreach).

Guard sensitive fields with [JsonIgnore] where needed.

Lean on the docs’ serialization helpers for typed tool calls.

To experiment fast, attach Swagger to inspect payloads.

Close deals faster, with fewer back‑and‑forths. 💼✨
