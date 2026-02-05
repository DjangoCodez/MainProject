import { Component, HostListener, ViewChild } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SoeNumberFormControl } from '@shared/extensions';
import { AG_NODE_PROPS } from '@ui/grid/grid.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { INumberEditorParams } from '@ui/grid/interfaces';
import { ICellEditorAngularComp } from 'ag-grid-angular';
import { ICellEditorParams } from 'ag-grid-community';

export type NumberCellEditorParams<T> = INumberEditorParams<T> &
  ICellEditorParams<T, number>;

@Component({
  selector: 'soe-number-cell-editor',
  imports: [ReactiveFormsModule, NumberboxComponent],
  templateUrl: './number-cell-editor.component.html',
})
export class NumberCellEditor<T extends AG_NODE_PROPS>
  implements ICellEditorAngularComp {
  @ViewChild('numberbox') numberbox!: NumberboxComponent;

  public displayValue = '';
  localControl = new SoeNumberFormControl(undefined);
  number?: number = undefined;
  decimals = 0;
  private params!: NumberCellEditorParams<T>;

  agInit(params: NumberCellEditorParams<T>): void {
    this.params = params;
    this.decimals = typeof params.decimals === 'undefined' ? 0 : params.decimals;

    if (this.numberbox) {
      this.numberbox.customNumberInputParser = this.params.colDef?.context?.customNumberInputParser;
      this.numberbox.customPrepareCalculationExpression = this.params.colDef?.context?.customPrepareCalculationExpression;
    }

    this.number = params.value ? Number(this.params.value) : undefined;
    if (this.number) {
      setTimeout(() => {
        this.localControl.patchValue(this.number);
      }, 10);
    }
  }

  @HostListener('focusout', ['$event'])
  onFocusOut(event: any) {
    this.params.stopEditing();
  }

  afterGuiAttached(): void {
    setTimeout(() => {
      this.numberbox.inputER?.nativeElement.focus();
      this.numberbox.inputER?.nativeElement.select();
      if (this.numberbox) {
        this.numberbox.customNumberInputParser = this.params.colDef?.context?.customNumberInputParser;
        this.numberbox.customPrepareCalculationExpression = this.params.colDef?.context?.customPrepareCalculationExpression;
      }
    }, 10);
  }

  getValue() {
    return this.number;
  }

  isCancelBeforeStart(): boolean {
    /*
    Called by grid.
    Called before editing starts.
    */
    if (typeof this.params.disabled === 'boolean') {
      return this.params.disabled;
    } else if (typeof this.params.disabled === 'function') {
      return this.params.disabled(this.params.data);
    }
    return false;
  }

  isCancelAfterEnd(): boolean {
    /*
    Called by grid.
    Can be used to cancel the edit before commiting.
    I.e. if we want to validate that date is between some interval
    or something like that.
    */
    //this.params.stopEditing();
    this.numberbox.setValues();
    return false;
  }

  onValueChanged(value: number) {
    this.number = value;
  }
}
