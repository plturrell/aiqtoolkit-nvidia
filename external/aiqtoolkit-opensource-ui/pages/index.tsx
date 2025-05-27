import React from 'react';
import Head from 'next/head';

/**
 * AIQToolkit Beautiful Homepage
 * 
 * Redirects to the beautiful showcase page
 */
export default function Home() {
  React.useEffect(() => {
    // Redirect to beautiful page
    window.location.href = '/beautiful';
  }, []);

  return (
    <>
      <Head>
        <title>AIQToolkit - Beautiful 10/10 Design</title>
        <meta name="description" content="Experience the Jony Ive-inspired redesign of AIQToolkit" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <link rel="icon" href="/favicon.ico" />
      </Head>
      
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Redirecting to beautiful experience...</p>
        </div>
      </div>
    </>
  );
}

// No server-side rendering needed
export const getServerSideProps = async () => {
  return {
    props: {}
  };
};