# MemoriaPruebas — WebSDD (normativa)

> Patrones obligatorios para tests. Lo que contradiga esto se repara antes del push.

## Estructura
- `UnitTest/`: lógica aislada + PageModels/Controladores con dependencias mockeadas.
- `IntegrationTest/`: `WebApplicationFactory` + `UseSetting("UseInMemoryDatabase","true")`. Requiere `public partial class Program` (deuda hasta implementarlo).
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
