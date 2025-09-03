-- 4) «Лучший» адрес для каждого пользователя: дефолтный, иначе самый новый (LATERAL)
SELECT
  u.id AS user_id,
  u.email,
  a.id AS address_id,
  a.line1, a.city, a.country_code, a.is_default, a.created_at
FROM users u
LEFT JOIN LATERAL (
  SELECT *
  FROM marketplace.addresses a
  WHERE a.user_id = u.id
  ORDER BY a.is_default DESC, a.created_at DESC
  LIMIT 1
) a ON true
ORDER BY u.created_at;