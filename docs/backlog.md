# Backlog do Workoutly

Fonte: Roteiro Workoutly e Roteiro Fase 2 na pasta superior. A Fase 2 descreve uma entrega histórica; não comprova implementação ou testes desta nova versão.

## Etapa contratada agora

| ID | Incremento | Aceite | Estado |
| --- | --- | --- | --- |
| BASE-01 | Repositório e instruções | Git local, fontes originais intactas, README e AGENTS. | Concluído — evidências em docs/evidencias/fundacao.md |
| BASE-02 | Solução nova | Projetos MAUI, API e testes incluídos na solução, sem código antigo. | Concluído — evidências em docs/evidencias/fundacao.md |
| BASE-03 | Contrato de saúde | GET /health retorna HTTP 200 e explicita ausência de banco; teste HTTP passa. | Concluído — evidências em docs/evidencias/fundacao.md |
| BASE-04 | Ferramentas de apoio | Skills locais, recursos, licenças e revisões registradas. | Concluído — evidências em docs/evidencias/fundacao.md |

## Incremento MySQL concluído

**DB-01 — concluído:** conexão configurada somente na API, bootstrap reaplicável do histórico de migrações, rota de prontidão e teste em MySQL 9.6.0 real. Evidências em docs/evidencias/mysql.md. O ambiente Docker está preparado, mas não executado neste Mac por configuração inicial pendente do Docker Desktop. A validação real usou instância nativa isolada e temporária.

## Conta e acesso concluídos

**AUTH-01 — concluído:** cadastro, login, perfil, restauração de sessão no SecureStorage e logout com revogação MySQL. Doze testes passaram em MySQL real e o fluxo foi exercitado no iPhone Simulator. Evidências em docs/evidencias/autenticacao.md.

**AUTH-02 — concluído:** perfil, exercícios, fichas, sessões e indicadores isolados por conta. Professor consulta somente alunos com vínculo ativo, inclusive detalhes das sessões, sem permissão de correção. Remover o vínculo bloqueia novas consultas e preserva os dados do aluno. Testes com dois professores e duas contas de aluno; evidências em docs/evidencias/historico.md.

## Catálogo concluído

**EX-01 — concluído:** dez exercícios iniciais compartilhados, cadastro privado com validação, busca, filtro de exercícios próprios e detalhes no app. Selecionar um item abre seus detalhes; a inclusão em fichas depende de FICHA-01. Quinze testes HTTP passaram em MySQL real. Cadastro, validação, ausência de conexão e recuperação após reabrir foram verificados no iPhone Simulator. Evidências em docs/evidencias/exercicios.md.

## Próximos incrementos

**AUTH-03 — concluído:** dois perfis, aluno e professor/admin. Cadastro público limitado a aluno, professor criado por comando administrativo, papel fornecido pela API e validado no servidor. As contas anteriores permanecem como aluno.

**VINC-01 — concluído:** código de vínculo com validade de sete dias, renovação, confirmação pelo aluno, consulta dos alunos vinculados e remoção da relação por qualquer parte. Um professor ativo por aluno. Autorização testada com dois professores; evidências em docs/evidencias/perfis-e-vinculos.md. O vínculo autoriza fichas e acompanhamento somente de leitura das sessões e indicadores em HIST-01.

**VISUAL-01 — concluído antes de FICHA-01:** identidade do Figma aplicada às telas existentes de acesso, cadastro, início, catálogo e formulário/detalhes de exercício. Tokens e Inter locais, marca e avatar originais; referências em docs/identidade-visual.md e verificação em docs/evidencias/identidade-visual.md. Não antecipa funções de fichas ou histórico.

| ID | Dependência | Requisito | Critério de aceite |
| --- | --- | --- | --- |
| AUTH-02 | AUTH-01 | Concluído: autorização dos recursos | Perfil, exercícios, fichas, sessões e indicadores isolados; professor restrito ao vínculo ativo. Evidências em docs/evidencias/historico.md. |
| FICHA-01 | EX-01, VINC-01 | Concluído: criar, consultar, editar e excluir fichas | Fichas próprias e de alunos vinculados; ordem, séries, repetições, revisões imutáveis e exclusão lógica. Evidências em docs/evidencias/fichas.md; sessões integradas em TREINO-01; indicadores integrados em HIST-01. |
| TREINO-01 | FICHA-01 | Concluído: registrar sessão | Séries realizadas, cargas decimais, repetições, duração, observações e consulta própria; revisão original preservada e reenvio sem duplicação. Evidências em docs/evidencias/treinos.md. |
| TREINO-02 | TREINO-01 | Concluído: corrigir registros próprios | Edição de séries, repetições, cargas, duração e observações; volume/contagens recalculados; versões e reenvio seguro, sem alterar contas alheias. Evidências em docs/evidencias/correcao-treinos.md. |
| HIST-01 | TREINO-02, VINC-01 | Concluído: histórico e indicadores | Filtro por datas, frequência, dias ativos, séries, repetições, duração, volume e evolução de carga; professor consulta somente alunos vinculados. Evidências em docs/evidencias/historico.md. |
| UX-01 | HIST-01 | Uso online compreensível | Carregamento, estados vazios e erros claros; falha de rede preserva o formulário para tentar novamente. |
| QA-01 | UX-01 | Validação multiplataforma | Fluxo completo no iOS e Android; registrar dispositivo, ambiente, versão, resultados e limites. |
| AVAL-01 | QA-01 | Avaliação com três adultos | Consentimento, P1/P2/P3, tarefas, tempo, erros, ajuda, notas de facilidade 1–5 e comentários reais. |
| DOC-01 | AVAL-01 | Documentação final | Resultados ligados às evidências, limitações explícitas, referências e declaração de uso de IA. |

Os demais incrementos estão **pendentes**, começando por UX-01. AUTH-02 e HIST-01 estão concluídos. Critérios da avaliação: três participantes concluem o fluxo principal sem auxílio, média de facilidade ≥ 4/5 e nenhuma falha crítica ou perda de dados. Se os resultados divergirem, relatar a divergência; nunca preencher resultados fictícios.

## Fora do MVP

Sincronização offline, redes sociais, wearables, prescrição clínica, AI Coach, lembretes, metas e exportação. A apresentação será preparada depois. O Figma do grupo foi fornecido em 20/10/2026 e orienta a interface; backend externo do grupo não foi fornecido.
