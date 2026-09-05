# סקירת הפרויקט — מפת ניווט לבודק

מסמך קצר שנועד לקריאה בכ-5 דקות: מה יש כאן, מאיפה להתחיל, ואיך נעשה שימוש ב-AI
בתהליך. הוא **מפנה** לתיעוד הקיים ואינו מחליף אותו — [`README.md`](../README.md) הוא
מקור האמת.

---

## מטרת הפרויקט

Proof of Concept למערכת חוצת-ארגונים לאחסון וחיפוש בקשות תמיכה ממשלתיות: עובד משרד
מגדיר קריטריוני סינון, קורא את התוצאות כטבלה וכגרף, שומר שאילתות נפוצות, ויכול לשאול
את אותה שאלה בשפה חופשית בעברית. מטלת take-home — הדגשה על חשיבה מערכתית, ארכיטקטורה
ועבודה Full-Stack.

## התחל כאן

| זמן | לקרוא | מה מקבלים |
|---|---|---|
| 5 דק' | המסמך הזה | מפה כללית — מבנה, מסמכים, אופן העבודה |
| +15 דק' | [`README.md`](../README.md) | הרצה, בחירות טכנולוגיות, הנחות, מגבלות, ומטריצת כיסוי הדרישות |
| +30 דק' | [`ARCHITECTURE.md`](ARCHITECTURE.md) + [`DESIGN_QA.md`](DESIGN_QA.md) | השכבות ומנוע השאילתות + Decision Log; ותשובות ל-8 שאלות התכנון |

## מבנה הפרויקט

- **Client** — React 19 + TypeScript + Vite, Ant Design v6 (RTL, `he_IL`), TanStack
  Query. טופס החיפוש נבנה דינמית מ-`GET /api/metadata` — אף שדה, תווית או רשימת ערכים
  אינם מקודדים קשיח.
- **Server** — .NET 8 Web API, ארבע שכבות (`Api` / `Application` / `Domain` /
  `Infrastructure`) בתלות חד-כיוונית; `Application` לא מכיר EF. `QueryDefinition` הוא
  אובייקט קנוני יחיד שכל הצרכנים חולקים.
- **Database** — SQL Server 2022 למימוש; המודל provider-agnostic (PostgreSQL כיעד
  קוד-פתוח). אין קובץ DB מצורף — הסכימה נבנית ממיגרציות EF והנתונים מ-`DbSeeder`
  דטרמיניסטי.
- **Infrastructure** — Docker Compose מרים `db + api + client` בפקודה אחת.

פירוט: [`README.md`](../README.md) → `## מבנה הפרויקט`.

## מסמכים מרכזיים

| מסמך | איזו שאלה הוא עונה |
|---|---|
| [`README.md`](../README.md) | איך מריצים, אילו טכנולוגיות ולמה, מה הנחות העבודה והמגבלות, ומיפוי כל דרישת מטלה להוכחה |
| [`ARCHITECTURE.md`](ARCHITECTURE.md) | שכבות, מודולים, מנוע השאילתות, מודל הנתונים, דיאגרמות ERD/Container, ו-Decision Log בן 14 החלטות |
| [`DESIGN_QA.md`](DESIGN_QA.md) | 8 שאלות התכנון: הרחבה בלי קוד, multi-tenant, RBAC, שאילתות כבדות, מניעת הרצה כפולה, ריבוי ספקי AI, ניטור, תשתיות רוחביות |
| [`DEVOPS.md`](DEVOPS.md) | סביבות DEV/TEST/PROD, CI/CD, ניהול secrets, קונפיגורציה, אסטרטגיית deployment — **תיאור ותכנון בלבד** |
| [`TEST_PLAN.md`](TEST_PLAN.md) | תרחישי בדיקה ידניים ומיפוי קצוות לטסטים אוטומטיים |
| [`REVIEW_NOTES.md`](REVIEW_NOTES.md) | סבב הביקורת: security, קוד מת, lint, וניקוי לפני הגשה |
| [`EXTENSIBILITY_DEMO.md`](EXTENSIBILITY_DEMO.md) | הדגמת הוספת תחום תמיכה חדש מקצה-לקצה, באפס קוד |
| [`contracts/`](contracts/) | החוזים המוקפאים: `query-definition` (+schema), `api-contract`, `metadata-model`, `error-model` |
| [`OVERVIEW_EN.md`](OVERVIEW_EN.md) | תקציר אנגלי לקורא שאינו דובר עברית (תקציר בלבד) |

## הרצה ובדיקה

```bash
cd infra
cp .env.example .env      # ערכו את MSSQL_SA_PASSWORD
docker compose up --build
```

| שירות | כתובת |
|---|---|
| `api` (.NET 8) | http://localhost:5080 · `/health` · Swagger ב-`/swagger` |
| `client` (Vite) | http://localhost:5173 |
| `db` (SQL Server 2022) | `localhost:1433` |

הרצה ידנית, משתמשי seed וזהות `X-User`: [`README.md`](../README.md) → `## הרצה`.
בדיקות: `dotnet test` (158) + `npm test` (56) — 214 בדיקות אוטומטיות; תרחישים ידניים
וקצוות ב-[`TEST_PLAN.md`](TEST_PLAN.md).

## פיתוח בליווי AI (AI-Assisted Engineering)

הפרויקט פותח תוך שימוש ב-Claude כ-**development agent בתוך workflow הנדסי מוגדר** — לא
כתחליף לשיקול הדעת של המפתח. ה-agent עבד בתוך הגבולות של
[`IMPLEMENTATION_PLAN.md`](../IMPLEMENTATION_PLAN.md) (החלטות טכנולוגיות נעולות §2,
Working Agreement §3, סדר השלבים S0–S11 §6), החוזים המוקפאים ב-[`contracts/`](contracts/),
ו-Definition of Done לכל שלב. ההחלטות הארכיטקטוניות, הביקורת והאימות הסופי נשארו אצל
המפתח.

| נכס | מה הוא מגדיר |
|---|---|
| [`CLAUDE.md`](../CLAUDE.md) + [`server/CLAUDE.md`](../server/CLAUDE.md) + [`client/CLAUDE.md`](../client/CLAUDE.md) | גבולות ארכיטקטוניים, מבנה, מוסכמות ורשימת "אל תעשה" לכל צד |
| [`.claude/commands/build-stage.md`](../.claude/commands/build-stage.md) | לולאת שלב יחיד: sanity-check → worktree → כרטיס משימה → **עצירה לאישור** → מימוש → שער DoD → PR |
| [`.claude/skills/new-task`](../.claude/skills/new-task) | כל משימה ב-git worktree מבודד, על branch מ-`origin/main` |
| [`server/.claude/skills/dotnet`](../server/.claude/skills/dotnet) | קונבנציות C#/.NET וגבולות האחריות בין השכבות |
| [`client/.claude/skills/react-components`](../client/.claude/skills/react-components) | רכיבי React קטנים, הפרדת UI מלוגיקה, שכבת API מטופסת |

מה שהמבנה הזה מדגים:

- הגדרת **architectural boundaries** ל-agent — רשימות "אל תעשה", whitelist לשדות סינון,
  בלי שכבות או תבניות מעבר לתוכנית
- קונבנציות ספציפיות לפרויקט, נפרדות ל-Backend ול-Frontend
- **staged development** — שלב אחד = PR אחד, בלי קפיצה קדימה
- **frozen contracts** — `QueryDefinition` וחוזי ה-API הוקפאו ב-S0 ומהווים מקור אמת
- **Definition of Done מוגדר וברור** לכל שלב: build + tests + code review לפני commit;
  ה-agent מבצע את בדיקות האימות שהוגדרו בשלב, והביקורת הסופית נשארת אצל המפתח
- "אל תמציא דרישות" — התוכנית והחוזים הם הגבול

זו תשתית ל-workflow, לא קוד מוצר; היא כלולה כדי שבודק יוכל לראות **כיצד** נבנה
הקוד, לא רק את התוצאה.

## כיסוי דרישות המטלה

מיפוי דרישה-דרישה להוכחה בקוד או במסמך: [`README.md`](../README.md) →
`## כיסוי דרישות המטלה`.
