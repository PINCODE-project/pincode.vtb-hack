# Многоэтапная сборка для оптимизации размера образа
FROM node:23-alpine AS base

# Устанавливаем зависимости для всех стадий
WORKDIR /app

# Копируем package.json и yarn конфигурацию
COPY package.json yarn.lock .yarnrc.yml ./
COPY .yarn ./.yarn

# Стадия установки зависимостей
FROM base AS deps
RUN yarn install --immutable

# Стадия сборки
FROM base AS builder
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Генерируем API и собираем проект
# Используем существующий openapi.json и генерируем типы только локально
RUN yarn kubb generate
RUN yarn build

# Стадия production
FROM node:23-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production \
    NEXT_TELEMETRY_DISABLED=1 \
    PORT=8080 \
    HOSTNAME=0.0.0.0

# Создаем пользователя nextjs
RUN addgroup --system --gid 1001 nextjs \
    && adduser --system --uid 1001 nextjs

# Копируем необходимые файлы из builder
COPY --from=builder --chown=nextjs:nextjs /app/public ./public
COPY --from=builder --chown=nextjs:nextjs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nextjs /app/.next/static ./.next/static

USER nextjs

EXPOSE 8080

# Запускаем приложение (Next.js standalone создает server.js)
CMD ["node", "server.js"]
