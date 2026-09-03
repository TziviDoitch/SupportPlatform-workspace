# הדגמה — הוספת תחום תמיכה בלי שינוי קוד

> נדרש ב-`IMPLEMENTATION_PLAN.md` §6 (S11) ו-§7 (קו אדום). התיאוריה:
> [`ARCHITECTURE.md`](ARCHITECTURE.md) §7.1. כאן — הרצה אמיתית עם פלט שנלכד.

**הטענה:** המערכת מונעת-metadata. תחום תמיכה (וכל `code` של `bodyTypes` /
`statuses` / `districts`) הוא **שורת נתונים**, לא קוד. הוספתו זורמת מקצה-לקצה —
`/api/metadata` → הטופס → `QueryDefinition` → ולידציה → מנוע ה-SQL → משפט השאלה —
בלי לגעת באף קובץ `.cs` / `.tsx`, ובלי build או restart.

## התרחיש

משרד התרבות מוסיף תחום **"חינוך"** (`education`).

### מנגנון ההוספה

`reference_domains` מאוכלס ב-`DbSeeder` (`server/src/Infrastructure/Persistence/DbSeeder.cs`
— מערך `Domains`, נתונים לא לוגיקה). שלוש דרכים להוסיף ערך, כולן data-only:

| דרך | מתי | שינוי קוד |
|---|---|---|
| שורה במערך `DbSeeder.Domains` | קלט ראשוני / דמו | עריכת נתונים בקובץ seed (לא לוגיקה) |
| מיגרציית data (`InsertData`) | סביבה מנוהלת | אין — מיגרציה = נתונים |
| `INSERT INTO reference_domains` | מערכת חיה | **אין — SQL בלבד** |

ההרצה למטה משתמשת ב-`INSERT` — הדרך שמוכיחה "אפס קוד, בלי restart".

## ההרצה (נלכדה מול API חי + LocalDB, S11)

### 1. מצב התחלתי — `GET /api/metadata`

```
domains = [{"code":"culture","label":"תרבות"},{"code":"sport","label":"ספורט"}]
registry.supportDomain = {"id":"supportDomain","label":"תחום תמיכה","kind":"codeList",
                          "referenceList":"domains","operators":["in"],"segmentable":true}
```

### 2. שינוי נתונים בלבד (SQL על ה-DB החי, בלי לעצור את ה-API)

```sql
INSERT INTO reference_domains (Code, Label) VALUES ('education', N'חינוך');
-- לצורך תוצאה לא-ריקה בהדגמה: תיוג מחדש של 5 בקשות קיימות
UPDATE TOP (5) support_requests SET SupportDomainCode = 'education'
  WHERE TenantId = 'culture-sport-admin' AND StatusCode = 'approved';
```

### 3. `GET /api/metadata` — אותו תהליך API, בלי restart

```
domains = [{"code":"culture","label":"תרבות"},
           {"code":"education","label":"חינוך"},
           {"code":"sport","label":"ספורט"}]
```

`filterFieldRegistry` **לא השתנה** — `supportDomain` כבר שם. הלקוח: `SearchForm`
מרנדר אופציה אחת לכל רשומה ב-`references.domains`, כך ש"חינוך" מופיע בפקד מעצמו.

### 4. `POST /api/search` עם `filters.supportDomain: ["education"]`

בקשה:

```json
{ "tenantId": "culture-sport-admin",
  "filters": { "supportDomain": ["education"] },
  "segmentation": ["supportYear"],
  "metrics": ["count", "sumAmountApproved"],
  "paging": { "pageNumber": 1, "pageSize": 50 }, "sort": [] }
```

תגובה (`200`):

```
questionText = כמה בקשות תמיכה עם תחום תמיכה: חינוך, בפילוח לפי שנת תמיכה?
rows        = [{"supportYear":2024,"count":2,"sumAmountApproved":184441.34},
               {"supportYear":2025,"count":3,"sumAmountApproved":799994.97}]
```

## מה קרה בלי שנגעו בקוד

| שלב | רכיב | התנהגות |
|---|---|---|
| metadata | `MetadataRepository` | קורא `reference_domains` — `education` מוחזר מיד |
| טופס | `SearchForm` / `CodeListField` | אופציה חדשה מ-`references.domains`, אפס שינוי קומפוננטה |
| בנייה | `buildQueryDefinition` | `filters.supportDomain = ["education"]` כמו כל code |
| ולידציה | `QueryDefinitionValidator` | `supportDomain` ב-registry ✔, צורת ערך = `codeList` ✔ |
| מנוע SQL | `DynamicQueryBuilder` → `CodeListFilterHandler` הקיים | `WHERE SupportDomainCode IN ('education')` |
| משפט | `QuestionTextRenderer` | לוקח את התווית מרשימת הייחוס → "תחום תמיכה: חינוך" |

**קבצים שהשתנו: 0.** Build: לא נדרש. Restart: לא נדרש.

## גבול ההרחבה

- **ערך חדש לשדה קיים** (התרחיש הזה) — נתונים בלבד.
- **שדה סינון חדש מעל `kind` קיים** — נתונים + **שורה אחת** ב-`FilterHandlers.Default`
  (`ARCHITECTURE.md` §7.2).
- **`kind` חדש** (טווח מספרי, טקסט) — תת-מחלקה אחת של `FilterHandler` (`§7.3`).
