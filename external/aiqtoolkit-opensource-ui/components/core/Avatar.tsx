import React from 'react';
import { IconUser, IconRobot } from '@tabler/icons-react';

interface AvatarProps {
  /** User object with role and optional metadata */
  user?: {
    role: 'user' | 'assistant' | 'system';
    name?: string;
    avatar?: string;
  };
  /** Size variant */
  size?: 'sm' | 'md' | 'lg';
  /** Visual style context */
  variant?: 'conversation' | 'sidebar' | 'status';
  /** Optional custom className */
  className?: string;
}

/**
 * Intelligent Avatar Component
 * 
 * One component that handles all avatar needs through intelligent defaults:
 * - Automatically determines appearance based on user role
 * - Adapts size and styling to context
 * - Gracefully handles missing data
 * - Maintains accessibility standards
 */
export const Avatar: React.FC<AvatarProps> = ({
  user = { role: 'user' },
  size = 'md',
  variant = 'conversation',
  className = ''
}) => {
  // Determine dimensions based on size
  const dimensions = {
    sm: 24,
    md: 32,
    lg: 40
  }[size];

  // Get semantic styling
  const baseClasses = `
    inline-flex items-center justify-center
    rounded-full shrink-0
    transition-all duration-200
    ${variant === 'conversation' ? 'bg-gradient-to-br' : 'bg-gray-100'}
  `;

  const roleStyles = {
    user: `
      from-blue-500 to-blue-600 text-white
      shadow-sm
    `,
    assistant: `
      from-emerald-500 to-emerald-600 text-white  
      shadow-sm
    `,
    system: `
      from-gray-400 to-gray-500 text-white
      shadow-sm
    `
  };

  // Custom avatar image
  if (user.avatar) {
    return (
      <img
        src={user.avatar}
        alt={user.name || `${user.role} avatar`}
        className={`${baseClasses} ${className}`}
        style={{ width: dimensions, height: dimensions }}
        loading="lazy"
      />
    );
  }

  // Icon-based avatar with role intelligence
  const Icon = user.role === 'assistant' ? IconRobot : IconUser;
  const iconSize = Math.round(dimensions * 0.6);

  return (
    <div
      className={`${baseClasses} ${roleStyles[user.role]} ${className}`}
      style={{ width: dimensions, height: dimensions }}
      role="img"
      aria-label={user.name || `${user.role} avatar`}
    >
      <Icon size={iconSize} strokeWidth={1.5} />
    </div>
  );
};