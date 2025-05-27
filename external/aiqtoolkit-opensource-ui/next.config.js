/** @type {import('next').NextConfig} */
const nextConfig = {
  // Remove standalone for Vercel deployment
  typescript: {
    // Ignore type errors during build for now
    ignoreBuildErrors: true,
  },
  eslint: {
    // Ignore ESLint errors during build
    ignoreDuringBuilds: true,
  },
  experimental: {
    serverActions: {
      bodySizeLimit: "5mb",
    },
  },
  images: {
    domains: ['localhost'],
    unoptimized: true,
  },
  webpack(config, { isServer, dev }) {
    config.experiments = {
      asyncWebAssembly: true,
      layers: true,
    };

    return config;
  },
  async redirects() {
    return [
      {
        source: '/',
        destination: '/beautiful',
        permanent: false,
      },
    ];
  },
  // Optimize for Vercel deployment
  swcMinify: true,
  compress: true,
  poweredByHeader: false,
  generateEtags: false,
};

module.exports = nextConfig;