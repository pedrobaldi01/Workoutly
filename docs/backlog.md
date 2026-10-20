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

## Próximos incrementos

| ID | Dependência | Requisito | Critério de aceite |
| --- | --- | --- | --- |
| AUTH-01 | DB-01 | Cadastro, login e logout | Credenciais válidas iniciam sessão, inválidas não; senha protegida e sessão no armazenamento seguro do aparelho. |
| AUTH-02 | AUTH-01 | Autorização | Dois usuários não acessam nem alteram dados privados um do outro. |
| EX-01 | AUTH-02 | Catálogo e exercícios próprios | Selecionar exercício inicial ou cadastrar exercício próprio; validar nome e campos. |
| FICHA-01 | EX-01 | Criar, consultar, editar e excluir fichas | Fichas com exercícios, séries e repetições; confirmar exclusão e respeitar o histórico já gravado. |
| TREINO-01 | FICHA-01 | Registrar sessão | Salvar carga, séries, repetições, duração e observações; não duplicar sessão em reenvio. |
| TREINO-02 | TREINO-01 | Corrigir registros próprios | Editar uma sessão, recalcular indicadores e manter dados de outros usuários intactos. |
| HIST-01 | TREINO-02 | Histórico e indicadores | Histórico consistente, frequência, volume e evolução de carga por exercício calculados a partir das sessões. |
| UX-01 | HIST-01 | Uso online compreensível | Carregamento, estados vazios e erros claros; falha de rede preserva o formulário para tentar novamente. |
| QA-01 | UX-01 | Validação multiplataforma | Fluxo completo no iOS e Android; registrar dispositivo, ambiente, versão, resultados e limites. |
| AVAL-01 | QA-01 | Avaliação com três adultos | Consentimento, P1/P2/P3, tarefas, tempo, erros, ajuda, notas de facilidade 1–5 e comentários reais. |
| DOC-01 | AVAL-01 | Documentação final | Resultados ligados às evidências, limitações explícitas, referências e declaração de uso de IA. |

Todos os próximos incrementos estão **pendentes**. Critérios da avaliação: três participantes concluem o fluxo principal sem auxílio, média de facilidade ≥ 4/5 e nenhuma falha crítica ou perda de dados. Se os resultados divergirem, relatar a divergência; nunca preencher resultados fictícios.

## Fora do MVP

Sincronização offline, redes sociais, wearables, prescrição clínica, AI Coach, lembretes, metas e exportação. A apresentação será preparada depois. Figma e backend externo do grupo não foram fornecidos e não bloqueiam esta fundação.
