export function upsert<T>(
  arr: T[],
  obj: T,
  prop: keyof T,
  addFirst: boolean
): T[] {
  const foundIndex = arr.findIndex((item: T) => item[prop] === obj[prop]);
  if (foundIndex === -1) {
    addFirst ? arr.unshift(obj) : arr.push(obj);
  } else {
    arr[foundIndex] = obj;
  }

  return [...arr];
}

export function deleteItem<T>(arr: T[], obj: T, prop: keyof T): T[] {
  arr = arr.filter(item => item[prop] !== obj[prop]);
  return [...arr];
}

/**
 *
 * @param arr simple array (such as SmallGenericType)
 * @returns Modifies the array in place.
 * Takes an object from the array as reference, copies the keys and sets all number/string values to 0/''.
 * Beware! Empty arrays or nested objects are not handled.
 */
export function addEmptyOption<T>(arr: T[]): void {
  if (!arr || arr.length === 0) return;
  const item = arr[0];
  const newItem: any = {};
  for (const key in item) {
    const type = typeof item[key];
    if (type === 'string') {
      newItem[key] = ' ';
    } else if (type === 'number') {
      newItem[key] = 0;
    }
  }
  arr.unshift(newItem);
}
