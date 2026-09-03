/** Formatare sume cu cifre tabulare + cod moneda (ex. "1.234,56 RON"). */
export const formatMoney = (amount: number, currencyCode?: string | null): string => {
  const formatted = new Intl.NumberFormat('ro-RO', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
  return currencyCode ? `${formatted} ${currencyCode}` : formatted;
};

/** Data afisata user-friendly (ex. "3 iun. 2026"). Accepta "YYYY-MM-DD" sau ISO. */
export const formatDate = (iso: string): string =>
  new Intl.DateTimeFormat('ro-RO', {
    day:   'numeric',
    month: 'short',
    year:  'numeric',
  }).format(new Date(iso));

/** "YYYY-MM-DD" pentru o data (default azi), in fusul orar local. */
export const toIsoDate = (date: Date = new Date()): string => {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
};

/** Intervalul [prima zi, ultima zi] al unei luni, ca string-uri ISO. */
export const monthRange = (year: number, monthIndex: number): { from: string; to: string } => ({
  from: toIsoDate(new Date(year, monthIndex, 1)),
  to:   toIsoDate(new Date(year, monthIndex + 1, 0)),
});
