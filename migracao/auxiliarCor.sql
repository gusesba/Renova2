SELECT
    ROW_NUMBER() OVER (ORDER BY MIN(TRIM(color))) AS "Id",
    MIN(TRIM(color)) AS "Valor",
    1 AS "LojaId"
FROM "Product"
WHERE color IS NOT NULL
  AND TRIM(color) <> ''
GROUP BY LOWER(TRIM(color))
ORDER BY "Id";