# Endpoints UI (vivo)

| Ruta | Auth | Notas |
|---|---|---|
| `/` | sí | principal interna 007 (fallback requiere auth; anónimo → `/Login`) |
| `/Login` GET/POST | no | login 007 (rate `Login`, anónimo) |
| `/Logout` POST | sí | logout 007 (rate `Login`, antiforgery, `no-store`) |
| `/Denegado` | no | 403 genérico 007 (`AccessDeniedPath`) |
| `/Privacy` | no | excepción consciente al fallback (007) |
| `/Error` | no | siempre anónimo (evita bucles, 007) |
| `/health` | no |liveness (exento de rate-limit) |
| `/health/ready` | no |ready sin SQL en base (exento de rate-limit) |

Las specs solo enlazan, no duplican.
