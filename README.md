# WebSDD — Web con Spec-Driven Development + DevSecOps

Web ASP.NET Core 10 (Razor Pages + MVC, BFF sin DB directa: consume API externa vía `ApiSettings:BaseUrl`).

Documentos rectores: `constitution.md` (qué no se negocia) + `AGENTS.md` (cómo trabajar). Leer ambos + `Memoria.md` antes de tocar código.

## Spec-Driven Development
- `constitution.md` manda ante conflicto; todo cambio nace en `specs/NNN-slug/` (`spec,plan,tasks,checklist`) con Aprobación y Control de Cambios.
- Plantillas en `specs/templates/`; trazabilidad `T-xxx→FR-xxx`; `CHECKLIST_PR.md` + 1 revisor; `Memoria.md` bitácora por fase (Estado/Decisiones/Deuda/A evitar).
- Specs base: `000` configuración, `001` hardening, `002` quality gates, `003` CI completo, `004` supply chain, `005` pre-commit, `006` runbook/ZAP.

## DevSecOps
- Local: `dotnet format`, build `TreatWarningsAsErrors` 0/0, tests en 3 suites, `gitleaks protect`, pre-commit.
- CI `ci-pr.yml`: build, CodeQL, Semgrep, SCA, secret (+critic, coverage, Stryker, Sonar en 003; Trivy/SBOM en 004).
- Supply chain: `NuGetAudit all`, Dependabot, Trivy HIGH/CRITICAL, SBOM CycloneDX. Release manual con `docs/runbook-release.md`; sin CD.
- Producto: headers + CSP nonce, HSTS, CORS 1 origen, rate-limit antes de Auth, cookie Strict, antiforgery, `IsLocalUrl`.

## Sub-agentes (`.opencode/agents/`)
| Agente | Hace |
|---|---|
| `slice-builder` | Slice UI→Servicio→Externo + cierre Memoria/checklist |
| `razor-ui` | Solo markup/CSS/JS, sin C# ni shell |
| `sec-auditor` | Auditoría solo lectura, reporta `ruta:línea` |
| `test-qa` | Tests + coverage advisory |
| `devsecops` | Calidad, CI, supply-chain y devux config |
| `spec-planner` | `spec/plan/tasks/checklist` sin tocar código |

Skills: `aspnet-core`, `bootstrap5-ui`, `slice-module`, `slice-pattern`, `quality-gates`.

## Prerrequisitos
- .NET 10 SDK (`dotnet --version` → 10.x)
- Python 3 (pre-commit + gates `scripts/check_coverage*.py`; en CI ubuntu ya existe)
- Gitleaks (hook `gitleaks protect --staged`; instalar desde https://github.com/gitleaks/gitleaks/releases)
- Docker (solo contenedores)

## Setup
```powershell
cp WebSDD/appsettings.Example.json WebSDD/appsettings.json
dotnet restore
pip install pre-commit
pre-commit install  # hooks locales format+gitleaks (requieren SDK+gitleaks; si faltan, el hook falla con mensaje claro, no silencioso)
# Windows: si format falla por EOL preexistente (CRLF vs lf), normalizar con dotnet format o commitear con --no-verify (ver Memoria Fase-5)
dotnet run --project WebSDD/WebSDD.csproj
docker compose up webapp  # :5001→8080, mock :7227
```

## Comandos
```powershell
dotnet build -c Release -p:TreatWarningsAsErrors=true
dotnet test UnitTest/UnitTest.csproj -c Release --no-build
dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build
gitleaks protect --staged -c .gitleaks.toml
```

## Estructura
- `WebSDD/` web, `UnitTest/|IntegrationTest/|SecurityTest/`, `specs/` + `specs/templates/`, `docs/` vivos, `scripts/` gates, `.github/workflows/` (ci-pr + dast-manual), `.opencode/agents/` + `.opencode/skills/`.
