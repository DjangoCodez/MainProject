import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { IconModule } from '@ui/icon/icon.module';
import { TextButtonCellRendererParams } from '@ui/grid/interfaces';
import { ICellRendererAngularComp } from 'ag-grid-angular';

@Component({
  selector: 'soe-text-button-cell-renderer',
  imports: [CommonModule, IconModule, TranslatePipe],
  templateUrl: './text-button-cell-renderer.component.html',
  styleUrls: ['./text-button-cell-renderer.component.scss'],
})
export class TextButtonCellRenderer implements ICellRendererAngularComp {
  public params!: TextButtonCellRendererParams;
  public value!: string;
  public tooltip!: string;
  public iconClass!: string;
  public show!: boolean;

  agInit(params: TextButtonCellRendererParams): void {
    this.params = params;
    this.tooltip = this.params.tooltip ? this.params.tooltip : '';
    this.iconClass = this.params.iconClass ? this.params.iconClass : '';
    this.params.value = this.params.valueFormatted ?? this.params.value;
    this.value = this.params.value;

    if (this.params.show) {
      if (typeof this.params.show === 'string') {
        // show is a string which then should be a property on the data row.
        // Use it to check if value is true.
        this.show = Boolean(this.params.data[this.params.show]);
      } else if (typeof this.params.show === 'function') {
        // show is a function. Evaluate function to check if result is true.
        this.show = this.params.show(this.params.data);
      }
    } else {
      this.show = true;
    }
  }

  refresh(params: TextButtonCellRendererParams): boolean {
    this.params = params;
    this.value = this.params.value;
    return true;
  }

  onChange(event: any) {
    this.value = event.target.value;
    if (this.params.onClick) this.params.onClick(this.params.data);
  }
}
