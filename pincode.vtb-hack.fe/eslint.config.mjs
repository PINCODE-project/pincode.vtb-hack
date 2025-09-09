import { dirname } from "path";
import { fileURLToPath } from "url";
import { FlatCompat } from "@eslint/eslintrc";
import pluginQuery from "@tanstack/eslint-plugin-query";
import pluginReactHooks from "eslint-plugin-react-hooks";
import pluginPrettier from "eslint-plugin-prettier";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const compat = new FlatCompat({
	baseDirectory: __dirname,
});

const eslintConfig = [
	...compat.extends("next/core-web-vitals", "next/typescript"),
	{
		plugins: {
			"@tanstack/query": pluginQuery,
			"react-hooks": pluginReactHooks,
			prettier: pluginPrettier,
		},
		rules: {
			"react-hooks/exhaustive-deps": "error",
			"react-hooks/rules-of-hooks": "error",
			"prettier/prettier": "off",
		},
		ignores: ["node_modules", ".next", "dist", "build", "coverage", "*.config.js", "*.config.mjs", "openapi.json"],
	},
];

export default eslintConfig;
