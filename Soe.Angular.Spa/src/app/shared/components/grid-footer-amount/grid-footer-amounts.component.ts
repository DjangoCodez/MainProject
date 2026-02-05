
import {
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  OnDestroy,
  signal,
  SimpleChanges,
} from '@angular/core';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { GridComponent } from '@ui/grid/grid.component';
import { GridFooterAmountComponent } from './grid-footer-amount.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Subscription } from 'rxjs';

type ChangeOnType = 'selection' | 'filter' | 'gridData';
type AmountPart<T> = {
  changeOn: ChangeOnType;
  labelKey: string;
  incVatKey: keyof T;
  exVatKey: keyof T;
};

@Component({
  selector: 'soe-grid-footer-amounts',
  templateUrl: './grid-footer-amounts.component.html',
  imports: [CheckboxComponent, GridFooterAmountComponent],
})
export class GridFooterAmountsComponent<T> implements OnDestroy {
  grid = input<GridComponent<T>>();
  rowData = input.required<BehaviorSubject<T[]>>();
  values = input.required<AmountPart<T>[]>();

  destroy$ = inject(DestroyRef);

  showSumsInclVat = signal(false);
  sums = signal<{ [index: string]: { incVat: number; exVat: number } }>({});

  subscribesOnSelectionChanged = computed(() =>
    this.values().some(v => v.changeOn === 'selection')
  );
  subscribesOnFilterChanged = computed(() =>
    this.values().some(v => v.changeOn === 'filter')
  );
  subscribesOnRowDataUpdated = computed(() =>
    this.values().some(v => v.changeOn === 'gridData')
  );
  private rowDataSubscription: Subscription | null = null;
  private selectionChangedSubscription: { unsubscribe: () => void } | null = null;
  private filterModifiedSubscription: { unsubscribe: () => void } | null = null;

  ngOnInit() {
    this.values().forEach(value => {
      this.sums.set({
        ...this.sums(),
        [value.labelKey]: {
          incVat: 0,
          exVat: 0,
        },
      });
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.grid && this.grid()) {
      const grid = this.grid()!;

      if (this.subscribesOnSelectionChanged() && !this.selectionChangedSubscription) {
        this.selectionChangedSubscription = grid.selectionChanged.subscribe(() => 
          this.summarizeSelectedRows()
        );
      }
      if (!this.filterModifiedSubscription) {
        this.filterModifiedSubscription = grid.filterModified.subscribe(() => 
          this.summarizeFilteredAmount()
        );
      }
    }
    if (changes.rowData && this.rowData() && !this.rowDataSubscription) {
      this.rowDataSubscription = this.rowData()
        .pipe(takeUntilDestroyed(this.destroy$))
        .subscribe(data => this.runAllSummaries(data));
    }
  }

  private runAllSummaries(allRows: T[]) {
    this.summarizeSelectedRows();
    this.summarizeFilteredAmount();
    this.summarizeTotalAmount(allRows);
  }

  private summarizeSelectedRows() {
    if (!this.subscribesOnSelectionChanged() || !this.grid()) return;
    const rows = this.grid()!.getSelectedRows();
    const summary = this.summarize('selection', rows);
    this.sums.set({
      ...this.sums(),
      ...summary,
    });
  }

  private summarizeFilteredAmount() {
    if (!this.subscribesOnFilterChanged() || !this.grid()) return;
    setTimeout(() => {
      const rows = this.grid()!.getFilteredRows();
      const summary = this.summarize('filter', rows);
      this.sums.set({
        ...this.sums(),
        ...summary,
      });
    }, 0);
  }

  private summarizeTotalAmount(allRows: T[]) {
    if (!this.subscribesOnRowDataUpdated() || !allRows) return;
    const summary = this.summarize('gridData', allRows);
    this.sums.set({
      ...this.sums(),
      ...summary,
    });
  }
  private summarize(type: ChangeOnType, rows: T[]) {
    const sums: { [key: string]: { incVat: 0; exVat: 0 } } = {};
    const parts = this.values().filter(r => r.changeOn === type);
    parts.forEach(part => {
      sums[part.labelKey] = {
        incVat: 0,
        exVat: 0,
      };
    });

    for (const row of rows) {
      for (const part of parts) {
        const incVat = (row[part.incVatKey] ?? 0) as number;
        const exVat = (row[part.exVatKey] ?? 0) as number;
        sums[part.labelKey].incVat += incVat;
        sums[part.labelKey].exVat += exVat;
      }
    }
    return sums;
  }

  ngOnDestroy(): void {
    this.selectionChangedSubscription?.unsubscribe();
    this.filterModifiedSubscription?.unsubscribe();
    this.rowDataSubscription?.unsubscribe();
  }
}
