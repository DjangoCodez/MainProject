import { Component, ElementRef, signal, viewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { AG_NODE_PROPS } from '@ui/grid/grid.component';
import { ICheckboxEditorParams } from '@ui/grid/interfaces';
import { ICellEditorAngularComp } from 'ag-grid-angular';
import { ICellEditorParams } from 'ag-grid-community';

export type CheckboxCellEditorParams<T> = ICheckboxEditorParams<T> &
  ICellEditorParams<T, boolean>;

@Component({
  selector: 'soe-checkbox-cell-editor',
  imports: [ReactiveFormsModule],
  templateUrl: './checkbox-cell-editor.component.html',
  styleUrls: ['./checkbox-cell-editor.component.scss'],
})
export class CheckboxCellEditor<T extends AG_NODE_PROPS>
  implements ICellEditorAngularComp
{
  input = viewChild<ElementRef>('checkboxInput');

  params!: CheckboxCellEditorParams<T>;

  control!: FormControl;

  showCheckbox = signal(false);

  agInit(params: CheckboxCellEditorParams<T>): void {
    this.control = new FormControl();
    this.params = params;

    if (this.params.showCheckbox) {
      if (typeof this.params.showCheckbox === 'string') {
        // showCheckbox is a string which then should be a property on the data row.
        // Use it to check if value is true.
        this.showCheckbox.set(
          Boolean((<any>this.params.data)[this.params.showCheckbox])
        );
      } else if (typeof this.params.showCheckbox === 'function') {
        // showCheckbox is a function. Evaluate function to check if result is true.
        this.showCheckbox.set(this.params.showCheckbox(this.params.data));
      }
    } else {
      // showCheckbox is not specified, then always show it.
      this.showCheckbox.set(true);
    }

    this.control.setValue(params.value);
  }

  afterGuiAttached() {
    this.input()?.nativeElement.focus();
  }

  getValue() {
    return this.control.value;
  }

  isPopup?(): boolean {
    return false;
  }

  getGui() {
    return this.input();
  }

  isCancelBeforeStart?(): boolean {
    if (typeof this.params.disabled === 'boolean') {
      return this.params.disabled;
    } else if (typeof this.params.disabled === 'function') {
      return this.params.disabled(this.params.data);
    }
    return false;
  }

  isCancelAfterEnd?(): boolean {
    return false;
  }

  onChange() {
    if (this.params.onClick)
      this.params.onClick(this.control.value, this.params.data);
  }
}
