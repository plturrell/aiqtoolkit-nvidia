import React, { useState, useEffect } from 'react';
import { IconMenu2, IconX, IconSettings, IconSun, IconMoon } from '@tabler/icons-react';
import { Button } from './Button';

interface LayoutProps {
  /** Main content area */
  children: React.ReactNode;
  /** Sidebar content */
  sidebar?: React.ReactNode;
  /** Header content */
  header?: React.ReactNode;
  /** Footer content */
  footer?: React.ReactNode;
  /** Show sidebar by default */
  sidebarOpen?: boolean;
  /** Sidebar position */
  sidebarSide?: 'left' | 'right';
  /** Custom className */
  className?: string;
}

/**
 * Intelligent Layout Component
 * 
 * Provides responsive, adaptive layout with:
 * - Responsive sidebar that adapts to screen size
 * - Dark/light mode toggle
 * - Accessibility-first navigation
 * - Smooth, purposeful transitions
 * - Intelligent space management
 */
export const Layout: React.FC<LayoutProps> = ({
  children,
  sidebar,
  header,
  footer,
  sidebarOpen: defaultSidebarOpen = false,
  sidebarSide = 'left',
  className = ''
}) => {
  const [sidebarOpen, setSidebarOpen] = useState(defaultSidebarOpen);
  const [darkMode, setDarkMode] = useState(false);
  const [isMobile, setIsMobile] = useState(false);

  // Detect mobile viewport
  useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth < 768);
    };
    
    checkMobile();
    window.addEventListener('resize', checkMobile);
    return () => window.removeEventListener('resize', checkMobile);
  }, []);

  // Auto-close sidebar on mobile when clicking outside
  useEffect(() => {
    if (isMobile && sidebarOpen) {
      const handleClickOutside = () => setSidebarOpen(false);
      document.addEventListener('click', handleClickOutside);
      return () => document.removeEventListener('click', handleClickOutside);
    }
  }, [isMobile, sidebarOpen]);

  // Dark mode detection and application
  useEffect(() => {
    const isDark = localStorage.getItem('darkMode') === 'true' || 
      (!localStorage.getItem('darkMode') && window.matchMedia('(prefers-color-scheme: dark)').matches);
    
    setDarkMode(isDark);
    document.documentElement.classList.toggle('dark', isDark);
  }, []);

  const toggleDarkMode = () => {
    const newDarkMode = !darkMode;
    setDarkMode(newDarkMode);
    localStorage.setItem('darkMode', newDarkMode.toString());
    document.documentElement.classList.toggle('dark', newDarkMode);
  };

  const toggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };

  // Prevent sidebar click from closing on mobile
  const handleSidebarClick = (e: React.MouseEvent) => {
    e.stopPropagation();
  };

  return (
    <div className={`
      min-h-screen bg-gray-50 dark:bg-gray-900
      transition-colors duration-300
      ${className}
    `}>
      {/* Mobile backdrop */}
      {isMobile && sidebarOpen && (
        <div 
          className="fixed inset-0 z-40 bg-black bg-opacity-50 transition-opacity"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      {sidebar && (
        <aside 
          className={`
            fixed top-0 z-50 h-full w-80
            bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700
            transform transition-transform duration-300 ease-in-out
            ${sidebarSide === 'right' ? 'right-0' : 'left-0'}
            ${sidebarOpen ? 'translate-x-0' : (
              sidebarSide === 'right' ? 'translate-x-full' : '-translate-x-full'
            )}
            ${isMobile ? 'shadow-xl' : ''}
          `}
          onClick={handleSidebarClick}
        >
          {/* Sidebar header */}
          <div className="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
              Navigation
            </h2>
            <Button
              variant="ghost"
              size="sm"
              className="h-8 w-8 p-0 md:hidden"
              onClick={() => setSidebarOpen(false)}
            >
              <IconX size={20} />
            </Button>
          </div>

          {/* Sidebar content */}
          <div className="flex-1 overflow-y-auto p-4">
            {sidebar}
          </div>
        </aside>
      )}

      {/* Main content */}
      <div className={`
        min-h-screen flex flex-col
        transition-all duration-300 ease-in-out
        ${sidebar && sidebarOpen && !isMobile ? (
          sidebarSide === 'right' ? 'mr-80' : 'ml-80'
        ) : ''}
      `}>
        {/* Header */}
        {header || sidebar ? (
          <header className="
            sticky top-0 z-30
            bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700
            px-4 py-3
          ">
            <div className="flex items-center justify-between">
              {/* Left side */}
              <div className="flex items-center gap-3">
                {sidebar && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 w-8 p-0"
                    onClick={toggleSidebar}
                    title="Toggle sidebar"
                  >
                    <IconMenu2 size={20} />
                  </Button>
                )}
                
                {header}
              </div>

              {/* Right side */}
              <div className="flex items-center gap-2">
                {/* Dark mode toggle */}
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-8 w-8 p-0"
                  onClick={toggleDarkMode}
                  title={darkMode ? "Switch to light mode" : "Switch to dark mode"}
                >
                  {darkMode ? <IconSun size={20} /> : <IconMoon size={20} />}
                </Button>

                {/* Settings */}
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-8 w-8 p-0"
                  title="Settings"
                >
                  <IconSettings size={20} />
                </Button>
              </div>
            </div>
          </header>
        ) : null}

        {/* Main content area */}
        <main className="flex-1 overflow-hidden">
          {children}
        </main>

        {/* Footer */}
        {footer && (
          <footer className="
            border-t border-gray-200 dark:border-gray-700
            bg-white dark:bg-gray-800
            px-4 py-3
          ">
            {footer}
          </footer>
        )}
      </div>
    </div>
  );
};