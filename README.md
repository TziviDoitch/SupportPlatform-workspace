# SupportPlatform — מערכת תמיכות רוחבית (PoC)

Proof of Concept למערכת לניהול וחיפוש בקשות תמיכה עבור מינהל התרבות והספורט.
מטלת Take‑Home — הדגמת חשיבה מערכתית, ארכיטקטורה, תכנון תשתיות ועבודה Full‑Stack.

> **סטטוס:** בהקמה. תוכנית העבודה המלאה: [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md).

## Stack

| תחום | טכנולוגיה |
|---|---|
| Backend | .NET 8 Web API (C#), EF Core 8 |
| DB | SQL Server (PoC) |
| Client | React + TypeScript + Vite, Ant Design, TanStack Query |
| גרפים | Chart.js |
| הרצה | Docker Compose |

## מבנה

```
server/    ASP.NET Core solution (Api / Application / Domain / Infrastructure)
client/    React + TypeScript (Vite)
docs/      ARCHITECTURE.md, DESIGN_QA.md, DEVOPS.md, contracts/
infra/     docker-compose ופריטי DevOps
```

## הרצה

יתווסף עם השלמת S0 (ראו `IMPLEMENTATION_PLAN.md`).
