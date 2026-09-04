import translations from './he.json';

export type Translations = typeof translations;

export const t = translations;

export const formatMessage = (template: string, values: Record<string, string | number>): string => {
  return Object.entries(values).reduce(
    (result, [key, value]) => result.replace(`{${key}}`, String(value)),
    template,
  );
};
