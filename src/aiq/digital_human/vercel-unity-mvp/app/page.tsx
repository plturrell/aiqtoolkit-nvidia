'use client';

import React, { useState, useEffect, useRef } from 'react';
import { Unity, useUnityContext } from 'react-unity-webgl';
import { Socket, io } from 'socket.io-client';
import axios from 'axios';
import { 
  Box, 
  Container, 
  Paper, 
  TextField, 
  Button, 
  List, 
  ListItem, 
  ListItemText,
  Typography,
  CircularProgress,
  Alert,
  Snackbar,
  AppBar,
  Toolbar,
  IconButton,
  Drawer,
  Switch,
  FormControlLabel
} from '@mui/material';
import { Send, Settings, Refresh } from '@mui/icons-material';


interface Message {
  id: string;
  sender: 'user' | 'ai';
  text: string;
  timestamp: Date;
  error?: boolean;
}

interface BackendConfig {
  useWebSocket: boolean;
  useMCP: boolean;
  useBrev: boolean;
  avatarUrl: string;
}

export default function UnityDigitalHumanMVP() {
  // Unity WebGL context configuration
  const { unityProvider, isLoaded, loadingProgression, sendMessage: sendToUnity } = useUnityContext({
    loaderUrl: '/unity/Build/DigitalHumanMVP.loader.js',
    dataUrl: '/unity/Build/DigitalHumanMVP.data',
    frameworkUrl: '/unity/Build/DigitalHumanMVP.framework.js',
    codeUrl: '/unity/Build/DigitalHumanMVP.wasm',
  });

  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [socket, setSocket] = useState<Socket | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  
  const [config, setConfig] = useState<BackendConfig>({
    useWebSocket: true,
    useMCP: true,
    useBrev: true,
    avatarUrl: 'https://models.readyplayerme.com/6745fc8cf1b7d73d7ccb4311.glb'
  });

  // Initialize WebSocket connection
  useEffect(() => {
    if (config.useWebSocket) {
      const socketInstance = io(process.env.NEXT_PUBLIC_WS_URL || 'http://localhost:8088', {
        transports: ['websocket'],
        reconnection: true,
        reconnectionDelay: 1000,
        reconnectionAttempts: 5
      });

      socketInstance.on('connect', () => {
        console.log('Connected to WebSocket server');
        setIsConnected(true);
        setError(null);
      });

      socketInstance.on('disconnect', () => {
        console.log('Disconnected from WebSocket server');
        setIsConnected(false);
      });

      socketInstance.on('error', (err) => {
        console.error('WebSocket error:', err);
        setError('WebSocket connection error');
      });

      socketInstance.on('chat_response', (data) => {
        const aiMessage: Message = {
          id: Date.now().toString(),
          sender: 'ai',
          text: data.response,
          timestamp: new Date()
        };
        setMessages(prev => [...prev, aiMessage]);
        setIsLoading(false);
        
        // Send message to Unity
        sendUnityMessage('OnChatResponse', JSON.stringify({
          text: data.response,
          emotion: data.emotion || 'neutral'
        }));
      });

      setSocket(socketInstance);

      return () => {
        socketInstance.disconnect();
      };
    }
  }, [config.useWebSocket]);

  // Unity communication
  useEffect(() => {
    if (isLoaded) {
      console.log('Unity application loaded');
      
      // Send initial configuration to Unity
      sendToUnity('DigitalHumanMVP', 'ConfigureAvatar', config.avatarUrl);
    }
  }, [isLoaded, config.avatarUrl, sendToUnity]);

  const sendUnityMessage = (methodName: string, data: string) => {
    if (isLoaded) {
      sendToUnity('DigitalHumanMVP', methodName, data);
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const sendMessage = async () => {
    if (!input.trim()) return;

    const userMessage: Message = {
      id: Date.now().toString(),
      sender: 'user',
      text: input,
      timestamp: new Date()
    };

    setMessages(prev => [...prev, userMessage]);
    setInput('');
    setIsLoading(true);

    // Send to Unity for visual feedback
    sendUnityMessage('OnUserMessage', input);

    try {
      if (config.useWebSocket && socket && isConnected) {
        // Send via WebSocket
        socket.emit('chat_message', {
          query: input,
          use_mcp: config.useMCP,
          use_brev: config.useBrev,
          session_id: 'unity-mvp-session',
          user_id: 'vercel-user'
        });
      } else {
        // Fallback to REST API
        const response = await axios.post('/api/chat', {
          query: input,
          use_mcp: config.useMCP,
          use_brev: config.useBrev
        });

        const aiMessage: Message = {
          id: Date.now().toString(),
          sender: 'ai',
          text: response.data.response,
          timestamp: new Date()
        };

        setMessages(prev => [...prev, aiMessage]);
        
        // Send to Unity
        sendUnityMessage('OnChatResponse', JSON.stringify({
          text: response.data.response,
          emotion: response.data.emotion || 'neutral'
        }));
      }
    } catch (error) {
      console.error('Error sending message:', error);
      const errorMessage: Message = {
        id: Date.now().toString(),
        sender: 'ai',
        text: 'Error: Failed to get response. Please try again.',
        timestamp: new Date(),
        error: true
      };
      setMessages(prev => [...prev, errorMessage]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  };

  const reconnectWebSocket = () => {
    if (socket) {
      socket.connect();
    }
  };

  const updateAvatarUrl = () => {
    sendUnityMessage('ConfigureAvatar', config.avatarUrl);
  };

  return (
    <Container maxWidth="xl" sx={{ height: '100vh', py: 2 }}>
      <AppBar position="static" sx={{ mb: 2 }}>
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            Unity Digital Human MVP - Vercel
          </Typography>
          <Typography variant="body2" sx={{ mr: 2 }}>
            {isConnected ? '🟢 Connected' : '🔴 Disconnected'}
          </Typography>
          <IconButton color="inherit" onClick={() => setSettingsOpen(true)}>
            <Settings />
          </IconButton>
        </Toolbar>
      </AppBar>

      <Box sx={{ display: 'flex', height: 'calc(100% - 80px)', gap: 2 }}>
        {/* Unity WebGL Container */}
        <Paper sx={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
          <Box sx={{ flex: 1, position: 'relative', bgcolor: '#000' }}>
            {!isLoaded && (
              <Box sx={{ 
                position: 'absolute', 
                top: '50%', 
                left: '50%', 
                transform: 'translate(-50%, -50%)',
                textAlign: 'center'
              }}>
                <CircularProgress size={60} />
                <Typography variant="h6" sx={{ mt: 2, color: 'white' }}>
                  Loading Unity Application... {Math.round(loadingProgression * 100)}%
                </Typography>
              </Box>
            )}
            <Unity
              unityProvider={unityProvider}
              style={{ 
                width: '100%', 
                height: '100%',
                visibility: isLoaded ? 'visible' : 'hidden'
              }}
            />
          </Box>
        </Paper>

        {/* Chat Interface */}
        <Paper sx={{ width: 400, display: 'flex', flexDirection: 'column' }}>
          <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider' }}>
            <Typography variant="h6">Chat with AI</Typography>
          </Box>
          
          <List sx={{ flex: 1, overflow: 'auto', p: 2 }}>
            {messages.map((message) => (
              <ListItem
                key={message.id}
                sx={{
                  flexDirection: 'column',
                  alignItems: message.sender === 'user' ? 'flex-end' : 'flex-start',
                  mb: 1
                }}
              >
                <Paper
                  sx={{
                    p: 1.5,
                    maxWidth: '80%',
                    bgcolor: message.sender === 'user' ? 'primary.main' : 'grey.100',
                    color: message.sender === 'user' ? 'white' : 'text.primary',
                    ...(message.error && { bgcolor: 'error.main', color: 'white' })
                  }}
                >
                  <ListItemText
                    primary={message.text}
                    secondary={
                      <Typography variant="caption" sx={{ 
                        color: message.sender === 'user' ? 'rgba(255,255,255,0.7)' : 'text.secondary' 
                      }}>
                        {new Date(message.timestamp).toLocaleTimeString()}
                      </Typography>
                    }
                  />
                </Paper>
              </ListItem>
            ))}
            {isLoading && (
              <ListItem sx={{ justifyContent: 'center' }}>
                <CircularProgress size={24} />
              </ListItem>
            )}
            <div ref={messagesEndRef} />
          </List>

          <Box sx={{ p: 2, borderTop: 1, borderColor: 'divider' }}>
            <TextField
              fullWidth
              variant="outlined"
              placeholder="Type your message..."
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyPress={handleKeyPress}
              disabled={isLoading}
              InputProps={{
                endAdornment: (
                  <IconButton
                    color="primary"
                    onClick={sendMessage}
                    disabled={isLoading || !input.trim()}
                  >
                    <Send />
                  </IconButton>
                )
              }}
            />
          </Box>
        </Paper>
      </Box>

      {/* Settings Drawer */}
      <Drawer anchor="right" open={settingsOpen} onClose={() => setSettingsOpen(false)}>
        <Box sx={{ width: 300, p: 3 }}>
          <Typography variant="h6" sx={{ mb: 3 }}>Settings</Typography>
          
          <FormControlLabel
            control={
              <Switch
                checked={config.useWebSocket}
                onChange={(e) => setConfig({...config, useWebSocket: e.target.checked})}
              />
            }
            label="Use WebSocket"
          />
          
          <FormControlLabel
            control={
              <Switch
                checked={config.useMCP}
                onChange={(e) => setConfig({...config, useMCP: e.target.checked})}
              />
            }
            label="Use MCP Tools"
          />
          
          <FormControlLabel
            control={
              <Switch
                checked={config.useBrev}
                onChange={(e) => setConfig({...config, useBrev: e.target.checked})}
              />
            }
            label="Use Brev Integration"
          />
          
          <TextField
            fullWidth
            label="Avatar URL"
            value={config.avatarUrl}
            onChange={(e) => setConfig({...config, avatarUrl: e.target.value})}
            sx={{ mt: 3, mb: 2 }}
          />
          
          <Button
            fullWidth
            variant="contained"
            onClick={updateAvatarUrl}
            sx={{ mb: 2 }}
          >
            Update Avatar
          </Button>
          
          {!isConnected && config.useWebSocket && (
            <Button
              fullWidth
              variant="outlined"
              startIcon={<Refresh />}
              onClick={reconnectWebSocket}
            >
              Reconnect WebSocket
            </Button>
          )}
        </Box>
      </Drawer>

      {/* Error Snackbar */}
      <Snackbar
        open={!!error}
        autoHideDuration={6000}
        onClose={() => setError(null)}
      >
        <Alert severity="error" onClose={() => setError(null)}>
          {error}
        </Alert>
      </Snackbar>
    </Container>
  );
}