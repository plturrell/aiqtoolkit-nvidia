// Environment configuration utility
import config from '../config.json';

export const getEnvConfig = () => {
  return {
    apiUrl: process.env.NEXT_PUBLIC_API_URL || config.apiEndpoints.chat.split('/v1')[0] || 'http://localhost:8000',
    apiVersion: process.env.NEXT_PUBLIC_API_VERSION || 'v1',
    wsUrl: process.env.NEXT_PUBLIC_WS_URL || config.webSocket.url || 'ws://localhost:8000/v1/ws',
    defaultModel: process.env.NEXT_PUBLIC_DEFAULT_MODEL || config.ui.defaultModel || 'aiq-toolkit',
    enableIntermediateSteps: process.env.NEXT_PUBLIC_ENABLE_INTERMEDIATE_STEPS === 'true' || config.ui.enableIntermediateSteps || true,
    maxSessionStorageSize: parseInt(process.env.NEXT_PUBLIC_MAX_SESSION_STORAGE_SIZE || '') || config.storage.maxSessionStorageSize || 5242880,
    enableWebSocket: process.env.NEXT_PUBLIC_ENABLE_WEBSOCKET !== 'false',
    enableStreaming: process.env.NEXT_PUBLIC_ENABLE_STREAMING !== 'false',
  };
};

export const getApiEndpoint = (endpoint: 'chat' | 'chatCompletion' | 'generate' | 'stream') => {
  const { apiUrl, apiVersion } = getEnvConfig();
  const endpoints = {
    chat: `${apiUrl}/${apiVersion}/workflows/run`,
    chatCompletion: `${apiUrl}/${apiVersion}/chat/completions`,
    generate: `${apiUrl}/${apiVersion}/generate`,
    stream: `${apiUrl}/${apiVersion}/stream`,
  };
  return endpoints[endpoint];
};
