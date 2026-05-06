WITH produtos_referencia AS (
    SELECT
        ROW_NUMBER() OVER (ORDER BY MIN(TRIM(product))) AS id,
        LOWER(TRIM(product)) AS valor_normalizado
    FROM "Product"
    WHERE product IS NOT NULL
      AND TRIM(product) <> ''
    GROUP BY LOWER(TRIM(product))
),
marcas AS (
    SELECT
        ROW_NUMBER() OVER (ORDER BY MIN(TRIM(brand))) AS id,
        LOWER(TRIM(brand)) AS valor_normalizado
    FROM "Product"
    WHERE brand IS NOT NULL
      AND TRIM(brand) <> ''
    GROUP BY LOWER(TRIM(brand))
),
tamanhos AS (
    SELECT
        ROW_NUMBER() OVER (ORDER BY MIN(TRIM(size))) AS id,
        LOWER(TRIM(size)) AS valor_normalizado
    FROM "Product"
    WHERE size IS NOT NULL
      AND TRIM(size) <> ''
    GROUP BY LOWER(TRIM(size))
),
cores AS (
    SELECT
        ROW_NUMBER() OVER (ORDER BY MIN(TRIM(color))) AS id,
        LOWER(TRIM(color)) AS valor_normalizado
    FROM "Product"
    WHERE color IS NOT NULL
      AND TRIM(color) <> ''
    GROUP BY LOWER(TRIM(color))
)
SELECT
    p.id AS "Id",
    ROUND(p.price, 2) AS "Preco",
    pr.id AS "ProdutoId",
    m.id AS "MarcaId",
    t.id AS "TamanhoId",
    c.id AS "CorId",
    p."providerId" AS "FornecedorId",
    LEFT(COALESCE(p.description, '-'), 1000) AS "Descricao",
    p.entry AS "Entrada",
    1 AS "LojaId",
    1 AS "Situacao",
    CASE
        WHEN p."providerId" = 1 THEN false
        ELSE true
    END AS "Consignado",
    p.id AS "Etiqueta"
FROM "Product" p
LEFT JOIN produtos_referencia pr
    ON LOWER(TRIM(p.product)) = pr.valor_normalizado
LEFT JOIN marcas m
    ON LOWER(TRIM(p.brand)) = m.valor_normalizado
LEFT JOIN tamanhos t
    ON LOWER(TRIM(p.size)) = t.valor_normalizado
LEFT JOIN cores c
    ON LOWER(TRIM(p.color)) = c.valor_normalizado
WHERE p."sellId" IS NULL
ORDER BY p.id;