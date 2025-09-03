-- 10) Медиана и 90-й перцентиль рейтинга по товарам (ordered-set aggregates)
SELECT
  r.product_id,
  p.name AS product_name,
  COUNT(*)                         AS reviews_cnt,
  AVG(r.rating)::numeric(10,2)     AS avg_rating,
  percentile_cont(0.5) WITHIN GROUP (ORDER BY r.rating) AS median_rating,
  percentile_cont(0.9) WITHIN GROUP (ORDER BY r.rating) AS p90_rating
FROM marketplace.reviews r
JOIN marketplace.products p ON p.id = r.product_id
GROUP BY r.product_id, p.name
ORDER BY reviews_cnt DESC, product_name;