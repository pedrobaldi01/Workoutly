CREATE TABLE IF NOT EXISTS workout_session_changes (
    session_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    version INT NOT NULL,
    operation_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL UNIQUE,
    request_hash CHAR(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    changed_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (session_id, version),
    CONSTRAINT fk_change_session FOREIGN KEY (session_id) REFERENCES workout_sessions(id),
    CONSTRAINT ck_change_version CHECK (version > 0)
);
-- A versão inicial de registros anteriores é preservada ao reaplicar.
INSERT IGNORE INTO workout_session_changes (session_id, version, operation_id, request_hash, changed_at)
SELECT id, 1, id, request_hash, created_at FROM workout_sessions;
INSERT IGNORE INTO schema_migrations (version) VALUES (7);
