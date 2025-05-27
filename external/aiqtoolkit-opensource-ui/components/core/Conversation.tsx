import React, { useState, useRef, useEffect, useCallback } from 'react';
import { IconArrowDown } from '@tabler/icons-react';
import { Message } from './Message';
import { Input } from './Input';
import { Button } from './Button';

interface ConversationMessage {
  id: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  timestamp?: Date;
  isStreaming?: boolean;
}

interface ConversationProps {
  /** Array of messages */
  messages: ConversationMessage[];
  /** Send message handler */
  onSendMessage: (content: string) => void;
  /** Edit message handler */
  onEditMessage?: (messageId: string, content: string) => void;
  /** Delete message handler */
  onDeleteMessage?: (messageId: string) => void;
  /** File upload handler */
  onUpload?: (files: FileList) => void;
  /** Voice input handler */
  onVoiceInput?: () => void;
  /** Loading state */
  loading?: boolean;
  /** Custom input placeholder */
  placeholder?: string;
  /** Show welcome message */
  showWelcome?: boolean;
  /** Custom className */
  className?: string;
}

/**
 * Intelligent Conversation Component
 * 
 * Complete conversation interface with:
 * - Auto-scrolling message list with user override
 * - Seamless message streaming
 * - Smart input handling
 * - Progressive disclosure of features
 * - Excellent performance with large conversations
 */
export const Conversation: React.FC<ConversationProps> = ({
  messages,
  onSendMessage,
  onEditMessage,
  onDeleteMessage,
  onUpload,
  onVoiceInput,
  loading = false,
  placeholder = "Type your message...",
  showWelcome = true,
  className = ''
}) => {
  const [inputValue, setInputValue] = useState('');
  const [autoScroll, setAutoScroll] = useState(true);
  const [showScrollButton, setShowScrollButton] = useState(false);
  
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const scrollAreaRef = useRef<HTMLDivElement>(null);
  const lastScrollTop = useRef(0);

  // Auto-scroll to bottom when new messages arrive or streaming
  useEffect(() => {
    if (autoScroll) {
      messagesEndRef.current?.scrollIntoView({ 
        behavior: 'smooth',
        block: 'end'
      });
    }
  }, [messages, autoScroll]);

  // Handle scroll behavior
  const handleScroll = useCallback(() => {
    const scrollArea = scrollAreaRef.current;
    if (!scrollArea) return;

    const { scrollTop, scrollHeight, clientHeight } = scrollArea;
    const isAtBottom = scrollHeight - scrollTop - clientHeight < 50;
    const isScrollingUp = scrollTop < lastScrollTop.current;

    // Disable auto-scroll if user scrolls up
    if (isScrollingUp && autoScroll) {
      setAutoScroll(false);
      setShowScrollButton(true);
    }

    // Re-enable auto-scroll if user scrolls to bottom
    if (isAtBottom && !autoScroll) {
      setAutoScroll(true);
      setShowScrollButton(false);
    }

    lastScrollTop.current = scrollTop;
  }, [autoScroll]);

  // Throttled scroll handler
  useEffect(() => {
    const scrollArea = scrollAreaRef.current;
    if (!scrollArea) return;

    let timeoutId: NodeJS.Timeout;
    const throttledHandleScroll = () => {
      clearTimeout(timeoutId);
      timeoutId = setTimeout(handleScroll, 100);
    };

    scrollArea.addEventListener('scroll', throttledHandleScroll);
    return () => {
      scrollArea.removeEventListener('scroll', throttledHandleScroll);
      clearTimeout(timeoutId);
    };
  }, [handleScroll]);

  // Handle sending messages
  const handleSendMessage = (content: string) => {
    if (content.trim()) {
      onSendMessage(content);
      setInputValue('');
      setAutoScroll(true);
      setShowScrollButton(false);
    }
  };

  // Handle message editing
  const handleEditMessage = (messageId: string, content: string) => {
    onEditMessage?.(messageId, content);
  };

  // Handle message deletion
  const handleDeleteMessage = (messageId: string) => {
    onDeleteMessage?.(messageId);
  };

  // Scroll to bottom manually
  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ 
      behavior: 'smooth',
      block: 'end'
    });
    setAutoScroll(true);
    setShowScrollButton(false);
  };

  // Welcome component
  const WelcomeMessage = () => (
    <div className="flex-1 flex items-center justify-center p-8">
      <div className="text-center max-w-md">
        <div className="w-16 h-16 mx-auto mb-4 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center">
          <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
        </div>
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
          Welcome to AIQ Toolkit
        </h2>
        <p className="text-gray-600 dark:text-gray-400 mb-4">
          Start a conversation with our AI assistant. Ask questions, get help, or explore what's possible.
        </p>
        <div className="text-sm text-gray-500 dark:text-gray-500">
          <p>💡 <strong>Tip:</strong> Use ⌘+Enter to send messages quickly</p>
        </div>
      </div>
    </div>
  );

  return (
    <div className={`flex flex-col h-full ${className}`}>
      {/* Messages area */}
      <div 
        ref={scrollAreaRef}
        className="flex-1 overflow-y-auto px-4 py-6"
        onScroll={handleScroll}
      >
        {messages.length === 0 && showWelcome ? (
          <WelcomeMessage />
        ) : (
          <div className="max-w-4xl mx-auto space-y-6">
            {messages.map((message) => (
              <Message
                key={message.id}
                message={message}
                onEdit={(content) => handleEditMessage(message.id, content)}
                onDelete={() => handleDeleteMessage(message.id)}
                showActions={!message.isStreaming}
              />
            ))}
            
            {/* Loading indicator */}
            {loading && (
              <div className="flex items-center gap-3 p-4">
                <div className="w-8 h-8 bg-gray-200 rounded-full flex items-center justify-center">
                  <div className="w-2 h-2 bg-gray-400 rounded-full animate-pulse" />
                </div>
                <div className="bg-gray-100 rounded-2xl rounded-bl-md px-4 py-3">
                  <div className="flex gap-1">
                    <div className="w-2 h-2 bg-gray-400 rounded-full animate-pulse" style={{ animationDelay: '0ms' }} />
                    <div className="w-2 h-2 bg-gray-400 rounded-full animate-pulse" style={{ animationDelay: '150ms' }} />
                    <div className="w-2 h-2 bg-gray-400 rounded-full animate-pulse" style={{ animationDelay: '300ms' }} />
                  </div>
                </div>
              </div>
            )}
            
            {/* Scroll anchor */}
            <div ref={messagesEndRef} />
          </div>
        )}
      </div>

      {/* Scroll to bottom button */}
      {showScrollButton && (
        <div className="absolute bottom-32 right-8 z-10">
          <Button
            variant="primary"
            size="sm"
            onClick={scrollToBottom}
            className="h-10 w-10 p-0 rounded-full shadow-lg"
            title="Scroll to bottom"
          >
            <IconArrowDown size={18} />
          </Button>
        </div>
      )}

      {/* Input area */}
      <div className="border-t border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-4">
        <div className="max-w-4xl mx-auto">
          <Input
            value={inputValue}
            onChange={setInputValue}
            onSubmit={handleSendMessage}
            onUpload={onUpload}
            onVoiceInput={onVoiceInput}
            loading={loading}
            placeholder={placeholder}
            autoFocus={messages.length === 0}
            showCount={inputValue.length > 3000}
          />
          
          {/* Input hints */}
          {inputValue.length === 0 && (
            <div className="mt-2 text-xs text-gray-500 text-center">
              Press <kbd className="px-1 py-0.5 bg-gray-100 rounded text-xs">Enter</kbd> to send • 
              <kbd className="px-1 py-0.5 bg-gray-100 rounded text-xs mx-1">⌘U</kbd> to upload • 
              <kbd className="px-1 py-0.5 bg-gray-100 rounded text-xs">⌘M</kbd> for voice
            </div>
          )}
        </div>
      </div>
    </div>
  );
};