import { Toaster } from 'react-hot-toast';
import { QueryClient, QueryClientProvider } from 'react-query';

import { appWithTranslation } from 'next-i18next';
import type { AppProps } from 'next/app';
import { Inter } from 'next/font/google';

import '@/styles/globals.css';
import ErrorBoundary from '@/components/ErrorBoundary/ErrorBoundary';

const inter = Inter({ subsets: ['latin'] });

function App({ Component, pageProps }: AppProps<{}>) {

  const queryClient = new QueryClient();

  return (
    <ErrorBoundary>
      <div className={inter.className}>
        <Toaster
          toastOptions={{
            style: {
              maxWidth: 500,
              wordBreak: 'break-all',
            },
          }}
        />
        <QueryClientProvider client={queryClient}>
          <Component {...pageProps} />
        </QueryClientProvider>
      </div>
    </ErrorBoundary>
  );
}

export default appWithTranslation(App);