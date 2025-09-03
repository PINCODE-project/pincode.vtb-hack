-- 8) Поиск дублей пользователей по email/username, пометка что оставить (оконные ранги)
WITH ranked AS (
  SELECT
    u.*,
    ROW_NUMBER() OVER (PARTITION BY u.email ORDER BY u.created_at DESC, u.id DESC)    AS rn_email,
    ROW_NUMBER() OVER (PARTITION BY u.username ORDER BY u.created_at DESC, u.id DESC) AS rn_username
  FROM marketplace.users u
),
marked AS (
  SELECT
    id, email, username, created_at,
    (rn_email > 1 OR rn_username > 1) AS is_duplicate
  FROM ranked
)
SELECT *,
       CASE WHEN is_duplicate THEN 'DUPLICATE' ELSE 'KEEP' END AS dedup_action
FROM marked
ORDER BY is_duplicate DESC, email, username, created_at DESC;