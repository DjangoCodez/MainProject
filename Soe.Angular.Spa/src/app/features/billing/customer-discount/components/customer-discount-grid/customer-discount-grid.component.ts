import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonCustomerService } from '@billing/shared/services/common-customer.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  Feature,
  SoeCategoryType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ProgressOptions } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { CustomerDiscountMarkupDTO } from '../../models/customer-discount.model';
import { CustomerDiscountService } from '../../services/customer-discount.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-customer-discount-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerDiscountGridComponent
  extends GridBaseDirective<CustomerDiscountMarkupDTO, CustomerDiscountService>
  implements OnInit
{
  service = inject(CustomerDiscountService);
  coreService = inject(CoreService);
  commonCustomerService = inject(CommonCustomerService);
  progressService = inject(ProgressService);
  performAction = new Perform<BackendResponse>(this.progressService);
  performLoad = new Perform<any>(this.progressService);
  markupRows: CustomerDiscountMarkupDTO[] = [];

  sysWholesellersDict: ISmallGenericType[] = [];
  customerCategories: ISmallGenericType[] = [];
  customers: ISmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Preferences_InvoiceSettings_Markup,
      'Billing.Invoices.Markup',
      {
        lookups: [
          this.loadWholeSellers(),
          this.loadCustomerCategories(),
          this.loadCustomers(),
        ],
      }
    );
    this.markupRows = [];
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      saveOption: {
        onAction: () => this.save(),
      },
    });
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton(
          'billing.invoices.markup.newcustomerdiscount',
          {
            iconName: signal('plus'),
            caption: signal('billing.invoices.markup.newcustomerdiscount'),
            tooltip: signal('billing.invoices.markup.newcustomerdiscount'),
            onAction: () => this.addNewRow(),
          }
        ),
      ],
    });
  }

  override saveStatus(): void {
    this.save();
  }

  override onGridReadyToDefine(
    grid: GridComponent<CustomerDiscountMarkupDTO>
  ): void {
    super.onGridReadyToDefine(grid);
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
        this.grid.addColumnAutocomplete(
          'sysWholesellerId',
          terms['billing.invoices.markup.wholeseller'],
          {
            editable: true,
            flex: 1,
            source: () => this.sysWholesellersDict,
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
            editable: true,
            flex: 1,
            source: () => this.customerCategories,
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
          'discountPercent',
          terms['billing.invoices.markup.discountpercent'],
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

  private loadWholeSellers(): Observable<ISmallGenericType[]> {
    return this.service.getSysWholesellersDict(false).pipe(
      tap(x => {
        this.sysWholesellersDict = x;
        this.sysWholesellersDict.push({ id: 65, name: 'Comfort' });

        this.sysWholesellersDict.splice(0, 0, {
          id: -2,
          name: this.translate.instant('common.personal'),
        });
        this.sysWholesellersDict.splice(0, 0, {
          id: -1,
          name: this.translate.instant('common.all'),
        });
      })
    );
  }

  private loadCustomerCategories() {
    return this.coreService
      .getCategoriesDict(SoeCategoryType.Customer, true)
      .pipe(tap(x => (this.customerCategories = x)));
  }

  private loadCustomers(): Observable<ISmallGenericType[]> {
    return this.commonCustomerService.getCustomersDict(true, false, true).pipe(
      tap(x => {
        this.customers = x;
      })
    );
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { isDiscount: boolean }
  ): Observable<CustomerDiscountMarkupDTO[]> {
    return super.loadData(id, { isDiscount: true });
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
      this.service.save(rowsToSave).pipe(tap(() => this.refreshGrid())),
      undefined,
      undefined,
      options
    );
  }

  addNewRow() {
    const row = new CustomerDiscountMarkupDTO();
    this.rowData.value.push(row);
    this.grid.setData(this.rowData.value);
    this.grid.scrollToFocus(row, 'sysWholesellerId');

    this.grid.api.startEditingCell({
      rowIndex: this.grid.api.getLastDisplayedRowIndex(),
      colKey: 'sysWholesellerId',
    });
    this.rowIsModified(row);
  }

  public rowIsModified(row: CustomerDiscountMarkupDTO) {
    row.isModified = true;
    this.grid.agGrid.api.refreshCells();
  }

  deleteRow(row: CustomerDiscountMarkupDTO) {
    if (this.markupRows) this.markupRows.push(row);

    const rows = this.rowData.value;
    if (rows) {
      const index: number = rows.indexOf(row);
      rows.splice(index, 1);
      this.grid.resetRows();
    }
    row.state = SoeEntityState.Deleted;
  }
}
