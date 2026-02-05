import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  ActionResultSave,
  CompanySettingType,
  Feature,
  SettingMainType,
  SoeEntityState,
  SoeOriginType,
  SoeReportTemplateType,
  TermGroup,
  TermGroup_EDIInvoiceStatus,
  TermGroup_EdiMessageType,
  TermGroup_EDIOrderStatus,
  TermGroup_EDISourceType,
  TermGroup_EDIStatus,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IEdiEntryViewDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ProgressOptions } from '@shared/services/progress';
import { ReportService } from '@shared/services/report.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridComponent } from '@ui/grid/grid.component';
import {
  IMessageboxComponentResponse,
  MessageboxType,
} from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellClassParams, CellValueChangedEvent } from 'ag-grid-community';
import { map, Observable, of, take, tap } from 'rxjs';
import { EdiEntryViewDTO, UpdateEdiEntryDTO } from '../../models/edi.model';
import { EdiService } from '../../services/edi.service';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-edi-grid',
  templateUrl: './edi-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EdiGridComponent
  extends GridBaseDirective<EdiEntryViewDTO, EdiService>
  implements OnInit
{
  coreService = inject(CoreService);
  reportService = inject(ReportService);
  service = inject(EdiService);
  progressService = inject(ProgressService);
  performLoad = new Perform<any>(this.progressService);
  performAction = new Perform<any>(this.progressService);
  messageboxService = inject(MessageboxService);

  originType!: SoeOriginType;
  ediStatus!: TermGroup_EDIStatus;
  classification!: SoeEntityState;
  disableFunctionButton = signal(true);

  readPermission!: boolean;
  modifyPermission!: boolean;

  // Company settings
  private ediReportTemplateId!: number;
  private createAutoAttestOnEdi = false;
  private disableAutoLoad = false;
  private allItemsSelectionSettingType = 0;

  allItemsSelection!: number;

  // Collections
  ediStatuses: SmallGenericType[] = [];
  orderStatuses: SmallGenericType[] = [];
  invoiceStatuses: SmallGenericType[] = [];
  billingTypes: SmallGenericType[] = [];
  suppliers: ISmallGenericType[] = [];
  allItemsSelectionDict: ISmallGenericType[] = [];
  gridRows: IEdiEntryViewDTO[] = [];

  buttonFunctions: MenuButtonItem[] = [];

  //Flags
  isOpened!: boolean;
  isOpenOrders!: boolean;
  isClosedOrders!: boolean;
  isOpenInvoices!: boolean;
  isClosedInvoices!: boolean;
  initialSetupDone = false;

  showSave: boolean = false;
  showTransferOrderRows: boolean = false;
  showCreateInvoice: boolean = false;
  showCreatePdf: boolean = false;

  //GUI
  isOkToSave: boolean = false;
  isOkToGeneratePdf: boolean = false;
  isOkToTransferToSupplierInvoice: boolean = false;
  isOkToTransferToOrder: boolean = false;

  constructor() {
    super();
    effect(() => {
      this.originType = this.additionalGridProps().originType;
      this.ediStatus = this.additionalGridProps().ediStatus;
    });
  }

  ngOnInit(): void {
    this.originType = this.additionalGridProps().originType;
    this.ediStatus = this.additionalGridProps().ediStatus;

    this.classification =
      this.ediStatus === TermGroup_EDIStatus.Unprocessed
        ? SoeEntityState.Active
        : SoeEntityState.Inactive;

    if (this.classification !== SoeEntityState.Active) {
      if (this.originType === SoeOriginType.Order) {
        this.allItemsSelectionSettingType =
          UserSettingType.EdiOrdersAllItemsSelection;
      } else {
        this.allItemsSelectionSettingType =
          UserSettingType.EdiSupplierInvoicesAllItemsSelection;
      }
    }

    this.startFlow(
      Feature.Billing_Import_EDI_All,
      'common.customer.invoices.edi',
      {
        additionalReadPermissions: [Feature.Billing_Import_XEEdi],
        lookups: [
          this.loadCompanySettings(),
          this.loadUserSettings(),
          this.loadSelectionTypes(),
          this.loadEdiReportTemplateId(),
          this.loadEdiStatuses(),
          this.loadEdiOrderStatuses(),
          this.loadEdiInvoicesStatuses(),
          this.loadBillingTypes(),
          this.loadSuppliers(),
        ],
      }
    );
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.readPermission =
      this.flowHandler.hasReadAccess(Feature.Billing_Import_EDI_All) ||
      this.flowHandler.hasReadAccess(Feature.Billing_Import_XEEdi);
    this.modifyPermission =
      this.flowHandler.hasModifyAccess(Feature.Billing_Import_EDI_All) ||
      this.flowHandler.hasModifyAccess(Feature.Billing_Import_XEEdi);
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    // Commented to remove as per PBI #82267
    // if (SoeConfigUtil.sysCountryId === TermGroup_Country.FI) {
    //   this.toolbarService.createItemGroup({
    //     items: [
    //       this.toolbarService.createToolbarButton('billing.import.edi.retrieveposts', {
    //         behaviour: 'standard',
    //         iconName: 'download',
    //         caption: 'billing.import.edi.retrieveposts',
    //         tooltip: 'billing.import.edi.retrievepoststooltip',
    //         disabled: false,
    //         hidden: false,
    //         onAction: () => this.addEdiEntries(),
    //       }),
    //     ],
    //   })
    // }

    if (this.ediStatus !== TermGroup_EDIStatus.Unprocessed) {
      this.toolbarService.createItemGroup({
        items: [
          this.toolbarService.createToolbarButton('search', {
            iconName: signal('search'),
            caption: signal('core.search'),
            tooltip: signal('core.search'),
            onAction: () => this.search(),
          }),
        ],
      });
    }
  }

  override onGridReadyToDefine(grid: GridComponent<EdiEntryViewDTO>): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.isOpened = this.ediStatus === TermGroup_EDIStatus.Unprocessed;
    this.isOpenOrders =
      this.isOpened && this.originType === SoeOriginType.Order;
    this.isOpenInvoices =
      this.isOpened && this.originType !== SoeOriginType.Order;
    this.isClosedOrders =
      !this.isOpened && this.originType === SoeOriginType.Order;
    this.isClosedInvoices =
      !this.isOpened && this.originType !== SoeOriginType.Order;

    this.translate
      .get([
        'core.open',
        'core.close',
        'core.delete',
        'common.amount',
        'common.customer.invoices.invoicedate',
        'common.customer.invoices.duedate',
        'billing.import.edi.downloadstatus',
        'billing.import.edi.orderstatus',
        'billing.import.edi.invoicestatus',
        'billing.import.edi.type',
        'billing.import.edi.invoicenr',
        'billing.import.edi.ordernr',
        'billing.order.syswholeseller',
        'billing.import.edi.supplier',
        'billing.import.edi.supplierordernr',
        'billing.import.edi.customernr',
        'billing.import.edi.showmoreinfo',
        'billing.import.edi.retrieveposts',
        'billing.import.edi.retrievepoststooltip',
        'common.date',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnModified('isModified');

        //Setup buttons
        this.showSave = this.isOpenOrders || this.isOpenInvoices;
        this.showTransferOrderRows = this.isOpenOrders || this.isOpenInvoices;
        this.showCreateInvoice = this.isOpenInvoices;
        this.showCreatePdf = true;

        if (this.isClosedOrders || this.isClosedInvoices) {
          this.buttonFunctions.push({
            id: SoeEntityState.Active,
            label: terms['core.open'],
          });
        }
        this.buttonFunctions.push({
          id: SoeEntityState.Inactive,
          label: terms['core.close'],
        });
        this.buttonFunctions.push({
          id: SoeEntityState.Deleted,
          label: terms['core.delete'],
        });

        this.grid.addColumnText(
          'statusName',
          terms['billing.import.edi.downloadstatus'],
          { flex: 1 }
        );

        this.grid.addColumnText(
          'invoiceStatusName',
          terms['billing.import.edi.invoicestatus'],
          { flex: 1, hide: this.isOpenOrders || this.isClosedOrders }
        );

        this.grid.addColumnText(
          'orderStatusName',
          terms['billing.import.edi.orderstatus'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'billingTypeName',
          terms['billing.import.edi.type'],
          { flex: 1 }
        );

        this.grid.addColumnText(
          'invoiceNr',
          terms['billing.import.edi.invoicenr'],
          { flex: 1, hide: this.isOpenOrders || this.isClosedOrders }
        );

        this.grid.addColumnText(
          'orderNr',
          terms['billing.import.edi.ordernr'],
          { flex: 1, editable: true }
        );

        this.grid.addColumnAutocomplete(
          'supplierId',
          terms['billing.import.edi.supplier'],
          {
            editable: true,
            flex: 1,
            source: () => this.suppliers,
            optionDisplayNameField: 'supplierNrName',
            optionIdField: 'id',
            optionNameField: 'name',
            cellClassRules: {
              'error-background-color': (data: CellClassParams) => {
                return this.hasInvalidSupplier(data.data.supplierId);
              },
            },
            hide: !this.isOpened,
          }
        );

        this.grid.addColumnText(
          'supplierNrName',
          terms['billing.import.edi.supplier'],
          { flex: 1, hide: this.isOpened }
        );

        this.grid.addColumnText(
          'sellerOrderNr',
          terms['billing.import.edi.supplierordernr'],
          { flex: 1 }
        );

        this.grid.addColumnText(
          'wholesellerName',
          terms['billing.order.syswholeseller'],
          { flex: 1 }
        );

        this.grid.addColumnText(
          'buyerId',
          terms['billing.import.edi.customernr'],
          { flex: 1 }
        );

        this.grid.addColumnNumber('sum', terms['common.amount'], {
          flex: 1,
        });

        this.grid.addColumnDate(
          'invoiceDate',
          terms['common.customer.invoices.invoicedate'],
          { flex: 1, enableHiding: false }
        );

        this.grid.addColumnDate(
          'dueDate',
          terms['common.customer.invoices.duedate'],
          { flex: 1, enableHiding: false }
        );

        this.grid.addColumnDate('date', terms['common.date'], {
          flex: 1,
          enableHiding: false,
          hide: this.isOpenInvoices || this.isClosedInvoices,
        });

        this.grid.addColumnIcon(null, '', {
          iconName: 'file-alt',
          showIcon: (row: EdiEntryViewDTO) => this.showOrderIcon(row),
          onClick: row => this.showOrder(row),
          filter: true,
          showSetFilter: true,
          hide: !this.isOpenOrders,
        });

        this.grid.addColumnIcon(null, '', {
          iconName: 'file-alt',
          showIcon: (row: EdiEntryViewDTO) => this.showInvoiceIcon(row),
          onClick: row => this.showInvoice(row),
          filter: true,
          showSetFilter: true,
          hide: this.isOpenOrders,
        });

        this.grid.addColumnIcon(null, '', {
          iconName: 'file-pdf',
          showIcon: row => this.showPdfIcon(row),
          onClick: row => this.showPdf(row),
          filter: true,
          showSetFilter: true,
        });

        this.grid.addColumnIcon(null, '', {
          iconName: 'info-circle',
          iconClass: 'information-color',
          tooltip: terms['billing.import.edi.showmoreinfo'],
          onClick: row => this.showInfo(row),
          filter: true,
          showSetFilter: true,
        });

        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      classification: number;
      originType: number;
    }
  ): Observable<EdiEntryViewDTO[]> {
    if (this.classification !== SoeEntityState.Active) {
      if (this.initialSetupDone) {
        this.search();
      } else {
        this.initialSetupDone = true;
        return of([]);
      }
    }

    this.clearFlags();

    return this.performLoad.load$(
      this.service.getGrid(undefined, {
        classification: this.classification,
        originType: this.originType,
      })
    );
  }

  override loadCompanySettings(): Observable<any> {
    const settingTypes: number[] = [
      CompanySettingType.CreateAutoAttestFromSupplierOnEDI,
    ];

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(x => {
        this.createAutoAttestOnEdi = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.CreateAutoAttestFromSupplierOnEDI
        );
      })
    );
  }

  override loadUserSettings(): Observable<any> {
    if (this.allItemsSelectionSettingType > 0) return of(undefined);

    const settingTypes: number[] = [this.allItemsSelectionSettingType];
    return this.coreService.getUserSettings(settingTypes).pipe(
      tap(x => {
        this.allItemsSelection = SettingsUtil.getIntUserSetting(
          x,
          this.allItemsSelectionSettingType,
          1,
          false
        );
      })
    );
  }

  private loadSelectionTypes() {
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
        })
      );
  }

  private loadEdiReportTemplateId() {
    return this.reportService
      .getCompanySettingReportId(
        SettingMainType.Company,
        CompanySettingType.AccountingDefaultAccountingOrder,
        SoeReportTemplateType.VoucherList
      )
      .pipe(
        tap(x => {
          this.ediReportTemplateId = x;
        })
      );
  }

  private loadEdiStatuses() {
    return this.coreService
      .getTermGroupContent(TermGroup.EDIStatus, false, true)
      .pipe(
        tap(x => {
          this.ediStatuses = x;
        })
      );
  }

  private loadEdiOrderStatuses() {
    return this.coreService
      .getTermGroupContent(TermGroup.EDIOrderStatus, false, true)
      .pipe(
        tap(x => {
          this.orderStatuses = x;
        })
      );
  }

  private loadEdiInvoicesStatuses() {
    return this.coreService
      .getTermGroupContent(TermGroup.EDIInvoiceStatus, false, true)
      .pipe(
        tap(x => {
          this.invoiceStatuses = x;
        })
      );
  }

  private loadBillingTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceBillingType, false, true)
      .pipe(
        tap(x => {
          this.billingTypes = x;
        })
      );
  }

  private loadSuppliers() {
    return this.service.getSuppliersDict(true, true, true).pipe(
      tap(x => {
        this.suppliers = x;
      })
    );
  }

  setAllItemSelection(value: number) {
    this.allItemsSelection = value;
  }

  private showOrderIcon(item: EdiEntryViewDTO) {
    return item.orderId &&
      item.orderId > 0 &&
      item.orderStatus === TermGroup_EDIOrderStatus.Processed &&
      (item.status === TermGroup_EDIStatus.UnderProcessing ||
        item.status === TermGroup_EDIStatus.Processed)
      ? true
      : false;
  }

  private showInvoiceIcon(item: EdiEntryViewDTO) {
    return item.invoiceId &&
      item.invoiceId > 0 &&
      item.invoiceStatus === TermGroup_EDIInvoiceStatus.Processed &&
      (item.status === TermGroup_EDIStatus.UnderProcessing ||
        item.status === TermGroup_EDIStatus.Processed)
      ? true
      : false;
  }

  private showPdfIcon(item: EdiEntryViewDTO) {
    return item.hasPdf;
  }

  private showInfo(item: EdiEntryViewDTO) {
    if (!item) return;

    // Columns
    const keys: string[] = [
      'core.info',
      'core.source',
      'common.currency',
      'common.errormessage',
      'billing.import.edi.messagefromoperator',
      'billing.import.edi.errorftp',
      'billing.import.edi.erroredi',
      'billing.import.edi.errorxml',
      'billing.import.edi.errordownload',
      'billing.import.edi.errorinterpretation',
      'billing.import.edi.errorunknown',
      'billing.import.edi.invoicestatus',
      'billing.import.edi.invoicenrmissing',
      'billing.import.edi.suppliermissing',
      'billing.import.edi.orderstatus',
      'billing.import.edi.ordernrmissing',
      'billing.import.edi.wholesellermissing',
      'billing.import.edi.downloaddate',
      'billing.import.edi.messagetype',
    ];

    this.translate
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        let message: string = '';

        if (item.operatorMessage && item.operatorMessage.length > 0) {
          message += terms['billing.import.edi.messagefromoperator'] + '\r\n';
          message += item.operatorMessage + '\r\n' + '\r\n';
        }

        if (
          item.status !== TermGroup_EDIStatus.Processed &&
          item.errorCode &&
          item.errorCode > 0
        ) {
          message += terms['common.errormessage'] + ':\r\n';
          switch (item.errorCode) {
            case ActionResultSave.EdiInvalidUri:
              message += terms['billing.import.edi.errorftp'];
              break;
            case ActionResultSave.EdiInvalidType:
              message += terms['billing.import.edi.erroredi'];
              break;
            case ActionResultSave.EdiFailedParse:
              message += terms['billing.import.edi.errorxml'];
              break;
            case ActionResultSave.EdiFailedFileListing:
              message += terms['billing.import.edi.errordownload'];
              break;
            case ActionResultSave.EdiFailedUnknown:
              message += terms['billing.import.edi.errorinterpretation'];
              break;
            default:
              message += terms['billing.imprt.edi.errorunknown'];
              break;
          }
          message += '\r\n' + '\r\n';
        }

        if (item.invoiceStatus === TermGroup_EDIInvoiceStatus.Error) {
          message += terms['billing.import.edi.invoicestatus'] + ': ' + '\r\n';
          if (!item.invoiceNr || item.invoiceNr.length === 0) {
            message += terms['billing.import.edi.invoicenrmissing'] + '\r\n';
          }

          if (!item.supplierId) {
            message += terms['billing.import.edi.suppliermissing'] + '\r\n';
          }
          message += '\r\n';
        }

        if (item.orderStatus == TermGroup_EDIOrderStatus.Error) {
          message += terms['billing.import.edi.orderstatus'] + ': ' + '\r\n';
          if (!item.orderNr || item.orderNr.length === 0) {
            message += terms['billing.import.edi.ordernrmissing'] + '\r\n';
          }

          if (
            !item.wholesellerName ||
            item.wholesellerName.length === 0 ||
            item.wholesellerId == 0
          ) {
            message += terms['billing.import.edi.wholesellermissing'] + '\r\n';
          }
          message += '\r\n';
        }

        message += terms['core.source'] + ': ' + item.sourceTypeName + '\r\n';
        message += terms['common.currency'] + ': ' + item.currencyCode + '\r\n';
        message +=
          terms['billing.import.edi.downloaddate'] +
          ': ' +
          (item.created
            ? DateUtil.format(new Date(item.created), 'yyyy-MM-dd HH:mm')
            : '') +
          '\r\n';
        message +=
          terms['billing.import.edi.messagetype'] +
          ': ' +
          item.ediMessageTypeName +
          '\r\n';

        this.messageboxService.information('core.info', message);
      });
  }

  private hasInvalidSupplier(supplierId: number) {
    return this.suppliers.filter(r => r.id === supplierId).length === 0
      ? true
      : false;
  }

  private showPdf(row: EdiEntryViewDTO) {
    const ediPdfReportUrl =
      '/ajax/downloadReport.aspx?templatetype=' +
      SoeReportTemplateType.SymbrioEdiSupplierInvoice +
      '&edientryid=' +
      row.ediEntryId;
    BrowserUtil.openInNewTab(window, ediPdfReportUrl);
  }

  private showOrder(row: EdiEntryViewDTO) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/order/status/default.aspx?invoiceId=${row.orderId}&invoiceNr=${row.invoiceNr}`
    );
  }

  private showInvoice(row: EdiEntryViewDTO) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/economy/supplier/invoice/status/?invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceNr}`
    );
  }

  search() {
    const filterModels = this.grid.agGrid.api.getFilterModel();
    this.loadFilteredGridData(filterModels);
  }

  loadFilteredGridData(filterModels: any) {
    const filterValues: any[] = [];
    const billingTypes: any[] = [];
    //   if (filterModels['billingTypeName']) {
    //       <any[]>filterModels['billingTypeName'].forEach((value: any) => {
    //         const billingType = this.billingTypes.fin
    //       })
    //       _.forEach(filterModels['billingTypeName'], (value) => {
    //           var billingType = _.find(this.billingTypes, { value: value.toString() });
    //           if (billingType)
    //               billingTypes.push(billingType.id);

    //       });
    //   }
    const buyerId: string = filterModels['buyerId']
      ? filterModels['buyerId'].filter
      : '';
    const dueDate: Date = new Date(<any>filterModels['dueDate']?.dateFrom);
    const invoiceDate: Date = new Date(
      <any>filterModels['invoiceDate']?.dateFrom
    );
    const orderNr: string = filterModels['orderNr']
      ? filterModels['orderNr'].filter
      : '';
    const orderStatuses: number[] = [];
    //   if (filterModels['orderStatusName']) {
    //     orderStatuses = filterModels['orderStatusName'];
    //       // _.forEach(filterModels['orderStatusName'], (value) => {
    //       //     var orderStatus = _.find(this.orderStatuses, { value: value.toString() });
    //       //     if (orderStatus)
    //       //         orderStatuses.push(orderStatus.id);

    //       // });
    //       <any[]>filterModels['orderStatusName'].forEach((value: any) => {

    //       })

    //   }
    const sellerOrderNr: string = filterModels['sellerOrderNr']
      ? filterModels['sellerOrderNr'].filter
      : '';
    const ediStatuses: any[] = [];
    //   if (filterModels['statusName']) {
    //       _.forEach(filterModels['statusName'], (value) => {
    //           var ediStatus = _.find(this.ediStatuses, { value: value.toString() });
    //           if (ediStatus)
    //               ediStatuses.push(ediStatus.id);

    //       });
    //   }

    const sum: number = filterModels['sum'] ? filterModels['sum'].filter : 0;
    const supplierNrName: string = filterModels['supplierNrName']
      ? filterModels['supplierNrName'].filter
      : '';
    //   this.progress.startLoadingProgress([() => {
    //       return this.importService.getFilteredEdiEntryViews(this.classification, this.originType, billingTypes, buyerId, dueDate, invoiceDate, orderNr, orderStatuses, sellerOrderNr, ediStatuses, sum, supplierNrName, this.allItemsSelection).then(x => {
    //           _.forEach(x, (row: EdiEntryViewDTO) => {
    //               //Fix dates
    //               if(row.date)
    //                   row.date = new Date(<any>row.date).date();
    //               if(row.dueDate)
    //                   row.dueDate = new Date(<any>row.dueDate).date();
    //               if(row.invoiceDate)
    //                   row.invoiceDate = new Date(<any>row.invoiceDate).date();

    //               row.supplierNrName = row.supplierNr + ' ' + row.supplierName;
    //               if (!row.supplierNr)
    //                   row.supplierNrName = '';

    //               row['hasInvalidSupplier'] = _.find(this.suppliers, { id: row.supplierId }) ? false : true;
    //           });
    //           return x;
    //       }).then(data => {
    //           this.setData(data);
    //       });
    //   }]);

    const entryModel = {
      classification: this.classification,
      originType: this.originType,
      billingTypes: billingTypes,
      buyerId: buyerId,
      dueDate: dueDate,
      invoiceDate: invoiceDate,
      orderNr: orderNr,
      orderStatuses: orderStatuses,
      sellerOrderNr: sellerOrderNr,
      ediStatuses: ediStatuses,
      sum: sum,
      supplierNrName: supplierNrName,
      allItemsSelection: this.allItemsSelection,
    };

    this.performLoad.load(
      this.service.getFilteredEdiEntryViews(entryModel).pipe(
        map(rows => {
          const ediRows: EdiEntryViewDTO[] = rows as EdiEntryViewDTO[];
          ediRows.forEach((row: EdiEntryViewDTO) => {
            //Fix dates
            if (row.date) row.date = new Date(<any>row.date);
            if (row.dueDate) row.dueDate = new Date(<any>row.dueDate);
            if (row.invoiceDate)
              row.invoiceDate = new Date(<any>row.invoiceDate);

            row.supplierNrName = row.supplierNr + ' ' + row.supplierName;
            if (!row.supplierNr) row.supplierNrName = '';

            row.hasInvalidSupplier = this.hasInvalidSupplier(
              row.supplierId ?? -1
            );
          });
          return ediRows;
        }),
        tap(x => {
          this.grid.setData(x);
        })
      )
    );
  }

  rowSelectionChanged(rows: EdiEntryViewDTO[]): void {
    this.disableFunctionButton.set(rows.length === 0);
    this.clearFlags();
    this.grid.getSelectedRows().forEach(row => {
      if (this.isOkToSaveRow(row)) this.isOkToSave = true;
      if (this.isOkTransferToSupplierInvoice(row))
        this.isOkToTransferToSupplierInvoice = true;
      if (this.isOkTransferToOrder(row)) this.isOkToTransferToOrder = true;
      if (this.okToGeneratePdf(row)) this.isOkToGeneratePdf = true;
    });
  }

  private onCellValueChanged(event: CellValueChangedEvent) {
    if (event.newValue === event.oldValue || !event.newValue) {
      return;
    }

    switch (event.colDef.field) {
      case 'supplierId':
        const supplier = this.suppliers.find(s => s.id == event.newValue);
        if (supplier) {
          event.data.supplierId = supplier.id;
          event.data.supplierName = '';
          event.data.supplierNr = '';
          event.data.hasInvalidSupplier = false;
        } else {
          event.data.supplierNrName = event.oldValue;
        }
        break;
    }
    this.setRowAsModified(event);
  }

  setRowAsModified(row: CellValueChangedEvent) {
    if (row.data) {
      row.data.isModified = true;
      const selRows = this.grid.getSelectedRows();
      row.node.setSelected(true);
      this.grid.refreshCells();
    }
  }

  save(options?: ProgressOptions) {
    const items: UpdateEdiEntryDTO[] = [];
    this.grid.getSelectedRows().forEach((row: EdiEntryViewDTO) => {
      const item = new UpdateEdiEntryDTO();
      item.ediEntryId = row.ediEntryId;
      item.supplierId = row.supplierId;
      item.orderNr = row.orderNr;
      items.push(item);
    });

    if (items.length > 0) {
      this.performAction.crud(
        CrudActionTypeEnum.Save,
        this.service.updateEdiEntries(items).pipe(
          tap(res => {
            if (res.success) {
              this.refreshGrid();
            }
          })
        ),
        undefined,
        undefined,
        options
      );
    }
  }

  private isOkToSaveRow(item: EdiEntryViewDTO) {
    return item.isModified;
  }

  isOkTransferToOrder(item: EdiEntryViewDTO) {
    return (
      item.orderNr &&
      item.orderNr.length > 0 &&
      item.orderStatus == TermGroup_EDIOrderStatus.Unprocessed &&
      (item.status == TermGroup_EDIStatus.Processed ||
        item.status == TermGroup_EDIStatus.UnderProcessing)
    );
  }

  isOkTransferToSupplierInvoice(item: EdiEntryViewDTO) {
    return (
      item.invoiceNr &&
      item.invoiceNr.length > 0 &&
      !item.invoiceId &&
      item.supplierId &&
      item.ediMessageType == TermGroup_EdiMessageType.SupplierInvoice &&
      item.invoiceStatus == TermGroup_EDIInvoiceStatus.Unprocessed &&
      (item.status == TermGroup_EDIStatus.UnderProcessing ||
        item.status == TermGroup_EDIStatus.Processed)
    );
  }

  okToGeneratePdf(item: EdiEntryViewDTO) {
    return (
      (item.status == TermGroup_EDIStatus.UnderProcessing ||
        item.status == TermGroup_EDIStatus.Processed) &&
      !item.hasPdf &&
      this.ediReportTemplateId > 0
    );
  }

  initTransferToOrder() {
    const keys: string[] = [
      'core.warning',
      'core.verifyquestion',
      'billing.import.edi.transferposttoorder',
      'billing.import.edi.transferpoststoorder',
      'billing.import.edi.invalidtransferposttoorder',
      'billing.import.edi.invalidtransferpoststoorder',
      'billing.import.edi.posttransferedtoorder',
      'billing.import.edi.poststransferedtoorder',
      'billing.import.edi.transferposttoorderfailed',
      'billing.import.edi.transferpoststoorderfailed',
    ];

    this.translate
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        let nbrOfValid: number = 0;
        let nbrOfInvalid: number = 0;
        const dict: number[] = [];

        this.grid.getSelectedRows().forEach(row => {
          if (this.isOkTransferToOrder(row)) {
            dict.push(row.ediEntryId);
            nbrOfValid += 1;
          } else {
            nbrOfInvalid += 1;
          }
        });

        let title = terms['core.verifyquestion'];
        let message = '';
        let successMessage = '';
        let errorMessage = '';
        let messageboxType: MessageboxType = 'custom';
        if (nbrOfInvalid === 0) {
          message =
            nbrOfValid.toString() +
            ' ' +
            (nbrOfValid > 1
              ? terms['billing.import.edi.transferpoststoorder']
              : terms['billing.import.edi.transferposttoorder']);
          successMessage =
            nbrOfValid > 1
              ? terms['billing.import.edi.poststransferedtoorder']
              : terms['billing.import.edi.posttransferedtoorder'];
          errorMessage =
            nbrOfValid > 1
              ? terms['billing.import.edi.transferpoststoorderfailed']
              : terms['billing.import.edi.transferposttoorderfailed'];
        } else {
          title = terms['core.warning'];
          message =
            nbrOfValid.toString() +
            ' ' +
            (nbrOfValid > 1
              ? terms['billing.import.edi.transferpoststoorder']
              : terms['billing.import.edi.transferposttoorder']);
          message +=
            '\n' +
            nbrOfInvalid.toString() +
            ' ' +
            (nbrOfInvalid > 1
              ? terms['billing.import.edi.invalidtransferpoststoorder']
              : terms['billing.import.edi.invalidtransferposttoorder']);
          successMessage =
            nbrOfValid > 1
              ? terms['billing.import.edi.poststransferedtoorder']
              : terms['billing.import.edi.posttransferedtoorder'];
          errorMessage =
            nbrOfValid > 1
              ? terms['billing.import.edi.transferpoststoorderfailed']
              : terms['billing.import.edi.transferposttoorderfailed'];
          messageboxType = 'warning';
        }

        const mb = this.messageboxService.show(title, message, {
          customIconName: 'question-circle',
          buttons: 'okCancel',
          type: messageboxType,
        });

        mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
          if (response?.result) this.transferToOrder(dict, errorMessage);
        });
      });
  }

  private transferToOrder(dict: any[], errorMessage: string) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.transferEdiToOrders(dict).pipe(
        tap(res => {
          const errorMsg = ResponseUtil.getErrorMessage(res);
          if (res.success) {
            this.refreshGrid();
          } else if (errorMsg && errorMsg.length > 0) {
            this.messageboxService.error(errorMessage, errorMsg);
          }
        })
      ),
      undefined,
      undefined
    );
  }

  initCreateInvoice() {
    const keys: string[] = [
      'core.warning',
      'core.verifyquestion',
      'common.edistransfertoinvoicevalid',
      'common.editransfertoinvoicevalid',
      'common.edistransfertoinvoiceinvalid',
      'common.editransfertoinvoiceinvalid',
      'common.invoiceswascreated',
      'common.invoicewascreated',
      'common.edistransfertoinvoicefailed',
      'common.editransfertoinvoicefailed',
    ];

    this.translate
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        let nbrOfValid: number = 0;
        let nbrOfInvalid: number = 0;
        const dict: number[] = [];

        this.grid.getSelectedRows().forEach(row => {
          if (this.isOkTransferToSupplierInvoice(row)) {
            dict.push(row.ediEntryId);
            nbrOfValid += 1;
          } else {
            nbrOfInvalid += 1;
          }
        });

        let title = terms['core.verifyquestion'];
        let message = '';
        let successMessage = '';
        let errorMessage = '';
        const messageboxType: MessageboxType = 'custom';

        if (nbrOfInvalid === 0) {
          message =
            nbrOfValid.toString() +
            ' ' +
            (nbrOfValid > 1
              ? terms['common.edistransfertoinvoicevalid']
              : terms['common.editransfertoinvoicevalid']);
          successMessage =
            nbrOfValid > 1
              ? terms['common.invoiceswascreated']
              : terms['common.invoicewascreated'];
          errorMessage =
            nbrOfValid > 1
              ? terms['common.edistransfertoinvoicefailed']
              : terms['common.editransfertoinvoicefailed'];
        } else {
          title = terms['core.warning'];
          message =
            nbrOfValid.toString() +
            ' ' +
            (nbrOfValid > 1
              ? terms['common.edistransfertoinvoicevalid']
              : terms['common.editransfertoinvoicevalid']);
          message +=
            '\n' +
            nbrOfInvalid.toString() +
            ' ' +
            (nbrOfInvalid > 1
              ? terms['common.edistransfertoinvoiceinvalid']
              : terms['common.editransfertoinvoiceinvalid']);
          successMessage =
            nbrOfValid > 1
              ? terms['common.invoiceswascreated']
              : terms['common.invoicewascreated'];
          errorMessage =
            nbrOfValid > 1
              ? terms['common.edistransfertoinvoicefailed']
              : terms['common.editransfertoinvoicefailed'];
        }

        const mb = this.messageboxService.show(title, message, {
          customIconName: 'question-circle',
          buttons: 'okCancel',
          type: messageboxType,
        });

        mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
          if (response?.result) this.createInvoice(dict, errorMessage);
        });
      });
  }

  private createInvoice(dict: number[], errorMessage: string) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.transferEdiToInvoices(dict).pipe(
        tap(res => {
          const errorMsg = ResponseUtil.getErrorMessage(res);
          if (res.success) {
            this.refreshGrid();
          } else if (
            ResponseUtil.getErrorNumber(res) &&
            errorMsg &&
            errorMsg.length > 0
          ) {
            this.messageboxService.error(
              this.translate.instant('core.error'),
              errorMsg
            );
          }
        })
      ),
      undefined,
      undefined
    );
  }

  createPdf() {
    // Columns
    const keys: string[] = [
      'core.warning',
      'core.verifyquestion',
      'common.createpdfsvalid',
      'common.createpdfvalid',
      'common.postsinvalid',
      'common.postinvalid',
      'common.pdfscreated',
      'common.pdfcreated',
      'common.pdfserror',
      'common.pdferror',
    ];

    this.translate
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        let nbrOfValid: number = 0;
        let nbrOfInvalid: number = 0;
        const dict: number[] = [];
        this.grid.getSelectedRows().forEach(row => {
          if (this.okToGeneratePdf(row)) {
            dict.push(row.ediEntryId);
            nbrOfValid += 1;
          } else {
            nbrOfInvalid += 1;
          }
        });

        let title = '';
        let message = '';
        let successMessage = '';
        let errorMessage = '';
        let messageboxType: MessageboxType = 'question';
        if (nbrOfInvalid === 0) {
          title = terms['core.verifyquestion'];
          message =
            nbrOfValid.toString() +
            ' ' +
            (nbrOfValid > 1
              ? terms['common.createpdfsvalid']
              : terms['common.createpdfvalid']);
          successMessage =
            nbrOfValid > 1
              ? terms['common.pdfscreated']
              : terms['common.pdfcreated'];
          errorMessage =
            nbrOfValid > 1
              ? terms['common.pdfserror']
              : terms['common.pdferror'];
          messageboxType = 'question';
        } else {
          title = terms['core.warning'];
          message =
            nbrOfValid.toString() +
            ' ' +
            (nbrOfValid > 1
              ? terms['common.createpdfsvalid']
              : terms['common.createpdfvalid']);
          message +=
            '\n' +
            nbrOfInvalid.toString() +
            ' ' +
            (nbrOfInvalid
              ? terms['common.postsinvalid']
              : terms['common.postinvalid']);
          successMessage =
            nbrOfValid > 1
              ? terms['common.pdfscreated']
              : terms['common.pdfcreated'];
          errorMessage =
            nbrOfValid > 1
              ? terms['common.pdfserror']
              : terms['common.pdferror'];
          messageboxType = 'warning';
        }

        const mb = this.messageboxService.show(title, message, {
          customIconName: 'question-circle',
          buttons: 'okCancel',
          type: messageboxType,
        });

        mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
          if (response?.result) this.generatePdfs(dict, errorMessage);
        });
      });
  }

  private generatePdfs(dict: any[], errorMessage: string) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.generateReportForEdi(dict).pipe(
        tap(res => {
          const errorMsg = ResponseUtil.getErrorMessage(res);
          if (res.success) {
            this.refreshGrid();
          } else if (
            ResponseUtil.getErrorNumber(res) &&
            errorMsg &&
            errorMsg.length > 0
          ) {
            this.messageboxService.error(
              this.translate.instant('core.error'),
              errorMsg
            );
          }
        })
      ),
      undefined,
      undefined
    );
  }

  initChangeEdiState(stateTo: MenuButtonItem) {
    if (!stateTo) return;

    const keys: string[] = [
      'core.warning',
      'core.verifyquestion',
      'common.ediclosevalid',
      'common.edisclosevalid',
      'common.ediclosefailed',
      'common.edisclosefailed',
      'common.edideletevalid',
      'common.edisdeletevalid',
      'common.edideletefailed',
      'common.edisdeletefailed',
      'common.edisopenvalid',
      'common.ediopenvalid',
      'common.edisopenfailed',
      'common.ediopenfailed',
    ];

    this.translate
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        let nbrOfValid: number = 0;
        const dict: number[] = [];

        this.grid.getSelectedRows().forEach(row => {
          dict.push(row.ediEntryId);
          nbrOfValid += 1;
        });

        const title = '';
        let message = '';
        let errorMessage = '';
        const messageboxType: MessageboxType = 'information';

        if (stateTo.id === SoeEntityState.Active) {
          message =
            nbrOfValid.toString() +
            ' ' +
            (nbrOfValid > 1
              ? terms['common.edisopenvalid']
              : terms['common.ediopenvalid']);
          errorMessage =
            nbrOfValid > 1
              ? terms['common.edisopenfailed']
              : terms['common.ediopenfailed'];
        } else if (stateTo.id === SoeEntityState.Inactive) {
          message =
            nbrOfValid.toString() +
            ' ' +
            (nbrOfValid > 1
              ? terms['common.edisclosevalid']
              : terms['common.ediclosevalid']);
          errorMessage =
            nbrOfValid > 1
              ? terms['common.edisclosefailed']
              : terms['common.ediclosefailed'];
        } else {
          message =
            nbrOfValid.toString() +
            ' ' +
            (nbrOfValid > 1
              ? terms['common.edisdeletevalid']
              : terms['common.edideletevalid']);
          errorMessage =
            nbrOfValid > 1
              ? terms['common.edisdeletefailed']
              : terms['common.common.edideletefailed'];
        }

        const mb = this.messageboxService.show(title, message, {
          customIconName: 'question-circle',
          buttons: 'okCancel',
          type: messageboxType,
        });

        mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
          if (response?.result)
            this.changeEdiState(dict, stateTo.id ?? 0, errorMessage);
        });
      });
  }

  private changeEdiState(
    dict: number[],
    stateTo: number,
    errorMessage: string
  ) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.changeEdiState(dict, stateTo).pipe(
        tap(res => {
          const errorMsg = ResponseUtil.getErrorMessage(res);
          if (res.success) {
            this.refreshGrid();
          } else if (
            ResponseUtil.getErrorNumber(res) &&
            errorMsg &&
            errorMsg.length > 0
          ) {
            this.messageboxService.error(
              this.translate.instant('core.error'),
              errorMsg
            );
          }
        })
      ),
      undefined,
      undefined
    );
  }

  private addEdiEntries() {
    // this.performAction.crud(
    // CrudActionTypeEnum.Save,
    this.service.addEdiEntrys(TermGroup_EDISourceType.EDI).pipe(
      tap(res => {
        if (res.success) {
          if (res.keys && res.keys.length > 0) this.generatePdfs(res.keys, '');
          this.refreshGrid();
        } else if (res.errorNumber && res.errorMessage) {
          this.messageboxService.error(
            this.translate.instant('core.error'),
            res.errorMessage
          );
        }
      })
    ),
      undefined,
      undefined;
    // );
  }

  private clearFlags() {
    this.isOkToSave = false;
    this.isOkToGeneratePdf = false;
    this.isOkToTransferToSupplierInvoice = false;
    this.isOkToTransferToOrder = false;
  }
}
