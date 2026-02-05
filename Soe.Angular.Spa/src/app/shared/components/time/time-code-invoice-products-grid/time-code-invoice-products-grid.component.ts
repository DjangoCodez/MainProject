import { Component, inject, input, Input, OnInit, signal } from '@angular/core';
import { FormArray } from '@angular/forms';
import { SoeFormGroup } from '@shared/extensions';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import {
  Feature,
  SoeEntityState,
  SoeTimeCodeType,
  TermGroup_ExpenseType,
  TermGroup_InvoiceProductVatType,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { IProductTimeCodeDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { ITimeCodeInvoiceProductDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TimeCodeInvoiceProductForm } from './models/time-code-invoice-product-form.model';
import { TimeCodeInvoiceProductsDialogComponent } from './time-code-invoice-products-dialog/time-code-invoice-products-dialog.component';
import { TimeCodeInvoiceProductsService } from './services/time-code-invoice-products.service';

@Component({
  selector: 'soe-time-code-invoice-products-grid',
  standalone: false,
  templateUrl:
    '../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class TimeCodeInvoiceProductsGridComponent
  extends EmbeddedGridBaseDirective<
    ITimeCodeInvoiceProductDTO,
    SoeFormGroup,
    TimeCodeInvoiceProductForm
  >
  implements OnInit
{
  private readonly service = inject(TimeCodeInvoiceProductsService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly dialogService = inject(DialogService);
  private readonly factorDecimals: number = 2;

  @Input({ required: true }) form!: SoeFormGroup;
  @Input({ required: true }) permission: Feature = Feature.None;

  noMargin = input(true);
  height = input(1); // the height of the grid behaves strangely without this - 1 px seems to fix the issue
  toolbarNoMargin = input(true);
  toolbarNoBorder = input(true);
  toolbarNoPadding = input(true);
  toolbarNoTopBottomPadding = input(true);

  private invoiceProducts: IProductTimeCodeDTO[] | undefined;

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(this.permission, '', {
      lookups: [this.loadInvoiceProducts()],
      skipInitialLoad: true,
    });
    this.form.valueChanges.subscribe(value => {
      this.rowData.next(value.invoiceProducts);
    });
  }

  override createGridToolbar(): void {
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: signal<string>('time.time.timecode.invoiceproducts'),
        }),
      ],
    });
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('addrow', {
          iconName: signal('plus'),
          tooltip: signal('common.newrow'),
          disabled: signal(!this.flowHandler.modifyPermission()),
          onAction: () => this.addRow(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<ITimeCodeInvoiceProductDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.time.timecode.invoiceproduct',
        'time.time.timecode.factor',
        'core.edit',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'invoiceProductId',
          terms['time.time.timecode.invoiceproduct'],
          this.invoiceProducts || [],
          undefined,
          { flex: 75, resizable: false, sortable: false }
        );
        this.grid.addColumnNumber(
          'factor',
          terms['time.time.timecode.factor'],
          {
            flex: 25,
            resizable: false,
            sortable: false,
            alignLeft: true,
            decimals: 0,
            maxDecimals: this.factorDecimals,
          }
        );
        if (this.flowHandler.modifyPermission()) {
          this.grid.addColumnIconEdit({
            tooltip: terms['core.edit'],
            onClick: row => {
              this.edit(row);
            },
          });
          this.grid.addColumnIconDelete({
            tooltip: terms['core.delete'],
            onClick: row => {
              this.deleteRow(row);
            },
          });
        }
        this.grid.context.suppressGridMenu = true;
        this.grid.context.suppressFiltering = true;
        this.grid.setNbrOfRowsToShow(1, 5);
        super.finalizeInitGrid();
        this.grid.resetColumns();
      });
  }

  override loadTerms(translationsKeys?: string[]): Observable<TermCollection> {
    return super.loadTerms([
      ...(translationsKeys || []),
      'core.add',
      'core.edit',
      'core.ok',
      'core.cancel',
    ]);
  }

  private loadInvoiceProducts(): Observable<void> {
    return this.performLoadData.load$(
      this.service
        .getTimeCodeInvoiceProducts()
        .pipe(tap(value => (this.invoiceProducts = value)))
    );
  }

  private get timeCodeInvoiceProducts(): FormArray<TimeCodeInvoiceProductForm> {
    return (this.form as any).invoiceProducts;
  }

  override addRow(): void {
    this.editInvoiceProduct();
  }

  override edit(row: ITimeCodeInvoiceProductDTO): void {
    if (this.flowHandler.modifyPermission()) {
      this.editInvoiceProduct(row);
    }
  }

  override deleteRow(row: ITimeCodeInvoiceProductDTO) {
    super.deleteRow(row, this.timeCodeInvoiceProducts);
  }

  private editInvoiceProduct(row?: ITimeCodeInvoiceProductDTO): void {
    const availableInvoiceProducts = this.getAvailableInvoiceProducts(row);

    if (availableInvoiceProducts.length === 0) {
      this.showCannotAddMessage();
      return;
    }

    this.dialogService
      .open(TimeCodeInvoiceProductsDialogComponent, {
        title: this.terms[row ? 'core.edit' : 'core.add'],
        dto: structuredClone(row),
        invoiceProducts: availableInvoiceProducts,
        factorDecimals: this.factorDecimals,
      })
      .afterClosed()
      .subscribe((response: any) => {
        if (response?.success) {
          if (row) {
            if (
              row.invoiceProductId !== response.invoiceProductId ||
              row.factor !== response.factor
            ) {
              row.invoiceProductId = response.invoiceProductId;
              row.factor = response.factor;
              this.timeCodeInvoiceProducts.patchValue(this.rowData.value || []);
              this.form.markAsDirty();
            }
          } else {
            const newrow: ITimeCodeInvoiceProductDTO = {
              timeCodeInvoiceProductId: 0,
              timeCodeId: this.form?.getIdControl()?.value || 0,
              invoiceProductId: response.invoiceProductId,
              factor: response.factor,
              invoiceProductPrice: 0,
            };
            super.addRow(
              newrow,
              this.timeCodeInvoiceProducts,
              TimeCodeInvoiceProductForm
            );
          }
          setTimeout(() => {
            this.grid.resetColumns(); // the columns are not resized properly without this when there were no items previously
          });
        }
      });
  }

  private getAvailableInvoiceProducts(row?: ITimeCodeInvoiceProductDTO) {
    const timeCodeType: SoeTimeCodeType = this.form.value.type;
    const expenseType: TermGroup_ExpenseType = this.form.value.expenseType;
    return (
      this.invoiceProducts?.filter(p => {
        if (row && row.invoiceProductId === p.id) {
          return true;
        }
        if (p.state !== SoeEntityState.Active) {
          return false;
        }
        if (
          timeCodeType !== SoeTimeCodeType.Material &&
          expenseType !== TermGroup_ExpenseType.Expense &&
          p.vatType !== TermGroup_InvoiceProductVatType.Service
        ) {
          return false;
        }
        return !this.rowData.value?.some(r => r.invoiceProductId === p.id);
      }) ?? []
    );
  }

  private showCannotAddMessage(): void {
    this.messageboxService.show(
      this.translate.instant('core.warning'),
      this.translate.instant(
        this.timeCodeInvoiceProducts.length === 0
          ? 'time.time.timecode.invoiceproducts.noitems'
          : 'time.time.timecode.invoiceproducts.allalreadyadded'
      ),
      {
        size: 'md',
        type: 'warning',
        buttons: 'ok',
      }
    );
  }
}
