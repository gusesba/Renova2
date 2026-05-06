SELECT
    ROW_NUMBER() OVER (ORDER BY MIN(TRIM(product))) AS "Id",
    MIN(TRIM(product)) AS "Valor",
    1 AS "LojaId"
FROM "Product"
WHERE product IS NOT NULL
  AND TRIM(product) <> ''
GROUP BY LOWER(TRIM(product))
ORDER BY "Id";