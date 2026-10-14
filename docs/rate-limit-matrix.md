# Rate Limit Matrix (vivo)

| Policy | Límite | Respuesta |
|---|---|---|
| Global | 1000/min/IP | 429 uniforme |
| Login POST | 5/5min/IP solo POST (el GET no consume) | 429 uniforme + mensaje + `Retry-After` |
| `/health*` | exento | medición fiable, sin rate-limit |

Las specs solo enlazan.
