import { Component, inject, OnInit, signal } from '@angular/core';
import { VoucherSeriesDTO } from '@features/economy/account-years-and-periods/models/account-years-and-periods.model';
import { EconomyService } from '@features/economy/services/economy.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
  SettingMainType,
  SoeReportTemplateType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDimSmallDTO,
  IAccountYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IVoucherGridDTO } from '@shared/models/generated-interfaces/VoucherHeadDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { GridResizeType } from '@ui/grid/enums/resize-type.enum';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  BehaviorSubject,
  forkJoin,
  map,
  Observable,
  of,
  take,
  tap,
} from 'rxjs';
import { VoucherForm } from '../../models/voucher-form.model';
import {
  SaveUserCompanySettingModel,
  VoucherGridDTO,
  VoucherGridFilterDTO,
  VoucherHeadDTO,
  VoucherRowDTO,
} from '../../models/voucher.model';
import { VoucherService } from '../../services/voucher.service';
import { RequestReportService } from '@shared/services/request-report.service';
import { ColDef, RowDoubleClickedEvent } from 'ag-grid-community';
import { VoucherParamsService } from '../../services/voucher-params.service';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-voucher-grid',
  templateUrl: './voucher-grid.component.html',
  styleUrl: './voucher-grid.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class VoucherGridComponent
  extends GridBaseDirective<VoucherGridDTO, VoucherService>
  implements OnInit
{
  service = inject(VoucherService);
  coreService = inject(CoreService);
  economyService = inject(EconomyService);
  private readonly requestReportService = inject(RequestReportService);
  progressService = inject(ProgressService);
  validationHandler = inject(ValidationHandler);
  urlService = inject(VoucherParamsService);
  ayService = inject(PersistedAccountingYearService);

  selectedVoucherSeriesType: number = 0;
  accountingOrderReportId: number = 0;

  performLoad = new Perform<any>(this.progressService);
  performDeleteAction = new Perform<VoucherService>(this.progressService);
  voucherSeriesTypesDict: SmallGenericType[] = [];
  voucherSeriesTypes: VoucherSeriesDTO[] = [];
  voucherSeries: VoucherSeriesDTO[] = [];
  accountDims: IAccountDimSmallDTO[] = [];
  accountYears: IAccountYearDTO[] = [];

  private accountYearId: number | undefined = undefined;
  private hasReadPermission = false;
  private filterIsReady = false;

  private toolbarPrintDisabled = signal(true);
  private toolbarDeleteDisabled = signal(true);

  form: VoucherForm = new VoucherForm({
    validationHandler: this.validationHandler,
    element: new VoucherHeadDTO(),
  });

  filter!: VoucherGridFilterDTO;

  ngOnInit(): void {
    super.ngOnInit();

    this.service.setIsTemplateSubject(this.urlService.isTemplate());

    this.startFlow(
      Feature.Economy_Accounting_Vouchers_Edit,
      'Economy.Accounting.Vouchers1',
      {
        lookups: [
          this.loadAccountingOrderReportId(),
          this.loadAccountDims(),
          this.loadAccountYears(),
          this.loadAccountYearDependentData(),
        ],
      }
    );
  }

  override loadTerms() {
    return super
      .loadTerms([
        'economy.accounting.voucher.vouchermodified',
        'core.attachments',
        'economy.accounting.voucher.missingrows',
        'economy.accounting.voucher.unbalancedrowswarning',
      ])
      .pipe(
        tap(terms => {
          this.terms = terms;
          this.service.setTerms(terms);
        })
      );
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.hasReadPermission = this.flowHandler.readPermission();
  }

  override createGridToolbar(): void {
    super.createGridToolbar();
    if (!this.urlService.isTemplate()) {
      //print
      this.toolbarService.createItemGroup({
        items: [
          this.toolbarService.createToolbarButton('print', {
            iconName: signal('print'),
            tooltip: signal('economy.accounting.voucher.printselectedvouchers'),
            disabled: this.toolbarPrintDisabled,
            onAction: () => this.printSelectedVouchers(),
          }),
        ],
      });
    }

    if (SoeConfigUtil.isSupportSuperAdmin) {
      //delete
      this.toolbarService.createItemGroup({
        items: [
          this.toolbarService.createToolbarButton('delete', {
            iconName: signal('times'),
            tooltip: signal(
              'economy.accounting.voucher.deleteselectedvouchers'
            ),
            disabled: this.toolbarDeleteDisabled,
            onAction: () => this.initDeleteSelectedVouchers(),
          }),
        ],
      });
    }
  }

  selectionChanged(data: IVoucherGridDTO[]) {
    this.toolbarDeleteDisabled.set(data.length === 0);
    this.toolbarPrintDisabled.set(data.length === 0);
  }

  override onGridReadyToDefine(grid: GridComponent<VoucherGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.grid.enableRowSelection();
    this.translate
      .get([
        'common.accountingrows.rownr',
        'common.text',
        'common.debit',
        'common.credit',
        'common.number',
        'common.date',
        'common.text',
        'economy.accounting.voucher.voucherseries',
        'economy.accounting.voucher.vatvoucher',
        'economy.accounting.voucher.sourcetype',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        if (this.hasReadPermission) {
          this.grid.onRowDoubleClicked = (
            event: RowDoubleClickedEvent<VoucherGridDTO, any>
          ) => {
            this.edit(event.data!);
          };
        }
        const columns: ColDef[] = [];

        columns.push(
          ColumnUtil.createColumnText(
            'rowNr',
            terms['common.accountingrows.rownr'],
            {
              flex: 1,
              enableHiding: true,
            }
          )
        );

        this.accountDims.forEach((ad, i) => {
          columns.push(
            ColumnUtil.createColumnText('dim' + (i + 1) + 'Name', ad.name, {
              flex: 1,
              enableHiding: true,
            })
          );
        });

        columns.push(
          ColumnUtil.createColumnText('text', terms['common.text'], {
            flex: 1,
            enableHiding: true,
          })
        );
        columns.push(
          ColumnUtil.createColumnNumber('amountDebet', terms['common.debit'], {
            flex: 1,
            enableHiding: false,
            decimals: 2,
            maxWidth: 80,
            minWidth: 80,
          })
        );
        columns.push(
          ColumnUtil.createColumnNumber(
            'amountCredit',
            terms['common.credit'],
            {
              flex: 1,
              enableHiding: false,
              decimals: 2,
              maxWidth: 80,
              minWidth: 80,
            }
          )
        );
        if (!this.urlService.isTemplate()) {
          this.grid.enableMasterDetail(
            {
              columnDefs: columns,
            },
            {
              autoHeight: false,
              getDetailRowData: (params: VoucherRowDTO) => {
                this.loadDetailRows(params).subscribe();
              },
            }
          );
        }
        this.grid.addColumnText('voucherNr', terms['common.number'], {
          flex: 2,
        });
        this.grid.addColumnDate('date', terms['common.date'], { flex: 4 });
        this.grid.addColumnText('text', terms['common.text'], {
          flex: 12,
        });

        this.grid.addColumnSelect(
          'voucherSeriesTypeId',
          terms['economy.accounting.voucher.voucherseries'],
          this.urlService.isTemplate()
            ? this.voucherSeries
            : this.voucherSeriesTypes,
          () => {},
          {
            flex: 4,
            dropDownIdLabel: 'voucherSeriesTypeId',
            dropDownValueLabel: 'voucherSeriesTypeName',
          }
        );
        this.grid.addColumnBool(
          'vatVoucher',
          terms['economy.accounting.voucher.vatvoucher'],
          {
            flex: 2,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'sourceTypeName',
          terms['economy.accounting.voucher.sourcetype'],
          {
            flex: 1,
            enableHiding: true,
          }
        );

        this.grid.addColumnIcon('hasDocumentsIconValue', '', {
          showIcon: () => true,
          showTooltipInFilter: true,
          suppressFilter: false,
          headerSeparator: true,
          useIconFromField: true,
          tooltipField: 'hasDocumentsTooltip',
          iconClassField: 'hasDocumentsIconClass',
          enableHiding: false,
        });
        this.grid.addColumnIcon('accRowsIconValue', '', {
          showIcon: () => true,
          showTooltipInFilter: true,
          suppressFilter: false,
          headerSeparator: true,
          useIconFromField: true,
          tooltipField: 'accRowsText',
          iconClassField: 'accRowsIconClass',
          enableHiding: false,
        });
        this.grid.addColumnIcon('modifiedIconValue', '', {
          showIcon: () => true,
          showTooltipInFilter: true,
          suppressFilter: false,
          headerSeparator: true,
          useIconFromField: true,
          iconClassField: 'modifiedIconClass',
          tooltipField: 'modifiedTooltip',
          enableHiding: false,
        });
        if (this.hasReadPermission) {
          this.grid.addColumnIconEdit({
            tooltip: terms['core.edit'],
            onClick: row => {
              this.edit(row);
            },
          });
        }
        super.finalizeInitGrid();
        this.grid.resizeColumns(GridResizeType.AutoAllAndHeaders);
      });
  }

  override loadUserSettings(): Observable<any> {
    const settingTypes: number[] = [UserSettingType.VoucherSeriesSelection];

    return this.performLoad.load$(
      this.coreService.getUserSettings(settingTypes).pipe(
        tap(data => {
          this.selectedVoucherSeriesType = SettingsUtil.getIntUserSetting(
            data,
            UserSettingType.VoucherSeriesSelection,
            0
          );
        })
      )
    );
  }

  override onFinished(): void {
    if (this.filterIsReady) {
      this.refreshGrid();
    }
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: any
  ): Observable<VoucherGridDTO[]> {
    if (!this.filter?.accountYearId) return of([]);

    if (this.urlService.isTemplate()) {
      return this.service
        .getVoucherTemplates(this.filter.accountYearId, id)
        .pipe(
          map(rows => {
            const data = rows.map(row => {
              this.service.setIconAndColor(row);
              return row;
            });
            this.notifyGridDataLoaded(data);
            return rows;
          })
        );
    } else if (this.terms) {
      return this.service
        .getVouchersBySeries(
          this.filter.accountYearId,
          this.filter.voucherSeriesTypeId,
          id
        )
        .pipe(
          map(rows => {
            const data = rows.map(row => {
              this.service.setIconAndColor(row);
              return row;
            });
            this.rowData = new BehaviorSubject<VoucherGridDTO[]>(data || []);
            this.notifyGridDataLoaded(data);
            return rows;
          })
        );
    }

    return of([]);
  }

  loadAccountingOrderReportId(): Observable<any> {
    return this.performLoad.load$(
      this.service
        .getCompanySettingReportId(
          SettingMainType.Company,
          CompanySettingType.AccountingDefaultAccountingOrder,
          SoeReportTemplateType.VoucherList
        )
        .pipe(
          tap(x => {
            this.accountingOrderReportId = x;
          })
        )
    );
  }

  loadAccountDims(): Observable<IAccountDimSmallDTO[]> {
    return this.performLoad.load$(
      this.economyService
        .getAccountDimsSmall(
          false,
          false,
          true,
          true,
          false,
          false,
          false,
          false,
          true
        )
        .pipe(
          tap(x => {
            this.accountDims = x;
          })
        )
    );
  }

  loadAccountYears(): Observable<IAccountYearDTO[]> {
    return this.performLoad.load$(
      this.service.getAccountYears(false, true).pipe(
        tap(x => {
          this.accountYears = x.reverse();
        })
      )
    );
  }

  loadAccountYearDependentData() {
    return this.ayService.ensureAccountYearIsLoaded$(() => {
      return forkJoin([
        this.loadVoucherSeries(),
        this.loadVoucherSeriesTypes(),
      ]);
    });
  }

  loadVoucherSeries(): Observable<VoucherSeriesDTO[]> {
    //Never use cache since latest or start number might have been updated else where
    if (this.urlService.isTemplate()) {
      return this.performLoad.load$(
        this.service
          .getVoucherSeriesByYear(
            this.accountYearId || this.ayService.selectedAccountYearId(),
            true
          )
          .pipe(
            tap(result => {
              this.voucherSeries = result;
            })
          )
      );
    } else {
      return of([]);
    }
  }

  loadVoucherSeriesTypes() {
    return this.performLoad.load$(
      this.service
        .getVoucherSeriesByYear(
          this.accountYearId || this.ayService.selectedAccountYearId(),
          false
        )
        .pipe(
          tap(data => {
            this.voucherSeriesTypesDict.push(
              new SmallGenericType(0, this.translate.instant('common.all'))
            );
            data.forEach(v => {
              this.voucherSeriesTypesDict.push(
                new SmallGenericType(
                  v.voucherSeriesTypeId,
                  v.voucherSeriesTypeNr + '. ' + v.voucherSeriesTypeName
                )
              );
              this.voucherSeriesTypes.push(v);
            });
            if (this.selectedVoucherSeriesType) {
              const serieToSelect = this.voucherSeriesTypes.find(
                s => s.voucherSeriesTypeId === this.selectedVoucherSeriesType
              );

              this.selectedVoucherSeriesType = serieToSelect
                ? serieToSelect.voucherSeriesTypeId
                : this.voucherSeriesTypes.length > 0
                  ? this.voucherSeriesTypes[0].voucherSeriesTypeId
                  : 0;
            }
          })
        )
    );
  }

  doFilter(filter: VoucherGridFilterDTO) {
    this.filterIsReady = true;
    this.filter = filter;
    this.accountYearId = filter.accountYearId;
    this.service.setFilterSubject(filter);
    if (this.grid.gridIsReady) {
      this.refreshGrid();
    }
  }

  loadDetailRows(params: any) {
    return this.service.getVoucherRows(params.data.voucherHeadId).pipe(
      tap(rows => {
        const dataRows: VoucherRowDTO[] = [];
        rows.forEach(row => {
          const dataRow = new VoucherRowDTO();
          Object.assign(dataRow, row);
          if (row.dim1Nr) dataRow.dim1Name = row.dim1Nr + ' - ' + row.dim1Name;
          if (row.dim2Nr) dataRow.dim2Name = row.dim2Nr + ' - ' + row.dim2Name;
          if (row.dim3Nr) dataRow.dim3Name = row.dim3Nr + ' - ' + row.dim3Name;
          if (row.dim4Nr) dataRow.dim4Name = row.dim4Nr + ' - ' + row.dim4Name;
          if (row.dim5Nr) dataRow.dim5Name = row.dim5Nr + ' - ' + row.dim5Name;
          if (row.dim6Nr) dataRow.dim6Name = row.dim6Nr + ' - ' + row.dim6Name;
          if (row.amount < 0) {
            dataRow.amountCredit = Math.abs(row.amount);
            dataRow.amountDebet = 0;
          } else {
            dataRow.amountDebet = row.amount;
            dataRow.amountCredit = 0;
          }
          dataRows.push(dataRow);
        });

        params.data.rows = dataRows.sort(
          (a, b) => (a.rowNr ? a.rowNr : 0) - (b.rowNr ? b.rowNr : 0)
        );
        params.data.rowsLoaded = true;
        params.successCallback(dataRows);
      })
    );
  }

  saveVoucherSeriesType(data: number) {
    const model = new SaveUserCompanySettingModel(
      SettingMainType.User,
      UserSettingType.VoucherSeriesSelection,
      data
    );
    this.coreService.saveIntSetting(model).subscribe();
  }

  printSelectedVouchers() {
    const ids = this.grid.getSelectedIds('voucherHeadId');
    this.setPrintButtonDisabled(true);
    this.performLoad.load(
      this.requestReportService.printVoucherList(ids).pipe(
        tap(() => {
          this.setPrintButtonDisabled(false);
        })
      )
    );
  }

  private setPrintButtonDisabled(disabled: boolean): void {
    this.toolbarPrintDisabled.set(disabled);
  }

  private initDeleteSelectedVouchers() {
    this.deleteSelectedVouchers();
  }

  private deleteSelectedVouchers() {
    const ids = this.grid.getSelectedIds('voucherHeadId');
    this.performDeleteAction.crud(
      CrudActionTypeEnum.Delete,
      this.service.deleteVouchersOnlySuperSupport(ids).pipe(
        tap((backendResponse: BackendResponse) => {
          if (backendResponse.success) this.refreshGrid();
        })
      )
    );
  }
}
