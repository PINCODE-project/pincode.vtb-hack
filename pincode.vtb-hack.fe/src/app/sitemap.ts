import { MetadataRoute } from "next";

export default function sitemap(): MetadataRoute.Sitemap {
	const baseUrl = "https://db-explorer.pincode-infra.ru";
	const currentDate = new Date();

	return [
		{
			url: baseUrl,
			lastModified: currentDate,
			changeFrequency: "weekly" as const,
			priority: 1.0,
		},
		{
			url: `${baseUrl}/databases`,
			lastModified: currentDate,
			changeFrequency: "daily" as const,
			priority: 0.9,
		},
		{
			url: `${baseUrl}/queries`,
			lastModified: currentDate,
			changeFrequency: "daily" as const,
			priority: 0.8,
		},
	];
}
