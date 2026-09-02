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

_`QueryDefinition` כאובייקט קנוני יחיד · `DynamicQueryBuilder` שבונה `IQueryable`
דרך whitelist מ-`FilterFieldRegistry` (§3.4 קו אדום) · Aggregation לפי
`segmentation` · `QuestionTextRenderer`. חוזה: [`contracts/query-definition.md`](contracts/query-definition.md)._

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

_Serilog + Correlation Id · ProblemDetails (RFC 7807,
[`contracts/error-model.md`](contracts/error-model.md)) · Validation
(FluentValidation) · Auth (JWT מינימלי / `X-User` + tenant filter + role check) ·
Audit Log · caching/dedup (`definitionHash`)._

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
