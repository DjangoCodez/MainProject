import { Component, inject, EventEmitter, Output } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  ProductPriceSearchResult,
  ProductSearchResult,
  SearchInvoiceProductDialogData,
} from './models/search-invoice-product-dialog.models';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { TranslateService } from '@ngx-translate/core';
import { IInvoiceProductSearchViewDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeSysPriceListProviderType } from '@shared/models/generated-interfaces/Enumerations';

@Component({
  selector: 'soe-search-invoice-product-dialog',
  templateUrl: './search-invoice-product-dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class SearchInvoiceProductDialogComponent extends DialogComponent<SearchInvoiceProductDialogData> {
  private readonly translate = inject(TranslateService);
  @Output() productAdded = new EventEmitter<ProductPriceSearchResult>();

  private terms: Record<string, string> = {};
  protected selectedProductPrice?: ProductSearchResult;
  protected selectedProduct?: IInvoiceProductSearchViewDTO;

  constructor() {
    super();
    this.setTitle();
  }

  private setTitle(): void {
    const keys: string[] = [];

    if (this.data.hideProducts)
      keys.push('common.searchinvoiceproduct.selectwholeseller');
    else keys.push('common.searchinvoiceproduct.searchproduct');

    this.translate.get(keys).subscribe(terms => {
      this.data.title = this.data.hideProducts
        ? terms['common.searchinvoiceproduct.selectwholeseller']
        : terms['common.searchinvoiceproduct.searchproduct'];
    });
  }

  hasNoSelectedRows(): boolean {
    if (this.data.hidePrices && this.selectedProduct) {
      return false;
    }
    return this.selectedProductPrice === undefined;
  }

  protected triggerOk(): void {
    if (!this.data.hidePrices) {
      this.dialogRef.close(this.selectedProductPrice);
      return;
    }

    //#region disableCloseOnAdd

    const productsWithPrices: ProductPriceSearchResult = {
      productId: this.selectedProduct?.productIds[0] || 0,
      productNr: this.selectedProduct?.number || '',
      productName: this.selectedProduct?.name || '',
      productInfo: this.selectedProduct?.extendedInfo || '',
      quantity: 1,
      imageUrl: this.selectedProduct?.imageUrl || '',
      type: this.selectedProduct?.type || SoeSysPriceListProviderType.Unknown,
      externalId: this.selectedProduct?.externalId || 0,
    };

    this.productAdded.emit(productsWithPrices);

    //#endregion disableCloseOnAdd
  }
}
