-- 6) Дерево категорий с уровнем и «хлебными крошками» (RECURSIVE CTE)
WITH RECURSIVE cat AS (
  SELECT id, parent_id, name, 1 AS depth, ARRAY[name] AS path
  FROM marketplace.categories
  WHERE parent_id IS NULL
  UNION ALL
  SELECT c.id, c.parent_id, c.name, cat.depth + 1, cat.path || c.name
  FROM marketplace.categories c
  JOIN cat ON c.parent_id = cat.id
)
SELECT
  id,
  parent_id,
  depth,
  array_to_string(path, ' > ') AS breadcrumb
FROM cat
ORDER BY path;