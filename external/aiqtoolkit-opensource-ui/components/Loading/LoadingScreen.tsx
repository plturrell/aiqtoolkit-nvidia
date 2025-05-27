import React from 'react';
import { Spinner } from '../Spinner/Spinner';

interface LoadingScreenProps {
  message?: string;
}

export const LoadingScreen: React.FC<LoadingScreenProps> = ({ message = 'Loading...' }) => {
  return (
    <div className="flex flex-col items-center justify-center h-screen bg-gray-100 dark:bg-gray-900">
      <Spinner size="lg" />
      <p className="mt-4 text-gray-700 dark:text-gray-300">{message}</p>
    </div>
  );
};

export const LoadingOverlay: React.FC<LoadingScreenProps> = ({ message = 'Loading...' }) => {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div className="p-6 bg-white dark:bg-gray-800 rounded-lg shadow-lg">
        <Spinner size="md" />
        <p className="mt-4 text-gray-700 dark:text-gray-300">{message}</p>
      </div>
    </div>
  );
};