import { useEffect, useRef, useState, useCallback } from 'react';
import config from '../config.json';

interface WebSocketOptions {
  onMessage?: (data: any) => void;
  onError?: (error: Event) => void;
  onOpen?: () => void;
  onClose?: () => void;
}

export const useWebSocket = (url?: string, options?: WebSocketOptions) => {
  const [connected, setConnected] = useState(false);
  const [reconnectAttempt, setReconnectAttempt] = useState(0);
  const wsRef = useRef<WebSocket | null>(null);
  const reconnectTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const optionsRef = useRef(options);
  optionsRef.current = options;

  const connect = useCallback(() => {
    const wsUrl = url || config.webSocket.url;
    
    try {
      const ws = new WebSocket(wsUrl);
      wsRef.current = ws;

      ws.onopen = () => {
        console.log('WebSocket connected');
        setConnected(true);
        setReconnectAttempt(0);
        optionsRef.current?.onOpen?.();
      };

      ws.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          optionsRef.current?.onMessage?.(data);
        } catch (error) {
          console.error('Error parsing WebSocket message:', error);
        }
      };

      ws.onerror = (error) => {
        console.error('WebSocket error:', error);
        optionsRef.current?.onError?.(error);
      };

      ws.onclose = () => {
        console.log('WebSocket disconnected');
        setConnected(false);
        wsRef.current = null;
        optionsRef.current?.onClose?.();
        
        // Attempt to reconnect
        if (reconnectAttempt < config.webSocket.maxReconnectAttempts) {
          setReconnectAttempt(prev => prev + 1);
          console.log(`Attempting to reconnect (${reconnectAttempt + 1}/${config.webSocket.maxReconnectAttempts})...`);
          
          reconnectTimeoutRef.current = setTimeout(() => {
            connect();
          }, config.webSocket.reconnectInterval);
        }
      };
    } catch (error) {
      console.error('Error creating WebSocket:', error);
      setConnected(false);
    }
  }, [url, reconnectAttempt]);

  const disconnect = useCallback(() => {
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current);
      reconnectTimeoutRef.current = null;
    }
    
    if (wsRef.current) {
      wsRef.current.close();
      wsRef.current = null;
    }
    
    setConnected(false);
    setReconnectAttempt(0);
  }, []);

  const sendMessage = useCallback((message: any) => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      wsRef.current.send(JSON.stringify(message));
    } else {
      console.warn('WebSocket is not connected. Message not sent:', message);
    }
  }, []);

  useEffect(() => {
    connect();
    
    return () => {
      disconnect();
    };
  }, [connect, disconnect]);

  return {
    connected,
    sendMessage,
    disconnect,
    reconnect: connect
  };
};
