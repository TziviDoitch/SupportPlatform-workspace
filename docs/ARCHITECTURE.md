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

## 9. דיאגרמות

_Mermaid — נוספות ב-S4._

### 9.1 ERD
### 9.2 Container diagram

## 10. Decision Log

_החלטות ארכיטקטוניות מהותיות: מה הוחלט, למה, ואילו חלופות נשקלו. מתעדכן תוך כדי._
