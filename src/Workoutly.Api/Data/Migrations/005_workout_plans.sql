CREATE TABLE IF NOT EXISTS workout_plans (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
    student_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    current_version INT NOT NULL,
    deleted_at DATETIME(6) NULL,
    INDEX ix_plans_student (student_id, deleted_at),
    CONSTRAINT fk_plan_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT ck_plan_version CHECK (current_version > 0)
);

CREATE TABLE IF NOT EXISTS workout_plan_versions (
    plan_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    version INT NOT NULL,
    name VARCHAR(120) NOT NULL,
    notes VARCHAR(1000) NOT NULL,
    author_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (plan_id, version),
    CONSTRAINT fk_version_plan FOREIGN KEY (plan_id) REFERENCES workout_plans(id),
    CONSTRAINT fk_version_author FOREIGN KEY (author_id) REFERENCES users(id)
);

-- Cópia dos dados do exercício: a ficha não depende de mudanças futuras do catálogo.
CREATE TABLE IF NOT EXISTS workout_plan_items (
    plan_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    version INT NOT NULL,
    position INT NOT NULL,
    exercise_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    name VARCHAR(120) NOT NULL,
    muscle_group VARCHAR(60) NOT NULL,
    equipment VARCHAR(80) NOT NULL,
    description VARCHAR(1000) NOT NULL,
    set_count INT NOT NULL,
    repetitions INT NOT NULL,
    PRIMARY KEY (plan_id, version, position),
    UNIQUE KEY uq_plan_exercise (plan_id, version, exercise_id),
    CONSTRAINT fk_item_version FOREIGN KEY (plan_id, version) REFERENCES workout_plan_versions(plan_id, version),
    CONSTRAINT fk_item_exercise FOREIGN KEY (exercise_id) REFERENCES exercises(id),
    CONSTRAINT ck_plan_sets CHECK (set_count BETWEEN 1 AND 20),
    CONSTRAINT ck_plan_reps CHECK (repetitions BETWEEN 1 AND 200),
    CONSTRAINT ck_plan_position CHECK (position BETWEEN 0 AND 49)
);
INSERT IGNORE INTO schema_migrations (version) VALUES (5);
