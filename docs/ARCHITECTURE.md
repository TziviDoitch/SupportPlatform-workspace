# ארכיטקטורה — מערכת תמיכות רוחבית (PoC)

> **סטטוס: כותרות פרקים בלבד (S0-f).** התוכן נכתב ב-S4 ומתעדכן בכל שלב לפי
> ה-DoD (`IMPLEMENTATION_PLAN.md` §3.8). מקור לתוכן: §4 (ארכיטקטורת יעד) ו-§2
> (החלטות טכנולוגיות נעולות).

## 1. סקירה כללית

_מה המערכת עושה, ה-vertical slice המרכזי (`metadata → QueryDefinition → /search →
results`), ותרשים הקשרים ברמה גבוהה._

## 2. שכבות ה-Backend

_ארבע שכבות: `Api` · `Application` · `Domain` · `Infrastructure`. חלוקת אחריות
וכיוון תלות חד-כיווני (`Application` לא מכיר EF Core)._

### 2.1 Api
### 2.2 Application
### 2.3 Domain
### 2.4 Infrastructure

## 3. מודולים אנכיים

_Metadata · Search · SavedQueries · NlQuery · Audit · Identity (stub) — מה כל
מודול מכסה ואיפה הוא יושב בשכבות._

## 4. מנוע השאילתות

חוזה: [`contracts/query-definition.md`](contracts/query-definition.md) ·
[`contracts/api-contract.md`](contracts/api-contract.md) §3. מומש ב-S2.

### 4.1 `QueryDefinition` — האובייקט הקנוני

רשומה ב-`Application/Search/QueryDefinition.cs`: `TenantId` · `Filters`
(מילון `fieldId → FilterValue`) · `Segmentation` · `Metrics` · `Paging` · `Sort`.
`FilterValue` הוא היררכיה סגורה (`Codes` · `YearRange` · `YearSingle`) עם
`FilterValueJsonConverter` שקורא מערך JSON כרשימת codes ואובייקט `{type}` כפילטר שנה.

### 4.2 ולידציה מול ה-Registry

`QueryDefinitionValidator` (FluentValidation) נטען דרך `ISearchMetadataProvider`
(reference lists + registry + רשימת ה-tenants). נדחים ב-400 (`error-model.md`):
`filters` key / `segmentation` / `sort.field` שאינו ב-registry, ערך שצורתו לא
תואמת ל-`kind`, שדה לא-`segmentable` בפילוח, טווח שנים הפוך, metric/direction לא
מוכר, `pageSize` מחוץ ל-1–200, tenant לא קיים.

### 4.3 `DynamicQueryBuilder` + היררכיית ה-handlers (§3.4 קו אדום)

`DynamicQueryBuilder` (Infrastructure) הוא מתזמן דק: קודם **דוחה כל `fieldId`
שאינו ב-registry — לפני שרץ handler כלשהו** — ואז מקפל את ה-handlers שנפתרו.
אין `switch` לפי `fieldId`, אין reflection, אין expression-from-string.

- `FilterHandler` (abstract) — תת-מחלקה אחת לכל `kind`: `CodeListFilterHandler`
  (IN מעל עמודת code), `YearRangeFilterHandler` (טווח / שנה בודדת — pattern
  match סגור על צורת הערך). ה-guard בבסיס בודק את צורת הערך מול ה-`kind`.
- **instance אחד לכל שדה**, נושא את ה-selector החזק שלו
  (`Expression<Func<SupportRequest,string|int>>`) — הוא גם מפתח ה-filter וגם
  מפתח ה-segmentation (`GroupKeySelector`). הרישום ב-`FilterHandlers.Default`.
- `FilterHandlerResolver` ממפה `fieldId → handler` (מתוך `IEnumerable<FilterHandler>`
  ב-DI). שדה סינון חדש = שורת רישום אחת; `kind` חדש = תת-מחלקה אחת. ה-resolver
  וה-builder לא משתנים.

### 4.4 Aggregation + `SearchQueryExecutor`

`SearchQueryExecutor` (Infrastructure) מחיל את ה-tenant scope (מ-`QueryDefinition.TenantId`
שכבר עבר ולידציה), מריץ את ה-builder, ואז מפלח (count + sumAmountApproved תמיד
מחושבים; ה-service מקרין רק את ה-metrics שהתבקשו):

- **0 שדות פילוח** → aggregate בודד ב-DB.
- **שדה פילוח אחד** → `GroupBy` ב-DB.
- **2+ שדות** → materialization מינימלי + GroupBy בזיכרון (פשטת PoC; שאילתות
  כבדות = `DESIGN_QA` §4).

הסכומים נלקחים מעל `double` כדי שה-provider של SQLite בטסטים יתרגם את ה-aggregate;
SQL Server היה שומר `decimal` נייטיב (הסכומים קטנים דיים כדי שזה יהיה מדויק לאגורה).
מיון וניפוי עמודים על ה-buckets; `page.totalRows` = מספר הקבוצות לפני עימוד.

### 4.5 `QuestionTextRenderer`

בונה את משפט העברית מהניסוח שהחוזה מגדיר בפועל
([`query-definition.md`](contracts/query-definition.md) "Reads as" +
`api-contract.md` §3): הפתיח "כמה בקשות תמיכה", תוויות ה-registry, תוויות ערכי
הייחוס, וסעיף "בפילוח לפי". אין ניסוח מומצא מעבר לכך; ל-metric הסכום אין ניסוח
בחוזה ולכן הוא לא מתואר במשפט.

### 4.6 גבולות אחריות

Controller (`SearchController`) = HTTP בלבד: bind → `ISearchService` → תוצאה.
כל החלטה עסקית (ולידציה, בחירת metrics, הרכבת התשובה, hashing, תזמון) ב-`SearchService`.
Infrastructure = גישת נתונים בלבד (EF, builder, handlers, החלת ה-tenant scope).

## 5. מסד נתונים

_SQL Server ל-PoC (נימוק: היכרות/רישוי) · PostgreSQL כיעד קוד-פתוח מועדף · המודל
provider-agnostic (EF Core, מעבר = החלפת provider + connection string) · JSON
כ-`nvarchar(max)` + `ToJson()`._

### 5.1 מודל הנתונים (מומש ב-S1)

ישויות ב-`SupportPlatform.Domain/Entities/`, מיפוי ב-`Infrastructure/Persistence/Configurations/`:

| ישות | טבלה | tenant-scoped |
|---|---|---|
| `Tenant` | `tenants` | – (מפתח: slug) |
| `User` | `users` | `TenantId` (FK) — מוכן ל-JWT של S8, ללא לוגיקת אימות ב-S1 |
| `SubmittingBody` | `submitting_bodies` | ✔ Global Query Filter |
| `SupportRequest` | `support_requests` | ✔ Global Query Filter |
| `ReferenceDomain/BodyType/Status/District` | `reference_*` | – (גלובלי ל-PoC) |
| `FilterFieldRegistryEntry` | `filter_field_registry` | – (whitelist, §3.4) |

- **בידוד tenant — fail-closed:** ה-Global Query Filter הוא
  `e => tenant.HasTenant && e.TenantId == tenant.TenantId`. בלי tenant context מוגדר —
  אפס שורות, לא "הכל". גישה חוצת-tenant רק דרך `IgnoreQueryFilters()` מפורש (טסטים / admin).
  `ITenantContext` נקבע ב-S1 מפרמטר הפיתוח `?tenantId=`; ב-S8 מהמשתמש המאומת.
- **Migration:** `InitialCreate` תחת `Infrastructure/Persistence/Migrations/`. כלי:
  `dotnet-ef` כ-local tool (`server/.config/dotnet-tools.json`).
- **Seed (`DbSeeder`, דטרמיניסטי ו-idempotent):** שורות ייחוס + 5 שורות registry (מ-`metadata-model.md`
  מילה במילה) · 2 tenants (`culture-sport-admin`, `welfare-admin`) · 3 משתמשי seed עם hash דטרמיניסטי
  (`SeedPasswordHasher`, PBKDF2-SHA256, ללא סיסמאות גולמיות) · ~40 `submitting_bodies` · ~500
  `support_requests` בהתפלגות מכוונת (שנים 2023–2025 30/40/30, סטטוס 55/25/20, תחום 60/40, שני tenants
  320/180). מופעל ב-`Program.cs` ב-Development בלבד (`Migrate()` + `Seed()`).
- **`?tenantId=` הוא חוזה פיתוח זמני ל-S1** — ראה §8.

## 6. Client

_מבנה `src/` (api / shared / features / state) · טופס דינמי מ-metadata · TanStack
Query · RTL עם Ant Design._

## 7. הרחבה עתידית

_הוספת תחום תמיכה / סוג גוף / שדה סינון שלם = שינוי **נתונים** בלבד (שורות ייחוס +
שורת registry + seed), ללא שינוי קוד. דוגמת metadata קונקרטית מקצה לקצה._

## 8. חתכים רוחביים

מומשו ב-S2 (יחד עם `POST /api/search`):

- **Correlation Id** — `CorrelationIdMiddleware` לוקח `X-Correlation-Id` מה-request
  או מייצר; מחזיר אותו ב-response, דוחף ל-Serilog `LogContext`, וקובע אותו כ-
  `HttpContext.TraceIdentifier` כך שהוא צף כ-`traceId` ב-ProblemDetails.
- **Serilog** — Console sink, `ILogger<T>` בהזרקה, בלי multi-sink config.
- **ProblemDetails (RFC 7807, [`contracts/error-model.md`](contracts/error-model.md))** —
  `AddProblemDetails` + `IExceptionHandler` (`Api/Errors`): `ValidationException`
  ו-`InvalidQueryException` → 400 `validation` עם `errors{}`; כל השאר → 500 `unexpected`
  מתועד. ה-`type`/`title` מ-`ProblemTypes`.
- **Validation** — FluentValidation על `QueryDefinition`, נבדק ב-service לפני שימוש.

עדיין לפי הבנייה: Auth (JWT / `X-User` + role check) — S8 · Audit Log · caching/dedup
(`definitionHash` כבר מחושב ב-`executionMeta`, ה-cache עצמו — S5).

### 8.1 גבול האימות (יעד S8; ב-S1 רק הנתונים)

זרימת היעד: `login → אימות credentials מול User.PasswordHash → הנפקת JWT → Bearer
authentication → זיהוי User → פתירת TenantId מה-User → הרשאה + בידוד tenant`.

- ב-S1 קיימת רק ישות `User` (עם hash דטרמיניסטי) — אין `AuthController`, אין הנפקת token,
  אין middleware. אין JWT מזויף/stub.
- `GET /api/metadata?tenantId=` הוא **חוזה פיתוח זמני ל-S1**. משהאימות ינחת ב-S8, ה-tenant של
  המשתמש המאומת הוא מקור הסמכות; **ה-API לא יבטח `tenantId` מהלקוח לצורך הרשאה** — הוא ישמש
  לכל היותר לבדיקת התאמה מול ה-token (אחרת 403, ראה `error-model.md`).

## 9. דיאגרמות

_Mermaid — נוספות ב-S4._

### 9.1 ERD
### 9.2 Container diagram

## 10. Decision Log

_החלטות ארכיטקטוניות מהותיות: מה הוחלט, למה, ואילו חלופות נשקלו. מתעדכן תוך כדי._
