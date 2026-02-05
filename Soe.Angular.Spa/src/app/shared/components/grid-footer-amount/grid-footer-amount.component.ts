import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';
import { LabelComponent } from '@ui/label/label.component';

@Component({
  selector: 'soe-grid-footer-amount',
  imports: [LabelComponent, CommonModule],
  styles: [
    `
      .discreet {
        font-weight: normal;
        font-size: small;
      }
    `,
  ],
  template: `<div>
    <div class="float-end">
      <div class="row">
        <div class="col-sm-2">
          <soe-label [labelKey]="labelKey()"></soe-label>
        </div>
      </div>
      <div class="row">
        <div class="col-sm-2 discreet">
          <span>{{ sum() | number: '1.2-2' }}</span>
        </div>
      </div>
    </div>
  </div> `,
})
export class GridFooterAmountComponent {
  labelKey = input.required<string>();
  sum = input.required<number>();
}
