import { Metadata } from "next";

export const metadata: Metadata = {
	metadataBase: new URL("https://db-explorer.pincode-infra.ru"),
	title: "DB Explorer",
	description:
		"Умный инструмент для проактивного контроля и оптимизации SQL-запросов PostgreSQL с анализом производительности и автоматическими рекомендациями",
	keywords: [
		"PostgreSQL",
		"SQL оптимизация",
		"Анализ запросов",
		"База данных",
		"Performance tuning",
		"EXPLAIN анализ",
		"Автовакуум",
		"Индексы",
		"CI/CD интеграция",
		"Мониторинг БД",
		"Оптимизация производительности",
		"SQL рекомендации",
	],
	authors: [
		{
			name: "Команда ПИН-КОД",
		},
	],
	applicationName: "DB Explorer",
	openGraph: {
		type: "website",
		locale: "ru_RU",
		url: "https://db-explorer.pincode-infra.ru/",
		siteName: "DB Explorer",
		title: "DB Explorer - умный инструмент для оптимизации PostgreSQL",
		description:
			"Анализ и оптимизация SQL-запросов с автоматическими рекомендациями, прогнозированием нагрузки и предотвращением проблем производительности",
	},
	twitter: {
		card: "summary_large_image",
		title: "DB Explorer - умный инструмент для оптимизации PostgreSQL",
		description: "Проактивный контроль SQL-запросов с анализом производительности и автоматическими рекомендациями",
	},
	robots: {
		index: true,
		follow: true,
		googleBot: {
			index: true,
			follow: true,
		},
	},
	alternates: {
		canonical: "https://db-explorer.pincode-infra.ru/",
	},
	category: "Database Tools",
};
