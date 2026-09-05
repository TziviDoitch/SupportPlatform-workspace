// Resolve display labels from metadata, falling back to the raw value.

export function labelForCode(
  list: readonly { code: string; label: string }[] | undefined,
  code: string | number,
): string {
  return list?.find((item) => item.code === String(code))?.label ?? String(code);
}

export function labelForField(
  registry: readonly { id: string; label: string }[],
  id: string,
): string {
  return registry.find((entry) => entry.id === id)?.label ?? id;
}
