import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000';

export default function Home() {
  const [messages, setMessages] = useState([]);
  const [inputValue, setInputValue] = useState('');
  const [selectedModel, setSelectedModel] = useState('gpt-3.5-turbo');
  const [models, setModels] = useState([]);
  const [workflows, setWorkflows] = useState([]);

  useEffect(() => {
    // Load available models
    axios.get(`${API_URL}/v1/models`)
      .then(res => setModels(res.data.models))
      .catch(err => console.error('Error loading models:', err));

    // Load available workflows
    axios.get(`${API_URL}/v1/workflows`)
      .then(res => setWorkflows(res.data.workflows))
      .catch(err => console.error('Error loading workflows:', err));
  }, []);

  const sendMessage = async () => {
    if (!inputValue.trim()) return;

    const newMessage = { role: 'user', content: inputValue };
    setMessages([...messages, newMessage]);
    setInputValue('');

    try {
      const response = await axios.post(`${API_URL}/v1/chat/completions`, {
        messages: [...messages, newMessage],
        model: selectedModel,
        temperature: 0.7
      });

      const assistantMessage = response.data.choices[0].message;
      setMessages(prev => [...prev, assistantMessage]);
    } catch (error) {
      console.error('Error sending message:', error);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-4xl mx-auto p-4">
        <h1 className="text-3xl font-bold text-center mb-8">AIQToolkit Standard UI</h1>
        
        {/* Model Selection */}
        <div className="mb-4">
          <label className="block text-sm font-medium mb-2">Model:</label>
          <select 
            value={selectedModel}
            onChange={(e) => setSelectedModel(e.target.value)}
            className="w-full p-2 border rounded"
          >
            {models.map(model => (
              <option key={model.id} value={model.id}>
                {model.id}
              </option>
            ))}
          </select>
        </div>

        {/* Chat Interface */}
        <div className="bg-white rounded-lg shadow-md p-4">
          <div className="h-96 overflow-y-auto mb-4 p-4 border rounded">
            {messages.map((msg, idx) => (
              <div key={idx} className={`mb-4 ${msg.role === 'user' ? 'text-right' : 'text-left'}`}>
                <div className={`inline-block p-3 rounded ${
                  msg.role === 'user' ? 'bg-blue-500 text-white' : 'bg-gray-200'
                }`}>
                  {msg.content}
                </div>
              </div>
            ))}
          </div>

          <div className="flex gap-2">
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && sendMessage()}
              className="flex-1 p-2 border rounded"
              placeholder="Type your message..."
            />
            <button
              onClick={sendMessage}
              className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
            >
              Send
            </button>
          </div>
        </div>

        {/* Workflows */}
        <div className="mt-8">
          <h2 className="text-xl font-semibold mb-4">Available Workflows</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {workflows.map(workflow => (
              <div key={workflow.id} className="bg-white p-4 rounded shadow">
                <h3 className="font-semibold">{workflow.name}</h3>
                <p className="text-gray-600">{workflow.description}</p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}