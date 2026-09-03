# תוכנית מימוש – מערכת תמיכות רוחבית (PoC)

מטלת Take‑Home, מפתח יחיד. מסמך יחיד: כללים + מבנה עבודה + משימות + הוראות התחלה.
**בשלב זה לא כותבים קוד** עד אישור. המסמך נכנס ל‑Git כ‑commit ראשון.

---

## 1. מה נמדד

**מסמכים:** ARCHITECTURE.md + 8 תשובות תכנון + README הם רכיב משמעותי בהגשה. נכתבים **במקביל** לפיתוח, לא בסוף. כל שלב (S1–S10) עדכן את הפרק הרלוונטי תוך כדי.

**מה בולט בבדיקת קוד:**
- Backend מובנה: שכבות, Repository, DI, Error handling, Logging, Validation — נכונות המבנה > כמות פיצ'רים.
- מנוע השאילתות: `QueryDefinition` קנוני + חיפוש דינמי בטוח (whitelist) + פילוחים + ניסוח שאלה קריא — לב הלוגיקה.
- הפשטת AI (Strategy+Factory), Saved Queries מלא, Audit — חייב "לעבוד", לא חייב להיות חכם.
- Client: טופס דינמי מ‑metadata, טבלה, גרף, NL — נקי, עקבי, פונקציונלי.

**עיקרון מנחה:** `QueryDefinition` הוא אובייקט קנוני יחיד — הטופס בונה אותו, ה‑NL parser מפיק אותו, השאילתה השמורה *היא* הוא, מנוע ה‑SQL מתרגם אותו, מנסח השאלה קורא אותו.

---

## 2. החלטות טכנולוגיות (נעולות)

| תחום | בחירה | הערה |
|---|---|---|
| Backend | **.NET 8 Web API** (C#) | אוצר המילים של המטלה (Solution / Repository / Services) |
| שכבת נתונים | **EF Core 8** | הרכבת `IQueryable` דינמית עם whitelist |
| DB (מימוש) | **SQL Server** (`mcr.microsoft.com/mssql/server`) | אילוץ מעשי — היכרות/רישוי קיים. JSON נשמר כ‑`nvarchar(max)` + `ToJson()` |
| DB (במסמך הארכיטקטורה) | לתאר **PostgreSQL** כיעד קוד‑פתוח מועדף | המודל provider‑agnostic; מעבר = החלפת provider + connection string. לנמק למה SQL Server ל‑PoC |
| Client | **React + TypeScript + Vite** | – |
| State | **TanStack Query** + hooks מקומיים | בלי Redux/מנגנון כבד |
| ספריית עיצוב | **Ant Design (antd)** | רכיבים מוכנים + תמיכת RTL (`ConfigProvider direction="rtl"`) |
| גרפים | **Chart.js** (`react-chartjs-2`) | גרף עמודות בסיסי |
| הפשטת AI | `INlQueryTranslator` + Factory לפי `configuration` | נמסרים `Mock` + `RuleBased`; LLM אמיתי = seam בלבד |
| Logging | **Serilog** (Console) + Correlation Id | בלי קונפיג רב‑sinks |
| Validation | **FluentValidation** על `QueryDefinition` | – |
| Auth | JWT מינימלי / `X-User` header + tenant filter + role check אחד | IdP אמיתי = תשובה במסמך |
| Container | **Docker Compose** (`mssql + api + client`) | "הרצה בפקודה אחת" |
| מקור אמת לקוד | **GitHub** — repo פרטי, Conventional Commits, tag `submission-v1`, CI ב‑GitHub Actions | |

---

## 3. כללי ברזל (Working Agreement) — גוברים על כל השאר

1. **זמן לפני שאפתנות.** פועלים לפי סדר S0→S11 ולפי סולם ה‑Fallback (§7). S1–S7 גמורים ומלוטשים עדיפים על S0–S11 חצי‑אפויים.
2. **אפס over‑engineering** מעבר למה שכתוב כאן. דפוס/שכבה/כלי שלא מופיע במפורש — לא נכנס. בספק → הגרסה הפשוטה.
3. **Vertical slice עובד קודם:** `metadata → QueryDefinition → /search → results` רץ מקצה‑לקצה לפני כל שאר הפיצ'רים.
4. **הליבה לא נגזרת:** `QueryDefinition` קנוני + `DynamicQueryBuilder` עם whitelist מ‑`FilterFieldRegistry` + extensibility מבוסס‑metadata — תקציב מלא תמיד, גם ב‑Fallback קיצוני.
5. **Feature עובד > infrastructure/תיעוד** בהתנגשות על זמן. המסמכים נכתבים תוך כדי, לא על חשבון slice שבור.
6. **לא ממציאים דרישות.** מממשים רק מה שכתוב במטלה המקורית. הרחבות שלא נדרשו → לכל היותר נזכרות ב‑`DESIGN_QA.md` כ"צעד הבא".
7. **`CLAUDE.md` בכל פרויקט (English):** `server/CLAUDE.md` ו‑`client/CLAUDE.md` — פקודות build/run/test, מבנה תיקיות, מוסכמות שמות, do/don't, Git workflow, skills & tools. `CLAUDE.md` בשורש מפנה לשניהם. נוצר כמשימה ראשונה ב‑S0‑b / S0‑c ומתעדכן ב‑DoD של כל שלב.
8. **Definition of Done לכל שלב:** קוד + טסטים ממוקדים + עדכון הפרק הרלוונטי ב‑`ARCHITECTURE.md`/`DESIGN_QA.md` + `dotnet test` / `npm test` ירוקים + `code-review` (medium) ללא ממצאי correctness פתוחים + `CLAUDE.md` מעודכן.
9. **בתחילת כל שלב:** לוודא שהמערכת עולה (`docker compose up`, `/health` מגיב, Client נטען) *לפני* שכותבים. נשבר בשלב קודם → מתקנים קודם.

---

## 4. ארכיטקטורת יעד (תמצית)

**Backend — 4 שכבות (+טסטים):**
```
Api            Controllers, Swagger, Auth, ProblemDetails, request validation
Application    Use‑case Services, DTOs, Validators, QueryDefinition,
              QuestionTextRenderer, INlQueryTranslator, caching/dedup
Domain        Entities, value objects, FilterFieldRegistry, חוזי metadata
Infrastructure EF Core DbContext, Repositories, DynamicQueryBuilder,
              AI providers impl, Serilog, Audit, Migrations + Seed
```
כיוון תלות חד‑כיווני. `Application` לא מכיר EF.

**מודולים אנכיים:** Metadata · Search · SavedQueries · NlQuery · Audit · Identity(stub).

**Client:**
```
src/
  api/        http client, interceptor (ProblemDetails→notification), config
  shared/     antd wrappers, RTL, chart wrapper, table wrapper, design tokens
  features/
    search/         טופס דינמי מ‑metadata + פאנל "שאלה קריאה"
    results/        טבלה + גרף
    saved-queries/  list / save / edit / delete / rerun
    nl-query/       free‑text → פאנל פרשנות → הרץ / שמור
  state/      hook + TanStack Query לכל feature
```

---

## 5. חוזי ליבה (מוקפאים ב‑S0 — מצורפים לכל כרטיס משימה)

נשמרים ב‑`docs/contracts/`: `query-definition.md` (+`.schema.json`), `api-contract.md`, `metadata-model.md`, `error-model.md`.

### QueryDefinition (טיוטה)
```jsonc
{
  "tenantId": "culture-sport-admin",
  "filters": {
    "submittingBodyType": ["association"],      // codes מ‑reference_body_types
    "supportDomain":      ["culture"],          // codes מ‑reference_domains
    "status":            ["approved"],          // codes מ‑reference_statuses
    "year": { "type": "range", "from": 2023, "to": 2025 }, // או {"type":"single","value":2025}
    "district":          ["north"]              // codes מ‑reference_districts
    // כל מפתח חייב להופיע ב‑filter_field_registry (whitelist)
  },
  "segmentation": ["year"],   // תת‑קבוצה של: district | bodyType | year | supportDomain
  "metrics": ["count"],       // count (Must) | sumAmountApproved (Should)
  "paging": { "pageSize": 50, "pageNumber": 1 },
  "sort": [ { "field": "supportYear", "direction": "asc" } ]
}
```

### REST API
| Method | Path | תשובה |
|---|---|---|
| GET | `/api/metadata?tenantId=` | רשימות ייחוס + `filter_field_registry` (מזין את הטופס הדינמי) |
| POST | `/api/search` | `{ questionText, rows[], aggregations[], page, executionMeta{durationMs,rowCount,cacheHit,definitionHash} }` |
| POST | `/api/nl-queries/parse` | `{ definition, interpretationText, confidence, unresolved[] }` |
| GET/POST/PUT/DELETE | `/api/saved-queries[/{id}]` | CRUD (scoped ל‑owner/tenant) |
| POST | `/api/saved-queries/{id}/run` | כמו `/search` |
| POST | `/api/auth/login` | JWT (seed users) |

### מודל ישויות
`support_requests` · `submitting_bodies` · `reference_domains/body_types/statuses/districts` · `filter_field_registry` · `tenants` · `saved_queries(definition, definition_hash, last_run_at, last_run_row_count)` · `audit_log(user, action, entity_type, entity_id, occurred_at, correlation_id, payload)`

### מודל שגיאה — RFC 7807 ProblemDetails
`{ type, title, status, detail, traceId, errors{} }`

---

## 6. תוכנית ביצוע — S0→S11

**מקרא:** ⭐ = לא לקצץ. 🚩 = נקודת הגשה מתגוננת. שעות: **מלא / רזה**.
לכל שלב לוקחים את הכרטיסים, מצרפים `docs/contracts/*`, מוסרים חתיכה‑חתיכה, וסוגרים לפי DoD (§3.8).

### S0 · Skeleton + חוזים — 3.0 / 2.0
- **a** repo: `git init`, מבנה `server/ client/ docs/ infra/`, `.gitignore`, `.editorconfig`, `LICENSE` (MIT), `README.md` שלד, `CLAUDE.md` (root, English). → *DoD: commit ראשון נדחף ל‑GitHub.*
- **b** Solution: 4 פרויקטים + טסטים, `Directory.Build.props` (nullable, warnings‑as‑errors), `/health`, `server/CLAUDE.md` (English — build/test commands, project structure, conventions, Git workflow, skills). → *DoD: `dotnet build` עובר, `/health`=200.*
- **c** Client: Vite + React‑TS, antd + `ConfigProvider` RTL, routing ל‑4 מסכים ריקים, `client/CLAUDE.md` (English — npm scripts, folder structure, conventions, component patterns, state management). → *DoD: `npm run dev` עולה, ניווט עובד.*
- **d** חוזי ליבה ב‑`docs/contracts/` (§5). → *DoD: 4 מסמכים + schema ולידית.*
- **e** Docker Compose (`mssql + api + client`) + `appsettings*.json` + `.env.example`. → *DoD: `docker compose up` מרים 3 שירותים, Client מדבר עם API.* (נלחצת בזמן? → Fallback: הרצה ידנית ב‑README.)
- **f** `docs/ARCHITECTURE.md` + `docs/DESIGN_QA.md` — כותרות פרקים בלבד.

### S1 · מודל נתונים + Metadata + Seed — 3.5 / 2.5
ישויות + `DbContext` + `TenantId` + Global Query Filter · טבלאות ייחוס + `FilterFieldRegistry` · migration ראשונה · `DbSeeder` דטרמיניסטי (2 tenants, ~500 `support_requests` בהתפלגות מכוונת, ~40 גופים, 3 משתמשי seed) · `GET /api/metadata`.
*רזה: tenant יחיד, ~200 שורות.*

### S2 ⭐ · מנוע השאילתות — 7.0 / 5.0
`QueryDefinition` + FluentValidation מול ה‑Registry · `DynamicQueryBuilder` — `IQueryable` דרך whitelist בלבד (שוויון/IN/טווח שנים), **שדה לא ב‑Registry נדחה** · Aggregation לפי `segmentation` (count) · `QuestionTextRenderer` בעברית — **חייב לשחזר את משפט הדוגמה במטלה** · `SearchService` + `POST /api/search` · כאן מוזרקים ProblemDetails + Serilog + Correlation Id.
*רזה: segmentation לשדה יחיד, count בלבד, תבניות משפט פשוטות.*

### S3 · Vertical slice ב‑Client — 4.0 / 3.0
`api/` core + interceptor · טופס חיפוש דינמי מ‑`/api/metadata` (בונה `QueryDefinition`) · פאנל "שאלה קריאה" (live) · טבלת תוצאות (paging/מיון, מצבי loading/empty/error).
**🚩 אחרי S3+S4 יש הגשה מתגוננת.**

### S4 ⭐ · מסמכי ליבה — draft — 4.0 / 3.0
`ARCHITECTURE.md`: שכבות, חלוקת אחריות, מודולריות, **הרחבה עתידית עם דוגמת metadata קונקרטית**, פרק Backend, פרק DB (+נימוק SQL Server ל‑PoC / PostgreSQL כיעד), פרק Client, **Decision Log** · דיאגרמות Mermaid: ERD + Container · טיוטת 8 התשובות ב‑`DESIGN_QA.md`.

### S5 · Saved Queries + Audit + Dedup — 4.0 / 2.5
`SavedQuery` CRUD + scoping · `POST /{id}/run` + עדכון `last_run` · `IAuditService` (קריאות מפורשות ב‑handlers, לא interceptor) · `definitionHash` (נורמליזציה קנונית → SHA‑256) + `IMemoryCache` (TTL) → `executionMeta.cacheHit` · מסך Saved Queries ב‑Client.
*רזה: בלי cache (תשובה ב‑`DESIGN_QA` §5); audit רק ל‑mutations + search.*

### S6 · NL Query + הפשטת AI — 3.5 / 2.0
`INlQueryTranslator` + `NlQueryTranslatorFactory` (בחירה לפי config) · `MockNlQueryTranslator` (דוגמאות המטלה → `QueryDefinition` ידוע) · `RuleBasedNlQueryTranslator` — מילון (סטטוס/סוג‑גוף/תחום/שנה+טווח/מחוז/מילות פילוח) + `unresolved[]` **— timebox 1.5ש׳** · `POST /api/nl-queries/parse` + audit · מסך NL ב‑Client (פרשנות → הרץ → שמור).
*רזה: Mock בלבד + ההפשטה + פסקה "הצעד הבא".*

### S7 · גרף + UI polish — 3.0 / 1.5
גרף עמודות (Chart.js) על `aggregations`, מתחלף לפי הפילוח · layout עקבי, מצבים אחידים, RTL, אפס console errors · Swagger + `api.http` · שדה `supportYear` בטופס: `Select` של השנים מה‑metadata (או `InputNumber` עם `min`/`max`/`precision=0`) במקום שני שדות מספר חופשיים.
*רזה: גרף סטטי יחיד, מיזוג מסך NL לתוך החיפוש, עיצוב מינימלי אבל עקבי.*

### S8 · Repos/Cross‑cutting hardening + Auth stub — 2.0 / 1.0
`IRepository<T>` + 2–3 ספציפיים (ניקוי הזרקות זמניות מ‑S2) · `ICurrentUser` + tenant scoping + בדיקת role אחת שמדגימה הפרדת גופים.
*רזה: `X-User` header במקום JWT.*

### S9 · בדיקות — 2.5 / 1.5
Unit: `DynamicQueryBuilder` (כל פילטר + **דחיית שדה זר**), Aggregation, `QuestionTextRenderer` (משפט הדוגמה), parser, יציבות `definitionHash` · happy‑path integration אחד (login→metadata→search→save→run→nl) · `docs/TEST_PLAN.md` ידני (כולל edge cases: טווח הפוך, אפס תוצאות, NL לא מזוהה, מחיקת שאילתה של אחר).
*רזה: Unit רק ל‑builder + renderer; בלי integration; `TEST_PLAN.md` נשאר.*

### S10 · DevOps doc + דיאגרמות נותרות + CI — 2.0 / 1.0
`docs/DEVOPS.md`: DEV/TEST/PROD, CI/CD stages, Secrets, ניהול קונפיגורציה, Deployment + rollback + דיאגרמת pipeline · Sequence diagram (חיפוש + NL) · `.github/workflows/ci.yml` (build+test — לא חייב לרוץ בענן).
*רזה: `DEVOPS.md` חצי עמוד, בלי Sequence.*

### S11 · Polish + Review + README + הגשה — 3.5 / 2.5
`code-review` (high) + `security-review` (injection ב‑builder, Secrets, scoping) + תיקונים + `docs/REVIEW_NOTES.md` · `README.md` מלא (§8) · **הדגמת "הוספת תחום תמיכה בלי שינוי קוד"** דרך seed/JSON + תיעוד · ניקוי סודות/קוד מת, lint נקי · `docker compose up` מ‑clone נקי עובד · מעבר על `TEST_PLAN.md` · tag `submission-v1`.

**סה"כ: ~44 / ~28ש׳.** יעד ריאלי: **32–36ש׳** (מלא S0–S6, רזה S7–S11). המסמכים נכתבים תוך כדי.

---

## 7. Fallback — סולם ויתורים (מהזול לכואב)

1. `DEVOPS.md` לחצי עמוד + בלי Sequence diagram (משאירים ERD+Container, `ci.yml` נשאר).
2. NL → Mock בלבד (משאירים `INlQueryTranslator`+Factory + פסקה "הצעד הבא").
3. בלי cache/hash — עונים על שאלה 5 ב‑`DESIGN_QA.md` בלבד.
4. UI פונקציונלי בלבד; ממזגים מסך NL לחיפוש; עיצוב מינימלי אך **עקבי**.
5. Unit חלקי — רק `DynamicQueryBuilder` (כולל דחיית שדה זר) + `QuestionTextRenderer`; בלי integration.
6. `X-User` header במקום JWT.
7. בלי Docker Compose — הוראות הרצה ידניות ב‑README.
8. tenant יחיד ב‑seed — multi‑tenant מודגם דרך `TenantId` + global filter + פסקה במסמך.

### קו אדום — לא מוותרים על:
`QueryDefinition` + `DynamicQueryBuilder` whitelist · `ARCHITECTURE.md` + Decision Log · 8 התשובות · `README.md` + הרצה מ‑clone נקי · הדגמת "הוספת תחום בלי קוד" · slice עובד טופס→חיפוש→טבלה.

---

## 8. README — חייב לכלול (לפי "אופן ההגשה" במטלה)

הוראות הרצה (Docker Compose + ידני + משתמשי seed + כתובות) · הסבר ארכיטקטוני (תקציר + קישור ל‑`ARCHITECTURE.md`) · בחירות טכנולוגיות (§2 + נימוקים) · מבנה הפרויקט · הנחות עבודה · מגבלות (מה לא מומש ולמה) · מסד נתונים (מיגרציות/seed) · קישורים ל‑`DESIGN_QA.md` / `DEVOPS.md` / `TEST_PLAN.md`.

### 8 שאלות התכנון (`DESIGN_QA.md`, ½–1 עמ' לכל אחת, עם הפניה לקוד)
1. הוספת סוגי תמיכה חדשים בלי שינוי קוד → metadata‑driven + `FilterFieldRegistry` + JSON config.
2. הוספת משרד ממשלתי נוסף → multi‑tenant (`TenantId` + global filter) + config per tenant + onboarding.
3. הרשאות בין גופים → RBAC + data‑scoping (tenant + domain + ownership), least‑privilege, היכן נאכף.
4. שאילתות כבדות → paging חובה, timeouts, אינדקסים, materialized/read‑model, async לדוחות גדולים.
5. מניעת הרצה חוזרת → `definitionHash` קנוני + cache TTL + `last_run` + debounce ב‑client.
6. ריבוי ספקי AI → Strategy+Factory, חוזה `INlQueryTranslator`, config‑selectable, fallback chain.
7. ניטור וחקירת תקלות → structured logging + correlation id, `/health`, metrics, tracing, Audit Log, alerting.
8. תשתיות רוחביות לשלב ראשון → auth מרכזי, config service, logging/monitoring, CI/CD + IaC, API gateway, מודל metadata גנרי, שכבת AI מופשטת, Audit, ניהול Secrets.

---

## 9. איך מתחילים — לפי הסדר (מבחינתך)

### פעם אחת בהתחלה
1. **GitHub:** צרי repo פרטי ריק (`support-platform`). Clone ל‑`C:\Projects\SupportPlatform-workspace`.
2. **commit ראשון:** הוסיפי את `IMPLEMENTATION_PLAN.md`, דחפי ל‑`main`.
   ```bash
   cd C:\Projects\SupportPlatform-workspace && git init && git add IMPLEMENTATION_PLAN.md && git commit -m "docs: add implementation plan" && git branch -M main && git remote add origin <GITHUB_URL> && git push -u origin main
   ```
3. ההחלטות הטכנולוגיות (§2) נעולות — לא חוזרים אליהן.

### הלולאה לכל שלב (S0 → S11)
לכל שלב, בסדר:
1. **בדיקת שפיות:** `docker compose up` → `/health` ירוק + Client נטען. שבור? מתקנים לפני שממשיכים.
2. **מוסרים חתיכה לעובד:** כרטיס אחד (תת‑שלב, למשל S0‑a) לפי התבנית ב‑§10 + מצרפים את `docs/contracts/*`.
3. **בודקים מול DoD** (§3.8): build/test ירוקים, מסמך רלוונטי עודכן, `CLAUDE.md` עודכן, `code-review` medium נקי.
4. **Commit** עם Conventional Commit (`feat:`, `fix:`, `docs:`, `test:`), דוחפים ל‑`main`.
5. שלב הבא.

### סדר המסירה
```
S0‑a → S0‑b → S0‑c → S0‑d → S0‑e → S0‑f      (skeleton + חוזים)
S1  (מודל + seed)  →  S2 ⭐ (מנוע שאילתות)  →  S3 (slice ב‑Client)  →  S4 ⭐ (מסמכים draft)
── 🚩 יש הגשה מתגוננת ──
S5 (Saved+Audit)  →  S6 (NL+AI)  →  S7 (גרף+polish)  →  S8 (hardening+auth)
S9 (בדיקות)  →  S10 (DevOps+CI)  →  S11 (review+README+הגשה)
```
אם נגמר הזמן: עוצרים איפה שאפשר, מפעילים Fallback (§7) על השלבים שנשארו, מוודאים שהקו האדום שלם.

### מה לתת לעובד עכשיו
כרטיס **S0‑a** בלבד (לפי §10), עם המשפט: *"אל תיגע ב‑S0‑b ואילך. סיים לפי ה‑DoD, עצור, דווח."*

---

## 10. תבנית כרטיס משימה (להעברה לעובד)

```markdown
### משימה: <S?-? כותרת>

הקשר: <2–3 שורות — איפה זה בארכיטקטורה, למה חשוב>
תלויות (חייבות להיות מוכנות): <S?-?>

קלט:
- docs/contracts/*  (query-definition, api-contract, metadata-model, error-model)
- <קבצים/מודולים קיימים רלוונטיים>

פלט (Deliverable):
- <קבצים/מחלקות/endpoints מדויקים>
- טסטים: <אילו>
- עדכון מסמך: <ARCHITECTURE.md פרק ? / DESIGN_QA.md שאלה ? / אין>
- עדכון server/CLAUDE.md או client/CLAUDE.md (English)

Definition of Done:
- [ ] <בדיקה פונקציונלית ספציפית>
- [ ] dotnet build / npm run build עובר
- [ ] טסטים רלוונטיים ירוקים
- [ ] code-review (medium) ללא ממצאי correctness פתוחים
- [ ] אין סודות/קוד מת
- [ ] server/CLAUDE.md או client/CLAUDE.md (English) עודכן עם פקודות/מוסכמות החדשות
- [ ] המערכת עדיין עולה (docker compose up / /health)

מאמץ: <S≈1–2ש׳ / M≈3–5ש׳ / L≈יום>   עדיפות: <Must/Should/Nice>
מוסכמות: ראה server|client/CLAUDE.md
היקף: בצע רק את המשימה הזו. אל תתחיל את הבאה. סיים, עצור, דווח.
```

---

*סוף. אחרי אישור — מתחילים מ‑S0‑a.*
