import { Component, inject, OnInit, signal } from '@angular/core';
import { ProjectTimeBlockDTO } from '@features/billing/project-time-report/models/project-time-report.model';
import { ManageService } from '@features/manage/services/manage.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import {
  CompanySettingType,
  Feature,
  SoeInvoiceRowDiscountType,
  SoeInvoiceRowType,
  SoeProductRowType,
  TermGroup_AttestEntity,
  TermGroup_TimeCodeRegistrationType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAttestStateDTO,
  IAttestTransitionDTO,
  IHandleBillingRowDTO,
  IProjectTimeBlockDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { DateUtil } from '@shared/util/date-util';
import { NumberUtil } from '@shared/util/number-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { CustomerInvoiceGridButtonFunctions } from '@shared/util/Enumerations';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ColDef } from 'ag-grid-community';
import { Observable, of, tap } from 'rxjs';
import {
  HandleBillingRowDTO,
  SearchCustomerInvoiceRowModel,
} from '../../models/handle-billing.model';
import { HandleBillingService } from '../../services/handle-billing.service';

@Component({
  templateUrl: 'handle-billing-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class HandleBillingGridComponent
  extends GridBaseDirective<HandleBillingRowDTO, HandleBillingService>
  implements OnInit
{
  service = inject(HandleBillingService);
  coreService = inject(CoreService);
  messageboxService = inject(MessageboxService);
  manageService = inject(ManageService);
  private readonly progress = inject(ProgressService);
  private readonly performGridLoad = new Perform<HandleBillingRowDTO[]>(
    this.progressService
  );
  private readonly performLoadDetails = new Perform<ProjectTimeBlockDTO[]>(
    this.progressService
  );

  // Permissions
  hasInvoiceRowPermission: boolean = false;
  hasStatusChangePermission: boolean = false;
  hasTransferToPreliminaryPermission: boolean = false;
  hasTransferToDefinitivePermission: boolean = false;
  hasSalesPricePermission = true;
  hasCurrencyPermission = false;
  hasPurchasePricePermission = true;

  hasEditProjectPermission = false;
  hasEditOrderPermission = false;
  hasInvoiceTimePermission = false;
  hasWorkTimePermission = false;
  hasTimeRowsPermission = false;

  // Company settings
  transferAndPrint = false;
  defaultBillingInvoiceReportId: number = 0;
  useExtendedTimeRegistration = false;
  productGuaranteeId: number = 0;
  attestStateReadyId: number = 0;
  usePartialInvoicingOnOrderRow = false;
  attestStateTransferredOrderToInvoiceId: number = 0;
  attestStateTransferredOrderToContractId: number = 0;

  // User settings
  onlyValidToTransfer: boolean = false;
  loadOnlyMine: boolean = false;

  // Lookups
  initialAttestState: IAttestStateDTO | null = null;

  // Collections
  excludedAttestStates: number[] = [];
  attestTransitions: IAttestTransitionDTO[] = [];
  attestStates: IAttestStateDTO[] = [];
  availableAttestStates: IAttestStateDTO[] = [];
  availableAttestStateOptions: any[] = [];

  // Values
  selectedAttestStateId: number = 0;

  // Summary
  filteredAmount: number = 0;
  filteredAmountValidForInvoice: number = 0;

  // Signals
  protected noSelectedRows = signal(true);
  protected readOnly = signal(false);

  // Functions
  buttonFunctions: MenuButtonItem[] = [];

  //Search model
  searchModel!: SearchCustomerInvoiceRowModel;

  ngOnInit(): void {
    console.log('Grid init');
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Order_HandleBilling,
      'Billing.Invoices.HouseholdDeduction.Applied',
      {
        lookups: [
          this.loadTerms(),
          this.loadCompanySettings(),
          this.loadUserSettings(),
          this.loadUserAttestTransitions(),
        ],
        useLegacyToolbar: true,
        additionalModifyPermissions: [
          Feature.Billing_Order_OrdersAll,
          Feature.Billing_Order_Orders,
          Feature.Billing_Order_OrdersUser,
          Feature.Billing_Order_Status,
          Feature.Billing_Product_Products_ShowSalesPrice,
          Feature.Billing_Product_Products_ShowPurchasePrice,
          Feature.Billing_Invoice_Status_DraftToOrigin,
          Feature.Billing_Project_Edit,
          Feature.Billing_Order_Orders_Edit,
          Feature.Time_Project_Invoice_InvoicedTime,
          Feature.Time_Project_Invoice_WorkedTime,
          Feature.Time_Project_Invoice_Edit,
          Feature.Economy_Customer_Invoice_Status_Foreign,
        ],
      }
    );
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();

    //this.loadOnlyMine = this.onlyMineLocked = response[Feature.Billing_Order_OrdersUser].readPermission;

    this.hasStatusChangePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Status
    );
    this.hasInvoiceRowPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Orders_Edit_ProductRows
    );
    this.hasTransferToPreliminaryPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Status_OrderToInvoice
    );
    this.hasTransferToDefinitivePermission =
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Order_Status_OrderToInvoice
      ) &&
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Invoice_Status_DraftToOrigin
      );
    this.hasSalesPricePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Product_Products_ShowSalesPrice
    );
    this.hasPurchasePricePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Product_Products_ShowPurchasePrice
    );
    this.hasEditProjectPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Project_Edit
    );
    this.hasEditOrderPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Orders_Edit
    );
    this.hasInvoiceTimePermission = this.flowHandler.hasModifyAccess(
      Feature.Time_Project_Invoice_InvoicedTime
    );
    this.hasWorkTimePermission = this.flowHandler.hasModifyAccess(
      Feature.Time_Project_Invoice_WorkedTime
    );
    this.hasTimeRowsPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Orders_Edit
    );
    this.hasCurrencyPermission = this.flowHandler.hasModifyAccess(
      Feature.Economy_Customer_Invoice_Status_Foreign
    );

    this.readOnly.set(!this.flowHandler.modifyPermission());
  }

  override onFinished(): void {
    if (this.hasTransferToPreliminaryPermission)
      this.buttonFunctions.push({
        id: CustomerInvoiceGridButtonFunctions.TransferToPreliminarInvoice,
        label: this.terms['core.transfertopreliminaryinvoice'],
      });
    if (this.hasTransferToPreliminaryPermission)
      this.buttonFunctions.push({
        id: CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndMergeOrders,
        label: this.terms['billing.contract.transfertopreliminaryandmerge'],
      });
    if (this.hasTransferToDefinitivePermission && this.transferAndPrint)
      this.buttonFunctions.push({
        id: CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndPrint,
        label: this.terms['core.transfertoinvoiceandprint'],
      });
    if (this.hasTimeRowsPermission)
      this.buttonFunctions.push({
        id: CustomerInvoiceGridButtonFunctions.SplitTimeRows,
        label: this.terms['billing.order.splittimerows'],
      });
  }

  // Lookups
  override loadUserSettings() {
    return this.coreService
      .getUserSettings([
        UserSettingType.BillingHandleBillingOnlyMine,
        UserSettingType.BillingHandleBillingOnlyValid,
      ])
      .pipe(
        tap(settings => {
          this.onlyValidToTransfer = SettingsUtil.getBoolUserSetting(
            settings,
            UserSettingType.BillingHandleBillingOnlyValid
          );

          this.loadOnlyMine = SettingsUtil.getBoolUserSetting(
            settings,
            UserSettingType.BillingHandleBillingOnlyMine
          );
        })
      );
  }

  override loadCompanySettings() {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.BillingStatusTransferredOrderToInvoiceAndPrint,
        CompanySettingType.BillingDefaultInvoiceTemplate,
        CompanySettingType.BillingStatusTransferredOrderToInvoice,
        CompanySettingType.BillingStatusTransferredOrderToContract,
        CompanySettingType.ProjectUseExtendedTimeRegistration,
        CompanySettingType.ProductGuarantee,
        CompanySettingType.BillingStatusOrderReadyMobile,
        CompanySettingType.BillingUsePartialInvoicingOnOrderRow,
      ])
      .pipe(
        tap(settings => {
          /*this.useQuantityPrices = SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.BillingUseQuantityPrices
          );*/
          this.transferAndPrint = SettingsUtil.getBoolCompanySetting(
            settings,
            CompanySettingType.BillingStatusTransferredOrderToInvoiceAndPrint
          );
          this.defaultBillingInvoiceReportId =
            SettingsUtil.getIntCompanySetting(
              settings,
              CompanySettingType.BillingDefaultInvoiceTemplate
            );
          this.attestStateTransferredOrderToInvoiceId =
            SettingsUtil.getIntCompanySetting(
              settings,
              CompanySettingType.BillingUseQuantityPrices
            );
          this.attestStateTransferredOrderToContractId =
            SettingsUtil.getIntCompanySetting(
              settings,
              CompanySettingType.BillingUseQuantityPrices
            );
          this.useExtendedTimeRegistration = SettingsUtil.getBoolCompanySetting(
            settings,
            CompanySettingType.ProjectUseExtendedTimeRegistration
          );
          this.productGuaranteeId = SettingsUtil.getIntCompanySetting(
            settings,
            CompanySettingType.ProductGuarantee
          );
          this.attestStateReadyId = SettingsUtil.getIntCompanySetting(
            settings,
            CompanySettingType.BillingStatusOrderReadyMobile
          );
          this.usePartialInvoicingOnOrderRow =
            SettingsUtil.getBoolCompanySetting(
              settings,
              CompanySettingType.BillingUsePartialInvoicingOnOrderRow
            );

          if (
            this.attestStateTransferredOrderToInvoiceId !== 0 &&
            !this.excludedAttestStates.includes(
              this.attestStateTransferredOrderToInvoiceId
            )
          )
            this.excludedAttestStates.push(
              this.attestStateTransferredOrderToInvoiceId
            );

          if (
            this.attestStateTransferredOrderToContractId !== 0 &&
            !this.excludedAttestStates.includes(
              this.attestStateTransferredOrderToContractId
            )
          )
            this.excludedAttestStates.push(
              this.attestStateTransferredOrderToContractId
            );
        })
      );
  }

  override loadTerms(): Observable<TermCollection> {
    return this.translate
      .get([
        'core.yes',
        'core.no',
        'core.continue',
        'core.warning',
        'core.verifyquestion',
        'billing.project.project',
        'common.quantity',
        'common.customer',
        'common.customer.invoices.ordernr',
        'common.customer.invoices.edi',
        'common.customer.invoices.productnr',
        'common.customer.invoices.productname',
        'common.customer.invoices.quantity',
        'common.customer.invoices.unit',
        'common.customer.invoices.price',
        'common.customer.invoices.discount',
        'billing.order.discounttype',
        'common.customer.invoices.sum',
        'billing.productrows.purchaseprice',
        'billing.productrows.purchasepricesum',
        'billing.productrows.marginalincome.short',
        'billing.productrows.marginalincomeratio.short',
        'core.transfertopreliminaryinvoice',
        'billing.contract.transfertopreliminaryandmerge',
        'core.transfertoinvoiceandprint',
        'billing.productrows.changeatteststate',
        'billing.project.project',
        'common.order',
        'billing.productrows.rownr',
        'common.date',
        'common.customer.invoice.willtransferorder2invoice',
        'billing.project.timesheet.employeenr',
        'common.employee',
        'common.yearweek',
        'common.weekday',
        'common.time.timedeviationcause',
        'billing.project.timesheet.chargingtype',
        'billing.project.timesheet.workedtime',
        'billing.project.timesheet.invoicedtime',
        'billing.productrows.functions.showconnectedtimerows',
        'billing.productrows.changeatteststate.errorlift',
        'billing.productrows.changeatteststate.errorstock',
        'common.customer.invoices.row',
        'common.customer.invoices.wrongstatetotransfer',
        'billing.productrows.changeatteststate.errortitle',
        'billing.order.invalidchangestatesingle',
        'billing.order.invalidchangestatesmultiple',
        'billing.order.validchangestatesingle',
        'billing.order.validchangestatemultiple',
        'billing.order.allselectedinvalid',
        'billing.order.changeatteststatefailed',
        'common.customer.invoices.amount',
        'common.customer.invoices.amountexvat',
        'common.customer.invoices.foreignamount',
        'common.customer.invoices.amounttoinvoice',
        'common.customer.invoices.currencyamounttotransfer',
        'billing.order.splittimerows',
        'billing.order.validsplitsingle',
        'billing.order.validsplitmultiple',
        'billing.order.invalidsplitstatesingle',
        'billing.order.invalidsplitstatemultiple',
        'billing.order.invalidstatefortransfertoinvoice',
        'billing.order.transferallrowsinfo',
        'billing.order.origindescription',
      ])
      .pipe(
        tap(terms => {
          this.terms = terms;
        })
      );
  }

  loadUserAttestTransitions() {
    return this.manageService
      .getUserAttestTransitions(TermGroup_AttestEntity.Order, 0, 0)
      .pipe(
        tap(result => {
          this.attestTransitions = result;
          // Add states from returned transitions
          this.attestTransitions.forEach(t => {
            if (
              !this.attestStates.find(
                a => a.attestStateId === t.attestStateToId
              )
            )
              this.attestStates.push(t.attestStateTo);
          });

          // Sort states
          this.attestStates = this.attestStates.sort(a => a.sort);

          // Get initial state

          const initAttestSts = this.attestStates.find(a => a.initial === true);
          if (initAttestSts) {
            this.initialAttestState = initAttestSts;
          } else {
            this.loadInitialAttestState().subscribe();
          }

          // Setup available states (exclude finished states)
          this.availableAttestStates = [];
          this.attestStates.forEach(attestState => {
            if (
              !this.excludedAttestStates.find(
                ex => ex === attestState.attestStateId
              )
            ) {
              this.availableAttestStates.push(attestState);
            }
          });

          // Setup available states for selector
          this.availableAttestStateOptions = [];
          this.availableAttestStateOptions = [
            {
              id: 0,
              name: this.terms['billing.productrows.changeatteststate'],
            },
            ...this.availableAttestStates.map(a => {
              return { id: a.attestStateId, name: a.name };
            }),
          ];
        })
      );
  }

  loadInitialAttestState(): Observable<IAttestStateDTO> {
    return this.coreService
      .getAttestStateInitial(TermGroup_AttestEntity.Order)
      .pipe(
        tap(x => {
          this.initialAttestState = x;

          if (!this.initialAttestState) {
            this.messageboxService.error(
              this.terms['billing.productrows.initialstatemissing.title'],
              this.terms['billing.productrows.initialstatemissing.message']
            );
          } else {
            this.attestStates.push(this.initialAttestState);
            // Sort states
            this.attestStates = this.attestStates.sort(
              (a, b) => a.sort - b.sort
            );
          }
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<HandleBillingRowDTO>) {
    super.onGridReadyToDefine(grid);

    // Details
    const detailColumns: ColDef[] = [];

    const timeColumnOptions = {
      enableHiding: true,
      enableRowGrouping: true,
      clearZero: false,
      alignLeft: false,
      minDigits: 5,
      cellClassRules: {
        excelTime: () => true,
      },
    };

    const timePayrollColumnOptions = {
      enableHiding: true,
      clearZero: false,
      alignLeft: false,
      enableRowGrouping: true,
      minDigits: 5,
      cellClassRules: {
        errorRow: (gridRow: any) =>
          gridRow.data &&
          gridRow.data.timePayrollQuantity < gridRow.data.scheduledQuantity,
        excelTime: () => true,
      },
    };

    detailColumns.push(
      ColumnUtil.createColumnText(
        'employeeNr',
        this.terms['billing.project.timesheet.employeenr'],
        { flex: 1, enableHiding: true, hide: true }
      )
    );
    detailColumns.push(
      ColumnUtil.createColumnText(
        'employeeName',
        this.terms['common.employee'],
        {
          flex: 1,
          enableHiding: true,
          hide: true,
          tooltipField: 'columnNameTooltip',
          cellClassRules: {
            errorRow: (row: any) =>
              row && row.data && row.data.employeeIsInactive,
          },
        }
      )
    );
    detailColumns.push(
      ColumnUtil.createColumnDate('date', this.terms['common.date'], {
        tooltipField: 'dateFormatted',
        cellClassRules: { excelDate: () => true },
        flex: 1,
      })
    );
    detailColumns.push(
      ColumnUtil.createColumnText('yearWeek', this.terms['common.yearweek'], {
        flex: 1,
        enableHiding: true,
        hide: true,
      })
    );
    detailColumns.push(
      ColumnUtil.createColumnText('weekDay', this.terms['common.weekday'], {
        flex: 1,
        enableHiding: true,
      })
    );
    if (this.useExtendedTimeRegistration) {
      detailColumns.push(
        ColumnUtil.createColumnText(
          'timeDeviationCauseName',
          this.terms['common.time.timedeviationcause'],
          { enableHiding: true, flex: 1 }
        )
      );
    }
    detailColumns.push(
      ColumnUtil.createColumnText(
        'timeCodeName',
        this.terms['billing.project.timesheet.chargingtype'],
        { enableHiding: true, flex: 1 }
      )
    );
    const quantityCol = ColumnUtil.createColumnText(
      'guantityFormatted',
      this.terms['common.quantity'],
      { enableHiding: false, flex: 1 }
    );
    quantityCol.cellClass = 'text-right';
    quantityCol.cellStyle = { 'padding-right': '5px' };
    detailColumns.push(quantityCol);

    if (this.hasWorkTimePermission) {
      detailColumns.push(
        ColumnUtil.createColumnTimeSpan(
          'timePayrollQuantityFormatted',
          this.terms['billing.project.timesheet.workedtime'],
          timeColumnOptions
        )
      );
      detailColumns.push(
        ColumnUtil.createColumnShape('timePayrollAttestStateColor', '', {
          width: 50,
          alignCenter: true,
          enableHiding: false,
          shape: 'circle',
          colorField: 'timePayrollAttestStateColor',
          tooltipField: 'timePayrollAttestStateName',
        })
      );
    }

    if (this.hasInvoiceTimePermission) {
      detailColumns.push(
        ColumnUtil.createColumnTimeSpan(
          'invoiceQuantityFormatted',
          this.terms['billing.project.timesheet.invoicedtime'],
          timeColumnOptions
        )
      );
      detailColumns.push(
        ColumnUtil.createColumnShape('customerInvoiceRowAttestStateColor', '', {
          width: 50,
          alignCenter: true,
          enableHiding: false,
          shape: 'circle',
          colorField: 'customerInvoiceRowAttestStateColor',
          tooltipField: 'customerInvoiceRowAttestStateName',
        })
      );
    }

    detailColumns.push(
      ColumnUtil.createColumnNumber(
        'amount',
        this.terms['common.customer.invoices.amount'],
        {
          enableHiding: false,
          decimals: 2,
        }
      )
    );
    detailColumns.push(
      ColumnUtil.createColumnNumber(
        'amountExVat',
        this.terms['common.customer.invoices.amountexvat'],
        {
          enableHiding: false,
          decimals: 2,
        }
      )
    );

    if (this.hasCurrencyPermission) {
      detailColumns.push(
        ColumnUtil.createColumnNumber(
          'amountCurrency',
          this.terms['common.customer.invoices.foreignamount'],
          {
            enableHiding: false,
            decimals: 2,
          }
        )
      );
    }

    detailColumns.push(
      ColumnUtil.createColumnShape('payrollAttestStateAttestStateColor', '', {
        width: 50,
        alignCenter: true,
        enableHiding: false,
        shape: 'circle',
        colorField: 'payrollAttestStateAttestStateColor',
        tooltipField: 'payrollAttestStateAttestStateName',
      })
    );
    detailColumns.push(
      ColumnUtil.createColumnNumber(
        'invoicedAmount',
        this.terms['common.customer.invoices.amounttoinvoice'],
        {
          enableHiding: false,
          decimals: 2,
          cellClassRules: {
            'text-right': () => true,
            errorRow: (gridRow: any) =>
              gridRow.data.invoicedAmount > 0 &&
              gridRow.data.invoicedAmount < gridRow.data.amountExVat,
          },
        }
      )
    );

    if (this.hasCurrencyPermission) {
      ColumnUtil.createColumnNumber(
        'invoicedAmountCurrency',
        this.terms['common.customer.invoices.currencyamounttotransfer'],
        {
          enableHiding: false,
          decimals: 2,
          cellClassRules: {
            'text-right': () => true,
            errorRow: (gridRow: any) =>
              gridRow.data.invoicedAmountCurrency > 0 &&
              gridRow.data.invoicedAmountCurrency < gridRow.data.amountCurrency,
          },
        }
      );
    }

    detailColumns.push(
      ColumnUtil.createColumnIcon('noteIcon', '', {
        useIconFromField: true,
        width: 50,
        enableHiding: false,
        editable: false,
        showIcon: (row: any) => Boolean(row && row.data.noteIcon),
        onClick: this.showNote.bind(this),
      })
    );

    this.grid.enableMasterDetail(
      {
        detailRowHeight: 200,
        isRowMaster: (row: HandleBillingRowDTO) => {
          return (
            row.isTimeProjectRow ||
            row.productRowType === SoeProductRowType.ExpenseRow
          );
        },
        columnDefs: detailColumns,
      },
      {
        autoHeight: false,
        getDetailRowData: this.loadProjectTimeBlocks.bind(this),

        /*getDetailRowData: (params: any) => {
          if (params.data.isTimeProjectRow) {
            // Hide expense columns
            this.grid.detailOptions.hideColumn('guantityFormatted');
            this.grid.detailOptions.hideColumn('timeCodeName');
            this.grid.detailOptions.hideColumn('from');
            this.grid.detailOptions.hideColumn('amount');
            this.grid.detailOptions.hideColumn('amountExVat');
            this.grid.detailOptions.hideColumn('amountCurrency');
            this.grid.detailOptions.hideColumn('payrollAttestStateColor');
            this.grid.detailOptions.hideColumn('invoicedAmount');
            this.grid.detailOptions.hideColumn('invoicedAmountCurrency');
          } else if (
            params.data.productRowType === SoeProductRowType.ExpenseRow
          ) {
            this.grid.detailOptions.hideColumn('date');
            this.grid.detailOptions.hideColumn('yearWeek');
            this.grid.detailOptions.hideColumn('weekDay');
            this.grid.detailOptions.hideColumn('timeDeviationCauseName');
            this.grid.detailOptions.hideColumn(
              'timePayrollQuantityFormatted'
            );
            this.grid.detailOptions.hideColumn('timePayrollAttestStateColor');
            this.grid.detailOptions.hideColumn('invoiceQuantityFormatted');
            this.grid.detailOptions.hideColumn(
              'customerInvoiceRowAttestStateColor'
            );
            this.grid.detailOptions.hideColumn('noteIcon');
          }
          // Load detail rows
          return this.loadProjectTimeBlocks(params);
        },*/
      }
    );

    // Main grid
    this.grid.enableRowSelection();

    this.grid.addColumnNumber(
      'rowNr',
      this.terms['billing.productrows.rownr'],
      {
        pinned: 'left',
        width: 20,
        enableHiding: false,
        editable: false,
      }
    );
    this.grid.addColumnIcon('rowTypeIcon', '', {
      useIconFromField: true,
      pinned: 'left',
      width: 50,
      enableHiding: false,
      editable: false,
      showIcon: row => Boolean(row && row.rowTypeIcon),
    });
    this.grid.addColumnText(
      'invoiceNr',
      this.terms['common.customer.invoices.ordernr'],
      {
        width: 100,
        enableGrouping: true,
        pinned: 'left',
        enableHiding: false,
        editable: false,
        buttonConfiguration: {
          iconPrefix: 'fal',
          iconClass: 'iconEdit',
          iconName: 'pencil',
          onClick: row => this.openOrder(row),
          show: row =>
            Boolean(row && row.invoiceId && this.hasEditOrderPermission),
        },
      }
    );
    this.grid.addColumnText(
      'description',
      this.terms['billing.order.origindescription'],
      {
        pinned: 'left',
        width: 200,
        enableGrouping: true,
        enableHiding: true,
        hide: true,
      }
    );
    this.grid.addColumnText('project', this.terms['billing.project.project'], {
      pinned: 'left',
      width: 200,
      enableGrouping: true,
      enableHiding: true,
      buttonConfiguration: {
        iconPrefix: 'fal',
        iconName: 'pencil',
        show: row =>
          Boolean(row && row.projectId && this.hasEditProjectPermission),
        onClick: this.openProject.bind(this),
      },
    });
    this.grid.addColumnText('customer', this.terms['common.customer'], {
      pinned: 'left',
      width: 200,
      enableGrouping: true,
      enableHiding: true,
    });

    this.grid.addColumnSingleValue();

    this.grid.addColumnText(
      'ediTextValue',
      this.terms['common.customer.invoices.edi'],
      { enableGrouping: true, enableHiding: true }
    );
    this.grid.addColumnText(
      'productNr',
      this.terms['common.customer.invoices.productnr'],
      { enableGrouping: true, enableHiding: true }
    );
    this.grid.addColumnText(
      'text',
      this.terms['common.customer.invoices.productname'],
      { enableGrouping: true, enableHiding: true }
    );
    this.grid.addColumnNumber(
      'quantity',
      this.terms['common.customer.invoices.quantity'],
      { enableHiding: true, aggFuncOnGrouping: 'sum' }
    );
    this.grid.addColumnText(
      'productUnitCode',
      this.terms['common.customer.invoices.unit'],
      { enableHiding: true }
    );

    if (this.hasSalesPricePermission) {
      this.grid.addColumnNumber(
        'amountCurrency',
        this.terms['common.customer.invoices.price'],
        {
          enableHiding: true,
          decimals: 2,
          maxDecimals: 4,
          aggFuncOnGrouping: 'sum',
        }
      );
      this.grid.addColumnNumber(
        'discountValue',
        this.terms['common.customer.invoices.discount'],
        { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' }
      );
      this.grid.addColumnText(
        'discountTypeText',
        this.terms['billing.order.discounttype'],
        { enableHiding: true }
      );
      this.grid.addColumnNumber(
        'sumAmountCurrency',
        this.terms['common.customer.invoices.sum'],
        { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' }
      );
    }

    if (this.hasPurchasePricePermission) {
      this.grid.addColumnNumber(
        'purchasePriceCurrency',
        this.terms['billing.productrows.purchaseprice'],
        {
          enableHiding: true,
          decimals: 2,
          maxDecimals: 4,
          aggFuncOnGrouping: 'sum',
        }
      );
      this.grid.addColumnNumber(
        'purchasePriceSum',
        this.terms['billing.productrows.purchasepricesum'],
        { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' }
      );

      if (this.hasSalesPricePermission) {
        this.grid.addColumnNumber(
          'marginalIncomeCurrency',
          this.terms['billing.productrows.marginalincome.short'],
          {
            enableHiding: true,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
            cellClassRules: {
              'text-right': () => true,
              errorRow: (gridRow: any) =>
                Boolean(
                  gridRow &&
                    gridRow.data &&
                    gridRow.data.marginalIncomeLimit &&
                    gridRow.data.marginalIncomeLimit < 0 &&
                    gridRow.data.sumAmountCurrency > 0
                ),
              deleted: () => false,
            },
          }
        );
        this.grid.addColumnNumber(
          'marginalIncomeRatio',
          this.terms['billing.productrows.marginalincomeratio.short'],
          {
            enableHiding: true,
            decimals: 2,
            cellClassRules: {
              'text-right': () => true,
              errorRow: (gridRow: any) =>
                Boolean(
                  gridRow &&
                    gridRow.data &&
                    gridRow.data.marginalIncomeLimit &&
                    gridRow.data.marginalIncomeLimit < 0 &&
                    gridRow.data.sumAmountCurrency > 0
                ),
              deleted: () => false,
            },
          }
        );
        this.grid.addColumnDate('date', this.terms['common.date'], {
          width: 100,
          pinned: 'right',
          enableHiding: false,
        });
        this.grid.addColumnShape('attestStateName', '', {
          width: 50,
          alignCenter: true,
          enableHiding: false,
          shape: 'circle',
          colorField: 'attestStateColor',
          tooltipField: 'attestStateName',
          pinned: 'right',
        });
      }
    }

    const defs = this.grid?.api.getColumnDefs();
    defs?.forEach((colDef: any) => {
      if (
        colDef.field !== 'isModified' &&
        colDef.field !== 'rowNr' &&
        colDef.field !== 'text' &&
        colDef['soeType'] !== 'icon' &&
        colDef['soeType'] !== 'shape'
      ) {
        colDef['collapseOnTextRow'] = true;
        colDef['collapseOnPageBreakRow'] = true;
        if (colDef.field !== 'sumAmountCurrency')
          colDef['collapseOnSubTotalRow'] = true;
      }
    });

    this.grid.useGrouping({
      stickyGroupTotalRow: 'bottom',
      includeFooter: true,
      includeTotalFooter: true,
      groupSelectsFiltered: false,
      keepColumnsAfterGroup: false,
      selectChildren: false,
    });

    this.grid.setSingelValueConfiguration(
      [
        {
          field: 'text',
          predicate: (data: HandleBillingRowDTO) =>
            data && data.type === SoeInvoiceRowType.TextRow,
          editable: false,
          spanTo: this.getSingleValueSpan(),
        },
        {
          field: 'text',
          predicate: (data: HandleBillingRowDTO) =>
            data && data.type === SoeInvoiceRowType.TextRow,
          editable: false,
          spanTo: this.getSingleValueSpan(),
        },
        {
          field: 'text',
          predicate: (data: HandleBillingRowDTO) =>
            data ? data.type === SoeInvoiceRowType.SubTotalRow : false,
          editable: false,
          cellClass: 'bold',
          cellRenderer: (data, value) => {
            const sum = data['sumAmountCurrency'] || '';
            return (
              "<span class='pull-left' style='width:150px'>" +
              value +
              "</span><span class='pull-right' style='padding-left:5px;padding-right:2px;margin-right:-2px;background-color:#FFFFFF;'>" +
              NumberUtil.formatDecimal(sum, 2) +
              '</span>'
            );
          },
          spanTo: 'attestStateNames',
        },
      ],
      true
    );

    super.finalizeInitGrid();
  }

  private getSingleValueSpan(): string {
    if (this.hasPurchasePricePermission) {
      return this.hasSalesPricePermission
        ? 'marginalIncomeRatio'
        : 'purchasePriceSum';
    } else {
      return this.hasSalesPricePermission
        ? 'sumAmountCurrency'
        : 'productUnitCode';
    }
  }

  private showNote(row: any) {}

  openOrder(row: any) {
    console.log('openOrder', row);
  }

  openProject(row: any) {
    console.log('openProject', row);
  }

  search(event: SearchCustomerInvoiceRowModel): void {
    this.searchModel = event;
    this.refreshGrid();
  }

  override loadData(
    id?: number | undefined
  ): Observable<HandleBillingRowDTO[]> {
    if (!this.searchModel) return of([]);

    return this.performGridLoad.load$(
      this.service
        .getGrid(undefined, {
          model: this.searchModel,
        })
        .pipe(
          tap(data => {
            data.forEach(row => {
              row.ediTextValue = row.ediEntryId
                ? this.terms['core.yes']
                : this.terms['core.no'];

              if (row.discountType === SoeInvoiceRowDiscountType.Percent) {
                row.discountTypeText = '%';
                row.discountValue = row.discountPercent;
              } else {
                row.discountTypeText = row.currencyCode;
                row.discountValue = row.discountAmount;
              }

              if (row.productRowType === SoeProductRowType.TimeBillingRow) {
                row.rowTypeIcon = 'file-invoice-dollar';
              } else if (row.isTimeProjectRow) {
                row.rowTypeIcon = 'clock';
              } else if (row.productRowType === SoeProductRowType.ExpenseRow) {
                row.rowTypeIcon = 'wallet';
              } else {
                switch (row.type) {
                  case SoeInvoiceRowType.ProductRow:
                    row.rowTypeIcon = 'box-alt';
                    break;
                  case SoeInvoiceRowType.TextRow:
                    row.rowTypeIcon = 'text';
                    break;
                  case SoeInvoiceRowType.PageBreakRow:
                    row.rowTypeIcon = 'cut';
                    break;
                  case SoeInvoiceRowType.SubTotalRow:
                    row.rowTypeIcon = 'calculator-alt';
                    break;
                }
              }
            });
            this.summarize(data);
          })
        )
    );
  }

  private loadProjectTimeBlocks(params: any) {
    if (!params.data['rowsLoaded']) {
      console.log(
        'load rows',
        params,
        params.data['rowsLoaded'],
        params.data.isTimeProjectRow
      );
      if (params.data.isTimeProjectRow) {
        this.service
          .getProjectTimeBlocksForInvoiceRow(
            params.data.invoiceId,
            params.data.customerInvoiceRowId
          )
          .pipe(
            tap((data: IProjectTimeBlockDTO[]) => {
              console.log('rows loaded', data);
              params.data['rows'] = data;
              params.data['rowsLoaded'] = true;
              return params.successCallback(data);
            })
          );
      } else if (params.data.productRowType === SoeProductRowType.ExpenseRow) {
        this.service
          .getExpenseRows(
            params.data.invoiceId,
            params.data.customerInvoiceRowId
          )
          .pipe(
            tap((data: IProjectTimeBlockDTO[]) => {
              data.forEach((r: any) => {
                if (
                  r.timeCodeRegistrationType ===
                  TermGroup_TimeCodeRegistrationType.Time
                )
                  r['guantityFormatted'] = DateUtil.minutesToTimeSpan(
                    r.quantity
                  );
                else
                  r['guantityFormatted'] = NumberUtil.formatDecimal(
                    r.quantity,
                    2
                  );
              });

              console.log('rows loaded', data);
              params.data['rows'] = data;
              params.data['rowsLoaded'] = true;
              return params.successCallback(data);
            })
          );
      }
    } else {
      return params.successCallback(params.data['rows']);
    }
  }

  selectionChanged(data: any) {
    this.noSelectedRows.set(data.length <= 0);
    this.summarize();
  }

  summarize(data?: IHandleBillingRowDTO[]) {
    this.filteredAmount = 0;
    this.filteredAmountValidForInvoice = 0;
    const rows = data ? data : this.grid.getFilteredRows();
    rows.forEach(row => {
      if (row.validForInvoice)
        this.filteredAmountValidForInvoice += row.sumAmountCurrency;
      this.filteredAmount += row.sumAmountCurrency;
    });
  }

  // control events
  transferFunctionSelected(item: MenuButtonItem) {
    /*switch (item.id) {
      case HouseholdDeductionGridButtonFunctions.SaveReceived: {
        this.saveReceived();
        break;
      }
      case HouseholdDeductionGridButtonFunctions.SavePartiallyApproved: {
        this.savePartiallyApproved();
        break;
      }
      case HouseholdDeductionGridButtonFunctions.SaveDenied: {
        this.saveDenied();
        break;
      }
    }*/
  }
}
