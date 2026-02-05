import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { DrillDownReportsService } from '../../services/drill-down-reports.service';
import {
  Feature,
  SoeOriginStatusClassificationGroup,
  SoeReportTemplateType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  DrilldownReportGridFlattenedDTO,
  IDrillDownReportDTO,
  SearchVoucherRowsAngDTO,
} from '../../models/drill-down-reports.model';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { EditComponentDialogData } from '@ui/dialog/edit-component-dialog/edit-component-dialog.component';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, of, take, tap } from 'rxjs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ValidationHandler } from '@shared/handlers';
import { ColDef } from 'ag-grid-community';
import { Perform } from '@shared/util/perform.class';
import { EconomyService } from '@features/economy/services/economy.service';
import { IAccountDimDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { VoucherEditComponent } from '@features/economy/voucher/components/voucher-edit/voucher-edit.component';
import { VoucherForm } from '@features/economy/voucher/models/voucher-form.model';
import { SupplierInvoiceDTO } from '@features/economy/shared/supplier-invoice/models/supplier-invoice.model';
import { SupplierInvoiceService } from '@features/economy/shared/supplier-invoice/services/supplier-invoice.service';
import { SupplierInvoiceForm } from '@features/economy/shared/supplier-invoice/models/supplier-invoice-form.model';
import { SupplierInvoiceHistoryDetailsComponent } from '@features/economy/shared/supplier-invoice/components/supplier-invoice-history-details/supplier-invoice-history-details.component';
import { BrowserUtil } from '@shared/util/browser-util';

@Component({
  selector: 'soe-drill-down-reports-grid',
  templateUrl: './drill-down-reports-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DrillDownReportsGridComponent
  extends GridBaseDirective<
    DrilldownReportGridFlattenedDTO,
    DrillDownReportsService
  >
  implements OnInit
{
  service = inject(DrillDownReportsService);
  economyService = inject(EconomyService);
  dialogService = inject(DialogService);
  validationHandler = inject(ValidationHandler);
  performLoad = new Perform<any>(this.progressService);
  searchDTO!: IDrillDownReportDTO;

  gridRowsSubject = new BehaviorSubject<DrilldownReportGridFlattenedDTO[]>([]);

  accountDimId = 0;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Distribution_DrillDownReports,
      'Common.Reports.DrilldownReports',
      {
        lookups: [this.getAccountDimStd()],
        skipDefaultToolbar: true,
        skipInitialLoad: true,
      }
    );
  }

  //#region Override methods

  override onGridReadyToDefine(
    grid: GridComponent<DrilldownReportGridFlattenedDTO>
  ) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'core.warning',
        'common.reports.drilldown.reportgroupname',
        'common.reports.drilldown.reportheadername',
        'common.reports.drilldown.accountnrshort',
        'common.reports.drilldown.accountname',
        'common.reports.drilldown.periodamount',
        'common.reports.drilldown.yearamount',
        'common.reports.drilldown.yearamountshort',
        'common.reports.drilldown.openingbalance',
        'common.reports.drilldown.prevyearamount',
        'common.reports.drilldown.prevperiodamount',
        'common.reports.drilldown.budgetperiodamount',
        'common.reports.drilldown.budgettoperiodamount',
        'common.reports.drilldown.periodprevperioddiff',
        'common.reports.drilldown.yearprevyeardiff',
        'common.reports.drilldown.periodbudgetdiff',
        'common.reports.drilldown.yearbudgetdiff',
        'common.reports.drilldown.vouchernr',
        'common.reports.drilldown.voucherseriesname',
        'common.reports.drilldown.vouchertext',
        'common.reports.drilldown.voucherdate',
        'common.reports.drilldown.debit',
        'common.reports.drilldown.credit',
        'economy.accounting.voucher.voucher',
        'common.credit',
        'common.rownr',
        'common.reports.drilldown.vernr',
        'core.edit',
        'common.reports.drilldown.invalidperiods',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'common.openinvoice',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        const columns: ColDef[] = [];

        columns.push(
          ColumnUtil.createColumnNumber('rowNr', terms['common.rownr'], {
            flex: 1,
          })
        );
        columns.push(
          ColumnUtil.createColumnText(
            'voucherNr',
            terms['common.reports.drilldown.vernr'],
            {
              flex: 1,
            }
          )
        );
        columns.push(
          ColumnUtil.createColumnText(
            'voucherSeriesName',
            terms['common.reports.drilldown.voucherseriesname'],
            {
              flex: 1,
            }
          )
        );
        columns.push(
          ColumnUtil.createColumnText(
            'voucherText',
            terms['common.reports.drilldown.vouchertext'],
            {
              flex: 1,
            }
          )
        );
        columns.push(
          ColumnUtil.createColumnDate(
            'voucherDate',
            terms['common.reports.drilldown.voucherdate'],
            {
              flex: 1,
            }
          )
        );
        columns.push(
          ColumnUtil.createColumnNumber(
            'debit',
            terms['common.reports.drilldown.debit'],
            {
              flex: 1,
              decimals: 2,
            }
          )
        );
        columns.push(
          ColumnUtil.createColumnNumber('credit', terms['common.credit'], {
            flex: 1,
            decimals: 2,
          })
        );
        columns.push(
          ColumnUtil.createColumnIcon('', '', {
            showIcon: row => this.canShowInvoice(row),
            iconName: 'file',
            tooltip: terms['common.openinvoice'],
            suppressFilter: false,
            onClick: row => this.openInvoice(row),
            headerSeparator: true,
          })
        );
        columns.push(
          ColumnUtil.createColumnIconEdit({
            tooltip: terms['core.edit'],
            onClick: row => {
              this.openVoucher(row);
            },
          })
        );

        this.grid.enableMasterDetail(
          {
            columnDefs: columns,
          },
          {
            suppressGridMenu: true,
            detailRowHeight: 120,
            addDefaultExpanderCol: false,
            getDetailRowData: (params: any) => {
              this.loadDetailRows(params).subscribe();
            },
          }
        );
        this.grid.addColumnText(
          'reportGroupName',
          terms['common.reports.drilldown.reportgroupname'],
          { hide: true, grouped: true }
        );

        this.grid.addColumnText(
          'reportHeaderName',
          terms['common.reports.drilldown.reportheadername'],
          { hide: true, grouped: true }
        );

        this.grid.addColumnText(
          'accountNrCount',
          terms['common.reports.drilldown.accountnrshort'],
          {}
        );

        this.grid.addColumnText(
          'accountName',
          terms['common.reports.drilldown.accountname'],
          {}
        );

        this.grid.addColumnNumber(
          'openingBalance',
          terms['common.reports.drilldown.openingbalance'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );
        this.grid.addColumnNumber(
          'periodAmount',
          terms['common.reports.drilldown.periodamount'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );
        this.grid.addColumnNumber(
          'yearAmount',
          terms['common.reports.drilldown.yearamount'] +
            ' / ' +
            terms['common.reports.drilldown.yearamountshort'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );

        this.grid.addColumnNumber(
          'budgetPeriodAmount',
          terms['common.reports.drilldown.budgetperiodamount'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );
        this.grid.addColumnNumber(
          'periodBudgetDiff',
          terms['common.reports.drilldown.periodbudgetdiff'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );

        this.grid.addColumnNumber(
          'budgetToPeriodEndAmount',
          terms['common.reports.drilldown.budgettoperiodamount'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );
        this.grid.addColumnNumber(
          'yearBudgetDiff',
          terms['common.reports.drilldown.yearbudgetdiff'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );

        this.grid.addColumnNumber(
          'prevPeriodAmount',
          terms['common.reports.drilldown.prevperiodamount'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );
        this.grid.addColumnNumber(
          'periodPrevPeriodDiff',
          terms['common.reports.drilldown.periodprevperioddiff'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );

        this.grid.addColumnNumber(
          'prevYearAmount',
          terms['common.reports.drilldown.prevyearamount'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );
        this.grid.addColumnNumber(
          'yearPrevYearDiff',
          terms['common.reports.drilldown.yearprevyeardiff'],
          { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' }
        );

        this.grid.groupDisplayType = 'multipleColumns';

        super.finalizeInitGrid();

        const accountNrCountColDef =
          this.grid.getColumnDefByField('accountNrCount');
        accountNrCountColDef.cellRenderer = 'agGroupCellRenderer';
        this.hideShowColumns();
        this.grid.resetColumns();
      });
  }

  //#endregion

  //#region Data Loding Functions

  getAccountDimStd() {
    return this.economyService.getAccountDimStd().pipe(
      tap((data: IAccountDimDTO) => {
        this.accountDimId = data.accountDimId;
      })
    );
  }

  loadDetailRows(params: any) {
    const model = this.getDefaultSearchVoucherRowsAngDTO();
    model.dim1AccountFr = params.data.accountNr;
    model.dim1AccountTo = params.data.accountNr;
    model.voucherDateFrom = this.searchDTO.accountPeriodFrom;
    model.voucherDateTo = this.searchDTO.accountPeriodTo;

    if (params.data.rowsLoaded) {
      return of([]).pipe(
        tap(() => {
          params.successCallback(params.data.rows);
        })
      );
    } else {
      return this.performLoad.load$(
        this.service.getDrilldownReportVoucherRows(model).pipe(
          tap(rows => {
            params.data.rowsLoaded = true;
            params.data.rows = rows;
            params.successCallback(rows);
          })
        )
      );
    }
  }

  //#endregion

  //#region Helper methods

  private showAllChangedColumns() {
    if (this.grid) {
      const allColumns: string[] = [
        'openingBalance',
        'budgetPeriodAmount',
        'periodBudgetDiff',
        'budgetToPeriodEndAmount',
        'yearBudgetDiff',
        'prevPeriodAmount',
        'periodPrevPeriodDiff',
        'prevYearAmount',
        'yearPrevYearDiff',
      ];
      this.grid.showColumns(allColumns);
    }
  }

  private hideShowColumns() {
    const sysReportTemplateTypeId = this.searchDTO?.sysReportTemplateTypeId;
    const budgetId = this.searchDTO?.budgetId || 0;
    const showColumns: string[] = [];
    const hideColumns: string[] = [];
    if (sysReportTemplateTypeId && this.grid) {
      this.showAllChangedColumns();
      if (sysReportTemplateTypeId == SoeReportTemplateType.BalanceReport) {
        showColumns.push('openingBalance');
        hideColumns.push(
          'prevPeriodAmount',
          'periodPrevPeriodDiff',
          'prevYearAmount',
          'yearPrevYearDiff',
          'budgetPeriodAmount',
          'periodBudgetDiff',
          'budgetToPeriodEndAmount',
          'yearBudgetDiff'
        );
      } else if (
        (sysReportTemplateTypeId == SoeReportTemplateType.ResultReport ||
          sysReportTemplateTypeId == SoeReportTemplateType.ResultReportV2) &&
        budgetId != 0
      ) {
        showColumns.push(
          'budgetPeriodAmount',
          'periodBudgetDiff',
          'budgetToPeriodEndAmount',
          'yearBudgetDiff'
        );
        hideColumns.push(
          'openingBalance',
          'prevPeriodAmount',
          'periodPrevPeriodDiff',
          'prevYearAmount',
          'yearPrevYearDiff'
        );
      } else {
        showColumns.push(
          'prevPeriodAmount',
          'periodPrevPeriodDiff',
          'prevYearAmount',
          'yearPrevYearDiff'
        );
        hideColumns.push(
          'openingBalance',
          'budgetPeriodAmount',
          'periodBudgetDiff',
          'budgetToPeriodEndAmount',
          'yearBudgetDiff'
        );
      }
      this.grid.showColumns(showColumns);
      this.grid.hideColumns(hideColumns);
    }
  }

  setGridData(data: DrilldownReportGridFlattenedDTO[] = this.getGridRows()) {
    this.gridRowsSubject.next(data);
  }

  getGridRows() {
    return this.gridRowsSubject.getValue() as DrilldownReportGridFlattenedDTO[];
  }

  getDefaultSearchVoucherRowsAngDTO(): SearchVoucherRowsAngDTO {
    const model = new SearchVoucherRowsAngDTO();
    model.voucherSeriesIdFrom = 0;
    model.voucherSeriesIdTo = 0;
    model.debitFrom = 0;
    model.debitTo = 0;
    model.creditFrom = 0;
    model.creditTo = 0;
    model.amountFrom = 0;
    model.amountTo = 0;
    model.voucherText = '';
    model.createdBy = '';
    model.dim1AccountId = this.accountDimId;
    model.dim1AccountFr = '';
    model.dim1AccountTo = '';
    model.dim2AccountId = 0;
    model.dim2AccountFr = '';
    model.dim2AccountTo = '';
    model.dim3AccountId = 0;
    model.dim3AccountFr = '';
    model.dim3AccountTo = '';
    model.dim4AccountId = 0;
    model.dim4AccountFr = '';
    model.dim4AccountTo = '';
    model.dim5AccountId = 0;
    model.dim5AccountFr = '';
    model.dim5AccountTo = '';
    model.dim6AccountId = 0;
    model.dim6AccountFr = '';
    model.dim6AccountTo = '';
    return model;
  }

  private canShowInvoice(row: any): boolean {
    return !!(row.invoiceId && row.invoiceId > 0);
  }

  private openVoucher(row: any): void {
    if (row.voucherHeadId)
      this.openEditInNewTab.emit({
        id: row.voucherHeadId,
        additionalProps: {
          editComponent: VoucherEditComponent,
          editTabLabel: 'economy.accounting.voucher.voucher',
          FormClass: VoucherForm,
        },
      });

    this.edit(
      {
        ...row.voucherHead,
      },
      {
        editComponent: VoucherEditComponent,
        editTabLabel: 'economy.accounting.voucher.voucher',
        FormClass: VoucherForm,
        rows: [],
        filteredRows: [],
      }
    );
  }

  private openInvoice(row: any): void {
    if (!row.invoiceId) return;
    BrowserUtil.openInNewTab(
      window,
      `/soe/economy/supplier/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleSupplierInvoices}&invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceNr}`
    );
  }

  //#endregion

  //#region UI events

  onSearchValueChange(search: IDrillDownReportDTO) {
    this.searchDTO = search;
    this.setGridData([] as DrilldownReportGridFlattenedDTO[]);
    this.hideShowColumns();
  }

  onCreateReport(data: DrilldownReportGridFlattenedDTO[]) {
    this.setGridData(data);
  }

  //#endregion
}
