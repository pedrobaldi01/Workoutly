# Perfis e vínculo de acompanhamento

Decisão do usuário: dois perfis, **aluno** e **professor/admin**. O professor poderá montar fichas e acompanhar treinos dos alunos vinculados. A base de acessos e vínculo está concluída. Fichas também foram implementadas no incremento seguinte, descrito em fichas.md; sessões próprias foram implementadas em TREINO-01 (treinos.md); acompanhamento pelo professor continua pendente.

## Como usar

O login é único, por e-mail e senha. Depois de entrar, a tela informa Aluno ou Professor / admin e oferece Meu professor ou Meus alunos. A pessoa não escolhe permissões na tela de acesso. O cadastro público cria somente alunos, inclusive se um cliente HTTP enviar campos extras como role ou isAdmin.

Para criar uma conta de professor no Mac, deixe o ambiente local ativo e execute na raiz:

```sh
python3 scripts/dev.py create-teacher --native
```

Se o ambiente já estava aberto antes desta atualização, reinicie Executar-local.command uma vez para aplicar a migração e preparar os metadados nativos. O comando pede nome, e-mail e senha com confirmação; a senha não aparece na tela nem é passada como argumento do processo. E-mail já existente é recusado, sem trocar a senha ou promover a conta anterior. Para Docker/servidor com ConnectionStrings__Workoutly configurada, use o mesmo comando sem --native. Esse caminho Docker continua preparado, mas não validado neste Mac.

O operador com acesso ao servidor também pode executar a API com `--create-teacher` e configurar TeacherSetup__Name, TeacherSetup__Email e TeacherSetup__Password no ambiente do processo. Não coloque esses valores em arquivos versionados ou argumentos de linha de comando. O comando não abre uma rota HTTP administrativa nem inicia o servidor, e exige esquema previamente migrado. Não cria uma sessão de acesso para a conta nova.

1. Professor entra em **Meus alunos → Gerar código**. O código vale sete dias e pode ser consultado novamente ao reabrir a tela.
2. Aluno entra em **Meu professor**, informa os 12 caracteres e confirma o compartilhamento do nome/e-mail com o professor.
3. O nome do professor aparece para o aluno, e o aluno passa a aparecer na lista daquele professor.
4. Aluno ou professor podem remover o vínculo após confirmação. A conta e seus registros permanecem preservados.

Neste MVP, cada aluno tem um professor ativo; o professor pode ter vários alunos. Para trocar de professor, o aluno remove o vínculo anterior e informa o novo código. Renovar código invalida o anterior para novos vínculos e preserva os alunos já associados. O código pode ser usado por vários alunos; ele não serve para fazer login ou obter o perfil de professor.

## Conceitos e decisões

**Autenticação** responde quem entrou: a sessão de acesso validada no MySQL identifica a conta. **Autorização** responde quais operações ela pode executar: o papel student/teacher determina as rotas permitidas, e o vínculo limita quais alunos um professor pode consultar. Esconder um botão no app não concede nem revoga acesso no servidor.

Em `src/Workoutly.Api/Auth/AuthService.cs`, o cadastro público define student explicitamente. A criação teacher é chamada somente pelo comando administrativo em Program.cs. As consultas de login e sessão retornam o papel de account_roles. `SessionAuthenticationHandler.cs` cria a claim de papel a cada requisição; o token permanece opaco e não carrega um papel controlado pelo cliente.

As políticas teacher e student em Program.cs usam a autorização por papel do ASP.NET Core. `Teaching/TeachingEndpoints.cs` aplica essas políticas às rotas e usa sempre o ID da sessão, ignorando tentativas de informar outro proprietário. `TeachingService.cs` consulta a relação com filtros de professor e aluno. O perfil professor/admin deste MVP não oferece uma listagem global de contas nem acesso irrestrito a dados privados.

A migração `004_roles_and_enrollment.sql` adiciona account_roles, teacher_invitations e student_teachers. As contas anteriores recebem student; reaplicar a migração não muda professores existentes. A chave primária do vínculo impede dois professores simultâneos para o mesmo aluno. A criação por código usa INSERT/SELECT com validade e papel do professor verificados no banco. Repetir o mesmo vínculo retorna o já existente. Outro vínculo ativo gera 409, sem sobrescrevê-lo.

O código usa 6 bytes aleatórios criptográficos apresentados como 12 caracteres hexadecimais, com validade de sete dias. Fica na tabela privada de convites e somente o próprio professor pode consultá-lo. As rotas de geração e associação têm limite de tentativas. Renovação transacional preserva o código anterior se a gravação falhar.

No app, `Services/AccountClient.cs` recebe o papel e realiza HTTP usando a sessão segura existente. `MainPage.xaml` apresenta o perfil e a entrada correspondente. `ConnectionsPage.xaml` e seu code-behind mostram código/lista para o professor e formulário/vínculo para o aluno, seguindo os estilos do Figma. Falhas não limpam o código digitado; expiração da sessão leva de volta ao acesso. O papel não é guardado no SecureStorage: a restauração consulta a API.

## Contratos

`POST /auth/register`, `POST /auth/login` e `GET /auth/me` agora incluem role: student ou teacher no objeto de perfil. Os demais campos continuam compatíveis.

| Rota | Perfil | Comportamento |
| --- | --- | --- |
| GET /teaching/invitation | Professor/admin | Código próprio e expiresAt, ou ambos nulos se não há convite ativo. |
| POST /teaching/invitation | Professor/admin | 201, gera/renova código. |
| GET /teaching/students | Professor/admin | 200, lista somente alunos vinculados. |
| GET /teaching/students/{id} | Professor/admin | 200 para aluno vinculado; 404 para outro aluno ou ID inexistente. |
| DELETE /teaching/students/{id} | Professor/admin | 204 se removeu vínculo próprio; 404 sem acesso à relação. |
| GET /enrollment | Aluno | 200, objeto teacher ou nulo dentro da resposta. |
| POST /enrollment | Aluno | Corpo com code; 201 vínculo criado, 200 se já era o mesmo, 400 código inválido/expirado, 409 se outro professor já está vinculado. |
| DELETE /enrollment | Aluno | 204, remove somente o vínculo da conta autenticada. |

Sem sessão: 401. Perfil incorreto: 403. Falhas de banco: 503 genérico. Fora de Development, exigir HTTPS; respostas dessas rotas usam no-store. Um vínculo não muda a autorização atual do catálogo: exercícios privados continuam pertencendo ao próprio criador. O acesso às fichas usa o vínculo ativo, conforme fichas.md. Alunos acessam apenas as próprias sessões em /sessions; o acesso do professor a sessões será implementado em HIST-01.

## Como testar

Execute `python3 scripts/test-native-mysql.py` para os contratos HTTP e o comando administrativo contra MySQL real isolado. Faça o fluxo professor → código → aluno → confirmação → professor → lista no simulador. Evidências e limites em [perfis e vínculos](evidencias/perfis-e-vinculos.md).

Referência técnica: [autorização por papel no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-10.0). As políticas de papel são combinadas com filtros de propriedade no banco, pois ter papel de professor não basta para autorizar acesso a qualquer aluno.
