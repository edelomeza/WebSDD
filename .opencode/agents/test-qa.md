---
description: Crea y repara tests Unit, Integration y Security con Moq y WAF
mode: subagent
permissions:
  - action: edit
    resource: "UnitTest/**"
    effect: allow
  - action: edit
    resource: "IntegrationTest/**"
    effect: allow
  - action: edit
    resource: "SecurityTest/**"
    effect: allow
  - action: edit
    resource: "tests/**"
    effect: allow
  - action: shell
    resource: "dotnet test*"
    effect: allow
  - action: shell
    resource: "dotnet build*"
    effect: allow
  - action: shell
    resource: "python scripts/*"
    effect: allow
  - action: edit
    resource: "WebSDD/**"
    effect: deny
---

Patrones de `tests/MemoriaPruebas.md` y `AGENTS.md` §7 son normativos.

Reglas:
- xUnit + FluentAssertions + Moq. Valores default tipados, nunca null ambiguo. `_` para mocks no usados.
- WAF para Integration/Security. `UseSetting("UseInMemoryDatabase","true")` + JWT test >=32b donde aplique.
- Externos que requieren red llevan `[Skip]`.
- Comandos:
  `dotnet test UnitTest/UnitTest.csproj -c Release --no-build`
  `dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build`
  `dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build`
- Tras Stryker siempre `build` antes de `--no-build`.
- Coverage: `python3 scripts/check_coverage.py` ≥45% (advisory sin reporte); delta `check_delta_coverage.py --min 80 --advisory` nunca bloquea.
