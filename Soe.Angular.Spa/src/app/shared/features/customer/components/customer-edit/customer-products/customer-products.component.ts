import { CellValueChangedEvent } from 'ag-grid-community';
import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { CommonCustomerService } from '@billing/shared/services/common-customer.service';
import { ProductSmallDTO } from '@features/billing/products/models/product.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CustomerForm } from '@shared/features/customer/models/customer-form.model';
import { CustomerProductPriceSmallDTO } from '@shared/features/customer/models/customer-product.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, of, Subject, take, takeUntil, tap } from 'rxjs';

@Component({
  selector: 'soe-customer-products',
  templateUrl: './customer-products.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerProductsComponent
  extends GridBaseDirective<CustomerProductPriceSmallDTO>
  implements OnInit
{
  private _destroy$ = new Subject<void>();

  commonCustomerService = inject(CommonCustomerService);

  @Input({ required: true }) form!: CustomerForm;
  // @Input() products: CustomerProductSmallDTO[] = [];
  // products: IProductSmallDTO[] = [];
  smallProducts: ProductSmallDTO[] = [];
  productRows$ = new BehaviorSubject<CustomerProductPriceSmallDTO[]>([]);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Customer_Customers_Edit,
      'Common.Customer.Customers.Directives.CustomerProducts',
      {
        skipInitialLoad: true,
        lookups: [this.loadProducts()],
      }
    );

    this.productRows$.next(this.form?.customerProducts.value);
    this.form?.customerProducts.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(rows => {
        if (!this.form?.customerProducts.dirty) {
          // this.rows$.next(<CustomerProductPriceSmallDTO[]>rows);
          this.productRows$.next(<CustomerProductPriceSmallDTO[]>rows);
        }
      });
  }

  override createGridToolbar(config?: Partial<ToolbarGridConfig>): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('plus', {
          iconName: signal('plus'),
          caption: signal('common.customer.customer.product.new'),
          tooltip: signal('common.customer.customer.product.new'),
          onAction: () => this.addNewRow(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<CustomerProductPriceSmallDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.customer.customer.product.productnr',
        'common.customer.customer.product.name',
        'common.customer.customer.product.price',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnAutocomplete<ProductSmallDTO>(
          'productId',
          terms['common.customer.customer.product.productnr'],
          {
            flex: 1,
            editable: true,
            source: () => this.smallProducts,
            optionIdField: 'productId',
            optionNameField: 'numberName',
            optionDisplayNameField: 'number',
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );

        // this.grid.addColumnTypeahead<ProductSmallDTO>('productId', terms['common.customer.customer.product.productnr'],
        //   {
        //   flex: 1,
        //   editable: true,
        //   source: () => of(this.smallProducts),
        //   optionIdField: 'productId',
        //   optionNameField: 'numberName',
        //   suppressFilter: true,
        //   suppressFloatingFilter: true,
        // });

        // var ee = SoeGridOptionsEvent.AfterCellEdit;

        // this.grid.addColumnTypeahead("productId", terms['common.customer.customer.product.productnr'],
        //   {
        //   flex: 1,
        //   editable: true,
        //   source: () => of(this.products),
        //   optionIdField: 'productId',
        //   optionNameField: 'name',
        //   suppressFilter: true,
        //   suppressFloatingFilter: true,
        // });

        this.grid.addColumnText(
          'name',
          terms['common.customer.customer.product.name'],
          {
            suppressFilter: true,
            editable: false,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnNumber(
          'price',
          terms['common.customer.customer.product.price'],
          {
            enableHiding: false,
            decimals: 2,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
          suppressFloatingFilter: true,
          suppressFilter: true,
        });
        this.grid.height.set(200);

        super.finalizeInitGrid();
      });

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });
  }

  onCellValueChanged(event: CellValueChangedEvent) {
    if (event.oldValue === event.newValue) return;

    if (event.colDef.field == 'productId') {
      const smallProduct = this.smallProducts.find(
        p => p.productId == event.data.productId
      );
      (event.data.productId = smallProduct?.productId),
        (event.data.name = smallProduct?.name ?? '');
      event.data.price = 0;
    }
    if (event.colDef.field == 'price') {
      event.data.price = parseFloat(event.data.price.toString());
    }
    this.grid.refreshCells();
    this.form?.customProductRowsPatchValues(this.productRows$.value);
    this.form?.markAsDirty();
  }

  private addNewRow() {
    const newRow = this.form?.addCustomerProductRow();
    this.grid.addRow(newRow);
    // this.rows$.next(this.form?.customerProducts.value);
    this.grid.api.startEditingCell({
      rowIndex: this.productRows$.value.length - 1,
      colKey: 'productId',
    });
  }

  deleteRow(row: CustomerProductPriceSmallDTO): void {
    const products: CustomerProductPriceSmallDTO[] =
      this.productRows$.value || [];
    this.productRows$.pipe(take(1)).subscribe(rows => {
      const indexToRemove = rows.indexOf(row);
      if (row.customerProductId === 0) {
        products.splice(indexToRemove, 1);
      } else {
        products[indexToRemove].isDelete = true;
      }
      this.productRows$.next(products.filter(x => x.isDelete === false));
      this.grid?.refreshCells();
      this.form?.customProductRowsPatchValues(this.productRows$.value);
      this.form?.markAsDirty();
    });
  }

  clearGridRows() {
    console.log('clearGridRows');
    this.form.customerProducts.clear();
    this.form?.markAsDirty();
    this.form?.markAsTouched();
  }

  loadProducts() {
    return this.commonCustomerService.getInvoiceProductsSmall(false).pipe(
      tap(x => {
        this.smallProducts = x;
      })
    );
  }

  getProduct(id: number) {
    return this.smallProducts.find(x => x.productId === id);
  }
}
