import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { AggregationType } from '@ui/grid/interfaces';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarButtonAction } from '@ui/toolbar/toolbar-button/toolbar-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Observable, of, take, tap } from 'rxjs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { DistributionSalesEuService } from '../../services/distribution-sales-eu.service';
import {
  DistributionSalesEuFilterDTO,
  SalesEUGridDTO,
} from '../../models/distribution-sales-eu.model';
import { ISalesEUDetailDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ExportUtil } from '@shared/util/export-util';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'soe-distribution-sales-eu-grid',
  templateUrl: './distribution-sales-eu-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DistributionSalesEuGridComponent
  extends GridBaseDirective<SalesEUGridDTO, DistributionSalesEuService>
  implements OnInit
{
  service = inject(DistributionSalesEuService);
  private readonly progress = inject(ProgressService);
  private readonly performLoad = new Perform<SalesEUGridDTO[]>(this.progress);
  private readonly performLoadDetails = new Perform<ISalesEUDetailDTO[]>(
    this.progress
  );
  private dateFilter = signal<DistributionSalesEuFilterDTO | undefined>(
    undefined
  );
  toolbarButtonSignal = signal<ToolbarButtonAction>(undefined);
  private toolbarDownloadDisabled = signal(true);

  constructor() {
    super();

    effect(() => {
      const action = this.toolbarButtonSignal();
      if (action) {
        this.CreateFile();
      }
    });
  }

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Export_SalesEU,
      'Economy.Distribution.SalesEU',
      { skipInitialLoad: true }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('download', {
          iconName: signal('download'),
          caption: signal('economy.reports.createfile'),
          tooltip: signal('economy.reports.createfile'),
          disabled: this.toolbarDownloadDisabled,
          onAction: () => this.CreateFile(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<SalesEUGridDTO>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.customer.customer.customernr',
        'common.customer',
        'common.customer.customer.vatnr',
        'economy.reports.valueofsaleofgoods',
        'economy.reports.valueofsaleofservices',
        'economy.reports.valueofTriangulationSales',
        'common.customer.invoices.invoicenr',
        'common.customer.invoices.invoicedate',
        'common.customer.invoices.amountexvat',
        'common.customer.invoices.amountexvat.tooltip',
        'economy.reports.valueofsaleofgoods.tooltip',
        'economy.reports.valueofsaleofservices.tooltip',
        'economy.reports.valueofTriangulationSales.tooltip',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        //Setup Detail Grid
        this.grid.enableMasterDetail(
          {
            detailRowHeight: 300,
            floatingFiltersHeight: 0,

            columnDefs: [
              ColumnUtil.createColumnNumber(
                'invoiceNr',
                terms['common.customer.invoices.invoicenr'],
                {
                  flex: 1,
                  alignLeft: true,
                  enableHiding: false,
                  sort: 'desc',
                }
              ),
              ColumnUtil.createColumnDate(
                'invoiceDate',
                terms['common.customer.invoices.invoicedate'],
                {
                  flex: 1,
                  enableHiding: false,
                }
              ),
              ColumnUtil.createColumnNumber(
                'totalAmountExVat',
                terms['common.customer.invoices.amountexvat'],
                {
                  flex: 1,
                  enableHiding: false,
                  decimals: 2,
                  tooltip:
                    terms['common.customer.invoices.amountexvat.tooltip'],
                }
              ),
              ColumnUtil.createColumnNumber(
                'sumGoodsSale',
                terms['economy.reports.valueofsaleofgoods'],
                {
                  flex: 1,
                  enableHiding: false,
                  decimals: 2,
                }
              ),
              ColumnUtil.createColumnNumber(
                'sumServiceSale',
                terms['economy.reports.valueofsaleofservices'],
                {
                  flex: 1,
                  enableHiding: false,
                  decimals: 2,
                }
              ),
            ],
          },
          {
            getDetailRowData: this.loadSalesEuDetails.bind(this),
          }
        );

        //Setup Master Grid
        this.grid.addColumnText('customerName', terms['common.customer'], {
          flex: 1,
          enableHiding: false,
          sort: 'asc',
        });
        this.grid.addColumnText(
          'customerNr',
          terms['common.customer.customer.customernr'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'vatNr',
          terms['common.customer.customer.vatnr'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnNumber(
          'sumGoodsSale',
          terms['economy.reports.valueofsaleofgoods'],
          {
            flex: 1,
            enableHiding: false,
            decimals: 2,
            tooltip: terms['economy.reports.valueofsaleofgoods.tooltip'],
          }
        );
        this.grid.addColumnNumber(
          'sumServiceSale',
          terms['economy.reports.valueofsaleofservices'],
          {
            flex: 1,
            enableHiding: false,
            decimals: 2,
            tooltip: terms['economy.reports.valueofsaleofservices.tooltip'],
          }
        );
        this.grid.addColumnNumber(
          'sumTriangulationSales',
          terms['economy.reports.valueofTriangulationSales'],
          {
            flex: 1,
            enableHiding: false,
            decimals: 2,
            tooltip: terms['economy.reports.valueofTriangulationSales.tooltip'],
          }
        );

        this.grid.addAggregationsRow({
          sumGoodsSale: AggregationType.Sum,
          sumServiceSale: AggregationType.Sum,
          sumTriangulationSales: AggregationType.Sum,
        });

        super.finalizeInitGrid();
      });
  }

  private resetFilter(): void {
    this.dateFilter.set(undefined);
    this.rowData = new BehaviorSubject<SalesEUGridDTO[]>([]);
  }

  override loadData(id?: number | undefined): Observable<SalesEUGridDTO[]> {
    if (!this.dateFilter()) return of([]);

    const from = this.dateFilter()?.startDate;
    const to = this.dateFilter()?.stopDate;

    if (!from || !to) return of([]);

    return this.performLoad.load$(
      this.service.getGrid(undefined, { startDate: from, stopDate: to })
    );
  }

  private loadSalesEuDetails(params: any): void {
    if (!params.data['detailsLoaded']) {
      if (this.dateFilter()) {
        const from = this.dateFilter()?.startDate;
        const to = this.dateFilter()?.stopDate;
        if (from && to) {
          this.performLoadDetails.load(
            this.service.getDetailGrid(params.data.actorId, from, to).pipe(
              tap(value => {
                params.data['detailsRows'] = value;
                params.data['detailsLoaded'] = true;
                params.successCallback(value);
              })
            )
          );
        }
      }
    } else {
      params.successCallback(params.data['detailsRows']);
    }
  }

  protected filterChanged(
    filter: DistributionSalesEuFilterDTO | undefined
  ): void {
    this.resetFilter();
    if (filter) {
      this.dateFilter.set(filter);
      this.refreshGrid();
    }
    this.toggleDownloadButton();
  }

  private toggleDownloadButton(): void {
    this.toolbarDownloadDisabled.set(
      !(
        this.dateFilter() &&
        this.dateFilter()?.startDate &&
        this.dateFilter()?.stopDate
      )
    );
  }

  private CreateFile(): void {
    this.toolbarButtonSignal.set(undefined);
    if (this.dateFilter()) {
      const from = this.dateFilter()?.startDate;
      const to = this.dateFilter()?.stopDate;
      const reportPeriod = this.dateFilter()?.reportPeriod;
      if (reportPeriod && from && to) {
        this.service
          .getExportFile(reportPeriod, from, to)
          .pipe(
            tap(results => {
              results.forEach(result =>
                ExportUtil.Export(result, 'Periodisk.txt')
              );
            })
          )
          .subscribe();
      }
    }
  }
}
