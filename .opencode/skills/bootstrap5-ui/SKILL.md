---
name: "bootstrap5-ui"
description: "Bootstrap 5 + Razor generico: layout, tablas, iconos, skeleton, accesibilidad"
license: "MIT"
---
# Bootstrap 5 UI generico

## Setup
- Layout compartido, `site.css/site.js`, `<meta viewport>`. No duplicar CDN.

## Tablas/UI
- Envolver en `.card-table`, `.table-jtable`, `.btn-icon` solo icono, paginacion con chevrons, skeleton + toggle JS. Colores por variables del tema, no fijos.

## Grid
- `.container>.row>.col-md-*`, breakpoints, `g-*`. No anidar containers.

## Razor
- `asp-for/validation-for/action/route/items`, `_ValidationScriptsPartial`, `nonce` desde `Context.Items`, `ModelOnly` + `novalidate`.

## A11y
- `aria-label` icon-only, `aria-current` nav, `alt`, semantico, no quitar `:focus`, contraste AA.
