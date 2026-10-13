# ASVS Web Checklist (vivo)

- [x] Validación servidor + antiforgery + `IsLocalUrl` (007: `LoginInput` + `ModelState` + `LocalRedirect`)
- [x] CSP nonce + headers OWASP (001: middleware propio + nonce por request)
- [x] Sin secretos en código/logs (007: `SecretFilter` + `AuthService` sin `strPWD/strToken` en logs)
- [x] Password nunca en caché/logs (007: claim solo `Name`+`access_token`, sin `IMemory` de password)

Las specs solo enlazan.
