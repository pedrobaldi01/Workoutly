-- Revisão anatômica: fontes e critérios em docs/catalogo-musculos.json.
-- Aplicar uma única vez; preservar descrições personalizadas em migrações futuras.
START TRANSACTION;
UPDATE exercises SET description = CASE id
    WHEN 'e1000000-0000-0000-0000-000000000001' THEN 'Músculos trabalhados: Principais: quadríceps e glúteo máximo. O adutor magno auxilia na extensão do quadril. Abdômen e eretores da coluna estabilizam o tronco; glúteo médio ajuda no controle do quadril.

Execução: Com a barra apoiada nas costas, flexione quadris e joelhos e retorne com controle.'
    WHEN 'e1000000-0000-0000-0000-000000000002' THEN 'Músculos trabalhados: Principais: quadríceps e glúteo máximo. Os adutores participam do movimento; a contribuição de cada grupo varia com a amplitude e o posicionamento. Não é um exercício de isolamento dos posteriores da coxa.

Execução: Mantenha costas e quadril apoiados e mova a plataforma sem perder o controle.'
    WHEN 'e1000000-0000-0000-0000-000000000003' THEN 'Músculos trabalhados: Principal: quadríceps femoral, formado por reto femoral, vasto lateral, vasto medial e vasto intermédio. Esses músculos estendem o joelho; os isquiotibiais não são o alvo principal.

Execução: Ajuste a máquina e estenda os joelhos de forma controlada.'
    WHEN 'e1000000-0000-0000-0000-000000000004' THEN 'Músculos trabalhados: Principal: peitoral maior. Tríceps braquial e deltoide anterior auxiliam no movimento. O manguito rotador contribui para a estabilidade do ombro.

Execução: No banco horizontal, desça e empurre a barra com as costas apoiadas.'
    WHEN 'e1000000-0000-0000-0000-000000000005' THEN 'Músculos trabalhados: Principais: latíssimo do dorso (grande dorsal), romboides e trapézio médio. Deltoide posterior, bíceps braquial e braquial auxiliam. Eretores da coluna e abdômen estabilizam o tronco inclinado.

Execução: Incline o tronco com a coluna estável e puxe a barra em direção ao abdômen.'
    WHEN 'e1000000-0000-0000-0000-000000000006' THEN 'Músculos trabalhados: Principal: latíssimo do dorso (grande dorsal). Redondo maior, bíceps braquial, braquial e braquiorradial auxiliam. Romboides e trapézio participam do controle das escápulas.

Execução: Traga a barra à frente, em direção à parte alta do peito, sem balançar o tronco.'
    WHEN 'e1000000-0000-0000-0000-000000000007' THEN 'Músculos trabalhados: Principais: deltoide anterior e porção média do deltoide. Tríceps braquial auxilia na extensão dos cotovelos. Trapézio e serrátil anterior participam da rotação das escápulas; o manguito rotador estabiliza o ombro.

Execução: Eleve os halteres a partir da altura dos ombros e retorne com controle.'
    WHEN 'e1000000-0000-0000-0000-000000000008' THEN 'Músculos trabalhados: Principais: bíceps braquial e braquial, que flexionam o cotovelo. O braquiorradial também participa; a musculatura do antebraço estabiliza a pegada.

Execução: Com a pegada supinada na barra, flexione os cotovelos sem usar impulso do tronco.'
    WHEN 'e1000000-0000-0000-0000-000000000009' THEN 'Músculos trabalhados: Principal: tríceps braquial, com participação das cabeças longa, lateral e medial na extensão do cotovelo. A pegada não transforma o exercício em isolamento de uma única cabeça.

Execução: Mantenha os braços próximos ao tronco e estenda os cotovelos contra a resistência da polia.'
    WHEN 'e1000000-0000-0000-0000-000000000010' THEN 'Músculos trabalhados: Principais: gastrocnêmios medial e lateral e sóleo, responsáveis pela flexão plantar do tornozelo. Esta descrição corresponde à execução em pé, com os joelhos estendidos.

Execução: Com apoio para equilíbrio, eleve e abaixe os calcanhares de forma controlada.'
    WHEN 'e1000000-0000-0000-0000-000000000011' THEN 'Músculos trabalhados: Principal: peitoral maior, na aproximação dos braços à frente do tórax. O deltoide anterior auxilia e o manguito rotador estabiliza o ombro. O tríceps não é o motor principal dessa variação.

Execução: Deitado no banco, abra e aproxime os braços mantendo leve flexão dos cotovelos.'
    WHEN 'e1000000-0000-0000-0000-000000000012' THEN 'Músculos trabalhados: Principal: porção média do deltoide. O supraespinal auxilia na abdução do braço. Trapézio e serrátil anterior participam da rotação das escápulas. Não é um movimento voltado principalmente ao deltoide posterior.

Execução: Eleve os braços para os lados com controle e sem impulso do tronco.'
    WHEN 'e1000000-0000-0000-0000-000000000013' THEN 'Músculos trabalhados: Principal: peitoral maior, com maior ênfase relativa na porção clavicular (parte superior do peito). Deltoide anterior e tríceps braquial auxiliam. A inclinação modifica a ênfase, sem isolar uma região do peito.

Execução: Com as costas no banco inclinado, desça e empurre a barra de forma controlada.'
    WHEN 'e1000000-0000-0000-0000-000000000014' THEN 'Músculos trabalhados: Principal: peitoral maior. Tríceps braquial e deltoide anterior auxiliam; o manguito rotador contribui para a estabilidade dos ombros. A descrição considera o banco horizontal.

Execução: Com as costas apoiadas no banco horizontal, desça e empurre os halteres com controle.'
    WHEN 'e1000000-0000-0000-0000-000000000015' THEN 'Músculos trabalhados: Principais: peitoral maior, tríceps braquial e deltoide anterior. O serrátil anterior participa do controle das escápulas; abdômen e glúteos ajudam a manter o alinhamento do corpo.

Execução: Flexione e estenda os braços mantendo o corpo alinhado.'
    WHEN 'e1000000-0000-0000-0000-000000000016' THEN 'Músculos trabalhados: Principais: latíssimo do dorso (grande dorsal), romboides e trapézio médio. Deltoide posterior, bíceps braquial e braquial auxiliam. A pegada e a trajetória dos cotovelos alteram a ênfase.

Execução: Sentado na polia baixa, puxe o cabo em direção ao tronco sem usar balanço.'
    WHEN 'e1000000-0000-0000-0000-000000000017' THEN 'Músculos trabalhados: Principais: latíssimo do dorso (grande dorsal), romboides e trapézio médio. Deltoide posterior e flexores do cotovelo auxiliam; o abdômen ajuda a resistir à rotação do tronco.

Execução: Com apoio no banco, puxe um halter de cada vez sem girar o tronco.'
    WHEN 'e1000000-0000-0000-0000-000000000018' THEN 'Músculos trabalhados: Principal: latíssimo do dorso (grande dorsal). Bíceps braquial, braquial e braquiorradial auxiliam na flexão dos cotovelos. Romboides e trapézio controlam as escápulas; o abdômen estabiliza o corpo.

Execução: Na barra, eleve e abaixe o corpo com controle, sem balanço.'
    WHEN 'e1000000-0000-0000-0000-000000000019' THEN 'Músculos trabalhados: Principais: glúteo máximo, quadríceps e isquiotibiais. Eretores da coluna e abdômen estabilizam o tronco; latíssimo do dorso, trapézio e músculos da pegada também participam. A descrição considera o terra convencional com barra.

Execução: Retire a barra do chão estendendo quadris e joelhos, mantendo-a próxima ao corpo.'
    WHEN 'e1000000-0000-0000-0000-000000000020' THEN 'Músculos trabalhados: Principais: glúteo máximo e isquiotibiais que cruzam o quadril: semitendíneo, semimembranáceo e cabeça longa do bíceps femoral. Eretores da coluna e abdômen estabilizam o tronco; o adutor magno auxilia na extensão do quadril.

Execução: Partindo em pé, leve o quadril para trás com leve flexão dos joelhos e retorne com controle.'
    WHEN 'e1000000-0000-0000-0000-000000000021' THEN 'Músculos trabalhados: Principais: isquiotibiais (bíceps femoral, semitendíneo e semimembranáceo), na flexão do joelho. Os gastrocnêmios também podem auxiliar; quadríceps e glúteos não são os alvos principais.

Execução: Deitado de barriga para baixo, flexione os joelhos mantendo o quadril apoiado.'
    WHEN 'e1000000-0000-0000-0000-000000000022' THEN 'Músculos trabalhados: Principais: isquiotibiais (bíceps femoral, semitendíneo e semimembranáceo), na flexão do joelho. Os gastrocnêmios podem auxiliar. A posição sentada mantém o quadril flexionado, diferenciando esta variação da mesa flexora.

Execução: Sentado com as costas apoiadas, flexione os joelhos contra a resistência da máquina.'
    WHEN 'e1000000-0000-0000-0000-000000000023' THEN 'Músculos trabalhados: Principais: quadríceps e glúteo máximo. Isquiotibiais e adutores participam; glúteo médio, panturrilhas e musculatura do tronco ajudam no equilíbrio. O comprimento do passo e a inclinação do tronco alteram a ênfase.

Execução: Avance alternando as pernas, flexionando e estendendo quadris e joelhos com controle.'
    WHEN 'e1000000-0000-0000-0000-000000000024' THEN 'Músculos trabalhados: Principais: quadríceps e glúteo máximo da perna da frente. Adutores e isquiotibiais auxiliam; glúteo médio e musculatura do tronco contribuem para o equilíbrio. A posição dos pés e do tronco modifica a ênfase.

Execução: Apoie o pé de trás no banco e flexione a perna da frente com controle.'
    WHEN 'e1000000-0000-0000-0000-000000000025' THEN 'Músculos trabalhados: Principal: glúteo máximo, na extensão do quadril. Isquiotibiais e adutor magno podem auxiliar; abdômen e eretores da coluna estabilizam o tronco. Não é uma extensão da coluna lombar.

Execução: Com a parte alta das costas apoiada no banco, eleve e abaixe o quadril com controle.'
    WHEN 'e1000000-0000-0000-0000-000000000026' THEN 'Músculos trabalhados: Principal ênfase: sóleo. Os gastrocnêmios também participam, mas a flexão dos joelhos reduz sua contribuição em comparação com a versão em pé. O sóleo não é isolado e também trabalha na versão em pé.

Execução: Sentado com os joelhos flexionados e apoiados, eleve e abaixe os calcanhares.'
    WHEN 'e1000000-0000-0000-0000-000000000027' THEN 'Músculos trabalhados: Músculos envolvidos: braquial, braquiorradial e bíceps braquial, na flexão do cotovelo com pegada neutra. Não é um exercício exclusivo do bíceps nem permite isolar um único flexor do cotovelo.

Execução: Flexione os cotovelos mantendo as palmas voltadas uma para a outra.'
    WHEN 'e1000000-0000-0000-0000-000000000028' THEN 'Músculos trabalhados: Principais: bíceps braquial e braquial. O braquiorradial também pode auxiliar. O apoio dos braços modifica a posição do ombro, mas não isola uma única cabeça do bíceps.

Execução: Com os braços apoiados no banco Scott e pegada supinada, flexione os cotovelos com controle.'
    WHEN 'e1000000-0000-0000-0000-000000000029' THEN 'Músculos trabalhados: Principal: tríceps braquial, incluindo as cabeças longa, lateral e medial, na extensão do cotovelo. O posicionamento do braço altera o comprimento da cabeça longa; não há isolamento completo de uma única cabeça.

Execução: Deitado no banco, flexione e estenda os cotovelos mantendo os braços estáveis.'
    WHEN 'e1000000-0000-0000-0000-000000000030' THEN 'Músculos trabalhados: Principal: deltoide anterior. A porção clavicular do peitoral maior auxilia na flexão do ombro. Trapézio e serrátil anterior participam do controle das escápulas. Difere da elevação lateral, que enfatiza a porção média do deltoide.

Execução: Eleve os halteres à frente com controle e sem balançar o tronco.'
    ELSE description END,
    muscle_group = CASE WHEN id='e1000000-0000-0000-0000-000000000027' THEN 'Braços e antebraços' ELSE muscle_group END
WHERE owner_id IS NULL AND id IN ('e1000000-0000-0000-0000-000000000001', 'e1000000-0000-0000-0000-000000000002', 'e1000000-0000-0000-0000-000000000003', 'e1000000-0000-0000-0000-000000000004', 'e1000000-0000-0000-0000-000000000005', 'e1000000-0000-0000-0000-000000000006', 'e1000000-0000-0000-0000-000000000007', 'e1000000-0000-0000-0000-000000000008', 'e1000000-0000-0000-0000-000000000009', 'e1000000-0000-0000-0000-000000000010', 'e1000000-0000-0000-0000-000000000011', 'e1000000-0000-0000-0000-000000000012', 'e1000000-0000-0000-0000-000000000013', 'e1000000-0000-0000-0000-000000000014', 'e1000000-0000-0000-0000-000000000015', 'e1000000-0000-0000-0000-000000000016', 'e1000000-0000-0000-0000-000000000017', 'e1000000-0000-0000-0000-000000000018', 'e1000000-0000-0000-0000-000000000019', 'e1000000-0000-0000-0000-000000000020', 'e1000000-0000-0000-0000-000000000021', 'e1000000-0000-0000-0000-000000000022', 'e1000000-0000-0000-0000-000000000023', 'e1000000-0000-0000-0000-000000000024', 'e1000000-0000-0000-0000-000000000025', 'e1000000-0000-0000-0000-000000000026', 'e1000000-0000-0000-0000-000000000027', 'e1000000-0000-0000-0000-000000000028', 'e1000000-0000-0000-0000-000000000029', 'e1000000-0000-0000-0000-000000000030')
  AND NOT EXISTS (SELECT 1 FROM schema_migrations WHERE version=9);
INSERT IGNORE INTO schema_migrations (version,name) VALUES (9,'exercise_muscle_descriptions');
COMMIT;
