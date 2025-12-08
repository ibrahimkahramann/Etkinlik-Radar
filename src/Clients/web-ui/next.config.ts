import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/events/:path*",
        destination: "http://localhost:80/api/events/:path*",
      },
      {
        source: "/api/followers/:path*",
        destination: "http://localhost:80/api/followers/:path*",
      },
      {
        source: "/api/identity/:path*",
        destination: "http://localhost:80/api/identity/:path*",
      },
    ];
  },
};

export default nextConfig;
