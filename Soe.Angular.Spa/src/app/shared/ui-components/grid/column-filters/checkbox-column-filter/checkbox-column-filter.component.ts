import { Component, signal } from '@angular/core';
import { ColumnUtil, SoeColumnType } from '@ui/grid/util/column-util';
import { CheckboxIndeterminate } from '@ui/grid/enums/checkbox-state.enum';
import { IFloatingFilterAngularComp } from 'ag-grid-angular';
import { FilterChangedEvent, IFloatingFilterParams } from 'ag-grid-community';

export type CheckboxFilterParams<T> = {
  setChecked: boolean;
  canBeIndeterminate: boolean;
};

export type CheckboxFloatingFilterParams = CheckboxFilterParams<unknown> &
  IFloatingFilterParams;

@Component({
  selector: 'soe-checkbox-column-filter',
  templateUrl: './checkbox-column-filter.component.html',
  styleUrls: ['./checkbox-column-filter.component.scss'],
})
export class CheckboxFloatingFilter implements IFloatingFilterAngularComp {
  private params!: CheckboxFloatingFilterParams;
  checked = false;
  readOnly = false;
  indeterminate = false;
  allowIndeterminate = false;
  isActiveColumn = signal(false);

  agInit(params: CheckboxFloatingFilterParams): void {
    this.params = params;
    this.checked = false;
    this.readOnly = false;
    this.allowIndeterminate = params.canBeIndeterminate;
    this.indeterminate = false;
    this.resetCheckbox(params.setChecked);
    this.setInstanceModel();

    if (
      this.params.column.getColDef().context.soeColumnType ===
      SoeColumnType.Active
    ) {
      // This is used to set 'is-active-column' class, to be able to style the column differently from other checkbox columns
      this.isActiveColumn.set(true);
    }
  }

  onParentModelChanged(
    parentModel: any,
    filterChangedEvent?: FilterChangedEvent | null
  ): void {
    if (
      !parentModel &&
      ColumnUtil.checkAndRemoveClearFlag(filterChangedEvent?.context)
    ) {
      this.resetCheckbox(
        this.allowIndeterminate
          ? CheckboxIndeterminate.Indeterminate
          : this.params.setChecked
      );
      this.setInstanceModel(false);
    } else {
      if (parentModel?.values?.length > 0 && parentModel.values[0] === 'true') {
        this.checked = true;
        this.indeterminate = false;
      }
    }
  }

  getModelAsString(): string {
    const indeterminate = this.indeterminate
      ? CheckboxIndeterminate.Indeterminate
      : 'false';
    return this.checked ? 'true' : indeterminate;
  }

  private resetCheckbox(setChecked: boolean | CheckboxIndeterminate) {
    const isIndeterminate = setChecked === CheckboxIndeterminate.Indeterminate;
    if (setChecked) {
      this.checked = !isIndeterminate;
      this.readOnly = false;
      this.indeterminate = isIndeterminate;
    } else {
      this.checked = false;
      this.readOnly = true;
      this.indeterminate = true;
    }
  }

  onChange(event: any): void {
    this.params.api.onFilterChanged();
    this.toggleCheckboxState(event.target.checked);
    this.setInstanceModel();
  }

  setInstanceModel(shouldEmit = true): void {
    this.params.api
      .setColumnFilterModel(
        this.params.column,
        this.indeterminate
          ? null
          : {
              filterType: 'set',
              values: [this.getModelAsString()],
            }
      )
      .then(() => {
        if (shouldEmit) {
          this.params.api.onFilterChanged();
        }
      });
  }

  toggleCheckboxState(checked: boolean): void {
    if (checked || this.indeterminate) {
      this.checked = this.indeterminate;
      this.indeterminate = this.allowIndeterminate
        ? !this.indeterminate
        : false;
    } else {
      this.checked = false;
      this.indeterminate = false;
    }
  }
}
