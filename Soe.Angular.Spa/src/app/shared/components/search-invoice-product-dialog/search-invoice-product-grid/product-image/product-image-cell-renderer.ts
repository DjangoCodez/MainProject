import {
  ApplicationRef,
  Component,
  ComponentRef,
  inject,
  OnDestroy,
  ViewContainerRef,
} from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ProductImageTooltipComponent } from './product-image-cell-tooltip';

import { ICellRendererParams } from 'ag-grid-community';
import { IInvoiceProductSearchViewDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeSysPriceListProviderType } from '@shared/models/generated-interfaces/Enumerations';

@Component({
    selector: 'app-product-image-cell-renderer',
    imports: [],
    template: `
    <div
      class="product-image-container"
      (mouseenter)="onMouseEnter($event)"
      (mouseleave)="onMouseLeave()">
      @if (showImage()) {
        <span>
          <img
            [src]="imageUrl"
            alt="Product Image"
            style="height: 40px; width: 40px;" />
          </span>
        } @else {
          <div></div>
        }
      </div>
    `,
    styles: [
        `
      .product-image-container {
        position: relative;
      }
    `,
    ]
})
export class ProductImageCellRendererComponent
  implements ICellRendererAngularComp, OnDestroy
{
  private appRef = inject(ApplicationRef);
  private viewContainerRef = inject(ViewContainerRef);

  params?: ICellRendererParams<IInvoiceProductSearchViewDTO>;
  imageUrl = '';
  private tooltipComponentRef: ComponentRef<ProductImageTooltipComponent> | null =
    null;

  agInit(params: ICellRendererParams<IInvoiceProductSearchViewDTO>): void {
    this.params = params;
    if (this.showImage()) {
      const data = this.params.data!;
      this.imageUrl = `https://www.rskdatabasen.se/thumb/w/id/${data.externalId}/BILD/100/100`;
    }
  }

  refresh(params: any): boolean {
    this.agInit(params);
    return true;
  }

  showImage() {
    if (!this.params || !this.params.data) return false;
    const data = this.params.data;
    return (
      data.type === SoeSysPriceListProviderType.Plumbing &&
      !!data.imageUrl &&
      data.imageUrl.length > 0
    );
  }

  onMouseEnter(event: MouseEvent): void {
    // Create the tooltip dynamically if it doesn't already exist.
    if (!this.tooltipComponentRef) {
      this.tooltipComponentRef = this.viewContainerRef.createComponent(
        ProductImageTooltipComponent
      );
      // Set inputs
      this.tooltipComponentRef.instance.params = this.params;
      this.tooltipComponentRef.instance.position = {
        top: event.clientY,
        left: event.clientX,
      };

      if (this.appRef.components.length === 0) {
        this.appRef.attachView(this.tooltipComponentRef.hostView);
      }
      document.body.appendChild(
        this.tooltipComponentRef.location.nativeElement
      );
    }
  }

  onMouseLeave(): void {
    if (this.tooltipComponentRef) {
      this.appRef.detachView(this.tooltipComponentRef.hostView);
      this.tooltipComponentRef.destroy();
      this.tooltipComponentRef = null;
    }
  }

  ngOnDestroy(): void {
    if (this.tooltipComponentRef) {
      this.appRef.detachView(this.tooltipComponentRef.hostView);
      this.tooltipComponentRef.destroy();
    }
  }
}
