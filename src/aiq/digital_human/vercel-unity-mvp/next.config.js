/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  
  // Unity WebGL specific configuration
  async headers() {
    return [
      {
        source: '/unity/:path*',
        headers: [
          {
            key: 'Cross-Origin-Embedder-Policy',
            value: 'require-corp'
          },
          {
            key: 'Cross-Origin-Opener-Policy',
            value: 'same-origin'
          },
          {
            key: 'Cache-Control',
            value: 'public, max-age=31536000, immutable'
          }
        ]
      }
    ];
  },

  // Enable WebAssembly
  webpack: (config) => {
    config.experiments = {
      ...config.experiments,
      syncWebAssembly: true,
      asyncWebAssembly: true
    };
    
    // Unity WebGL loader configuration
    config.module.rules.push({
      test: /\.(wasm)$/,
      type: 'webassembly/async'
    });

    return config;
  },

  // Environment variables
  env: {
    NEXT_PUBLIC_WS_URL: process.env.NEXT_PUBLIC_WS_URL || 'http://localhost:8088',
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000',
    BREV_SHELL_ID: process.env.BREV_SHELL_ID || 'langchain-structured-report-generation-6d35aa'
  },

  // Enable static export for Unity files
  output: 'standalone',
  
  // Optimize for production
  swcMinify: true,
  compress: true,
  
  // Image optimization
  images: {
    domains: ['models.readyplayerme.com'],
    unoptimized: process.env.NODE_ENV === 'development'
  }
};

module.exports = nextConfig;