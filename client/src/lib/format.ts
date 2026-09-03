/** Shared `he-IL` value formatting. One definition each — don't re-create these inline. */

const currencyIls = new Intl.NumberFormat('he-IL', {
  style: 'currency',
  currency: 'ILS',
  maximumFractionDigits: 0,
});

/** e.g. `1234` → `‏1,234 ₪` (no agorot). */
export function formatCurrencyIls(value: number): string {
  return currencyIls.format(value);
}

/** Locale-grouped integer, e.g. `1234` → `1,234`. */
export function formatIntHe(value: number): string {
  return value.toLocaleString('he-IL');
}

/** ISO date → `he-IL` short date; `null`/empty → an em dash. */
export function formatDateHe(iso: string | null | undefined): string {
  return iso ? new Date(iso).toLocaleDateString('he-IL') : '—';
}
