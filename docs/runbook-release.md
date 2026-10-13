# Runbook Release (vivo)

> Release manual. Sin CD, sin hosting: publicar, verificar y tagear sin improvisar.
> Canónico aquí; `constitution.md` §10 y `AGENTS.md` §9 solo enlazan, no duplican.
> Rutas reales en `docs/endpoints-ui.md`. Sin secretos en comandos ni logs.

## 0. Precondiciones
- `main` verde (CI `ci-pr.yml`), trabajo commiteado, SDK .NET 10.
- Env requeridos (ver `WebSDD/appsettings.Example.json`): `ApiSettings__BaseUrl`, `DataProtection__KeysPath`.

## 1. Publish (dry-run local)
```powershell
dotnet publish WebSDD/WebSDD.csproj -c Release -o ./publish -p:TreatWarningsAsErrors=true
```
Esperado: `0 Advertencia(s), 0 Errores`, existe `./publish/WebSDD.dll`. (`publish/` está gitignorado.)

## 2. Ejecutar
Vía A (sin Docker):
```powershell
$env:ApiSettings__BaseUrl="http://localhost:7227/"
$env:DataProtection__KeysPath="$PWD/dpkeys-local"
dotnet ./publish/WebSDD.dll --urls http://localhost:5290
```
Vía B (requiere Docker): `docker compose up --build webapp` → `:5001→8080`, mock `:7227`.

## 3. Verificación
```powershell
curl.exe -s -o NUL -w "%{http_code}\n" http://localhost:5290/health/ready
# → 200
curl.exe -s -D - http://localhost:5290/health/ready -o NUL | Select-String "content-security-policy|x-content-type-options|referrer-policy"
# → presentes (ver SPEC-001)
curl.exe -s -o NUL -w "%{http_code}\n" http://localhost:5290/ruta-inexistente-xyz
# → 404 fail-closed
```
- Login: N/A en base (sin ruta de login en `docs/endpoints-ui.md`). Con slice auth-UI: POST credenciales → 200 + cookie; sin token en ruta protegida → 401.
- 401: no demostrable hoy (ninguna ruta exige auth).
- ZAP: manual vía Actions `dast-manual` (`workflow_dispatch`), informativo hasta URL estable.

## 4. Tag
```powershell
git tag vX.Y.Z-sliceN
git push origin vX.Y.Z-sliceN
```
Sustituir `X.Y.Z` + `N`. No pushear tag sin revisor.

## 5. Rollback (sin hosting: N/A documentado)
Sin hosting no hay despliegue que revertir. Con hosting futuro: re-tagear versión anterior y repetir §1–§3. Registro fuera del repo si aplica (`AGENTS.md` §9).

## 6. Condición futura de bloqueo ZAP
ZAP pasa a bloqueante (`fail_action: true`, sin `continue-on-error`) cuando haya URL estable + baseline de falsos positivos con allowlist documentada (`plan.md` 006 §2).
