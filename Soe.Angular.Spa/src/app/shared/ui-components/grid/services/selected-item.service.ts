import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Dict {
  dict: Record<number, boolean>;
}

@Injectable()
export class SelectedItemService<T, R> {
  idField: keyof R | undefined;
  items: Record<string, { value: T; row: R }> = {};
  private selectedItemsBS$ = new BehaviorSubject<Dict>({ dict: {} });
  selectedItems$ = this.selectedItemsBS$.asObservable();

  get changedItems(): string[] {
    return Object.keys(this.items);
  }

  toggle(row: R, idField: string | undefined, value: T) {
    if (!idField) return;

    this.idField = idField as keyof R;
    const itemKey = row[idField as keyof R] as string;

    if (this.items[itemKey]) {
      delete this.items[itemKey];
    } else {
      this.items[itemKey] = { value, row };
    }

    this.emitChanges();
  }

  clear(): void {
    this.items = {};
    this.emitChanges();
  }

  private emitChanges(): void {
    this.selectedItemsBS$.next(this.toDict());
  }

  toDict(): Dict {
    const dict = this.changedItems.reduce((prev, key) => {
      return { ...prev, [key]: this.items[key].value };
    }, {});

    return { dict };
  }
}
