import { Component, HostListener, ViewChild } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SoeTextFormControl } from '@shared/extensions';
import { AG_NODE_PROPS } from '@ui/grid/grid.component'
import { TimeboxComponent, TimeboxValue } from '@ui/forms/timebox/timebox.component';
import { ITimeSpanEditorParams } from '@ui/grid/interfaces';
import { ICellEditorAngularComp } from 'ag-grid-angular';
import { ICellEditorParams } from 'ag-grid-community';

export type TimeSpanCellEditorParams<T> = ITimeSpanEditorParams<T> &
  ICellEditorParams<T, number>;

@Component({
  selector: 'soe-time-span-cell-editor',
  imports: [ReactiveFormsModule, TimeboxComponent],
  templateUrl: './time-span-cell-editor.component.html',
  styleUrl: './time-span-cell-editor.component.scss',
})
export class TimeSpanCellEditor<T extends AG_NODE_PROPS>
  implements ICellEditorAngularComp
{
  @ViewChild('timebox') timebox!: TimeboxComponent;

  time?: TimeboxValue;
  params!: TimeSpanCellEditorParams<T>;
  localControl = new SoeTextFormControl(undefined);

  agInit(params: TimeSpanCellEditorParams<T>): void {
    /*
    Called by grid.
    Can be before view is initialized, input might not exist yet.
    */
    this.params = params;
    this.time = params.value ? params.value : undefined;
    if (params.value) this.localControl.patchValue(params.value);
  }

  @HostListener('focusout', ['$event'])
  onFocusOut(event: any) {
    this.params.stopEditing();
  }

  getValue() {
    /*
    Called by grid.
    Is called when editing is finished (focus is dropped, user tabs, etc.)
    */
    return this.time;
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

    this.timebox.formatValue(this.timebox.inputER?.nativeElement, true);
    this.localControl.patchValue(this.timebox.value);
    return false;
  }

  timeChanged(changedValue?: any) {
    /*
    Change is triggered from manual input
    */
    this.time = changedValue;
    this.localControl.patchValue(this.time);
  }
}
