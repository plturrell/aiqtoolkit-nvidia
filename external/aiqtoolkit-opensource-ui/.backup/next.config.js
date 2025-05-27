const nextConfig = {
  output: 'standalone',
  typescript: {
    // Enable type checking in builds
    ignoreBuildErrors: false,
  },
  experimental: {
    serverActions: {
      bodySizeLimit: "5mb",
    },
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
    ]
  },
};

module.exports = nextConfig;
