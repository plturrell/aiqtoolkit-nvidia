import React, { createContext, useContext, useEffect, useState } from 'react';
import { useWebSocket } from '../hooks/useWebSocket';
import { webSocketMessageTypes } from '@/utils/app/const';
import HomeContext from '@/pages/api/home/home.context';

interface WebSocketContextType {
  connected: boolean;
  sendMessage: (message: any) => void;
  lastMessage: any;
  error: string | null;
}

const WebSocketContext = createContext<WebSocketContextType>({
  connected: false,
  sendMessage: () => {},
  lastMessage: null,
  error: null,
});

export const useWebSocketContext = () => useContext(WebSocketContext);

interface WebSocketProviderProps {
  children: React.ReactNode;
  url?: string;
}

export const WebSocketProvider: React.FC<WebSocketProviderProps> = ({ children, url }) => {
  const [lastMessage, setLastMessage] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);
  const { dispatch: homeDispatch } = useContext(HomeContext);

  const { connected, sendMessage } = useWebSocket(url, {
    onMessage: (data) => {
      setLastMessage(data);
      handleWebSocketMessage(data);
    },
    onError: (err) => {
      setError('WebSocket connection error');
      homeDispatch({ field: 'webSocketConnected', value: false });
      homeDispatch({ field: 'loading', value: false });
      homeDispatch({ field: 'messageIsStreaming', value: false });
    },
    onOpen: () => {
      setError(null);
      homeDispatch({ field: 'webSocketConnected', value: true });
    },
    onClose: () => {
      homeDispatch({ field: 'webSocketConnected', value: false });
    },
  });

  const handleWebSocketMessage = (message: any) => {
    // Handle loading state
    homeDispatch({ field: 'loading', value: false });
    
    if (message?.status === 'complete') {
      setTimeout(() => {
        homeDispatch({ field: 'messageIsStreaming', value: false });
      }, 200);
    }

    // Handle human in the loop messages
    if (message?.type === webSocketMessageTypes.systemInteractionMessage) {
      // This should be handled in the Chat component
      return;
    }

    // Filter intermediate steps if disabled
    if (sessionStorage.getItem('enableIntermediateSteps') === 'false' && 
        message?.type === webSocketMessageTypes.systemIntermediateMessage) {
      console.log('Ignoring intermediate steps');
      return;
    }

    // Handle error messages
    if (message?.type === 'error') {
      message.content.text = `Something went wrong. Please try again. \n\n<details id=${message?.id}><summary></summary>${JSON.stringify(message?.content)}</details>`;
      setTimeout(() => {
        homeDispatch({ field: 'messageIsStreaming', value: false });
      }, 200);
    }
  };

  return (
    <WebSocketContext.Provider value={{ connected, sendMessage, lastMessage, error }}>
      {children}
    </WebSocketContext.Provider>
  );
};