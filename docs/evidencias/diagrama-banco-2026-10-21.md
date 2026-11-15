# Diagrama do banco — 21/10/2026

Criado [diagrama visual do Workoutly](../diagramas/workoutly-banco.html), com três visões ER: contas e vínculos; catálogo e fichas; treinos e séries. A divisão mantém as cardinalidades e as chaves legíveis. As tabelas repetidas são as mesmas entidades em contextos diferentes.

Fonte primária: consultas somente de leitura a `information_schema.columns`, `key_column_usage`, `referential_constraints`, `statistics` e `check_constraints` do MySQL local. Esquema versão 9. Conferência com migrações 001–009; as migrações 008 e 009 atualizam conteúdo, sem criar novas tabelas.

Cobertura verificada: **13 tabelas, 69 colunas, 16 FKs e 11 restrições CHECK**. Os metadados completos estão em [esquema-workoutly.json](../diagramas/esquema-workoutly.json), sem dados cadastrais, tokens ou valores de hashes. O [modelo técnico](../diagramas/modelo-banco-workoutly.md) inclui Mermaid editável, tipos SQL, índices e detalhes das chaves compostas.

Validação: todas as tabelas do metadado estão representadas; todas as FKs aparecem em ao menos uma visão. Uma relação de revisão/itens se repete em duas áreas e é contada apenas uma vez. Passou o `self_check.py --offline` da skill diagram-design, incluindo acessibilidade SVG e verificação de arquivo independente. Renderização das três visões inspecionada visualmente em Edge headless isolado. Em largura de 390 px não há transbordamento horizontal da página; cada diagrama possui sua própria área rolável. `git diff --check` passou.

As cardinalidades refletem as restrições do banco. A obrigatoriedade de papel no cadastro, os papéis válidos em vínculos, a versão atual da ficha e as permissões por aluno também dependem da API e estão anotadas no documento. Não foram inventadas tabelas para indicadores, imagens ou músculos individuais. O diagrama não modifica o esquema nem os dados do aplicativo.

Perfil visual adotado: claro com detalhes verdes, Arial/Georgia e fontes técnicas do sistema, sem dependência de fontes remotas. Formato HTML com SVG embutido; três desenhos em tamanho ajustado ao conteúdo, preservados em rolagem local em telas estreitas. Não foi gravado perfil global de estilo.
