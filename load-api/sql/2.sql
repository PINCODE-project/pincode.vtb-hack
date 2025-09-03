-- 2) Ежедневные заказы за 30 дней + скользящее среднее за 7 дней (generate_series + оконка)
WITH days AS (
  SELECT generate_series(
    date_trunc('day', now()) - interval '29 days',
    date_trunc('day', now()),
    interval '1 day'
  )::date AS day
),
per_day AS (
  SELECT d.day, COUNT(o.id) AS orders_cnt
  FROM marketplace.days d
  LEFT JOIN marketplace.orders o ON o.placed_at::date = d.day
  GROUP BY d.day
)
SELECT
  day,
  orders_cnt,
  ROUND(AVG(orders_cnt) OVER (
    ORDER BY day
    ROWS BETWEEN 6 PRECEDING AND CURRENT ROW
  ), 2) AS ma7
FROM per_day
ORDER BY day;
