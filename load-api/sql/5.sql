-- 5) Топ-3 самых дорогих товара у каждого продавца (DENSE_RANK по цене)
SELECT *
FROM (
  SELECT
    v.id   AS vendor_id,
    v.display_name,
    p.id   AS product_id,
    p.name AS product_name,
    p.unit_price,
    DENSE_RANK() OVER (PARTITION BY v.id ORDER BY p.unit_price DESC, p.id) AS rnk
  FROM marketplace.vendors v
  JOIN marketplace.products p ON p.vendor_id = v.id
  WHERE p.is_active
) t
WHERE rnk <= 3
ORDER BY display_name, unit_price DESC;