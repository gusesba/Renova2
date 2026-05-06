SELECT
    ROW_NUMBER() OVER (ORDER BY MIN(TRIM(brand))) AS "Id",
    MIN(TRIM(brand)) AS "Valor",
    1 AS "LojaId"
FROM "Product"
WHERE brand IS NOT NULL
  AND TRIM(brand) <> ''
GROUP BY LOWER(TRIM(brand))
ORDER BY "Id";