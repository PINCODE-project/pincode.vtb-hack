-- 9) Товары без единого заказа + доля таких по каталогу и по категории (ANTI JOIN + агрегация)
WITH ordered_products AS (
  SELECT DISTINCT oi.product_id FROM marketplace.order_items oi
),
unused AS (
  SELECT p.id, p.name, c.name AS category_name
  FROM marketplace.products p
  LEFT JOIN marketplace.categories c ON c.id = p.category_id
  WHERE NOT EXISTS (SELECT 1 FROM ordered_products op WHERE op.product_id = p.id)
),
by_cat AS (
  SELECT category_name, COUNT(*) AS cnt_unused
  FROM unused
  GROUP BY category_name
),
tot AS (SELECT COUNT(*) AS total_products FROM marketplace.products)
SELECT
  bc.category_name,
  bc.cnt_unused,
  ROUND(bc.cnt_unused::numeric / NULLIF(t.total_products, 0), 3) AS share_of_catalog
FROM by_cat bc
CROSS JOIN tot t
ORDER BY bc.cnt_unused DESC NULLS LAST;