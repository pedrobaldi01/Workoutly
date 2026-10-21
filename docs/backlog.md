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

**AUTH-02 — parcial:** identidade, perfil e exercícios isolados por conta. Listagem e detalhes de exercícios foram testados com dois usuários, incluindo tentativa de consultar ID privado alheio. A autorização de fichas e treinos será implementada junto com esses recursos.

## Catálogo concluído

**EX-01 — concluído:** dez exercícios iniciais compartilhados, cadastro privado com validação, busca, filtro de exercícios próprios e detalhes no app. Selecionar um item abre seus detalhes; a inclusão em fichas depende de FICHA-01. Quinze testes HTTP passaram em MySQL real. Cadastro, validação, ausência de conexão e recuperação após reabrir foram verificados no iPhone Simulator. Evidências em docs/evidencias/exercicios.md.

## Próximos incrementos

**AUTH-03 — concluído:** dois perfis, aluno e professor/admin. Cadastro público limitado a aluno, professor criado por comando administrativo, papel fornecido pela API e validado no servidor. As contas anteriores permanecem como aluno.

**VINC-01 — concluído:** código de vínculo com validade de sete dias, renovação, confirmação pelo aluno, consulta dos alunos vinculados e remoção da relação por qualquer parte. Um professor ativo por aluno. Autorização testada com dois professores; evidências em docs/evidencias/perfis-e-vinculos.md. O vínculo ainda não expõe sessões nem permite montar fichas, recursos dos próximos incrementos.

**VISUAL-01 — concluído antes de FICHA-01:** identidade do Figma aplicada às telas existentes de acesso, cadastro, início, catálogo e formulário/detalhes de exercício. Tokens e Inter locais, marca e avatar originais; referências em docs/identidade-visual.md e verificação em docs/evidencias/identidade-visual.md. Não antecipa funções de fichas ou histórico.

| ID | Dependência | Requisito | Critério de aceite |
| --- | --- | --- | --- |
| AUTH-02 | AUTH-01 | Autorização dos próximos recursos | Parcial: perfil e exercícios isolados. Aplicar filtragem e testes de propriedade a fichas e treinos. |
| FICHA-01 | EX-01, VINC-01 | Criar, consultar, editar e excluir fichas | Fichas próprias do aluno e montagem pelo professor para alunos vinculados; exercícios, séries e repetições; confirmar exclusão e preservar o histórico. |
| TREINO-01 | FICHA-01 | Registrar sessão | Salvar carga, séries, repetições, duração e observações; não duplicar sessão em reenvio. |
| TREINO-02 | TREINO-01 | Corrigir registros próprios | Editar uma sessão, recalcular indicadores e manter dados de outros usuários intactos. |
| HIST-01 | TREINO-02, VINC-01 | Histórico e indicadores | Histórico, frequência, volume e evolução de carga a partir das sessões; professor acompanha somente alunos vinculados. |
| UX-01 | HIST-01 | Uso online compreensível | Carregamento, estados vazios e erros claros; falha de rede preserva o formulário para tentar novamente. |
| QA-01 | UX-01 | Validação multiplataforma | Fluxo completo no iOS e Android; registrar dispositivo, ambiente, versão, resultados e limites. |
| AVAL-01 | QA-01 | Avaliação com três adultos | Consentimento, P1/P2/P3, tarefas, tempo, erros, ajuda, notas de facilidade 1–5 e comentários reais. |
| DOC-01 | AVAL-01 | Documentação final | Resultados ligados às evidências, limitações explícitas, referências e declaração de uso de IA. |

Os demais incrementos estão **pendentes**, começando por FICHA-01; AUTH-02 permanece parcial. Critérios da avaliação: três participantes concluem o fluxo principal sem auxílio, média de facilidade ≥ 4/5 e nenhuma falha crítica ou perda de dados. Se os resultados divergirem, relatar a divergência; nunca preencher resultados fictícios.

## Fora do MVP

Sincronização offline, redes sociais, wearables, prescrição clínica, AI Coach, lembretes, metas e exportação. A apresentação será preparada depois. O Figma do grupo foi fornecido em 20/10/2026 e orienta a interface; backend externo do grupo não foi fornecido.
