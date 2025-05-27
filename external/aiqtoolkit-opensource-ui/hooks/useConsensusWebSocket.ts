import { useState, useEffect, useCallback, useRef } from 'react';

export interface ConsensusMessage {
  type: string;
  [key: string]: any;
}

export interface UseConsensusWebSocketOptions {
  url?: string;
  reconnectInterval?: number;
  maxReconnectAttempts?: number;
}

export const useConsensusWebSocket = (options: UseConsensusWebSocketOptions = {}) => {
  const {
    url = 'ws://localhost:8000/ws/consensus',
    reconnectInterval = 5000,
    maxReconnectAttempts = 5
  } = options;

  const [isConnected, setIsConnected] = useState(false);
  const [lastMessage, setLastMessage] = useState<ConsensusMessage | null>(null);
  const [error, setError] = useState<Error | null>(null);
  
  const ws = useRef<WebSocket | null>(null);
  const reconnectTimer = useRef<NodeJS.Timeout | null>(null);
  const reconnectAttempts = useRef(0);
  const messageQueue = useRef<ConsensusMessage[]>([]);

  const connect = useCallback(() => {
    if (ws.current?.readyState === WebSocket.OPEN) {
      return;
    }

    try {
      ws.current = new WebSocket(url);

      ws.current.onopen = () => {
        console.log('Consensus WebSocket connected');
        setIsConnected(true);
        setError(null);
        reconnectAttempts.current = 0;

        // Send any queued messages
        while (messageQueue.current.length > 0) {
          const message = messageQueue.current.shift();
          if (message) {
            ws.current?.send(JSON.stringify(message));
          }
        }
      };

      ws.current.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          setLastMessage(data);
        } catch (err) {
          console.error('Failed to parse WebSocket message:', err);
          setError(new Error('Failed to parse message'));
        }
      };

      ws.current.onerror = (event) => {
        console.error('WebSocket error:', event);
        setError(new Error('WebSocket connection error'));
      };

      ws.current.onclose = () => {
        console.log('WebSocket disconnected');
        setIsConnected(false);
        
        // Attempt to reconnect
        if (reconnectAttempts.current < maxReconnectAttempts) {
          reconnectAttempts.current++;
          reconnectTimer.current = setTimeout(() => {
            console.log(`Attempting to reconnect (${reconnectAttempts.current}/${maxReconnectAttempts})`);
            connect();
          }, reconnectInterval);
        }
      };
    } catch (err) {
      console.error('Failed to create WebSocket:', err);
      setError(err as Error);
    }
  }, [url, reconnectInterval, maxReconnectAttempts]);

  const disconnect = useCallback(() => {
    if (reconnectTimer.current) {
      clearTimeout(reconnectTimer.current);
      reconnectTimer.current = null;
    }
    
    if (ws.current) {
      ws.current.close();
      ws.current = null;
    }
    
    setIsConnected(false);
  }, []);

  const sendMessage = useCallback((message: ConsensusMessage) => {
    if (ws.current?.readyState === WebSocket.OPEN) {
      ws.current.send(JSON.stringify(message));
    } else {
      // Queue message to send when connected
      messageQueue.current.push(message);
      
      // Attempt to connect if not connected
      if (!isConnected) {
        connect();
      }
    }
  }, [isConnected, connect]);

  useEffect(() => {
    connect();
    
    return () => {
      disconnect();
    };
  }, [connect, disconnect]);

  return {
    isConnected,
    lastMessage,
    error,
    sendMessage,
    connect,
    disconnect
  };
};

export default useConsensusWebSocket;