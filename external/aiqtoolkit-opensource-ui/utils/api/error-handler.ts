export interface APIError {
  message: string;
  code?: string;
  details?: any;
}

export class APIErrorHandler {
  static parseError(error: any): APIError {
    if (error instanceof Response) {
      return {
        message: `HTTP ${error.status}: ${error.statusText}`,
        code: error.status.toString(),
      };
    }
    
    if (error instanceof Error) {
      return {
        message: error.message,
        code: 'UNKNOWN_ERROR',
      };
    }
    
    if (typeof error === 'string') {
      // Check for HTML responses
      if (error.includes('<!DOCTYPE html>')) {
        if (error.includes('404')) {
          return {
            message: 'API endpoint not found. Please check your configuration.',
            code: '404',
          };
        }
        return {
          message: 'Received an HTML response instead of JSON. The API server might be misconfigured.',
          code: 'HTML_RESPONSE',
        };
      }
      
      return {
        message: error,
        code: 'STRING_ERROR',
      };
    }
    
    return {
      message: 'An unknown error occurred',
      code: 'UNKNOWN',
      details: error,
    };
  }
  
  static formatUserMessage(error: APIError): string {
    let message = error.message;
    
    if (error.code === '404') {
      message = 'The requested API endpoint was not found. Please check your server configuration.';
    } else if (error.code === 'HTML_RESPONSE') {
      message = 'The server returned an HTML page instead of data. This usually means the API URL is incorrect.';
    } else if (error.code === 'NETWORK_ERROR') {
      message = 'Unable to connect to the server. Please check if the server is running.';
    }
    
    return `Something went wrong: ${message}\n\n<details><summary>Error Details</summary>\nCode: ${error.code || 'UNKNOWN'}\n${error.details ? JSON.stringify(error.details, null, 2) : ''}</details>`;
  }
  
  static async handleFetchError(response: Response): Promise<APIError> {
    let errorText = '';
    
    try {
      errorText = await response.text();
    } catch {
      errorText = 'Unable to read error response';
    }
    
    return this.parseError(errorText);
  }
}
