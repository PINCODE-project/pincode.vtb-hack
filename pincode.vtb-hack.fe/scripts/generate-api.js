#!/usr/bin/env node

const https = require("https");
const http = require("http");
const fs = require("fs");
const path = require("path");
const { exec } = require("child_process");
const { config } = require("@dotenvx/dotenvx");

config({
	path: `.env.${process.env.NODE_ENV ?? "development"}`,
});

/**
 * Загружает OpenAPI схему из API и генерирует типы с помощью kubb
 */
async function generateApi() {
	try {
		// Читаем переменные окружения
		const apiBaseUrl = "https://localhost:3000/backend";
		const docsEndpoint = `${apiBaseUrl}/api/core/docs-json`;

		console.log(`🚀 Загружаем OpenAPI схему из: ${docsEndpoint}`);

		// Загружаем схему
		const openApiSchema = await fetchOpenApiSchema(docsEndpoint);

		// Сохраняем в openapi.json
		const openApiPath = path.join(process.cwd(), "openapi.json");
		fs.writeFileSync(openApiPath, JSON.stringify(openApiSchema, null, 2));
		console.log(`✅ OpenAPI схема сохранена в: ${openApiPath}`);

		// Запускаем kubb generate
		console.log("🔧 Генерируем типы и хуки...");
		await runKubbGenerate();

		console.log("🎉 Генерация завершена успешно!");
	} catch (error) {
		console.error("❌ Ошибка при генерации API:", error.message);
		process.exit(1);
	}
}

/**
 * Загружает OpenAPI схему по HTTP/HTTPS
 * @param {string} url URL для загрузки схемы
 * @returns {Promise<Object>} OpenAPI схема
 */
function fetchOpenApiSchema(url) {
	return new Promise((resolve, reject) => {
		const isHttps = url.startsWith("https://");
		const isLocalhost = url.includes("localhost") || url.includes("127.0.0.1");
		const client = isHttps ? https : http;

		const options = {
			headers: {
				Accept: "application/json",
				"User-Agent": "OpenAPI-Schema-Fetcher/1.0.0",
			},
		};

		// Для HTTPS и localhost игнорируем проверку SSL сертификатов
		if (isHttps && isLocalhost) {
			console.log("⚠️  Игнорируем SSL сертификаты для localhost");
			options.rejectUnauthorized = false;
			options.requestCert = false;
			options.agent = new https.Agent({
				rejectUnauthorized: false,
			});
		}

		const req = client.get(url, options, (res) => {
			let data = "";

			// Обработка редиректов
			if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
				console.log(`Редирект на: ${res.headers.location}`);
				return fetchOpenApiSchema(res.headers.location).then(resolve).catch(reject);
			}

			if (res.statusCode !== 200) {
				reject(new Error(`HTTP ${res.statusCode}: ${res.statusMessage} при запросе к ${url}`));
				return;
			}

			res.on("data", (chunk) => (data += chunk));
			res.on("end", () => {
				try {
					const schema = JSON.parse(data);
					resolve(schema);
				} catch (parseError) {
					reject(new Error(`Ошибка парсинга JSON: ${parseError.message}`));
				}
			});
		});

		req.on("error", (error) => {
			let errorMessage = `Ошибка сети: ${error.message}`;

			if (error.code === "ECONNREFUSED") {
				errorMessage = `Невозможно подключиться к серверу. Убедитесь, что бэкенд запущен по адресу: ${url}`;
			} else if (error.message.includes("certificate")) {
				errorMessage = `Проблема с SSL сертификатом. Попробуйте использовать HTTP вместо HTTPS для локальной разработки.`;
			}

			reject(new Error(errorMessage));
		});

		// Таймаут 30 секунд
		req.setTimeout(30000, () => {
			req.destroy();
			reject(new Error("Таймаут запроса (30с)"));
		});
	});
}

/**
 * Запускает kubb generate
 * @returns {Promise<void>}
 */
function runKubbGenerate() {
	return new Promise((resolve, reject) => {
		exec("yarn kubb generate", (error, stdout, stderr) => {
			if (error) {
				reject(new Error(`Ошибка выполнения kubb generate: ${error.message}`));
				return;
			}

			if (stderr) {
				console.warn("⚠️ Предупреждения kubb:", stderr);
			}

			if (stdout) {
				console.log(stdout);
			}

			resolve();
		});
	});
}

// Запускаем скрипт если он вызван напрямую
if (require.main === module) {
	generateApi();
}

module.exports = { generateApi };
