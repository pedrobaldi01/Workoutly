# Workoutly

Aplicativo acadêmico de acompanhamento de treinos, criado do zero para o Projeto Integrador 6.

## Situação atual

Esta entrega é a **fundação técnica**, não o MVP funcional. O app MAUI tem uma tela inicial; a API responde em `/health`; os testes verificam esse contrato HTTP. Cadastro, autenticação, fichas, exercícios, sessões, indicadores e MySQL ainda serão implementados.

Nenhum código do aplicativo anterior foi reutilizado. Os roteiros enviados ao professor permanecem na pasta superior, sem alterações.

## Componentes

| Local | Responsabilidade |
| --- | --- |
| `src/Workoutly.Mobile` | Interface mobile nativa em C# e XAML para Android e iOS. |
| `src/Workoutly.Api` | Serviço HTTP ASP.NET Core. Futuramente centraliza autenticação, validações e acesso ao MySQL. |
| `tests/Workoutly.Api.Tests` | Testes HTTP executados em servidor de teste em memória. Não substituem testes reais de banco ou dispositivo. |
| `docs` | Requisitos, backlog, decisões e evidências. |
| `.agents/skills` | Procedimentos de apoio ao Codex, não dependências do app. |

## Pré-requisitos

- SDK .NET **10.0.401** (ou patch compatível), workload set **10.0.401.1** e MAUI **10.0.110**; ver `global.json` e `Directory.Build.props`.
- Workloads MAUI: `dotnet workload restore src/Workoutly.Mobile/Workoutly.Mobile.csproj`.
- Mac para compilar e executar iOS: Xcode selecionado, runtime do simulador e componentes compatíveis com o workload.
- Android: Android SDK e JDK 21. O teste no Windows requer a configuração desses componentes nesse computador.
- Para instalar em iPhone físico: assinatura e provisionamento Apple.

Neste Mac o SDK utilizado está em `/Users/pedrobaldi/.dotnet/dotnet`. Se o comando `dotnet --version` mostrar outro SDK, use esse executável ou ajuste o PATH do terminal/IDE.

## Abrir e executar

Abra `Workoutly.slnx` no Rider ou Visual Studio compatível com .NET 10.

### API e testes

Na raiz do repositório:

```sh
dotnet run --project src/Workoutly.Api --launch-profile http
```

Abra http://localhost:5080/health. Resposta esperada:

```json
{"service":"Workoutly.Api","status":"ok","databaseConfigured":false}
```

A API de desenvolvimento escuta somente no computador local. Não está exposta à rede e o aplicativo ainda não se comunica com ela. HTTPS e acesso por dispositivos serão configurados no incremento de integração.

```sh
dotnet build src/Workoutly.Api/Workoutly.Api.csproj
dotnet test tests/Workoutly.Api.Tests/Workoutly.Api.Tests.csproj
```

### iPhone no Mac

Na IDE, selecione `Workoutly.Mobile`, o framework `net10.0-ios`, um simulador de iPhone e **Run**.

Para conferir apenas a compilação no Mac Apple Silicon:

```sh
dotnet build src/Workoutly.Mobile/Workoutly.Mobile.csproj -f net10.0-ios -p:RuntimeIdentifier=iossimulator-arm64
```

### Android no Mac ou Windows

Na IDE, selecione `Workoutly.Mobile`, `net10.0-android` e um emulador ou aparelho Android com depuração USB.

```sh
dotnet build src/Workoutly.Mobile/Workoutly.Mobile.csproj -f net10.0-android
```

Neste Mac, o SDK detectou Java 25 por padrão. Para usar o JDK 21 já instalado sem alterar o Java global:

```sh
dotnet build src/Workoutly.Mobile/Workoutly.Mobile.csproj -f net10.0-android -p:JavaSdkDirectory=/opt/homebrew/opt/openjdk@21/libexec/openjdk.jdk/Contents/Home
```

Esse caminho é específico deste Mac. No Windows ou em outro Mac, selecione o JDK 21 instalado na máquina pela IDE ou pela propriedade `JavaSdkDirectory`.

A solução inclui o projeto mobile: compilar a solução inteira exige os workloads das plataformas. Para trabalhar somente no backend, compile o projeto da API e execute seu projeto de testes.

## Como vamos desenvolver

Leia `docs/backlog.md` para a sequência e critérios de aceite, `docs/arquitetura.md` para a separação de responsabilidades e `docs/skills.md` para usar as skills instaladas. O próximo incremento é a persistência MySQL. Depois vêm conta e acesso, fichas, sessões e indicadores, em fluxos completos entre app, API e banco.

Ao concluir cada incremento serão registrados o conceito, a decisão, onde aparece no código e a forma de testar. Slides e avaliação com participantes permanecem para as etapas correspondentes.

O openGym é referência de experiência e organização, não código incorporado. Ver `docs/referencias.md`.
