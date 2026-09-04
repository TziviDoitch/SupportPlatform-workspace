# SupportPlatform — מערכת תמיכות רוחבית (PoC)

Proof of Concept למערכת חוצת-ארגונים לאחסון וחיפוש בקשות תמיכה ממשלתיות.
מטלת Take‑Home — הדגמת חשיבה מערכתית, ארכיטקטורה, תכנון תשתיות ועבודה Full‑Stack.
.NET 8 Web API + React/TypeScript, הרצה ב-Docker Compose.

התוכנית המלאה (החלטות נעולות, מבנה עבודה, שלבי S0–S11): [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md).

> **מחפשים דרישה מסוימת מהמטלה?** [כיסוי דרישות המטלה](#כיסוי-דרישות-המטלה) ממפה כל
> דרישה לסטטוס שלה ולהוכחה בקוד או במסמך.

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
| CI/CD, Deployment אוטומטי, IaC (`.github/workflows/` וכו') | לא מומש — מתואר בלבד | המטלה קובעת לגבי DevOps "אין צורך לממש בפועל". התכנון המלא ב-[`docs/DEVOPS.md`](docs/DEVOPS.md); מגבלות ההיקף מפורטות שם §6. |
| `docker compose` — `healthcheck` על `db` + `restart` policy | חסר | הרצה קרה ראשונה עלולה להיות racy; `up` שני פותר. |
| Client ב-Docker | Vite dev server, לא build סטטי מאחורי שרת | קיצור דרך מכוון ל-PoC (`client/Dockerfile`). |
| `IMemoryCache` dedup | per-instance | PoC single-node (`DESIGN_QA.md` §5). |
| כתיבות audit | `SaveChanges` נפרד לכל אירוע, לא טרנזקציוני | PoC (`DESIGN_QA.md` §7). |
| `appsettings.Development.json` | מכיל סיסמת SA של קונטיינר מקומי חד-פעמי (זהה ל-`.env.example`) | כדי ש-`dotnet run` יעבוד מ-clone נקי. אין secret production במאגר. |
| בדיקות | SQLite (endpoint/infra), לא SQL Server; `EnsureCreated()` בטסטים ולא שרשרת המיגרציות | דטרמיניזם ומהירות; שרשרת המיגרציות מורצת ב-`dotnet run` בפועל. |

---

## בדיקות

```bash
cd server && dotnet test SupportPlatform.sln     # 158 בדיקות
cd client && npm test                             # 56 בדיקות (vitest)
cd client && npm run lint                         # oxlint
```

Unit על מנוע השאילתות (כולל דחיית שדה זר), אגרגציה, `QuestionTextRenderer` (משפט
הדוגמה מהמטלה), ה-parser, ויציבות `definitionHash`; ומסלול happy-path אחד מקצה-לקצה
(`HappyPathIntegrationTests`) מעל `WebApplicationFactory` + SQLite. בדיקות ידניות
וקצוות: [`docs/TEST_PLAN.md`](docs/TEST_PLAN.md).

---

## כיסוי דרישות המטלה

מיפוי מדרישות המטלה עצמן אל ההוכחה בקוד או במסמך. הטבלאות מסודרות לפי סעיפי
המטלה, ולא לפי מה שנבנה.

**מקרא סטטוס:** `מומש` = קיים ורץ בקוד · `מומש חלקית` = חלק מהדרישה בלבד ·
`מתואר` = תיעוד/תכנון ללא מימוש (כפי שהמטלה מתירה) · `לא מומש` = לא נבנה, עם סיבה.

### 1. דרישות פונקציונליות

| דרישה | סטטוס | היכן |
|---|---|---|
| סינון לפי **גוף מגיש** (עמותה / רשות מקומית / אגודה / מוסד תרבות) | מומש | שדה `bodyType` ב-`filter_field_registry`; ערכים ב-`reference_body_types` |
| סינון לפי **תחום תמיכה** (תרבות / ספורט / מוזיאונים / ספריות / אירועים) | מומש | שדה `supportDomain`; ערכים ב-`reference_domains` |
| סינון לפי **סטטוס בקשה** (הוגשה / בבדיקה / אושרה / נדחתה) | מומש | שדה `status`; ערכים ב-`reference_statuses` |
| סינון לפי **שנת תמיכה — שנה בודדת** | מומש | `supportYear`, `FilterValue.YearSingle` |
| סינון לפי **שנת תמיכה — טווח שנים** | מומש | `supportYear`, `FilterValue.YearRange` |
| **פילוחים** — מחוז · סוג גוף · שנת תמיכה · תחום תמיכה | מומש (4 מתוך 4) | `Segmentable = true` ב-`DbSeeder`; אגרגציה ב-`SearchQueryExecutor` |
| **ניסוח שאלה קריאה** מהפרמטרים | מומש | `Application/Search/QuestionTextRenderer.cs`. תבנית עברית קנונית ("כמה בקשות תמיכה עם … בפילוח לפי …?") המכסה את כל פרמטרי משפט הדוגמה שבמטלה — לא ציטוט מילולי שלו. נעול בטסט `QuestionTextRendererTests` |
| **הצגת טבלה** | מומש | `client/src/features/results/ResultsTable/` |
| **הצגת גרף בסיסי** | מומש | `client/src/features/results/ResultsChart/` (Chart.js), מתחלף לפי הפילוח |
| **שמירת שאילתה** | מומש | `POST /api/saved-queries` · `SaveQueryButton` |
| **עדכון שאילתה** | מומש | `PUT /api/saved-queries/{id}` מעדכן שם **ו-**`definition`. בלקוח נחשף שינוי שם (`RenameQueryModal`); עדכון ה-`definition` נעשה דרך שמירה מחדש מהחיפוש |
| **מחיקת שאילתה** | מומש | `DELETE /api/saved-queries/{id}` — דורש role `admin` |
| **הרצה מחדש של שאילתה** | מומש | `POST /api/saved-queries/{id}/run` → מציג `ResultsSection` מלא |
| **תשאול בשפה חופשית — פירוש השאלה** | מומש | `RuleBasedNlQueryProvider` (מבוסס חוקים, דטרמיניסטי) |
| **המרה למבנה השאילתה** | מומש | הפלט הוא `QueryDefinition` — אותו אובייקט שהטופס בונה |
| **הצגת הפרשנות** | מומש | `InterpretationPanel` + `interpretationText` + `unresolved[]` |
| **אפשרות להריץ** | מומש | כפתור "הרץ" ב-`NlQueryPage` → `POST /api/search` |
| **החלפה פשוטה בין ספקי AI בעתיד** | מומש | `INlQueryProvider` + keyed DI, נבחר ב-`NlQuery:Provider`. ראו [`DESIGN_QA.md`](docs/DESIGN_QA.md) §6 |
| הצגת **רשימת בקשות גולמית** (`resultKind: "list"`) | לא מומש | תוכנן כ-S7-b; דורש שינוי חוזה מוקפא. הפרשנות האגרגטיבית מכוסה דרך הפילוחים + הטבלה + הגרף. ראו [מגבלות](#מגבלות) |

### 2. ארכיטקטורה

| דרישה | סטטוס | היכן |
|---|---|---|
| מבנה המערכת | מומש + מתועד | [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) §1 |
| חלוקת אחריות | מומש + מתועד | [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) §2, §4.6 |
| שכבות | מומש + מתועד | [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) §2 — תלות חד-כיוונית, `Application` לא מכיר EF |
| מודולריות | מומש + מתועד | [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) §3 — מודולים אנכיים |
| הרחבה עתידית — תחומי תמיכה חדשים | מומש + מודגם | [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) §7 + [`EXTENSIBILITY_DEMO.md`](docs/EXTENSIBILITY_DEMO.md) — הוספת תחום = שורת נתונים, אפס קוד |
| הרחבה עתידית — מקורות מידע חדשים | מתואר | [`DESIGN_QA.md`](docs/DESIGN_QA.md) §1–§2 |
| הרחבה למשרדי ממשלה נוספים (multi-tenant) | מומש | המערכת רב-דיירית מלאה, לא חד-משרדית: `TenantId` על `SupportRequest` · `SubmittingBody` · `SavedQuery` · `User`; בידוד **fail-closed** ב-Global Query Filter (`SupportPlatformDbContext` — בלי tenant מוגדר מוחזרות אפס שורות); `TenantAccessGuard` ⇒ 403 על tenant זר; **שני משרדים ב-seed** (`culture-sport-admin`, `welfare-admin`) עם משתמשים ונתונים בכל אחד. מכוסה בטסטים חוצי-tenant (`TenantQueryFilterTests`, `TenantAccessGuardTests`, וטסטי endpoint). הוספת משרד = שורה ב-`tenants` + משתמשים — [`DESIGN_QA.md`](docs/DESIGN_QA.md) §2 |
| metadata ורשימות ייחוס **פר-tenant** | לא מומש — החלטה מודעת | `filter_field_registry` ו-`reference_*` הן גלובליות (בלי `TenantId`), ולכן כל ה-tenants חולקים את אותם שדות סינון ואותן רשימות ערכים. **אינו פוגע בבידוד הנתונים** (הוא מלא — ראו השורה הקודמת). נבחן ונדחה ל-PoC: `Code` הוא ה-PK של טבלאות הייחוס, ולכן בידוד אמיתי מחייב PK מורכב `(TenantId, Code)` ובעקבותיו **ארבעה FK מורכבים** מ-`support_requests` ומ-`submitting_bodies`; `TenantId` היה משתתף אז בשני FK במקביל — אותו תרחיש multiple-cascade-paths שכבר חייב את המיגרציה `TenantAndReferenceFkDeleteBehavior` — והמיגרציה היתה הרסנית (החלפת PK), בניגוד לכלל ה-additive-only ([`DEVOPS.md`](docs/DEVOPS.md) §5.4). זהו שינוי מודל, לא הוספת עמודה — [`DESIGN_QA.md`](docs/DESIGN_QA.md) §2 |
| החלטות הנדסיות ונימוקן | מתועד | [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) §10 — **Decision Log**, 14 החלטות עם חלופות ומחיר |

### 3. צד שרת

| דרישה | סטטוס | היכן |
|---|---|---|
| מבנה Solution | מומש | `server/SupportPlatform.sln` — 4 פרויקטי `src` + 3 פרויקטי `tests` |
| שכבות | מומש | `Api` / `Application` / `Domain` / `Infrastructure` |
| Services | מומש | `SearchService` · `MetadataService` · `SavedQueryService` · `NlQueryService` |
| Repository Pattern | מומש | `Infrastructure/Repositories/` — `IRepository<T>` לקריאה + `ISupportRequestRepository.Query()` + repositories ייעודיים |
| טיפול בשגיאות | מומש | `IExceptionHandler` גלובלי + RFC 7807 ProblemDetails + `ProblemTypes`; [`error-model.md`](docs/contracts/error-model.md) |
| Logging | מומש | Serilog (Console) + `CorrelationIdMiddleware` — `X-Correlation-Id` בכל תשובה ובכל שורת לוג |
| Validation | מומש | FluentValidation על `QueryDefinition`, מול ה-whitelist שב-`filter_field_registry` |

### 4. בסיס נתונים

| דרישה | סטטוס | היכן |
|---|---|---|
| מבנה נתונים | מומש | [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) §5.1 + דיאגרמת ERD ב-§9.1 |
| `Saved Queries` | מומש | טבלת `saved_queries` — `definition`, `definition_hash`, `last_run_at`, `last_run_row_count` |
| `Audit Log` | מומש | טבלת `audit_log` — user · action · entity · `correlation_id` · payload. `IAuditService` בקריאה מפורשת מה-service |
| נימוק בחירת ה-DB | מתועד | [בחירות טכנולוגיות](#בחירות-טכנולוגיות-ונימוקים) + [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) §10 החלטה 1 — SQL Server למימוש, PostgreSQL כיעד קוד-פתוח |
| עדיפות לקוד פתוח / ענן | מתואר | המודל provider-agnostic; מעבר ל-PostgreSQL = החלפת provider + connection string |
| קובץ מסד הנתונים | מומש (חלופה) | לא מצורף קובץ DB; הסכימה נבנית ממיגרציות EF והנתונים מ-`DbSeeder` דטרמיניסטי — ראו [מסד נתונים](#מסד-נתונים--מיגרציות-ו-seed) |

### 5. Client

| דרישה | סטטוס | היכן |
|---|---|---|
| מבנה הפרויקט | מומש | `client/src/` — `api` · `components` · `features` · `hooks` · `lib` · `models` · `state` |
| Components | מומש | משותפים ב-`components/` (`DataTable`, `BarChart`, `SectionTitle`, `PageLoader`); ייעודיים תחת ה-feature |
| Services | מומש | `api/` — `http` seam + interceptor ל-ProblemDetails + client לכל endpoint |
| State Management | מומש | TanStack Query (`state/queryClient.ts`) + hook ייעודי לכל feature; בלי Redux |
| UX בסיסי | מומש | Ant Design v6, RTL + `he_IL`, מצבי loading/empty/error אחידים, `ErrorBoundary` |
| טופס דינמי מ-metadata | מומש | `SearchForm` נבנה כולו מ-`GET /api/metadata` — אף שדה, תווית או ערך אינם מקודדים קשיח |

### 6. DevOps

המטלה קובעת: *"אין צורך לממש בפועל."* לכן כל השורות בסעיף זה הן **מתואר** —
ראו [`docs/DEVOPS.md`](docs/DEVOPS.md).

| דרישה | סטטוס | היכן |
|---|---|---|
| DEV / TEST / PROD | מתואר | [`DEVOPS.md`](docs/DEVOPS.md) §1 — טבלת סביבות + הפרדה מפורשת בין קיים למתוכנן |
| CI/CD | מתואר | [`DEVOPS.md`](docs/DEVOPS.md) §2 — שלבי הפייפליין, gates, דיאגרמת תכנון. **אין `.github/workflows/` ב-repo** |
| ניהול Secrets | מתואר | [`DEVOPS.md`](docs/DEVOPS.md) §3 — מה קיים (`.env` + `.gitignore`) מול היעד (secret store / Managed Identity) |
| ניהול קונפיגורציה | מתואר (עם בסיס ממומש) | [`DEVOPS.md`](docs/DEVOPS.md) §4 — היררכיית `appsettings` → env vars **קיימת ופעילה**; בחירת ספק AI ב-`NlQuery:Provider` היא הדגמה חיה |
| אסטרטגיית Deployment | מתואר | [`DEVOPS.md`](docs/DEVOPS.md) §5 — Blue/Green, health checks, מיגרציות כשלב נפרד, rollback |

### 7. שאלות התכנון (8)

כולן נענו ב-[`docs/DESIGN_QA.md`](docs/DESIGN_QA.md), עם הפניה לקוד בכל תשובה.

| # | שאלה | תשובה |
|---|---|---|
| 1 | הוספת סוגי תמיכה חדשים בלי שינוי קוד | [`DESIGN_QA.md`](docs/DESIGN_QA.md) §1 — מודגם מקצה-לקצה ב-[`EXTENSIBILITY_DEMO.md`](docs/EXTENSIBILITY_DEMO.md) |
| 2 | הוספת משרד ממשלתי נוסף | [`DESIGN_QA.md`](docs/DESIGN_QA.md) §2 |
| 3 | ניהול הרשאות בין גופים | [`DESIGN_QA.md`](docs/DESIGN_QA.md) §3 |
| 4 | התמודדות עם שאילתות כבדות | [`DESIGN_QA.md`](docs/DESIGN_QA.md) §4 |
| 5 | מניעת הרצה חוזרת של אותה שאילתה | [`DESIGN_QA.md`](docs/DESIGN_QA.md) §5 |
| 6 | תמיכה במספר ספקי AI | [`DESIGN_QA.md`](docs/DESIGN_QA.md) §6 |
| 7 | ניטור וחקירת תקלות | [`DESIGN_QA.md`](docs/DESIGN_QA.md) §7 |
| 8 | תשתיות רוחביות לשלב ראשון | [`DESIGN_QA.md`](docs/DESIGN_QA.md) §8 |

### 8. אופן ההגשה

| דרישה | סטטוס | היכן |
|---|---|---|
| קוד צד שרת | מומש | `server/` |
| קוד צד קלינט | מומש | `client/` |
| README — הוראות הרצה | מומש | [הרצה](#הרצה) |
| README — הסבר ארכיטקטוני | מומש | [ארכיטקטורה — תקציר](#ארכיטקטורה--תקציר) + [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) |
| README — בחירות טכנולוגיות | מומש | [בחירות טכנולוגיות](#בחירות-טכנולוגיות-ונימוקים) |
| README — מבנה הפרויקט | מומש | [מבנה הפרויקט](#מבנה-הפרויקט) |
| README — הנחות עבודה | מומש | [הנחות עבודה](#הנחות-עבודה) |
| README — מגבלות | מומש | [מגבלות](#מגבלות) |
| קובץ מסד הנתונים במידת הצורך | מומש (חלופה) | מיגרציות EF + `DbSeeder` במקום קובץ DB — ראו §4 לעיל |
| תשובות לשאלות התכנון | מומש | [`docs/DESIGN_QA.md`](docs/DESIGN_QA.md) |

---

## מסמכים

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — שכבות, חלוקת אחריות, הרחבה עתידית, Decision Log, דיאגרמות
- [`docs/DESIGN_QA.md`](docs/DESIGN_QA.md) — 8 שאלות התכנון (הרחבה בלי קוד, multi-tenant, RBAC, שאילתות כבדות, dedup, ריבוי ספקי AI, ניטור, תשתיות)
- [`docs/TEST_PLAN.md`](docs/TEST_PLAN.md) — תוכנית בדיקות ידנית + מיפוי קצוות לטסטים
- [`docs/REVIEW_NOTES.md`](docs/REVIEW_NOTES.md) — סבב הביקורת (S11): security, קוד מת, lint
- [`docs/EXTENSIBILITY_DEMO.md`](docs/EXTENSIBILITY_DEMO.md) — הדגמת הוספת תחום בלי קוד
- [`docs/contracts/`](docs/contracts/) — `query-definition` (+schema), `api-contract`, `metadata-model`, `error-model` (מוקפאים)
- [`docs/OVERVIEW_EN.md`](docs/OVERVIEW_EN.md) — **English overview** — תקציר לקורא שאינו דובר עברית (תקציר בלבד; התיעוד העברי הוא מקור האמת)
- [`docs/DEVOPS.md`](docs/DEVOPS.md) — DEV/TEST/PROD, CI/CD, ניהול Secrets, ניהול קונפיגורציה, אסטרטגיית Deployment (**תיאור ותכנון בלבד — המטלה אינה דורשת מימוש**)
