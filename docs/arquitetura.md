# Arquitetura do Workoutly

## O que existe nesta fundação

`Workoutly.Mobile` possui a tela `MainPage.xaml` e seu code-behind de inicialização. `AppShell` organiza a navegação inicial. C# e XAML são mantidos juntos, sem serviços ou camadas vazias.

`Workoutly.Api` possui `GET /health`: retorna HTTP 200 e JSON com `service`, `status` e `databaseConfigured=false`. É uma indicação de que o processo atende HTTP, não de que o banco funciona. Não há autenticação ou acesso a dados nesta etapa. Não há rota de previsão do tempo do template.

`Workoutly.Api.Tests` inicia a aplicação com WebApplicationFactory e realiza uma requisição HTTP de verdade no servidor de testes em memória. Isso verifica serialização, status e contrato sem precisar de uma porta TCP ou MySQL.

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
