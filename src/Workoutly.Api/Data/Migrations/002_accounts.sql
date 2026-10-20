CREATE TABLE IF NOT EXISTS users (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
    name VARCHAR(120) NOT NULL,
    email VARCHAR(254) NOT NULL,
    normalized_email VARCHAR(254) COLLATE utf8mb4_bin NOT NULL UNIQUE,
    password_hash VARCHAR(512) NOT NULL,
    created_at DATETIME(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS user_sessions (
    token_hash CHAR(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
    user_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    created_at DATETIME(6) NOT NULL,
    expires_at DATETIME(6) NOT NULL,
    INDEX ix_sessions_user (user_id),
    INDEX ix_sessions_expiry (expires_at),
    CONSTRAINT fk_sessions_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT IGNORE INTO schema_migrations (version, name) VALUES (2, 'accounts');
