import {
  SupplierInvoiceCostAllocationDTO,
  SupplierInvoiceDTO,
} from '@features/economy/shared/supplier-invoice/models/supplier-invoice.model';
import { SupplierInvoicesArrivalHallDTO } from '@features/economy/supplier-invoices-arrival-hall/models/supplier-invoices-arrival-hall.model';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TermGroup_InvoiceVatType } from '@shared/models/generated-interfaces/Enumerations';
import { arrivalHallToInvoiceDTO } from './model-converter';
import { AbstractControl, FormArray } from '@angular/forms';
import { AccountingRowsForm } from '@shared/components/accounting-rows/accounting-rows/accounting-rows-form.model';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { SupplierInvoiceCostAllocationRowsForm } from '../components/cost-allocation/models/cost-allocation-form.model';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import { IProjectTinyDTO } from '@shared/models/generated-interfaces/ProjectDTOs';

interface ISupplierInvoiceForm {
  validationHandler: ValidationHandler;
  element: SupplierInvoiceDTO | SupplierInvoicesArrivalHallDTO | undefined;
}

export const constructIdField = (dto: any) => {
  let idField = '';
  if (dto.invoiceId) {
    idField += `invoiceId-${dto.invoiceId};`;
    return idField.slice(0, -1);
  }
  if (dto.scanningEntryId) idField += `scanningEntryId-${dto.scanningEntryId};`;
  if (dto.ediEntryId) idField += `ediEntryId-${dto.ediEntryId};`;
  if (dto.supplierInvoiceHeadIOId)
    idField += `supplierInvoiceHeadIOId-${dto.supplierInvoiceHeadIOId};`;
  return idField ? idField.slice(0, -1) : 'unknown-0';
};

export const deconstructIdField = (idField: string) => {
  const result = {
    invoiceId: undefined as number | undefined,
    scanningEntryId: undefined as number | undefined,
    ediEntryId: undefined as number | undefined,
    hasId: false,
  };

  if (!idField || idField === 'unknown-0') {
    return result;
  }

  const idPairs = idField.split(';');

  for (const pair of idPairs) {
    const [type, id] = pair.split('-');
    const numericId = Number(id);

    if (!isNaN(numericId)) {
      switch (type) {
        case 'invoiceId':
          result.invoiceId = numericId;
          result.hasId = true;
          break;
        case 'scanningEntryId':
          result.scanningEntryId = numericId;
          result.hasId = true;
          break;
        case 'ediEntryId':
          result.ediEntryId = numericId;
          result.hasId = true;
          break;
      }
    }
  }

  return result;
};

export class SupplierInvoiceForm extends SoeFormGroup {
  private suppValidationHandler: ValidationHandler;

  constructor(
    { validationHandler, element }: ISupplierInvoiceForm,
    isReadOnly = false
  ) {
    const dto = arrivalHallToInvoiceDTO(element);
    super(validationHandler, {
      // Main fields
      idField: new SoeTextFormControl('', {
        isIdField: true,
      }),
      ediEntryId: new SoeNumberFormControl(dto?.ediEntryId),
      scanningEntryId: new SoeNumberFormControl(dto?.scanningEntryId),
      invoiceId: new SoeNumberFormControl(dto?.invoiceId),
      seqNr: new SoeTextFormControl(dto?.seqNr),
      status: new SoeSelectFormControl(dto?.originStatusName),
      source: new SoeSelectFormControl(dto?.source),
      invoiceNr: new SoeTextFormControl(
        dto?.invoiceNr,
        { required: !isReadOnly, isNameField: true },
        'economy.supplier.invoice.invoicenr'
      ),
      ocr: new SoeTextFormControl(
        dto?.ocr,
        { required: false },
        'economy.supplier.invoice.ocr'
      ),

      currencyId: new SoeSelectFormControl(
        dto.currencyId,
        { required: false },
        'economy.spplier.invoice.currency'
      ),
      currencyDate: new SoeDateFormControl(
        dto.currencyDate || null,
        { required: false },
        'economy.supplier.invoice.currencydate'
      ),
      currencyRate: new SoeNumberFormControl(
        dto.currencyRate || 1,
        { required: false, minDecimals: 1, maxDecimals: 2 },
        'economy.supplier.invoice.currencyrate'
      ),

      // Supplier field
      actorId: new SoeSelectFormControl(
        dto?.actorId,
        { required: !isReadOnly },
        'economy.supplier.supplier.supplier'
      ),
      invoiceImage: new SoeTextFormControl(undefined),

      // Date fields
      invoiceDate: new SoeDateFormControl(
        dto?.invoiceDate || null,
        { required: !isReadOnly },
        'economy.supplier.invoice.invoicedate'
      ),
      voucherDate: new SoeDateFormControl(
        dto?.voucherDate || null,
        { required: !isReadOnly },
        'economy.supplier.invoice.voucherdate'
      ),
      dueDate: new SoeDateFormControl(
        dto?.dueDate || null,
        { required: !isReadOnly },
        'economy.supplier.invoice.duedate'
      ),

      // Amounts: mapping to the appropriate DTO properties
      totalAmount: new SoeNumberFormControl(
        dto?.totalAmount ?? 0,
        { required: !isReadOnly, minDecimals: 1, maxDecimals: 2 },
        'economy.supplier.invoice.total'
      ),
      totalAmountCurrency: new SoeNumberFormControl(
        dto?.totalAmountCurrency ?? 0,
        { required: !isReadOnly, minDecimals: 1, maxDecimals: 2 },
        'economy.supplier.invoice.total'
      ),
      vatAmount: new SoeNumberFormControl(
        dto?.totalAmountCurrency ?? 0,
        { required: !isReadOnly, minDecimals: 1, maxDecimals: 2 },
        'economy.supplier.invoice.vat'
      ),
      vatAmountCurrency: new SoeNumberFormControl(
        dto?.vatAmountCurrency ?? 0,
        { required: !isReadOnly, minDecimals: 1, maxDecimals: 2 },
        'economy.supplier.invoice.vat'
      ),

      // VAT fields
      vatType: new SoeSelectFormControl(
        dto?.vatType || TermGroup_InvoiceVatType.Merchandise,
        { required: !isReadOnly },
        'economy.supplier.invoice.vattype'
      ),
      // Assuming you want to select a VAT code from a list, mapping it from vatCodeId:
      vatCodeId: new SoeSelectFormControl(
        dto?.vatCodeId || null,
        { required: false },
        'economy.supplier.invoice.vatcode'
      ),

      // References
      // Assuming 'referenceYour' contains the supplier reference
      supplierReference: new SoeTextFormControl(
        dto?.referenceYour || '',
        { required: false },
        'economy.supplier.invoice.yourreference'
      ),
      ourReference: new SoeTextFormControl(
        dto?.referenceOur || '',
        { required: false },
        'economy.supplier.invoice.ourreference'
      ),

      paymentInformationRowId: new SoeSelectFormControl(
        dto?.paymentNr || null,
        { required: false }
      ),
      paymentNr: new SoeSelectFormControl(
        dto?.paymentNr || null,
        { required: false },
        'economy.supplier.invoice.paytoaccount'
      ),
      paymentTypeName: new SoeTextFormControl(''),

      timeDiscountDate: new SoeDateFormControl(dto?.timeDiscountDate || null),
      timeDiscountPercent: new SoeNumberFormControl(
        dto?.timeDiscountPercent || 0
      ),
      supplierInvoiceCostAllocationRows:
        new FormArray<SupplierInvoiceCostAllocationRowsForm>([]),
      projectId: new SoeSelectFormControl(dto?.projectId || null),
      orderCustomerInvoiceId: new SoeSelectFormControl(
        dto?.orderCustomerInvoiceId || null
      ),
      accountingRows: new FormArray<AccountingRowsForm>([]),
    });
    this.setIdField();
    this.suppValidationHandler = validationHandler;
  }

  public setIdField() {
    this.patchValue({
      idField: constructIdField(this.value),
    });
  }

  get idField() {
    return this.get('idField') as SoeTextFormControl;
  }

  get scanningEntryId() {
    return this.get('scanningEntryId') as SoeNumberFormControl;
  }

  get ediEntryId() {
    return this.get('ediEntryId') as SoeNumberFormControl;
  }

  get invoiceId() {
    return this.get('invoiceId') as SoeNumberFormControl;
  }

  get actorId() {
    return this.get('actorId') as SoeNumberFormControl;
  }

  get invoiceNr() {
    return this.get('invoiceNr') as SoeTextFormControl;
  }
  get ocr() {
    return this.get('ocr') as SoeTextFormControl;
  }

  get paymentNr() {
    return this.get('paymentNr') as SoeTextFormControl;
  }
  get paymentInformationRowId() {
    return this.get('paymentInformationRowId') as SoeSelectFormControl;
  }

  get invoiceDate() {
    return this.get('invoiceDate') as SoeDateFormControl;
  }
  get dueDate() {
    return this.get('dueDate') as SoeDateFormControl;
  }
  get voucherDate() {
    return this.get('voucherDate') as SoeDateFormControl;
  }

  get currencyId() {
    return this.get('currencyId') as SoeSelectFormControl;
  }
  get currencyRate() {
    return this.get('currencyRate') as SoeNumberFormControl;
  }
  get currencyDate() {
    return this.get('currencyDate') as SoeDateFormControl;
  }
  get currencyDateValue() {
    return this.currencyDate.value;
  }

  get vatType() {
    return this.get('vatType') as SoeSelectFormControl;
  }
  get vatCodeId() {
    return this.get('vatCodeId') as SoeSelectFormControl;
  }

  get totalAmountCurrency() {
    return this.get('totalAmountCurrency') as SoeNumberFormControl;
  }
  get totalAmount() {
    return this.get('totalAmount') as SoeNumberFormControl;
  }
  get vatAmountCurrency() {
    return this.get('vatAmountCurrency') as SoeNumberFormControl;
  }
  get vatAmount() {
    return this.get('vatAmount') as SoeNumberFormControl;
  }

  get timeDiscountDate() {
    return this.get('timeDiscountDate') as SoeDateFormControl;
  }
  get timeDiscountPercent() {
    return this.get('timeDiscountPercent') as SoeNumberFormControl;
  }

  get projectId() {
    return this.get('projectId') as SoeSelectFormControl;
  }
  get orderCustomerInvoiceId() {
    return this.get('orderCustomerInvoiceId') as SoeSelectFormControl;
  }

  get accountingRows() {
    return this.get('accountingRows') as FormArray<AccountingRowsForm>;
  }

  get supplierInvoiceCostAllocationRows() {
    return this.get(
      'supplierInvoiceCostAllocationRows'
    ) as FormArray<SupplierInvoiceCostAllocationRowsForm>;
  }

  patchSupplierInvoiceCostAllocationRow(
    r: SupplierInvoiceCostAllocationDTO | undefined
  ) {
    if (r) {
      this.supplierInvoiceCostAllocationRows.push(
        new SupplierInvoiceCostAllocationRowsForm({
          validationHandler: this.suppValidationHandler,
          element: r,
        }),
        { emitEvent: false }
      );
    }
  }

  public patchValueOrderProject(
    order: ICustomerInvoiceSmallGridDTO | undefined,
    project: IProjectTinyDTO | undefined
  ) {
    if (order) {
      this.orderCustomerInvoiceId.setValue(order.invoiceId);
    }
    if (project) {
      this.projectId.setValue(project.projectId);
    }
  }

  public setFormDirty() {
    this.markAsDirty();
    this.markAsTouched();
    this.updateValueAndValidity();
  }

  patchSupplierInvoiceCostAllocationRows(
    rows: SupplierInvoiceCostAllocationDTO[] | undefined
  ) {
    this.supplierInvoiceCostAllocationRows?.clear();
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.patchSupplierInvoiceCostAllocationRow(r);
      });
      this.supplierInvoiceCostAllocationRows.updateValueAndValidity();
    }
  }

  patchAccountingRow(r: AccountingRowDTO | undefined) {
    if (r) {
      this.accountingRows.push(
        new AccountingRowsForm({
          validationHandler: this.suppValidationHandler,
          element: r,
        }),
        { emitEvent: false }
      );
    }
  }

  patchAccountingRows(rows: AccountingRowDTO[] | undefined, emitEvent = true) {
    this.accountingRows?.clear({ emitEvent: emitEvent });
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.patchAccountingRow(r);
      });
      this.accountingRows.updateValueAndValidity({ emitEvent: emitEvent });
    }
  }

  public patchSilently(values: Partial<typeof this.value>) {
    this.patchValue(values as any, { emitEvent: false, onlySelf: true });
  }

  // small helper to guard patches (no-op if equal)
  public maybeSet<T>(ctrl: AbstractControl<T, T>, next: T | null | undefined) {
    const current = ctrl.value;
    if (next === undefined) return;
    if (current !== next) ctrl.setValue(next as any);
  }
}
