-- 1) Последний заказ и общее число заказов по каждому пользователю (LATERAL + оконки)
SELECT
  u.id        AS user_id,
  u.email,
  stats.orders_count,
  o_last.id   AS last_order_id,
  o_last.placed_at AS last_order_at,
  o_last.total_amount
FROM marketplace.users u
LEFT JOIN LATERAL (
  SELECT COUNT(*) AS orders_count
  FROM marketplace.orders o WHERE o.user_id = u.id
) stats ON true
LEFT JOIN LATERAL (
  SELECT o.*
  FROM marketplace.orders o
  WHERE o.user_id = u.id
  ORDER BY o.placed_at DESC, o.id DESC
  LIMIT 1
) o_last ON true
ORDER BY last_order_at DESC NULLS LAST
LIMIT 100;