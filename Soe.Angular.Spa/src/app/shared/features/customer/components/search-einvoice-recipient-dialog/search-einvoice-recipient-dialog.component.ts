import { Component, inject } from '@angular/core';
import {
  EInvoiceRecipientModelDTO,
  EInvoiceRecipientSearchDTO,
  SearchEinvoiceRecipientDialogData,
} from '../../models/search-einvoice-recipient-dialog.model';
import { BehaviorSubject, map, tap } from 'rxjs';
import { SearchEinvoiceRecipientService } from '../../services/select-einvoice-recipient.service';
import { EinvoiceRecipientLookupForm } from '../../models/einvoice-recipient-lookup-form.model';
import { ValidationHandler } from '@shared/handlers';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { IGridFilterModified } from '@ui/grid/interfaces';

@Component({
  selector: 'soe-search-einvoice-recipient-dialog',
  standalone: false,
  templateUrl: './search-einvoice-recipient-dialog.component.html',
  providers: [FlowHandlerService],
})
export class SearchEinvoiceRecipientDialogComponent extends DialogComponent<SearchEinvoiceRecipientDialogData> {
  isSearching: boolean = false;
  einvoiceRecipientData = new BehaviorSubject<EInvoiceRecipientModelDTO[]>([]);
  searchEinvoiceRecipientService = inject(SearchEinvoiceRecipientService);
  progressService = inject(ProgressService);
  performRecipients = new Perform<EInvoiceRecipientModelDTO[]>(
    this.progressService
  );
  selectedRecipient: EInvoiceRecipientModelDTO | undefined;

  validationHandler = inject(ValidationHandler);
  form: EinvoiceRecipientLookupForm = new EinvoiceRecipientLookupForm({
    validationHandler: this.validationHandler,
    element: new EInvoiceRecipientModelDTO(),
  });

  searchRecipients(model: EInvoiceRecipientSearchDTO) {
    if (!this.isValidSearchModel(model)) {
      this.einvoiceRecipientData.next([]);
      this.isSearching = false;
      return;
    }

    this.isSearching = true;
    model.receiveElectronicInvoiceCapability = true;
    this.performRecipients.load(
      this.searchEinvoiceRecipientService.getRecipientsBySearch(model).pipe(
        map((searchResults: EInvoiceRecipientSearchDTO[]) =>
          searchResults.map(
            result =>
              ({
                name: result.name,
                orgNo: result.orgNo,
                vatNo: result.vatNo,
                gln: result.gln,
              }) as EInvoiceRecipientModelDTO
          )
        ),
        tap((recipients: EInvoiceRecipientModelDTO[]) => {
          this.einvoiceRecipientData.next(recipients);
          this.isSearching = false;
        })
      )
    );
  }

  cancel() {
    this.dialogRef.close(this.data.einvoiceRecipientValue);
  }

  protected update(): void {
    if (this.selectedRecipient) this.dialogRef.close(this.selectedRecipient);
  }

  changeSelection(selected: EInvoiceRecipientModelDTO) {
    this.selectedRecipient = selected;
  }

  filterChanged(value: IGridFilterModified) {
    const model = new EInvoiceRecipientSearchDTO();
    model.name = value.name ? value.name.filter : '';
    model.orgNo = value.orgNo ? value.orgNo.filter : '';
    model.vatNo = value.vatNo ? value.vatNo.filter : '';
    model.gln = value.gln ? value.gln.filter : '';

    if (!this.isSearching) {
      this.searchRecipients(model);
    }
  }

  private isValidSearchModel(model: EInvoiceRecipientSearchDTO): boolean {
    return !!(model.name || model.orgNo || model.vatNo || model.gln);
  }
}
