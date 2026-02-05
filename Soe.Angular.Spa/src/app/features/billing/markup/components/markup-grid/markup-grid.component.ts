import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonCustomerService } from '@billing/shared/services/common-customer.service';
import { SupplierService } from '@features/economy/services/supplier.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  Feature,
  SoeCategoryType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IMarkupDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressOptions, ProgressService } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { map, Observable, take, tap } from 'rxjs';
import { MarkupDTO } from '../../models/markup.model';
import { MarkupService } from '../../services/markup.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-markup-grid',
  templateUrl: './markup-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class MarkupGridComponent
  extends GridBaseDirective<MarkupDTO, MarkupService>
  implements OnInit
{
  supplierService = inject(SupplierService);
  coreService = inject(CoreService);
  service = inject(MarkupService);
  progressService = inject(ProgressService);
  commonCustomerService = inject(CommonCustomerService);
  performLoad = new Perform<MarkupDTO[]>(this.progressService);

  wholeSeller: ISmallGenericType[] = [];
  categories: ISmallGenericType[] = [];
  customers: ISmallGenericType[] = [];
  markupRows: MarkupDTO[] = [];
  performAction = new Perform<BackendResponse>(this.progressService);

  constructor(public flowHandler: FlowHandlerService) {
    super();

    this.startFlow(
      Feature.Billing_Preferences_InvoiceSettings_Markup,
      'Billing.Invoices.DeliveryConditions',
      {
        lookups: [
          this.loadWholeSellers(),
          this.loadCategories(),
          this.loadCustomers(),
        ],
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      reloadOption: {
        onAction: () => this.refreshGrid(),
      },
      saveOption: {
        onAction: () => this.save(),
      },
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('plus', {
          iconName: signal('plus'),
          caption: signal('billing.invoices.markup.newmarkup'),
          tooltip: signal('billing.invoices.markup.newmarkup'),
          onAction: () => this.addRow(),
        }),
      ],
    });
  }

  private loadWholeSellers(): Observable<ISmallGenericType[]> {
    return this.supplierService.getSmallGenericSysWholesellers(false).pipe(
      tap(x => {
        this.wholeSeller = x;
        this.wholeSeller.push({ id: 65, name: 'Comfort' });
      })
    );
  }

  private loadCategories(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getCategoriesDict(SoeCategoryType.Customer, false)
      .pipe(
        tap(x => {
          this.categories = x;
          this.categories.push({ id: 0, name: '' });
        })
      );
  }

  private loadCustomers(): Observable<ISmallGenericType[]> {
    return this.commonCustomerService.getCustomersDict(true, false, true).pipe(
      tap(x => {
        this.customers = x;
        this.customers.push({ id: 0, name: '' });
      })
    );
  }

  override onGridReadyToDefine(grid: GridComponent<MarkupDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.translate
      .get([
        'common.date',
        'billing.invoices.markup.newcustomerdiscount',
        'billing.invoices.markup.newmarkup',
        'billing.invoices.markup.wholeseller',
        'billing.invoices.markup.materialclass',
        'billing.invoices.markup.customercategory',
        'billing.invoices.markup.supplieragreemmentpercent',
        'billing.invoices.markup.productgroup',
        'billing.invoices.markup.markuppercent',
        'billing.invoices.markup.customerdiscount',
        'billing.invoices.markup.discountpercent',
        'billing.invoices.markup.customer',
        'common.all',
        'common.personal',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');

        this.grid.addColumnAutocomplete(
          'sysWholesellerId',
          terms['billing.invoices.markup.wholeseller'],
          {
            editable: true,
            flex: 1,
            source: () => this.wholeSeller,
            optionDisplayNameField: 'wholesellerName',
            optionIdField: 'id',
            optionNameField: 'name',
          }
        );
        this.grid.addColumnText(
          'code',
          terms['billing.invoices.markup.materialclass'],
          {
            editable: true,
            flex: 1,
          }
        );
        this.grid.addColumnAutocomplete(
          'categoryId',
          terms['billing.invoices.markup.customercategory'],
          {
            enableHiding: false,
            editable: true,
            limit: 7,
            flex: 1,
            source: () => this.categories,
            optionDisplayNameField: 'categoryName',
            optionIdField: 'id',
            optionNameField: 'name',
          }
        );
        this.grid.addColumnAutocomplete(
          'actorCustomerId',
          terms['billing.invoices.markup.customer'],
          {
            editable: true,
            limit: 7,
            flex: 1,
            source: () => this.customers,
            optionIdField: 'id',
            optionNameField: 'name',
            optionDisplayNameField: 'customerName',
          }
        );

        this.grid.addColumnNumber(
          'wholesellerDiscountPercent',
          terms['billing.invoices.markup.supplieragreemmentpercent'],
          {
            flex: 1,
            enableHiding: true,
            editable: false,
            decimals: 2,
          }
        );
        this.grid.addColumnText(
          'productIdFilter',
          terms['billing.invoices.markup.productgroup'],
          {
            flex: 1,
            enableHiding: false,
            editable: true,
          }
        );
        this.grid.addColumnNumber(
          'markupPercent',
          terms['billing.invoices.markup.markuppercent'],
          {
            flex: 1,
            enableHiding: false,
            decimals: 2,
            editable: true,
          }
        );
        this.grid.addColumnDate('created', terms['common.date'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnIconDelete({ onClick: r => this.deleteRow(r) });

        super.finalizeInitGrid();
      });
  }

  override loadData(id?: number | undefined): Observable<IMarkupDTO[]> {
    return this.performLoad.load$(
      this.service.getGrid(undefined, { isDiscount: false }).pipe(
        map(rows => {
          rows.forEach(row => {
            row.categoryName =
              this.categories.find(e => e.id == row.categoryId)?.name || '';
            row.customerName =
              this.customers.find(e => e.id == row.actorCustomerId)?.name || '';

            this.markupRows = [];
          });
          return rows;
        })
      )
    );
  }

  deleteRow(row: MarkupDTO) {
    if (this.markupRows) this.markupRows.push(row);

    const rows = this.rowData.value;
    if (rows) {
      const index: number = rows.indexOf(row);
      rows.splice(index, 1);
      this.grid.resetRows();
    }
    row.state = SoeEntityState.Deleted;
  }

  save(options?: ProgressOptions) {
    const markupRows = this.rowData.value;
    const rowsToSave = [];

    for (const row of markupRows) {
      if (row.isModified) {
        rowsToSave.push(row);
      }
    }

    if (this.markupRows) {
      for (const row of this.markupRows) rowsToSave.push(row);
    }

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(rowsToSave).pipe(tap(() => this.loadData())),
      undefined,
      undefined,
      options
    );
  }

  addRow() {
    const row = new MarkupDTO();
    this.rowData.value.push(row);
    this.grid.setData(this.rowData.value);
    this.focusFirstCell();
    this.rowIsModified(row);
  }

  private focusFirstCell(): void {
    const lastRowIdx = this.grid?.api.getLastDisplayedRowIndex();
    this.grid?.api.setFocusedCell(lastRowIdx, 'sysWholesellerId');
    this.grid?.api.startEditingCell({
      rowIndex: lastRowIdx,
      colKey: 'sysWholesellerId',
    });
  }

  onCellValueChanged(row: CellValueChangedEvent) {
    this.rowIsModified(row.data);
    switch (row.colDef.field) {
      case 'categoryId':
        //clear customer: discount is either for category or for customer
        row.data.actorCustomerId = 0;
        this.grid.refreshCells();
        break;
      case 'actorCustomerId':
        //clear customercategory: discount is either for category or for customer
        row.data.categoryId = 0;
        this.grid.refreshCells();
        break;
    }
  }

  public rowIsModified(row: MarkupDTO) {
    row.isModified = true;
    this.grid.agGrid.api.refreshCells();
  }
}
