import React, { Component } from 'react';
import { Button, Form, FormGroup, Input, Alert, Card, CardBody, CardHeader, Badge } from 'reactstrap';
import './DealDeskChat.css';

export class DealDeskChat extends Component {
  constructor(props) {
    super(props);
    
    this.state = {
      accessCode: '',
      message: '',
      messages: [],
      isConnected: false,
      isLoading: false,
      error: null,
      showAccessForm: true,
      eventSource: null
    };
  }

  componentWillUnmount() {
    if (this.state.eventSource) {
      this.state.eventSource.close();
    }
  }

  handleAccessCodeSubmit = (e) => {
    e.preventDefault();
    if (this.state.accessCode.trim()) {
      this.setState({ showAccessForm: false });
    }
  };

  setExampleMessage = (message) => {
    this.setState({ message });
  };

  handleMessageSubmit = async (e) => {
    e.preventDefault();
    
    if (!this.state.message.trim()) return;

    const userMessage = {
      id: Date.now(),
      speaker: 'You',
      text: this.state.message,
      timestamp: new Date(),
      isUser: true
    };

    this.setState({
      messages: [...this.state.messages, userMessage],
      isLoading: true,
      error: null,
      message: ''
    });

    try {
      const response = await fetch('/api/dealdesk/chat', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          message: userMessage.text,
          accessCode: this.state.accessCode
        })
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Failed to send message');
      }

      // Read the streaming response
      const reader = response.body.getReader();
      const decoder = new TextDecoder();

      while (true) {
        const { done, value } = await reader.read();
        
        if (done) {
          this.setState({ isLoading: false, isConnected: false });
          break;
        }

        const chunk = decoder.decode(value);
        const lines = chunk.split('\n');

        for (const line of lines) {
          if (line.startsWith('data: ')) {
            const jsonStr = line.substring(6);
            if (jsonStr.trim()) {
              try {
                const data = JSON.parse(jsonStr);
                
                if (data.type === 'complete') {
                  this.setState({ isLoading: false, isConnected: false });
                  continue;
                }

                const agentMessage = {
                  id: `${data.speaker}-${Date.now()}-${Math.random()}`,
                  speaker: data.speaker,
                  text: data.text,
                  status: data.status,
                  timestamp: new Date(),
                  isUser: false
                };

                this.setState(prevState => ({
                  messages: [...prevState.messages, agentMessage]
                }));

              } catch (error) {
                console.error('Error parsing SSE data:', error);
              }
            }
          }
        }
      }

    } catch (error) {
      console.error('Error sending message:', error);
      this.setState({
        error: error.message,
        isLoading: false
      });
    }
  };

  getSpeakerColor = (speaker) => {
    const colors = {
      'PricingAgent': 'primary',
      'LegalAgent': 'warning', 
      'FinanceAgent': 'success',
      'VPApprovalAgent': 'danger',
      'SalesRepProxyAgent': 'info',
      'System': 'dark'
    };
    return colors[speaker] || 'secondary';
  };

  render() {
    if (this.state.showAccessForm) {
      return (
        <div className="access-form-container">
          <Card className="access-card">
            <CardHeader>
              <h4>🏢 AI Deal Desk Assistant</h4>
              <p className="mb-0">Enter access code to continue</p>
            </CardHeader>
            <CardBody>
              <Form onSubmit={this.handleAccessCodeSubmit}>
                <FormGroup>
                  <Input
                    type="password"
                    placeholder="Access Code"
                    value={this.state.accessCode}
                    onChange={(e) => this.setState({ accessCode: e.target.value })}
                    required
                  />
                </FormGroup>
                <Button color="primary" type="submit" block>
                  Access Chat
                </Button>
              </Form>
            </CardBody>
          </Card>
        </div>
      );
    }

    return (
      <div className="deal-desk-chat">
        <Card className="chat-card">
          <CardHeader>
            <h4>🤖 AI Deal Desk Assistant</h4>
            <p className="mb-0">Chat with our AI agents to generate deal proposals and get expert advice</p>
          </CardHeader>
          <CardBody>
            {this.state.error && (
              <Alert color="danger" className="mb-3">
                {this.state.error}
              </Alert>
            )}

            <div className="messages-container">
              {this.state.messages.length === 0 && (
                <div className="chat-welcome">
                  <h5>🤖 Welcome to AI Deal Desk Assistant!</h5>
                  <p>Our AI agents can help you with deal proposals, pricing analysis, legal review, and strategic decisions.</p>
                  
                  <div className="example-buttons">
                    <h6>Try these examples:</h6>
                    <button 
                      className="btn btn-outline-primary btn-sm mb-2"
                      onClick={() => this.setExampleMessage("Generate a comprehensive deal proposal for ACME Corp, 250 seats, 2-year contract, requesting 20% discount")}
                    >
                      📋 Generate Deal Proposal
                    </button>
                    <button 
                      className="btn btn-outline-info btn-sm mb-2"
                      onClick={() => this.setExampleMessage("What are the standard discount rates for enterprise deals over $500K?")}
                    >
                      💰 Pricing Guidance
                    </button>
                    <button 
                      className="btn btn-outline-warning btn-sm mb-2"
                      onClick={() => this.setExampleMessage("Review the legal risks of a 3-year contract with early termination clauses")}
                    >
                      ⚖️ Legal Analysis
                    </button>
                    <button 
                      className="btn btn-outline-success btn-sm mb-2"
                      onClick={() => this.setExampleMessage("Calculate the financial impact and ROI for a $750K annual deal")}
                    >
                      📊 Financial Analysis
                    </button>
                  </div>

                  <div className="agent-intro">
                    <h6>Meet our AI experts:</h6>
                    <div className="row">
                      <div className="col-md-6">
                        <small><strong>💼 PricingAgent:</strong> Pricing strategy & discounts</small><br/>
                        <small><strong>⚖️ LegalAgent:</strong> Contract terms & compliance</small><br/>
                        <small><strong>📊 FinanceAgent:</strong> ROI & financial projections</small>
                      </div>
                      <div className="col-md-6">
                        <small><strong>🏆 VPAgent:</strong> Strategic decisions & approvals</small><br/>
                        <small><strong>🤝 SalesRepAgent:</strong> Customer relationships</small><br/>
                        <small><strong>🤖 General Assistant:</strong> Other questions</small>
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {this.state.messages.map((msg) => (
                <div key={msg.id} className={`message ${msg.isUser ? 'user-message' : 'agent-message'}`}>
                  {!msg.isUser && (
                    <div className="agent-header">
                      <Badge color={this.getSpeakerColor(msg.speaker)}>
                        {msg.speaker}
                      </Badge>
                      <small className="timestamp">
                        {msg.timestamp.toLocaleTimeString()}
                      </small>
                    </div>
                  )}
                  <div className="message-content">
                    <pre className="message-text">{msg.text}</pre>
                  </div>
                </div>
              ))}

              {this.state.isLoading && (
                <div className="message agent-message">
                  <div className="agent-header">
                    <Badge color="info">AI Agents</Badge>
                  </div>
                  <div className="message-content">
                    <div className="typing-indicator">
                      <span></span>
                      <span></span>
                      <span></span>
                    </div>
                  </div>
                </div>
              )}
            </div>

            <Form onSubmit={this.handleMessageSubmit} className="message-form">
              <FormGroup className="message-input-group">
                <Input
                  type="textarea"
                  placeholder="Ask about deal proposals, pricing, legal terms, or any deal-related questions..."
                  value={this.state.message}
                  onChange={(e) => this.setState({ message: e.target.value })}
                  disabled={this.state.isLoading}
                  rows="3"
                />
                <Button 
                  color="primary" 
                  type="submit" 
                  disabled={this.state.isLoading || !this.state.message.trim()}
                >
                  {this.state.isLoading ? 'Processing...' : 'Send'}
                </Button>
              </FormGroup>
            </Form>
          </CardBody>
        </Card>
      </div>
    );
  }
}