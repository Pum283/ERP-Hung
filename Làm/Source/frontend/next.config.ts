import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Standalone for MonsterASP IIS + HttpPlatformHandler (Node)
  output: "standalone",
  eslint: { ignoreDuringBuilds: true },
  typescript: { ignoreBuildErrors: true },
};

export default nextConfig;
