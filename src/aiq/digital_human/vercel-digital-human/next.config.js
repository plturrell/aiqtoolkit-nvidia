/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  env: {
    NVIDIA_API_KEY: process.env.NVIDIA_API_KEY,
    BREV_API_ENDPOINT: process.env.BREV_API_ENDPOINT,
    BREV_API_KEY: process.env.BREV_API_KEY,
    LANGCHAIN_ENDPOINT: process.env.LANGCHAIN_ENDPOINT,
  },
  async headers() {
    return [
      {
        source: '/api/:path*',
        headers: [
          { key: 'Access-Control-Allow-Origin', value: '*' },
          { key: 'Access-Control-Allow-Methods', value: 'GET,POST,PUT,DELETE,OPTIONS' },
          { key: 'Access-Control-Allow-Headers', value: 'Content-Type, Authorization' },
        ],
      },
    ];
  },
}

module.exports = nextConfig