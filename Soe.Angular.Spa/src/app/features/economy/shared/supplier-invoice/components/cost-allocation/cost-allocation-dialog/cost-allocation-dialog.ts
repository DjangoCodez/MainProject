import {
  Component,
  computed,
  inject,
  OnInit,
  signal,
  WritableSignal,
} from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';

import { ValidationHandler } from '@shared/handlers/validation.handler';
import { SupplierInvoiceCostAllocationDTO } from '../../../models/supplier-invoice.model';
import { TranslateService } from '@ngx-translate/core';

import {
  CostAllocationDialogData,
  CostAllocationDialogDataResult,
  CostAllocationMode,
  SupplierInvoiceCostAllocationRowsForm,
} from '../models/cost-allocation-form.model';
import { CostAllocationValidators } from '../models/cost-allocation-form-validators.model';
import { IProjectTinyDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { IEmployeeSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Component({
  selector: 'soe-cost-allocation-dialog',
  templateUrl: './cost-allocation-dialog.html',
  standalone: false,
})
export class CostAllocationDialog
  extends DialogComponent<CostAllocationDialogData>
  implements OnInit
{
  translateService = inject(TranslateService);
  validationHandler = inject(ValidationHandler);
  remainingAmount: WritableSignal<number> = signal(0);
  costAllocationMode: CostAllocationMode = CostAllocationMode.None;
  isCostAllocationModeChargedToProject = signal(false);
  customerInvoices: ICustomerInvoiceSmallGridDTO[] = [];
  filteredCustomerInvoices: ICustomerInvoiceSmallGridDTO[] = [];
  timeCodes: ISmallGenericType[] = [];
  employees: ISmallGenericType[] = [];
  employeeSmallDTOs: IEmployeeSmallDTO[] = [];

  projectTinyDTOs: IProjectTinyDTO[] = [];
  projects: ISmallGenericType[] = [];
  products: IProductSmallDTO[] = [];
  remainingAmountMessage = computed(() => {
    const message = this.translateService.instant(
      'economy.supplierInvoice.costAllocation.remainingAmount.message'
    );
    return `${message} : ${this.remainingAmount().toFixed(2)}`;
  });

  form: SupplierInvoiceCostAllocationRowsForm =
    new SupplierInvoiceCostAllocationRowsForm({
      validationHandler: this.validationHandler,
      element: new SupplierInvoiceCostAllocationDTO(),
    });
  isNew: boolean = false;

  invoiceTotalAmount: number = 0;
  totalAccolationAmount: number = 0;

  ngOnInit(): void {
    // Initialize form with data from dialog input
    this.setDialogData();
    this.setDisableControls();
    this.setHandlers();
    this.addValidators();
  }

  //#region Helper methods

  private setDisableControls() {
    this.form.orderAmountCurrency.disable();
  }

  private addValidators(): void {
    this.form.addValidators(CostAllocationValidators.overCostAllocation);
  }

  private setHandlers(): void {
    if (this.costAllocationMode === CostAllocationMode.ReBilled) {
      this.form.supplementCharge.valueChanges.subscribe(value => {
        this.calculateAndPatchOrderAmountCurrency();
      });

      this.form.rowAmountCurrency.valueChanges.subscribe(value => {
        this.calculateAndPatchOrderAmountCurrency();
      });
    } else {
      this.form.employeeId.valueChanges.subscribe(empId => {
        const employee = this.employeeSmallDTOs.find(
          e => e.employeeId === empId
        );
        if (employee)
          this.form.patchValue({
            employeeName: employee.name,
            employeeNr: employee.employeeNr,
          });
      });
      this.form.timeCodeId.valueChanges.subscribe(timeCodeId => {
        const timeCode = this.timeCodes.find(e => e.id === timeCodeId);
        if (timeCode)
          this.form.patchValue({
            timeCodeName: timeCode.name,
          });
      });

      this.form.projectAmountCurrency.valueChanges.subscribe(value => {
        this.calculateRemainingAmount(value);
      });
      this.form.chargeCostToProject.valueChanges.subscribe((value: boolean) => {
        const currentVal = this.form.projectAmountCurrency.value;
        this.calculateRemainingAmount(currentVal);
      });
    }
    this.form.orderId.valueChanges.subscribe(orderId => {
      const order = this.customerInvoices.find(p => p.invoiceId === orderId);
      if (order) {
        this.form.patchValue({
          customerInvoiceNumberName:
            order.customerInvoiceNumberNameWithoutDescription,
          orderNr: order.invoiceNr,
        });
      }
    });

    this.form.projectId.valueChanges.subscribe(projId => {
      const project = this.projectTinyDTOs.find(p => p.projectId === projId);
      if (project) {
        this.form.patchValue({
          projectName: project.name,
          projectNr: project.number,
          projectNrName: `${project.number} ${project.name}`,
        });
      }
    });
  }

  private calculateAndPatchOrderAmountCurrency() {
    let orderAmount = 0;
    if (
      !this.form.supplementCharge.value ||
      this.form.supplementCharge.value === 0
    ) {
      orderAmount = this.form.rowAmountCurrency.value;
    } else {
      orderAmount =
        this.form.rowAmountCurrency.value +
        (this.form.rowAmountCurrency.value / 100) *
          this.form.supplementCharge.value;
    }
    this.form?.orderAmountCurrency.setValue(orderAmount);
    this.calculateRemainingAmount(orderAmount);
  }

  private setFilteredCustomerInvoices(
    filteredArray: ICustomerInvoiceSmallGridDTO[]
  ): void {
    this.filteredCustomerInvoices = [];
    this.filteredCustomerInvoices.push({
      invoiceId: 0,
      projectId: 0,
      invoiceNr: '',
      customer: '',
      customerInvoiceNumberName: '',
      customerInvoiceNumberNameWithoutDescription: '',
      priceListTypeId: 0,
    } as ICustomerInvoiceSmallGridDTO);
    this.filteredCustomerInvoices.push(...filteredArray);
  }

  private calculateRemainingAmount(value: number) {
    let currentRemaining = 0;
    if (
      this.form.chargeCostToProject?.value ||
      this.costAllocationMode === CostAllocationMode.ReBilled
    ) {
      currentRemaining = Number(value || 0);
    }

    // Handle the case where the value has changed
    const remaining =
      Number(this.data?.invoiceTotalAmount || 0) -
      Number(this.data?.totalAccolationAmount || 0) -
      currentRemaining;

    this.remainingAmount.set(remaining);
    this.form.patchValue({ remainingAllocationAmount: remaining });
  }

  private setDialogData(): void {
    if (!!this.data?.isNew) {
      this.isNew = true;
    }
    if (this.data?.costAllocationMode) {
      this.costAllocationMode = this.data.costAllocationMode;
      this.isCostAllocationModeChargedToProject.set(
        this.costAllocationMode === CostAllocationMode.ChargedToProject
      );
    }

    if (this.data?.rowItem) {
      this.form.reset(this.data.rowItem);
    }
    if (this.data?.customerInvoices) {
      this.customerInvoices = this.data.customerInvoices;
      this.setFilteredCustomerInvoices(this.customerInvoices);
    }
    if (this.data?.projects) {
      this.projectTinyDTOs = this.data.projects;
      this.projects = this.data.projects.map(proj => {
        return {
          id: proj.projectId,
          name: proj.number + ' ' + proj.name,
        } as ISmallGenericType;
      });
    }
    if (this.data.timeCodes) this.timeCodes = this.data.timeCodes;
    if (this.data.employees) {
      this.employeeSmallDTOs = this.data.employees;
      this.employees = this.data.employees.map(emp => {
        return {
          id: emp.employeeId,
          name: emp.name,
        } as ISmallGenericType;
      });
    }
    if (this.data?.products) {
      this.products = this.data.products;
    }
    if (this.data?.supplierInvoiceId) {
      this.form.patchValue({ supplierInvoiceId: this.data.supplierInvoiceId });
    }

    this.invoiceTotalAmount = this.data?.invoiceTotalAmount || 0;
    this.totalAccolationAmount = this.data?.totalAccolationAmount || 0;
    if (this.costAllocationMode === CostAllocationMode.ReBilled) {
      this.totalAccolationAmount -= this.data.orderAmountCurrency ?? 0;
      this.calculateAndPatchOrderAmountCurrency();
    } else {
      if (this.data?.chargeCostToProject) {
        this.totalAccolationAmount -= this.data.projectAmountCurrency ?? 0;
      }
      this.calculateRemainingAmount(this.data.projectAmountCurrency ?? 0);
    }
  }

  //#endregion

  //#region UI events

  closeDialog(): void {
    this.dialogRef.close({
      result: false,
      isNew: this.isNew,
      costAllocationMode: this.costAllocationMode,
    } as CostAllocationDialogDataResult);
  }

  ok(): void {
    if (this.form.valid) {
      this.dialogRef.close({
        result: true,
        rowItem: this.form.getRawValue(),
        isNew: this.isNew,
        costAllocationMode: this.costAllocationMode,
      } as CostAllocationDialogDataResult);
    } else {
      this.closeDialog();
    }
  }

  //#endregion
}
