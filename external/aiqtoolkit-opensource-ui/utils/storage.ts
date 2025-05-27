import { getEnvConfig } from './env';

export const safeSessionStorage = {
  setItem: (key: string, value: string) => {
    try {
      const { maxSessionStorageSize } = getEnvConfig();
      
      // Check if the value exceeds the size limit
      if (value.length > maxSessionStorageSize) {
        console.warn(`Session storage item "${key}" exceeds size limit. Attempting to compress...`);
        
        // If it's a conversation with attachments, remove the attachment content
        try {
          const parsed = JSON.parse(value);
          if (parsed.messages) {
            parsed.messages = parsed.messages.map((msg: any) => {
              if (msg.attachment?.content && msg.attachment.content.length > 1000) {
                return {
                  ...msg,
                  attachment: {
                    ...msg.attachment,
                    content: '[Content removed to save space]',
                  },
                };
              }
              return msg;
            });
            value = JSON.stringify(parsed);
          }
        } catch (e) {
          // If parsing fails, truncate the value
          value = value.substring(0, maxSessionStorageSize);
        }
      }
      
      sessionStorage.setItem(key, value);
    } catch (error) {
      if (error instanceof DOMException && error.name === 'QuotaExceededError') {
        console.error('Session storage quota exceeded. Clearing old data...');
        
        // Clear old conversations to make space
        const keysToKeep = ['selectedConversation', 'settings', 'folders'];
        const allKeys = Object.keys(sessionStorage);
        
        allKeys.forEach(key => {
          if (!keysToKeep.includes(key) && key.startsWith('conversation-')) {
            sessionStorage.removeItem(key);
          }
        });
        
        // Try again
        try {
          sessionStorage.setItem(key, value);
        } catch (retryError) {
          console.error('Failed to save to session storage even after cleanup');
        }
      } else {
        console.error('Error saving to session storage:', error);
      }
    }
  },
  
  getItem: (key: string): string | null => {
    try {
      return sessionStorage.getItem(key);
    } catch (error) {
      console.error('Error reading from session storage:', error);
      return null;
    }
  },
  
  removeItem: (key: string) => {
    try {
      sessionStorage.removeItem(key);
    } catch (error) {
      console.error('Error removing from session storage:', error);
    }
  },
  
  clear: () => {
    try {
      sessionStorage.clear();
    } catch (error) {
      console.error('Error clearing session storage:', error);
    }
  },
};
