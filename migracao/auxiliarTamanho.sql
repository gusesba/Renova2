SELECT
    ROW_NUMBER() OVER (ORDER BY MIN(TRIM(size))) AS "Id",
    MIN(TRIM(size)) AS "Valor",
    1 AS "LojaId"
FROM "Product"
WHERE size IS NOT NULL
  AND TRIM(size) <> ''
GROUP BY LOWER(TRIM(size))
ORDER BY "Id";