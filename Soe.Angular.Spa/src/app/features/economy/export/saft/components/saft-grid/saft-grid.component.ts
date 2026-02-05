import { Component, OnInit, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ExportUtil } from '@shared/util/export-util';
import { IconUtil } from '@shared/util/icon-util';
import { Perform } from '@shared/util/perform.class';
import { DateUtil } from '@shared/util/date-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take } from 'rxjs';
import { SaftExportDTO } from '../../models/SaftExportDTO.model';
import { ISaftGridSearch } from '../../models/SaftGridSearchDTO.model';
import { SaftService } from '../../services/saft.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-saft-grid',
  templateUrl: './saft-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SaftGridComponent
  extends GridBaseDirective<SaftExportDTO, SaftService>
  implements OnInit
{
  private progress = inject(ProgressService);
  private translateService = inject(TranslateService);
  public flowHandler = inject(FlowHandlerService);
  service = inject(SaftService);
  performGridLoad = new Perform<SaftExportDTO[]>(this.progress);
  performFileDownload = new Perform<BackendResponse>(this.progress);
  fromDate!: Date;
  toDate!: Date;

  constructor() {
    super();
    const today = DateUtil.getToday();
    this.fromDate = DateUtil.getDateFirstInMonth(today);
    this.toDate = DateUtil.getDateLastInMonth(today);
  }

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Export_SAFT, 'economy.export.saft', {
      useLegacyToolbar: true,
    });
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar();

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'download'),
          label: 'economy.reports.createfile',
          title: 'economy.reports.createfile',
          onClick: () => this.downloadFile(),
          disabled: signal(false),
          hidden: signal(false),
        }),
      ],
    });
  }

  downloadFile() {
    this.performFileDownload.crud(
      CrudActionTypeEnum.Load,
      this.service.getSAFTExportFile(this.fromDate, this.toDate),
      (result: BackendResponse) => {
        if (result.success) {
          const strValue = ResponseUtil.getStringValue(result);
          if (strValue && strValue.length > 0) {
            ExportUtil.Export(strValue, 'SAFT.xml');
          } else {
            this.progress.loadError({
              title: this.translateService.instant('core.info'),
              message: this.translateService.instant(
                'core.noresultfromselection'
              ),
            });
            console.log('Error: ', result);
          }
        }
      }
    );
  }

  executeSearch(event: ISaftGridSearch) {
    this.fromDate = event.fromDate;
    this.toDate = event.toDate;
    this.refreshGrid();
  }

  override loadData(id?: number | undefined): Observable<SaftExportDTO[]> {
    return this.performGridLoad.load$(
      this.service.getGrid(undefined, {
        dateFrom: this.fromDate,
        dateTo: this.toDate,
      })
    );
  }

  onGridReadyToDefine(grid: GridComponent<SaftExportDTO>) {
    super.onGridReadyToDefine(grid);

    this.translateService
      .get([
        'economy.export.saft.vouchernr',
        'common.date',
        'economy.export.saft.accountno',
        'common.text',
        'economy.accounting.account',
        'common.debit',
        'common.credit',
        'economy.accounting.account.vatdeduction',
        'economy.export.saft.vatcode',
        'economy.common.paymentmethods.customernr',
        'economy.supplier.invoice.liquidityplanning.suppliernr',
        'economy.export.saft.vatamount',
        'common.sum',
        'common.name',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'voucherNr',
          terms['economy.export.saft.vouchernr'],
          {
            enableGrouping: true,
          }
        );
        this.grid.addColumnDate('date', terms['common.date'], {
          enableGrouping: true,
        });
        this.grid.addColumnText(
          'accountNr',
          terms['economy.export.saft.accountno'],
          {
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'accountName',
          terms['economy.accounting.account'],
          {
            enableGrouping: true,
          }
        );
        this.grid.addColumnText('voucherText', terms['common.text'], {
          enableGrouping: true,
        });
        this.grid.addColumnNumber('debetAmount', terms['common.debit'], {
          enableGrouping: true,
          aggFuncOnGrouping: 'sum',
        });
        this.grid.addColumnNumber('creditAmount', terms['common.credit'], {
          enableGrouping: true,
          aggFuncOnGrouping: 'sum',
        });
        this.grid.addColumnText(
          'vatRate',
          terms['economy.accounting.account.vatdeduction'],
          {
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'customerId',
          terms['economy.common.paymentmethods.customernr'],
          {
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'supplierId',
          terms['economy.supplier.invoice.liquidityplanning.suppliernr'],
          {
            enableGrouping: true,
          }
        );
        this.grid.addColumnText('supplierCustomerName', terms['common.name'], {
          enableGrouping: true,
        });
        this.grid.addColumnText(
          'vatCode',
          terms['economy.export.saft.vatcode'],
          {
            enableGrouping: true,
          }
        );
        this.grid.addColumnNumber(
          'taxAmount',
          terms['economy.export.saft.vatamount'],
          {
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
          }
        );

        this.grid.showGroupPanel();
        this.grid.groupDisplayType = 'groupRows';
        this.grid.useGrouping({
          includeFooter: true,
          includeTotalFooter: true,
          keepColumnsAfterGroup: false,
          selectChildren: false,
          groupSelectsFiltered: true,
          totalTerm: terms['common.sum'],
        });

        super.finalizeInitGrid();
      });
  }
}
