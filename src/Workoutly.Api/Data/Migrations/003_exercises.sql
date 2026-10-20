CREATE TABLE IF NOT EXISTS exercises (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
    owner_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    name VARCHAR(120) NOT NULL,
    muscle_group VARCHAR(60) NOT NULL,
    equipment VARCHAR(80) NOT NULL,
    description VARCHAR(1000) NOT NULL DEFAULT '',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    INDEX ix_exercises_owner_name (owner_id, name),
    CONSTRAINT fk_exercises_owner FOREIGN KEY (owner_id) REFERENCES users(id) ON DELETE CASCADE
) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;

-- IDs estáveis: reaplicar a migração não duplica nem sobrescreve registros.
INSERT IGNORE INTO exercises (id, name, muscle_group, equipment) VALUES
('e1000000-0000-0000-0000-000000000001', 'Agachamento livre', 'Pernas', 'Barra'),
('e1000000-0000-0000-0000-000000000002', 'Leg press', 'Pernas', 'Máquina'),
('e1000000-0000-0000-0000-000000000003', 'Cadeira extensora', 'Pernas', 'Máquina'),
('e1000000-0000-0000-0000-000000000004', 'Supino reto', 'Peito', 'Barra'),
('e1000000-0000-0000-0000-000000000005', 'Remada curvada', 'Costas', 'Barra'),
('e1000000-0000-0000-0000-000000000006', 'Puxada frontal', 'Costas', 'Polia'),
('e1000000-0000-0000-0000-000000000007', 'Desenvolvimento com halteres', 'Ombros', 'Halteres'),
('e1000000-0000-0000-0000-000000000008', 'Rosca direta', 'Bíceps', 'Barra'),
('e1000000-0000-0000-0000-000000000009', 'Tríceps na polia', 'Tríceps', 'Polia'),
('e1000000-0000-0000-0000-000000000010', 'Elevação de panturrilhas', 'Panturrilhas', 'Peso corporal');

INSERT IGNORE INTO schema_migrations (version, name) VALUES (3, 'exercises');
