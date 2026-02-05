import { inject, Injectable } from '@angular/core';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import {
  IBlockPaymentModel,
  IInvoiceTextActionModel,
} from '@shared/models/generated-interfaces/EconomyModels';
import { InvoiceTextType } from '@shared/models/generated-interfaces/Enumerations';
import { IInvoiceTextDTO } from '@shared/models/generated-interfaces/InvoiceTextDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  blockSupplierInvoicePayment,
  getSupplierInvoiceText,
  invoiceTextAction,
  saveSupplierInvoicesForUploadedImages,
} from '@shared/services/generated-service-endpoints/economy/SupplierInvoice.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoiceSharedService {
  /**
   * Should be of value to grid/edit components that are working with supplier invoices.
   */
  private readonly http = inject(SoeHttpClient);

  blockSupplierInvoicePayment(model: IBlockPaymentModel) {
    return this.http.post<IBlockPaymentModel>(
      blockSupplierInvoicePayment(),
      model
    );
  }

  getSupplierInvoiceBlockText(invoiceId: number) {
    return this.http.get<IInvoiceTextDTO>(
      getSupplierInvoiceText(
        invoiceId,
        undefined,
        InvoiceTextType.SupplierInvoiceBlockReason
      )
    );
  }

  getSupplierInvoiceActionText(
    type: InvoiceTextType,
    invoiceId?: number,
    ediEntryId?: number
  ) {
    return this.http.get<IInvoiceTextDTO>(
      getSupplierInvoiceText(type, invoiceId, ediEntryId)
    );
  }

  applySupplierInvoiceAction(model: IInvoiceTextActionModel) {
    return this.http.post<IActionResult>(invoiceTextAction(), model);
  }

  public saveInvoicesForUploadedImages(dataStorageIds: number[]) {
    return this.http.post<BackendResponse>(
      saveSupplierInvoicesForUploadedImages(dataStorageIds),
      dataStorageIds
    );
  }
}
