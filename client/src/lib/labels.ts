/**
 * Resolve display labels from metadata, falling back to the raw value. Both the search form and
 * the results/interpretation views build labels this way — keep it in one place.
 */

/** Reference-list code → its label, or the code itself if the list doesn't carry it. */
export function labelForCode(
  list: readonly { code: string; label: string }[] | undefined,
  code: string | number,
): string {
  return list?.find((item) => item.code === String(code))?.label ?? String(code);
}

/** Filter-field id → its registry label, or the id itself if it isn't registered. */
export function labelForField(
  registry: readonly { id: string; label: string }[],
  id: string,
): string {
  return registry.find((entry) => entry.id === id)?.label ?? id;
}
