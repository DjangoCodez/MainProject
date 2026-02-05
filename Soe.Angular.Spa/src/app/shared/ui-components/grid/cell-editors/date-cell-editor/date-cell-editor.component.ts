import {
  Component,
  ViewChild,
  ViewEncapsulation,
  OnDestroy,
} from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SoeDateFormControl } from '@shared/extensions';
import { DateUtil } from '@shared/util/date-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { AG_NODE_PROPS } from '@ui/grid/grid.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { IDateEditorParams } from '@ui/grid/interfaces';
import { ICellEditorAngularComp } from 'ag-grid-angular';
import { ICellEditorParams } from 'ag-grid-community';

export type DateCellEditorParams<T> = IDateEditorParams<T> &
  ICellEditorParams<T, Date>;

@Component({
  selector: 'soe-date-cell-editor',
  imports: [ReactiveFormsModule, DatepickerComponent],
  templateUrl: './date-cell-editor.component.html',
  styleUrls: [
    './../../../forms/datepicker/datepicker.component.scss',
    './date-cell-editor.component.scss',
  ],
  encapsulation: ViewEncapsulation.None,
  host: { class: 'soe-date-cell-editor' },
})
export class DateCellEditor<T extends AG_NODE_PROPS>
  implements ICellEditorAngularComp, OnDestroy
{
  @ViewChild('datepicker') datepicker!: DatepickerComponent;

  date?: Date;
  params!: DateCellEditorParams<T>;
  localControl = new SoeDateFormControl(undefined);
  readonly dateFormat = DateUtil.languageDateFormat;
  readonly dateFormatText = DateUtil.languageDateFormatText;
  readonly currentLanguage = SoeConfigUtil.language;
  lastClickedElement: Element | null = null;
  private mousedownHandler?: (event: MouseEvent) => void;

  constructor() {}

  agInit(params: DateCellEditorParams<T>): void {
    /*
    Called by grid.
    Can be before view is initialized, input might not exist yet.
    */
    this.params = params;
    this.date = params.value ? new Date(params.value) : undefined;
    if (this.date) {
      this.localControl.patchValue(this.date);
    }

    // Track clicks and prevent AG-Grid from seeing clicks on Material overlay
    // Otherwise clicks on the datepicker popup close the editor immediately as it's "outside the grid"
    this.mousedownHandler = (event: MouseEvent) => {
      this.lastClickedElement = event.target as Element;

      // Check if click is on Material datepicker overlay
      let current = event.target as Element;
      while (current) {
        if (
          current.classList?.contains('mat-datepicker-popup') ||
          current.classList?.contains('mat-datepicker-content') ||
          current.classList?.contains('cdk-overlay-pane')
        ) {
          // Stop AG-Grid from seeing this click
          event.stopPropagation();
          event.stopImmediatePropagation();
          return;
        }
        current = current.parentElement as Element;
      }
    };

    document.addEventListener('mousedown', this.mousedownHandler, true);
  }

  ngOnDestroy(): void {
    // Clean up event listener when component is destroyed
    if (this.mousedownHandler) {
      document.removeEventListener('mousedown', this.mousedownHandler, true);
    }
  }

  getValue(): Date | undefined {
    /*
    Called by grid.
    Is called when editing is finished (focus is dropped, user tabs, etc.)
    */
    return this.date;
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

    if (this.localControl.value === '' || this.localControl.value === null) {
      this.datepicker.clearDatepickerValue();
    }
    return false;
  }

  isPopup() {
    /*
    Called by grid.
    Returning true tells AG-Grid this is a popup editor.
    This prevents the grid from closing when clicking inside the popup.
    */
    return true;
  }

  onDateChanged(changedValue?: Date) {
    /*
    Change is triggered from manual input or calendar selection
    */
    this.date = changedValue;
  }

  onCalendarClosed() {
    /*
    Called when the calendar popup closes.
    If a date was selected via the calendar, stop editing.
    */
    if (this.date) {
      setTimeout(() => {
        this.params.stopEditing();
      }, 100);
    }
  }

  onEnterPressed() {
    /*
    Called when user presses Enter.
    Commit the value and navigate to the next cell.
    */
    this.params.api.tabToNextCell();
  }

  isInsideOfButton(): boolean {
    let className: string = 'soe-datepicker-button';
    let currentElement: Element | null = this.lastClickedElement;
    while (currentElement) {
      if (currentElement.classList.contains(className)) {
        return true;
      }
      currentElement = currentElement.parentElement;
    }
    return false;
  }
}
