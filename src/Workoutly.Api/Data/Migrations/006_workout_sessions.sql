CREATE TABLE IF NOT EXISTS workout_sessions (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
    student_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    plan_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    plan_version INT NOT NULL,
    started_at DATETIME(6) NOT NULL,
    duration_seconds INT NOT NULL,
    notes VARCHAR(1000) NOT NULL,
    request_hash CHAR(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    INDEX ix_sessions_student_started (student_id, started_at, id),
    UNIQUE KEY uq_session_revision (id, plan_id, plan_version),
    CONSTRAINT fk_session_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT fk_session_plan_version FOREIGN KEY (plan_id, plan_version) REFERENCES workout_plan_versions(plan_id, version),
    CONSTRAINT ck_session_duration CHECK (duration_seconds BETWEEN 1 AND 86400)
);
CREATE TABLE IF NOT EXISTS workout_session_sets (
    session_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    plan_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    plan_version INT NOT NULL,
    exercise_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    set_position INT NOT NULL,
    repetitions INT NOT NULL,
    weight_kg DECIMAL(6,2) NOT NULL,
    PRIMARY KEY (session_id, exercise_id, set_position),
    CONSTRAINT fk_set_session_revision FOREIGN KEY (session_id, plan_id, plan_version) REFERENCES workout_sessions(id, plan_id, plan_version),
    CONSTRAINT fk_set_plan_exercise FOREIGN KEY (plan_id, plan_version, exercise_id) REFERENCES workout_plan_items(plan_id, version, exercise_id),
    CONSTRAINT ck_session_set_position CHECK (set_position BETWEEN 0 AND 19),
    CONSTRAINT ck_session_repetitions CHECK (repetitions BETWEEN 1 AND 200),
    CONSTRAINT ck_session_weight CHECK (weight_kg BETWEEN 0 AND 2000)
);
INSERT IGNORE INTO schema_migrations (version) VALUES (6);
