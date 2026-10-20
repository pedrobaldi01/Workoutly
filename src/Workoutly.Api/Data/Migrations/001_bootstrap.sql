-- Fundação do banco. Tabelas do domínio entram nos próximos incrementos.
-- DDL no MySQL faz commit implícito: este script pode ser reaplicado após interrupção.
CREATE TABLE IF NOT EXISTS schema_migrations (
    version INT NOT NULL PRIMARY KEY,
    name VARCHAR(120) NOT NULL,
    applied_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT IGNORE INTO schema_migrations (version, name) VALUES (1, 'bootstrap');
