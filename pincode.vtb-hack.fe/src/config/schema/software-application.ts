import { SoftwareApplication, WithContext } from "schema-dts";

/**
 * JSON-LD схема для программного приложения согласно стандарту Schema.org
 * Используется для SEO и структурированных данных
 */
export const softwareApplicationSchema: WithContext<SoftwareApplication> = {
	"@context": "https://schema.org",
	"@type": "SoftwareApplication",
	name: "DB Explorer",
	description:
		"Умный инструмент для проактивного контроля и оптимизации SQL-запросов PostgreSQL с анализом производительности и автоматическими рекомендациями",
	url: "https://db-explorer.pincode-infra.ru",
	applicationCategory: "DatabaseTool",
	operatingSystem: "Web Browser",
	offers: {
		"@type": "Offer",
		price: "0",
		priceCurrency: "RUB",
	},
	publisher: {
		"@type": "Organization",
		name: "Команда ПИН-КОД",
		address: {
			"@type": "PostalAddress",
			addressCountry: "RU",
			addressLocality: "Yekaterinburg",
		},
	},
	featureList: [
		"Анализ плана выполнения SQL-запросов",
		"Автоматические рекомендации по оптимизации",
		"Прогнозирование ресурсоемкости запросов",
		"Интеграция с CI/CD",
		"Мониторинг производительности PostgreSQL",
	],
};
