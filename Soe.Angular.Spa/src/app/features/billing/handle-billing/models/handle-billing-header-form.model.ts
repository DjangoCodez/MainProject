import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeDateRangeFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IHandleBillingRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SearchCustomerInvoiceRowModel } from './handle-billing.model';

interface IHandleBillingHeaderForm {
  validationHandler: ValidationHandler;
  element: any | undefined;
}

export class HandleBillingHeaderForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IHandleBillingHeaderForm) {
    super(validationHandler, {
      dateRange: new SoeDateRangeFormControl([
        element?.fromDate || new Date(),
        element?.fromDate || new Date(),
      ]),
      selectedOrders: new SoeSelectFormControl(element?.selectedOrders || []),
      selectedProjects: new SoeSelectFormControl(
        element?.selectedProjects || []
      ),
      selectedCustomers: new SoeSelectFormControl(
        element?.selectedCustomers || []
      ),
      selectedOrderTypes: new SoeSelectFormControl(
        element?.selectedOrderTypes || []
      ),
      selectedOrderContractTypes: new SoeSelectFormControl(
        element?.selectedOrderContractTypes || []
      ),
      onlyMine: new SoeCheckboxFormControl(element?.onlyMine || false),
      onlyValidToTransfer: new SoeCheckboxFormControl(
        element?.onlyValidToTransfer || false
      ),
    });
  }

  get dateRange(): SoeDateRangeFormControl {
    return <SoeDateRangeFormControl>this.controls.dateRange;
  }

  get selectedOrders(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedOrders;
  }

  get selectedProjects(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedProjects;
  }

  get selectedCustomers(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedCustomers;
  }

  get selectedOrderTypes(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedOrderTypes;
  }

  get selectedOrderContractTypes(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedOrderContractTypes;
  }

  get onlyMine(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.onlyMine;
  }

  get onlyValidToTransfer(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.onlyValidToTransfer;
  }

  getSearchModel(): SearchCustomerInvoiceRowModel {
    const searchModel: SearchCustomerInvoiceRowModel = {
      projects: this.selectedProjects.value,
      orders: this.selectedOrders.value,
      customers: this.selectedCustomers.value,
      orderTypes: this.selectedOrderTypes.value,
      orderContractTypes: this.selectedOrderContractTypes.value,
      from: new Date(this.dateRange.value[0]),
      to: new Date(this.dateRange.value[1]),
      onlyValid: this.onlyValidToTransfer.value,
      onlyMine: this.onlyMine.value,
    };

    return searchModel;
  }
}
