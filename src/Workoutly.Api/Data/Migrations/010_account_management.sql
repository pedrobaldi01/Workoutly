-- ALTER TABLE no MySQL faz commit implícito. Cada etapa pode ser retomada separadamente.
SET @account_ddl = IF(EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema=DATABASE() AND table_name='exercises' AND column_name='archived'
), 'SELECT 1', 'ALTER TABLE exercises ADD COLUMN archived BOOLEAN NOT NULL DEFAULT FALSE');
PREPARE account_migration FROM @account_ddl;
EXECUTE account_migration;
DEALLOCATE PREPARE account_migration;

SET @account_ddl = IF(EXISTS (
    SELECT 1 FROM information_schema.referential_constraints
    WHERE constraint_schema=DATABASE() AND table_name='workout_plan_versions' AND constraint_name='fk_version_author'
) AND EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema=DATABASE() AND table_name='workout_plan_versions' AND column_name='author_id' AND is_nullable='NO'
), 'ALTER TABLE workout_plan_versions DROP FOREIGN KEY fk_version_author', 'SELECT 1');
PREPARE account_migration FROM @account_ddl;
EXECUTE account_migration;
DEALLOCATE PREPARE account_migration;

SET @account_ddl = IF(EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema=DATABASE() AND table_name='workout_plan_versions' AND column_name='author_id' AND is_nullable='NO'
), 'ALTER TABLE workout_plan_versions MODIFY COLUMN author_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL', 'SELECT 1');
PREPARE account_migration FROM @account_ddl;
EXECUTE account_migration;
DEALLOCATE PREPARE account_migration;

SET @account_ddl = IF(EXISTS (
    SELECT 1 FROM information_schema.referential_constraints
    WHERE constraint_schema=DATABASE() AND table_name='workout_plan_versions' AND constraint_name='fk_version_author'
), 'SELECT 1', 'ALTER TABLE workout_plan_versions ADD CONSTRAINT fk_version_author FOREIGN KEY (author_id) REFERENCES users(id)');
PREPARE account_migration FROM @account_ddl;
EXECUTE account_migration;
DEALLOCATE PREPARE account_migration;

INSERT IGNORE INTO schema_migrations (version,name) VALUES (10,'account_management');
