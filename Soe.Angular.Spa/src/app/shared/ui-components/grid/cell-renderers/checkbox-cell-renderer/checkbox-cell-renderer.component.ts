import { Component, signal } from '@angular/core';
import { CheckboxCellRendererParams } from '@ui/grid/interfaces';
import { ICellRendererAngularComp } from 'ag-grid-angular';

@Component({
  selector: 'soe-checkbox-cell-renderer',
  templateUrl: './checkbox-cell-renderer.component.html',
  styleUrls: ['./checkbox-cell-renderer.component.scss'],
})
export class CheckboxCellRenderer implements ICellRendererAngularComp {
  public params!: CheckboxCellRendererParams;
  public value!: boolean;

  showCheckbox = signal(false);

  agInit(params: CheckboxCellRendererParams): void {
    this.params = params;
    this.value = this.params.value;

    if (this.params.showCheckbox) {
      if (typeof this.params.showCheckbox === 'string') {
        // showCheckbox is a string which then should be a property on the data row.
        // Use it to check if value is true.
        this.showCheckbox.set(
          Boolean(this.params.data[this.params.showCheckbox])
        );
      } else if (typeof this.params.showCheckbox === 'function') {
        // showCheckbox is a function. Evaluate function to check if result is true.
        this.showCheckbox.set(this.params.showCheckbox(this.params.data));
      }
    } else {
      // showCheckbox is not specified, then always show it.
      this.showCheckbox.set(true);
    }
  }

  refresh(params: CheckboxCellRendererParams): boolean {
    this.params = params;
    this.value = this.params.value;
    return true;
  }
}
