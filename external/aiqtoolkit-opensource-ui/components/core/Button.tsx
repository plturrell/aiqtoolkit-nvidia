import React from 'react';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  /** Visual hierarchy and purpose */
  variant?: 'primary' | 'secondary' | 'ghost' | 'destructive';
  /** Size appropriate to context */
  size?: 'sm' | 'md' | 'lg';
  /** Loading state with built-in spinner */
  loading?: boolean;
  /** Icon before text */
  icon?: React.ReactNode;
  /** Full width button */
  fullWidth?: boolean;
}

/**
 * Purposeful Button Component
 * 
 * Single button component that handles all use cases:
 * - Clear visual hierarchy through variants
 * - Appropriate sizing for context
 * - Built-in loading states
 * - Excellent accessibility
 * - Meaningful animations
 */
export const Button: React.FC<ButtonProps> = ({
  variant = 'secondary',
  size = 'md',
  loading = false,
  icon,
  fullWidth = false,
  children,
  disabled,
  className = '',
  ...props
}) => {
  // Base styles that never change
  const baseClasses = `
    inline-flex items-center justify-center
    font-medium rounded-xl
    transition-all duration-200 ease-in-out
    focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2
    disabled:pointer-events-none disabled:opacity-50
    ${fullWidth ? 'w-full' : ''}
  `;

  // Size variants
  const sizeClasses = {
    sm: 'h-8 px-3 text-sm gap-1.5',
    md: 'h-10 px-4 text-sm gap-2',
    lg: 'h-12 px-6 text-base gap-2'
  }[size];

  // Visual hierarchy variants
  const variantClasses = {
    primary: `
      bg-blue-600 text-white
      hover:bg-blue-700 active:bg-blue-800
      focus-visible:ring-blue-500
      shadow-sm hover:shadow-md
    `,
    secondary: `
      bg-gray-100 text-gray-900 border border-gray-200
      hover:bg-gray-50 hover:border-gray-300
      active:bg-gray-200
      focus-visible:ring-gray-500
    `,
    ghost: `
      text-gray-700
      hover:bg-gray-100 active:bg-gray-200
      focus-visible:ring-gray-500
    `,
    destructive: `
      bg-red-600 text-white
      hover:bg-red-700 active:bg-red-800
      focus-visible:ring-red-500
      shadow-sm hover:shadow-md
    `
  }[variant];

  // Loading spinner component
  const LoadingSpinner = () => (
    <svg 
      className="animate-spin h-4 w-4" 
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <circle 
        cx="12" 
        cy="12" 
        r="10" 
        stroke="currentColor" 
        strokeWidth="2"
        className="opacity-25"
      />
      <path 
        fill="currentColor" 
        d="M4 12a8 8 0 018-8v8H4z"
        className="opacity-75"
      />
    </svg>
  );

  return (
    <button
      className={`${baseClasses} ${sizeClasses} ${variantClasses} ${className}`}
      disabled={disabled || loading}
      {...props}
    >
      {loading ? (
        <>
          <LoadingSpinner />
          {children && <span className="opacity-75">{children}</span>}
        </>
      ) : (
        <>
          {icon && <span className="shrink-0">{icon}</span>}
          {children}
        </>
      )}
    </button>
  );
};