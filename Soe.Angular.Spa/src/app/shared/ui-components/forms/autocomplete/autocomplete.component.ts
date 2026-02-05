import {
  FormControl,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
} from '@angular/forms';
import { ValueAccessorDirective } from '../directives/value-accessor.directive';
import {
  AfterViewInit,
  Component,
  effect,
  ElementRef,
  input,
  OnInit,
  output,
  signal,
  viewChild,
  OnDestroy,
  inject,
  Injector,
  computed,
} from '@angular/core';
import {
  StringKeyOfNumberProperty,
  StringKeyOfStringProperty,
} from '@shared/types/keyof-types';
import { map, startWith, takeUntil } from 'rxjs/operators';
import { Subscription } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  MatAutocompleteModule,
  MatAutocompleteTrigger,
} from '@angular/material/autocomplete';
import { CommonModule } from '@angular/common';
import { LabelComponent } from '@ui/label/label.component';
import { TranslatePipe } from '@ngx-translate/core';

type FilterFunction = (
  values: SmallGenericType[],
  value: string | SmallGenericType
) => SmallGenericType[];

@Component({
  selector: 'soe-autocomplete',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    LabelComponent,
    TranslatePipe,
  ],
  templateUrl: './autocomplete.component.html',
  styleUrls: ['./autocomplete.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: AutocompleteComponent,
    },
  ],
})
export class AutocompleteComponent<T>
  extends ValueAccessorDirective<T>
  implements OnInit, AfterViewInit, OnDestroy
{
  inputId = input<string>(Math.random().toString(24));
  labelKey = input('');
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  hasLabel = computed(() => {
    return (
      this.labelKey() ||
      this.secondaryLabelKey() ||
      this.secondaryLabelPrefixKey() ||
      this.secondaryLabelPostfixKey()
    );
  });
  placeholderKey = input('');
  inline = input(false);
  alignInline = input(false);
  width = input(0);
  dropUp = input(false);
  items = input<Array<T>>([]);
  optionIdField = input.required<StringKeyOfNumberProperty<T>>(); // Not sure why these are not as simple as in the select component?
  optionNameField = input.required<StringKeyOfStringProperty<T>>(); // Not sure why these are not as simple as in the select component?
  optionDisplayNameField = input<StringKeyOfStringProperty<T>>(); // Not sure why these are not as simple as in the select component?
  limit = input(20);
  waitPopulate = input(0); // Wait for population of items before showing the options
  defaultEmptyValue = input<number | undefined>(undefined);
  filterInput = input<FilterFunction | undefined>(undefined);
  manualReadOnly = input(false);

  isReadOnly = computed(() => this.readOnly() || this.manualReadOnly());

  valueChanged = output<T | undefined>();

  autoCompleteTrigger = viewChild<MatAutocompleteTrigger | undefined>(
    MatAutocompleteTrigger
  );
  inputfield = viewChild<ElementRef>('inputfield');
  content = viewChild<ElementRef>('content');
  hasContent = signal<boolean>(false);

  localControl = new FormControl<string>('');
  private valueChangesSub?: Subscription;

  records: { id: number; name: string }[] = [];
  filteredRecords = signal<{ id: number; name: string }[]>([]);
  lastSelectedRecord?: { id: number; name: string } | null = null;
  lastClickedElement: Element | null = null;
  currentOptionIndex = -1;

  itemsChangeEffectRef = effect(() => {
    const _items = this.items();
    this.records = _items
      ? _items?.map(x => ({
          id: x[this.optionIdField()] as number,
          name: x[this.optionNameField()] as string,
        }))
      : [];
    this.localControl.setValue('');
  });

  constructor() {
    super(inject(Injector));
  }

  ngOnInit(): void {
    super.ngOnInit();

    document.addEventListener('mousedown', event => {
      this.lastClickedElement = event.target as Element;
    });

    this.valueChangesSub = this.localControl.valueChanges
      .pipe(
        startWith(''),
        map(value => {
          return this.filterFunction(this.records, value || '');
        })
      )
      .subscribe(x => {
        this.filteredRecords.set(x.slice(0, this.limit()));
      });

    this.control.valueChanges.pipe(takeUntil(this._destroy$)).subscribe(x => {
      this.lastSelectedRecord = this.records.find(
        record => record[this.optionIdField()] === x
      );
      this.setDisabledStateForLocalControl();
      this.showLocalDisplayName();
    });
  }

  ngAfterViewInit(): void {
    super.ngAfterViewInit();

    this.inputfield()?.nativeElement.addEventListener(
      'keydown',
      this.onKeyDown.bind(this)
    );

    if (this.elemHasContent(this.content())) this.hasContent.set(true);
  }

  ngOnDestroy(): void {
    super.ngOnDestroy();
    this.itemsChangeEffectRef?.destroy();
    this.valueChangesSub?.unsubscribe();
  }

  onKeyDown(event: KeyboardEvent): void {
    const options = this.filteredRecords();
      switch (event.key) {
        case 'ArrowDown':
          this.currentOptionIndex =
            (this.currentOptionIndex + 1) % options.length;
          event.preventDefault();
          break;
        case 'ArrowUp':
          this.currentOptionIndex =
            (this.currentOptionIndex - 1 + options.length) % options.length;
          event.preventDefault();
          break;
        case 'Escape':
          this.setLastSelected();
          this.currentOptionIndex = -1;
          break;
        case 'Backspace':
          if (this.inputfield()?.nativeElement.value === '')
            this.currentOptionIndex = -1;
          break;
        case 'Enter':
        case 'Tab':
          {
            if (
              this.currentOptionIndex >= 0 &&
              this.currentOptionIndex < options.length
            ) {
              this.selectOption(options[this.currentOptionIndex]);
            } else if (options.length > 0) {
              this.selectOption(options[0]);
            }

            if (event.key === 'Enter') {
              event.preventDefault();
              this.autoCompleteTrigger()?.closePanel();
            }
          }
          break;
      }
  }

  get filterFunction(): FilterFunction {
    if (this.filterInput()) return this.filterInput()!;

    return this._filter;
  }

  private _filter(
    values: SmallGenericType[],
    value: string | SmallGenericType
  ): SmallGenericType[] {
    let filterValue = '';
    if (typeof value === 'object') filterValue = value.name.toLowerCase();
    else filterValue = value.toLowerCase();

    return values.filter(item =>
      item.name?.toLowerCase().includes(filterValue)
    );
  }

  clearAutocomplete(): void {
    this.control.patchValue(this.defaultEmptyValue());
    this.localControl.patchValue(null);
    this.control.markAsDirty();
  }

  isInsideOfOptionList(className: string): boolean {
    let currentElement: Element | null = this.lastClickedElement;
    while (currentElement) {
      if (currentElement.classList.contains(className)) {
        return true;
      }
      currentElement = currentElement.parentElement;
    }
    return false;
  }

  setFocus(event: FocusEvent, delay: number = 0, closePanel = false): void {
    setTimeout(() => {
      this.currentOptionIndex = -1;
      this.inputfield()?.nativeElement.focus();
      this.inputfield()?.nativeElement.select();
      if (closePanel) this.autoCompleteTrigger()?.closePanel();
    }, delay);
  }

  looseFocus(event: FocusEvent): void {
    const insideOptionList = this.isInsideOfOptionList(
      'autocomplete-list-group'
    );
    const currentValue = this.localControl.value;

    if (!insideOptionList) {
      if (typeof currentValue === 'string' && !this.lastSelectedRecord) {
        this.clearAutocomplete();
      } else if (this.lastSelectedRecord) {
        this.setLastSelected();
      }
    }
  }

  setLastSelected(): void {
    this.localControl.setValue(this.lastSelectedRecord?.name || '');
    this.control.patchValue(this.lastSelectedRecord?.id || null);
    this.control.markAsDirty();
  }

  selectOption(item: { id: number; name: string }): void {
    this.currentOptionIndex = -1;
    this.lastSelectedRecord = item;
    this.localControl.setValue(item.name, { emitEvent: false });
    this.control.patchValue(item.id, { emitEvent: false });
    this.control.markAsDirty();
    this.valueChanged.emit(item as T);
  }

  onSelect(event: any, item: { id: number; name: string }): void {
    this.selectOption(item);
  }

  displayFn(item?: { id: number; name: string }): string {
    if (typeof item === 'string') {
      return item;
    } else {
      if (item) {
        return item.name;
      }
      return this.localControl ? this.localControl.value || '' : '';
    }
  }

  private findSelectedItem(): T | undefined {
    return this.items()
      ? this.items().find(
          item => item[this.optionIdField()] === this.control.value
        )
      : undefined;
  }

  private showLocalDisplayName(): void {
    if (!this.control) return;

    const item = this.findSelectedItem();

    this.localControl.patchValue(
      (item
        ? item[this.optionDisplayNameField() || this.optionNameField()] ||
          item[this.optionNameField()]
        : '') as string
    );
  }

  private setDisabledStateForLocalControl(): void {
    if (!this.control) return;

    if (this.control.enabled === this.localControl.enabled) return;

    this.control.enabled
      ? this.localControl.enable()
      : this.localControl.disable();
  }
}
