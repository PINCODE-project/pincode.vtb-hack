# DB Explorer

🗄️ Современные сервисы всё чаще сталкиваются с проблемами производительности баз данных. Даже один неудачный SQL-запрос
может привести к падению сервиса, долгим простоям и огромным потерям. При этом найти и исправить такие запросы без
глубоких экспертных знаний — задача крайне сложная.

💪 Мы создали платформу для проактивного анализа и оптимизации PostgreSQL. Она позволяет обнаружить проблемные запросы
ещё на этапе разработки, оценить их «стоимость» без выполнения и дать рекомендации по оптимизации структуры БД и
настроек PostgreSQL.

🤖 Умные рекомендации позволяют не только переписывать запросы и добавлять индексы, но и предлагать оптимальные настройки
PostgreSQL — от autovacuum до work_mem. Всё это сопровождается встроенной документацией для быстрого понимания и
внедрения.

😎 А ещё сервис легко интегрируется в CI/CD, поэтому критические запросы отлавливаются до того, как попадут в прод.

Возможности сервиса:
- Мониторинг метрик БД в реальном времени (блокировки, autovacuum, кэш, временные файлы, индексы).
- Анализ SQL-запросов без выполнения (EXPLAIN + Generic Plan).
- AI-рекомендации по оптимизации запросов и конфигурации PostgreSQL.
- Алгоритмический анализ SQL и EXPLAIN по десяткам встроенных правил.
- Сравнение стоимости запросов до и после оптимизации.
- Поддержка кастомных правил анализа (на основе RegExp).
- Визуализация метрик и логов с выбором периода и графиками.
- Анализ истории запросов через pg_stat_statements.
- Встроенная документация для каждой рекомендации.

## Основные ссылки

[DB Explorer](https://db-explorer.pincode-infra.ru/)

[Документация к API](https://sql-analyzer.pincode-infra.ru/swagger/index.html/)

## Иллюстрации

![Demo1](https://github.com/PINCODE-project/pincode.vtb-hack/blob/main/service/demo1.png?raw=true "DB Explorer")
![Demo2](https://github.com/PINCODE-project/pincode.vtb-hack/blob/main/service/demo2.png?raw=true "DB Explorer")
![Demo3](https://github.com/PINCODE-project/pincode.vtb-hack/blob/main/service/demo3.png?raw=true "DB Explorer")
![Demo4](https://github.com/PINCODE-project/pincode.vtb-hack/blob/main/service/demo4.png?raw=true "DB Explorer")
![Demo5](https://github.com/PINCODE-project/pincode.vtb-hack/blob/main/service/demo5.png?raw=true "DB Explorer")
![Demo6](https://github.com/PINCODE-project/pincode.vtb-hack/blob/main/service/demo6.png?raw=true "DB Explorer")
![Demo7](https://github.com/PINCODE-project/pincode.vtb-hack/blob/main/service/demo7.png?raw=true "DB Explorer")
![Demo8](https://github.com/PINCODE-project/pincode.vtb-hack/blob/main/service/demo8.png?raw=true "DB Explorer")
![Demo9](https://github.com/PINCODE-project/pincode.vtb-hack/blob/main/service/demo9.png?raw=true "DB Explorer")
![Demo10](https://github.com/PINCODE-project/pincode.vtb-hack/blob/main/service/demo10.png?raw=true "DB Explorer")