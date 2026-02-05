import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonCustomerService } from '@billing/shared/services/common-customer.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ICustomerStatisticsDTO } from '@shared/models/generated-interfaces/CustomerStatisticsDTO';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { tap } from 'rxjs';
import { ValidationHandler } from '@shared/handlers';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { AggregationType } from '@ui/grid/interfaces';

@Component({
  selector: 'soe-customer-statistics',
  templateUrl: './customer-statistics.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerStatisticsComponent
  extends GridBaseDirective<ICustomerStatisticsDTO>
  implements OnInit
{
  readonly commonCustomerService = inject(CommonCustomerService);
  readonly coreService = inject(CoreService);

  validationHandler = inject(ValidationHandler);

  @Input({ required: true }) customerId: number = 0;
  allItemsSelectionDict: SmallGenericType[] = [];

  statisticsForm = new SoeFormGroup(this.validationHandler, {
    selectedAllItemsId: new SoeSelectFormControl(1),
    selectedTotal: new SoeNumberFormControl(0, { decimals: 2, disabled: true }),
    filteredTotal: new SoeNumberFormControl(0, { decimals: 2, disabled: true }),
  });

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'statisticsGrid', {
      skipInitialLoad: true,
      lookups: [this.loadSelectionTypes()],
    });
  }

  loadCustomerStatistics() {
    return this.commonCustomerService
      .getCustomerStatistics(
        this.customerId,
        this.statisticsForm.get('selectedAllItemsId')?.value
      )
      .pipe(
        tap(data => {
          this.rowData.next(data);
          this.calculateFilteredRows(data);
        })
      )
      .subscribe();
  }

  loadSelectionTypes() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.ChangeStatusGridAllItemsSelection,
        false,
        true,
        true
      )
      .pipe(
        tap(x => {
          this.allItemsSelectionDict = x;
          this.statisticsForm
            .get('selectedAllItemsId')
            ?.setValue(x.length > 0 ? x[0].id : 1);
        })
      );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ICustomerStatisticsDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.date',
        'common.type',
        'common.invoicenr',
        'common.productnr',
        'common.name',
        'common.quantity',
        'common.price',
        'common.amount',
        'common.purchaseprice',
        'common.customer.customer.marginalincome',
        'common.customer.customer.marginalincomeratioprocent',
        'common.statistics',
      ])
      .subscribe(translations => {
        this.terms = translations;

        this.grid.api.updateGridOptions({
          onFilterModified: () => {
            setTimeout(
              () => this.calculateFilteredRows(this.grid.getFilteredRows()),
              100
            );
          },
        });

        this.grid.addColumnText('productName', this.terms['common.name'], {
          flex: 1,
          enableGrouping: true,
        });
        this.grid.addColumnDate('date', this.terms['common.date'], {
          flex: 1,
          enableHiding: false,
          enableGrouping: true,
        });
        this.grid.addColumnText('invoiceNr', this.terms['common.invoicenr'], {
          flex: 1,
          enableGrouping: true,
        });
        this.grid.addColumnNumber(
          'productQuantity',
          this.terms['common.quantity'],
          { flex: 1, enableGrouping: true, aggFuncOnGrouping: 'sum' }
        );
        this.grid.addColumnNumber('productPrice', this.terms['common.price'], {
          flex: 1,
          enableHiding: true,
          decimals: 2,
          enableGrouping: true,
        });
        this.grid.addColumnNumber(
          'productPurchasePrice',
          this.terms['common.purchaseprice'],
          { flex: 1, enableHiding: true, decimals: 2 }
        );
        this.grid.addColumnNumber(
          'productSumAmount',
          this.terms['common.amount'],
          {
            flex: 1,
            enableHiding: true,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
          }
        );
        this.grid.addColumnNumber(
          'productMarginalIncome',
          this.terms['common.customer.customer.marginalincome'],
          { flex: 1, enableGrouping: true, enableHiding: true, decimals: 2 }
        );
        this.grid.addColumnNumber(
          'productMarginalRatio',
          this.terms['common.customer.customer.marginalincomeratioprocent'],
          { flex: 1, enableGrouping: true, enableHiding: true, decimals: 2 }
        );
        this.grid.addColumnText('originType', this.terms['common.type'], {
          flex: 1,
          enableHiding: false,
          enableGrouping: true,
        });
        this.grid.addColumnText('productNr', this.terms['common.productnr'], {
          flex: 1,
          enableGrouping: true,
        });

        this.grid.context.suppressGridMenu = true;
        this.grid.useGrouping();
        this.grid.setNbrOfRowsToShow(5);
        this.grid.enableRowSelection();

        this.grid.addAggregationsRow({
          productQuantity: AggregationType.Sum,
          productSumAmount: AggregationType.Sum,
        });

        super.finalizeInitGrid();
      });
  }

  allItemsSelectionChanged(selectionItem: number) {
    this.statisticsForm.get('selectedAllItemsId')?.setValue(selectionItem);
  }

  protected rowDataSelectionChanged(rows: ICustomerStatisticsDTO[]): void {
    let selectedTotal: number = 0;

    rows.forEach(row => {
      selectedTotal += row ? row.productSumAmount : 0;
    });

    this.statisticsForm.get('selectedTotal')?.setValue(selectedTotal);
  }

  private calculateFilteredRows(rows: ICustomerStatisticsDTO[]) {
    let filteredTotal: number = 0;

    rows.forEach(row => {
      filteredTotal += row ? row.productSumAmount : 0;
    });

    this.statisticsForm.get('filteredTotal')?.setValue(filteredTotal);
  }
}
