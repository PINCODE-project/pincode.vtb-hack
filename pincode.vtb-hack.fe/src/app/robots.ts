import { MetadataRoute } from "next";

export default function robots(): MetadataRoute.Robots {
	return {
		rules: {
			userAgent: "*",
			allow: ["/"],
			disallow: ["/backend/", "/api/", "/admin/", "/_next/", "/private/"],
		},
		sitemap: ["https://db-explorer.pincode-infra.ru/sitemap.xml"],
		host: "https://db-explorer.pincode-infra.ru/",
	};
}
