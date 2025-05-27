'use client';

import React, { useState, useEffect, useRef } from 'react';
import axios from 'axios';

export default function DigitalHumanPage() {
  const [messages, setMessages] = useState<Array<{sender: string, text: string, type: string}>>([]);
  const [input, setInput] = useState('');
  const [isConnected, setIsConnected] = useState(false);
  const [sources, setSources] = useState<string[]>([]);
  const [context, setContext] = useState<any>({});
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // Initial message
    setMessages([{
      sender: 'AI',
      text: 'Welcome! I\'m your NVIDIA-powered digital human assistant integrated with LangChain.',
      type: 'ai'
    }]);
    setIsConnected(true);
  }, []);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const sendMessage = async () => {
    if (!input.trim()) return;

    const userMessage = { sender: 'You', text: input, type: 'user' };
    setMessages(prev => [...prev, userMessage]);
    setInput('');

    try {
      const response = await axios.post('/api/chat', {
        query: input,
        use_mcp: true,
        use_brev: true
      });

      const { response: aiResponse, sources_used, context: responseContext } = response.data;

      setMessages(prev => [...prev, {
        sender: 'AI',
        text: aiResponse,
        type: 'ai'
      }]);

      setSources(sources_used);
      setContext(responseContext);

    } catch (error) {
      setMessages(prev => [...prev, {
        sender: 'Error',
        text: 'Failed to process request',
        type: 'error'
      }]);
    }
  };

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <div className="container mx-auto max-w-7xl p-6">
        <div className="mb-8 text-center">
          <h1 className="text-4xl font-bold mb-2">Digital Human - Vercel Deployment</h1>
          <p className="text-gray-400">NVIDIA AI + LangChain + Brev Integration</p>
          <div className="mt-4 inline-block bg-green-600 px-4 py-2 rounded-lg">
            Status: {isConnected ? 'Connected' : 'Disconnected'}
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="bg-gray-800 rounded-lg p-6">
            <h2 className="text-2xl font-semibold mb-4">Chat Interface</h2>
            <div className="bg-gray-900 rounded-lg p-4 h-96 overflow-y-auto mb-4">
              {messages.map((msg, idx) => (
                <div key={idx} className={`mb-4 ${msg.type === 'user' ? 'ml-auto' : ''} max-w-[80%]`}>
                  <div className={`p-3 rounded-lg ${
                    msg.type === 'user' ? 'bg-blue-600' : 
                    msg.type === 'error' ? 'bg-red-600' : 'bg-gray-700'
                  }`}>
                    <strong>{msg.sender}:</strong> {msg.text}
                  </div>
                </div>
              ))}
              <div ref={messagesEndRef} />
            </div>
            
            <div className="flex gap-2">
              <input
                type="text"
                value={input}
                onChange={(e) => setInput(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && sendMessage()}
                className="flex-1 bg-gray-700 text-white px-4 py-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-500"
                placeholder="Ask me anything..."
              />
              <button
                onClick={sendMessage}
                className="bg-green-600 hover:bg-green-700 px-6 py-2 rounded-lg font-semibold transition"
              >
                Send
              </button>
            </div>
          </div>

          <div className="bg-gray-800 rounded-lg p-6">
            <h2 className="text-2xl font-semibold mb-4">System Status</h2>
            
            <div className="mb-6">
              <h3 className="text-lg font-semibold mb-2">Active Sources:</h3>
              <div className="flex flex-wrap gap-2">
                {['NVIDIA NIM', 'MCP Web Search', 'File Browser', 'LangChain', 'Brev Integration'].map(source => (
                  <span key={source} className="bg-green-600 px-3 py-1 rounded-full text-sm">
                    {source}
                  </span>
                ))}
              </div>
            </div>

            <div className="mb-6">
              <h3 className="text-lg font-semibold mb-2">Last Query Sources:</h3>
              <div className="flex flex-wrap gap-2">
                {sources.map((source, idx) => (
                  <span key={idx} className="bg-blue-600 px-3 py-1 rounded-full text-sm">
                    {source}
                  </span>
                ))}
              </div>
            </div>

            <div>
              <h3 className="text-lg font-semibold mb-2">Context:</h3>
              <pre className="bg-gray-900 p-4 rounded-lg text-xs overflow-x-auto">
                {JSON.stringify(context, null, 2)}
              </pre>
            </div>
          </div>
        </div>

        <div className="mt-8 text-center text-gray-400">
          <p>Powered by NVIDIA ACE • Deployed on Vercel • Integrated with Brev LangChain</p>
        </div>
      </div>
    </div>
  );
}