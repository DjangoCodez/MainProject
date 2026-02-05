import { ICellEditorAngularComp } from 'ag-grid-angular';
import { ICellEditorParams } from 'ag-grid-community';
import { Component, HostListener, ViewChild } from '@angular/core';
import { SoeDateFormControl } from '@shared/extensions';
import { AG_NODE_PROPS } from '@ui/grid/grid.component'
import { TimeboxComponent, TimeboxValue } from '@ui/forms/timebox/timebox.component';
import { ReactiveFormsModule } from '@angular/forms';
import { ITimeEditorParams } from '@ui/grid/interfaces';

export type TimeCellEditorParams<T> = ITimeEditorParams<T> &
  ICellEditorParams<T, Date>;

@Component({
  selector: 'soe-time-cell-editor',
  imports: [ReactiveFormsModule, TimeboxComponent],
  templateUrl: './time-cell-editor.component.html',
  styleUrl: './time-cell-editor.component.scss',
})
export class TimeCellEditor<T extends AG_NODE_PROPS>
  implements ICellEditorAngularComp
{
  @ViewChild('timebox') timebox!: TimeboxComponent;

  time?: TimeboxValue;
  params!: TimeCellEditorParams<T>;
  localControl = new SoeDateFormControl(undefined);

  agInit(params: TimeCellEditorParams<T>): void {
    /*
    Called by grid.
    Can be before view is initialized, input might not exist yet.
    */
    this.params = params;
    this.time = params.value ? new Date(params.value) : undefined;
    if (this.time) {
      this.localControl.patchValue(this.time);
    }
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
    // this.timebox.formatValue(this.timebox.inputER?.nativeElement);
    // this.localControl.patchValue(this.timebox.control.value);
    // this.time = this.localControl.value;
    // this.params.api.stopEditing();

    this.timebox.formatValue(this.timebox.inputER?.nativeElement, true);
    this.localControl.patchValue(this.timebox.value);
    return false;
  }

  timeChanged(changedValue?: any) {
    /*
    Change is triggered from manual input
    */
    this.time = changedValue;
  }
}
