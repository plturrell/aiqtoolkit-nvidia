import { NextRequest, NextResponse } from 'next/server';
import axios from 'axios';

// Environment variables
const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:8000';
const BREV_API_URL = process.env.BREV_API_URL || 'https://api.brev.dev';
const LANGCHAIN_API_KEY = process.env.LANGCHAIN_API_KEY;
const OPENAI_API_KEY = process.env.OPENAI_API_KEY;

export async function POST(request: NextRequest) {
  try {
    const { query, use_mcp, use_brev } = await request.json();

    // Prepare the request payload
    const payload = {
      query,
      use_mcp,
      use_brev,
      session_id: request.headers.get('x-session-id') || 'vercel-session',
      user_id: request.headers.get('x-user-id') || 'vercel-user',
      config: {
        langchain_api_key: LANGCHAIN_API_KEY,
        openai_api_key: OPENAI_API_KEY
      }
    };

    // If using Brev integration
    if (use_brev && process.env.BREV_SHELL_ID) {
      try {
        // Connect to Brev environment
        const brevResponse = await axios.post(
          `${BREV_API_URL}/shells/${process.env.BREV_SHELL_ID}/execute`,
          {
            command: 'python run_langchain_report.py',
            params: { query }
          },
          {
            headers: {
              'Authorization': `Bearer ${process.env.BREV_API_KEY}`,
              'Content-Type': 'application/json'
            }
          }
        );

        if (brevResponse.data && brevResponse.data.result) {
          return NextResponse.json({
            response: brevResponse.data.result,
            source: 'brev',
            emotion: analyzeEmotion(brevResponse.data.result)
          });
        }
      } catch (brevError) {
        console.error('Brev integration error:', brevError);
        // Fallback to regular backend
      }
    }

    // Regular backend call
    const response = await axios.post(`${BACKEND_URL}/chat`, payload, {
      headers: {
        'Content-Type': 'application/json',
        'X-API-Key': process.env.API_KEY || ''
      },
      timeout: 30000
    });

    // Add emotion analysis for Unity avatar
    const emotion = analyzeEmotion(response.data.response);

    return NextResponse.json({
      response: response.data.response,
      source: response.data.source || 'backend',
      emotion,
      metadata: response.data.metadata
    });

  } catch (error) {
    console.error('Chat API error:', error);
    
    // Fallback response
    const fallbackResponse = 'I apologize, but I\'m having trouble processing your request. Please try again.';
    
    return NextResponse.json({
      response: fallbackResponse,
      error: true,
      emotion: 'confused'
    }, { status: 500 });
  }
}

// Simple emotion analyzer for avatar expressions
function analyzeEmotion(text: string): string {
  const lowerText = text.toLowerCase();
  
  if (lowerText.includes('sorry') || lowerText.includes('apologize')) {
    return 'apologetic';
  } else if (lowerText.includes('happy') || lowerText.includes('great') || lowerText.includes('excellent')) {
    return 'happy';
  } else if (lowerText.includes('confused') || lowerText.includes('unclear')) {
    return 'confused';
  } else if (lowerText.includes('thinking') || lowerText.includes('processing')) {
    return 'thinking';
  } else if (lowerText.includes('!')) {
    return 'excited';
  }
  
  return 'neutral';
}