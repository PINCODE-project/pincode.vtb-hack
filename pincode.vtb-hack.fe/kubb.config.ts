import { config } from "@dotenvx/dotenvx";
import { defineConfig } from "@kubb/core";
import { pluginOas } from "@kubb/plugin-oas";
import { pluginReactQuery } from "@kubb/plugin-react-query";
import { pluginTs } from "@kubb/plugin-ts";
import { pluginClient } from "@kubb/plugin-client";

config({
    path: `.env.${process.env.NODE_ENV ?? "development"}`,
});

export default defineConfig({
    input: {
        path: "openapi.json",
    },
    output: {
        path: "./src/generated",
        clean: true,
    },
    hooks: {
        done: ["prettier --write --log-level silent ./src/generated", "eslint --fix --max-warnings=0 ./src/generated"],
    },
    plugins: [
        pluginOas({ validate: true }),
        pluginClient({
            importPath: "@src/lib/client",
            baseURL: process.env.API_BASE_URL ?? "/api",
            output: {
                path: "clients",
                banner: `
                /* eslint-disable */
                // @ts-nocheck
                `,
            },
        }),
        pluginTs({
            enumType: "constEnum",
            output: {
                path: "models",
                banner: `
                /* eslint-disable */
                `,
            },
            unknownType: "unknown",
        }),
        pluginReactQuery({
            client: {
                importPath: "@src/lib/client",
            },
            output: { path: "hooks" },
            group: {
                type: "tag",
                name({ group }) {
                    return `${group}`;
                },
            },
        }),
    ],
});
