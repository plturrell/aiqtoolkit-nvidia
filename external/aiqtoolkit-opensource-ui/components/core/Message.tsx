import React, { useState, useRef, useEffect } from 'react';
import { IconCopy, IconCheck, IconVolume2, IconPlayerPause, IconEdit, IconTrash } from '@tabler/icons-react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { Avatar } from './Avatar';
import { Button } from './Button';

interface MessageProps {
  /** Message data */
  message: {
    id?: string;
    role: 'user' | 'assistant' | 'system';
    content: string;
    timestamp?: Date;
    isStreaming?: boolean;
  };
  /** Message actions */
  onEdit?: (content: string) => void;
  onDelete?: () => void;
  onSpeak?: (content: string) => void;
  /** Visual state */
  showActions?: boolean;
  className?: string;
}

/**
 * Intelligent Message Component
 * 
 * Handles all message types with contextual intelligence:
 * - Automatic layout based on message role
 * - Progressive disclosure of actions
 * - Built-in editing capabilities
 * - Accessibility-first design
 * - Smooth, purposeful animations
 */
export const Message: React.FC<MessageProps> = ({
  message,
  onEdit,
  onDelete,
  onSpeak,
  showActions = true,
  className = ''
}) => {
  const [isEditing, setIsEditing] = useState(false);
  const [editContent, setEditContent] = useState(message.content);
  const [isCopied, setIsCopied] = useState(false);
  const [isSpeaking, setIsSpeaking] = useState(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Auto-resize textarea
  useEffect(() => {
    if (isEditing && textareaRef.current) {
      const textarea = textareaRef.current;
      textarea.style.height = 'auto';
      textarea.style.height = `${textarea.scrollHeight}px`;
    }
  }, [isEditing, editContent]);

  // Handle copy action
  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(message.content);
      setIsCopied(true);
      setTimeout(() => setIsCopied(false), 2000);
    } catch (error) {
      console.error('Copy failed:', error);
    }
  };

  // Handle text-to-speech
  const handleSpeak = () => {
    if ('speechSynthesis' in window) {
      if (isSpeaking) {
        window.speechSynthesis.cancel();
        setIsSpeaking(false);
      } else {
        const utterance = new SpeechSynthesisUtterance(message.content);
        utterance.onend = () => setIsSpeaking(false);
        utterance.onerror = () => setIsSpeaking(false);
        setIsSpeaking(true);
        window.speechSynthesis.speak(utterance);
      }
    }
    onSpeak?.(message.content);
  };

  // Handle edit save
  const handleSaveEdit = () => {
    if (editContent.trim() !== message.content) {
      onEdit?.(editContent.trim());
    }
    setIsEditing(false);
  };

  // Handle edit cancel
  const handleCancelEdit = () => {
    setEditContent(message.content);
    setIsEditing(false);
  };

  // Message layout classes based on role
  const isUser = message.role === 'user';
  const messageClasses = `
    group relative
    ${isUser ? 'ml-auto max-w-[85%]' : 'mr-auto max-w-[95%]'}
    animate-fade-in
  `;

  // Content container classes
  const contentClasses = `
    flex gap-3 p-4
    ${isUser ? 'flex-row-reverse' : 'flex-row'}
  `;

  // Message bubble classes
  const bubbleClasses = `
    relative px-4 py-3 rounded-2xl
    ${isUser 
      ? 'bg-blue-600 text-white rounded-br-md' 
      : 'bg-gray-100 text-gray-900 rounded-bl-md'
    }
    ${message.isStreaming ? 'animate-pulse-subtle' : ''}
  `;

  // Actions container
  const ActionsContainer = ({ children }: { children: React.ReactNode }) => (
    <div className={`
      flex items-center gap-1 mt-2 opacity-0 group-hover:opacity-100
      transition-opacity duration-200
      ${isUser ? 'justify-end' : 'justify-start'}
    `}>
      {children}
    </div>
  );

  return (
    <div className={`${messageClasses} ${className}`}>
      <div className={contentClasses}>
        {/* Avatar */}
        <Avatar 
          user={{ role: message.role }} 
          size="md" 
          variant="conversation"
        />

        {/* Message Content */}
        <div className="flex-1 min-w-0">
          {isEditing ? (
            // Edit Mode
            <div className="space-y-3">
              <textarea
                ref={textareaRef}
                value={editContent}
                onChange={(e) => setEditContent(e.target.value)}
                className="
                  w-full p-3 border border-gray-300 rounded-xl
                  resize-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
                  text-gray-900 placeholder-gray-500
                "
                placeholder="Edit your message..."
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && (e.metaKey || e.ctrlKey)) {
                    handleSaveEdit();
                  }
                  if (e.key === 'Escape') {
                    handleCancelEdit();
                  }
                }}
              />
              <div className="flex gap-2 justify-end">
                <Button 
                  variant="ghost" 
                  size="sm" 
                  onClick={handleCancelEdit}
                >
                  Cancel
                </Button>
                <Button 
                  variant="primary" 
                  size="sm" 
                  onClick={handleSaveEdit}
                  disabled={!editContent.trim()}
                >
                  Save
                </Button>
              </div>
            </div>
          ) : (
            // Display Mode
            <div className={bubbleClasses}>
              {isUser ? (
                // User message - simple text
                <p className="whitespace-pre-wrap break-words">
                  {message.content}
                </p>
              ) : (
                // Assistant message - rich markdown
                <div className="prose prose-sm max-w-none">
                  <ReactMarkdown
                    remarkPlugins={[remarkGfm]}
                    components={{
                      // Custom link styling
                      a: ({ href, children }) => (
                        <a 
                          href={href} 
                          target="_blank" 
                          rel="noopener noreferrer"
                          className="text-blue-600 hover:text-blue-800 underline"
                        >
                          {children}
                        </a>
                      ),
                      // Custom code styling
                      code: ({ children, className }) => (
                        <code className={`
                          bg-gray-200 text-gray-800 px-1 py-0.5 rounded text-sm
                          ${className || ''}
                        `}>
                          {children}
                        </code>
                      ),
                      // Custom pre styling
                      pre: ({ children }) => (
                        <pre className="
                          bg-gray-800 text-gray-100 p-3 rounded-lg overflow-x-auto
                          text-sm leading-relaxed
                        ">
                          {children}
                        </pre>
                      )
                    }}
                  >
                    {message.content}
                  </ReactMarkdown>
                </div>
              )}

              {/* Timestamp */}
              {message.timestamp && (
                <time className={`
                  block text-xs mt-2 opacity-70
                  ${isUser ? 'text-blue-100' : 'text-gray-500'}
                `}>
                  {message.timestamp.toLocaleTimeString([], {
                    hour: '2-digit',
                    minute: '2-digit'
                  })}
                </time>
              )}
            </div>
          )}

          {/* Actions */}
          {showActions && !isEditing && (
            <ActionsContainer>
              {!message.isStreaming && (
                <>
                  {/* Copy */}
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={handleCopy}
                    className="h-8 w-8 p-0"
                    title="Copy message"
                  >
                    {isCopied ? (
                      <IconCheck size={16} className="text-green-600" />
                    ) : (
                      <IconCopy size={16} />
                    )}
                  </Button>

                  {/* Speak */}
                  {message.role === 'assistant' && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={handleSpeak}
                      className="h-8 w-8 p-0"
                      title={isSpeaking ? "Stop speaking" : "Read aloud"}
                    >
                      {isSpeaking ? (
                        <IconPlayerPause size={16} className="text-red-500" />
                      ) : (
                        <IconVolume2 size={16} />
                      )}
                    </Button>
                  )}

                  {/* Edit (user messages only) */}
                  {message.role === 'user' && onEdit && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setIsEditing(true)}
                      className="h-8 w-8 p-0"
                      title="Edit message"
                    >
                      <IconEdit size={16} />
                    </Button>
                  )}

                  {/* Delete */}
                  {onDelete && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={onDelete}
                      className="h-8 w-8 p-0 text-red-600 hover:text-red-700"
                      title="Delete message"
                    >
                      <IconTrash size={16} />
                    </Button>
                  )}
                </>
              )}
            </ActionsContainer>
          )}
        </div>
      </div>
    </div>
  );
};