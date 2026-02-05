import { Component, OnInit, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IIntrastatTransactionExportDTO } from '@shared/models/generated-interfaces/CommodityCodeDTO';
import {
  Feature,
  IntrastatReportingType,
  SoeOriginStatusClassificationGroup,
  SoeOriginType,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { Observable, take, tap } from 'rxjs';
import { ValidationHandler } from '@shared/handlers';
import { CrudActionTypeEnum } from '@shared/enums';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IntrastatExportGridHeaderDTO } from '../../models/intrastat-export.model';
import { IntrastatExportService } from '../../services/intrastat-export.service';
import { PurchaseEditComponent } from '@features/billing/purchase/components/purchase-edit/purchase-edit.component';
import { PurchaseForm } from '@features/billing/purchase/models/purchase-form.model';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-intrastat-export-grid',
  templateUrl: './intrastat-export-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class IntrastatExportGridComponent
  extends GridBaseDirective<
    IIntrastatTransactionExportDTO,
    IntrastatExportService
  >
  implements OnInit
{
  public progressService = inject(ProgressService);
  service = inject(IntrastatExportService);
  public validationHandler = inject(ValidationHandler);
  dialogService = inject(DialogService);

  isButtonDisable = signal(true);

  performGridLoad = new Perform<IIntrastatTransactionExportDTO[]>(
    this.progressService
  );
  searchModel: IntrastatExportGridHeaderDTO =
    new IntrastatExportGridHeaderDTO();

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Intrastat_ReportsAndExport,
      'common.intrastat.reportingandexport'
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      reloadOption: {
        onAction: () => this.refreshGrid(),
      },
    });
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('download', {
          iconName: signal('download'),
          caption: signal('economy.reports.createfile'),
          tooltip: signal('economy.reports.createfile'),
          disabled: this.isButtonDisable,
          onAction: () => this.createExportFile(),
        }),
      ],
    });
  }

  canCreateFile(isDisabled: boolean) {
    this.isButtonDisable.set(isDisabled);
  }

  createExportFile() {
    if (this.searchModel) {
      const model = {
        DateFrom: this.searchModel.fromDate,
        DateTo: this.searchModel.endDate,
        HasDateInterval: true,
        Special:
          this.searchModel.reportingType === IntrastatReportingType.Export
            ? 'export'
            : 'import',
      };
      this.performGridLoad.crud(
        CrudActionTypeEnum.Save,
        this.service.createIntrastatExport(model).pipe(
          tap(result => {
            BrowserUtil.openInSameTab(
              window,
              ResponseUtil.getStringValue(result)
            );
          })
        ),
        undefined,
        undefined,
        {
          showToastOnComplete: false,
        }
      );
    }
  }

  doSearch(event: IntrastatExportGridHeaderDTO) {
    this.searchModel = event;
    this.refreshGrid();
  }

  override loadData(
    id?: number | undefined
  ): Observable<IIntrastatTransactionExportDTO[]> {
    return this.performGridLoad.load$(
      this.service.getGrid(undefined, {
        intrastatReportingType: this.searchModel.reportingType,
        fromDate: DateUtil.toDateTimeString(this.searchModel.fromDate),
        toDate: DateUtil.toDateTimeString(this.searchModel.endDate),
      })
    );
  }

  onGridReadyToDefine(
    grid: GridComponent<IIntrastatTransactionExportDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.type',
        'common.name',
        'common.intrastat.vatnr',
        'common.country',
        'common.countryoforigin',
        'common.customer.invoices.productnr',
        'common.customer.invoices.productname',
        'common.customer.invoices.quantity',
        'common.customer.invoices.unit',
        'common.commoditycodes.netweight',
        'common.commoditycodes.otherquantity',
        'common.commoditycodes.code',
        'economy.accounting.liquidityplanning.transactiontype',
        'common.customer.invoices.seqnr',
        'common.number',
        'common.customer.invoices.invoicedate',
        'common.voucherdate',
        'common.customer.invoices.amount',
        'common.customer.invoices.foreignamount',
        'common.customer.invoices.currencycode',
        'common.startdate',
        'common.stopdate',
        'common.intrastat.import',
        'common.intrastat.export',
        'common.intrastat.both',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('originTypeName', terms['common.type'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: false,
          grouped: true,
        });

        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: false,
        });
        this.grid.addColumnText('vatNr', terms['common.intrastat.vatnr'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: false,
        });
        this.grid.addColumnText('country', terms['common.country'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: false,
        });
        this.grid.addColumnText(
          'originCountry',
          terms['common.countryoforigin'],
          { flex: 1, enableGrouping: true, enableHiding: false, grouped: true }
        );
        this.grid.addColumnText(
          'productNr',
          terms['common.customer.invoices.productnr'],
          { flex: 1, enableGrouping: true, enableHiding: true }
        );
        this.grid.addColumnText(
          'productName',
          terms['common.customer.invoices.productname'],
          { flex: 1, enableGrouping: true, enableHiding: true }
        );
        this.grid.addColumnNumber(
          'quantity',
          terms['common.customer.invoices.quantity'],
          { flex: 1, enableHiding: true, aggFuncOnGrouping: 'sum' }
        );
        this.grid.addColumnText(
          'productUnitCode',
          terms['common.customer.invoices.unit'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnNumber(
          'netWeight',
          terms['common.commoditycodes.netweight'],
          {
            flex: 1,
            enableHiding: false,
            decimals: 3,
            editable: true,
            maxDecimals: 3,
            aggFuncOnGrouping: 'sum',
          }
        );
        this.grid.addColumnText(
          'otherQuantity',
          terms['common.commoditycodes.otherquantity'],
          { flex: 1, enableHiding: true, editable: false }
        );
        this.grid.addColumnText(
          'intrastatCodeName',
          terms['common.commoditycodes.code'],
          { flex: 1, enableGrouping: true, enableHiding: true }
        );
        this.grid.addColumnText(
          'intrastatTransactionTypeName',
          terms['economy.accounting.liquidityplanning.transactiontype'],
          { flex: 1, enableGrouping: true, enableHiding: true }
        );

        this.grid.addColumnText(
          'seqNr',
          terms['common.customer.invoices.seqnr'],
          {
            flex: 1,
            enableGrouping: false,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('originNr', terms['common.number'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnDate(
          'invoiceDate',
          terms['common.customer.invoices.invoicedate'],
          { flex: 1, enableHiding: true, enableGrouping: true }
        );
        this.grid.addColumnDate('voucherDate', terms['common.voucherdate'], {
          flex: 1,
          enableHiding: true,
          enableGrouping: true,
        });
        this.grid.addColumnNumber(
          'amount',
          terms['common.customer.invoices.amount'],
          { flex: 1, enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' }
        );
        this.grid.addColumnNumber(
          'amountCurrency',
          terms['common.customer.invoices.foreignamount'],
          { flex: 1, enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' }
        );
        this.grid.addColumnText(
          'currencyCode',
          terms['common.customer.invoices.currencycode'],
          { flex: 1, enableHiding: true }
        );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => this.openOrigin(row),
        });

        this.grid.agGrid.api.updateGridOptions({
          groupDefaultExpanded: 1,
        });

        this.grid.useGrouping({
          stickyGroupTotalRow: 'bottom',
          includeFooter: true,
          includeTotalFooter: true,
          groupSelectsFiltered: false,
          keepColumnsAfterGroup: false,
          selectChildren: false,
        });

        super.finalizeInitGrid();
      });
  }

  public openOrigin(row: any) {
    switch (row.originType) {
      case SoeOriginType.SupplierInvoice:
        this.openSupplierInvoice(row);
        break;
      case SoeOriginType.Purchase:
        this.openPurchase(row);
        break;
      default:
        this.openCustomerInvoice(row);
        break;
    }
  }

  openSupplierInvoice(row: any) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/economy/supplier/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleSupplierInvoices}&invoiceId=${row.originId}&invoiceNr=${row.originNr}`
    );
  }

  openPurchase(row: any) {
    /*const supplierInvoiceId = row.originId;
    const invoiceNr = row.originNr;
    BrowserUtil.openInNewTab(
      window,
      `/soe/economy/supplier/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleSupplierInvoices}&invoiceId=${supplierInvoiceId}&invoiceNr=${invoiceNr}`
    );*/

    this.edit(
      {
        ...row,
        purchaseNr: row.originNr,
        purchaseId: row.originId,
      },
      {
        filteredRows: [],
        editComponent: PurchaseEditComponent,
        editTabLabel: 'billing.purchase.list.purchase',
        FormClass: PurchaseForm,
      }
    );
  }

  openCustomerInvoice(row: any) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}&invoiceId=${row.originId}&invoiceNr=${row.originNr}`
    );
  }
}
