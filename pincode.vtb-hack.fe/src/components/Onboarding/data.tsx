export const STEPS = [
	{
		title: "Подключение БД",
		short_description: "Подключите PostgreSQL для анализа",
		full_description: "Подключите вашу базу данных PostgreSQL для получения анализа производительности.",
		media: {
			type: "image" as const,
			src: "/images/onboarding1.png",
			alt: "Подключение базы данных",
		},
		// action: {
		// 	label: "Подключить БД",
		// 	onClick: () => {
		// 		setOnboardingOpen(false);
		// 		open();
		// 	},
		// },
	},
	{
		title: "Рекомендации по кластеру от ИИ",
		short_description: "Получайте персонализированные советы",
		full_description: "Детальный анализ метрик и настроек БД, а также алгоритмические рекомендации.",
		media: {
			type: "image" as const,
			src: "/images/onboarding2.png",
			alt: "ИИ рекомендации",
		},
		// action: {
		// 	label: "Посмотреть пример рекомендаций",
		// 	href: "/databases",
		// },
	},
	{
		title: "Проанализируй запрос",
		short_description: "Анализ производительности SQL",
		full_description: "Загрузите ваши SQL-запросы для детального анализа с помощью алгоритмов и ИИ.",
		media: {
			type: "image" as const,
			src: "/images/onboarding3.png",
			alt: "Анализ запросов",
		},
		// action: {
		// 	label: "Перейти к анализу запросов",
		// 	href: "/queries",
		// },
	},
	{
		title: "Замени оптимизированным от ИИ",
		short_description: "Автоматическая оптимизация запросов",
		full_description:
			"Получите оптимизированные версии ваших SQL-запросов от нашего ИИ и сравните их с изначальными.",
		media: {
			type: "image" as const,
			src: "/images/onboarding4.png",
			alt: "Оптимизация ИИ",
		},
		// action: {
		// 	label: "Начать оптимизацию",
		// 	onClick: () => {
		// 		toast.success("Готово! Теперь вы можете начать работу с платформой 🚀");
		// 	},
		// },
	},
];
