import { inject, Injectable } from '@angular/core';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { SupplierInvoiceSharedService } from '../services/supplier-invoice-shared.service';
import { TextBlockDialogComponent } from '@shared/components/text-block-dialog/text-block-dialog.component';
import { TextBlockDialogData } from '@shared/components/text-block-dialog/models/text-block-dialog.model';
import { TranslateService } from '@ngx-translate/core';
import {
  InvoiceTextType,
  SimpleTextEditorDialogMode,
  SoeEntityType,
  TextBlockType,
} from '@shared/models/generated-interfaces/Enumerations';
import { BrowserUtil } from '@shared/util/browser-util';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoiceUtilityService {
  private readonly dialogService = inject(DialogService);
  private readonly translationService = inject(TranslateService);
  private readonly supplierService = inject(SupplierInvoiceSharedService);

  public showBlockForPaymentDialog(
    blockPayment: boolean,
    callback: (isBlocked: boolean) => void,
    invoiceId?: number,
    ediEntryId?: number
  ) {
    return this.openInvoiceTextModal({
      type: InvoiceTextType.SupplierInvoiceBlockReason,
      invoiceId,
      ediEntryId,
      apply: blockPayment,
      callback,
      termToggle: 'economy.supplier.invoice.blockpayment',
      termUntoggle: 'economy.supplier.invoice.unblockforpayment',
    });
  }
  public showUnderInvestigationDialog(
    isUnderInvestigation: boolean,
    callback: (isBlocked: boolean) => void,
    invoiceId?: number,
    ediEntryId?: number
  ) {
    return this.openInvoiceTextModal({
      type: InvoiceTextType.UnderInvestigationReason,
      invoiceId,
      ediEntryId,
      apply: isUnderInvestigation,
      callback,
      termToggle: 'economy.supplier.invoice.underinvestigation',
      termUntoggle: 'economy.supplier.invoice.notunderinvestigation',
    });
  }

  private openInvoiceTextModal(options: {
    type: InvoiceTextType;
    invoiceId: number | undefined;
    ediEntryId: number | undefined;
    apply: boolean;
    callback: (doApply: boolean) => void;
    termUntoggle: string;
    termToggle: string;
  }) {
    this.supplierService
      .getSupplierInvoiceActionText(
        options.type,
        options.invoiceId,
        options.ediEntryId
      )
      .subscribe(invoiceText => {
        this.showBlockOrUnderInvestigationForPayment({
          ...options,
          currentReason: invoiceText?.text ?? '',
        });
      });
  }

  private showBlockOrUnderInvestigationForPayment(options: {
    type: InvoiceTextType;
    invoiceId: number | undefined;
    ediEntryId: number | undefined;
    apply: boolean;
    currentReason: string;
    callback: (doApply: boolean) => void;
    termUntoggle: string;
    termToggle: string;
  }) {
    const {
      type,
      invoiceId,
      ediEntryId,
      apply,
      currentReason,
      callback,
      termToggle,
      termUntoggle,
    } = options;
    const dialogData = {
      ...new TextBlockDialogData(),
      title: apply
        ? this.translationService.instant(termToggle)
        : this.translationService.instant(termUntoggle),
      size: 'lg' as const,
      editPermission: apply,
      text: currentReason,
      entity: SoeEntityType.SupplierInvoice,
      type: TextBlockType.TextBlockEntity,
      mode: SimpleTextEditorDialogMode.AddSupplierInvoiceBlockReason,
    };
    this.dialogService
      .open(TextBlockDialogComponent, dialogData)
      .afterClosed()
      .subscribe(result => {
        if (result !== false) {
          this.supplierService
            .applySupplierInvoiceAction({
              type,
              applyAction: apply,
              reason: result,
              invoiceId: invoiceId,
              ediEntryId: ediEntryId,
            })
            .subscribe(() => callback(apply));
        }
      });
  }
  public openSupplierCentralInNewTab(supplierId: number | undefined) {
    supplierId &&
      BrowserUtil.openInNewTab(
        window,
        `/soe/economy/supplier/suppliercentral/?supplier=${supplierId}&spa=True`
      );
  }
}
