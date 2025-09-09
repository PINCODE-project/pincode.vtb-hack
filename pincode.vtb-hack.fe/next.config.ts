import type { NextConfig } from "next";

const nextConfig: NextConfig = {
	reactStrictMode: true,
	output: "standalone",
	env: {
		API_BASE_URL: process.env.API_BASE_URL,
	},
	async rewrites() {
		return [
			{
				source: "/backend/:path*",
				destination: `http://localhost:9001/:path*`,
			},
		];
	},
};

export default nextConfig;
