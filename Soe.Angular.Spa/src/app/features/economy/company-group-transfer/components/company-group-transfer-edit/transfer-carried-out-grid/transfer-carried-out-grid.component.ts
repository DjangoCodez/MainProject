import {
  Component,
  EventEmitter,
  inject,
  input,
  OnInit,
  output,
  Output,
} from '@angular/core';
import { AccountDistributionEntryDTO } from '@features/economy/inventory-writeoffs/models/inventory-writeoffs.model';
import { VoucherEditComponent } from '@features/economy/voucher/components/voucher-edit/voucher-edit.component';
import { VoucherForm } from '@features/economy/voucher/models/voucher-form.model';
import { VoucherService } from '@features/economy/voucher/services/voucher.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  CompanyGroupTransferStatus,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { ICompanyGroupTransferRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { OpenEditInNewTab } from '@ui/tab/models/multi-tab-wrapper.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IIconButtonConfiguration } from '@ui/grid/interfaces';
import { DetailGridInfo } from 'ag-grid-community';
import { take } from 'rxjs';
import { CompanyGroupTransferHeadDTO } from '../../../models/company-group-transfer.model';
import { CompanyGroupTransferService } from '../../../services/company-group-transfer.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
@Component({
  selector: 'soe-transfer-carried-out-grid',
  templateUrl: './transfer-carried-out-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TransferCarriedOutGridComponent
  extends GridBaseDirective<
    CompanyGroupTransferHeadDTO,
    CompanyGroupTransferService
  >
  implements OnInit
{
  @Output() deleteCompleted = new EventEmitter<
    ICompanyGroupTransferRowDTO | CompanyGroupTransferHeadDTO
  >();
  openEditInNewTab = output<OpenEditInNewTab>();
  isConsolidation = input(true);

  service = inject(CompanyGroupTransferService);
  voucherService = inject(VoucherService);
  progressService = inject(ProgressService);
  messageboxService = inject(MessageboxService);
  performSaveData = new Perform<BackendResponse>(this.progressService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_CompanyGroup_Transfers,
      'Economy.Accounting.Transfers.CarriedOut',
      { skipInitialLoad: true }
    );
  }

  onGridReadyToDefine(grid: GridComponent<CompanyGroupTransferHeadDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'economy.accounting.companygroup.companygroupvoucher',
        'common.reports.drilldown.budget',
        'common.reports.drilldown.periodamount',
        'economy.accounting.companygroup.completedtransfers',
        'common.date',
        'common.missingrequired',
        'core.warning',
        'economy.accounting.accountyear',
        'economy.accounting.voucherseriestype',
        'economy.accounting.companygroup.periodfrom',
        'economy.accounting.companygroup.periodto',
        'economy.accounting.companygroup.companyfrom',
        'economy.accounting.companygroup.companyto',
        'economy.accounting.companygroup.companygroupbudget',
        'economy.accounting.companygroup.budgetname',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'economy.accounting.companygroup.transfererror',
        'economy.accounting.companygroup.accountyear',
        'economy.accounting.companygroup.periodfrom',
        'economy.accounting.companygroup.periodto',
        'economy.accounting.companygroup.voucherserie',
        'economy.accounting.companygroup.transfertype',
        'economy.accounting.companygroup.created',
        'common.name',
        'common.status',
        'core.delete',
        'common.company',
        'common.period',
        'common.reports.drilldown.vouchernr',
        'common.text',
        'economy.accounting.companygroup.conversionrate',
        'economy.accounting.companygroup.transfered',
        'economy.accounting.companygroup.childbudget',
        'economy.accounting.companygroup.masterbudget',
        'economy.accounting.balance.balance',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableMasterDetail(
          {
            detailRowHeight: 50,

            columnDefs: [
              ColumnUtil.createColumnNumber(
                'voucherNr',
                terms['common.reports.drilldown.vouchernr'],
                {
                  flex: 1,
                  maxWidth: 85,
                  pinned: 'left',
                  clearZero: true,
                  allowEmpty: true,
                  alignLeft: true,
                  hide: !this.isConsolidation(),
                  buttonConfiguration: <
                    IIconButtonConfiguration<ICompanyGroupTransferRowDTO>
                  >{
                    iconPrefix: 'fal',
                    iconName: 'pen',
                    onClick: row => this.openVoucher(row),
                    show: row => this.isVoucherDeletable(row),
                  },
                }
              ),
              ColumnUtil.createColumnText('budgetName', terms['common.name'], {
                flex: 1,
                maxWidth: 175,
                pinned: 'left',
                hide: this.isConsolidation(),
              }),
              ColumnUtil.createColumnText(
                'childActorCompanyNrName',
                terms['common.company'],
                {
                  flex: 1,
                }
              ),
              ColumnUtil.createColumnText(
                'accountPeriodText',
                terms['common.period'],
                {
                  flex: 1,
                  hide: !this.isConsolidation(),
                }
              ),
              ColumnUtil.createColumnText('status', terms['common.status'], {
                flex: 1,
              }),
              ColumnUtil.createColumnText('voucherText', terms['common.text'], {
                flex: 1,
                hide: !this.isConsolidation(),
              }),
              ColumnUtil.createColumnText(
                'voucherSeriesName',
                terms['economy.accounting.companygroup.voucherserie'],
                {
                  flex: 1,
                  hide: !this.isConsolidation(),
                }
              ),
              ColumnUtil.createColumnNumber(
                'conversionFactor',
                terms['economy.accounting.companygroup.conversionrate'],
                {
                  flex: 1,
                  decimals: 4,
                }
              ),
              ColumnUtil.createColumnDateTime(
                'created',
                terms['economy.accounting.companygroup.transfered'],
                {
                  flex: 1,
                }
              ),
              ColumnUtil.createColumnIconDelete({
                onClick: (row: ICompanyGroupTransferRowDTO) =>
                  this.deleteVoucherRow(row),
                showIcon: r => this.isVoucherDeletable(r),
              }),
            ],
          },
          {
            autoHeight: false,
            getDetailRowData: (params: any) => {
              params.successCallback(params.data.companyGroupTransferRows);

              this.grid.api.forEachDetailGridInfo(
                (gridInfo: DetailGridInfo) => {
                  gridInfo?.api?.setColumnsVisible(
                    [
                      'voucherNr',
                      'voucherText',
                      'voucherSeriesName',
                      'accountPeriodText',
                    ],
                    this.isConsolidation()
                  );
                  gridInfo?.api?.setColumnsVisible(
                    ['budgetName'],
                    !this.isConsolidation()
                  );
                }
              );
            },
          }
        );

        this.grid.addColumnText(
          'accountYearText',
          terms['economy.accounting.companygroup.accountyear'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'fromAccountPeriodText',
          terms['economy.accounting.companygroup.periodfrom'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'toAccountPeriodText',
          terms['economy.accounting.companygroup.periodto'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'transferTypeName',
          terms['economy.accounting.companygroup.transfertype'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText('transferStatusName', terms['common.status'], {
          flex: 1,
        });
        this.grid.addColumnDateTime(
          'transferDate',
          terms['economy.accounting.companygroup.created'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnIconDelete({
          onClick: r => this.deleteTransferRow(r),
          showIcon: r => this.isTransferDeletable(r),
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid();
      });
  }

  private isTransferDeletable(row: CompanyGroupTransferHeadDTO): boolean {
    return row.transferStatus === CompanyGroupTransferStatus.Transfered;
  }

  public isVoucherDeletable(row: ICompanyGroupTransferRowDTO) {
    return !!row.voucherHeadId && row.voucherHeadId > 0;
  }

  deleteTransferRow(row: CompanyGroupTransferHeadDTO) {
    const mb = this.messageboxService.warning(
      'core.warning',
      this.translate.instant('economy.accounting.companygroup.deletetransfer')
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.result) {
        this.grid?.deleteRow(row);
        this.performSaveData.crud(
          CrudActionTypeEnum.Save,
          this.service.delete(row.companyGroupTransferHeadId || 0),
          () => {
            this.deleteCompleted.emit(row);
          }
        );
      }
    });
  }

  deleteVoucherRow(row: ICompanyGroupTransferRowDTO) {
    const mb = this.messageboxService.warning(
      'core.warning',
      this.translate.instant('economy.accounting.companygroup.deletevoucher')
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.result) {
        this.performSaveData.crud(
          CrudActionTypeEnum.Save,
          this.voucherService.delete(row.voucherHeadId || 0, true),
          (result: any) => {
            if (result.success) {
              this.deleteCompleted.emit(row);
            }
          }
        );
      }
    });
  }

  openVoucher(row: AccountDistributionEntryDTO) {
    if (row.voucherHeadId && row.voucherHeadId != null)
      this.openEditInNewTab.emit({
        id: row.voucherHeadId,
        additionalProps: {
          editComponent: VoucherEditComponent,
          editTabLabel: 'economy.accounting.voucher.voucher',
          FormClass: VoucherForm,
        },
      });
  }
}
