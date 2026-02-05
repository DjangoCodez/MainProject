import { Component, inject, input, Input, OnInit, signal } from '@angular/core';
import { FormArray } from '@angular/forms';
import { SoeFormGroup } from '@shared/extensions';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import {
  Feature,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { IProductTimeCodeDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { ITimeCodePayrollProductDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TimeCodePayrollProductForm } from './models/time-code-payroll-product-form.model';
import { TimeCodePayrollProductsDialogComponent } from './time-code-payroll-products-dialog/time-code-payroll-products-dialog.component';
import { TimeCodePayrollProductsService } from './services/time-code-payroll-products.service';

@Component({
  selector: 'soe-time-code-payroll-products-grid',
  standalone: false,
  templateUrl:
    '../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class TimeCodePayrollProductsGridComponent
  extends EmbeddedGridBaseDirective<
    ITimeCodePayrollProductDTO,
    SoeFormGroup,
    TimeCodePayrollProductForm
  >
  implements OnInit
{
  private readonly service = inject(TimeCodePayrollProductsService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly dialogService = inject(DialogService);
  private readonly factorDecimals: number = 5;

  @Input({ required: true }) form!: SoeFormGroup;
  @Input({ required: true }) permission: Feature = Feature.None;

  noMargin = input(true);
  height = input(1); // the height of the grid behaves strangely without this - 1 px seems to fix the issue
  toolbarNoMargin = input(true);
  toolbarNoBorder = input(true);
  toolbarNoPadding = input(true);
  toolbarNoTopBottomPadding = input(true);

  private payrollProducts: IProductTimeCodeDTO[] | undefined;

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(this.permission, '', {
      lookups: [this.loadPayrollProducts()],
      skipInitialLoad: true,
    });
    this.form.valueChanges.subscribe(value => {
      this.rowData.next(value.payrollProducts);
    });
  }

  override createGridToolbar(): void {
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: signal<string>(
            'time.payroll.payrollproduct.payrollproducts'
          ),
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
    grid: GridComponent<ITimeCodePayrollProductDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.payroll.payrollproduct.payrollproduct',
        'time.time.timecode.factor',
        'core.edit',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'payrollProductId',
          terms['time.payroll.payrollproduct.payrollproduct'],
          this.payrollProducts || [],
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

  private loadPayrollProducts(): Observable<void> {
    return this.performLoadData.load$(
      this.service
        .getTimeCodePayrollProducts()
        .pipe(tap(value => (this.payrollProducts = value)))
    );
  }

  private get timeCodePayrollProducts(): FormArray<TimeCodePayrollProductForm> {
    return (this.form as any).payrollProducts;
  }

  override addRow(): void {
    this.editPayrollProduct();
  }

  override edit(row: ITimeCodePayrollProductDTO): void {
    if (this.flowHandler.modifyPermission()) {
      this.editPayrollProduct(row);
    }
  }

  override deleteRow(row: ITimeCodePayrollProductDTO) {
    super.deleteRow(row, this.timeCodePayrollProducts);
  }

  private editPayrollProduct(row?: ITimeCodePayrollProductDTO): void {
    const availablePayrollProducts = this.getAvailablePayrollProducts(row);

    if (availablePayrollProducts.length === 0) {
      this.showCannotAddMessage();
      return;
    }

    this.dialogService
      .open(TimeCodePayrollProductsDialogComponent, {
        title: this.terms[row ? 'core.edit' : 'core.add'],
        dto: structuredClone(row),
        payrollProducts: availablePayrollProducts,
        factorDecimals: this.factorDecimals,
      })
      .afterClosed()
      .subscribe((response: any) => {
        if (response?.success) {
          if (row) {
            if (
              row.payrollProductId !== response.payrollProductId ||
              row.factor !== response.factor
            ) {
              row.payrollProductId = response.payrollProductId;
              row.factor = response.factor;
              this.timeCodePayrollProducts.patchValue(this.rowData.value || []);
              this.form.markAsDirty();
            }
          } else {
            const newrow: ITimeCodePayrollProductDTO = {
              timeCodePayrollProductId: 0,
              timeCodeId: this.form?.getIdControl()?.value || 0,
              payrollProductId: response.payrollProductId,
              factor: response.factor,
            };
            super.addRow(
              newrow,
              this.timeCodePayrollProducts,
              TimeCodePayrollProductForm
            );
          }
          setTimeout(() => {
            this.grid.resetColumns(); // the columns are not resized properly without this when there were no items previously
          });
        }
      });
  }

  private getAvailablePayrollProducts(row?: ITimeCodePayrollProductDTO) {
    return (
      this.payrollProducts?.filter(p => {
        if (row && row.payrollProductId === p.id) {
          return true;
        }
        if (p.state !== SoeEntityState.Active) {
          return false;
        }
        return !this.rowData.value?.some(r => r.payrollProductId === p.id);
      }) ?? []
    );
  }

  private showCannotAddMessage(): void {
    this.messageboxService.show(
      this.translate.instant('core.warning'),
      this.translate.instant(
        this.timeCodePayrollProducts.length === 0
          ? 'time.time.timecode.payrollproducts.noitems'
          : 'time.time.timecode.payrollproducts.allalreadyadded'
      ),
      {
        size: 'md',
        type: 'warning',
        buttons: 'ok',
      }
    );
  }
}
