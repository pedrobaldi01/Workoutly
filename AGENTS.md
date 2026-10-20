# Workoutly

## Acordos do projeto
- Responda em português e explique o conceito, a decisão e a verificação ao concluir cada incremento.
- Implementação nova: não reutilizar o aplicativo antigo. Documentos acadêmicos na pasta superior são fontes de requisitos e não devem ser editados sem pedido específico.
- Stack: .NET 10, MAUI com XAML e code-behind, API ASP.NET Core e MySQL somente no servidor.
- Cadastro, login, perfil, logout e catálogo de exercícios estão integrados ao MySQL pela API; o app usa SecureStorage para a sessão. Exercícios iniciais são compartilhados e cadastros próprios são privados. Fichas e treinos ainda não existem. Não apresentar planejamento como resultado.
- Implementar o menor incremento completo pedido; evitar abstrações antecipadas, serviços vazios e recursos fora do MVP.
- Funcionalidades finais online; preservar formulário em falhas de rede. Não implementar sincronização offline.
- Autenticação usa PasswordHasher do ASP.NET Core Identity e tokens opacos revogáveis, guardados somente como hash no servidor. Não substituir por hash simples de senha ou sessão em Preferences.
- Não embutir credenciais ou dados pessoais. Somente dados fictícios nos testes automatizados.
- Ler docs/backlog.md e docs/arquitetura.md antes de alterar o escopo. Usar GLOSSARY.md para os termos.
- Não adicionar integrações sociais, wearables, prescrição clínica ou AI Coach ao MVP.

## Verificações
- API: `dotnet build src/Workoutly.Api/Workoutly.Api.csproj`.
- Integração HTTP: `dotnet test tests/Workoutly.Api.Tests/Workoutly.Api.Tests.csproj`.
- Persistência real: `python3 scripts/dev.py test-db` com o MySQL Compose disponível, ou `python3 scripts/test-native-mysql.py` neste Mac. Sem banco, o teste MySql é ignorado explicitamente.
- Preparar esquema com `python3 scripts/dev.py migrate`; nunca executar DDL automaticamente no startup. Não imprimir conexões nem incluir credenciais no Git.
- iOS no Mac: `dotnet build src/Workoutly.Mobile/Workoutly.Mobile.csproj -f net10.0-ios -p:RuntimeIdentifier=iossimulator-arm64`.
- Android: `dotnet build src/Workoutly.Mobile/Workoutly.Mobile.csproj -f net10.0-android` (requer JDK 21 e Android SDK).
- Compilação não prova execução. Registrar a plataforma e os limites da verificação em docs/evidencias/.
- Testar comportamento e contratos públicos, sem testes que apenas repitam a implementação.

## Agent skills
- Skills locais em .agents/skills; origem e revisão em docs/skills.md. Ler o SKILL.md aplicável antes de usar uma skill.
- Tarefas em docs/backlog.md, com IDs e dependências. Não publicar issues, PRs ou mensagens externas automaticamente.
- Vocabulário em GLOSSARY.md; decisões em docs/adr/. Configuração em docs/agents/.
- Usar tdd para regras e contratos novos quando houver comportamento a testar; diagnosing-bugs para falhas difíceis; code-review para revisão solicitada.
- diagram-design para visuais que ajudem a explicar o sistema; definir o perfil visual antes do primeiro diagrama.
- Usar subagentes quando solicitado ou quando as instruções da skill aplicada exigirem delegação.
