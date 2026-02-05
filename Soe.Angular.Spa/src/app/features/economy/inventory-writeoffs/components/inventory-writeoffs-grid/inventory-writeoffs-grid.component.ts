import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { VoucherEditComponent } from '@features/economy/voucher/components/voucher-edit/voucher-edit.component';
import { VoucherForm } from '@features/economy/voucher/models/voucher-form.model';
import { TranslateService } from '@ngx-translate/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  Feature,
  SoeAccountDistributionType,
  SoeEntityState,
  TermGroup_AccountDistributionTriggerType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDimSmallDTO,
  IAccountDistributionEntryDTO,
  IAccountDistributionEntryRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { IconUtil } from '@shared/util/icon-util';
import { Perform } from '@shared/util/perform.class';
import { AggregationType } from '@ui/grid/interfaces';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { RowDoubleClickedEvent } from 'ag-grid-community';
import { Observable, take, tap, concatMap, of, map } from 'rxjs';
import { AccountDistributionEditComponent } from '../../../account-distribution/components/account-distribution-edit/account-distribution-edit.component';
import { PeriodAccountDistributionForm } from '../../../account-distribution/models/account-distribution-form.model';
import { EconomyService } from '../../../services/economy.service';
import {
  AccountDistributionEntryDTO,
  AccountDistributionEntryRowDTO,
  DeleteDistributionEntryModel,
  TransferAccountDistributionEntryToVoucherDTO,
  TransferToAccountDistributionEntryDTO,
} from '../../models/inventory-writeoffs.model';
import { InventoryWriteoffsService } from '../../services/inventory-writeoffs.service';
import { InventoryNotesDialogComponent } from '../inventory-notes-dialog/inventory-notes-dialog.component';
import { InventoryNotesDialogData } from '../inventory-notes-dialog/models/inventory-notes.model';
import { AccountDistributionUrlParamsService } from '@features/economy/account-distribution/services/account-distribution-params.service';

@Component({
  selector: 'soe-inventory-writeoffs-grid',
  templateUrl: './inventory-writeoffs-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class InventoryWriteoffsGridComponent
  extends GridBaseDirective<
    AccountDistributionEntryDTO,
    InventoryWriteoffsService
  >
  implements OnInit
{
  buttonTypeofDisabled = signal(true);
  service = inject(InventoryWriteoffsService);
  private readonly translationService = inject(TranslateService);
  public flowHandler = inject(FlowHandlerService);
  public progressService = inject(ProgressService);
  public economyService = inject(EconomyService);
  public dialogService = inject(DialogService);
  public messageboxService = inject(MessageboxService);
  urlService = inject(AccountDistributionUrlParamsService);

  enableTransferToVoucher = computed(() => {
    const rows = this.selectedRows();
    return rows?.some(r => !r.voucherHeadId);
  });
  enableReverse = computed(() => {
    const rows = this.selectedRows();
    const reversableRows = rows?.filter(r => r.voucherHeadId && !r.isReversal);
    return reversableRows.length > 0;
  });
  disableTransferToVoucher = computed(() => {
    return !this.enableTransferToVoucher();
  });
  disableReverse = computed(() => {
    return !this.enableReverse();
  });
  enableWriteOffDelete = computed(() => {
    return !this.enableTransferToVoucher();
  });

  performAction = new Perform<InventoryWriteoffsService>(this.progressService);
  performGridLoad = new Perform<AccountDistributionEntryDTO[]>(
    this.progressService
  );
  selectedMonth: Date = DateUtil.getToday();

  //columns
  private accountDim1ColumnName = '';
  private accountDim2ColumnName = '';
  private accountDim3ColumnName = '';
  private accountDim4ColumnName = '';
  private accountDim5ColumnName = '';
  private accountDim6ColumnName = '';

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      this.urlService.isPeriod()
        ? Feature.Economy_Accounting_AccountDistributionEntry
        : Feature.Economy_Inventory_WriteOffs,
      this.urlService.isPeriod()
        ? 'economy.accounting.accountdistributionentry.entries'
        : 'economy.inventory.inventories.writeoff',
      { lookups: [this.getDimLabels()], useLegacyToolbar: true }
    );
  }

  getSelectedDateString(format: string = "yyyy-MM-dd'T'HH:mm:ss.000'Z'") {
    return DateUtil.format(
      new Date(
        this.selectedMonth.getFullYear(),
        this.selectedMonth.getMonth(),
        1
      ),
      format
    );
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      reloadOption: {
        onClick: () => this.refreshGrid(),
      },
    });

    if (this.urlService.isPeriod()) {
      this.toolbarUtils.createLegacyGroup({
        buttons: [
          this.toolbarUtils.createLegacyButton({
            icon: IconUtil.createIcon('fal', 'remove'),
            label: 'core.delete',
            title: 'core.delete',
            onClick: () => this.deleteSelectedEntries(),
            disabled: this.buttonTypeofDisabled,
            hidden: signal(false),
          }),
        ],
      });
    }

    const buttonTitle = this.urlService.isPeriod()
      ? 'economy.accounting.accountdistributionentry.accrual'
      : 'economy.inventory.writeoffs.transfertovoucher';
    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'calendar'),
          label: buttonTitle,
          title: buttonTitle,
          onClick: () => this.initTransferSelectedItemsToVoucher(),
          disabled: this.disableTransferToVoucher,
          hidden: signal(false),
        }),
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'arrow-right-arrow-left'),
          label: 'economy.inventory.writeoffs.reverse',
          title: 'economy.inventory.writeoffs.reverseinfo',
          onClick: () => this.initReverseSelectedItems(),
          disabled: this.disableReverse,
          hidden: signal(this.urlService.isPeriod()),
        }),
      ],
    });

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'upload'),
          label: 'economy.accounting.accountdistributionentry.getdetails',
          title: 'economy.accounting.accountdistributionentry.getdetails',
          onClick: () => this.uploadFiles(),
          disabled: signal(false),
          hidden: signal(false),
        }),
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'info-square'),
          label: '',
          title: '',
          onClick: () =>
            this.messageboxService.information(
              'core.info',
              this.urlService.isWriteOff()
                ? 'economy.accounting.inventory.writeoffs.getdetails.information'
                : 'economy.accounting.accountdistributionentry.getdetails.information'
            ),
          disabled: signal(false),
          hidden: signal(false),
        }),
      ],
    });

    if (this.urlService.isWriteOff()) {
      this.toolbarUtils.createLegacyGroup({
        buttons: [
          this.toolbarUtils.createLegacyButton({
            icon: IconUtil.createIcon('fal', 'remove'),
            label: 'core.delete',
            title: 'core.delete',
            onClick: () => this.deleteSelectedEntries(),
            disabled: this.enableWriteOffDelete,
            hidden: signal(false),
          }),
        ],
      });
    }
  }

  deleteSelectedEntries(ignoreDeleteWarning: boolean = false) {
    if (this.grid.selectedRowsCount() > 0) {
      const rows: IAccountDistributionEntryDTO[] = [];

      let warning = false;
      this.grid.getSelectedRows().forEach(row => {
        if (
          row.triggerType !=
          TermGroup_AccountDistributionTriggerType.Distribution
        ) {
          warning = true;
        }

        if (row.voucherHeadId == null && row.state === SoeEntityState.Active)
          rows.push(row);
      });

      if (rows.length > 0) {
        if (warning && !ignoreDeleteWarning) {
          this.messageboxService
            .warning(
              'core.warning',
              'economy.inventory.writeoffs.deletewarning'
            )
            .afterClosed()
            .subscribe(res => {
              if (res.result) {
                this.deleteSelectedEntries(res.result);
              }
              return;
            });
        }

        if (!warning || ignoreDeleteWarning) {
          const model = new DeleteDistributionEntryModel();
          model.accountDistributionEntryDTOs = rows;
          model.accountDistributionType = this.urlService.typeId();
          this.performAction.crud(
            CrudActionTypeEnum.Delete,
            this.service.delete(model).pipe(
              tap(() => {
                this.refreshGrid();
              })
            ),
            undefined,
            undefined,
            {}
          );
        }
      } else this.showWarningNoneSelected();
    } else this.showWarningNoneSelected();
  }

  showWarningNoneSelected() {
    this.messageboxService.warning(
      'core.info',
      this.urlService.isWriteOff()
        ? 'economy.inventory.writeoffs.noentriesselected'
        : 'economy.accounting.accountdistributionentry.noentriesselected'
    );
  }

  uploadFiles() {
    this.buttonTypeofDisabled.set(true);

    const model = new TransferToAccountDistributionEntryDTO();
    model.periodDate = this.getSelectedDateString(`yyyy-MM-dd'T'HH:mm:ss.000`);
    model.accountDistributionType = this.urlService.typeId();

    this.loadSearch(model);
  }

  loadSearch(model: TransferToAccountDistributionEntryDTO) {
    this.performGridLoad.load(
      this.service.transferToAccountDistributionEntry(model).pipe(
        tap((result: any) => {
          if (result.success) {
            this.refreshGrid();
          } else {
            let message: string = result.errorMessage;
            if (result.errorMessage == 'AccountYear')
              message = this.translate.instant(
                'economy.accounting.accountdistributionentry.accountyearmissingmessage'
              );
            else if (result.errorMessage == 'AccountPeriod')
              message = this.translate.instant(
                'economy.accounting.accountdistributionentry.accountperiodclosed'
              );

            this.messageboxService.error(
              this.translate.instant('core.error'),
              message
            );
          }
        })
      )
    );
  }

  initTransferSelectedItemsToVoucher() {
    const countOfUnVoucheredRows = this.selectedRows().filter(
      r => !r.voucherHeadId
    ).length;

    if (countOfUnVoucheredRows == 0) return;

    const validRows = this.grid.getSelectedRows().filter(x => {
      return (
        x.voucherHeadId == null &&
        x.state === SoeEntityState.Active &&
        !x.periodError
      );
    });
    const invalidCount = countOfUnVoucheredRows - validRows.length;

    if (validRows.length > 0 && invalidCount == 0) {
      this.createTypeOf(validRows);
      return;
    }

    if (this.urlService.isWriteOff() && invalidCount > 0) {
      const warning =
        invalidCount +
        ' ' +
        this.translationService.instant(
          'economy.inventory.writeoffs.perioderrorinfomulti'
        );
      this.messageboxService
        .warning(
          'core.warning',
          invalidCount == 1
            ? 'economy.inventory.writeoffs.perioderrorinfosingle'
            : warning
        )
        .afterClosed()
        .subscribe(() => {
          if (validRows.length > 0) this.createTypeOf(validRows);
        });
    } else if (!this.urlService.isWriteOff() && validRows.length > 0) {
      this.createTypeOf(validRows);
    } else {
      this.messageboxService.information(
        'core.info',
        'economy.accounting.accountdistributionentry.nowrowstotransfer'
      );
    }
  }

  initReverseSelectedItems() {
    const dialog = this.messageboxService.question(
      this.terms['core.info'],
      this.terms['common.question.cannotbeundone']
    );
    dialog.afterClosed().subscribe((proceed: IMessageboxComponentResponse) => {
      if (!proceed.result) return;

      this.performAction.crud(
        CrudActionTypeEnum.Save,
        this.service.reverseAccountDistributionEntries({
          accountDistributionEntryDTOs: this.selectedRows(),
          accountDistributionType:
            SoeAccountDistributionType.Inventory_WriteOff,
        }),
        () => this.refreshGrid()
      );
    });
  }

  createTypeOf(rows: AccountDistributionEntryDTO[]) {
    const model = new TransferAccountDistributionEntryToVoucherDTO();
    model.accountDistributionEntryDTOs = rows;
    model.periodDate = new Date(this.selectedMonth);
    model.accountDistributionType = this.urlService.typeId();

    const save$ = this.service.transferAccountDistributionEntryToVoucher(model).pipe(
      concatMap(resp => {
        if (!resp.success)
          return of(resp);

        return this.refreshGrid$().pipe(
          map(() => resp)
        );
      })
    );

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      save$,
      undefined,
      undefined,
      {}
    );
  }

  override loadData(
    id?: number | undefined
  ): Observable<AccountDistributionEntryDTO[]> {
    return this.performGridLoad.load$(
      this.service
        .getGrid(undefined, {
          periodDate: this.getSelectedDateString(`yyyyMMdd'T'HHmmss`),
          accountDistributionType: this.urlService.typeId(),
          onlyActive: true,
        })
        .pipe(
          tap((data: any[]) => {
            data.forEach(item => {
              this.service.setInventoryName(item);
              this.service.setNotesIcon(item);
              this.service.setSequenceNumber(
                item,
                this.terms['economy.supplier.invoice.preliminary']
              );
            });
            return data;
          })
        )
    );
  }

  private getDimLabels() {
    return this.economyService
      .getAccountDimsSmall(
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
      )
      .pipe(
        tap((accountDims: IAccountDimSmallDTO[]) => {
          const dim1 = accountDims.find(d => d.accountDimNr == 1);
          if (dim1) this.accountDim1ColumnName = dim1.name;

          const dim2 = accountDims.find(d => d.accountDimNr == 2);
          if (dim2) this.accountDim2ColumnName = dim2.name;

          const dim3 = accountDims.find(d => d.accountDimNr == 3);
          if (dim3) this.accountDim3ColumnName = dim3.name;

          const dim4 = accountDims.find(d => d.accountDimNr == 4);
          if (dim4) this.accountDim4ColumnName = dim4.name;

          const dim5 = accountDims.find(d => d.accountDimNr == 5);
          if (dim5) this.accountDim5ColumnName = dim5.name;

          const dim6 = accountDims.find(d => d.accountDimNr == 6);
          if (dim6) this.accountDim6ColumnName = dim6.name;
        })
      );
  }

  onGridReadyToDefine(grid: GridComponent<AccountDistributionEntryDTO>) {
    super.onGridReadyToDefine(grid);

    this.translationService
      .get([
        'common.rownumber',
        'common.description',
        'common.note',
        'common.name',
        'economy.inventory.writeoffs.date',
        'common.type',
        'economy.accounting.accountdistributionentry.status',
        'economy.inventory.writeoffs.periodamount',
        'economy.accounting.voucher.voucher',
        'economy.accounting.accountdistributionentry.accounting',
        'economy.accounting.accountdistributionentry.debet',
        'economy.accounting.accountdistributionentry.credit',
        'economy.accounting.vatverification.vouchernumber',
        'economy.inventory.inventories.purchasedate',
        'economy.inventory.writeoffs.inventoryname',
        'economy.inventory.writeoffs.writeoffamount',
        'economy.inventory.writeoffs.writeoffyearamount',
        'economy.inventory.writeoffs.writeofftotalamount',
        'economy.inventory.inventories.writeoffsum',
        'economy.inventory.writeoffs.currentamount',
        'economy.accounting.accountdistributionentry.sourcetype',
        'economy.accounting.accountdistributionentry.source',
        'common.categories',
        'common.supplierinvoice',
        'common.customerinvoice',
        'core.warning',
        'core.info',
        'core.edit',
        'economy.accounting.accountdistributionentry.deletepermantentlywarning',
        'economy.accounting.accountdistributionentry.noentriesselected',
        'economy.supplier.invoice.preliminary',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'core.aggrid.totals.selected',
        'economy.accounting.accountdistributionentry.nowrowstodelete',
        'economy.accounting.accountdistributionentry.nowrowstotransfer',
        'economy.inventory.inventories.writeoff',
        'economy.accounting.accountdistributionentry.accountyearmissingmessage',
        'economy.accounting.accountdistributionentry.accountperiodclosed',
        'economy.inventory.writeoffs.perioderror',
        'economy.inventory.writeoffs.perioderrorinfosingle',
        'economy.inventory.writeoffs.perioderrorinfomulti',
        'economy.inventory.inventories.writeoffdate',
        'economy.accounting.accountdistributionentry.periodamount',
        'common.question.cannotbeundone',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        //Details
        this.grid.enableMasterDetail(
          {
            detailRowHeight: 120,
            floatingFiltersHeight: 0,

            columnDefs: [
              ColumnUtil.createColumnRowSelection(),
              ColumnUtil.createColumnText(
                'dim1NrName',
                this.accountDim1ColumnName,
                {
                  flex: 1,
                  suppressFilter: true,
                }
              ),
              ColumnUtil.createColumnText(
                'dim2NrName',
                this.accountDim2ColumnName,
                {
                  flex: 1,
                  suppressFilter: true,
                }
              ),
              ColumnUtil.createColumnText(
                'dim3NrName',
                this.accountDim3ColumnName,
                {
                  flex: 1,
                  suppressFilter: true,
                }
              ),
              ColumnUtil.createColumnText(
                'dim4NrName',
                this.accountDim4ColumnName,
                {
                  flex: 1,
                  suppressFilter: true,
                }
              ),
              ColumnUtil.createColumnText(
                'dim5NrName',
                this.accountDim5ColumnName,
                {
                  flex: 1,
                  suppressFilter: true,
                }
              ),
              ColumnUtil.createColumnText(
                'dim6NrName',
                this.accountDim6ColumnName,
                {
                  flex: 1,
                  suppressFilter: true,
                }
              ),
              ColumnUtil.createColumnNumber(
                'sameBalance',
                terms['economy.accounting.accountdistributionentry.debet'],
                {
                  flex: 1,
                  suppressFilter: true,
                  decimals: 2,
                }
              ),
              ColumnUtil.createColumnNumber(
                'oppositeBalance',
                terms['economy.accounting.accountdistributionentry.credit'],
                {
                  flex: 1,
                  suppressFilter: true,
                  decimals: 2,
                }
              ),
            ],
          },
          {
            getDetailRowData: (params: any) => {
              this.loadDetailRows(params);
            },
          }
        );

        // Master
        this.grid.enableRowSelection();
        this.grid.onRowDoubleClicked =
          this.doubleClickAccountDistributionEntry.bind(this);
        if (this.urlService.isPeriod()) {
          this.grid.addColumnNumber('rowId', terms['common.rownumber'], {
            flex: 1,
          });
          this.grid.addColumnText(
            'accountDistributionHeadName',
            terms['common.name'],
            {
              flex: 1,
              enableHiding: true,
              buttonConfiguration: {
                iconPrefix: 'fal',
                iconName: 'pen',
                show: () => true,
                onClick: r => this.editAccountDistributionEntry(r),
              },
            }
          );
          this.grid.addColumnDate(
            'date',
            terms['economy.inventory.writeoffs.date'],
            { flex: 1 }
          );
          this.grid.addColumnText(
            'typeName',
            terms['economy.accounting.accountdistributionentry.sourcetype'],
            { flex: 1 }
          );
          this.grid.addColumnText(
            'sourceSeqNr',
            terms['economy.accounting.accountdistributionentry.source'],
            { flex: 1 }
          );
          this.grid.addColumnIcon('', '', {
            iconName: 'pen',
            iconClass: 'pen',
            tooltip: terms['core.edit'],
            onClick: row => {
              this.handleEditSource(row);
            },
            showIcon: row => {
              return this.showEditSource(row);
            },
            flex: 1,
          });
          this.grid.addColumnText(
            'status',
            terms['economy.accounting.accountdistributionentry.status'],
            { flex: 1 }
          );
          this.grid.addColumnNumber(
            'amount',
            terms['economy.accounting.accountdistributionentry.periodamount'],
            {
              decimals: 2,
              flex: 1,
            }
          );
          this.grid.addColumnNumber(
            'voucherNr',
            terms['economy.accounting.voucher.voucher'],
            { flex: 1 }
          );
          this.grid.addColumnIcon('', '', {
            flex: 1,
            iconName: 'pen',
            iconClass: 'pen',
            tooltip: terms['core.edit'],
            onClick: row => this.openVoucher(row),
            showIcon: row => {
              return this.showEditIcon(row);
            },
          });
        } else {
          this.grid.addColumnNumber('rowId', terms['common.rownumber'], {
            enableHiding: false,
            minWidth: 80,
          });
          this.grid.addColumnDate(
            'inventoryPurchaseDate',
            terms['economy.inventory.inventories.purchasedate'],
            { enableHiding: true, hide: true }
          );
          this.grid.addColumnDate(
            'inventoryWriteOffDate',
            terms['economy.inventory.inventories.writeoffdate'],
            { enableHiding: true, hide: true }
          );
          this.grid.addColumnText(
            'inventoryName',
            terms['economy.inventory.writeoffs.inventoryname'],
            { enableHiding: false }
          );
          this.grid.addColumnText(
            'inventoryDescription',
            terms['common.description'],
            {
              enableHiding: true,
              hide: true,
            }
          );
          this.grid.addColumnIcon('notesIcon', terms['common.note'], {
            flex: 1,
            tooltipField: 'inventoryNotes',
            onClick: row => this.editNotes(row),
            enableHiding: true,
            enableResize: true,
            suppressFilter: false,
            minWidth: 50,
            showIcon: () => {
              return true;
            },
            hide: false,
          });
          this.grid.addColumnDate(
            'date',
            terms['economy.inventory.writeoffs.date'],
            { enableHiding: false }
          );
          this.grid.addColumnText('typeName', terms['common.type'], {
            enableHiding: false,
          });
          this.grid.addColumnText('categories', terms['common.categories'], {
            enableHiding: true,
          });
          this.grid.addColumnText(
            'status',
            terms['economy.accounting.accountdistributionentry.status'],
            { enableHiding: false }
          );
          this.grid.addColumnNumber(
            'amount',
            terms['economy.inventory.writeoffs.periodamount'],
            { enableHiding: false, decimals: 2 }
          );
          this.grid.addColumnNumber(
            'writeOffAmount',
            terms['economy.inventory.writeoffs.writeoffamount'],
            { enableHiding: true, decimals: 2 }
          );
          this.grid.addColumnNumber(
            'writeOffYear',
            terms['economy.inventory.writeoffs.writeoffyearamount'],
            { enableHiding: true, decimals: 2 }
          );
          this.grid.addColumnNumber(
            'writeOffTotal',
            terms['economy.inventory.writeoffs.writeofftotalamount'],
            { enableHiding: true, decimals: 2 }
          );
          this.grid.addColumnNumber(
            'writeOffSum',
            terms['economy.inventory.inventories.writeoffsum'],
            { enableHiding: true, decimals: 2 }
          );
          this.grid.addColumnNumber(
            'currentAmount',
            terms['economy.inventory.writeoffs.currentamount'],
            { enableHiding: true, decimals: 2 }
          );
          this.grid.addColumnNumber(
            'voucherNr',
            terms['economy.accounting.voucher.voucher'],
            { clearZero: true, enableHiding: false }
          );
          this.grid.addColumnIcon('', '', {
            iconName: 'pen',
            iconClass: 'pen',
            tooltip: terms['core.edit'],
            onClick: row => this.openVoucher(row),
            enableHiding: false,
            showIcon: row => {
              return this.showEditIcon(row);
            },
          });
          this.grid.addColumnIcon('', '', {
            enableHiding: false,
            iconName: 'exclamation-triangle',
            iconClass: 'warningColor',
            showIcon: row => row.periodError,
            tooltip: terms['economy.inventory.writeoffs.perioderror'],
          });
        }

        this.grid.addAggregationsRow({
          amount: AggregationType.Sum,
          writeOffAmount: AggregationType.Sum,
          writeOffYear: AggregationType.Sum,
          writeOffTotal: AggregationType.Sum,
          currentAmount: AggregationType.Sum,
        });

        super.finalizeInitGrid();
      });
  }

  private handleEditSource(row: AccountDistributionEntryDTO) {
    if (row.sourceVoucherHeadId) this.showSourceVoucher(row);
    else if (row.sourceSupplierInvoiceId || row.supplierInvoiceId)
      this.showSourceSupplierInvoice(row);
    else if (row.sourceCustomerInvoiceId && row.sourceCustomerInvoiceId != null)
      this.showSourceCustomerInvoice(row);
  }
  private showSourceSupplierInvoice(row: AccountDistributionEntryDTO) {
    BrowserUtil.openInNewWindow(
      window,
      `/soe/economy/supplier/invoice/status/?invoiceId=${row.sourceSupplierInvoiceId}&invoiceNr=${row.sourceSupplierInvoiceSeqNr}`
    );
  }
  private showSourceCustomerInvoice(row: AccountDistributionEntryDTO) {
    BrowserUtil.openInNewWindow(
      window,
      `/soe/economy/customer/invoice/status/?invoiceId=${row.sourceCustomerInvoiceId}&invoiceNr=${row.sourceCustomerInvoiceSeqNr}`
    );
  }

  private showSourceVoucher(row: AccountDistributionEntryDTO) {
    row = {
      ...row,
      voucherHeadId: row.sourceVoucherHeadId,
      voucherNr: row.sourceVoucherNr,
    };
    this.openVoucher(row);
  }

  doubleClickAccountDistributionEntry(
    event: RowDoubleClickedEvent<AccountDistributionEntryDTO, any>
  ) {
    const row = { ...(event.data as AccountDistributionEntryDTO) };
    this.editAccountDistributionEntry(row);
  }
  editAccountDistributionEntry(row: AccountDistributionEntryDTO) {
    if (row.accountDistributionHeadId && row.state === SoeEntityState.Active)
      this.edit(
        {
          ...row,
        },
        {
          editComponent: AccountDistributionEditComponent,
          editTabLabel:
            'economy.accounting.accountdistribution.accountdistribution',
          FormClass: PeriodAccountDistributionForm,
        }
      );
  }

  openVoucher(row: AccountDistributionEntryDTO) {
    if (
      row.voucherHeadId &&
      row.voucherHeadId != null &&
      row.state === SoeEntityState.Active
    )
      this.edit(
        {
          ...row,
          inventoryId: row.inventoryId,
        },
        {
          editComponent: VoucherEditComponent,
          editTabLabel: 'economy.accounting.voucher.voucher',
          FormClass: VoucherForm,
        }
      );
  }

  editNotes(row: AccountDistributionEntryDTO) {
    this.dialogService
      .open(
        InventoryNotesDialogComponent,
        new InventoryNotesDialogData(row, this.grid.getAllRows())
      )
      .afterClosed()
      .subscribe(value => {
        if (value && value.length) this.rowData.next(value);
      });
  }

  loadDetailRows(params: any) {
    if (params.data.accountDistributionEntryRowDTO.length == 0) {
      params.successCallback([]);
      return;
    }
    const data = params.data.accountDistributionEntryRowDTO.map(
      (row: IAccountDistributionEntryRowDTO) =>
        new AccountDistributionEntryRowDTO(row)
    );
    params.successCallback(data);
  }

  private showEditSource(row: AccountDistributionEntryDTO) {
    if (row.sourceSeqNr) return true;
    return false;
  }

  showEditIcon(row: AccountDistributionEntryDTO) {
    if (
      row.state === SoeEntityState.Deleted ||
      (row.voucherHeadId &&
        row.voucherHeadId != null &&
        row.state === SoeEntityState.Active)
    )
      return true;

    return false;
  }

  triggerSelectedItem(rows: AccountDistributionEntryDTO[]) {
    this.selectionChanged(rows);
    this.buttonTypeofDisabled.set(rows.length > 0 ? false : true);
  }

  doFilterByDate(dateValue: Date) {
    this.selectedMonth = dateValue;
    this.refreshGrid();
  }
}
