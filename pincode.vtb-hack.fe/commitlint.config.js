"use strict";
const paint = {
  red: (str) => `\x1b[31m${str}\x1b[0m`,
  green: (str) => `\x1b[32m${str}\x1b[0m`,
  magenta: (str) => `\x1b[35m${str}\x1b[0m`,
};
module.exports = {
  extends: ["@commitlint/config-conventional"],
  rules: {
    "type-enum": [
      2,
      "always",
      [
        "build",
        "chore",
        "ci",
        "docs",
        "feat",
        "fix",
        "perf",
        "refactor",
        "release",
        "revert",
        "style",
        "test",
      ],
    ],
    // К месседже всегда должно быть упомянание задачи а Jira
    "references-empty": [2, "never"],
    // Разрешаем коммиты любой длины
    "header-max-length": () => [0, "always", Infinity],
  },
  parserPreset: {
    parserOpts: {
      referenceActions: null,
      issuePrefixes: [
        "(CORE|SP|MED|PP|VTB)-",
      ],
    },
  },
  ignores: [
    (message) =>
      message.includes("[manually skip]") ||
      message.includes("[ci skip]") ||
      message.includes("[master]"),
  ],
  helpUrl: `

  ${paint.red("Сообщение коммита не соответствет шаблону")}
    Шаблон текста коммита:     ${paint.green("тип_изменения: описание_изменения (id_задачи_в_Jira)")}
    Допустимые типы изменений: ${paint.green("build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test")}

    Пример: ${paint.magenta("fix: исправляет метод генерации данных (CORE-422)")}`,
};
