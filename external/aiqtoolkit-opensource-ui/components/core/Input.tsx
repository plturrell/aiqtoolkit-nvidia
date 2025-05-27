import React, { useState, useRef, useEffect } from 'react';
import { IconSend, IconPaperclip, IconMicrophone } from '@tabler/icons-react';
import { Button } from './Button';

interface InputProps {
  /** Current input value */
  value: string;
  /** Change handler */
  onChange: (value: string) => void;
  /** Submit handler */
  onSubmit: (value: string) => void;
  /** Upload handler for attachments */
  onUpload?: (files: FileList) => void;
  /** Voice input handler */
  onVoiceInput?: () => void;
  /** Loading state */
  loading?: boolean;
  /** Disabled state */
  disabled?: boolean;
  /** Placeholder text */
  placeholder?: string;
  /** Maximum character count */
  maxLength?: number;
  /** Show character count */
  showCount?: boolean;
  /** Auto-focus on mount */
  autoFocus?: boolean;
  /** Custom className */
  className?: string;
}

/**
 * Intelligent Input Component
 * 
 * Handles all chat input needs with built-in intelligence:
 * - Auto-resizing textarea for optimal UX
 * - File upload with drag & drop
 * - Voice input capabilities
 * - Keyboard shortcuts and accessibility
 * - Character counting and validation
 * - Progressive enhancement
 */
export const Input: React.FC<InputProps> = ({
  value,
  onChange,
  onSubmit,
  onUpload,
  onVoiceInput,
  loading = false,
  disabled = false,
  placeholder = "Type a message...",
  maxLength = 4000,
  showCount = false,
  autoFocus = false,
  className = ''
}) => {
  const [isDragOver, setIsDragOver] = useState(false);
  const [isRecording, setIsRecording] = useState(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Auto-resize textarea
  useEffect(() => {
    const textarea = textareaRef.current;
    if (textarea) {
      textarea.style.height = 'auto';
      const scrollHeight = textarea.scrollHeight;
      const maxHeight = 120; // Approximately 6 lines
      textarea.style.height = `${Math.min(scrollHeight, maxHeight)}px`;
    }
  }, [value]);

  // Focus on mount if requested
  useEffect(() => {
    if (autoFocus && textareaRef.current) {
      textareaRef.current.focus();
    }
  }, [autoFocus]);

  // Handle form submission
  const handleSubmit = (e?: React.FormEvent) => {
    e?.preventDefault();
    if (value.trim() && !loading && !disabled) {
      onSubmit(value.trim());
    }
  };

  // Handle keyboard shortcuts
  const handleKeyDown = (e: React.KeyboardEvent) => {
    // Submit on Enter (without Shift)
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit();
    }

    // Upload shortcut (Cmd/Ctrl + U)
    if ((e.metaKey || e.ctrlKey) && e.key === 'u' && onUpload) {
      e.preventDefault();
      fileInputRef.current?.click();
    }

    // Voice input shortcut (Cmd/Ctrl + M)
    if ((e.metaKey || e.ctrlKey) && e.key === 'm' && onVoiceInput) {
      e.preventDefault();
      handleVoiceInput();
    }
  };

  // Handle file upload
  const handleFileUpload = (files: FileList | null) => {
    if (files && files.length > 0 && onUpload) {
      onUpload(files);
    }
  };

  // Handle drag and drop
  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
    handleFileUpload(e.dataTransfer.files);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
  };

  // Handle voice input
  const handleVoiceInput = () => {
    if (onVoiceInput) {
      setIsRecording(!isRecording);
      onVoiceInput();
    }
  };

  // Character count and validation
  const characterCount = value.length;
  const isOverLimit = characterCount > maxLength;
  const isValid = value.trim().length > 0 && !isOverLimit;

  return (
    <div className={`relative ${className}`}>
      {/* Main input container */}
      <form onSubmit={handleSubmit} className="relative">
        <div className={`
          flex items-end gap-2 p-4
          bg-white border border-gray-200 rounded-2xl
          transition-all duration-200
          ${isDragOver ? 'border-blue-500 bg-blue-50' : ''}
          ${disabled ? 'opacity-50 pointer-events-none' : ''}
          focus-within:border-blue-500 focus-within:ring-1 focus-within:ring-blue-500
        `}
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        >
          {/* Attachment button */}
          {onUpload && (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="h-10 w-10 p-0 shrink-0"
              onClick={() => fileInputRef.current?.click()}
              title="Attach file (⌘U)"
              disabled={disabled}
            >
              <IconPaperclip size={20} />
            </Button>
          )}

          {/* Text input */}
          <textarea
            ref={textareaRef}
            value={value}
            onChange={(e) => onChange(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            disabled={disabled}
            maxLength={maxLength}
            className="
              flex-1 min-h-[40px] max-h-[120px]
              resize-none border-0 bg-transparent
              text-gray-900 placeholder-gray-500
              focus:outline-none focus:ring-0
              text-base leading-relaxed
            "
            style={{ 
              scrollbarWidth: 'thin',
              scrollbarColor: 'rgba(0,0,0,0.2) transparent'
            }}
          />

          {/* Action buttons */}
          <div className="flex items-center gap-1 shrink-0">
            {/* Voice input */}
            {onVoiceInput && (
              <Button
                type="button"
                variant="ghost"
                size="sm"
                className={`h-10 w-10 p-0 ${isRecording ? 'text-red-600' : ''}`}
                onClick={handleVoiceInput}
                title="Voice input (⌘M)"
                disabled={disabled}
              >
                <IconMicrophone size={20} />
              </Button>
            )}

            {/* Send button */}
            <Button
              type="submit"
              variant={isValid ? "primary" : "ghost"}
              size="sm"
              className="h-10 w-10 p-0"
              disabled={!isValid || loading}
              loading={loading}
              title="Send message (Enter)"
            >
              <IconSend size={18} />
            </Button>
          </div>
        </div>

        {/* Character count */}
        {showCount && (
          <div className={`
            absolute -bottom-6 right-0 text-xs
            ${isOverLimit ? 'text-red-600' : 'text-gray-500'}
          `}>
            {characterCount} / {maxLength}
          </div>
        )}
      </form>

      {/* Hidden file input */}
      {onUpload && (
        <input
          ref={fileInputRef}
          type="file"
          multiple
          className="hidden"
          onChange={(e) => handleFileUpload(e.target.files)}
          accept="image/*,audio/*,video/*,.pdf,.doc,.docx,.txt"
        />
      )}

      {/* Drag overlay */}
      {isDragOver && (
        <div className="
          absolute inset-0 z-10
          flex items-center justify-center
          bg-blue-50 border-2 border-dashed border-blue-500 rounded-2xl
          text-blue-700 font-medium
        ">
          Drop files to upload
        </div>
      )}
    </div>
  );
};