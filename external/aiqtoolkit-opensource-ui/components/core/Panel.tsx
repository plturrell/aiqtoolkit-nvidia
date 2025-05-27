import React, { useState } from 'react';
import { IconChevronDown, IconX } from '@tabler/icons-react';
import { Button } from './Button';

interface PanelProps {
  /** Panel title */
  title: string;
  /** Panel content */
  children: React.ReactNode;
  /** Panel variant */
  variant?: 'default' | 'accent' | 'subtle';
  /** Collapsible panel */
  collapsible?: boolean;
  /** Initially collapsed */
  defaultCollapsed?: boolean;
  /** Dismissible panel */
  dismissible?: boolean;
  /** Dismiss handler */
  onDismiss?: () => void;
  /** Panel size */
  size?: 'sm' | 'md' | 'lg';
  /** Custom className */
  className?: string;
}

/**
 * Versatile Panel Component
 * 
 * Handles all panel/card needs with intelligence:
 * - Automatic styling based on variant and size
 * - Collapsible functionality with smooth animations
 * - Dismissible panels for temporary content
 * - Accessibility-first design
 * - Consistent spacing and typography
 */
export const Panel: React.FC<PanelProps> = ({
  title,
  children,
  variant = 'default',
  collapsible = false,
  defaultCollapsed = false,
  dismissible = false,
  onDismiss,
  size = 'md',
  className = ''
}) => {
  const [isCollapsed, setIsCollapsed] = useState(defaultCollapsed);

  // Variant styles
  const variantClasses = {
    default: 'bg-white border-gray-200 dark:bg-gray-800 dark:border-gray-700',
    accent: 'bg-blue-50 border-blue-200 dark:bg-blue-900/20 dark:border-blue-800',
    subtle: 'bg-gray-50 border-gray-200 dark:bg-gray-900 dark:border-gray-700'
  }[variant];

  // Size styles
  const sizeClasses = {
    sm: 'text-sm',
    md: 'text-base',
    lg: 'text-lg'
  }[size];

  const paddingClasses = {
    sm: 'p-3',
    md: 'p-4',
    lg: 'p-6'
  }[size];

  const titleSizeClasses = {
    sm: 'text-sm font-medium',
    md: 'text-base font-semibold',
    lg: 'text-lg font-semibold'
  }[size];

  return (
    <div className={`
      border rounded-xl shadow-sm
      transition-all duration-200
      ${variantClasses}
      ${sizeClasses}
      ${className}
    `}>
      {/* Header */}
      <div className={`
        flex items-center justify-between
        ${paddingClasses}
        ${children && !isCollapsed ? 'border-b border-gray-200 dark:border-gray-700' : ''}
      `}>
        {/* Title and collapse button */}
        <div className="flex items-center gap-2 flex-1">
          {collapsible && (
            <Button
              variant="ghost"
              size="sm"
              className="h-6 w-6 p-0 shrink-0"
              onClick={() => setIsCollapsed(!isCollapsed)}
              aria-label={isCollapsed ? "Expand panel" : "Collapse panel"}
            >
              <IconChevronDown 
                size={16} 
                className={`
                  transition-transform duration-200
                  ${isCollapsed ? '-rotate-90' : 'rotate-0'}
                `}
              />
            </Button>
          )}
          
          <h3 className={`
            text-gray-900 dark:text-white
            ${titleSizeClasses}
            ${collapsible ? 'select-none cursor-pointer' : ''}
          `}
          onClick={collapsible ? () => setIsCollapsed(!isCollapsed) : undefined}
          >
            {title}
          </h3>
        </div>

        {/* Dismiss button */}
        {dismissible && (
          <Button
            variant="ghost"
            size="sm"
            className="h-6 w-6 p-0 shrink-0 ml-2"
            onClick={onDismiss}
            aria-label="Dismiss panel"
          >
            <IconX size={16} />
          </Button>
        )}
      </div>

      {/* Content */}
      {children && (
        <div className={`
          overflow-hidden transition-all duration-300 ease-in-out
          ${isCollapsed ? 'max-h-0 opacity-0' : 'max-h-screen opacity-100'}
        `}>
          <div className={paddingClasses}>
            {children}
          </div>
        </div>
      )}
    </div>
  );
};