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

### Docker Compose (הרצה בפקודה אחת)

דרוש Docker Desktop.

```bash
cd infra
cp .env.example .env      # ערכו את MSSQL_SA_PASSWORD
docker compose up --build
```

מרים שלושה שירותים:

| שירות | כתובת | הערה |
|---|---|---|
| `db` (SQL Server 2022) | `localhost:1433` | משתמש `sa`, סיסמה מ‑`.env`. נתונים נשמרים ב‑volume `mssql-data` |
| `api` (.NET 8) | http://localhost:5080/health · Swagger ב‑`/swagger` | |
| `client` (Vite) | http://localhost:5173 | קריאות `/api/*` עוברות proxy ל‑`api` |

עצירה: `docker compose down` (הוסיפו `-v` כדי למחוק גם את נתוני ה‑DB).

### הרצה ידנית (בלי Docker)

- **api:** `cd server && dotnet run --project src/Api` → http://localhost:5080
- **client:** `cd client && npm install && npm run dev` → http://localhost:5173
- **db:** SQL Server מקומי; עדכנו `ConnectionStrings:SqlServer` ב‑`server/src/Api/appsettings.Development.json`.
