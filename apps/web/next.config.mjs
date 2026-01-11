//@ts-check

import { composePlugins, withNx } from '@nx/next';

// Determine if we're in production
const isProd = process.env.NODE_ENV === 'production';

// API URL for connect-src CSP directive
const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

// Build script-src with conditional unsafe-eval
const scriptSrc = [
  "'self'",
  "'unsafe-inline'", // Required for Next.js
  ...(isProd ? [] : ["'unsafe-eval'"]), // Only allow eval in development
].join(' ');

// Build connect-src with API origin
const connectSrc = [
  "'self'",
  "ws:",
  "wss:", // WebSocket support for SignalR
  apiUrl, // Allow API calls to configured API origin
].join(' ');

// Security headers configuration
const securityHeaders = [
  {
    // Content Security Policy - restricts resource loading
    key: 'Content-Security-Policy',
    value: [
      "default-src 'self'",
      `script-src ${scriptSrc}`,
      "style-src 'self' 'unsafe-inline'", // unsafe-inline needed for Tailwind
      "img-src 'self' data: blob:",
      "font-src 'self'",
      `connect-src ${connectSrc}`,
      "object-src 'none'", // Prevent loading of plugins
      "frame-ancestors 'none'",
      "base-uri 'self'",
      "form-action 'self'",
    ].join('; '),
  },
  {
    // Prevent MIME type sniffing
    key: 'X-Content-Type-Options',
    value: 'nosniff',
  },
  {
    // Control iframe embedding
    key: 'X-Frame-Options',
    value: 'DENY',
  },
  {
    // Control referrer information
    key: 'Referrer-Policy',
    value: 'strict-origin-when-cross-origin',
  },
  {
    // Disable browser features we don't need
    key: 'Permissions-Policy',
    value: 'camera=(), microphone=(), geolocation=()',
  },
];

// Add HSTS only in production (requires HTTPS)
if (isProd) {
  securityHeaders.push({
    key: 'Strict-Transport-Security',
    value: 'max-age=31536000; includeSubDomains',
  });
}

/**
 * @type {import('@nx/next/plugins/with-nx').WithNxOptions}
 **/
const nextConfig = {
  // Use this to set Nx-specific options
  // See: https://nx.dev/recipes/next/next-config-setup
  nx: {},
  output: 'standalone', // Required for Docker deployment
  
  // Security headers
  async headers() {
    return [
      {
        // Apply to all routes
        source: '/:path*',
        headers: securityHeaders,
      },
    ];
  },
};

const plugins = [
  // Add more Next.js plugins to this list if needed.
  withNx,
];

export default composePlugins(...plugins)(nextConfig);
