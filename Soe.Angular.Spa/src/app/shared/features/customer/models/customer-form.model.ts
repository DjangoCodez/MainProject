import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CustomerDTO } from './customer.model';
import { FormArray, FormControl } from '@angular/forms';
import { ContactAddressForm } from '@shared/components/contact-addresses/contact-addresses-form.model';
import {
  IAccountingSettingsRowDTO,
  ICustomerProductPriceSmallDTO,
  ICustomerUserDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CustomerOriginUserForm } from './customer-origin-user-form.model';
import { arrayToFormArray, clearAndSetFormArray } from '@shared/util/form-util';
import { AccountingSettingsForm } from '@shared/components/accounting-settings/accounting-settings/accounting-settings-form.model';
import { CustomerProductForm } from './customer-product-form.model';
import { CustomerProductPriceSmallDTO } from './customer-product.model';
import { ContactAddressItem } from '@shared/components/contact-addresses/contact-addresses.model';

interface ICustomerForm {
  validationHandler: ValidationHandler;
  element: CustomerDTO;
}

export class CustomerForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ICustomerForm) {
    super(validationHandler, {
      actorCustomerId: new SoeNumberFormControl(element?.actorCustomerId || 0, {
        isIdField: true,
      }),
      active: new SoeCheckboxFormControl(element?.active || false),
      isPrivatePerson: new SoeCheckboxFormControl(
        element?.isPrivatePerson || false
      ),
      hasConsent: new SoeCheckboxFormControl(element?.hasConsent || false),
      consentDate: new SoeDateFormControl(
        element?.consentDate || null,
        {
          required: true,
          disabled: !element?.hasConsent,
        },
        'common.consent'
      ),
      customerNr: new SoeTextFormControl(
        element?.customerNr || '',
        {
          required: true,
          maxLength: 50,
        },
        'common.customer.customer.customernr'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
          maxLength: 256,
        },
        'common.name'
      ),
      orgNr: new SoeTextFormControl(element?.orgNr || '', { maxLength: 50 }),
      vatNr: new SoeTextFormControl(element?.vatNr || '', { maxLength: 50 }),
      supplierNr: new SoeTextFormControl(element?.supplierNr || '', {
        maxLength: 50,
      }),
      departmentNr: new SoeTextFormControl(element?.departmentNr || '', {
        maxLength: 5,
      }),
      sysCountryId: new SoeSelectFormControl(
        element?.sysCountryId || undefined
      ),
      sysLanguageId: new SoeSelectFormControl(
        element?.sysLanguageId || undefined
      ),
      currencyId: new SoeSelectFormControl(
        element?.currencyId || undefined,
        {
          required: true,
        },
        'common.currency.vouchercurrency',
      ),
      contactAddresses: new FormArray<ContactAddressForm>([]),
      customerUsers: new FormArray<CustomerOriginUserForm>([]),
      participants: new SoeTextFormControl(element?.participants || '', {}),

      vatType: new SoeSelectFormControl(element?.vatType ?? 0),
      priceListTypeId: new SoeSelectFormControl(
        element?.priceListTypeId || undefined
      ),
      sysWholeSellerId: new SoeSelectFormControl(
        element?.sysWholeSellerId || undefined
      ),
      discountMerchandise: new SoeNumberFormControl(
        element?.discountMerchandise || undefined,
        { decimals: 2 }
      ),
      discountService: new SoeNumberFormControl(
        element?.discountService || undefined,
        { decimals: 2 }
      ),
      creditLimit: new SoeNumberFormControl(element?.creditLimit || undefined, {
        decimals: 2,
      }),
      discount2Merchandise: new SoeNumberFormControl(
        element?.discount2Merchandise || undefined,
        { decimals: 2 }
      ),
      discount2Service: new SoeNumberFormControl(
        element?.discount2Service || undefined,
        { decimals: 2 }
      ),
      invoiceReference: new SoeTextFormControl(
        element?.invoiceReference || '',
        { maxLength: 50 }
      ),
      invoiceDeliveryType: new SoeSelectFormControl(
        element?.invoiceDeliveryType || undefined
      ),
      invoiceDeliveryProvider: new SoeSelectFormControl(
        element?.invoiceDeliveryProvider || undefined
      ),
      contactEComId: new SoeSelectFormControl(
        element?.contactEComId || undefined
      ),
      orderContactEComId: new SoeSelectFormControl(
        element?.orderContactEComId || undefined
      ),
      reminderContactEComId: new SoeSelectFormControl(
        element?.reminderContactEComId || undefined
      ),
      contactGLNId: new SoeSelectFormControl(
        element?.contactGLNId || undefined
      ),
      invoiceLabel: new SoeTextFormControl(element?.invoiceLabel || '', {
        maxLength: 1024,
      }),
      disableInvoiceFee: new SoeCheckboxFormControl(
        element?.disableInvoiceFee || false
      ),
      addAttachementsToEInvoice: new SoeCheckboxFormControl(
        element?.addAttachementsToEInvoice || false
      ),
      addSupplierInvoicesToEInvoice: new SoeCheckboxFormControl(
        element?.addSupplierInvoicesToEInvoice || false
      ),
      triangulationSales: new SoeCheckboxFormControl(
        element?.triangulationSales || false
      ),
      isFinvoiceCustomer: new SoeCheckboxFormControl(
        element?.isFinvoiceCustomer || false
      ),
      finvoiceAddress: new SoeTextFormControl(element?.finvoiceAddress || '', {
        maxLength: 30,
        disabled: true,
      }),
      finvoiceOperator: new SoeTextFormControl(
        element?.finvoiceOperator || '',
        { maxLength: 30, disabled: true }
      ),
      deliveryTypeId: new SoeSelectFormControl(
        element?.deliveryTypeId || undefined
      ),
      deliveryConditionId: new SoeSelectFormControl(
        element?.deliveryConditionId || undefined
      ),
      paymentConditionId: new SoeSelectFormControl(
        element?.paymentConditionId || undefined
      ),
      gracePeriodDays: new SoeNumberFormControl(element?.gracePeriodDays || 0),
      invoicePaymentService: new SoeSelectFormControl(
        element?.invoicePaymentService || undefined
      ),
      bankAccountNr: new SoeTextFormControl(element?.bankAccountNr || '', {
        maxLength: 30,
      }),
      payingCustomerId: new SoeSelectFormControl(
        element?.payingCustomerId || undefined
      ),
      isCashCustomer: new SoeCheckboxFormControl(
        element?.isCashCustomer || false
      ),
      isOneTimeCustomer: new SoeCheckboxFormControl(
        element?.isOneTimeCustomer || false
      ),
      contractNr: new SoeTextFormControl(element?.contractNr || '', {
        maxLength: 100,
      }),

      agreementTemplate: new SoeSelectFormControl(
        element?.agreementTemplate || undefined
      ),
      offerTemplate: new SoeSelectFormControl(
        element?.offerTemplate || undefined
      ),
      orderTemplate: new SoeSelectFormControl(
        element?.orderTemplate || undefined
      ),
      billingTemplate: new SoeSelectFormControl(
        element?.billingTemplate || undefined
      ),

      //Settings > Block Section
      blockOrder: new SoeCheckboxFormControl(element?.blockOrder || false),
      blockInvoice: new SoeCheckboxFormControl(element?.blockInvoice || false),
      blockNote: new SoeTextFormControl(element?.blockNote || '', {
        maxLength: 30,
      }),

      categoryIds: arrayToFormArray(element?.categoryIds || []),
      importInvoicesDetailed: new SoeCheckboxFormControl(
        element?.importInvoicesDetailed || false
      ),

      accountingSettings: new FormArray<AccountingSettingsForm>([]),

      note: new SoeTextFormControl(element?.note || ''),
      showNote: new SoeCheckboxFormControl(element?.showNote || false),

      customerProducts: new FormArray<CustomerProductForm>([]),
      selectedContactPerson: new SoeNumberFormControl(0),
    });

    this.thisValidationHandler = validationHandler;

    if (element?.contactAddresses)
      this.customContactAddressesPatchValue(
        element?.contactAddresses as ContactAddressItem[],
        false,
        false
      );
  }

  get actorCustomerId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.actorCustomerId;
  }

  get active(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.active;
  }

  get isPrivatePerson(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isPrivatePerson;
  }

  get hasConsent(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.hasConsent;
  }

  get consentDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.consentDate;
  }

  get customerNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.customerNr;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get orgNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.orgNr;
  }
  get vatNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.vatNr;
  }
  get supplierNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierNr;
  }

  get sysCountryId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysCountryId;
  }

  get sysLanguageId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysLanguageId;
  }

  get currencyId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.currencyId;
  }

  get contactAddresses(): FormArray<ContactAddressForm> {
    return <FormArray>this.controls.contactAddresses;
  }

  get customerUsers(): FormArray<CustomerOriginUserForm> {
    return <FormArray>this.controls.customerUsers;
  }

  get participants(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.participants;
  }

  get contactPersons(): FormArray<FormControl<number>> {
    return <FormArray>this.controls.contactPersons;
  }

  get vatType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.vatType;
  }

  get priceListTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.priceListTypeId;
  }

  get sysWholeSellerId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysWholeSellerId;
  }

  get discountMerchandise(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.discountMerchandise;
  }

  get discountService(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.discountService;
  }

  get creditLimit(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.creditLimit;
  }

  get discount2Merchandise(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.discount2Merchandise;
  }

  get discount2Service(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.discount2Service;
  }

  get invoiceReference(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceReference;
  }

  get contactEComId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.contactEComId;
  }

  get orderContactEComId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.orderContactEComId;
  }

  get reminderContactEComId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.reminderContactEComId;
  }

  get contactGLNId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.contactGLNId;
  }

  get invoiceLabel(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceLabel;
  }

  get disableInvoiceFee(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.disableInvoiceFee;
  }

  get addAttachementsToEInvoice(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.addAttachementsToEInvoice;
  }

  get addSupplierInvoicesToEInvoice(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.addSupplierInvoicesToEInvoice;
  }

  get triangulationSales(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.triangulationSales;
  }

  get isFinvoiceCustomer(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isFinvoiceCustomer;
  }

  get finvoiceAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.finvoiceAddress;
  }

  get finvoiceOperator(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.finvoiceOperator;
  }

  get deliveryTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.deliveryTypeId;
  }

  get deliveryConditionId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.deliveryConditionId;
  }

  get paymentConditionId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.paymentConditionId;
  }

  get gracePeriodDays(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.gracePeriodDays;
  }

  get invoicePaymentService(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.invoicePaymentService;
  }

  get bankAccountNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.bankAccountNr;
  }

  get payingCustomerId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.payingCustomerId;
  }

  get isCashCustomer(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isCashCustomer;
  }

  get isOneTimeCustomer(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isOneTimeCustomer;
  }

  get agreementTemplate(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.agreementTemplate;
  }

  get offerTemplate(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.offerTemplate;
  }

  get ordertemplate(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.ordertemplate;
  }

  get billingTemplate(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.billingTemplate;
  }

  get blockOrder(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.blockOrder;
  }

  get blockInvoice(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.blockInvoice;
  }

  get blockNote(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.blockNote;
  }

  get categoryIds(): FormArray<FormControl<number>> {
    return <FormArray>this.controls.categoryIds;
  }

  get importInvoicesDetailed(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.importInvoicesDetailed;
  }

  get accountingSettings(): FormArray<AccountingSettingsForm> {
    return <FormArray>this.controls.accountingSettings;
  }

  get note(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.note;
  }

  get showNote(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showNote;
  }

  get customerProducts(): FormArray<CustomerProductForm> {
    return <FormArray>this.controls.customerProducts;
  }

  get contractNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.contractNr;
  }

  customCategoryIdsPatchValue(categoryIds: number[]) {
    clearAndSetFormArray(categoryIds, this.categoryIds);
  }

  customContactAddressesPatchValue(
    rows: ContactAddressItem[],
    allowShowSecret: boolean,
    readOnly: boolean
  ) {
    this.patchContactAddressesRows(rows, allowShowSecret, readOnly);
  }

  private patchContactAddressesRows(
    rows: ContactAddressItem[] | undefined,
    allowShowSecret: boolean,
    readOnly: boolean
  ) {
    this.contactAddresses?.clear();
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.contactAddresses.push(
          new ContactAddressForm({
            validationHandler: this.thisValidationHandler,
            element: r,
            allowShowSecret,
            readOnly,
          }),
          { emitEvent: false }
        );
      });
      this.contactAddresses.updateValueAndValidity();
    }
  }

  customOriginUsersPatchValue(originUsers: ICustomerUserDTO[]) {
    if (originUsers) {
      (this.controls.customerUsers as FormArray).clear();
      let mainUser = '';
      const users: string[] = [];
      let participant = '';
      if (originUsers) {
        for (const originUser of originUsers) {
          const row = new CustomerOriginUserForm({
            validationHandler: this.thisValidationHandler,
            element: originUser,
          });
          (this.controls.customerUsers as FormArray).push(row, {
            emitEvent: false,
          });
          if (originUser.main) {
            mainUser = originUser.name;
          } else {
            users.push(originUser.name);
          }
        }
        if (mainUser) {
          participant = '<b>' + mainUser + '</b>';
        }
        if (users.length > 0) {
          if (participant) {
            participant = participant + ',';
          }
          participant = participant + users.join(',');
        }
        this.participants.patchValue(participant);
      }
    }
  }

  customContactPersonsPatchValue(contactPersonIds: number[]) {
    // ------- Temperory solution as per the Supplier Feature. -----------//

    //If contactPersons is not initialized, it has to default to null,
    //since the backend will remove the mappings otherwise.

    //Since the control cannot be null, we dynamically add the control
    //if the contactPersonIds are loaded.

    if (!this.contactPersons && contactPersonIds) {
      this.addControl('contactPersons', arrayToFormArray(contactPersonIds));
      return;
    }

    if (this.contactPersons && !contactPersonIds) {
      this.removeControl('contactPersons');
      return;
    }

    // ------- ### Temperory solution as per the Supplier Feature. ### --------//

    if (contactPersonIds) {
      clearAndSetFormArray(contactPersonIds, this.contactPersons);
    }
  }

  customAccountingSettingsPathValue(rows: IAccountingSettingsRowDTO[]) {
    this.patchAccountingSettingsRows(rows);
  }

  private patchAccountingSettingsRows(
    rows: IAccountingSettingsRowDTO[] | undefined
  ) {
    this.accountingSettings?.clear({ emitEvent: false });
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.accountingSettings.push(
          new AccountingSettingsForm({
            validationHandler: this.thisValidationHandler,
            element: r,
          }),
          { emitEvent: false }
        );
      });
      this.accountingSettings.updateValueAndValidity();
    }
  }

  addCustomerProductRow(): CustomerProductPriceSmallDTO {
    const newRowDto = new CustomerProductPriceSmallDTO(this.getNewRowId());
    newRowDto.customerProductId = 0;
    newRowDto.productId = 0;
    newRowDto.number = '';
    newRowDto.name = '';
    newRowDto.price = 0;
    newRowDto.isDelete = false;

    const newCustomerProduct = new CustomerProductForm({
      validationHandler: this.thisValidationHandler,
      element: newRowDto,
    });

    this.customerProducts.push(newCustomerProduct, { emitEvent: false });
    this.markAsDirty();
    this.markAsTouched();
    this.customerProducts.markAsDirty();
    this.customerProducts.updateValueAndValidity();

    return newRowDto;
  }

  private getNewRowId(): number {
    let minId = 0;

    if (this.customerProducts.value.length > 0) {
      minId = (<CustomerProductPriceSmallDTO[]>this.customerProducts.value)
        .map(x => x.productRowId)
        .reduce((a, b) => Math.min(a, b));
    }

    return minId > 0 ? 0 : --minId;
  }

  customProductRowsPatchValues(pRows: ICustomerProductPriceSmallDTO[]) {
    const productRows: CustomerProductPriceSmallDTO[] =
      CustomerProductPriceSmallDTO.ToCustomerProductPriceSmallDTO(pRows);
    this.customerProducts.clear({ emitEvent: false });
    if (productRows) {
      for (const prw of productRows) {
        if (
          prw.productRowId !== null &&
          prw.productRowId !== undefined &&
          typeof prw.productRowId === 'number'
        ) {
          const row = new CustomerProductForm({
            validationHandler: this.thisValidationHandler,
            element: prw,
          });
          if (prw.isDelete) row.disable();
          this.customerProducts.push(row, { emitEvent: false });
        }
      }
      this.customerProducts.updateValueAndValidity();
    }
    return <CustomerProductPriceSmallDTO[]>this.customerProducts.value;
  }

  deleteCustomerProductRow(row: CustomerProductPriceSmallDTO) {
    const productRows = <CustomerProductPriceSmallDTO[]>(
      this.customerProducts.getRawValue()
    );
    productRows.forEach((rw, i) => {
      if (rw.productRowId == row.productRowId) {
        rw.isDelete = true;
        this.markAsDirty();
        this.customerProducts.markAsDirty();
      }
    });
    this.customProductRowsPatchValues(productRows);
  }

  removeEmptyProductRows() {
    const rows: CustomerProductPriceSmallDTO[] = [];
    this.customerProducts.value.forEach(b => {
      if (!((b.productRowId <= 0 && b.isDelete) || !b.totalAmount)) {
        rows.push(b);
      }
    });
    this.customProductRowsPatchValues(rows);
  }
}
