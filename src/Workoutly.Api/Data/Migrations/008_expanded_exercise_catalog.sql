-- Amplia o catálogo compartilhado sem alterar contas, exercícios privados ou fichas.
INSERT IGNORE INTO exercises (id, name, muscle_group, equipment, description) VALUES
('e1000000-0000-0000-0000-000000000011', 'Crucifixo com halteres', 'Peito', 'Halteres', 'Com as costas apoiadas no banco, abra os braços com leve flexão nos cotovelos e retorne com controle.'),
('e1000000-0000-0000-0000-000000000012', 'Elevação lateral', 'Ombros', 'Halteres', 'Eleve os braços lateralmente com controle, mantendo o tronco estável.'),
('e1000000-0000-0000-0000-000000000013', 'Supino inclinado', 'Peito', 'Barra', 'Apoie as costas no banco inclinado e mova a barra com controle.'),
('e1000000-0000-0000-0000-000000000014', 'Supino com halteres', 'Peito', 'Halteres', 'Com as costas apoiadas, empurre os halteres e retorne de forma controlada.'),
('e1000000-0000-0000-0000-000000000015', 'Flexão de braços', 'Peito', 'Peso corporal', 'Mantenha o corpo alinhado enquanto flexiona e estende os braços.'),
('e1000000-0000-0000-0000-000000000016', 'Remada baixa', 'Costas', 'Polia', 'Sente-se com o tronco firme e puxe o cabo em direção ao abdômen.'),
('e1000000-0000-0000-0000-000000000017', 'Remada unilateral', 'Costas', 'Halteres', 'Apoie o corpo no banco e puxe o halter mantendo a coluna estável.'),
('e1000000-0000-0000-0000-000000000018', 'Barra fixa', 'Costas', 'Barra fixa', 'Puxe o corpo com controle e evite balançar o tronco.'),
('e1000000-0000-0000-0000-000000000019', 'Levantamento terra', 'Pernas e costas', 'Barra', 'Mantenha a barra próxima ao corpo e coordene a extensão de quadris e joelhos.'),
('e1000000-0000-0000-0000-000000000020', 'Levantamento terra romeno', 'Posteriores de coxa', 'Barra', 'Leve o quadril para trás com leve flexão dos joelhos e mantenha a coluna estável.'),
('e1000000-0000-0000-0000-000000000021', 'Mesa flexora', 'Posteriores de coxa', 'Máquina', 'Ajuste o apoio da máquina e flexione os joelhos sem perder a posição do quadril.'),
('e1000000-0000-0000-0000-000000000022', 'Cadeira flexora', 'Posteriores de coxa', 'Máquina', 'Mantenha as costas apoiadas e flexione os joelhos de forma controlada.'),
('e1000000-0000-0000-0000-000000000023', 'Afundo com deslocamento', 'Pernas', 'Halteres', 'Dê um passo à frente, flexione os joelhos com estabilidade e avance alternando as pernas.'),
('e1000000-0000-0000-0000-000000000024', 'Agachamento búlgaro', 'Pernas', 'Banco e halteres', 'Apoie o pé de trás no banco e desça com controle mantendo o equilíbrio.'),
('e1000000-0000-0000-0000-000000000025', 'Elevação pélvica', 'Glúteos', 'Barra e banco', 'Apoie a parte alta das costas no banco e eleve o quadril com controle.'),
('e1000000-0000-0000-0000-000000000026', 'Panturrilha sentado', 'Panturrilhas', 'Máquina', 'Com os joelhos apoiados, eleve e abaixe os calcanhares de forma controlada.'),
('e1000000-0000-0000-0000-000000000027', 'Rosca martelo', 'Bíceps', 'Halteres', 'Flexione os cotovelos mantendo as palmas voltadas uma para a outra.'),
('e1000000-0000-0000-0000-000000000028', 'Rosca Scott', 'Bíceps', 'Banco Scott e barra', 'Mantenha os braços apoiados no banco e flexione os cotovelos com controle.'),
('e1000000-0000-0000-0000-000000000029', 'Tríceps testa', 'Tríceps', 'Barra e banco', 'Com as costas apoiadas, flexione e estenda os cotovelos mantendo os braços estáveis.'),
('e1000000-0000-0000-0000-000000000030', 'Elevação frontal', 'Ombros', 'Halteres', 'Eleve os braços à frente com controle e sem balançar o tronco.');

INSERT IGNORE INTO schema_migrations (version, name) VALUES (8, 'expanded_exercise_catalog');
