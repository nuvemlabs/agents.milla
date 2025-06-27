import React, { Component } from 'react';

export class Home extends Component {
  static displayName = Home.name;

  render() {
    return (
      <div className="container">
        <div className="row justify-content-center">
          <div className="col-md-8">
            <h1>🏢 AI Deal Desk Assistant</h1>
            <p className="lead">Intelligent deal proposal generation powered by AI agents</p>
            
            <div className="card mb-4">
              <div className="card-body">
                <h5 className="card-title">About This Demo</h5>
                <p className="card-text">
                  This AI-powered deal desk assistant uses multiple specialized AI agents to help sales teams 
                  generate comprehensive deal proposals, analyze pricing strategies, review legal terms, 
                  and make strategic decisions.
                </p>
              </div>
            </div>

            <div className="row">
              <div className="col-md-6">
                <h6>🤖 AI Agents</h6>
                <ul className="list-unstyled">
                  <li><strong>💼 Pricing Agent</strong> - Pricing strategy & discount analysis</li>
                  <li><strong>⚖️ Legal Agent</strong> - Contract terms & compliance review</li>
                  <li><strong>📊 Finance Agent</strong> - ROI analysis & financial projections</li>
                  <li><strong>🏆 VP Agent</strong> - Strategic decisions & executive recommendations</li>
                  <li><strong>🤝 Sales Rep Agent</strong> - Customer relationship management</li>
                </ul>
              </div>
              <div className="col-md-6">
                <h6>🛠️ Technology Stack</h6>
                <ul className="list-unstyled">
                  <li><strong>Backend:</strong> ASP.NET Core 7.0</li>
                  <li><strong>Frontend:</strong> React 18</li>
                  <li><strong>AI Framework:</strong> Microsoft AutoGen</li>
                  <li><strong>AI Provider:</strong> OpenAI GPT-4</li>
                  <li><strong>Deployment:</strong> Azure Web App</li>
                </ul>
              </div>
            </div>

            <div className="alert alert-info mt-4">
              <h6>🚀 Get Started</h6>
              <p className="mb-0">
                Head to the <strong>Deal Chat</strong> page to start interacting with our AI agents. 
                Try asking for deal proposals, pricing guidance, or strategic advice!
              </p>
            </div>

            <div className="mt-4">
              <h6>📖 Example Use Cases</h6>
              <ul>
                <li>Generate comprehensive deal proposals with pricing, legal terms, and financial projections</li>
                <li>Get pricing recommendations for complex enterprise deals</li>
                <li>Analyze legal risks and compliance requirements</li>
                <li>Evaluate financial impact and ROI of proposed deals</li>
                <li>Receive executive-level strategic recommendations</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    );
  }
}
