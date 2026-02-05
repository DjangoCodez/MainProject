import { Component, Input, OnInit, inject } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  CustomerInvoiceSearchDTO,
  ICustomerInvoiceSearchResultDTO,
  InvoiceGridFormDTO,
  SearchCustomerInvoiceDTO,
} from '../../model/customer-invoice-search.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { SoeOriginType } from '@shared/models/generated-interfaces/Enumerations';
import { CustomerInvoiceSearchForm } from '../../model/customer-invoice-search-form.model';
import { ValidationHandler } from '@shared/handlers';
import { SelectCustomerInvoiceService } from '../../service/select-customer-invoice.service';
import { Perform } from '@shared/util/perform.class'
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { BehaviorSubject, tap } from 'rxjs';
import { PaymentImportIODTO } from '@features/economy/import-payments/models/import-payments.model';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'soe-select-customer-invoice-dialog',
  templateUrl: './select-customer-invoice-dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class SelectCustomerInvoiceDialogComponent
  extends DialogComponent<CustomerInvoiceSearchDTO>
  implements OnInit
{
  @Input() currentMainInvoiceId: any;

  // data load
  handler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  selectCustomerInvoiceService = inject(SelectCustomerInvoiceService);
  performInvoices = new Perform<ICustomerInvoiceSearchResultDTO[]>(
    this.progressService
  );
  invoiceData = new BehaviorSubject<ICustomerInvoiceSearchResultDTO[]>([]);

  validationHandler = inject(ValidationHandler);
  form: CustomerInvoiceSearchForm = new CustomerInvoiceSearchForm({
    validationHandler: this.validationHandler,
    element: new InvoiceGridFormDTO(),
  });

  originType = SoeOriginType.Order;
  selectedInvoices: SearchCustomerInvoiceDTO[] | undefined;
  isHideProjects = false;
  importRow: PaymentImportIODTO | undefined;
  paidAmount: string = '';
  paidDate: string = '';
  invoiceNr: string = '';

  ngOnInit() {
    if (this.data.invoiceValue) {
      this.form?.invoiceNumber.patchValue(this.data.invoiceValue.projectNr);

      const model = new SearchCustomerInvoiceDTO();
      model.customerName = '';
      model.customerNr = '';
      model.externalNr = '';
      model.internalText = '';
      model.number = '';
      model.originType = this.data.originType;
      model.projectName = '';
      model.projectNr = '';
      model.includeVoucher = true;
      if (
        this.data.invoiceValue.projectId &&
        this.data.invoiceValue.projectId > 0
      ) {
        model.projectId = this.data.invoiceValue.projectId;
      } else {
        model.projectId = undefined;
      }
      if (!model.projectId || model.projectId === 0) {
        this.isHideProjects = true;
      }

      if (
        (model.customerId && model.customerId > 0) ||
        (model.projectId && model.projectId > 0) ||
        (model.currentMainInvoiceId && model.currentMainInvoiceId > 0)
      ) {
        this.loadInvoices(model);
      }
    } else {
      this.isHideProjects = true;
    }

    if (this.data.importRow) {
      this.importRow = this.data.importRow;
      this.invoiceNr = this.importRow.invoiceNr;
      this.paidAmount = this.importRow.paidAmount?.toFixed(2).toString() || '';
      const pipe = new DatePipe(SoeConfigUtil.language);
      this.paidDate =
        pipe.transform(this.importRow.paidDate, 'shortDate') || '';
    }
    if (this.data.originType) {
      this.originType = this.data.originType;
    }

    this.form?.invoiceNumber.disable();
  }

  cancel() {
    this.dialogRef.close(this.data.invoiceValue);
  }

  delete() {
    this.dialogRef.close(this.selectedInvoices);
  }

  protected ok(): void {
    if (this.selectedInvoices) {
      this.dialogRef.close(this.selectedInvoices);
    } else this.cancel();
  }

  changeSelection(event: any) {
    this.selectedInvoices = event;
  }

  onRowDoubleClicked(data: any) {
    this.selectedInvoices = data;
    this.ok();
  }

  clearProject() {
    this.isHideProjects = true;
    this.form?.invoiceNumber.patchValue('');

    const model = new SearchCustomerInvoiceDTO();
    model.customerName = '';
    model.customerNr = '';
    model.externalNr = '';
    model.internalText = '';
    model.number = '';
    model.originType = this.originType;
    model.projectName = '';
    model.projectNr = '';
    model.projectId = undefined;
    model.includeVoucher = true;
    this.loadInvoices(model);
  }

  filterChange(value: any) {
    const model = new SearchCustomerInvoiceDTO();
    model.customerName = value.customerName ? value.customerName?.filter : '';
    model.customerNr = value.customerNr ? value.customerNr?.filter : '';
    model.internalText = value.internalText ? value.internalText?.filter : '';
    model.number = value.number ? value.number.filter : '';
    model.originType = this.originType;
    model.projectName = value.projectName ? value.projectName.filter : '';
    model.projectNr = value.projectNr ? value.projectNr.filter : '';
    model.projectId = value.projectId ? value.projectId : undefined;
    model.includeVoucher = true;
    this.loadInvoices(model);
  }

  loadInvoices(model: SearchCustomerInvoiceDTO) {
    this.performInvoices.load(
      this.selectCustomerInvoiceService.getInvoicesBySearch(model).pipe(
        tap(value => {
          this.invoiceData.next(value);
        })
      )
    );
  }
}
