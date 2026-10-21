CREATE TABLE IF NOT EXISTS account_roles (
    user_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
    role VARCHAR(20) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    CONSTRAINT ck_account_role CHECK (role IN ('student', 'teacher')),
    CONSTRAINT fk_role_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Contas anteriores continuam como aluno. Reaplicar não rebaixa professores.
INSERT IGNORE INTO account_roles (user_id, role) SELECT id, 'student' FROM users;

CREATE TABLE IF NOT EXISTS student_teachers (
    student_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
    teacher_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    INDEX ix_student_teacher (teacher_id),
    CONSTRAINT ck_distinct_student_teacher CHECK (student_id <> teacher_id),
    CONSTRAINT fk_enrollment_student FOREIGN KEY (student_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_enrollment_teacher FOREIGN KEY (teacher_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS teacher_invitations (
    teacher_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
    code CHAR(12) CHARACTER SET ascii COLLATE ascii_bin NOT NULL UNIQUE,
    expires_at DATETIME(6) NOT NULL,
    CONSTRAINT fk_invitation_teacher FOREIGN KEY (teacher_id) REFERENCES users(id) ON DELETE CASCADE
);

INSERT IGNORE INTO schema_migrations (version, name) VALUES (4, 'roles_and_enrollment');
