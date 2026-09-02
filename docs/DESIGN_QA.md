# שאלות תכנון — מערכת תמיכות רוחבית (PoC)

> **סטטוס: כותרות פרקים בלבד (S0-f).** טיוטת התשובות נכתבת ב-S4, מלוטשת ב-S11.
> כל תשובה: ½–1 עמוד, עם הפניה לקוד. מקור השאלות: `IMPLEMENTATION_PLAN.md` §8.

## 1. הוספת סוגי תמיכה חדשים בלי שינוי קוד

_metadata-driven · `FilterFieldRegistry` · JSON config · הדגמה דרך seed._

## 2. הוספת משרד ממשלתי נוסף

_multi-tenant: `TenantId` + Global Query Filter · config per tenant · תהליך
onboarding._

## 3. הרשאות בין גופים

_RBAC + data-scoping (tenant + domain + ownership) · least-privilege · היכן נאכף
בקוד._

## 4. שאילתות כבדות

_paging חובה · timeouts · אינדקסים · materialized / read-model · async לדוחות
גדולים._

## 5. מניעת הרצה חוזרת של אותה שאילתה

_`definitionHash` קנוני (נורמליזציה → SHA-256) · cache TTL · `last_run` · debounce
ב-client._

## 6. ריבוי ספקי AI

_Strategy + Factory · חוזה `INlQueryTranslator` · בחירה לפי config · fallback
chain._

## 7. ניטור וחקירת תקלות

_structured logging + correlation id · `/health` · metrics · tracing · Audit Log ·
alerting._

## 8. תשתיות רוחביות לשלב ראשון

_auth מרכזי · config service · logging/monitoring · CI/CD + IaC · API gateway ·
מודל metadata גנרי · שכבת AI מופשטת · Audit · ניהול Secrets._
