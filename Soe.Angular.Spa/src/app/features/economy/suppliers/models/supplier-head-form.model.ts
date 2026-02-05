import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IPaymentInformationDTO,
  ISupplierDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SupplierDTO } from './supplier.model';
import { FormArray, FormControl } from '@angular/forms';
import { ContactAddressForm } from '@shared/components/contact-addresses/contact-addresses-form.model';
import { AccountingSettingsFormArray } from '@shared/components/accounting-settings/accounting-settings/accounting-settings-form.model';
import { arrayToFormArray, clearAndSetFormArray } from '@shared/util/form-util';
import { PaymentInformationForm } from '../../../../shared/components/payment-information/payment-information-form.model';
import { ContactAddressItem } from '@shared/components/contact-addresses/contact-addresses.model';

interface ISupplierHeadForm {
  validationHandler: ValidationHandler;
  element: ISupplierDTO | undefined;
}

export class SupplierHeadForm extends SoeFormGroup {
  supplierValidationHandler: ValidationHandler;

  get actorSupplierId(): FormControl<number> {
    return <FormControl>this.controls.actorSupplierId;
  }

  get accountingSettings() {
    return <AccountingSettingsFormArray>this.controls.accountingSettings;
  }

  get contactAddresses(): FormArray<ContactAddressForm> {
    return <FormArray>this.controls.contactAddresses;
  }

  get contactPersons(): FormArray<FormControl<number>> {
    return <FormArray>this.controls.contactPersons;
  }

  get categoryIds(): FormArray<FormControl<number>> {
    return <FormArray>this.controls.categoryIds;
  }

  get paymentInformationDomestic(): PaymentInformationForm {
    return <PaymentInformationForm>this.controls.paymentInformationDomestic;
  }

  get paymentInformationForegin(): PaymentInformationForm {
    return <PaymentInformationForm>this.controls.paymentInformationForegin;
  }

  get selectedContactPerson(): FormControl<number> {
    return <FormControl>this.controls.selectedContactPerson;
  }

  get note(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.note;
  }

  constructor({ validationHandler, element }: ISupplierHeadForm) {
    super(validationHandler, {
      actorSupplierId: new SoeTextFormControl(
        element?.actorSupplierId || 0,
        {
          isIdField: true,
        },
        'economy.supplier.supplier.supplier'
      ),
      isPrivatePerson: new SoeCheckboxFormControl(
        element?.isPrivatePerson || false,
        {},
        'common.privateperson'
      ),
      hasConsent: new SoeCheckboxFormControl(
        element?.hasConsent || false,
        {},
        'common.consent'
      ),
      consentDate: new SoeDateFormControl(
        element?.consentDate || new Date(),
        {},
        'common.consentdate'
      ),
      supplierNr: new SoeTextFormControl(
        element?.supplierNr || '',
        {
          isNameField: true,
          required: true,
          maxLength: 50,
        },
        'economy.supplier.supplier.suppliernrshort'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          required: true,
          maxLength: 100,
        },
        'common.name'
      ),
      orgNr: new SoeTextFormControl(
        element?.orgNr || '',
        {
          maxLength: 50,
        },
        'common.orgnrshort'
      ),
      vatNr: new SoeTextFormControl(
        element?.vatNr || '',
        {
          maxLength: 50,
        },
        'economy.supplier.supplier.vatnr'
      ),
      ourCustomerNr: new SoeTextFormControl(
        element?.ourCustomerNr || '',
        {
          maxLength: 50,
        },
        'economy.supplier.supplier.ourcustomernr'
      ),
      sysCountryId: new SoeSelectFormControl(
        element?.sysCountryId || 0,
        {},
        'common.country'
      ),
      sysLanguageId: new SoeSelectFormControl(
        element?.sysLanguageId || 0,
        {},
        'common.language'
      ),
      currencyId: new SoeSelectFormControl(
        element?.currencyId || 0,
        {},
        'common.currency.vouchercurrency'
      ),
      ourReference: new SoeTextFormControl(
        element?.ourReference || '',
        {
          maxLength: 200,
        },
        'economy.supplier.supplier.ourreference'
      ),
      invoiceReference: new SoeTextFormControl(
        element?.invoiceReference || '',
        {
          maxLength: 50,
        },
        'economy.supplier.supplier.invoicereference'
      ),
      manualAccounting: new SoeCheckboxFormControl(
        element?.manualAccounting || false,
        {},
        'economy.supplier.supplier.manualaccounting'
      ),
      interim: new SoeCheckboxFormControl(element?.interim || false, {}),
      note: new SoeTextFormControl(element?.note || '', {}, 'common.note'),
      showNote: new SoeCheckboxFormControl(
        element?.showNote || false,
        {},
        'economy.supplier.supplier.shownoteininvoice'
      ),
      vatType: new SoeSelectFormControl(element?.vatType, {}, 'common.vattype'),
      vatCodeId: new SoeSelectFormControl(
        element?.vatCodeId,
        {},
        'economy.accounting.vatcode.vatcode'
      ),
      paymentConditionId: new SoeSelectFormControl(
        element?.paymentConditionId,
        {},
        'economy.accounting.paymentcondition.paymentcondition'
      ),
      factoringSupplierId: new SoeSelectFormControl(
        element?.factoringSupplierId,
        {},
        'economy.supplier.supplier.factoringsupplier'
      ),
      sysWholeSellerId: new SoeSelectFormControl(
        element?.sysWholeSellerId,
        {},
        'economy.supplier.supplier.syswholeseller'
      ),
      attestWorkFlowGroupId: new SoeSelectFormControl(
        element?.attestWorkFlowGroupId,
        {},
        'economy.supplier.attestworkflow.attestworkflowgroup.attestworkflowgroup'
      ),
      deliveryTypeId: new SoeSelectFormControl(
        element?.deliveryTypeId,
        {},
        'common.customer.customer.deliverytype'
      ),
      deliveryConditionId: new SoeSelectFormControl(
        element?.deliveryConditionId,
        {},
        'common.customer.customer.deliverycondition'
      ),
      contactEcomId: new SoeSelectFormControl(
        element?.contactEcomId,
        {},
        'economy.supplier.supplier.purchaseemail'
      ),
      intrastatCodeId: new SoeSelectFormControl(
        element?.intrastatCodeId || 0,
        {},
        'common.commoditycodes.code'
      ),
      copyInvoiceNrToOcr: new SoeCheckboxFormControl(
        element?.copyInvoiceNrToOcr,
        {},
        'economy.supplier.supplier.copyinvoicenrtoocr'
      ),
      blockPayment: new SoeCheckboxFormControl(
        element?.blockPayment,
        {},
        'economy.supplier.supplier.blockpayment'
      ),
      isEDISupplier: new SoeCheckboxFormControl(
        element?.isEDISupplier,
        {},
        'economy.supplier.supplier.isedisupplier'
      ),
      accountingSettings: new AccountingSettingsFormArray(validationHandler),
      contactAddresses: new FormArray<ContactAddressForm>([]),
      categoryIds: arrayToFormArray(element?.categoryIds || []),
      paymentInformationDomestic: new PaymentInformationForm({
        paymentValidationHandler: validationHandler,
        element: element?.paymentInformationDomestic,
        isForeign: false,
      }),
      paymentInformationForegin: new PaymentInformationForm({
        paymentValidationHandler: validationHandler,
        element: element?.paymentInformationForegin,
        isForeign: true,
      }),
      selectedContactPerson: new SoeNumberFormControl(0),
    });

    this.controls.consentDate.disable();

    this.supplierValidationHandler = validationHandler;
    this.accountingSettings.rawPatch(element?.accountingSettings ?? []);
    this.setChangeHandlers();
  }

  setChangeHandlers() {
    this.get(['hasConsent'])?.valueChanges.subscribe(
      (value: boolean | undefined) => {
        if (value === true) {
          this.get(['consentDate'])?.enable();
        } else {
          this.get(['consentDate'])?.disable();
        }
      }
    );
  }

  customContactAddressesPatchValue(
    rows: ContactAddressItem[],
    allowShowSecret: boolean,
    readOnly: boolean
  ) {
    this.patchContactAddressesRows(rows, allowShowSecret, readOnly);
  }

  customContactPersonsPatchValue(contactPersonIds: number[]) {
    //Not 100% happy with this.

    //If contactPersons is not initialized, it has to default to null,
    //since the backend will remove the mappings otherwise.

    //Since the control cannot be null, we dynamically add the control
    //if the contactPersonIds are loaded.

    if (!this.contactPersons && contactPersonIds) {
      //contactPersons default to null if not initialized
      this.addControl('contactPersons', arrayToFormArray(contactPersonIds));
      return;
    }

    if (this.contactPersons && !contactPersonIds) {
      this.removeControl('contactPersons');
      return;
    }

    if (contactPersonIds) {
      clearAndSetFormArray(contactPersonIds, this.contactPersons, true);
    }
  }

  customCategoryIdsPatchValue(categoryIds: number[]) {
    clearAndSetFormArray(categoryIds, this.categoryIds, true);
  }

  customPaymentInformationDomesticPatchValue(item: IPaymentInformationDTO) {
    item && this.paymentInformationDomestic.customPatchValue(item);
  }

  customPaymentInformationForeignPatchValue(item: IPaymentInformationDTO) {
    item && this.paymentInformationForegin.customPatchValue(item);
  }

  customPatchValue(
    element: SupplierDTO,
    allowShowSecret: boolean,
    readOnly: boolean
  ) {
    this.reset(element);
    this.accountingSettings.rawPatch(element.accountingSettings);
    this.customContactAddressesPatchValue(
      element.contactAddresses,
      allowShowSecret,
      readOnly
    );
    this.customContactPersonsPatchValue(element.contactPersons);
    this.customCategoryIdsPatchValue(element.categoryIds);
    this.customPaymentInformationDomesticPatchValue(
      element.paymentInformationDomestic
    );
    this.customPaymentInformationForeignPatchValue(
      element.paymentInformationForegin
    );
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
            validationHandler: this.supplierValidationHandler,
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
}
