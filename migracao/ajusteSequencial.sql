SELECT setval(
    pg_get_serial_sequence('"Cliente"', 'Id'),
    COALESCE(MAX("Id"), 1)
)
FROM "Cliente";

SELECT setval(
    pg_get_serial_sequence('"ProdutoReferencia"', 'Id'),
    COALESCE(MAX("Id"), 1)
)
FROM "ProdutoReferencia";

SELECT setval(
    pg_get_serial_sequence('"Marca"', 'Id'),
    COALESCE(MAX("Id"), 1)
)
FROM "Marca";

SELECT setval(
    pg_get_serial_sequence('"Tamanho"', 'Id'),
    COALESCE(MAX("Id"), 1)
)
FROM "Tamanho";

SELECT setval(
    pg_get_serial_sequence('"Cor"', 'Id'),
    COALESCE(MAX("Id"), 1)
)
FROM "Cor";

SELECT setval(
    pg_get_serial_sequence('"ProdutoEstoque"', 'Id'),
    COALESCE(MAX("Id"), 1)
)
FROM "ProdutoEstoque";