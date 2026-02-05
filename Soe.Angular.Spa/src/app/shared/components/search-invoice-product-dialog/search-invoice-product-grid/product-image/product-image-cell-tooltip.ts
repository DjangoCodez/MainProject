import { CommonModule } from '@angular/common';
import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';

@Component({
    selector: 'app-product-image-tooltip',
    imports: [CommonModule],
    template: `
    <div
      class="ag-theme-belham ag-popup product-image-tooltip"
      [ngStyle]="{
        'left.px': computedLeft,
        'top.px': computedTop,
        display: displayStyle
      }">
      @if (params.data.externalId) {
        <img
          [src]="imageUrl"
          (load)="onImageLoad()"
          [style.height.px]="imageHeight"
          [style.width.px]="imageWidth" />
        } @else {
          <div>{{ params.data.name }}</div>
        }
      </div>
    `,
    styles: [
        `
      .ag-popup > div {
        z-index: 5000 !important;
      }
      .product-image-tooltip {
        padding: 5px;
        color: var(--ag-foreground-color);
        background-color: #ffffff;
        // border: 1px red solid;
        position: fixed;
        z-index: 10000;
        box-shadow: 0px 0px 10px 0px rgba(0, 0, 0, 0.3);
      }
    `,
    ]
})
export class ProductImageTooltipComponent implements OnInit, OnChanges {
  @Input() params: any;
  @Input() position: { top: number; left: number } = { top: 0, left: 0 };

  imageHeight = 200;
  imageWidth = 200;
  displayStyle = 'none';
  computedLeft = 0;
  computedTop = 0;

  get imageUrl(): string {
    return `https://www.rskdatabasen.se/thumb/w/id/${this.params.data.externalId}/BILD/${this.imageHeight}/${this.imageWidth}`;
  }

  ngOnInit(): void {
    this.setPosition();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.position && this.position) {
      this.setPosition();
    }
  }

  setPosition(): void {
    if (this.params && this.params.eGridCell) {
      const rect = this.params.eGridCell.getBoundingClientRect();
      this.computedLeft = rect.x + rect.width - 5;
      this.computedTop = rect.y - this.imageHeight - 5;
    } else if (this.position) {
      this.computedLeft = this.position.left;
      this.computedTop = this.position.top;
    }
  }

  onImageLoad(): void {
    this.displayStyle = 'block';
  }
}
