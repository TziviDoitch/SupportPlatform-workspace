# SupportPlatform — מערכת תמיכות רוחבית (PoC)

Proof of Concept למערכת חוצת-ארגונים לאחסון וחיפוש בקשות תמיכה ממשלתיות.
מטלת Take-Home — הדגמת חשיבה מערכתית, ארכיטקטורה, תכנון תשתיות ועבודה Full-Stack.
.NET 8 Web API + React/TypeScript. הרצה מקומית ב-Windows: `run-local.ps1` (בלי Docker).

התוכנית המלאה (החלטות נעולות, מבנה עבודה, שלבי S0–S11): [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md).
מיפוי דרישה-דרישה להוכחה: [כיסוי דרישות המטלה](#כיסוי-דרישות-המטלה) בהמשך.

---

## הרצה

על Windows, `run-local.ps1` מרים את הכול — LocalDB · API · client — בפקודה אחת, בלי
Docker. זו הדרך שנבדקה בסביבה זו.

### דרוש שיהיה מותקן

| רכיב | בדיקה מהירה | הערה |
|---|---|---|
| Windows + PowerShell | — | מובנה |
| .NET 8 SDK | `dotnet --version` → `8.*` | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Node.js 20+ | `node --version` | — |
| SQL Server **LocalDB** | `sqllocaldb info` → מציג `MSSQLLocalDB` | מגיע עם Visual Studio, או מתקין נפרד "SQL Server Express LocalDB" |

### הרצה

```powershell
cd path\to\SupportPlatform     # שורש הריפו — התיקייה שמכילה את run-local.ps1
.\run-local.ps1
```

אם PowerShell חוסם סקריפטים: `powershell -ExecutionPolicy Bypass -File .\run-local.ps1`

הרצה ראשונה מריצה `npm install` + `dotnet build` (~1–2 דק'). אחר כך הסקריפט מריץ
מיגרציות + seed ומרים:

| רכיב | כתובת |
|---|---|
| API | http://localhost:5080 · `/health` · Swagger ב-`/swagger` |
| client | http://localhost:5173 (קריאות `/api/*` עוברות proxy ל-API) |

פורט תפוס: `.\run-local.ps1 -ApiPort <n> -ClientPort <n>`.

### עצירה

`Ctrl+C` בטרמינל — עוצר API + client יחד. אל תסגרו את החלון בכוח: זה מדלג על הניקוי
ומשאיר את LocalDB במצב שדורש איפוס (ראו [פתרון תקלות](#פתרון-תקלות)). LocalDB ממשיך
לרוץ ברקע ונעצר לבד; לעצירה מלאה: `sqllocaldb stop MSSQLLocalDB`.

### לאן ה-API מתחבר (connection string)

ה-API קורא את מחרוזת החיבור בשם `SqlServer` דרך `IConfiguration`
(`Infrastructure/DependencyInjection.cs` → `UseSqlServer(config.GetConnectionString("SqlServer"))`).
`run-local.ps1` מזריק אותה כ-env var `ConnectionStrings__SqlServer` =
`Server=(localdb)\MSSQLLocalDB;Database=SupportPlatform;Trusted_Connection=True`.

`appsettings.json` מחזיק `""` (placeholder); `appsettings.Development.json` מחזיק ערך
נפילה מול SQL Server על `localhost:1433`. `DesignTimeDbContextFactory.cs` מכיל מחרוזת
נוספת — לשימוש `dotnet ef` בלבד בזמן פיתוח, לא בזמן ריצה. המיגרציות + seed רצים
מ-`Program.cs` (`db.Database.Migrate()` + `DbSeeder.Seed` ב-Development).

### פתרון תקלות

- **API נופל על `CREATE DATABASE … SupportPlatform.mdf already exists` (שגיאה 5170 / 1801):**
  שרידי DB מהרצה קודמת שנקטעה בכוח (החלון נסגר במקום `Ctrl+C`). `run-local.ps1` מזהה
  ומנקה את זה אוטומטית ב-preflight; אם בכל זאת קרה, אפסו ידנית:
  ```powershell
  sqllocaldb stop MSSQLLocalDB
  Remove-Item "$env:USERPROFILE\SupportPlatform.mdf","$env:USERPROFILE\SupportPlatform_log.ldf" -Force
  .\run-local.ps1
  ```
- **`sqllocaldb info` מראה `Stopped` אבל חיבורים נכשלים / `SQL Server process failed to start`:**
  ה-instance תקוע אחרי הרג כוחני. `sqllocaldb stop MSSQLLocalDB -k` ואז `sqllocaldb start MSSQLLocalDB`.
- **`SQL Server LocalDB not found`:** התקינו "SQL Server Express LocalDB" (מגיע עם
  Visual Studio, או כמתקין נפרד).
- **פורט 5080 / 5173 תפוס:** `.\run-local.ps1 -ApiPort <n> -ClientPort <n>`.

### משתמשי seed

אין מסך התחברות ב-PoC. הזהות היא כותרת `X-User` (ברירת מחדל `sarah`); הלקוח שולח
אותה מ-`client/src/api/config.ts`. להחלפת משתמש — **בורר המשתמש בהדר** (הבחירה נשמרת
ב-`localStorage`), או שליחת הכותרת ידנית (Swagger / `server/src/Api/SupportPlatform.Api.http`).

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

**הליבה — `QueryDefinition` הוא אובייקט קנוני יחיד.** הטופס בונה אותו, ה-NL parser
מפיק אותו, השאילתה השמורה *היא* הוא, מנוע ה-SQL מתרגם אותו, מנסח השאלה קורא אותו —
מבנה אחד, חמישה צרכנים. `DynamicQueryBuilder` מרכיב `IQueryable` דרך whitelist
מ-`filter_field_registry` בלבד — שדה שלא ברשימה נדחה לפני שרץ handler, בלי `switch`,
בלי reflection, בלי ביטויים מחרוזתיים.

**מודולים אנכיים:** Metadata · Search · SavedQueries · NlQuery · Audit · Identity(stub).

**Client:** `api/` (http seam + interceptor), `hooks/` + `lib/` משותפים, `features/`
(search / results / saved-queries / nl-query), `state/` (TanStack Query client).
הטופס נבנה דינמית מ-`GET /api/metadata` — אף שדה/תווית/רשימת ערכים לא מקודדים קשיח.

### נקודות קצה

| Method | Path | תיאור |
|---|---|---|
| `GET` | `/api/metadata?tenantId=` | רשימות ייחוס + `filterFieldRegistry` (מזין את הטופס) |
| `POST` | `/api/search` | הרצת `QueryDefinition` → `questionText` / `rows` / `aggregations` / `page` / `executionMeta` |
| `GET/POST/PUT/DELETE` | `/api/saved-queries[/{id}]` | CRUD, scoped ל-owner+tenant; DELETE של שאילתת משתמש אחר דורש role `admin` |
| `POST` | `/api/saved-queries/{id}/run` | הרצה חוזרת; תגובה כמו `/search` |
| `POST` | `/api/nl-queries/parse` | טקסט חופשי → `{ definition, interpretationText, confidence, unresolved }` |
| `GET` | `/health` | `200 Healthy` |

כל בקשה מחזירה `X-Correlation-Id`; כל שגיאה היא `application/problem+json` (RFC 7807) —
ראו [`docs/contracts/error-model.md`](docs/contracts/error-model.md).

### החלטות מפתח

תמצית ההחלטות ההנדסיות הגדולות. כל אחת עם חלופה ומחיר ב-[`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) §10 (Decision Log — 12 החלטות).

- **`QueryDefinition` אובייקט קנוני יחיד** — הטופס בונה, ה-NL parser מפיק, השאילתה השמורה *היא* הוא, מנוע ה-SQL מתרגם, מנסח השאלה קורא. מבנה אחד, חמישה צרכנים, אפס תרגום כפול. (§10.3)
- **4 שכבות, תלות חד-כיוונית, `Application` לא מכיר EF** — הלוגיקה ומנוע השאילתות מבודדים מ-framework; הפרדת אחריות שנבדקת בטסטים. (§10.2)
- **`DynamicQueryBuilder` דרך whitelist מ-`filter_field_registry` בלבד** — בלי `switch`, reflection או ביטויים מחרוזתיים; שדה זר נדחה לפני הרצה. גם בטיחות injection וגם הרחבה בלי קוד. (§10.4)
- **בידוד multi-tenant fail-closed** — Global Query Filter של EF: בלי tenant context מוגדר מוחזרות אפס שורות, לא "הכל". `TenantAccessGuard` דוחה tenant זר ב-403. (§10.5)
- **SQL Server ל-PoC, PostgreSQL כיעד פרודקשן** — בחירה פרגמטית (היכרות/רישוי) מול הנכון לטווח ארוך; המעבר זול בכוונה — כל הגישה דרך EF `IQueryable`, בלי SQL גולמי. (§10.1)
- **זהות דרך כותרת `X-User` (auth stub), לא JWT** — קיצור PoC מוצהר; האכיפה כבר ב-service (`TenantAccessGuard` + כלל role אחד), היעד OIDC/JWT מול IdP מרכזי. (§10.9)
- **NL מבוסס-חוקים דטרמיניסטי, נבחר ב-configuration** — אין ספריית NLP עברית בת-קיימא ל-.NET 8; מילה שלא מופתה נכנסת ל-`unresolved`, לא מנוחשת. ספק LLM עתידי = אותו `INlQueryProvider`, שינוי קונפיג בלבד. (§10.10, §10.11)
- **חתכים רוחביים נכנסים עם הצרכן הראשון** — logging, correlation id, error model ו-cache נכנסו ב-S2 יחד עם `/search`, לא כתשתית ספקולטיבית מוקדמת. (§10.8)

---

## בחירות טכנולוגיות ונימוקים

| תחום | בחירה | נימוק ענייני |
|---|---|---|
| Backend | .NET 8 Web API (C#) | טיפוסיות חזקה + EF בשל להרכבת שאילתה דינמית בטוחה; מתאים היטב לאוצר המילים של המטלה (Solution / Repository / Services); אזרח סוג א' ב-Azure — יעד הענן הטבעי כאן. חלופות שקולות: Java/Spring, Node/NestJS |
| שכבת נתונים | EF Core 8 | `IQueryable` דינמי דרך whitelist, בלי SQL גולמי — גם בטיחות injection וגם אי-תלות ב-provider |
| Client | React + TypeScript + Vite | מאגר כוח-אדם גדול, אקוסיסטם רחב, DX מהיר. חלופה שקולה: Angular (אופייני יותר ל-enterprise/ממשל) |
| ספריית עיצוב | Ant Design v6 (RTL, `he_IL`) | רכיבים בשלים + תמיכת RTL מובנית — קריטי ל-UI ממשלתי בעברית |
| State | TanStack Query + hooks מקומיים | caching / dedup / retry ל-server-state מהקופסה; חוסך boilerplate של Redux למה שהוא בעיקר נתוני שרת |
| גרפים | Chart.js (`react-chartjs-2`) | גרף עמודות בסיסי — הדרישה מינימלית, בלי להביא ספריה כבדה |
| הפשטת AI | `INlQueryProvider` + בחירה לפי configuration | הגבול הוא הדרישה; המימוש (`ruleBased`) מוחלף בקונפיג — נכון בכל קנה מידה |
| Logging | Serilog + Correlation Id | structured logging הוא הבסיס הנכון; Console sink עכשיו, sink מנוהל (App Insights / Seq / ELK) בהמשך — בלי שינוי קוד |
| Validation | FluentValidation על `QueryDefinition` | ולידציה מוצהרת, נבדקת, מופרדת מה-controller |

### מסד הנתונים — מה מומש מול מה נכון

**מומש: SQL Server.** בחירה **פרגמטית ל-PoC** — היכרות ורישוי קיימים קיצרו את זמן
ההקמה, שהוא המשאב הקריטי במטלה מתוחמת-זמן. זו לא ההמלצה ארוכת-הטווח.

**הנכון לפרודקשן: PostgreSQL.**

- **רישוי** — קוד פתוח, אפס עלות רישוי. בקנה מידה ממשלתי (הרבה סביבות, הרבה
  instances) עלות הרישוי של SQL Server משמעותית.
- **ענן מנוהל** — מוצר מנוהל מדרגה ראשונה בכל העננים: Azure Database for PostgreSQL
  (Flexible Server), AWS RDS/Aurora, GCP Cloud SQL. גיבויים, HA, patching ו-scaling
  מנוהלים — פחות תחזוקה שוטפת.
- **התאמה למודל** — `jsonb` נייטיב מתאים בול ל-`audit_log.payload` ולהגדרות
  metadata עתידיות; אקוסיסטם extensions עשיר.

**המעבר זול בכוונה:** כל הגישה דרך EF Core `IQueryable` בלבד — אין SQL גולמי ואין
פיצ'ר ספציפי ל-SQL Server. מעבר = החלפת חבילת ה-provider ל-`Npgsql`, עדכון
connection string, והרצת המיגרציות מחדש.

**מתי כן להישאר על SQL Server:** ארגון שהוא ממילא Microsoft-shop עם רישוי enterprise
קיים ו-Azure SQL כיעד — Azure SQL הוא מוצר מנוהל מצוין, וההיצמדות היחידה שיש להימנע
ממנה (פיצ'רים ספציפיים ל-provider) ממילא לא קיימת בקוד.

### תשתית ופריסה — עקרון מנחה

עדיפות ל-**שירותים מנוהלים** על פני self-hosted (DB מנוהל, container hosting מנוהל,
secret store מנוהל, observability מנוהל), ול-**רכיבי קוד-פתוח בלי רישוי** כדי להימנע
מ-lock-in בקנה מידה. ה-API כבר **חסר-מצב** (הזהות בכל בקשה, בלי session), ולכן
מתאים ל-scaling אופקי ול-Blue/Green. **Auth:** כותרת `X-User` היא קיצור PoC מוצהר;
היעד הוא OIDC/JWT מול IdP מרכזי (Azure AD / IdP ממשלתי). **Container:** `infra/docker-compose.yml`
מרים את הכול בפקודה אחת (חלופה ל-`run-local.ps1` למי שיש Docker Desktop); `db` נושא
`healthcheck` (`sqlcmd SELECT 1`) ו-`api` ממתין ל-`service_healthy`, כך שהרצה קרה
ראשונה לא מתחילה לפני ש-SQL Server מקבל חיבורים. בפרודקשן — container hosting מנוהל
ל-API, static hosting ל-Client, ו-DB מנוהל (לא Compose). פירוט: [`docs/DEVOPS.md`](docs/DEVOPS.md).

---

## מבנה הפרויקט

```
server/   פתרון .NET 8 — Api / Application / Domain / Infrastructure (+ tests/, פרויקט לכל שכבה)
client/   React + TypeScript (Vite) — api / hooks / lib / features / state / models / components
docs/     ARCHITECTURE.md · DESIGN_QA.md · DEVOPS.md · TEST_PLAN.md · EXTENSIBILITY_DEMO.md · contracts/
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
  (320 `culture-sport-admin` / 180 `welfare-admin`; שנים 30/40/30; סטטוס 15/20/45/20).
  רשימות הייחוס מכילות את הערכים שהמטלה מונה: 4 סוגי גוף, 5 תחומי תמיכה, 4 סטטוסים,
  3 מחוזות — נעול בטסט `DbSeederTests.Reference_lists_carry_the_values_the_assignment_enumerates`.
  סיסמאות seed נשמרות כ-hash בלבד (`SeedPasswordHasher`, PBKDF2).
- **ישויות:** `support_requests` · `submitting_bodies` · `reference_domains/body_types/statuses/districts`
  · `filter_field_registry` · `tenants` · `users` · `saved_queries` · `audit_log`.
- **הרחבה בלי קוד:** הוספת תחום/סטטוס/מחוז = שורת נתונים ב-`reference_*`. הודגם
  מקצה-לקצה ב-[`docs/EXTENSIBILITY_DEMO.md`](docs/EXTENSIBILITY_DEMO.md).

אין קובץ DB מצורף — הסכימה נבנית ממיגרציות EF והנתונים מ-`DbSeeder`, כך שכל clone נקי
מגיע לאותו מצב בדיוק.

---

## הנחות ומגבלות

**הנחות עבודה**

- משתמש-מפתח יחיד, מטלת take-home. בונים רק מה שהתוכנית מפרטת; בספק — הגרסה הפשוטה.
- ה-`QueryDefinition` וחוזי `docs/contracts/` הוקפאו ב-S0 ומהווים מקור אמת; Swagger
  הוא החוזה החי שחייב להתאים להם.
- זהות דרך `X-User` — כותרת חסרה/לא מוכרת נופלת חזרה למשתמש seed. `tenantId` בגוף
  הבקשה מאומת מול הזהות, לא נאמן (אי-התאמה ⇒ 403).
- ה-NL parser עברי מבוסס-חוקים בכוונה — אין ספריית NLP עברית בת-קיימא ל-.NET 8
  (`ARCHITECTURE.md` Decision Log); מילה שלא ממופה נכנסת ל-`unresolved`, לא מנוחשת.

**מה לא מומש, ולמה**

| נושא | מצב | סיבה |
|---|---|---|
| תצוגת רשומות גולמית (`resultKind: "list"`) | לא מומש | המנוע הוא **מנוע אגרגציה** (`count` / `sumAmountApproved` לפי `segmentation`). "הצג את כלל הבקשות" מכוסה דרך הפילוחים + הטבלה + הגרף; תצוגת שורות גולמית תוכננה כ-S7-b ודורשת שינוי חוזה מוקפא. |
| אימות אמיתי (JWT / IdP / `/api/auth/login`) | לא מומש | יעד production; ה-PoC משתמש בתפר `X-User`. אין נתיב שמחזיר `401`. |
| CI/CD, Deployment אוטומטי, IaC | מתואר בלבד | המטלה קובעת לגבי DevOps "אין צורך לממש בפועל". התכנון המלא ב-[`docs/DEVOPS.md`](docs/DEVOPS.md). אין `.github/workflows/`. |
| metadata ורשימות ייחוס פר-tenant | לא מומש — החלטה מודעת | `filter_field_registry` ו-`reference_*` גלובליות. בידוד **הנתונים** מלא ואינו נפגע; בידוד הייחוס מחייב PK מורכב + ארבעה FK מורכבים — שינוי מודל, לא הוספת עמודה. `DESIGN_QA.md` §2. |
| Client ב-Docker | Vite dev server, לא build סטטי | קיצור דרך מכוון ל-PoC (`client/Dockerfile`). |
| `IMemoryCache` dedup | per-instance | PoC single-node (`DESIGN_QA.md` §5). |
| כתיבות audit | `SaveChanges` נפרד לכל אירוע, לא טרנזקציוני | PoC (`DESIGN_QA.md` §7). |
| `appsettings.Development.json` | מכיל סיסמת SA של קונטיינר מקומי חד-פעמי (זהה ל-`.env.example`) | כדי ש-`dotnet run` יעבוד מ-clone נקי. אין secret production במאגר. |
| בדיקות | SQLite (endpoint/infra), לא SQL Server; `EnsureCreated()` בטסטים | דטרמיניזם ומהירות; שרשרת המיגרציות מורצת ב-`dotnet run` בפועל. |

---

## בדיקות

```bash
cd server && dotnet test SupportPlatform.sln     # 165 בדיקות
cd client && npm test                             # 56 בדיקות (vitest)
cd client && npm run lint                         # oxlint
```

Unit על מנוע השאילתות (כולל דחיית שדה זר), אגרגציה, `QuestionTextRenderer` (משפט
הדוגמה מהמטלה), ה-parser, ויציבות `definitionHash`; ומסלול happy-path אחד מקצה-לקצה
(`HappyPathIntegrationTests`) מעל `WebApplicationFactory` + SQLite. תרחישים ידניים
וקצוות: [`docs/TEST_PLAN.md`](docs/TEST_PLAN.md).

---

## כיסוי דרישות המטלה

שורה לכל סעיף במטלה. פירוט מלא בקוד ובמסמכים המקושרים.

**מקרא:** `מומש` = קיים ורץ · `מתואר` = תיעוד/תכנון ללא מימוש (כפי שהמטלה מתירה) ·
`לא מומש` = לא נבנה, עם סיבה.

| דרישה במטלה | סטטוס | היכן |
|---|---|---|
| חיפוש — גוף מגיש / תחום תמיכה / סטטוס / שנה בודדת / טווח שנים | מומש | `filter_field_registry` + `reference_*`; `FilterValue`. רשימות הייחוס נושאות את כל הערכים שהמטלה מונה (4 סוגי גוף, 5 תחומי תמיכה, 4 סטטוסים) — נעול בטסט `DbSeederTests` |
| פילוחים — מחוז · סוג גוף · שנת תמיכה · תחום תמיכה | מומש (4/4) | `Segmentable` ב-`DbSeeder`; אגרגציה ב-`SearchQueryExecutor` |
| ניסוח שאלה קריאה מהפרמטרים | מומש | `QuestionTextRenderer` — תבנית עברית קנונית ("כמה בקשות תמיכה עם … בפילוח לפי …?"), נעולה בטסט. הערה: תבנית **ספירה**; לניסוח "הצג את כלל הבקשות" ראו השורה האחרונה |
| הצגת נתונים — טבלה + גרף בסיסי | מומש | `features/results/ResultsTable/` + `ResultsChart/` (Chart.js), מתחלף לפי הפילוח |
| שמירת שאילתות — שמור / עדכן / מחק / הרץ מחדש | מומש | `/api/saved-queries` CRUD + `/{id}/run`. עדכון `definition` דרך שמירה מחדש; שינוי שם ב-`RenameQueryModal`. מחיקה: הבעלים מוחק את שלו ללא role נוסף; מחיקת שאילתה של משתמש אחר דורשת `admin` |
| תשאול בשפה חופשית — פירוש / המרה / הצגת פרשנות / הרצה | מומש | `RuleBasedNlQueryProvider` (דטרמיניסטי) → `QueryDefinition` → `InterpretationPanel` → כפתור "הרץ" |
| החלפה פשוטה בין ספקי AI | מומש | `INlQueryProvider` + keyed DI, נבחר ב-`NlQuery:Provider`. `DESIGN_QA.md` §6 |
| ארכיטקטורה — מבנה / חלוקת אחריות / שכבות / מודולריות / הרחבה | מומש + מתועד | [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) §1–§4, §7 |
| הרחבה — תחומי תמיכה חדשים בלי שינוי קוד | מומש + מודגם | [`EXTENSIBILITY_DEMO.md`](docs/EXTENSIBILITY_DEMO.md) — הוספת תחום = שורת נתונים |
| הרחבה — מקורות מידע חדשים | מתואר | [`DESIGN_QA.md`](docs/DESIGN_QA.md) §1 |
| הרחבה למשרדי ממשלה נוספים | מומש | multi-tenant מלא: `TenantId` + Global Query Filter **fail-closed** + `TenantAccessGuard` (403); שני משרדים ב-seed. `DESIGN_QA.md` §2 |
| צד שרת — Solution / שכבות / Services / Repository Pattern / שגיאות / Logging / Validation | מומש | 4 פרויקטי `src` + 3 `tests`; `IExceptionHandler` + RFC 7807; Serilog + Correlation Id; FluentValidation |
| בסיס נתונים — מבנה נתונים / Saved Queries / Audit Log | מומש | [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) §5 + ERD §9.1; טבלאות `saved_queries` · `audit_log` |
| נימוק בחירת ה-DB + עדיפות קוד-פתוח | מתועד | [בחירות טכנולוגיות](#בחירות-טכנולוגיות-ונימוקים) — SQL Server למימוש, PostgreSQL כיעד |
| קובץ מסד הנתונים | חלופה | מיגרציות EF + `DbSeeder` דטרמיניסטי במקום dump |
| Client — מבנה / Components / Services / State Management / UX בסיסי | מומש | `client/src/` — `api` · `components` · `features` · `hooks` · `lib` · `state`; TanStack Query; טופס דינמי מ-metadata |
| DevOps — DEV/TEST/PROD · CI/CD · Secrets · קונפיגורציה · Deployment | מתואר | [`DEVOPS.md`](docs/DEVOPS.md) — המטלה: "אין צורך לממש בפועל" |
| 8 שאלות התכנון ("אין צורך לממש") | נענו | [`DESIGN_QA.md`](docs/DESIGN_QA.md) — כל שאלה עם **תשובה קצרה** + מה קיים בקוד + יעד |
| אופן ההגשה — קוד שרת + קלינט, README (6 סעיפים), תשובות | מומש | `server/` · `client/` · מסמך זה · `DESIGN_QA.md` |
| הצגת רשימת בקשות גולמית (`resultKind: "list"`) | לא מומש | המנוע הוא מנוע אגרגציה; ראו [הנחות ומגבלות](#הנחות-ומגבלות) |

---

## פיתוח בליווי AI

הקוד נכתב תוך שימוש ב-Claude כ-**development agent בתוך workflow הנדסי מוגדר** — לא
כתחליף לשיקול דעת. ה-agent עבד בתוך הגבולות של [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md)
(החלטות טכנולוגיות נעולות §2, Working Agreement §3, סדר השלבים S0–S11 §6), החוזים
המוקפאים ב-[`docs/contracts/`](docs/contracts/), ו-Definition of Done לכל שלב.
ההחלטות הארכיטקטוניות, הביקורת והאימות הסופי נשארו אצל המפתח.

| נכס | מה הוא מגדיר |
|---|---|
| `CLAUDE.md` + `server/CLAUDE.md` + `client/CLAUDE.md` | גבולות ארכיטקטוניים, מבנה, מוסכמות ורשימת "אל תעשה" לכל צד |
| `.claude/commands/build-stage.md` | לולאת שלב יחיד: sanity-check → worktree → כרטיס משימה → **עצירה לאישור** → מימוש → שער DoD → PR |
| `.claude/skills/new-task` | כל משימה ב-git worktree מבודד, על branch מ-`origin/main` |

עקרונות שנאכפו: שלב אחד = PR אחד · חוזים מוקפאים · whitelist לשדות סינון · בלי שכבות
או תבניות מעבר לתוכנית · "אל תמציא דרישות". התשתית כלולה כדי שבודק יראה **כיצד** נבנה
הקוד, לא רק את התוצאה.

---

## מסמכים

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — שכבות, חלוקת אחריות, מנוע השאילתות, מודל הנתונים, הרחבה עתידית, Decision Log, דיאגרמות ERD/Container
- [`docs/DESIGN_QA.md`](docs/DESIGN_QA.md) — **המענה ל-8 שאלות התכנון מהמטלה** (סעיף "שאלות תכנון"): תשובה קצרה לכל שאלה, מה קיים בקוד, ומה היעד
- [`docs/DEVOPS.md`](docs/DEVOPS.md) — DEV/TEST/PROD, CI/CD, ניהול Secrets, ניהול קונפיגורציה, אסטרטגיית Deployment (**תיאור ותכנון בלבד — המטלה אינה דורשת מימוש**)
- [`docs/TEST_PLAN.md`](docs/TEST_PLAN.md) — תוכנית בדיקות ידנית + מיפוי קצוות לטסטים
- [`docs/EXTENSIBILITY_DEMO.md`](docs/EXTENSIBILITY_DEMO.md) — הדגמת הוספת תחום בלי קוד, מקצה-לקצה
- [`docs/contracts/`](docs/contracts/) — `query-definition` (+schema), `api-contract`, `metadata-model`, `error-model` (מוקפאים)
