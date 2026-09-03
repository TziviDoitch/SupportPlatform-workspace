# שאלות תכנון — מערכת תמיכות רוחבית (PoC)

> **סטטוס: טיוטה (S4).** התשובות נכתבו ב-S4 ומלוטשות ב-S11. מקור השאלות:
> `IMPLEMENTATION_PLAN.md` §8. כל תשובה: מה מומש עד כה (עם הפניה לקוד) ומה היעד
> שמעבר ל-PoC. ההיקף מכוון — PoC, לא מערכת שלמה.

---

## 1. הוספת סוגי תמיכה חדשים בלי שינוי קוד

**הגישה:** metadata-driven. שני מקורות נתונים במסד מגדירים את כל שדות החיפוש —
טבלאות ה-`reference_*` (ערכים) ו-`filter_field_registry` (ה-whitelist של השדות).
הקוד לא מכיר אף שם שדה או ערך קונקרטי.

**מומש (S1–S3):**

- צד שרת — `GET /api/metadata` (`MetadataService` / `MetadataRepository`) מחזיר
  את רשימות הייחוס + `filterFieldRegistry` מהמסד. `DynamicQueryBuilder`
  (`Infrastructure/Search/`) בונה שאילתה **רק** משדות שנמצאים ב-registry ודוחה
  כל אחר ב-400 (§3.4 קו אדום).
- צד לקוח (S3) — `SearchForm` מרנדר פקד אחד לכל רשומת registry בזמן ריצה, לפי
  `kind` ו-`referenceList`. אין שדה מקודד קשיח (`client/CLAUDE.md` → "The search
  slice").

**הדגמה — הוספת תחום "חינוך", אפס קוד:** מוסיפים שורה ל-`reference_domains`
(`{code:"education", label:"חינוך"}`) דרך `DbSeeder` / מיגרציית data / INSERT.
מיד: המטא-דאטה מחזירה את הערך → הטופס מציג אותו → `QueryDefinition` מכיל
`filters.supportDomain:["education"]` → הבנאי מפיק `WHERE SupportDomainCode IN
('education')` → מנסח השאלה קורא "בתחום חינוך". הנתיב המלא מפורט ב-`ARCHITECTURE.md`
§7.1.

**הצעד הבא (מעבר ל-PoC):** שדה סינון *שלם* חדש מעל `kind` קיים דורש היום שורת
רישום אחת ב-`FilterHandlers.Default` (§7.2). קובץ JSON חיצוני של הגדרות שדה
(operators + column mapping) שנטען ל-`filter_field_registry` ב-onboarding יבטל גם
את השורה הזו. `kind` חדש (טווח מספרי, טקסט) = תת-מחלקת `FilterHandler` אחת (§7.3).

---

## 2. הוספת משרד ממשלתי נוסף

**הגישה:** multi-tenant מבוסס-שורה. כל ישות עסקית נושאת `TenantId`, ו-Global Query
Filter של EF Core מבטיח שאין דרך לשכוח את הסינון.

**מומש (S1):**

- `SupportRequest` ו-`SubmittingBody` הם tenant-scoped
  (`Infrastructure/Persistence/Configurations/`), עם Global Query Filter
  **fail-closed**: `e => tenant.HasTenant && e.TenantId == tenant.TenantId` —
  בלי tenant context מוגדר מקבלים אפס שורות, לא "הכל" (`ARCHITECTURE.md` §5.1,
  §10 החלטה 5).
- `ITenantContext` / `TenantContext` (`Infrastructure/Persistence/`) הוא הנקודה
  היחידה שקובעת את ה-scope. ב-S1 הוא נגזר מפרמטר הפיתוח `?tenantId=`.
- ה-seeder כבר מריץ שני tenants (`culture-sport-admin`, `welfare-admin`) עם
  התפלגות נתונים שונה, כדי שהבידוד יהיה בדיק.
- רשומות הייחוס גלובליות ל-PoC; אם משרד חדש מביא סט ערכים משלו, הן יקבלו גם הן
  `TenantId` (שינוי מודל קטן).

**הצעד הבא:** תהליך onboarding — שורת `tenants`, קונפיגורציית ברירת מחדל per-tenant
(רשימות ייחוס, registry, branding), משתמשי-על ראשוניים, ואפשרות schema/DB נפרד
ללקוח שדורש בידוד פיזי. ה-`TenantId` שממנו נגזר ה-scope יעבור ב-S8 מ-`?tenantId=`
ל-claim ב-JWT (`ARCHITECTURE.md` §8.1).

---

## 3. הרשאות בין גופים

**הגישה:** שתי שכבות — RBAC (מה מותר לתפקיד) + data-scoping (על אילו שורות).
העיקרון: least-privilege, נאכף בשרת בלבד, לעולם לא בסמכות הלקוח.

**מומש (S1–S5):**

- data-scoping ברמת ה-tenant — ה-Global Query Filter (שאלה 2). זו כבר הפרדת גופים
  אכיפה: משתמש של `welfare-admin` לא יכול לראות שורות של `culture-sport-admin`
  גם אם ינחש מזהים.
- **ownership scoping (S5):** שאילתה שמורה שייכת ל-`OwnerUsername` + `TenantId`;
  `SavedQueryRepository` מסנן תמיד לפי שניהם, ופעולה על רשומה מחוץ ל-scope מחזירה
  **404** (לא 403 — לא מדליף קיום, `api-contract.md` §5).
- **seam זהות (S5):** `ICurrentUser` נגזר מכותרת `X-User` (`ARCHITECTURE.md` §5.2).
- ל-`User` יש שדה `Role` במודל, עדיין בלי לוגיקה שנשענת עליו.

**הצעד הבא (S8):**

- אימות → זהות → תפקיד: `login` → אימות מול `User.PasswordHash` → JWT עם
  `tenantId` + `role` → אכיפה ב-`ICurrentUser` + policy/authorization ב-controller
  (`ARCHITECTURE.md` §8.1). ה-`X-User` של S5 הופך למקור לא-מהימן.
- scoping דק יותר מ-tenant: אופציונלית לפי `supportDomain` (למשל רפרנט תרבות לא
  רואה ספורט).
- בדיקת role אחת קונקרטית שתדגים הפרדה (§6 S8 "רזה: `X-User` header").

**איפה זה נאכף:** תמיד ב-Application/Infrastructure — הבנאי הדינמי מקבל את ה-tenant
מ-`QueryDefinition.TenantId` שכבר עבר ולידציה, וה-`?tenantId=` בגוף הבקשה לא
מהימן לצורך הרשאה (§10 החלטה 6).

---

## 4. שאילתות כבדות

**הגישה:** להגביל את מרחב התוצאה כברירת מחדל, לדחוף aggregation למסד, ולהשאיר
נתיב async לדוחות גדולים.

**מומש (S2):**

- **paging חובה** — `QueryDefinition.paging` עם `pageSize` מוגבל ל-1–200
  (`QueryDefinitionValidator`); ברירת מחדל 50. `page.totalRows` מוחזר בנפרד.
- **aggregation ב-DB** — `SearchQueryExecutor` מריץ `GroupBy` בצד השרת עבור 0–1
  שדות פילוח; רק מחזירים count/sum, לא שורות גולמיות.
- **אינדקסים** — `support_requests` מאונדקס על `TenantId` ועל
  `(TenantId, SupportYear)` (`SupportRequestConfig`).
- **פשטת PoC מודעת** — 2+ שדות פילוח מבוצעים עם GroupBy בזיכרון אחרי
  materialization מינימלי (`ARCHITECTURE.md` §4.4, §10 החלטה 7).

**הצעד הבא:** command timeout + `AsNoTracking` בכל שאילתת קריאה; אינדקסים
מכוסים לצירופי הפילוח הנפוצים; read-model / materialized view לצירופים כבדים
חוזרים; דחייה של דוחות גדולים ל-job אסינכרוני עם הורדת קובץ; `cache` (שאלה 5)
שמונע חישוב חוזר.

---

## 5. מניעת הרצה חוזרת של אותה שאילתה

**הגישה:** hash קנוני של ה-`QueryDefinition` כמפתח cache + מטא-דאטה של הרצה
אחרונה + debounce בלקוח.

**מומש (S2–S5):**

- `DefinitionHasher` (`Application/Search/`) מחשב SHA-256 **קנוני** של ה-definition:
  מפתחות `filters`, ה-codes בתוך כל ערך, ו-`metrics` ממוינים; `segmentation` ו-`sort`
  נשמרים כסדרם (הסדר משמעותי לתוצאה). `[]` ו-`["count"]` נותנים אותו hash. מוחזר
  כ-`executionMeta.definitionHash` בכל תשובת `/search`.
- **`IMemoryCache` עם TTL (S5):** `SearchService` בודק את ה-hash לפני הרצה; פגיעה →
  מוחזרת התוצאה השמורה עם `executionMeta.cacheHit = true`. TTL מ-`Search:CacheTtlSeconds`
  (ברירת מחדל 60ש'). `0` מכבה dedup — זה מנוף ה-fallback של §7.3, ומה שהטסטים של
  ה-endpoint משתמשים בו כדי להישאר דטרמיניסטיים. ה-cache **חסום**: `SizeLimit = 1000`
  entries (`Program.cs`) + פקיעה ב-TTL — הגדרות שונות בתוך החלון לא מנפחות זיכרון בלי גבול.
  הוא per-instance בזיכרון — מכוון ל-PoC (ראה "הצעד הבא").
- **`saved_queries.last_run_at` / `last_run_row_count` (S5):** מתעדכנים ב-`POST /{id}/run`
  ומוצגים במסך השאילתות השמורות — המשתמש רואה שהשאילתה כבר רצה ומתי.
- הלקוח עושה debounce (~400ms, `useDebouncedValue`) בהקלדה בטופס (`ARCHITECTURE.md` §6.1).

**הצעד הבא (מעבר ל-PoC):** cache מבוזר (Redis) משותף בין instances במקום ה-per-instance
בזיכרון; invalidation מכוון כשנתוני המקור משתנים; מדדי hit-rate.

---

## 6. ריבוי ספקי AI (תרגום שפה טבעית → `QueryDefinition`)

**הגישה:** Strategy + Factory מאחורי חוזה יחיד. שכבת ה-AI היא seam שאפשר להחליף
בלי לגעת בשאר המערכת.

**מומש:** טרם — זה S6. אבל התשתית שמאפשרת אותו כבר קיימת: `QueryDefinition` הוא
אובייקט קנוני שה-NL parser יפיק בדיוק כמו שהטופס בונה (§10 החלטה 3), כך שהתרגום
מזין את אותו נתיב ולידציה + מנוע SQL בלי קוד חדש בצד הצרכן.

**היעד (S6):**

- `INlQueryTranslator` — חוזה אחד: טקסט + tenant → `{ definition, interpretationText,
  confidence, unresolved[] }` (`api-contract.md` §4).
- `NlQueryTranslatorFactory` בוחר מימוש לפי `configuration` (מפתח config, לא
  קומפילציה).
- מימושים: `MockNlQueryTranslator` (דוגמאות המטלה → `QueryDefinition` ידוע,
  תמיד עובד) ו-`RuleBasedNlQueryTranslator` (מילון סטטוס/סוג-גוף/תחום/שנה/מחוז +
  `unresolved[]` למה שלא זוהה). LLM אמיתי = מימוש נוסף של אותו ממשק — seam בלבד
  ב-PoC.
- `fallback chain` — אם ספק ראשי נכשל/לא בטוח (confidence נמוך), נופלים למימוש
  הבא (Mock) במקום להחזיר שגיאה.

**הצעד הבא מעבר ל-PoC:** ספק LLM מנוהל עם timeout, מכסות, ו-redaction של מידע
רגיש לפני שליחה החוצה; caching של פרשנויות.

---

## 7. ניטור וחקירת תקלות

**הגישה:** כל בקשה נושאת מזהה מתאם יחיד שמופיע בכל שורת לוג ובכל תשובת שגיאה, כך
שאפשר לשחזר בקשה מקצה-לקצה מדיווח משתמש.

**מומש (S2):**

- **structured logging + correlation id** — `CorrelationIdMiddleware` לוקח
  `X-Correlation-Id` מהבקשה או מייצר, מחזיר אותו ב-response header, דוחף ל-Serilog
  `LogContext`, וקובע כ-`HttpContext.TraceIdentifier` כך שהוא צף כ-`traceId`
  ב-ProblemDetails (`ARCHITECTURE.md` §8).
- **Serilog** — Console sink, מובנה (`Api/Program.cs`).
- **`/health`** — קיים מ-S0 (`AddHealthChecks` → `200 Healthy`).
- **error model אחיד** — כל שגיאה RFC 7807 עם `traceId` לציטוט בדיווח באג
  (`error-model.md`).

**מומש (S5):** **Audit Log** — `IAuditService.Record(...)` נכתב על כל `search` ועל
`create`/`update`/`delete`/`run` של שאילתות שמורות; כל שורה נושאת `User`, `CorrelationId`
(אותו id של §7 מעלה, חסום ל-64 תווים ב-`CorrelationIdMiddleware`), `OccurredAt` ו-`payload`
JSON. זה עונה על "מי הריץ מה ומתי" וקושר חזרה ללוגים דרך ה-correlation id.

**מגבלות מודעות ב-PoC:**

- **נפח.** audit על *כל* `/search` (כולל cache hit) עם ה-`QueryDefinition` המלא ב-payload
  הוא גס — עם debounce של 400ms בלקוח זו שורה לכל עצירת-הקלדה. ל-PoC זה מקובל ונותן הדגמה
  מלאה של "מי הריץ מה"; בפרודקשן: sampling / תור אסינכרוני, הפרדת audit-קריאה מ-audit-mutation,
  ו-payload רזה (hash בלבד) ל-reads.
- **אטומיות.** `Record(...)` מבצע `SaveChanges` משלו — שורת ה-audit לא נכתבת באותה טרנזקציה
  של הפעולה שהיא מתעדת. ל-PoC בלי טרנזקציות זה בסדר; בפרודקשן: אותה טרנזקציה, או outbox
  pattern, כדי שלא ייווצר audit ל-פעולה שנכשלה (או להפך).

**הצעד הבא:** metrics (latency, rowCount, cacheHit — כבר נאספים ב-`executionMeta`,
נותר לייצא ל-Prometheus/OTel); distributed tracing (OpenTelemetry על אותו
correlation id); alerting על שיעור 5xx / latency; sink מרוכז (Seq / ELK) במקום
console; שאילתות/דוחות מעל ה-`audit_log`.

---

## 8. תשתיות רוחביות לשלב ראשון

מה חוצה-מערכת ולכן נבנה פעם אחת, לא per-feature. הטבלה מסמנת מה כבר קיים ב-PoC:

| תשתית | סטטוס ב-PoC | הערה |
|---|---|---|
| מודל metadata גנרי | **מומש (S1)** | `filter_field_registry` + `reference_*` — שאלה 1 |
| logging + correlation id | **מומש (S2)** | Serilog + `CorrelationIdMiddleware` — שאלה 7 |
| error model אחיד (RFC 7807) | **מומש (S2)** | `IExceptionHandler` + `ProblemTypes` |
| dedup / cache | **מומש (S5)** | `DefinitionHasher` → `IMemoryCache` — שאלה 5 |
| שכבת AI מופשטת | seam (S6) | `INlQueryTranslator` + Factory — שאלה 6 |
| auth מרכזי | חלקי (S5 seam; S8 מלא) | `ICurrentUser` מ-`X-User` + scoping owner/tenant; JWT + role — S8, שאלות 2–3 |
| Audit | **מומש (S5)** | `IAuditService.Record(...)`, קריאות מפורשות ב-services (לא interceptor) |
| config management + Secrets | חלקי | `appsettings*.json` + `.env.example` + env ב-Compose; יעד: config service + secret store (Key Vault / Secrets Manager) |
| CI/CD + IaC | יעד (S10) | `.github/workflows/ci.yml` (build+test); IaC לא בהיקף |
| API gateway | יעד | reverse proxy לניתוב, rate-limit, TLS termination — `ARCHITECTURE.md` §9.2 |

**עיקרון מנחה (PoC):** בונים תשתית רוחבית **כשיש צרכן ממשי** — החתכים הרוחביים
נכנסו ב-S2 יחד עם `POST /api/search` ולא לפני כן (§10 החלטה 8, `IMPLEMENTATION_PLAN.md`
§3.2 "אפס over-engineering"). כל שאר הפריטים מתועדים כ"צעד הבא" ולא מיושמים
ספקולטיבית.
