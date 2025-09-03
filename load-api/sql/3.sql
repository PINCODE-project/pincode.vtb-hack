-- 3) Второй заказ каждого клиента (ROW_NUMBER)
SELECT *
FROM (
  SELECT
    o.*,
    ROW_NUMBER() OVER (PARTITION BY o.user_id ORDER BY o.placed_at, o.id) AS rn
  FROM marketplace.orders o
) x
WHERE rn = 2
ORDER BY placed_at;