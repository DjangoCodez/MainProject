import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { IconModule } from '@ui/icon/icon.module';
import { MultiValueCellRendererParams } from '@ui/grid/interfaces';
import { ICellRendererAngularComp } from 'ag-grid-angular';

@Component({
  selector: 'soe-multi-value-cell-renderer',
  imports: [CommonModule, IconModule],
  template: `
    <div title="{{ values?.join(', ') ?? '' }}">
      @for (item of values; track $index) {
        <span class="item">{{ item }}</span>
      }
    </div>
  `,
  styles: [
    `
      .item {
        overflow: 'hidden';
        text-overflow: 'ellipsis';
        padding: 0 5px;
        border-radius: 5px;
        border: 1px solid #cccccc;
      }

      .item:not(:first-child) {
        margin-left: 5px;
      }
    `,
  ],
})
export class MultiValueCellRenderer<T, U = unknown>
  implements ICellRendererAngularComp
{
  public values?: string[];

  agInit(params: MultiValueCellRendererParams<T, U>): void {
    this.setValues(params);
  }

  refresh(params: MultiValueCellRendererParams<T, U>): boolean {
    this.setValues(params);
    return true;
  }

  private setValues(params: MultiValueCellRendererParams<T, U>) {
    if (!params.value) {
      this.values = [];
      return;
    }
    this.values = params.value;
  }
}
