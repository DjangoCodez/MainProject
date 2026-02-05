import { AsyncPipe } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  HostListener,
  OnDestroy,
  OnInit,
  ViewChild,
  ViewEncapsulation,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import {
  MatAutocomplete,
  MatAutocompleteModule,
} from '@angular/material/autocomplete';
import { TranslatePipe } from '@ngx-translate/core';
import {
  StringKeyOfNumberProperty,
  StringKeyOfStringProperty,
} from '@shared/types';
import { AG_NODE, AG_NODE_PROPS } from '@ui/grid/grid.component';
import { IconModule } from '@ui/icon/icon.module';
import { IAutocompleteEditorParams } from '@ui/grid/interfaces/cell-editor.interface';
import { DataCallback2 } from '@ui/grid/util/column-util';
import { ICellEditorAngularComp } from 'ag-grid-angular';
import { ICellEditorParams } from 'ag-grid-community';
import { map, Observable, of, startWith, Subject, takeUntil } from 'rxjs';

export type AutocompleteCellEditorParams<T, U> = IAutocompleteEditorParams<
  T,
  U
> &
  ICellEditorParams<T, number>;

export function getAutocompleteCacheKey(field: string) {
  return `autocomplete_${field}_items`;
}

type Option = { id: number; name: string };

@Component({
  selector: 'soe-autocomplete-cell-editor',
  imports: [
    ReactiveFormsModule,
    AsyncPipe,
    MatAutocompleteModule,
    IconModule,
    TranslatePipe,
  ],
  templateUrl: './autocomplete-cell-editor.component.html',
  styleUrls: [
    '../../../forms/autocomplete/autocomplete.component.scss',
    './autocomplete-cell-editor.component.scss',
  ],
  encapsulation: ViewEncapsulation.None,
})
export class AutocompleteCellEditor<T extends AG_NODE_PROPS, U>
  implements ICellEditorAngularComp, AfterViewInit, OnDestroy, OnInit
{
  @ViewChild('input') input!: ElementRef<HTMLInputElement>;
  @ViewChild('auto') autocomplete!: MatAutocomplete;

  // Core data
  value = 0;
  initialValue = 0;
  items: U[] = [];
  records: Option[] = [];
  filteredRecords = new Observable<Option[]>();

  // Configuration from params
  callback?: DataCallback2<T, U | undefined>;
  optionIdField!: StringKeyOfNumberProperty<U>;
  optionNameField!: StringKeyOfStringProperty<U>;
  optionDisplayNameField!: StringKeyOfStringProperty<T>;
  source!: (data?: T) => U[];
  allowNavigationFrom?: (value: any, data: AG_NODE<T>) => boolean;
  params!: AutocompleteCellEditorParams<AG_NODE<T>, U>;
  limit?: number = 20;
  showButton = false;

  // State tracking
  localControl = new FormControl<string>('');
  private destroy$ = new Subject<void>();
  private hasTyped = false;

  ngOnInit() {
    this.localControl.valueChanges
      .pipe(
        startWith(this.localControl.value),
        map(value => this._filter(value || '')),
        takeUntil(this.destroy$)
      )
      .subscribe(filtered => {
        this.filteredRecords = of(filtered.slice(0, this.limit));
      });
  }

  ngAfterViewInit() {
    setTimeout(() => {
      this.input.nativeElement.focus();
      this.input.nativeElement.select();
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  agInit(params: AutocompleteCellEditorParams<T, U>): void {
    this.params = params;
    this.value = params.value || 0;
    this.initialValue = this.value;
    this.optionIdField = params.optionIdField;
    this.optionNameField = params.optionNameField;
    this.optionDisplayNameField = params.optionDisplayNameField;
    this.limit = params.limit;
    this.allowNavigationFrom = params.allowNavigationFrom;
    this.source = params.source;
    this.callback = params.updater;
    this.initButton();
    this.loadItems();
  }

  getValue() {
    return this.value;
  }

  isCancelBeforeStart(): boolean {
    if (typeof this.params.disabled === 'boolean') {
      return this.params.disabled;
    }
    if (typeof this.params.disabled === 'function') {
      return this.params.disabled(this.params.data);
    }
    return false;
  }

  isCancelAfterEnd() {
    return false;
  }

  isPopup() {
    return false;
  }

  @HostListener('keydown', ['$event'])
  handleKeys(event: KeyboardEvent) {
    const isNavigationKey =
      event.code === 'Enter' ||
      event.code === 'NumpadEnter' ||
      event.code === 'Tab';

    if (!isNavigationKey && event.code !== 'Escape') {
      this.hasTyped = true;
      return;
    }

    // Stop event from bubbling to AG Grid
    event.stopPropagation();
    event.stopImmediatePropagation();
    if (event.cancelable) event.preventDefault();

    if (event.code === 'Escape') {
      this.cancel();
      return;
    }

    // Select active option before navigating
    if (this.hasTyped) {
      this.selectActiveOption();
    }

    // Navigate if allowed
    const inputValue = this.input.nativeElement.value;
    const isAllowedToNavigate = this.allowNavigationFrom
      ? this.allowNavigationFrom(inputValue, this.params.data!)
      : true;

    if (isAllowedToNavigate) {
      this.focusCurrentCell();
      if (event.shiftKey) {
        this.params.api.tabToPreviousCell(event);
      } else {
        this.params.api.tabToNextCell(event);
      }
    }
  }

  onSelectionChange(event: any, option: Option) {
    if (!event.isUserInput) return;

    this.selectOption(option.id);
    this.params.api.stopEditing(false);

    setTimeout(() => {
      this.focusCurrentCell();
      this.params.api.tabToNextCell();
    }, 100);
  }

  onClearClick() {
    this.value = 0;
    if (this.callback) {
      this.callback(this.params.data, undefined);
    }
    this.params.api.stopEditing(false);
  }

  private initButton() {
    const config = this.params.buttonConfiguration;
    if (!config?.show) {
      this.showButton = false;
      return;
    }

    if (typeof config.show === 'number') {
      this.showButton = this.value === config.show;
    } else if (typeof config.show === 'function') {
      this.showButton = config.show(this.params.data);
    }
  }

  private loadItems() {
    this.items = this.source(this.params.data);
    this.records = this.items.map(x => ({
      id: x[this.optionIdField] as number,
      name: x[this.optionNameField] as string,
    }));

    const currentItem = this.items.find(
      x => x[this.optionIdField] === this.value
    );

    let displayValue = '';
    if (currentItem) {
      displayValue = currentItem[this.optionNameField] as string;
    } else if (this.params.data && this.optionDisplayNameField) {
      displayValue = (this.params.data as any)[
        this.optionDisplayNameField
      ] as string;
    }

    if (displayValue) {
      this.localControl.patchValue(displayValue);
    }
  }

  private _filter(value: string | Option): Option[] {
    const filterValue =
      typeof value === 'object'
        ? value.name.toLowerCase()
        : value.toLowerCase();

    return this.items
      .filter(item =>
        (item[this.optionNameField] as string)
          ?.toLowerCase()
          .includes(filterValue)
      )
      .map(x => ({
        id: x[this.optionIdField] as number,
        name: x[this.optionNameField] as string,
      }));
  }

  private selectActiveOption() {
    if (!this.autocomplete) return;

    const activeOption = this.autocomplete._keyManager?.activeItem;
    if (!activeOption) return;

    const option = this.records.find(r => r.id.toString() === activeOption.id);
    if (option) {
      this.selectOption(option.id);
    }
  }

  private selectOption(id: number) {
    this.value = id;
    const selectedItem = this.items.find(
      x => Number(x[this.optionIdField]) === id
    );
    if (this.callback && selectedItem) {
      this.callback(this.params.data, selectedItem);
    }
  }

  private cancel() {
    this.value = this.initialValue;
    this.params.api.stopEditing(true);
  }

  private focusCurrentCell() {
    const focused = this.params.api.getFocusedCell();
    const currentColId = this.params.column.getColId();

    if (
      !focused ||
      focused.rowIndex !== this.params.rowIndex ||
      focused.column.getColId() !== currentColId
    ) {
      this.params.api.setFocusedCell(this.params.rowIndex, currentColId);
    }
  }
}
