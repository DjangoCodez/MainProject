import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StorageService {
  get(key: string) {
    if (localStorage) {
      const item = localStorage.getItem(key);
      if (item) {
        const json = JSON.parse(item);

        if (json.expire && json.expire < new Date()) return undefined;

        return json.value;
      }
    }

    return undefined;
  }

  set(key: string, value: any, expire?: Date) {
    if (!localStorage) return;

    try {
      localStorage.setItem(
        key,
        JSON.stringify({ value: value, expire: expire })
      );
    } catch (e: any) {
      if (e.name === 'QuotaExceededError') {
        this.clear();
        return;
      }
      console.error(e);
    }
  }

  remove(key: string) {
    if (localStorage) localStorage.removeItem(key);
  }

  clear() {
    if (localStorage) localStorage.clear();
  }
}
