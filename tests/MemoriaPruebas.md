# MemoriaPruebas — WebSDD (normativa)

> Patrones obligatorios para tests. Lo que contradiga esto se repara antes del push.

## Estructura
- `UnitTest/`: lógica aislada + PageModels/Controladores con dependencias mockeadas.
- `IntegrationTest/`: `WebApplicationFactory` + `UseSetting("UseInMemoryDatabase","true")`. En .NET 10 el `Program` generado ya es público (ASP0027: `public partial class Program` innecesario).
- Config en tests: `appsettings.json` es gitignorado y no existe en CI. Los tests WAF usan factoría compartida con `UseSetting` espejo de `appsettings.Example.json` (fail-fast de `Program.cs` lo exige); prohibido asumir el archivo real en disco.
- `SecurityTest/`: auth requerida → redirect/401, antiforgery real, XSS `MemberData`, secretos.

## Moq
- `ReturnsAsync(default(T))` o `(T?)null!`, nunca `ReturnsAsync(null)`.
- `_` (discard) para mocks no usados (S1481).
- Asserts `TempData` con mensaje exacto del controlador.

## Externos
- Requieren red/API externa → `[Skip("Requiere API externa")]`.

## Comandos
```powershell
dotnet test UnitTest/UnitTest.csproj -c Release --no-build
dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build
```
