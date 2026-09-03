# Review Notes — S11

מסמך ההגשה של סבב הביקורת (`IMPLEMENTATION_PLAN.md` §6, S11). מסכם מה נבדק,
מה תוקן, ומה נותר מודע-ומתועד.

## 1. סבב איכות (בוצע)

ביקורת מפתח-בכיר על כל הפרויקט + השוואת מבנה מול שני פרויקטי Malam ייחוס
(`mevaker-hamedina-client`, `MevakerHamedina`). ~57 קבצים שונו. עיקרי התיקונים:

- **טיפול שגיאות — עקביות.** `MetadataController` בנה `BadRequest("string")` ידני
  → כעת זורק ומחזיר `application/problem+json` ככל שאר הנתיבים. פורמט ה-`detail`
  של ולידציה תוקן ("One or more validation errors occurred." במקום נוסח שגוי
  שהתייחס תמיד ל-QueryDefinition).
- **`[ProducesResponseType]`** על כל ה-actions בכל 4 ה-controllers + `ProblemDetails`
  כ-error type — Swagger מתעד כעת 400/403/404.
- **לקוח: משטח שגיאה אחד לכל כשל.** קודם: toast **וגם** באנר inline לאותו כשל
  חיפוש, עם פורמוט `traceId` כפול בשני קבצים. כעת: `formatProblemDetail` יחיד;
  `searchApi.run` מעביר `{ notify: false }` (הבאנר ב-`ResultsPanel` הוא המשטח);
  mutations/parse שומרים toast.
- **לקוח: React `ErrorBoundary`** (`App/ErrorBoundary`) — קודם לא היה, throw ברינדור
  החשיך את כל האפליקציה.
- **לקוח: `tsconfig` `strict: true`** — קודם כלל "אין `any`" היה מוסכמה בלבד, לא
  נאכף. הופעל — אפס תיקונים נדרשו.
- **קוד מת / כפילות** — הוסר `PagePlaceholder`; אוחדו פורמטים (`lib/format`,
  `lib/labels`, `lib/queryDefinition`); `useMetadata` הועבר ל-`hooks/` משותף.
- **בקרות inert** — פאנל הרצה-מחדש הציג pager + כותרות מיון לחיצות שלא עשו כלום;
  כעת read-only.
- **תיעוד** — הוסרו באנרי "טיוטה (S4)"; `/api/auth/login` ו-`401` סומנו "יעד
  production, לא ממומש"; תוקן cross-ref שבור ב-`TEST_PLAN.md`.

בדיקות/build/lint אחרי הסבב: **server 153 ✔, client 49 ✔, שני lint נקיים, 0 warnings.**

> הערה: `code-review (high)` האוטומטי לא הורץ בנפרד — במקומו בוצעה ביקורת ידנית
> מקיפה (חקירה רב-סוכנית + סבב איכות). ניתן להריץ `/code-review high` על ה-branch
> בנפרד אם רוצים גם את המעבר האוטומטי.

## 2. Security review — שלוש הנקודות של S11

### 2.1 Injection במנוע השאילתות

- **Whitelist לפני הכול.** `DynamicQueryBuilder.Apply`
  (`server/src/Infrastructure/Search/DynamicQueryBuilder.cs:19-22`) דוחה כל
  `field id` שלא ב-`filter_field_registry` **לפני** שרץ handler כלשהו. הרובד
  הראשי הוא `QueryDefinitionValidator` (FluentValidation); הבנאי חוזר על הבדיקה
  כ-defense-in-depth. `DynamicQueryBuilderTests.An_unknown_field_id_is_rejected_before_any_handler_runs`
  מוכיח את הסדר עם spy.
- **אין string-parsing ואין reflection על קלט.** `FilterPredicates`
  (`server/src/Infrastructure/Search/Filters/FilterPredicates.cs`) בונה עצי
  `Expression` מ-selectors שהם lambdas מהודרים שנמסרים ברישום
  (`FilterHandlers.Default`). `Enumerable.Contains` נלקח דרך delegate, לא מחרוזת קסם.
- **ערכים פרמטריים.** קודים נכנסים כ-`Expression.Constant(codes.ToArray())` →
  EF Core מייצר `WHERE ... IN (@p0, @p1, ...)`.
- **אין `switch` על field id / kind** (§10 החלטה 4).

הרצה חיה (S11):

```
POST /api/search  filters = { "costCenter; DROP TABLE support_requests;--": ["x"] }
→ 400  application/problem+json
   errors = {"filters.costCenter":["'costCenter' is not a known filter field."]}
support_requests: 500 rows (לא הושפע)
```

### 2.2 Secrets

| פריט | מצב |
|---|---|
| `server/src/Api/appsettings.Development.json` — `Password=Your_strong_Passw0rd` | סיסמת SA של קונטיינר SQL מקומי חד-פעמי, **זהה ל-`infra/.env.example`**. לא production, לא API key, לא סיסמת משתמש. נשאר כדי ש-`dotnet run` יעבוד מ-clone נקי; מתועד ב-README §מגבלות. |
| `DbSeeder.DemoPassword = "pass"` | סיסמת דמו של משתמשי seed, מתועדת ב-`api-contract.md` §1. מאוחסנת רק כ-hash (`SeedPasswordHasher`, PBKDF2). |
| JWT / IdP secrets | אין — אין auth ב-PoC. |
| מפתח LLM | אין — ה-NL parser הוא rule-based, בלי ספק חיצוני. |
| `.gitignore` | חוסם `.env`, `.env.*` (מלבד `.env.example`), `appsettings.*.local.json`, `secrets.json`. |

מסקנה: **אין secret production מחויב למאגר.** הפריט היחיד דמוי-סוד הוא סיסמת
קונטיינר מקומי חד-פעמי הזהה לקובץ ה-example.

### 2.3 Tenant scoping

- **Global query filter fail-closed.** `SupportPlatformDbContext.OnModelCreating` —
  `e => tenant.HasTenant && e.TenantId == tenant.TenantId`. אין tenant בהקשר ⇒
  אפס שורות. `TenantQueryFilterTests` מכסה.
- **`TenantAccessGuard.EnsureTenant`** (`Application/Identity`) — קו הגנה שני:
  `tenantId` ריק ⇒ ה-tenant של הקורא; שונה ⇒ `ForbiddenException` (403). נקרא
  ע"י `SearchService` / `MetadataService` / `NlQueryService`. `?tenantId=` /
  `definition.tenantId` מאומתים, לא נאמנים.
- **403 מול 404 — לא מדליף קיום.** גישה לשאילתה שמורה של אחר ⇒ `NotFoundException`
  (404), לא 403.
- **כלל role יחיד** — מחיקת שאילתה שמורה דורשת `Roles.Admin`, נבדק **אחרי** בדיקת
  ה-scope (id מחוץ ל-scope נשאר 404, analyst בתוך scope מקבל 403).

הרצה חיה (S11): `michal` (welfare) → `GET /api/metadata?tenantId=culture-sport-admin`
→ `403 application/problem+json`.

### 2.4 Cross-cutting נוסף שנבדק

- Correlation id: `X-Correlation-Id: s11-demo-42` → אותו ערך חוזר ב-header התגובה
  ומופיע כ-`traceId`.
- `ProblemDetails` על כל נתיב שגיאה (`Content-Type: application/problem+json`).
- MD5 ב-`DbSeeder.DeterministicGuid` — גזירת id דטרמיניסטית, **לא** שימוש
  קריפטוגרפי; לא ממתיק סיסמאות. תקין.

## 3. קוד מת / lint

- `grep` על `TODO|FIXME|HACK|NotImplementedException` ב-`server/src` + `client/src` — **נקי**.
- אין בלוקי קוד מסומנים כהערה.
- `PagePlaceholder` (קומפוננטה לא בשימוש) — הוסרה בסבב האיכות.
- `npm run lint` (oxlint) — נקי. `dotnet build` (`TreatWarningsAsErrors`) — 0 warnings.
- `NlParseResponse.confidence` נקרא בלקוח אך לא מוצג ב-UI — **נשאר בכוונה**: השדה
  חלק מחוזה `api-contract.md` §4; הסרתו מהמודל תפחית נאמנות לחוזה.

## 4. הוספת תחום בלי קוד

בוצעה והודגמה מקצה-לקצה מול API חי — ראו [`EXTENSIBILITY_DEMO.md`](EXTENSIBILITY_DEMO.md).
תמצית: `INSERT` יחיד ל-`reference_domains` → התחום מופיע ב-`/api/metadata`, בטופס,
בתוצאת `/api/search` ובמשפט השאלה — **0 קבצים שונו, בלי build, בלי restart.**

## 5. Docker Compose מ-clone נקי

לא ניתן להריץ `docker compose up` בסביבת הפיתוח הנוכחית (Docker לא מותקן — זו
הסיבה שקיים `run-local.ps1`). נבדק **סטטית**:

- `infra/docker-compose.yml` — שלושה שירותים (`db` / `api` / `client`); סיסמת ה-DB
  מ-`${MSSQL_SA_PASSWORD:?...}` עם הודעת שגיאה מנחה; connection string ל-api נבנה
  מאותו משתנה; `client` מקבל `VITE_API_PROXY_TARGET: http://api:8080`. פורטים
  `5080:8080 / 5173:5173 / 1433:1433` תואמים ל-README.
- `server/Dockerfile` — multi-stage נקי (sdk → aspnet:8.0), מפרסם `src/Api`, לא
  מעתיק `tests/`. `client/Dockerfile` — `node:22-alpine`, `npm ci` מול lockfile
  מועתק, `.dockerignore` חוסם `node_modules`.
- **מגבלה ידועה (מתועדת ב-README):** אין `healthcheck` על `db` ו-`api` משתמש
  ב-`depends_on` פשוט — בהרצה קרה ראשונה ה-api עלול לפנות ל-DB לפני שהוא מוכן.
  `docker compose up` שני, או המתנה קצרה, פותרים. אימות הרצה חיה מ-clone נקי
  נותר לביצוע במכונה עם Docker.

## 6. TEST_PLAN.md

נסקר ועודכן לפי התנהגות שאחרי סבב האיכות (בעיקר: כשל חיפוש מוצג כבאנר inline,
לא כ-toast — תרחיש 1.6). ראו הקובץ.

## 7. פריטים מודעים שלא נסגרו (מחוץ ל-scope של S11 / משאבים)

- הרצת `docker compose up` חיה מ-clone נקי — צריך מכונה עם Docker.
- `code-review (high)` אוטומטי — בוצעה ביקורת ידנית במקום.
- S10 (`DEVOPS.md`, CI, דיאגרמות pipeline/sequence) — שלב נפרד, לא נכלל.
- Tag `submission-v1` — לא נוצר. הסבב על branch לא-ממוזג ולא-committed; ה-tag צריך
  להצביע על ה-commit הסופי ב-`main` אחרי מיזוג. ראו הצ'קליסט בסוף התשובה.
