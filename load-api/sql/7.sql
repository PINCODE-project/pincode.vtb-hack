-- 7) Платежи и суммы по статусам/провайдерам + все итоги (GROUPING SETS)
SELECT
  COALESCE(provider, 'ALL')               AS provider,
  COALESCE(status::text, 'ALL')           AS status,
  COUNT(*)                                AS payments_cnt,
  SUM(amount)                             AS total_amount
FROM marketplace.payments
GROUP BY GROUPING SETS (
  (provider, status),  -- детально
  (provider),          -- по провайдеру
  (status),            -- по статусу
  ()                   -- общий итог
)
ORDER BY provider NULLS LAST, status NULLS LAST;