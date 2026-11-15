# Gerenciamento de conta

## Implementação

- No app MAUI, toque no avatar verde no canto superior direito para abrir **Gerenciar conta** com perfil de aluno ou professor.
- As operações HTTP são `PATCH /auth/me`, `POST /auth/me/password` e `POST /auth/me/delete`. A sessão identifica a conta. Alterar e-mail, alterar senha ou excluir a conta exige a senha atual.
- A troca de senha revoga todas as sessões de acesso. A exclusão do aluno remove suas fichas e registros de treino. A exclusão do professor preserva a ficha e o histórico do aluno, remove a autoria e arquiva exercícios privados com referências em fichas.
- A versão do esquema passa de 9 para 10. Aplique a migração explicitamente antes de usar as novas operações. No MySQL, use `python3 scripts/dev.py migrate` com o ambiente local configurado. Na API PostgreSQL, aplique o patch no repositório da API e execute `dotnet Workoutly.Api.dll --migrate` no ambiente do servidor com a conexão configurada. Coordene a migração com o deploy porque a prontidão da versão anterior espera o esquema 9. Confirme esquema 10 em `GET /health/ready` após a atualização.

## Verificação

- macOS Apple Silicon, SDK .NET 10.0.401. A API local e o projeto de testes compilaram. A compilação C# e a geração XAML do app iOS para `iossimulator-arm64` passaram.
- O empacotamento iOS foi bloqueado no host de tarefas do linker do MSBuild. A execução no simulador e no iPhone não foi verificada.
- O runner VSTest foi bloqueado ao abrir seu socket e a instância temporária de MySQL encerrou durante a inicialização. Os testes de integração foram preparados para ambiente isolado e sua execução está pendente.
- O patch PostgreSQL passou em `git apply --check --whitespace=error-all` contra os arquivos da branch `main` de `JoaoAza/Workoutly-API`. A migração e o deploy do Render estão pendentes.

## Meu perfil do aluno

- O cartão **Meu perfil** aparece no topo de **Gerenciar conta** para alunos. Mostra nome, e-mail, peso e altura. Peso e altura são opcionais e preenchidos após o cadastro.
- O aluno consulta e edita seu perfil em `GET /auth/me/student-profile` e `PUT /auth/me/student-profile`. O PostgreSQL recebe os campos na migração 12. Contas anteriores mantêm o nome já cadastrado.
- A API, o projeto de testes e C#/XAML do app para o simulador iOS compilaram sem erros em macOS Apple Silicon. O teste com PostgreSQL foi bloqueado ao abrir o socket local do VSTest. A execução no aparelho e a aplicação da migração no Render estão pendentes.
