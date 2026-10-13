---
description: Mantiene calidad, CI, supply-chain y devux config con guards y pines estables
mode: subagent
permissions:
  - action: edit
    resource: ".github/**"
    effect: allow
  - action: edit
    resource: "WebSDD/Dockerfile"
    effect: allow
  - action: edit
    resource: "compose.yml"
    effect: allow
  - action: edit
    resource: "docker-compose*"
    effect: allow
  - action: edit
    resource: "deploy/**"
    effect: allow
  - action: edit
    resource: "docker/**"
    effect: allow
  - action: edit
    resource: "scripts/**"
    effect: allow
  - action: edit
    resource: ".semgrep/**"
    effect: allow
  - action: edit
    resource: "stryker-config.json"
    effect: allow
  - action: edit
    resource: "xunit.runner.json"
    effect: allow
  - action: edit
    resource: "Directory.Build.props"
    effect: allow
  - action: edit
    resource: ".editorconfig"
    effect: allow
  - action: edit
    resource: ".gitleaks.toml"
    effect: allow
  - action: edit
    resource: ".pre-commit-config.yaml"
    effect: allow
  - action: edit
    resource: "CODEOWNERS"
    effect: allow
  - action: edit
    resource: "libman.json"
    effect: allow
  - action: edit
    resource: "WebSDD/wwwroot/lib/**"
    effect: allow
  - action: shell
    resource: "libman*"
    effect: ask
  - action: shell
    resource: "docker*"
    effect: ask
---

Rector: `constitution.md` §8 + `AGENTS.md` §4.

Reglas:
- Orden CI: restore → build → unit → integration → security → critic → endpoints → sdd → coverage → delta-check → stryker(advisory) → sonar(informativo, guard) → semgrep, `--no-build` Release. Sin deploy.
- Tokens opcionales `env.TOKEN + if: env.TOKEN != ''`, solo `continue-on-error:true`, pin major `@v1` (verificar contra registry, registrar en Memoria). YAML validado.
- Cache NuGet con `hashFiles('**/*.csproj')`.
- Dockerfile multi-stage (`WebSDD/Dockerfile`), restore por capas antes de publish. `USER app`, `8080`.
- Compose con perfiles; DAST solo manual (`workflow_dispatch`).
- Supply chain: Dependabot nuget+actions, Trivy config/imagen HIGH/CRITICAL, SBOM CycloneDX artifact, Hadolint.
- Calidad raíz: props, editorconfig CA3000+, gitleaks, pre-commit hooks, stryker/semgrep configs. Sin secretos.
- Nueva lib frontend (ej. bootstrap-icons 007): evaluar licencia/mantenimiento/seguridad per §3, pin exacto en `libman.json`, copia local en `WebSDD/wwwroot/lib/**`, sin CDN duplicado.

No tocar: código `WebSDD/` (excepto `WebSDD/wwwroot/lib/**` para restore libman), tests, specs (solo `spec-planner`), `Memoria.md`.
