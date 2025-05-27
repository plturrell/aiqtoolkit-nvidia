import { NextRequest, NextResponse } from 'next/server';
import axios from 'axios';

// Environment variables
const NVIDIA_API_KEY = process.env.NVIDIA_API_KEY || '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';
const BREV_API_ENDPOINT = process.env.BREV_API_ENDPOINT || 'https://your-brev-instance.brev.dev/api';
const LANGCHAIN_ENDPOINT = process.env.LANGCHAIN_ENDPOINT || 'https://langchain-structured-report-generation-6d35aa.brev.dev/api';

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const { query, use_mcp, use_brev } = body;

    // Initialize context
    const context = {
      query,
      timestamp: new Date().toISOString(),
      sources: []
    };

    // 1. Search web using MCP (can be replaced with external API)
    let webResults = [];
    if (use_mcp) {
      try {
        const webSearch = await axios.post('https://api.duckduckgo.com/api/search', {
          q: query,
          format: 'json'
        });
        webResults = webSearch.data.Results || [];
        context.sources.push('web_search');
      } catch (e) {
        console.error('Web search error:', e);
      }
    }

    // 2. Integrate with Brev LangChain if enabled
    let langchainResults = null;
    if (use_brev) {
      try {
        const langchainResponse = await axios.post(LANGCHAIN_ENDPOINT + '/generate', {
          prompt: query,
          context: webResults
        }, {
          headers: {
            'Authorization': `Bearer ${process.env.BREV_API_KEY}`,
            'Content-Type': 'application/json'
          }
        });
        langchainResults = langchainResponse.data;
        context.sources.push('langchain_brev');
      } catch (e) {
        console.error('LangChain error:', e);
      }
    }

    // 3. Generate response using NVIDIA NIM
    const prompt = `
User Query: ${query}

Web Search Results: ${JSON.stringify(webResults.slice(0, 3))}
LangChain Analysis: ${JSON.stringify(langchainResults)}

Based on the above information, provide a comprehensive response.
`;

    const nimResponse = await axios.post(
      'https://integrate.api.nvidia.com/v1/chat/completions',
      {
        model: 'meta/llama3-70b-instruct',
        messages: [
          { role: 'system', content: 'You are a helpful AI assistant with web search and LangChain capabilities.' },
          { role: 'user', content: prompt }
        ],
        temperature: 0.7,
        max_tokens: 1000
      },
      {
        headers: {
          'Authorization': `Bearer ${NVIDIA_API_KEY}`,
          'Content-Type': 'application/json'
        }
      }
    );

    const response = nimResponse.data.choices[0].message.content;

    return NextResponse.json({
      response,
      sources_used: context.sources,
      context: {
        ...context,
        web_results: webResults.slice(0, 3),
        langchain_results: langchainResults
      }
    });

  } catch (error) {
    console.error('API error:', error);
    return NextResponse.json(
      { error: 'Failed to process request' },
      { status: 500 }
    );
  }
}