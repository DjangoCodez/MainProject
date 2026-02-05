import {
  Component,
  effect,
  inject,
  input,
  OnInit,
  output,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SupplierInvoiceReBilledService } from '../../../services/supplier-invoice-re-billed-service';
import { SupplierInvoiceCostAllocationDTO } from '../../../models/supplier-invoice.model';
import {
  Feature,
  SoeOriginStatusClassificationGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { GridComponent } from '@ui/grid/grid.component';
import { take } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { DialogService } from '@ui/dialog/services/dialog.service';

import { IProjectTinyDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import { BrowserUtil } from '@shared/util/browser-util';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { CostAllocationDialog } from '../cost-allocation-dialog/cost-allocation-dialog';
import {
  CostAllocationDialogData,
  CostAllocationDialogDataResult,
  CostAllocationMode,
} from '../models/cost-allocation-form.model';

@Component({
  selector: 'soe-re-billed',
  templateUrl: './re-billed.html',
  standalone: false,
  providers: [FlowHandlerService],
})
export class ReBilledComponent
  extends GridBaseDirective<
    SupplierInvoiceCostAllocationDTO,
    SupplierInvoiceReBilledService
  >
  implements OnInit
{
  customerInvoices = input.required<ICustomerInvoiceSmallGridDTO[]>();
  projects = input.required<IProjectTinyDTO[]>();
  products = input.required<IProductSmallDTO[]>();
  rowsData = input.required<SupplierInvoiceCostAllocationDTO[]>();
  invoiceTotalAmount = input.required<number>();
  totalAccolationAmount = input.required<number>();
  reBilledChanged = output<SupplierInvoiceCostAllocationDTO[]>();
  orderProjectChanged = output<SupplierInvoiceCostAllocationDTO>();
  supplierInvoiceId = input.required<number>();

  service = inject(SupplierInvoiceReBilledService);
  messageboxService = inject(MessageboxService);
  dialogService = inject(DialogService);
  gridRows = new BehaviorSubject<SupplierInvoiceCostAllocationDTO[]>([]);

  markups: ISmallGenericType[] = [];

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
      'Cost.Allocation.ReBilled',
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
        'economy.supplier.invoice.surcharge',
        'common.sum',
        'common.amount',
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
          'supplementCharge',
          terms['economy.supplier.invoice.surcharge'],
          { flex: 1 }
        );

        this.grid.addColumnNumber('rowAmountCurrency', terms['common.amount'], {
          flex: 1,
        });
        this.grid.addColumnNumber('orderAmountCurrency', terms['common.sum'], {
          flex: 1,
        });
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
            this.editReBilled(row);
          },
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => this.deleteReBilled(row),
        });
        this.grid.dynamicHeight = true;
        this.grid.setNbrOfRowsToShow(1, 3);
        super.finalizeInitGrid({ hidden: true });
      });
  }

  //#endregion

  //#region Helping Methods

  openOrder(row: SupplierInvoiceCostAllocationDTO) {
    const url = `/soe/billing/order/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&orderId=${row.orderId}&orderNr=${row.orderNr}`;
    BrowserUtil.openInNewTab(window, url);
  }

  openProject(row: SupplierInvoiceCostAllocationDTO) {
    const url = `/soe/billing/project/list/default.aspx?&projectId=${row.projectId}&projectNr=${row.projectNr}`;
    BrowserUtil.openInNewTab(window, url);
  }

  updateParentRows(rows: SupplierInvoiceCostAllocationDTO[]) {
    this.reBilledChanged.emit(rows);
  }

  updateOrderProject(row: SupplierInvoiceCostAllocationDTO) {
    this.orderProjectChanged.emit(row);
  }

  //#endregion

  //#region UI Events

  editReBilled(row: SupplierInvoiceCostAllocationDTO) {
    //open edit dialog
    const reBilledComponent = this.dialogService.open(CostAllocationDialog, {
      title: this.translate.instant('economy.supplier.invoice.rebilled'),
      size: 'md',
      rowItem: row,
      supplierInvoiceId: this.supplierInvoiceId(),
      customerInvoices: this.customerInvoices(),
      timeCodes: [],
      employees: [],
      projects: this.projects(),
      products: this.products(),
      isNew: false,
      orderAmountCurrency: row.orderAmountCurrency || 0,
      rowAmountCurrency: row.rowAmountCurrency || 0,
      invoiceTotalAmount: this.invoiceTotalAmount(),
      totalAccolationAmount: this.totalAccolationAmount(),
      costAllocationMode: CostAllocationMode.ReBilled,
      projectAmountCurrency: 0,
      chargeCostToProject: false,
    } as CostAllocationDialogData);

    reBilledComponent
      .afterClosed()
      .subscribe((result: CostAllocationDialogDataResult) => {
        if (!result.result) {
          return;
        }
        //update row logic

        let reBilledArray: SupplierInvoiceCostAllocationDTO[] = [];
        if (result.rowItem) {
          if (result.isNew) {
            reBilledArray = [...this.rowsData(), result.rowItem];
          } else {
            reBilledArray = this.rowsData().map(r => {
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
          this.updateParentRows(reBilledArray);
          this.updateOrderProject(result.rowItem);
        }
      });
  }

  deleteReBilled(row: SupplierInvoiceCostAllocationDTO) {
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
            r => r.customerInvoiceRowId !== row.customerInvoiceRowId
          )
        );
      }
    });
  }

  //#endregion
}
