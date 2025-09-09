import type { NextConfig } from "next";

const nextConfig: NextConfig = {
	reactStrictMode: true,
	output: "standalone",
	env: {
		API_BASE_URL: process.env.API_BASE_URL,
	},
};

export default nextConfig;
