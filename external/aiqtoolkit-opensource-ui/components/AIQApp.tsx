import React, { useState, useEffect } from 'react';
import { 
  Layout, 
  Conversation, 
  Panel,
  Button 
} from './core';
import { 
  IconPlus, 
  IconHistory, 
  IconSettings,
  IconBrain,
  IconDatabase,
  IconCloud
} from '@tabler/icons-react';

interface ConversationData {
  id: string;
  title: string;
  lastMessage?: string;
  timestamp: Date;
}

interface MessageData {
  id: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  timestamp: Date;
  isStreaming?: boolean;
}

/**
 * AIQToolkit Main Application
 * 
 * Showcases the new design system with:
 * - Clean, purposeful interface
 * - Intelligent component composition
 * - Smooth interactions and transitions
 * - Perfect accessibility
 * - Jony Ive-inspired simplicity
 */
export const AIQApp: React.FC = () => {
  const [conversations, setConversations] = useState<ConversationData[]>([]);
  const [currentConversation, setCurrentConversation] = useState<string | null>(null);
  const [messages, setMessages] = useState<MessageData[]>([]);
  const [loading, setLoading] = useState(false);

  // Sample conversations for demo
  useEffect(() => {
    const sampleConversations: ConversationData[] = [
      {
        id: '1',
        title: 'Getting Started with AIQ',
        lastMessage: 'Here are the key features you should know about...',
        timestamp: new Date(Date.now() - 1000 * 60 * 30) // 30 minutes ago
      },
      {
        id: '2', 
        title: 'NVIDIA Integration Setup',
        lastMessage: 'Your GPU acceleration is now configured correctly.',
        timestamp: new Date(Date.now() - 1000 * 60 * 60 * 2) // 2 hours ago
      },
      {
        id: '3',
        title: 'Digital Human Configuration',
        lastMessage: 'The avatar animation system is ready to use.',
        timestamp: new Date(Date.now() - 1000 * 60 * 60 * 24) // 1 day ago
      }
    ];
    setConversations(sampleConversations);
  }, []);

  // Load conversation messages
  useEffect(() => {
    if (currentConversation) {
      // Simulate loading conversation
      setLoading(true);
      setTimeout(() => {
        const sampleMessages: MessageData[] = [
          {
            id: '1',
            role: 'assistant',
            content: 'Hello! I\'m your AIQToolkit assistant. I can help you with:\n\n• **GPU-accelerated workflows** using NVIDIA technology\n• **Digital human interfaces** with real-time animation\n• **Multi-agent consensus** systems\n• **Production deployment** and scaling\n\nWhat would you like to explore first?',
            timestamp: new Date(Date.now() - 1000 * 60 * 5)
          }
        ];
        setMessages(sampleMessages);
        setLoading(false);
      }, 500);
    } else {
      setMessages([]);
    }
  }, [currentConversation]);

  // Create new conversation
  const createNewConversation = () => {
    const newConversation: ConversationData = {
      id: Date.now().toString(),
      title: 'New Conversation',
      timestamp: new Date()
    };
    setConversations([newConversation, ...conversations]);
    setCurrentConversation(newConversation.id);
  };

  // Send message
  const handleSendMessage = async (content: string) => {
    if (!currentConversation) {
      createNewConversation();
    }

    const userMessage: MessageData = {
      id: Date.now().toString(),
      role: 'user',
      content,
      timestamp: new Date()
    };

    setMessages(prev => [...prev, userMessage]);
    setLoading(true);

    // Simulate AI response
    setTimeout(() => {
      const aiMessage: MessageData = {
        id: (Date.now() + 1).toString(),
        role: 'assistant',
        content: generateAIResponse(content),
        timestamp: new Date()
      };
      setMessages(prev => [...prev, aiMessage]);
      setLoading(false);
    }, 1500);
  };

  // Generate AI response (demo)
  const generateAIResponse = (userInput: string): string => {
    const responses = [
      `That's a great question about "${userInput}". Let me break this down for you:\n\n**Key Points:**\n• AIQToolkit provides GPU acceleration for 10-13x performance gains\n• Our framework-agnostic approach works with existing infrastructure\n• Digital humans offer engaging user experiences\n\n**Next Steps:**\n1. Review the documentation\n2. Try the example workflows\n3. Configure your environment\n\nWould you like me to elaborate on any of these points?`,
      
      `Regarding "${userInput}", here's what you need to know:\n\n**Technical Overview:**\n• NVIDIA GPU optimization with custom CUDA kernels\n• Real-time consensus algorithms using Nash equilibrium\n• Production-ready deployment with monitoring\n\n**Benefits:**\n• 12.8x faster similarity computation\n• 11.7x speedup in Nash equilibrium solving\n• Enterprise-grade security and scalability\n\nHow can I help you implement this?`,
      
      `I understand you're asking about "${userInput}". Here's my analysis:\n\n**Current Capabilities:**\n• Multi-framework agent orchestration\n• GPU-accelerated financial analysis\n• Real-time digital human interfaces\n\n**Integration Options:**\n• REST API endpoints\n• WebSocket real-time communication\n• Docker/Kubernetes deployment\n\nWhat specific use case are you working on?`
    ];
    
    return responses[Math.floor(Math.random() * responses.length)];
  };

  // Edit message
  const handleEditMessage = (messageId: string, content: string) => {
    setMessages(prev => prev.map(msg => 
      msg.id === messageId ? { ...msg, content } : msg
    ));
  };

  // Delete message
  const handleDeleteMessage = (messageId: string) => {
    setMessages(prev => prev.filter(msg => msg.id !== messageId));
  };

  // Sidebar content
  const sidebarContent = (
    <div className="space-y-6">
      {/* New conversation button */}
      <Button
        variant="primary"
        fullWidth
        icon={<IconPlus size={20} />}
        onClick={createNewConversation}
      >
        New Conversation
      </Button>

      {/* Recent conversations */}
      <Panel title="Recent Conversations" size="sm">
        <div className="space-y-2">
          {conversations.map(conv => (
            <button
              key={conv.id}
              onClick={() => setCurrentConversation(conv.id)}
              className={`
                w-full text-left p-3 rounded-lg transition-colors
                ${currentConversation === conv.id 
                  ? 'bg-blue-100 dark:bg-blue-900/20 text-blue-900 dark:text-blue-100' 
                  : 'hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300'
                }
              `}
            >
              <div className="font-medium text-sm truncate mb-1">
                {conv.title}
              </div>
              {conv.lastMessage && (
                <div className="text-xs opacity-75 truncate">
                  {conv.lastMessage}
                </div>
              )}
              <div className="text-xs opacity-50 mt-1">
                {conv.timestamp.toLocaleDateString()}
              </div>
            </button>
          ))}
        </div>
      </Panel>

      {/* System status */}
      <Panel title="System Status" variant="subtle" size="sm">
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <IconBrain size={16} className="text-green-600" />
              <span className="text-sm">AI Models</span>
            </div>
            <span className="text-xs text-green-600 font-medium">Online</span>
          </div>
          
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <IconDatabase size={16} className="text-blue-600" />
              <span className="text-sm">GPU Acceleration</span>
            </div>
            <span className="text-xs text-blue-600 font-medium">Active</span>
          </div>
          
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <IconCloud size={16} className="text-purple-600" />
              <span className="text-sm">Cloud Services</span>
            </div>
            <span className="text-xs text-purple-600 font-medium">Connected</span>
          </div>
        </div>
      </Panel>

      {/* Quick actions */}
      <Panel title="Quick Actions" size="sm">
        <div className="space-y-2">
          <Button variant="ghost" size="sm" fullWidth icon={<IconHistory size={16} />}>
            View All Conversations
          </Button>
          <Button variant="ghost" size="sm" fullWidth icon={<IconSettings size={16} />}>
            Settings & Preferences
          </Button>
        </div>
      </Panel>
    </div>
  );

  // Header content
  const headerContent = (
    <div className="flex-1">
      <h1 className="text-lg font-semibold text-gray-900 dark:text-white">
        AIQToolkit Assistant
      </h1>
      <p className="text-sm text-gray-600 dark:text-gray-400">
        Intelligent agent orchestration platform
      </p>
    </div>
  );

  return (
    <Layout
      sidebar={sidebarContent}
      header={headerContent}
      sidebarOpen={true}
      className="h-screen"
    >
      <div className="h-full">
        {currentConversation ? (
          <Conversation
            messages={messages}
            onSendMessage={handleSendMessage}
            onEditMessage={handleEditMessage}
            onDeleteMessage={handleDeleteMessage}
            loading={loading}
            placeholder="Ask me anything about AIQToolkit..."
            className="h-full"
          />
        ) : (
          <div className="h-full flex items-center justify-center">
            <div className="text-center max-w-md">
              <div className="w-20 h-20 mx-auto mb-6 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center">
                <IconBrain size={32} className="text-white" />
              </div>
              <h2 className="text-2xl font-semibold text-gray-900 dark:text-white mb-4">
                Welcome to AIQToolkit
              </h2>
              <p className="text-gray-600 dark:text-gray-400 mb-6">
                Your intelligent agent orchestration platform. Create a new conversation to get started.
              </p>
              <Button
                variant="primary"
                size="lg"
                onClick={createNewConversation}
                icon={<IconPlus size={20} />}
              >
                Start New Conversation
              </Button>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};