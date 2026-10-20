# Arquitetura do Workoutly

## O que existe atualmente

`Workoutly.Mobile` possui a tela `MainPage.xaml` e seu code-behind de inicialização. `AppShell` organiza a navegação inicial. C# e XAML são mantidos juntos, sem serviços ou camadas vazias.

`Workoutly.Api` possui `GET /health` para verificar o processo e `GET /health/ready` para consultar a versão persistida no MySQL. A conexão é configurada por variável de ambiente, somente no servidor. O comando `--migrate` prepara o histórico de versões; abrir a API não modifica o banco. Ainda não há autenticação ou tabelas de treino. Detalhes e comandos em mysql.md.

`Workoutly.Api.Tests` inicia a aplicação com WebApplicationFactory e realiza uma requisição HTTP de verdade no servidor de testes em memória. Os testes HTTP sem banco verificam serialização, status e estados de falha. O teste Category=MySql verifica a migração e sua persistência em uma base MySQL real e aleatória, isolada dos dados da aplicação. O script nativo também verifica Kestrel por TCP local e reinício da API. Sem configuração de teste, o teste MySql é ignorado explicitamente, sem alegar validação real.

## Fluxo previsto

O usuário interage com o app MAUI. O app enviará requisições HTTP para a API. A API fará validações e autorização e persistirá os dados no MySQL. Somente a API conhecerá a conexão com o banco.

Esses três componentes são separados porque a interface muda sem exigir conexão direta ao banco e as regras de acesso ficam centralizadas. Os futuros contratos serão documentados conforme cada recurso for implementado; não há contratos simulados de cadastro ou treino nesta base.

## Conexão e histórico

A versão final exige conexão com a API. Na ausência de rede, o formulário em andamento será preservado para nova tentativa. Não haverá sincronização offline nesta versão. Edições e exclusões de fichas devem preservar a identificação dos exercícios e dados registrados em sessões passadas; a estratégia será implementada junto ao modelo de persistência.

## Aprendizado deste incremento

- **Solução**: agrupa os projetos para a IDE; não é um servidor nem o aplicativo.
- **Aplicativo MAUI**: gera apps nativos de cada plataforma a partir de C# e XAML.
- **API**: processo independente que responde a requisições HTTP; estará entre o app e o banco.
- **Teste de integração**: verifica o comportamento do endpoint a partir de uma requisição, não chama diretamente a função da rota.
- **Git**: registra versões de código e documentação; publicar no GitHub é uma ação separada.
- **Skill**: procedimento lido pelo agente quando apropriado. Não faz parte do aplicativo que o usuário instala.
