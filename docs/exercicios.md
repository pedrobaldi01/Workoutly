# Catálogo e exercícios próprios

O catálogo identifica os movimentos que serão usados nas fichas. Cada exercício contém nome, grupo muscular, equipamento e descrição opcional. Séries, repetições e carga pertencem ao planejamento ou ao treino realizado, e por isso não são campos do catálogo.

## Como usar

1. Inicie o ambiente com `Executar-local.command` e abra o app no simulador.
2. Entre na conta e toque em **Explorar exercícios**.
3. Busque por nome, grupo ou equipamento. A busca ignora diferenças de maiúsculas e acentos e filtra os dados carregados; **Atualizar** consulta novamente a API.
4. Toque em um exercício para consultar os detalhes. O catálogo inicial tem dez movimentos; a indicação **Meu exercício** identifica registros da própria conta.
5. Use **Novo exercício**, preencha os campos e toque em **Salvar exercício**. A lista é recarregada após salvar; **Somente meus exercícios** permite conferir os registros privados.

Os valores do catálogo servem para identificar registros, sem gerar treinos ou prescrições. A seleção para compor uma ficha será implementada em FICHA-01. Edição e exclusão de exercícios não fazem parte deste incremento.

## Contrato HTTP

As três rotas exigem `Authorization: Bearer <token>` válido. O identificador do usuário vem da sessão validada, nunca de parâmetros enviados pelo app.

| Método e rota | Resultado |
| --- | --- |
| `GET /exercises` | 200: lista ordenada com catálogo inicial e exercícios próprios. |
| `GET /exercises/{id}` | 200: exercício visível para a conta; 404 para inexistente ou pertencente a outra conta. |
| `POST /exercises` | 201 com exercício criado e header Location; 400 para campos inválidos. Sempre cria um exercício privado. |

Sem sessão ou com sessão expirada, as rotas retornam 401. Falha de banco retorna 503 com mensagem genérica. Fora de Development, a API exige HTTPS, assim como o app em Release. Respostas dessas rotas usam `Cache-Control: no-store`.

Corpo de criação:

```json
{
  "name": "Remada unilateral",
  "muscleGroup": "Costas",
  "equipment": "Halteres",
  "description": "Variação identificada pelo usuário."
}
```

Nome: 2–120 caracteres; grupo: 1–60; equipamento: 1–80; descrição opcional: até 1000. Espaços nas extremidades são removidos e campos obrigatórios em branco são rejeitados. A resposta inclui `id` e `isShared`; não expõe o proprietário. Grupo e equipamento são textos livres para permitir variações próprias. Nomes repetidos são permitidos, pois podem identificar variações; não há reenvio automático do POST. O botão fica bloqueado durante a requisição.

## Conceito, código e decisão

- **Catálogo compartilhado e propriedade:** `Data/Migrations/003_exercises.sql` cria a tabela `exercises`. `owner_id` nulo identifica um exercício inicial; nos privados, a chave estrangeira referencia `users`. Seeds com IDs fixos evitam duplicação ao reaplicar a migração. As migrações 001 e 002 permanecem preservadas.
- **Autorização:** `Exercises/ExerciseEndpoints.cs` exige sessão; `ExerciseService.cs` filtra cada consulta por catálogo compartilhado ou proprietário autenticado. A consulta de detalhes usa o mesmo filtro da listagem. Um ID conhecido não concede acesso aos dados de outro usuário.
- **Interface e comunicação:** `ExercisesPage.xaml` apresenta a lista e a busca; `ExercisePage.xaml` exibe detalhes ou cadastro. Seus code-behinds controlam carregamento e navegação. `Services/AccountClient.cs` usa a sessão segura já existente para enviar HTTP e serialização gerada para compatibilidade com o app nativo.
- **Validação no servidor:** os limites são aplicados na API mesmo que o cliente seja outro. Uma falha de conexão mantém os campos preenchidos para nova tentativa; não há fila nem sincronização offline.

Os caminhos acima são relativos a `src/Workoutly.Api` e `src/Workoutly.Mobile`, conforme o componente. A separação permite testar acesso e persistência pela API sem depender do simulador.

## Verificar

Execute `python3 scripts/test-native-mysql.py`. Os testes HTTP de exercícios verificam catálogo autenticado, migração reaplicável, cadastro, limites dos campos, rejeição sem persistir dados inválidos, recuperação em outra instância da API e isolamento entre duas contas. O MySQL temporário é separado do banco usado para demonstração.

As observações de execução no aparelho e os limites ficam em [evidências deste incremento](evidencias/exercicios.md).
