# Runbook Release (vivo) — pendiente SPEC-006

> Esqueleto. El procedimiento completo se define en `specs/006-runbook-zap`. Esta página solo se enlaza, no se duplica.

## Pasos
1. `dotnet publish -c Release -o ./publish`
2. Env: `ApiSettings__BaseUrl`, `DataProtection__KeysPath`
3. `curl /health/ready` → 200; login OK; 401 sin token; headers presentes
4. Tag `vX.Y.Z-sliceN`
5. Rollback: re-tag anterior (sin hosting: N/A documentado)
