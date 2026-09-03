# SupportPlatform — מערכת תמיכות רוחבית (PoC)

Proof of Concept למערכת חוצת-ארגונים לאחסון וחיפוש בקשות תמיכה ממשלתיות.
מטלת Take‑Home — הדגמת חשיבה מערכתית, ארכיטקטורה, תכנון תשתיות ועבודה Full‑Stack.
.NET 8 Web API + React/TypeScript, הרצה ב-Docker Compose.

התוכנית המלאה (החלטות נעולות, מבנה עבודה, שלבי S0–S11): [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md).

---

## הרצה

### א. Docker Compose — הרצה בפקודה אחת (מומלץ)

דרוש Docker Desktop.

```bash
cd infra
cp .env.example .env      # ערכו את MSSQL_SA_PASSWORD (>=8 תווים, 3 מתוך 4 סוגי תווים)
docker compose up --build
```

מרים שלושה שירותים:

| שירות | כתובת | הערה |
|---|---|---|
| `db` (SQL Server 2022) | `localhost:1433` | משתמש `sa`, סיסמה מ‑`.env`. נתונים ב‑volume `mssql-data` |
| `api` (.NET 8) | http://localhost:5080 · `/health` · Swagger ב‑`/swagger` | ב-Development מריץ מיגרציות + seed באתחול |
| `client` (Vite) | http://localhost:5173 | קריאות `/api/*` עוברות proxy ל‑`api` |

עצירה: `docker compose down` (`-v` מוחק גם את נתוני ה‑DB).

> **הערה:** אין `healthcheck` על `db` ו-`api` תלוי בו ב-`depends_on` פשוט. בהרצה קרה
> ראשונה ה-api עלול לעלות לפני ש-SQL Server מקבל חיבורים ולקרוס; `docker compose up`
> שני עולה תקין. (שיפור מתוכנן — ראו [מגבלות](#מגבלות).)

### ב. הרצה ידנית (בלי Docker)

דרוש: .NET 8 SDK, Node 20+, ו-SQL Server מקומי (או LocalDB ב-Windows).

- **api:** `cd server && dotnet run --project src/Api` → http://localhost:5080
  (ב-Development מריץ מיגרציות + seed; דורש DB זמין לפי `ConnectionStrings:SqlServer`
  ב-`server/src/Api/appsettings.Development.json`).
- **client:** `cd client && npm install && npm run dev` → http://localhost:5173
- **Windows + LocalDB:** `./run-local.ps1` מפעיל את שלושתם יחד מול `(localdb)\MSSQLLocalDB`.

### משתמשי seed

אין מסך התחברות ב-PoC. הזהות היא כותרת `X-User` (ברירת מחדל `sarah`); הלקוח שולח
אותה מ-`client/src/api/config.ts`. לפעולה כמשתמש אחר — שנו את הערך שם, או שלחו את
הכותרת ידנית (Swagger / `server/src/Api/SupportPlatform.Api.http`).

| שם משתמש | tenant | role | סיסמה (דמו) |
|---|---|---|---|
| `sarah` | `culture-sport-admin` | analyst | `pass` |
| `dan` | `culture-sport-admin` | admin | `pass` |
| `michal` | `welfare-admin` | analyst | `pass` |

---

## ארכיטקטורה — תקציר

מפורט: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) (כולל Decision Log, ERD, דיאגרמת Container).

**Backend — 4 שכבות**, תלות חד-כיוונית, `Application` לא מכיר EF:

```
Api             Controllers דקים, Swagger, ProblemDetails (IExceptionHandler + ProblemTypes),
                Middleware (correlation id), זהות (X-User)
Application     שירותי use-case, DTOs, validators; Search/ = QueryDefinition + FilterValue +
                validator + QuestionTextRenderer + BucketPaging; NlQuery/ = תפר ה-AI + parser
Domain          entities, FilterFieldRegistry — בלי הפניות framework
Infrastructure  EF Core DbContext, repositories, migrations, seed; Search/ = DynamicQueryBuilder +
                Filters/ handlers + executor
```

**הליבה:** `QueryDefinition` הוא אובייקט קנוני יחיד — הטופס בונה אותו, ה-NL parser
מפיק אותו, השאילתה השמורה *היא* הוא, מנוע ה-SQL מתרגם אותו, מנסח השאלה קורא אותו.
`DynamicQueryBuilder` מרכיב `IQueryable` דרך whitelist מ-`filter_field_registry` בלבד —
שדה שלא ברשימה נדחה לפני שרץ handler, בלי `switch`, בלי reflection, בלי ביטויים מחרוזתיים.

**מודולים אנכיים:** Metadata · Search · SavedQueries · NlQuery · Audit · Identity(stub).

**Client:** `api/` (http seam + interceptor), `hooks/` + `lib/` משותפים, `features/`
(search / results / saved-queries / nl-query), `state/` (TanStack Query client).
הטופס נבנה דינמית מ-`GET /api/metadata` — אף שדה/תווית/רשימת ערכים לא מקודדים קשיח.

### נקודות קצה

| Method | Path | תיאור |
|---|---|---|
| `GET` | `/api/metadata?tenantId=` | רשימות ייחוס + `filterFieldRegistry` (מזין את הטופס) |
| `POST` | `/api/search` | הרצת `QueryDefinition` → `questionText` / `rows` / `aggregations` / `page` / `executionMeta` |
| `GET/POST/PUT/DELETE` | `/api/saved-queries[/{id}]` | CRUD, scoped ל-owner+tenant; DELETE דורש role `admin` |
| `POST` | `/api/saved-queries/{id}/run` | הרצה חוזרת; תגובה כמו `/search` |
| `POST` | `/api/nl-queries/parse` | טקסט חופשי → `{ definition, interpretationText, confidence, unresolved }` |
| `GET` | `/health` | `200 Healthy` |

כל בקשה מחזירה `X-Correlation-Id`; כל שגיאה היא `application/problem+json` (RFC 7807) —
ראו [`docs/contracts/error-model.md`](docs/contracts/error-model.md).

---

## בחירות טכנולוגיות ונימוקים

מקור: `IMPLEMENTATION_PLAN.md` §2 (נעול).

| תחום | בחירה | נימוק |
|---|---|---|
| Backend | .NET 8 Web API (C#) | אוצר המילים של המטלה (Solution / Repository / Services) |
| שכבת נתונים | EF Core 8 | הרכבת `IQueryable` דינמית עם whitelist בטוח |
| DB (מימוש) | SQL Server (`mcr.microsoft.com/mssql/server`) | אילוץ מעשי — היכרות/רישוי קיימים. JSON כ-`nvarchar(max)` + `ToJson()` |
| DB (יעד קוד-פתוח) | PostgreSQL | המודל provider-agnostic; מעבר = החלפת provider + connection string (`ARCHITECTURE.md` §5) |
| Client | React + TypeScript + Vite | — |
| State | TanStack Query + hooks מקומיים | בלי Redux/מנגנון כבד |
| ספריית עיצוב | Ant Design v6 (`ConfigProvider direction="rtl"`, `he_IL`) | רכיבים מוכנים + תמיכת RTL |
| גרפים | Chart.js (`react-chartjs-2`) | גרף עמודות בסיסי על ה-`aggregations` |
| הפשטת AI | `INlQueryProvider` + בחירה לפי configuration | ממומש `ruleBased` (דטרמיניסטי, בלי LLM); ספק אמיתי = תפר בלבד |
| Logging | Serilog (Console) + Correlation Id | בלי קונפיג רב-sinks |
| Validation | FluentValidation על `QueryDefinition` | — |
| Auth | כותרת `X-User` + tenant filter + בדיקת role אחת | IdP/JWT אמיתי = יעד production (`ARCHITECTURE.md` §8.1) |
| Container | Docker Compose (`mssql + api + client`) | "הרצה בפקודה אחת" |

---

## מבנה הפרויקט

```
server/   פתרון .NET 8 — Api / Application / Domain / Infrastructure (+ tests/, פרויקט לכל שכבה)
client/   React + TypeScript (Vite) — api / hooks / lib / features / state / models / components
docs/     ARCHITECTURE.md · DESIGN_QA.md · TEST_PLAN.md · REVIEW_NOTES.md · EXTENSIBILITY_DEMO.md · contracts/
infra/    docker-compose.yml · .env.example
run-local.ps1   הרצה ידנית מול LocalDB (Windows, בלי Docker)
```

מוסכמות לכל פרויקט: [`server/CLAUDE.md`](server/CLAUDE.md), [`client/CLAUDE.md`](client/CLAUDE.md).

---

## מסד נתונים — מיגרציות ו-seed

- **מיגרציות** (`server/src/Infrastructure/Persistence/Migrations/`, additive):
  `InitialCreate` → `TenantAndReferenceFkDeleteBehavior` → `SavedQueriesAndAudit`.
  ב-`dotnet run` ב-Development, `Program.cs` מריץ `Migrate()` ואז `DbSeeder.Seed()`.
  ידנית: `dotnet tool restore` ואז
  `dotnet dotnet-ef database update --project src/Infrastructure --startup-project src/Infrastructure`.
- **seed** (`DbSeeder`): דטרמיניסטי (RNG seed קבוע) ו-idempotent (no-op אם יש כבר
  שורות). 2 tenants, 3 משתמשים, ~40 גופים, 500 בקשות בהתפלגות מכוונת
  (320 `culture-sport-admin` / 180 `welfare-admin`; שנים 30/40/30; סטטוס 55/25/20).
  סיסמאות seed נשמרות כ-hash בלבד (`SeedPasswordHasher`, PBKDF2).
- **ישויות:** `support_requests` · `submitting_bodies` · `reference_domains/body_types/statuses/districts`
  · `filter_field_registry` · `tenants` · `users` · `saved_queries` · `audit_log`.
- **הרחבה בלי קוד:** הוספת תחום/סטטוס/מחוז = שורת נתונים ב-`reference_*`. הודגם
  מקצה-לקצה ב-[`docs/EXTENSIBILITY_DEMO.md`](docs/EXTENSIBILITY_DEMO.md).

---

## הנחות עבודה

- משתמש-מפתח יחיד, מטלת take-home. בונים רק מה שהתוכנית מפרטת; בספק — הגרסה הפשוטה.
- ה-`QueryDefinition` וחוזי `docs/contracts/` הוקפאו ב-S0 ומהווים מקור אמת; Swagger
  הוא החוזה החי שחייב להתאים להם.
- מנוע השאילתות מחזיר **אגרגציה** (`count` / `sumAmountApproved` לפי `segmentation`).
  "הצג את כלל הבקשות" מכוסה דרך הפילוחים + הטבלה + הגרף; תצוגת רשומות גולמית (`resultKind: "list"`)
  תוכננה כ-S7-b ולא מומשה (שינוי חוזה מוקפא).
- ה-NL parser עברי הוא מבוסס-חוקים בכוונה — אין ספריית NLP עברית בת-קיימא ל-.NET 8
  (`ARCHITECTURE.md` §10 החלטה 12); מילה שלא ממופה נכנסת ל-`unresolved`, לא מנוחשת.
- זהות דרך `X-User` — כותרת חסרה/לא מוכרת נופלת חזרה למשתמש seed. `tenantId` בגוף
  הבקשה מאומת מול הזהות, לא נאמן (אי-התאמה ⇒ 403).

---

## מגבלות

מה שלא מומש, ולמה:

| נושא | מצב | סיבה |
|---|---|---|
| אימות אמיתי (JWT / IdP / `/api/auth/login`) | לא מומש | יעד production; ה-PoC משתמש בתפר `X-User`. אין נתיב שמחזיר `401`. |
| תצוגת רשומות גולמית (`resultKind: "list"`) | לא מומש | S7-b — שינוי חוזה מוקפא; האגרגציה מכסה את הצורך. |
| `docs/DEVOPS.md`, CI (`.github/workflows/`), דיאגרמות pipeline/sequence | לא מומש | שלב S10 נפרד. |
| `docker compose` — `healthcheck` על `db` + `restart` policy | חסר | הרצה קרה ראשונה עלולה להיות racy; `up` שני פותר. |
| Client ב-Docker | Vite dev server, לא build סטטי מאחורי שרת | קיצור דרך מכוון ל-PoC (`client/Dockerfile`). |
| `IMemoryCache` dedup | per-instance | PoC single-node (`DESIGN_QA.md` §5). |
| כתיבות audit | `SaveChanges` נפרד לכל אירוע, לא טרנזקציוני | PoC (`DESIGN_QA.md` §7). |
| `appsettings.Development.json` | מכיל סיסמת SA של קונטיינר מקומי חד-פעמי (זהה ל-`.env.example`) | כדי ש-`dotnet run` יעבוד מ-clone נקי. אין secret production במאגר. |
| בדיקות | SQLite (endpoint/infra), לא SQL Server; `EnsureCreated()` בטסטים ולא שרשרת המיגרציות | דטרמיניזם ומהירות; שרשרת המיגרציות מורצת ב-`dotnet run` בפועל. |

---

## בדיקות

```bash
cd server && dotnet test SupportPlatform.sln     # 153 בדיקות
cd client && npm test                             # 49 בדיקות (vitest)
cd client && npm run lint                         # oxlint
```

Unit על מנוע השאילתות (כולל דחיית שדה זר), אגרגציה, `QuestionTextRenderer` (משפט
הדוגמה מהמטלה), ה-parser, ויציבות `definitionHash`; ומסלול happy-path אחד מקצה-לקצה
(`HappyPathIntegrationTests`) מעל `WebApplicationFactory` + SQLite. בדיקות ידניות
וקצוות: [`docs/TEST_PLAN.md`](docs/TEST_PLAN.md).

---

## מסמכים

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — שכבות, חלוקת אחריות, הרחבה עתידית, Decision Log, דיאגרמות
- [`docs/DESIGN_QA.md`](docs/DESIGN_QA.md) — 8 שאלות התכנון (הרחבה בלי קוד, multi-tenant, RBAC, שאילתות כבדות, dedup, ריבוי ספקי AI, ניטור, תשתיות)
- [`docs/TEST_PLAN.md`](docs/TEST_PLAN.md) — תוכנית בדיקות ידנית + מיפוי קצוות לטסטים
- [`docs/REVIEW_NOTES.md`](docs/REVIEW_NOTES.md) — סבב הביקורת (S11): security, קוד מת, lint
- [`docs/EXTENSIBILITY_DEMO.md`](docs/EXTENSIBILITY_DEMO.md) — הדגמת הוספת תחום בלי קוד
- [`docs/contracts/`](docs/contracts/) — `query-definition` (+schema), `api-contract`, `metadata-model`, `error-model` (מוקפאים)
- `docs/DEVOPS.md` — DEV/TEST/PROD, CI/CD, secrets, deployment (**מתוכנן — S10**)
