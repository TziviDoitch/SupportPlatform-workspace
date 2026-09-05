const currencyIls = new Intl.NumberFormat('he-IL', {
  style: 'currency',
  currency: 'ILS',
  maximumFractionDigits: 0,
});

export function formatCurrencyIls(value: number): string {
  return currencyIls.format(value);
}

export function formatIntHe(value: number): string {
  return value.toLocaleString('he-IL');
}

export function formatDateHe(iso: string | null | undefined): string {
  return iso ? new Date(iso).toLocaleDateString('he-IL') : '—';
}
