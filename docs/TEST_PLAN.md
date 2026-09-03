# תוכנית בדיקות ידנית — מערכת תמיכות רוחבית (PoC)

בדיקות אוטומטיות מכסות את מנוע השאילתות, מנסח השאלה, המנתח, יציבות ה-`definitionHash`
ומסלול happy-path אחד מקצה-לקצה (`dotnet test` — ראו [`ARCHITECTURE.md`](ARCHITECTURE.md) §8.2).
המסמך הזה משלים בבדיקות ידניות מול ה-UI ובקצוות שקשה לכסות אוטומטית.

## מוקדמות

1. `cd infra && docker compose up --build` — שלושת השירותים עולים.
2. `http://localhost:5080/health` → `200 Healthy`.
3. `http://localhost:5173` נטען, ללא שגיאות ב-console.
4. ה-DB עולה עם seed דטרמיניסטי: 2 tenants, 3 משתמשים, ~40 גופים, 500 בקשות
   (320 ב-`culture-sport-admin`, 180 ב-`welfare-admin`).

## משתמשי seed

| שם משתמש | tenant | role | סיסמה (דמו) |
|---|---|---|---|
| `sarah` | `culture-sport-admin` | analyst | `pass` |
| `dan` | `culture-sport-admin` | admin | `pass` |
| `michal` | `welfare-admin` | analyst | `pass` |

הזהות ב-PoC היא כותרת `X-User` (ברירת מחדל `sarah`) — אין מסך התחברות. הלקוח שולח
`X-User` מ-`api/config.ts`; לבדיקה כמשתמש אחר, שנו שם או שלחו את הכותרת ידנית (Swagger / `api.http`).

---

## 1. מסך חיפוש (`/search`)

| # | תרחיש | צעדים | תוצאה צפויה |
|---|---|---|---|
| 1.1 | טעינת הטופס מ-metadata | פתחו את `/search` | הטופס נבנה מ-`filterFieldRegistry`: שדות `סוג גוף`, `תחום תמיכה`, `סטטוס`, `מחוז` (בחירה מרובה) + `שנת תמיכה` (טווח/יחיד). הסדר תואם ל-`sortOrder`. |
| 1.2 | חיפוש בסיסי | בחרו `סטטוס = מאושר`, פילוח לפי `שנת תמיכה`, הריצו | טבלת תוצאות עם שורות לפי שנה, פאנל "שאלה קריאה" מציג משפט עברי תקין, גרף עמודות מוצג (פילוח יחיד). |
| 1.3 | משפט הדוגמה מהמטלה | סוג גוף = עמותה, תחום = תרבות, סטטוס = מאושר, שנים 2023–2025, פילוח לפי שנה | פאנל השאלה: `כמה בקשות תמיכה עם סוג גוף: עמותה, תחום תמיכה: תרבות, סטטוס: מאושר, שנת תמיכה: 2023–2025, בפילוח לפי שנת תמיכה?` |
| 1.4 | עימוד ומיון | הריצו חיפוש ללא פילוח, שנו עמוד וכיוון מיון בטבלה | הבקשה נשלחת מחדש עם `paging`/`sort` מעודכנים; ה-pager משקף את `page.totalRows`. |
| 1.5 | בלי גרף בפילוח מרובה | פלחו לפי `מחוז` + `שנת תמיכה` | הטבלה מציגה את הצירופים; הגרף **לא** מוצג (מוצג רק בפילוח שדה יחיד). |
| 1.6 | מצבי טעינה/ריק/שגיאה | (טעינה) הריצו חיפוש; (ריק) ראו 2.2; (שגיאה) עצרו את ה-api והריצו | טעינה: `PageLoader` ממורכז. שגיאה: **באנר `Alert` inline** באזור התוצאות עם `title` + `detail` + `traceId` — בלי toast כפול (כשל חיפוש מוצג במקום התוצאות, לא כ-toast). |

## 2. תוצאות

| # | תרחיש | צעדים | תוצאה צפויה |
|---|---|---|---|
| 2.1 | אגרגציה = ברירת מחדל | חיפוש עם פילוח לפי שנה | כל שורה = דלי (`count`, ו-`sumAmountApproved` אם נבחר). |
| 2.2 | **קצה: אפס תוצאות** | סננו `שנת תמיכה = 1990` (יחיד) | הבקשה מצליחה (200); הטבלה מציגה מצב ריק / דלי אחד עם `count = 0`; אין שגיאה; פאנל השאלה עדיין תקין. |

## 3. שאילתות שמורות (`/saved-queries`)

| # | תרחיש | צעדים | תוצאה צפויה |
|---|---|---|---|
| 3.1 | שמירה | במסך החיפוש, "שמור שאילתה", תנו שם, שמרו | מופיעה ברשימת השאילתות השמורות עם `ownerUsername` + `tenantId` של הקורא. |
| 3.2 | הרצה מחדש | ברשימה, "הרץ" על שורה | מוצגים כותרת סיכום + `ResultsPanel` (גרף + טבלה) במצב **read-only** — בלי pager ובלי כותרות מיון לחיצות (הרצה מחדש לא מקבלת override של הגדרה); `lastRunAt`/`lastRunRowCount` מתעדכנים. |
| 3.3 | שם ריק | "שמור שאילתה" עם שם רווחים בלבד | `400` ProblemDetails; toast שגיאה; לא נשמר. |
| 3.4 | מחיקה ע"י admin | כ-`dan`, מחקו שאילתה שלו | `204`; נעלמת מהרשימה. |
| 3.5 | **קצה: מחיקת שאילתה של משתמש אחר** | כ-`sarah` צרו שאילתה; כ-`dan` (אותו tenant) נסו למחוק דרך `DELETE /api/saved-queries/{id}` | `404` (לא `403`) — ה-scope הוא owner + tenant, וקיום השורה לא מודלף. אותו דבר כ-`michal` (tenant אחר). |
| 3.6 | **קצה: analyst מנסה למחוק שאילתה שלו** | כ-`sarah` (analyst) צרו שאילתה ומחקו אותה | `403` ProblemDetails (`type` נגמר ב-`/forbidden`); השורה נשארת. |

## 4. שאלה חופשית / NL (`/nl-query`)

| # | תרחיש | צעדים | תוצאה צפויה |
|---|---|---|---|
| 4.1 | פירוש מלא | הזינו `כמה עמותות בתחום התרבות אושרו בשנת 2024 לפי מחוז`, "פרש" | פאנל הפירוש: משפט read-back מהשרת, פילטרים שדה-שדה (`תחום = תרבות`, `סטטוס = מאושר`, `שנה = 2024`), פילוח `מחוז`, `unresolved` ריק. |
| 4.2 | הרצה אחרי פירוש | לחצו "הרץ" בפאנל הפירוש | עובר למסלול `POST /api/search` הרגיל (אותו `ResultsPanel`); הפירוש עצמו **לא** הריץ שאילתה. |
| 4.3 | **קצה: טקסט לא מזוהה** | הזינו `כמה בקשות הוגשו על ידי אשכולות אזוריים` | `200`; `filters` ריק; `unresolved` מכיל את המילים שלא מופו; שום פילטר לא הומצא. |
| 4.4 | טקסט ריק | "פרש" בלי טקסט | `400` עם שגיאת שדה `text`. |
| 4.5 | שדה שנזכר אך לא נוצל | `... לפי סטטוס` (סטטוס אינו segmentable) | הפילוח לא כולל `status`; העובדה שנזכר ולא נוצל מדווחת (לא מושמט בשקט). |

## 5. חתכים רוחביים

| # | תרחיש | צעדים | תוצאה צפויה |
|---|---|---|---|
| 5.1 | בידוד tenant | כ-`michal` פנו ל-`GET /api/metadata?tenantId=culture-sport-admin` | `403` `forbidden` — הזהות סמכותית; לא ניתן לגשת ל-tenant אחר. |
| 5.2 | Correlation Id | שלחו בקשה עם `X-Correlation-Id: test-1` | אותו ערך חוזר ב-header התגובה ומופיע כ-`traceId` בשגיאות. |
| 5.3 | ProblemDetails | כל שגיאה למעלה | `Content-Type: application/problem+json`, גוף `{ type, title, status, detail, traceId, errors }`. |
| 5.4 | Error boundary | (dev) זרקו חריגה בקומפוננטת מסך והריצו | במקום דף לבן — `Result` שגיאה בעברית עם כפתור "רענן דף" תקין; שאר ה-shell (header/nav) שריד. |
| 5.5 | הרחבה בלי קוד | ראו [`EXTENSIBILITY_DEMO.md`](EXTENSIBILITY_DEMO.md) | `INSERT` ל-`reference_domains` → התחום החדש ב-`/api/metadata`, בטופס, בתוצאת `/api/search` ובמשפט השאלה — בלי build/restart. |

---

## קצוות שכוסו אוטומטית (ל-reference)

| קצה | טסט |
|---|---|
| טווח שנים הפוך (`from > to`) | `QueryDefinitionValidatorTests.Reversed_year_range_is_rejected` · `SearchEndpointTests.Reversed_year_range_is_a_400_with_a_field_error` |
| אפס תוצאות | `SearchQueryExecutorTests.A_filter_that_matches_no_rows_returns_a_zero_bucket` |
| שדה זר (לא ב-Registry) | `DynamicQueryBuilderTests.An_unknown_field_id_is_rejected_before_any_handler_runs` |
| NL לא מזוהה | `NlQueriesEndpointTests.Reports_words_it_could_not_map_instead_of_inventing_filters` |
| מחיקת שאילתה של אחר | `SavedQueriesEndpointTests.Another_users_saved_query_is_a_404_*` · `Delete_by_an_analyst_is_a_problem_details_403_*` |
| happy-path מלא | `HappyPathIntegrationTests.Identity_metadata_search_save_run_and_parse_compose` |
