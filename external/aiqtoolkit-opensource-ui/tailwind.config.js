/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  darkMode: 'class', // Enable class-based dark mode
  theme: {
    extend: {
      // Use our design system tokens
      colors: {
        // Override with semantic colors
        primary: 'var(--color-accent)',
        gray: {
          50: 'var(--color-surface-secondary)',
          100: 'var(--color-surface-tertiary)',
          200: 'var(--color-border-secondary)',
          300: 'var(--color-border-primary)',
          400: 'var(--color-content-placeholder)',
          500: 'var(--color-content-tertiary)',
          600: 'var(--color-content-secondary)',
          700: 'var(--color-content-secondary)',
          800: 'var(--color-surface-tertiary)',
          900: 'var(--color-content-primary)',
        },
        blue: {
          50: 'rgba(0, 122, 255, 0.1)',
          100: 'rgba(0, 122, 255, 0.2)',
          500: 'var(--color-accent)',
          600: 'var(--color-accent)',
          700: 'var(--color-accent-hover)',
          800: 'var(--color-accent-pressed)',
          900: 'rgba(0, 122, 255, 0.8)',
        },
        green: {
          500: 'var(--color-success)',
          600: 'var(--color-success)',
        },
        red: {
          500: 'var(--color-error)',
          600: 'var(--color-error)',
        }
      },
      fontFamily: {
        sans: [
          '-apple-system',
          'BlinkMacSystemFont', 
          'SF Pro Display',
          'Inter',
          'Segoe UI',
          'Roboto',
          'sans-serif'
        ],
        mono: [
          'SF Mono',
          'Monaco',
          'Cascadia Code',
          'Segoe UI Mono',
          'Consolas',
          'monospace'
        ]
      },
      spacing: {
        // Use our 8pt spacing system
        'xs': 'var(--spacing-xs)',
        'sm': 'var(--spacing-sm)', 
        'md': 'var(--spacing-md)',
        'lg': 'var(--spacing-lg)',
        'xl': 'var(--spacing-xl)',
        '2xl': 'var(--spacing-2xl)',
        '3xl': 'var(--spacing-3xl)',
      },
      borderRadius: {
        'sm': 'var(--radius-sm)',
        'md': 'var(--radius-md)', 
        'lg': 'var(--radius-lg)',
        'xl': 'var(--radius-lg)',
        '2xl': 'var(--radius-lg)',
        'full': 'var(--radius-full)',
      },
      boxShadow: {
        'sm': 'var(--elevation-subtle)',
        'md': 'var(--elevation-moderate)', 
        'lg': 'var(--elevation-prominent)',
        'xl': 'var(--elevation-prominent)',
      },
      transitionTimingFunction: {
        'ease-in-out': 'var(--transition-standard)',
      },
      animation: {
        'fade-in': 'fadeIn 0.3s ease-in-out',
        'pulse-subtle': 'pulse 2s infinite',
        blink: 'blink 1s step-start infinite',
        flicker: 'flicker 1.5s infinite',
        glitch: 'glitch 1s infinite',
        ghost: 'ghost 3s ease-in-out infinite',
        flash: 'flash 0.5s ease-in-out',
        crack: 'crack 0.6s ease-in-out forwards',
        darken: 'darken 1s forwards',
        loadingBar: 'loadingBar 2s ease-in-out infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0', transform: 'translateY(8px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        blink: {
          '0%, 100%': { opacity: 1 },
          '50%': { opacity: 0 },
        },
        flicker: {
          '0%, 100%': { opacity: '1' },
          '50%': { opacity: '0.4' },
        },
        glitch: {
          '0%': { transform: 'translate(0)' },
          '20%': { transform: 'translate(-2px, 2px)' },
          '40%': { transform: 'translate(2px, -2px)' },
          '60%': { transform: 'translate(-2px, -2px)' },
          '80%': { transform: 'translate(2px, 2px)' },
          '100%': { transform: 'translate(0)' },
        },
        ghost: {
          '0%': { opacity: '0' },
          '50%': { opacity: '1' },
          '100%': { opacity: '0' },
        },
        flash: {
          '0%': { backgroundColor: 'rgba(255, 255, 255, 0)' },
          '50%': { backgroundColor: 'rgba(255, 255, 255, 0.5)' },
          '100%': { backgroundColor: 'rgba(255, 255, 255, 0)' },
        },
        crack: {
          '0%': { backgroundSize: '100%', opacity: '1' },
          '50%': { backgroundSize: '120%', opacity: '1' },
          '100%': { backgroundSize: '100%', opacity: '0' },
        },
        loadingBar: {
          '0%': { transform: 'translateX(-100%)' },
          '50%': { transform: 'translateX(0%)' },
          '100%': { transform: 'translateX(100%)' },
        }
      },
      typography: {
        DEFAULT: {
          css: {
            color: 'var(--color-content-primary)',
            maxWidth: 'none',
            p: {
              color: 'var(--color-content-primary)',
            },
            h1: {
              color: 'var(--color-content-primary)',
              fontWeight: '600',
            },
            h2: {
              color: 'var(--color-content-primary)',
              fontWeight: '600',
            },
            h3: {
              color: 'var(--color-content-primary)',
              fontWeight: '600',
            },
            strong: {
              color: 'var(--color-content-primary)',
              fontWeight: '600',
            },
            a: {
              color: 'var(--color-accent)',
              textDecoration: 'none',
              borderBottom: '1px solid transparent',
              transition: 'border-color 0.15s ease',
              '&:hover': {
                borderBottomColor: 'var(--color-accent)',
              }
            },
            code: {
              color: 'var(--color-content-primary)',
              backgroundColor: 'var(--color-surface-tertiary)',
              padding: '0.125rem 0.25rem',
              borderRadius: 'var(--radius-sm)',
              fontSize: '0.875em',
              fontWeight: '500',
            },
            'code::before': {
              content: '""',
            },
            'code::after': {
              content: '""',
            },
            pre: {
              backgroundColor: 'var(--color-surface-tertiary)',
              border: '1px solid var(--color-border-secondary)',
              borderRadius: 'var(--radius-md)',
            },
            blockquote: {
              borderLeftColor: 'var(--color-accent)',
              color: 'var(--color-content-secondary)',
            },
            ul: {
              listStyleType: 'disc',
            },
            ol: {
              listStyleType: 'decimal',
            },
          }
        }
      }
    },
  },
  variants: {
    extend: {
      visibility: ['group-hover'],
    },
  },
  plugins: [require('@tailwindcss/typography')],
};