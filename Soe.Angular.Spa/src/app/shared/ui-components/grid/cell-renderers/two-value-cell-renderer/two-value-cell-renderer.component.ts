import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { IconModule } from '@ui/icon/icon.module';
import { TwoValueCellRendererParams } from '@ui/grid/interfaces';
import { ICellRendererAngularComp } from 'ag-grid-angular';

@Component({
  selector: 'soe-two-value-cell-renderer',
  imports: [CommonModule, IconModule],
  template: `
    <div
      class="value-container"
      title="{{ primaryValue }} &#13;{{ secondaryValue }}">
      <span class="primary-value">{{ primaryValue }}</span>
      <span class="secondary-value">{{ secondaryValue }}</span>
    </div>
  `,
  styles: [
    `
      .value-container {
        display: flex;
        flex-direction: column;
      }

      .value-container span {
        line-height: 100%;
      }

      .primary-value {
        font-size: 0.9rem;
      }

      .secondary-value {
        font-size: 0.7rem;
        opacity: 0.7;
      }
    `,
  ],
})
export class TwoValueCellRenderer<T> implements ICellRendererAngularComp {
  public primaryValue!: string;
  public secondaryValue!: string;

  agInit(params: TwoValueCellRendererParams<T>): void {
    this.setValues(params);
  }

  refresh(params: TwoValueCellRendererParams<T>): boolean {
    this.setValues(params);
    return true;
  }

  private setValues(params: TwoValueCellRendererParams<T>) {
    if (params.data) {
      this.primaryValue = params.data[params.primaryValueKey] as string;
      this.secondaryValue = params.data[params.secondaryValueKey] as string;
    }
  }
}
