import {
  Component,
  effect,
  inject,
  input,
  OnInit,
  output,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SupplierInvoiceCostAllocationDTO } from '../../../models/supplier-invoice.model';
import { SupplierInvoiceChargedToProjectService } from '../../../services/supplier-invoice-charged-to-project-service';

import {
  Feature,
  SoeOriginStatusClassificationGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { BehaviorSubject, take } from 'rxjs';
import { GridComponent } from '@ui/grid/grid.component';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { IEmployeeSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogService } from '@ui/dialog/services/dialog.service';

import { IProjectTinyDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import { BrowserUtil } from '@shared/util/browser-util';
import { CostAllocationDialog } from '../cost-allocation-dialog/cost-allocation-dialog';
import {
  CostAllocationDialogData,
  CostAllocationDialogDataResult,
  CostAllocationMode,
} from '../models/cost-allocation-form.model';

@Component({
  selector: 'soe-charged-to-project',
  templateUrl: './charged-to-project.html',
  standalone: false,
  providers: [FlowHandlerService],
})
export class ChargedToProjectComponent
  extends GridBaseDirective<
    SupplierInvoiceCostAllocationDTO,
    SupplierInvoiceChargedToProjectService
  >
  implements OnInit
{
  customerInvoices = input.required<ICustomerInvoiceSmallGridDTO[]>();
  projects = input.required<IProjectTinyDTO[]>();
  employees = input.required<IEmployeeSmallDTO[]>();
  timeCodes = input.required<ISmallGenericType[]>();
  rowsData = input.required<SupplierInvoiceCostAllocationDTO[]>();

  invoiceTotalAmount = input.required<number>();
  totalAccolationAmount = input.required<number>();
  supplierInvoiceId = input.required<number>();

  chargedToProjectChanged = output<SupplierInvoiceCostAllocationDTO[]>();
  orderProjectChanged = output<SupplierInvoiceCostAllocationDTO>();

  service = inject(SupplierInvoiceChargedToProjectService);
  messageboxService = inject(MessageboxService);
  dialogService = inject(DialogService);
  gridRows = new BehaviorSubject<SupplierInvoiceCostAllocationDTO[]>([]);

  constructor() {
    super();
    effect(() => {
      // React to input changes if needed
      this.gridRows.next(this.rowsData());
    });
  }
  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Supplier_Invoice,
      'Cost.Allocation.ChargedToProject',
      {
        skipDefaultToolbar: true,
        skipInitialLoad: true,
      }
    );
  }

  //#region Override methods

  onGridReadyToDefine(grid: GridComponent<SupplierInvoiceCostAllocationDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.date',
        'economy.supplier.invoice.customerinvoice',
        'economy.supplier.invoice.project',
        'economy.supplier.invoice.timecode',
        'economy.supplier.invoice.chargedtoproject.employee',
        'common.sum',
        'economy.supplier.invoice.chargecosttoproject',
        'economy.supplier.invoice.includeimage',
        'core.edit',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'customerInvoiceNumberName',
          terms['economy.supplier.invoice.customerinvoice'],
          {
            flex: 1,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pencil',
              onClick: row => this.openOrder(row),
              show: row => row && row.orderId > 0,
            },
          }
        );
        this.grid.addColumnText(
          'projectNrName',
          terms['economy.supplier.invoice.project'],
          {
            flex: 1,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pencil',
              onClick: row => this.openProject(row),
              show: row => row && row.projectId > 0,
            },
          }
        );
        this.grid.addColumnText(
          'timeCodeName',
          terms['economy.supplier.invoice.timecode'],
          { flex: 1 }
        );
        this.grid.addColumnNumber(
          'projectAmountCurrency',
          terms['common.sum'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnBool(
          'chargeCostToProject',
          terms['economy.supplier.invoice.chargecosttoproject'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnBool(
          'includeSupplierInvoiceImage',
          terms['economy.supplier.invoice.includeimage'],
          {
            flex: 1,
          }
        );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.editChargedToProject(row);
          },
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => this.deleteChargedToProject(row),
        });
        this.grid.dynamicHeight = true;
        this.grid.setNbrOfRowsToShow(1, 3);
        super.finalizeInitGrid({ hidden: true });
      });
  }

  //#endregion

  //#region Helping Methods

  updateParentRows(rows: SupplierInvoiceCostAllocationDTO[]) {
    this.chargedToProjectChanged.emit(rows);
  }

  updateOrderProject(row: SupplierInvoiceCostAllocationDTO) {
    this.orderProjectChanged.emit(row);
  }

  //#endregion

  //#region UI Events

  openProject(row: SupplierInvoiceCostAllocationDTO) {
    const url = `/soe/billing/project/list/default.aspx?&projectId=${row.projectId}&projectNr=${row.projectNr}`;
    BrowserUtil.openInNewTab(window, url);
  }

  openOrder(row: SupplierInvoiceCostAllocationDTO) {
    const url = `/soe/billing/order/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&orderId=${row.orderId}&orderNr=${row.orderNr}`;
    BrowserUtil.openInNewTab(window, url);
  }

  editChargedToProject(row: SupplierInvoiceCostAllocationDTO) {
    //open edit dialog
    const chargedToProjectComponent = this.dialogService.open(
      CostAllocationDialog,
      {
        title: `${this.translate.instant('economy.supplier.invoice.chargedtoproject')}`,
        size: 'md',
        rowItem: row,
        supplierInvoiceId: this.supplierInvoiceId(),
        customerInvoices: this.customerInvoices(),
        timeCodes: this.timeCodes(),
        employees: this.employees(),
        projects: this.projects(),
        isNew: false,
        projectAmountCurrency: row.projectAmountCurrency || 0.0,
        invoiceTotalAmount: this.invoiceTotalAmount(),
        totalAccolationAmount: this.totalAccolationAmount(),
        costAllocationMode: CostAllocationMode.ChargedToProject,
        chargeCostToProject: row.chargeCostToProject,
      } as CostAllocationDialogData
    );

    chargedToProjectComponent
      .afterClosed()
      .subscribe((result: CostAllocationDialogDataResult) => {
        if (!result.result) {
          return;
        }
        //update row logic

        let chargedToProjectArray: SupplierInvoiceCostAllocationDTO[] = [];
        if (result.rowItem) {
          if (result.isNew) {
            chargedToProjectArray = [...this.rowsData(), result.rowItem];
          } else {
            chargedToProjectArray = this.rowsData().map(r => {
              if (
                r.timeCodeTransactionId ===
                result.rowItem?.timeCodeTransactionId
              ) {
                r = result.rowItem;
                r.projectNrName = `${result.rowItem.projectNr} ${result.rowItem.projectName}`;
                r.employeeNrName = `${result.rowItem.employeeNr} ${result.rowItem.employeeName}`;
              }
              return r;
            });
          }
          this.updateParentRows(chargedToProjectArray);
          this.updateOrderProject(result.rowItem);
        }
      });
  }

  deleteChargedToProject(row: SupplierInvoiceCostAllocationDTO) {
    //open delete confirmation dialog
    const mb = this.messageboxService.warning(
      this.translate.instant('core.warning'),
      this.translate.instant('core.deleterowwarning')
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.result) {
        //delete row logic
        this.updateParentRows(
          this.rowsData().filter(
            r => r.timeCodeTransactionId !== row.timeCodeTransactionId
          )
        );
      }
    });
  }

  //#endregion
}
