import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import {
  Feature,
  TermGroup,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellClassParams } from 'ag-grid-community';
import { Observable, of, take, tap } from 'rxjs';
import { PaymentImportDTO } from '../../models/import-payments.model';
import { ImportPaymentsService } from '../../services/import-payments.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service';

@Component({
  selector: 'soe-import-payments-grid',
  templateUrl: './import-payments-grid.component.html',
  styleUrls: ['./import-payments-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ImportPaymentsGridComponent
  extends GridBaseDirective<PaymentImportDTO, ImportPaymentsService>
  implements OnInit
{
  isGridReady = false;
  isFilterLoad = false;

  private readonly coreService = inject(CoreService);

  service = inject(ImportPaymentsService);

  durationSelection!: number;

  private importPaymentTypes: SmallGenericType[] = []; 

  ngOnInit() {
    super.ngOnInit();

    this.service.durationSelection$.subscribe(durationSelection => {
      this.durationSelection = durationSelection;
    });

    this.startFlow(
      Feature.Economy_Import_Payments,
      'Economy.Import.Payment.Payments',
      {
        lookups: [this.loadImportPaymentTypes()],
      }
    );
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms(['economy.import.payment.download']);
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
  }

  override onGridReadyToDefine(grid: GridComponent<PaymentImportDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'economy.import.payments.importdate',
        'economy.import.payments.syspaymenttype',
        'economy.import.payments.type',
        'economy.import.payments.totalamount',
        'economy.import.payments.numberofpayments',
        'economy.import.payment.importBatchId',
        'core.createdby',
        'common.status',
        'core.created',
        'core.edit',
        'economy.import.payment.label',
        'economy.import.payment.paiddate',
        'economy.import.payment.importpaymenttype',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'batchId',
          terms['economy.import.payment.importBatchId'],
          {
            flex: 1,
            enableHiding: true,
            cellClassRules: {
              'disabled-grid-row-background-color': (
                params: CellClassParams
              ) => {
                return this.disabledCellRules(params);
              },
            },
          }
        );
        this.grid.addColumnSelect<SmallGenericType>(
          'importPaymentTypeTermId',
          terms['economy.import.payment.importpaymenttype'],
          this.importPaymentTypes,
          undefined,
          {
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'importDate',
          terms['economy.import.payments.importdate'],
          {
            flex: 1,
            enableHiding: true,
            cellClassRules: {
              'disabled-grid-row-background-color': (
                params: CellClassParams
              ) => {
                return this.disabledCellRules(params);
              },
            },
          }
        );

        this.grid.addColumnText(
          'typeName',
          terms['economy.import.payments.type'],
          {
            flex: 1,
            cellClassRules: {
              'disabled-grid-row-background-color': (
                params: CellClassParams
              ) => {
                return this.disabledCellRules(params);
              },
            },
          }
        );
        this.grid.addColumnNumber(
          'totalAmount',
          terms['economy.import.payments.totalamount'],
          {
            flex: 1,
            decimals: 2,
            cellClassRules: {
              'disabled-grid-row-background-color': (
                params: CellClassParams
              ) => {
                return this.disabledCellRules(params);
              },
            },
          }
        );
        this.grid.addColumnText(
          'numberOfPayments',
          terms['economy.import.payments.numberofpayments'],
          {
            flex: 1,
            cellClassRules: {
              'disabled-grid-row-background-color': (
                params: CellClassParams
              ) => {
                return this.disabledCellRules(params);
              },
            },
          }
        );
        this.grid.addColumnText('createdBy', terms['core.createdby'], {
          flex: 1,
          enableHiding: true,
          cellClassRules: {
            'disabled-grid-row-background-color': (params: CellClassParams) => {
              return this.disabledCellRules(params);
            },
          },
        });
        this.grid.addColumnDate('created', terms['core.created'], {
          flex: 1,
          enableHiding: true,
          cellClassRules: {
            'disabled-grid-row-background-color': (params: CellClassParams) => {
              return this.disabledCellRules(params);
            },
          },
        });
        this.grid.addColumnText('statusName', terms['common.status'], {
          flex: 1,
          enableHiding: true,
          cellClassRules: {
            'disabled-grid-row-background-color': (params: CellClassParams) => {
              return this.disabledCellRules(params);
            },
          },
        });

        this.grid.addColumnText(
          'paymentLabel',
          terms['economy.import.payment.label'],
          {
            enableHiding: true,
            flex: 1,
            cellClassRules: {
              'disabled-grid-row-background-color': (
                params: CellClassParams
              ) => {
                return this.disabledCellRules(params);
              },
            },
          }
        );
        this.grid.addColumnIcon('transferStateIcon', '', {
          flex: 1,
          showIcon: row => row.showTransferStatusIcon,
          enableHiding: true,
          //iconName: 'exclamation-triangle',
          tooltipField: 'transferStateIconText',
          iconClassField: 'transferStateIconClass',
          cellClassRules: {
            'disabled-grid-row-background-color': (params: CellClassParams) => {
              return this.disabledCellRules(params);
            },
          },
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
          cellClassRules: {
            'disabled-grid-row-background-color': (params: CellClassParams) => {
              return this.disabledCellRules(params);
            },
          },
        });
        super.finalizeInitGrid();
      });
  }

  override loadData(): Observable<PaymentImportDTO[]> {
    if (!this.durationSelection) {
      return of([]);
    }

    const additionalProps = {
      allItemsSelection: this.durationSelection
    };

    return super.loadData(undefined, additionalProps);
  }

  disabledCellRules(params: any) {
    if (params.data.state === SoeEntityState.Inactive) {
      return true;
    }
    return false;
  }

  override onFinished(): void {
    this.isGridReady = true;
    if (this.isFilterLoad) {
      this.refreshGrid();
    }
  }

  onFilterReady(durationSelection: number) {
    this.isFilterLoad = true;
    this.service.setDurationSelectionSubject(durationSelection);
    if (this.isGridReady) {
      this.refreshGrid();
    }
  }

  filterOnChange(durationSelection: number) {
    if (this.isFilterLoad && this.isGridReady) {
      this.service.setDurationSelectionSubject(durationSelection);
      this.refreshGrid();
    }
  }

  
  private loadImportPaymentTypes(): Observable<SmallGenericType[]>  {
    return this.coreService
      .getTermGroupContent(TermGroup.ImportPaymentType, false, false)
      .pipe(
        tap(res => {
          this.importPaymentTypes = res;
        })
      );
  }
}
