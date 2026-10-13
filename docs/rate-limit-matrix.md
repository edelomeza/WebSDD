# Rate Limit Matrix (vivo)

| Policy | Límite | Respuesta |
|---|---|---|
| Global | 1000/min/IP | 429 uniforme |
| Login formas | 5/5min | 429 uniforme |
| `/health*` | exento | medición fiable, sin rate-limit |

Las specs solo enlazan.
