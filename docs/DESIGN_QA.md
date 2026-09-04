# שאלות תכנון — מערכת תמיכות רוחבית (PoC)

> **סטטוס: עודכן עד S9.** התשובות נכתבו ב-S4 ומתעדכנות תוך כדי; ליטוש ההגשה
> (S11) טרם בוצע. מקור השאלות: `IMPLEMENTATION_PLAN.md` §8. כל תשובה: מה מומש
> עד כה (עם הפניה לקוד) ומה היעד שמעבר ל-PoC. ההיקף מכוון — PoC, לא מערכת שלמה.

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

**מומש (S8):** ה-`TenantId` שממנו נגזר ה-scope כבר סמכותי מזהות הקורא, לא מהקלט.
`TenantAccessGuard.EnsureTenant` (`Application/Identity`) נקרא ב-`SearchService`,
`MetadataService`, `NlQueryService`: `tenantId` בגוף/query שאינו של הקורא → **403**
(`error-model.md`), חסר → מושלם מהזהות. `?tenantId=` נשאר בחוזה אך מאומת ולא נאמן.
ה-Global Query Filter נשאר שכבת ההגנה השנייה.

**הצעד הבא:** תהליך onboarding — שורת `tenants`, קונפיגורציית ברירת מחדל per-tenant
(רשימות ייחוס, registry, branding), משתמשי-על ראשוניים, ואפשרות schema/DB נפרד
ללקוח שדורש בידוד פיזי. מקור הזהות עצמו יעבור מ-`X-User` ל-claim ב-JWT
(`ARCHITECTURE.md` §8.1) — שינוי מקור, לא שינוי אכיפה.

---

## 3. הרשאות בין גופים

**הגישה:** שתי שכבות — RBAC (מה מותר לתפקיד) + data-scoping (על אילו שורות).
העיקרון: least-privilege, נאכף בשרת בלבד, לעולם לא בסמכות הלקוח.

**מומש (S1–S8):**

- data-scoping ברמת ה-tenant — ה-Global Query Filter (שאלה 2). זו כבר הפרדת גופים
  אכיפה: משתמש של `welfare-admin` לא יכול לראות שורות של `culture-sport-admin`
  גם אם ינחש מזהים.
- **ownership scoping (S5):** שאילתה שמורה שייכת ל-`OwnerUsername` + `TenantId`;
  `SavedQueryRepository` מסנן תמיד לפי שניהם, ופעולה על רשומה מחוץ ל-scope מחזירה
  **404** (לא 403 — לא מדליף קיום, `api-contract.md` §5).
- **זהות סמכותית (S8):** `ICurrentUser` נגזר מכותרת `X-User` (auth stub, בלי JWT).
  ה-`TenantId` וה-`Role` שלו הם מקור הסמכות — `TenantAccessGuard` דוחה `tenantId`
  זר ב-403 (שאלה 2), וה-`?tenantId=` שוב לא נאמן.
- **כלל RBAC אחד קונקרטי (S8):** מחיקת שאילתה שמורה דורשת role `admin`
  (`SavedQueryService.Delete`). הבדיקה רצה **אחרי** ה-scope: רשומה מחוץ ל-scope
  נשארת 404, `analyst` על רשומה בתוך ה-scope מקבל 403. `dan` (admin) מוחק,
  `sarah`/`michal` (analyst) לא — מדגים RBAC מעל data-scoping. מכוסה ב-
  `SavedQueriesEndpointTests` + `SavedQueryServiceTests`.

**הצעד הבא:**

- מקור הזהות עובר מ-`X-User` ל-JWT: `POST /api/auth/login` → אימות מול
  `User.PasswordHash` → token עם `tenantId` + `role` claims → Bearer auth
  (`ARCHITECTURE.md` §8.1, §10 החלטה 13). האכיפה עצמה (`TenantAccessGuard`, כלל
  ה-role) לא משתנה — רק המקור שממנו `ICurrentUser` קורא.
- RBAC רחב יותר: policy/`[Authorize]` per endpoint במקום כלל בודד ב-service.
- scoping דק יותר מ-tenant: אופציונלית לפי `supportDomain` (למשל רפרנט תרבות לא
  רואה ספורט).

**איפה זה נאכף:** תמיד ב-Application — `TenantAccessGuard` ב-services, כלל ה-role
ב-`SavedQueryService`, וה-בנאי הדינמי מקבל את ה-tenant מ-`QueryDefinition.TenantId`
שכבר עבר ולידציה (§10 החלטות 6, 13).

---

## 4. שאילתות כבדות

**הגישה:** להגביל את מרחב התוצאה כברירת מחדל, לדחוף aggregation למסד, ולהשאיר
נתיב async לדוחות גדולים.

**מומש (S2):**

- **paging חובה** — `QueryDefinition.paging` עם `pageSize` מוגבל ל-1–200
  (`QueryDefinitionValidator`); ברירת מחדל 50. `page.totalGroups` מוחזר בנפרד.
- **aggregation ב-DB** — `SearchQueryExecutor` מריץ `GroupBy` בצד השרת עבור 0–1
  שדות פילוח; רק מחזירים count/sum, לא שורות גולמיות.
- **אינדקסים** — `support_requests` מאונדקס על `TenantId` ועל
  `(TenantId, SupportYear)` (`SupportRequestConfig`).
- **פשטת PoC מודעת** — 2+ שדות פילוח מבוצעים עם GroupBy בזיכרון אחרי
  materialization מינימלי (`ARCHITECTURE.md` §4.4, §10 החלטה 7), חסום בתקרה של
  50,000 שורות שמעליה חוזר 400 מפורש. **הצעד הבא:** GroupBy קומפוזיטי ב-DB.

**הצעד הבא:** command timeout + `AsNoTracking` בכל שאילתת קריאה; אינדקסים
מכוסים לצירופי הפילוח הנפוצים; read-model / materialized view לצירופים כבדים
חוזרים; דחייה של דוחות גדולים ל-job אסינכרוני עם הורדת קובץ; `cache` (שאלה 5)
שמונע חישוב חוזר.

---

## 5. מניעת הרצה חוזרת של אותה שאילתה

**הגישה:** hash קנוני של ה-`QueryDefinition` כמפתח cache + מטא-דאטה של הרצה
אחרונה + הרצה יזומה בלקוח (כפתור "חיפוש").

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
- **אחסון ה-`QueryDefinition` השמור (S5):** `saved_queries` מחזיק את ה-`QueryDefinition`
  כ-JSON קנוני ב-`DefinitionJson`, יחד עם `DefinitionHash`. בכל טעינה של שאילתה שמורה ה-JSON
  עובר `Deserialize` + ולידציה מחדש דרך אותו `IValidator<QueryDefinition>` של `/api/search`
  (`SavedQueryService`). שינוי *שובר* עתידי במבנה החוזה המוקפא של `QueryDefinition` (למשל שדה
  `required` חדש) ידרוש **data migration** חד-פעמית מעל `saved_queries.DefinitionJson`. אין
  כרגע שדה `version` ב-JSON ואין שינוי DB — מכוון ל-PoC.
- החיפוש בלקוח מופעל בלחיצה מפורשת על "חיפוש" — אין הרצה אוטומטית בהקלדה (`ARCHITECTURE.md` §6.1).

**הצעד הבא (מעבר ל-PoC):** cache מבוזר (Redis) משותף בין instances במקום ה-per-instance
בזיכרון; invalidation מכוון כשנתוני המקור משתנים; מדדי hit-rate.

---

## 6. ריבוי ספקי AI (תרגום שפה טבעית → `QueryDefinition`)

**הגישה:** ממשק אחד, מימוש נבחר בקונפיגורציה. שכבת ה-AI היא seam שאפשר להחליף בלי לגעת
בשאר המערכת — וזה מה שנמדד, לא חוכמת המתרגם.

### מומש עכשיו (S6)

- **`INlQueryProvider`** — חוזה יחיד: `Translate(text, tenantId, SearchMetadata) → NlTranslation`
  (`definition`, `confidence`, `unresolved[]`). ספק **לא** ניגש למסד, לא מריץ חיפוש ולא
  מוודא; ה-metadata נמסר לו כקלט (`ARCHITECTURE.md` §4.7). `Translate` ולא `Parse`: פרסור
  הוא איך שהמנתח הנוכחי עובד, לא מה שהממשק מבטיח. `Parse` נשאר שם ה-use case וה-endpoint
  לפי החוזה — `API Parse → NlQueryService → provider Translate`.
- **בחירה לפי `configuration`** — `AddApplication` מחזיק מפת `provider key → סוג`, רושם כל
  ספק ב-**keyed DI** (`AddKeyedScoped`, מובנה ב-.NET 8), ורושם את `INlQueryProvider` כ-resolver
  שקורא את `NlQuery:Provider`. **הוספת ספק = שורה במפה + ערך בקונפיג**, בלי קומפילציה מחדש.
  מפתח לא מוכר נכשל בעליית האפליקציה עם הודעה שמציינת את המפתח ואת הספקים המובנים.
  שלושה טסטים ב-`DependencyInjectionTests` נועלים את זה: ברירת המחדל, ספק אחר שנבחר לפי
  מפתח, ומפתח לא קיים.
- **`RuleBasedNlQueryProvider`** (מפתח `ruleBased`) — המימוש היחיד ב-PoC: מנתח **דטרמיניסטי**
  שאוצר המילים שלו הוא ה-metadata. שלושה כללים — `CodeListFilterRule` (כל שדה `codeList`
  ב-registry), `YearRule`, `SegmentationRule`. מה שלא זוהה חוזר ב-`unresolved`;
  **אף ערך לא מומצא**, וגם מילה דו-משמעית לא נפתרת בניחוש.
- **`NlQueryService`** מריץ על התוצר את אותו `IValidator<QueryDefinition>` ואת אותו
  `QuestionTextRenderer` ש-`/api/search` משתמש בהם — ספק אינו מקור אמון. הפרסור לא
  מריץ שאילתה: המשתמש רואה את הפרשנות ולוחץ "הרץ" (`api-contract.md` §4). טסט DI מקבע
  שה-use case בכלל לא תלוי ב-`ISearchService`.

### אופציה עתידית — לא נדרשת ל-PoC

- **ספק LLM** (Gemini / OpenAI / מודל מקומי) = מימוש נוסף של אותו ממשק, שנבחר באותו מפתח
  קונפיג. **ה-PoC לא מדבר עם אף שירות AI חיצוני**: אין מפתח API, אין תלות רשת, והרצה
  מ-clone נקי לא דורשת הרשמה לשירות — `ARCHITECTURE.md` §10 החלטה 11.
- **`fallback chain`** — ספק ראשי נכשל או מחזיר `confidence` נמוך → נפילה למנתח הכללים
  במקום שגיאה. `confidence` כבר מוחזר בחוזה בדיוק בשביל החלטה כזו; מה שחסר הוא ספק שני
  שיהיה שווה ליפול ממנו.
- **הקשחת ספק מנוהל** — timeout, מכסות, redaction של מידע רגיש לפני שליחה החוצה,
  caching של פרשנויות.
- **מורפולוגיה עברית אמיתית** — הנרמול הנוכחי מכוון-מוגבל (§10 החלטה 12); ספק LLM מייתר אותו.

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
  הוא גס — עם הרצה יזומה בלקוח זו שורה לכל לחיצת "חיפוש". ל-PoC זה מקובל ונותן הדגמה
  מלאה של "מי הריץ מה"; בפרודקשן: sampling / תור אסינכרוני, הפרדת audit-קריאה מ-audit-mutation,
  ו-payload רזה (hash בלבד) ל-reads.
- **ריצת שאילתה שמורה = שתי שורות audit (מכוון).** `POST /api/saved-queries/{id}/run` מייצר
  שורת `run` (`EntityType=SavedQuery` + מזהה השאילתה השמורה) **וגם** שורת `search`
  (`EntityType=QueryDefinition` + ה-`QueryDefinition` ב-payload), כי ה-run מריץ דרך
  `ISearchService`. אלו אינן שורות כפולות בטעות — הן מתעדות שני דברים שונים ("איזו שאילתה
  שמורה הורצה" מול "חיפוש עם ה-definition הזה בוצע") ומקושרות באותו `CorrelationId`. ניתוח
  שצריך להפריד חיפושים יזומים מריצות של שאילתות שמורות מצטרף לפי `CorrelationId` / `EntityType`,
  לא לפי `Action` בלבד.
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
| שכבת AI מופשטת | **מומש (S6)** | `INlQueryProvider` נבחר לפי `NlQuery:Provider` (keyed DI); המימוש: `RuleBasedNlQueryProvider` — מנתח דטרמיניסטי, בלי LLM חיצוני — שאלה 6 |
| auth מרכזי | **מומש (S8 — auth stub)** | `ICurrentUser` מ-`X-User` (בלי JWT) + `TenantAccessGuard` (tenant זר → 403) + כלל role אחד (מחיקת שאילתה שמורה = `admin`) — שאלות 2–3. יעד production: `login` + JWT (§8.1, החלטה 13) |
| Audit | **מומש (S5)** | `IAuditService.Record(...)`, קריאות מפורשות ב-services (לא interceptor) |
| config management + Secrets | חלקי | `appsettings*.json` + `.env.example` + env ב-Compose; יעד: config service + secret store (Key Vault / Secrets Manager) |
| CI/CD + IaC | יעד (S10) | `.github/workflows/ci.yml` (build+test); IaC לא בהיקף |
| API gateway | יעד | reverse proxy לניתוב, rate-limit, TLS termination — `ARCHITECTURE.md` §9.2 |

**עיקרון מנחה (PoC):** בונים תשתית רוחבית **כשיש צרכן ממשי** — החתכים הרוחביים
נכנסו ב-S2 יחד עם `POST /api/search` ולא לפני כן (§10 החלטה 8, `IMPLEMENTATION_PLAN.md`
§3.2 "אפס over-engineering"). כל שאר הפריטים מתועדים כ"צעד הבא" ולא מיושמים
ספקולטיבית.
