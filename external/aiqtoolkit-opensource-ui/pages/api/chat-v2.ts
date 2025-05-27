import { ChatBody } from '@/types/chat';
import { getApiEndpoint } from '@/utils/env';
import { APIErrorHandler } from '@/utils/api/error-handler';

export const config = {
  runtime: 'edge',
  api: {
    bodyParser: {
      sizeLimit: '5mb',
    },
  },
};

const handler = async (req: Request): Promise<Response> => {
  // Extract the request body
  let { chatCompletionURL = '', messages = [], additionalProps = { enableIntermediateSteps: true } } = (await req.json()) as ChatBody;

  // Use configured endpoint if no URL provided
  if (!chatCompletionURL) {
    chatCompletionURL = getApiEndpoint('chat');
  }

  try {
    let payload;
    
    // For generate endpoint, the request schema is {input_message: "user question"}
    if (chatCompletionURL.includes('generate')) {
      if (messages?.length > 0 && messages[messages.length - 1]?.role === 'user') {
        payload = {
          input_message: messages[messages.length - 1]?.content ?? ''
        };
      } else {
        throw new Error('User message not found: messages array is empty or invalid.');
      }
    }
    // For chat endpoint, use OpenAI compatible schema
    else {
      payload = {
        messages,
        model: "string",
        temperature: 0,
        max_tokens: 0,
        top_p: 0,
        use_knowledge_base: true,
        top_k: 0,
        collection_name: "string",
        stop: true,
        additionalProp1: {}
      };
    }

    console.log('Making request to:', chatCompletionURL);

    const response = await fetch(chatCompletionURL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    console.log('Received response:', response.status);

    if (!response.ok) {
      const error = await APIErrorHandler.handleFetchError(response);
      const userMessage = APIErrorHandler.formatUserMessage(error);
      
      return new Response(userMessage, {
        status: 200,
        headers: { 'Content-Type': 'text/plain' },
      });
    }

    // Handle streaming responses
    if (chatCompletionURL.includes('stream')) {
      console.log('Processing streaming response');
      const encoder = new TextEncoder();
      const decoder = new TextDecoder();

      const responseStream = new ReadableStream({
        async start(controller) {
          const reader = response?.body?.getReader();
          let buffer = '';
          let counter = 0;
          
          try {
            while (true) {
              const { done, value } = await reader?.read();
              if (done) break;

              buffer += decoder.decode(value, { stream: true });
              const lines = buffer.split('\n');
              buffer = lines.pop() || '';

              for (const line of lines) {
                if (line.startsWith('data: ')) {
                  const data = line.slice(5);
                  if (data.trim() === '[DONE]') {
                    controller.close();
                    return;
                  }
                  
                  try {
                    const parsed = JSON.parse(data);
                    const content = parsed.choices[0]?.message?.content || parsed.choices[0]?.delta?.content || '';
                    if (content) {
                      controller.enqueue(encoder.encode(content));
                    }
                  } catch (error) {
                    console.error('Error parsing JSON:', error);
                  }
                }
                
                // Handle intermediate steps
                if (line.startsWith('intermediate_data: ') && additionalProps.enableIntermediateSteps) {
                  const data = line.split('intermediate_data: ')[1];
                  if (data.trim() === '[DONE]') {
                    controller.close();
                    return;
                  }
                  
                  try {
                    const intermediate = JSON.parse(data);
                    const intermediateMessage = {
                      id: intermediate.id || '',
                      status: intermediate.status || 'in_progress',
                      error: intermediate.error || '',
                      type: 'system_intermediate',
                      parent_id: intermediate.parent_id || 'default',
                      intermediate_parent_id: intermediate.intermediate_parent_id || 'default',
                      content: {
                        name: intermediate.name || 'Step',
                        payload: intermediate.payload || 'No details',
                      },
                      time_stamp: intermediate.time_stamp || new Date().toISOString(),
                      index: counter++
                    };
                    
                    const messageString = `<intermediatestep>${JSON.stringify(intermediateMessage)}</intermediatestep>`;
                    controller.enqueue(encoder.encode(messageString));
                  } catch (error) {
                    console.error('Error parsing intermediate data:', error);
                  }
                }
              }
            }
          } catch (error) {
            console.error('Stream reading error:', error);
            controller.close();
          } finally {
            console.log('Response processing completed');
            controller.close();
            reader?.releaseLock();
          }
        },
      });

      return new Response(responseStream);
    }
    
    // Handle non-streaming responses
    else {
      console.log('Processing non-streaming response');
      const data = await response.text();
      let parsed = null;
      
      try {
        parsed = JSON.parse(data);
      } catch (error) {
        console.error('Error parsing JSON response:', error);
      }
      
      // Extract content from various response formats
      const content =
        parsed?.output ||
        parsed?.answer ||
        parsed?.value ||
        (Array.isArray(parsed?.choices) ? parsed.choices[0]?.message?.content : null) ||
        parsed ||
        data;
      
      if (content) {
        console.log('Response processing completed');
        return new Response(content);
      } else {
        console.error('Unable to extract content from response');
        return new Response(data);
      }
    }
  } catch (error: any) {
    console.error('Error during request:', error);
    const apiError = APIErrorHandler.parseError(error);
    const userMessage = APIErrorHandler.formatUserMessage(apiError);
    
    return new Response(userMessage, { status: 200 });
  }
};

export default handler;