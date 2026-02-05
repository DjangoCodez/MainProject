import { SupplierInvoiceCostAllocationDTO } from '@features/economy/shared/supplier-invoice/models/supplier-invoice.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { IProjectTinyDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { IEmployeeSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
interface ISupplierInvoiceCostAllocationRowsForm {
  validationHandler: ValidationHandler;
  element: SupplierInvoiceCostAllocationDTO | undefined;
}

export class SupplierInvoiceCostAllocationRowsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({
    validationHandler,
    element,
  }: ISupplierInvoiceCostAllocationRowsForm) {
    super(validationHandler, {
      customerInvoiceRowId: new SoeNumberFormControl(
        element?.customerInvoiceRowId || undefined
      ),
      timeCodeTransactionId: new SoeNumberFormControl(
        element?.timeCodeTransactionId || 0
      ),
      supplierInvoiceId: new SoeNumberFormControl(
        element?.supplierInvoiceId || 0
      ),
      createdDate: new SoeDateFormControl(element?.createdDate || new Date()),
      projectId: new SoeNumberFormControl(
        element?.projectId || 0,
        { required: true },
        'economy.supplier.invoice.project'
      ),
      orderId: new SoeNumberFormControl(
        element?.orderId || 0,
        { required: true },
        'economy.supplier.invoice.customerinvoice'
      ),
      attestStateId: new SoeNumberFormControl(element?.attestStateId || 0),
      timeInvoiceTransactionId: new SoeNumberFormControl(
        element?.timeInvoiceTransactionId || 0
      ),
      projectAmount: new SoeNumberFormControl(element?.projectAmount || 0.0, {
        decimals: 2,
      }),
      projectAmountCurrency: new SoeNumberFormControl(
        element?.projectAmountCurrency || 0.0,
        { decimals: 2 }
      ),
      rowAmount: new SoeNumberFormControl(element?.rowAmount || 0.0, {
        decimals: 2,
      }),
      rowAmountCurrency: new SoeNumberFormControl(
        element?.rowAmountCurrency || 0.0,
        { decimals: 2 }
      ),
      orderAmount: new SoeNumberFormControl(element?.orderAmount || 0.0, {
        decimals: 2,
      }),
      orderAmountCurrency: new SoeNumberFormControl(
        element?.orderAmountCurrency || 0.0,
        { maxDecimals: 2 }
      ),
      supplementCharge: new SoeNumberFormControl(
        element?.supplementCharge || 0.0,
        { maxDecimals: 2 }
      ),

      chargeCostToProject: new SoeCheckboxFormControl(
        element?.chargeCostToProject || false
      ),
      includeSupplierInvoiceImage: new SoeCheckboxFormControl(
        element?.includeSupplierInvoiceImage || false
      ),
      isReadOnly: new SoeCheckboxFormControl(element?.isReadOnly || false),

      productId: new SoeNumberFormControl(element?.productId || 0),
      productNr: new SoeTextFormControl(element?.productNr || ''),
      productName: new SoeTextFormControl(element?.productName || ''),
      timeCodeId: new SoeNumberFormControl(element?.timeCodeId || 0),
      timeCodeCode: new SoeTextFormControl(element?.timeCodeCode || ''),
      timeCodeDescription: new SoeTextFormControl(
        element?.timeCodeDescription || ''
      ),
      timeCodeName: new SoeTextFormControl(element?.timeCodeName || ''),
      employeeId: new SoeNumberFormControl(element?.employeeId || 0),
      employeeNr: new SoeTextFormControl(element?.employeeNr || ''),
      employeeName: new SoeTextFormControl(element?.employeeName || ''),
      employeeDescription: new SoeTextFormControl(
        element?.employeeDescription || ''
      ),
      projectNr: new SoeTextFormControl(element?.projectNr || ''),
      projectName: new SoeTextFormControl(element?.projectName || ''),
      projectNrName: new SoeTextFormControl(element?.projectNrName || ''),
      orderNr: new SoeTextFormControl(element?.orderNr || ''),
      customerInvoiceNumberName: new SoeTextFormControl(
        element?.customerInvoiceNumberName || ''
      ),
      attestStateName: new SoeTextFormControl(element?.attestStateName || ''),
      attestStateColor: new SoeTextFormControl(element?.attestStateColor || ''),
      state: new SoeNumberFormControl(element?.state, {}),
      isTransferToOrderRow: new SoeCheckboxFormControl(
        element?.isTransferToOrderRow || false
      ),
      isConnectToProjectRow: new SoeCheckboxFormControl(
        element?.isConnectToProjectRow || false
      ),
      remainingAllocationAmount: new SoeNumberFormControl(
        element?.remainingAllocationAmount || 0.0,
        { decimals: 2 }
      ),
    });

    this.thisValidationHandler = validationHandler;
  }

  get customerInvoiceRowId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.customerInvoiceRowId;
  }

  get timeCodeTransactionId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.timeCodeTransactionId;
  }

  get supplierInvoiceId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.supplierInvoiceId;
  }

  get createdDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.createdDate;
  }

  get projectId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.projectId;
  }

  get orderId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.orderId;
  }

  get attestStateId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.attestStateId;
  }

  get timeInvoiceTransactionId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.timeInvoiceTransactionId;
  }

  get projectAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.projectAmount;
  }

  get projectAmountCurrency(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.projectAmountCurrency;
  }

  get rowAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.rowAmount;
  }

  get rowAmountCurrency(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.rowAmountCurrency;
  }

  get orderAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.orderAmount;
  }

  get orderAmountCurrency(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.orderAmountCurrency;
  }

  get supplementCharge(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.supplementCharge;
  }

  get chargeCostToProject(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.chargeCostToProject;
  }

  get includeSupplierInvoiceImage(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.includeSupplierInvoiceImage;
  }

  get isReadOnly(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isReadOnly;
  }

  get productId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.productId;
  }

  get productNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productNr;
  }

  get productName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productName;
  }

  get timeCodeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.timeCodeId;
  }

  get timeCodeCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeCodeCode;
  }

  get timeCodeDescription(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeCodeDescription;
  }

  get timeCodeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeCodeName;
  }

  get employeeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.employeeId;
  }

  get employeeNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeNr;
  }

  get employeeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeName;
  }

  get employeeDescription(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeDescription;
  }

  get projectNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.projectNr;
  }

  get projectName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.projectName;
  }
  get projectNrName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.projectNrName;
  }

  get orderNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.orderNr;
  }

  get customerInvoiceNumberName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.customerInvoiceNumberName;
  }

  get attestStateName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.attestStateName;
  }

  get attestStateColor(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.attestStateColor;
  }

  get state(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.state;
  }

  get isTransferToOrderRow(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isTransferToOrderRow;
  }

  get isConnectToProjectRow(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isConnectToProjectRow;
  }

  get remainingAllocationAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.remainingAllocationAmount;
  }
}

export class CostAllocationDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  supplierInvoiceId!: number;
  customerInvoices?: ICustomerInvoiceSmallGridDTO[];
  projects?: IProjectTinyDTO[];
  products?: IProductSmallDTO[];
  rowItem?: SupplierInvoiceCostAllocationDTO;
  orderAmountCurrency: number = 0;
  rowAmountCurrency: number = 0;
  invoiceTotalAmount: number = 0;
  totalAccolationAmount: number = 0;
  isNew!: boolean;
  employees?: IEmployeeSmallDTO[];
  timeCodes?: ISmallGenericType[];
  projectAmountCurrency: number = 0;
  costAllocationMode: CostAllocationMode = CostAllocationMode.None;
  chargeCostToProject: boolean = false;
}

export class CostAllocationDialogDataResult {
  result!: boolean;
  rowItem?: SupplierInvoiceCostAllocationDTO;
  isNew!: boolean;
  costAllocationMode!: CostAllocationMode;
}

export enum CostAllocationMode {
  None = 0,
  ChargedToProject = 1,
  ReBilled = 2,
}
