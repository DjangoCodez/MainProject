import { Injectable } from '@angular/core';
import {
  ImportPaymentIOState,
  ImportPaymentIOStatus,
  TermGroup_BillingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { PaymentImportIODTO } from '../models/import-payments.model';

@Injectable({
  providedIn: 'root',
})
export class ImportPaymentSharedHandler {
  public setBillingType(x: any, terms: any = []) {
    switch (x.type) {
      case TermGroup_BillingType.Debit:
        x.typeName = terms['economy.import.payment.debit'];
        break;
      case TermGroup_BillingType.Credit:
        x.typeName = terms['economy.import.payment.credit'];
        break;
      default:
    }
  }

  public setStatus(x: any, terms: any = []) {
    x.statusId = x.status;
    switch (x.status) {
      case ImportPaymentIOStatus.FullyPaid:
        x.statusName = terms['economy.import.payment.fullypaid'];
        break;
      case ImportPaymentIOStatus.Match:
        x.statusName = terms['economy.import.payment.matched'];
        break;
      case ImportPaymentIOStatus.Paid:
        x.statusName = terms['economy.import.payment.paid'];
        x.isSelectDisabled = true;
        break;
      case ImportPaymentIOStatus.PartlyPaid:
        x.statusName = terms['economy.import.payment.partly_paid'];
        break;
      case ImportPaymentIOStatus.Rest:
        x.statusName = terms['economy.import.payment.rest'];
        break;
      case ImportPaymentIOStatus.Unknown:
        x.statusName = terms['economy.import.payment.unknown'];
        break;
      case ImportPaymentIOStatus.Error:
        x.statusName = terms['economy.import.payment.error'];
        break;
      case ImportPaymentIOStatus.Manual:
        x.statusName = terms['core.manual'];
        break;
      case ImportPaymentIOStatus.ManuallyHandled:
        x.statusName = terms['economy.import.payment.manualstatus'];
        break;
      case ImportPaymentIOStatus.Deleted:
        x.statusName = terms['economy.import.payment.deleted'];
        break;
      default:
        break;
    }

    if (x.state == ImportPaymentIOState.Closed) x.isSelectDisabled = true;
  }

  getMatchCode(matchCodeId: number, matchCodesDict: any[]) {
    return matchCodesDict.find(p => p.matchCodeId == matchCodeId);
  }

  setMatchCode(x: any, matchCodesDict: any[]) {
    const matchCode = this.getMatchCode(x.matchCodeId, matchCodesDict);
    x.matchCodeName = matchCode?.name || '';
  }

  public setStatusTexts(
    r: PaymentImportIODTO,
    terms: any = [],
    matchCodesDict: any[]
  ) {
    this.setBillingType(r, terms);
    this.setStatus(r, terms);
    this.setMatchCode(r, matchCodesDict);
    r.paymentTypeName = 'Girering';
  }
}
