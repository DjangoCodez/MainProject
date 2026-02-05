import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteAccountDistributionEntries,
  getAccountDistributionEntries,
  getAccountDistributionEntriesForHead,
  reverseAccountDistributionEntries,
  transferAccountDistributionEntryToVoucher,
  transferToAccountDistributionEntry,
} from '@shared/services/generated-service-endpoints/economy/AccountDistributionEntry.endpoints';
import { Observable } from 'rxjs';
import {
  AccountDistributionEntryDTO,
  TransferToAccountDistributionEntryDTO,
} from '../models/inventory-writeoffs.model';
import {
  IDeleteDistributionEntryModel,
  IReverseAccountDistributionEntryModel,
  ISaveInventoryNotesModel,
  ITransferAccountDistributionEntryToVoucherModel,
} from '@shared/models/generated-interfaces/EconomyModels';
import { TermGroup_AccountDistributionRegistrationType } from '@shared/models/generated-interfaces/Enumerations';
import { saveNotesAndDescription } from '@shared/services/generated-service-endpoints/economy/InventoryV2.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class InventoryWriteoffsService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    periodDate: '',
    accountDistributionType: 0,
    onlyActive: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      periodDate: string;
      accountDistributionType: number;
      onlyActive: boolean;
    }
  ): Observable<AccountDistributionEntryDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<AccountDistributionEntryDTO[]>(
      getAccountDistributionEntries(
        this.getGridAdditionalProps.periodDate,
        this.getGridAdditionalProps.accountDistributionType,
        this.getGridAdditionalProps.onlyActive
      )
    );
  }

  transferToAccountDistributionEntry(
    model: TransferToAccountDistributionEntryDTO
  ): Observable<any> {
    return this.http.post<TransferToAccountDistributionEntryDTO>(
      transferToAccountDistributionEntry(),
      model
    );
  }

  reverseAccountDistributionEntries(
    model: IReverseAccountDistributionEntryModel
  ) {
    return this.http.post<BackendResponse>(
      reverseAccountDistributionEntries(),
      model
    );
  }

  getAccountDistributionEntriesForHead(
    id: number
  ): Observable<AccountDistributionEntryDTO[]> {
    return this.http.get<AccountDistributionEntryDTO[]>(
      getAccountDistributionEntriesForHead(id)
    );
  }

  transferAccountDistributionEntryToVoucher(
    model: ITransferAccountDistributionEntryToVoucherModel
  ): Observable<any> {
    return this.http.post<ITransferAccountDistributionEntryToVoucherModel>(
      transferAccountDistributionEntryToVoucher(),
      model
    );
  }

  delete(model: IDeleteDistributionEntryModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      deleteAccountDistributionEntries(),
      model
    );
  }

  saveNotesAndDescription(model: ISaveInventoryNotesModel) {
    return this.http.post<BackendResponse>(saveNotesAndDescription(), model);
  }

  setNotesIcon(item: AccountDistributionEntryDTO) {
    if (item.inventoryNotes) {
      item.notesIcon = 'file-alt';
    } else {
      item.notesIcon = 'file';
    }
  }

  setSequenceNumber(
    item: AccountDistributionEntryDTO,
    defaultSequenceNumber: any
  ) {
    if (this.isCustomerInvoice(item)) {
      item.sourceSeqNr =
        item.sourceCustomerInvoiceSeqNr ?? defaultSequenceNumber;
    } else if (this.isSupplierInvoice(item)) {
      item.sourceSeqNr =
        item.sourceSupplierInvoiceSeqNr ?? defaultSequenceNumber;
    } else if (this.isVoucher(item)) {
      item.sourceSeqNr = item.sourceVoucherNr;
    } else if (this.hasSupplierInvoice(item)) {
      item.sourceSeqNr =
        item.sourceSupplierInvoiceSeqNr ?? defaultSequenceNumber;
    }
  }

  private isCustomerInvoice(item: AccountDistributionEntryDTO): boolean {
    return (
      item.registrationType ===
      TermGroup_AccountDistributionRegistrationType.CustomerInvoice
    );
  }

  private isSupplierInvoice(item: AccountDistributionEntryDTO): boolean {
    return (
      item.registrationType ===
      TermGroup_AccountDistributionRegistrationType.SupplierInvoice
    );
  }

  private isVoucher(item: AccountDistributionEntryDTO): boolean {
    return (
      item.registrationType ===
      TermGroup_AccountDistributionRegistrationType.Voucher
    );
  }

  private hasSupplierInvoice(item: AccountDistributionEntryDTO): boolean {
    return item.supplierInvoiceId != null;
  }

  setInventoryName(item: AccountDistributionEntryDTO) {
    if (item.inventoryNr) {
      item.inventoryName = item.inventoryNr + ' - ' + item.inventoryName;
    }
  }
}
