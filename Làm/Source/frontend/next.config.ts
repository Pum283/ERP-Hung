import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Static export for MonsterASP IIS wwwroot (no Node process required)
  output: "export",
  images: { unoptimized: true },
  eslint: { ignoreDuringBuilds: true },
  typescript: { ignoreBuildErrors: true },
};

export default nextConfig;
