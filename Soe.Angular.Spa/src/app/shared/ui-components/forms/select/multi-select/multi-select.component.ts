import {
  AfterViewInit,
  Component,
  ElementRef,
  OnChanges,
  OnDestroy,
  OnInit,
  Renderer2,
  SimpleChanges,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import {
  FormArray,
  FormControl,
  NG_VALUE_ACCESSOR,
  NgControl,
  ReactiveFormsModule,
} from '@angular/forms';
import { takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component';
import { LabelComponent } from '@ui/label/label.component';
import { TranslatePipe } from '@ngx-translate/core';
import { ClickOutsideDirective } from '@shared/directives/click-outside/click-outside.directive';

interface SelectProps {
  id: number;
  name: string;
}

enum IsAllSelectedState {
  NoneSelected,
  SomeSelected,
  AllSelected,
}

@Component({
  selector: 'soe-multi-select',
  imports: [
    CommonModule,
    ClickOutsideDirective,
    ReactiveFormsModule,
    ButtonComponent,
    LabelComponent,
    TranslatePipe,
  ],
  templateUrl: './multi-select.component.html',
  styleUrls: ['./multi-select.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: MultiSelectComponent,
    },
    { provide: NgControl, multi: true, useExisting: MultiSelectComponent },
  ],
})
export class MultiSelectComponent<T extends SelectProps>
  extends ValueAccessorDirective<number[]>
  implements OnInit, AfterViewInit, OnChanges, OnDestroy
{
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
  dropdownWidth = input(0);
  dropUp = input(false);
  items = input<Array<T>>([]);
  optionIdField = input<keyof T>('id');
  optionNameField = input<keyof T>('name');
  sortById = input(false);
  sortByName = input(false);
  limit = input(100);
  showSelected = input(false);
  showSelectedItems = input(false);
  showSelectedItemsInline = input(false);
  sortSelectedItemsBy = input<'name' | 'id' | 'none'>('name');
  hideHasSelectedItems = input(false);
  filterButtonCaption = input('core.filter');
  hideFilterButton = input(false);

  onItemSelect = output<T>();
  onItemDeselect = output<T>();
  onSelectionOpened = output();
  onSelectionComplete = output<number[]>();

  content = viewChild<ElementRef>('content');
  selectCtrl = viewChild<ElementRef>('selectCtrl');

  private _renderer = inject(Renderer2);

  hasContent = signal(false);

  selectedItems = new FormArray<FormControl<number>>([]);
  visibleItems: Array<T> = [];
  searchCtrl = new FormControl('');
  isOpen = signal<boolean | undefined>(undefined);
  visibleValue = '';
  allSelectedItems: string[] = [];

  isAllSelected = signal<IsAllSelectedState>(IsAllSelectedState.NoneSelected);
  isAllSelectedChecked = signal(false);
  isAllSelectedIndeterminate = signal(false);

  toggleOpenState = () => {
    this.isOpen.set(!this.isOpen());
    this.isOpen() && this.onSelectionOpened.emit();
  };

  closeList = () => {
    if (this.isOpen()) this.isOpen.set(false);
  };

  openEff = effect(() => {
    if (this.isOpen() === false) {
      this.onSelectionComplete.emit(this.selectedItems.value);
    }
  });

  ngOnChanges(changes: SimpleChanges): void {
    const { items } = changes;
    if (items) {
      this.updateVisibleList();
      this.updateSelectedItemsVisibleValue();
    }
  }

  ngOnInit(): void {
    super.ngOnInit();

    this.control.value instanceof Array &&
      this.resetSelectedItems(this.control.value);

    this.searchCtrl.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(() => this.updateVisibleList());

    this.control.valueChanges.pipe(takeUntil(this._destroy$)).subscribe(val => {
      val instanceof Array && this.resetSelectedItems(val);
    });
  }

  private resetSelectedItems(selectedIds: number[]): void {
    this.selectedItems.clear();
    new Set(selectedIds).forEach((item: number) => {
      this.selectedItems.push(this.toControl(item));
    });
    this.updateSelectedItemsVisibleValue();
  }

  ngAfterViewInit(): void {
    super.ngAfterViewInit();

    if (this.elemHasContent(this.content())) this.hasContent.set(true);
  }

  setSelectionComplete(): void {
    this.isOpen.set(false);
  }

  getItemById(id: number): T | undefined {
    return this.items().find(item => item[this.optionIdField()] === id);
  }

  toControl(idValue: number): FormControl<number> {
    return new FormControl<number>(idValue, {
      nonNullable: true,
    });
  }

  updateVisibleList(): void {
    const items = this.items()
      .filter(item => {
        if (!this.searchCtrl.value) {
          return item;
        }
        return (item[this.optionNameField()] as string)
          .toLowerCase()
          .includes(this.searchCtrl.value?.toLowerCase() || '');
      })
      .slice(0, this.limit());

    if (this.sortById() || this.sortByName()) {
      this.visibleItems = items.sort((a, b) => {
        if (this.sortById()) {
          return (
            (a[this.optionIdField()] as number) -
            (b[this.optionIdField()] as number)
          );
        } else if (this.sortByName()) {
          return (a[this.optionNameField()] as string).localeCompare(
            b[this.optionNameField()] as string
          );
        } else {
          return 0;
        }
      });
    } else {
      this.visibleItems = items;
    }
  }

  isSelected(item: T): boolean {
    return this.selectedItems.value.includes(
      item[this.optionIdField()] as number
    );
  }

  selectItem(itemToSelect: T): void {
    const index = this.selectedItems.value.findIndex(
      item => item === itemToSelect[this.optionIdField()]
    );
    const value = this.control.value as number[];
    this.control.markAsDirty();
    if (index > -1) {
      this.onItemDeselect.emit(itemToSelect);
      value.splice(index, 1);
      this.control.patchValue([...value]);
    } else {
      value.push(itemToSelect[this.optionIdField()] as number);
      this.control.patchValue([...value]);
      this.onItemSelect.emit(itemToSelect);
    }

    this.updateSelectedItemsVisibleValue();
  }

  updateSelectedItemsVisibleValue(): void {
    const idField = this.optionIdField();
    const nameField = this.optionNameField();
    const selectedItems = this.selectedItems.value
      .map(id => this.items().find(i => i[idField] === id))
      .filter((item): item is T => !!item);

    const sortBy = this.sortSelectedItemsBy();
    if (sortBy === 'id') {
      selectedItems.sort((a, b) => (a[idField] as number) - (b[idField] as number));
    } else if (sortBy === 'name') {
      selectedItems.sort((a, b) => (a[nameField] as string).localeCompare(b[nameField] as string));
    }

    this.visibleValue = selectedItems.map(item => item[nameField]).join(', ');
    this.allSelectedItems = selectedItems.map(item => item[nameField] as string);
    this.setIsAllSelected();
  }

  isAllSelectedChanged() {
    const allSelected = this.selectedItems.length === this.items().length;
    if (allSelected) {
      this.unselectAll();
    } else {
      this.selectAll();
    }

    this.setIsAllSelected();
  }

  unselectAll(): void {
    const hasSelectedItems = this.selectedItems.length > 0;
    this.control.patchValue([]);
    this.updateSelectedItemsVisibleValue();
    hasSelectedItems && this.control.markAsDirty();
  }

  selectAll(): void {
    const allSelected = this.selectedItems.length === this.items().length;
    this.selectedItems.clear();
    this.control.patchValue(
      this.items().map(item => item[this.optionIdField()] as number)
    );
    this.updateSelectedItemsVisibleValue();
    !allSelected && this.control.markAsDirty();
  }

  private setIsAllSelected(): void {
    const hasSelectedItems = this.selectedItems.length > 0;
    const allSelected = this.selectedItems.length === this.items().length;

    if (allSelected) {
      this.isAllSelected.set(IsAllSelectedState.AllSelected);
      this.isAllSelectedChecked.set(true);
      this.isAllSelectedIndeterminate.set(false);
    } else if (!hasSelectedItems) {
      this.isAllSelected.set(IsAllSelectedState.NoneSelected);
      this.isAllSelectedChecked.set(false);
      this.isAllSelectedIndeterminate.set(false);
    } else {
      this.isAllSelected.set(IsAllSelectedState.SomeSelected);
      this.isAllSelectedChecked.set(false);
      this.isAllSelectedIndeterminate.set(true);
    }
  }

  setDisabledState(isDisabled: boolean): void {
    super.setDisabledState(isDisabled);
    this._renderer.setProperty(
      this.selectCtrl()?.nativeElement,
      'disabled',
      isDisabled
    );
  }

  ngOnDestroy(): void {
    super.ngOnDestroy();
    this.openEff?.destroy();
  }
}
