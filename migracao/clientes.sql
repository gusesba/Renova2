SELECT
    id AS "Id",
    LEFT(
        LPAD(
            REGEXP_REPLACE(COALESCE(phone, ''), '\D', '', 'g'),
            10,
            '0'
        ),
        11
    ) AS "Contato",
    name AS "Nome",
    1 AS "LojaId",
    NULL::int4 AS "UserId",
    false AS "Doacao",
    '-' AS "Obs"
FROM "Cliente";