import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { SOEMessageBoxImage, SOEMessageBoxButtons, HouseholdDeductionGridButtonFunctions, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { SetSequenceNumberController } from "./Dialogs/SetSequenceNumber/SetSequenceNumberController";
import { SoeHouseholdClassificationGroup, Feature, CompanySettingType, SettingMainType, SoeReportTemplateType, SoeReportType, SoeEntityType, TermGroup, TermGroup_HouseHoldTaxDeductionType, UserSettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IActionResult, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { IInvoiceService, InvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { EditHouseholdDataController } from "./Dialogs/EditHouseholdData/EditHouseholdData";
import { EditHouseholdFileController } from "./Dialogs/EditHouseholdFile/EditHouseholdFile";
import { ApprovedAmountController } from "./Dialogs/ApprovedAmount/ApprovedAmountController";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";
import { HouseholdTaxDeductionPrintDTO } from "../../../Common/Models/RequestReports/HouseholdTaxDeductionPrintDTO";
import { IColumnAggregations } from "../../../Util/SoeGridOptionsAg";
import { HouseholdTaxDeductionFileRowDTO } from "../../../Common/Models/HouseholdTaxDeductionFileRowDTOs";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    bulkDate: Date;
    classification: SoeHouseholdClassificationGroup;

    applyText: string;
    appliedText: string;

    filteredTotal = 0;
    selectedTotal = 0;

    // Collections
    private householdRows: any[];
    private filteredHouseholdRows: any[];
    private taxDeductionTypes: ISmallGenericType[];

    //Terms
    terms: { [index: string]: string };

    // Filter properties
    private appliedDateFrom: Date = null;
    private appliedDateTo: Date = null;
    private receivedDateFrom: Date = null;
    private receivedDateTo: Date = null;
    private deniedDateFrom: Date = null;
    private deniedDateTo: Date = null;
    private paidDateFrom: Date = null;
    private paidDateTo: Date = null;

    // Permissions
    private reportPermission: boolean;
    private rotPermission: boolean;
    private rutPermission: boolean;

    // Settings
    private householdReportTemplateId: number; //Used for both rut and rot
    private defaultPrintButtonOption: number;
    private householdTaxDeductionProductId = 0;
    private householdTaxDeductionDeniedProductId = 0;
    private household50TaxDeductionProductId = 0;

    private _selectedTaxDeductionType = TermGroup_HouseHoldTaxDeductionType.None;

    private get selectedTaxDeductionType(): TermGroup_HouseHoldTaxDeductionType {
        return this._selectedTaxDeductionType;
    }
    private set selectedTaxDeductionType(value: TermGroup_HouseHoldTaxDeductionType) {
        if (value != this._selectedTaxDeductionType) {
            this._selectedTaxDeductionType = value;
            this.updateTaxDeductionTypeSetting();

            if (this.isApplied)
                this.setSplitButtonFunctions();
        }
    }

    // Functions
    private saveFunctions: any = [];
    private printFunctions: any = [];
    private selectedPrintOption: any;

    //Statuses
    appliedStatuses: any = [];
    selectedStatuses: any = [];
    selectedTypes: any = [];

    // Grid header and footer
    gridFooterComponentUrl: any;

    //modal
    private modalInstance: any;

    //Activation
    private activated = false;

    get isApply() {
        return this.classification === SoeHouseholdClassificationGroup.Apply;
    }

    get isApplied() {
        return this.classification === SoeHouseholdClassificationGroup.Applied;
    }

    get isReceived() {
        return this.classification === SoeHouseholdClassificationGroup.Received;
    }

    get hasSelectedRows() {
        return this.gridAg.options.getSelectedRows().length > 0;
    }

    get oneValidRowSelected() {
        const selectedRows = this.gridAg.options.getSelectedRows();
        return selectedRows.length === 1 && selectedRows[0].amount === selectedRows[0].approvedAmount;
    }

    //@ngInject
    constructor(
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        $uibModal,
        private invoiceService: IInvoiceService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private readonly requestReportService: IRequestReportService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService) {
        super(gridHandlerFactory, "Billing.Invoices.HouseholdDeduction", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onBeforeSetUpGrid(() => this.onBeforeGridSetup())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x));

        this.gridFooterComponentUrl = urlHelperService.getViewUrl("gridFooter.html");

        this.modalInstance = $uibModal;
        
        this.onTabActivated(() => this.onLocalTabActivated() )
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;
        this.classification = parameters.classification;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired( () => { this.loadGridData(); });
        }

        if (this.classification === SoeHouseholdClassificationGroup.Apply || this.classification === SoeHouseholdClassificationGroup.Applied)
            this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");
    }

    private onLocalTabActivated() {
        if (!this.activated) {
            this.activated = true;
            this.flowHandler.start(
                [
                    { feature: Feature.Billing_Invoice_Household_ROT, loadModifyPermissions: true, loadReadPermissions: true },
                    { feature: Feature.Billing_Invoice_RUT, loadModifyPermissions: true, loadReadPermissions: true },
                    { feature: Feature.Billing_Distribution_Reports_Selection_Download, loadModifyPermissions: true, loadReadPermissions: true },
                    { feature: Feature.Billing_Distribution_Reports_Selection, loadModifyPermissions: true, loadReadPermissions: true }
               ]
            );
        }
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.rotPermission = response[Feature.Billing_Invoice_Household_ROT].modifyPermission;
        this.rutPermission = response[Feature.Billing_Invoice_RUT].modifyPermission;

        this.readPermission = this.modifyPermission = this.rotPermission;
        this.reportPermission = response[Feature.Billing_Distribution_Reports_Selection].readPermission && response[Feature.Billing_Distribution_Reports_Selection_Download].readPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());

        if (this.classification === SoeHouseholdClassificationGroup.All)
            this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeaderAll.html"));
        else
            this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
    }

    private onBeforeGridSetup(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadHouseHoldTaxTypes(), 
            this.loadCompanySettings(),
        ]).then( () => {
            return this.loadUserSettings();
        });
    }

    private setSplitButtonFunctions() {
        this.saveFunctions = [];
        this.printFunctions = [];

        const keys: string[] = [
            "billing.invoices.householddeduction.approve",
            "billing.invoices.householddeduction.deny",
            "billing.invoices.householddeduction.savexml",
            "common.printreport",
            "billing.invoices.householddeduction.approvepartial",
            "billing.invoices.householddeduction.withdraw",
            "billing.invoices.householddeduction.editandsavexml"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.saveFunctions.push({ id: HouseholdDeductionGridButtonFunctions.SaveReceived, name: terms["billing.invoices.householddeduction.approve"], icon: 'fal fa-fw fa-save' });
            this.saveFunctions.push({ id: HouseholdDeductionGridButtonFunctions.SavePartiallyApproved, name: terms["billing.invoices.householddeduction.approvepartial"], icon: 'fal fa-fw fa-save', disabled: () => { return !this.gridAg.options.getSelectedRows() || this.gridAg.options.getSelectedRows().length > 1 } });
            this.saveFunctions.push({ id: HouseholdDeductionGridButtonFunctions.SaveDenied, name: terms["billing.invoices.householddeduction.deny"], icon: 'fal fa-fw fa-save', disabled: () => { return !this.gridAg.options.getSelectedRows() || this.gridAg.options.getSelectedRows().length > 1 } });
            this.saveFunctions.push({ id: HouseholdDeductionGridButtonFunctions.WithdrawApplied, name: terms["billing.invoices.householddeduction.withdraw"], icon: 'fal fa-fw fa-save' });

            
            this.printFunctions.push({ id: HouseholdDeductionGridButtonFunctions.Print, name: terms["common.printreport"], icon: 'fal fa-fw fa-print' });
            this.printFunctions.push({ id: HouseholdDeductionGridButtonFunctions.SaveXML, name: terms["billing.invoices.householddeduction.savexml"], icon: 'fal fa-fw fa-print' }); 
            this.printFunctions.push({ id: HouseholdDeductionGridButtonFunctions.EditAndSaveXML, name: terms["billing.invoices.householddeduction.editandsavexml"], icon: 'fal fa-fw fa-print' });
        });
    }

    
    // Lookups
    private loadTerms(): ng.IPromise<any> {
        this.appliedStatuses = [];
        const keys: string[] = [
            "billing.invoices.householddeduction.applied",
            "billing.invoices.householddeduction.unpaid",
            "billing.invoices.householddeduction.paid",
            "billing.invoices.householddeduction.approved",
            "billing.invoices.householddeduction.partiallyapproved",
            "billing.invoices.householddeduction.denied",
            "common.customerinvoice",
            "billing.invoices.householddeduction.voucher"
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.appliedStatuses.push({ id: 1, label: terms["billing.invoices.householddeduction.unpaid"] });
            this.appliedStatuses.push({ id: 2, label: terms["billing.invoices.householddeduction.paid"] });
            this.appliedStatuses.push({ id: 3, label: terms["billing.invoices.householddeduction.applied"] });
            this.appliedStatuses.push({ id: 4, label: terms["billing.invoices.householddeduction.approved"] });
            this.appliedStatuses.push({ id: 6, label: terms["billing.invoices.householddeduction.partiallyapproved"] });
            this.appliedStatuses.push({ id: 5, label: terms["billing.invoices.householddeduction.denied"] });
        });
    }

    private getDefaultHouseholdReport() {
        return this.reportService.getSettingOrStandardReportId(SettingMainType.Company, CompanySettingType.BillingDefaultHouseholdDeductionTemplate, SoeReportTemplateType.HousholdTaxDeduction, SoeReportType.CrystalReport).then((x) => {
            this.householdReportTemplateId = x;
        });
    }

    private loadHouseHoldTaxTypes(): ng.IPromise<any> {
        const deferral = this.$q.defer<any>();
        this.coreService.getTermGroupContent(TermGroup.HouseHoldTaxDeductionType, false, true).then(x => {
            this.taxDeductionTypes = x;
            if (!this.rotPermission) {
                this.taxDeductionTypes = this.taxDeductionTypes.filter(y => y.id != TermGroup_HouseHoldTaxDeductionType.ROT && y.id != TermGroup_HouseHoldTaxDeductionType.GREEN);
            }
            if (!this.rutPermission) {
                this.taxDeductionTypes = this.taxDeductionTypes.filter(y => y.id != TermGroup_HouseHoldTaxDeductionType.RUT);
            }
            
            deferral.resolve();
            
        });
        return deferral.promise;
    }

    private setupGrid() {

        //this.gridAg.options.setName("Billing.Invoices.HouseholdDeduction" + "_" + this.classification);

        // Columns
        const keys: string[] = [
            "billing.invoices.householddeduction.invoicenr",
            "billing.invoices.householddeduction.property",
            "billing.invoices.householddeduction.socialsecnr",
            "billing.invoices.householddeduction.name",
            "billing.invoices.householddeduction.amount",
            "billing.invoices.householddeduction.seqnbr",
            "billing.invoices.householddeduction.applieddate",
            "billing.invoices.householddeduction.receiveddate",
            "billing.invoices.householddeduction.denieddate",
            "billing.invoices.householddeduction.status",
            "billing.invoices.householddeduction.approved",
            "core.edit",
            "core.delete",
            "common.customer.invoices.showinvoice",
            "common.type",
            "common.percent",
            "common.customer.invoices.paydate",
            "billing.invoices.householddeduction.editinfo",
            "billing.invoices.householddeduction.approvedamount",
            "common.report.selection.vouchernr",
            "common.customer.customer.rot.comment",
            "billing.invoices.householddeduction.withdraw"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            //this.gridAg.addColumnText("houseHoldTaxDeductionTypeName", terms["common.type"], null);
            this.gridAg.addColumnSelect("houseHoldTaxDeductionTypeName", terms["common.type"], null, { displayField: "houseHoldTaxDeductionTypeName", selectOptions: null, populateFilterFromGrid: true, enableRowGrouping: true } );
            this.gridAg.addColumnText("invoiceNr", terms["billing.invoices.householddeduction.invoicenr"], null, null, { enableRowGrouping: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (r) => r && r.invoiceId, callback: this.edit.bind(this) } });
            if (this.classification === SoeHouseholdClassificationGroup.Received || this.classification === SoeHouseholdClassificationGroup.Denied || this.classification === SoeHouseholdClassificationGroup.All)
                this.gridAg.addColumnText("voucherNr", terms["common.report.selection.vouchernr"], null, null, { enableRowGrouping: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (r) => r && r.voucherHeadId, callback: this.openVoucher.bind(this) } });
            this.gridAg.addColumnText("property", terms["billing.invoices.householddeduction.property"], null, null, { enableRowGrouping: true });
            this.gridAg.addColumnText("socialSecNr", terms["billing.invoices.householddeduction.socialsecnr"], null, null, { enableRowGrouping: true });
            this.gridAg.addColumnText("name", terms["billing.invoices.householddeduction.name"], null, null, { enableRowGrouping: true });
            this.gridAg.addColumnNumber("amount", terms["billing.invoices.householddeduction.amount"], null, { enableHiding: false, decimals: 2, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });

            let exportKey = "";
            switch (this.classification) {
                case SoeHouseholdClassificationGroup.Apply:
                    exportKey = "billing.invoices.householddeduction.applyrot";

                    this.gridAg.addColumnDate("payDate", terms["common.customer.invoices.paydate"], null, true);
                    this.gridAg.addColumnIcon(null, "...", null, { icon: "fal fa-user-edit", suppressFilter: true, onClick: this.editDeductionInfo.bind(this), toolTip: terms["billing.invoices.householddeduction.editinfo"] });
                    this.gridAg.addColumnIcon(null, "...", null, { icon: "fal fa-file-text", suppressFilter: true, showIcon: (row) => row.comment && row.comment.length > 0, onClick: this.showComment.bind(this), toolTip: terms["common.customer.customer.rot.comment"] });
                    this.gridAg.addColumnDelete(terms["core.delete"], this.initDeleteRow.bind(this));
                    break;
                case SoeHouseholdClassificationGroup.Applied:
                    exportKey = "billing.invoices.householddeduction.appliedmany";

                    this.gridAg.addColumnText("seqNr", terms["billing.invoices.householddeduction.seqnbr"], null);
                    this.gridAg.addColumnDate("payDate", terms["common.customer.invoices.paydate"], null, true);
                    this.gridAg.addColumnDate("appliedDate", terms["billing.invoices.householddeduction.applieddate"], null);
                    this.gridAg.addColumnText("houseHoldTaxDeductionPercent", terms["common.percent"], null);
                    this.gridAg.addColumnIcon(null, "...", null, { icon: "fal fa-user-edit", suppressFilter: true, onClick: this.editDeductionInfo.bind(this), toolTip: terms["billing.invoices.householddeduction.editinfo"] });
                    this.gridAg.addColumnIcon(null, "...", null, { icon: "fal fa-leaf okColor", suppressFilter: true, showIcon: (row) => row.houseHoldTaxDeductionType === TermGroup_HouseHoldTaxDeductionType.GREEN, onClick: this.showGreenApplyInfo.bind(this) });
                    this.gridAg.addColumnIcon(null, "...", null, { icon: "fal fa-file-text", suppressFilter: true, showIcon: (row) => row.comment && row.comment.length > 0, onClick: this.showComment.bind(this), toolTip: terms["common.customer.customer.rot.comment"] });
                    this.gridAg.addColumnDelete(terms["core.delete"], this.initDeleteRow.bind(this));

                    this.selectedPrintOption = _.find(this.printFunctions, (x) => x.id === this.defaultPrintButtonOption);
                    break;
                case SoeHouseholdClassificationGroup.Received:
                    exportKey = "billing.invoices.householddeduction.recieved";
                    
                    this.gridAg.addColumnText("seqNr", terms["billing.invoices.householddeduction.seqnbr"], null);
                    this.gridAg.addColumnDate("payDate", terms["common.customer.invoices.paydate"], null, true);
                    this.gridAg.addColumnDate("appliedDate", terms["billing.invoices.householddeduction.applieddate"], null);
                    this.gridAg.addColumnDate("receivedDate", terms["billing.invoices.householddeduction.approved"], null);
                    this.gridAg.addColumnText("houseHoldTaxDeductionPercent", terms["common.percent"], null);
                    this.gridAg.addColumnNumber("approvedAmount", terms["billing.invoices.householddeduction.approvedamount"], null, { enableHiding: false, decimals: 2 });
                    this.gridAg.addColumnIcon(null, "...", null, { icon: "fal fa-file-text", suppressFilter: true, showIcon: (row) => row.comment && row.comment.length > 0, onClick: this.showComment.bind(this), toolTip: terms["common.customer.customer.rot.comment"] });
                    this.gridAg.addColumnIcon("statusIcon", "...", null, { suppressFilter: true, toolTipField: "householdStatus" });
                    //this.gridAg.addColumnIcon("", "...", null, { showIcon: (row) => { return row['selectable'] }, icon: "far fa-arrow-left", toolTip: terms["billing.invoices.householddeduction.withdraw"], onClick: this.withdrawRecieved.bind(this) } );
                    break;
                case SoeHouseholdClassificationGroup.Denied:
                    exportKey = "billing.invoices.householddeduction.denied";

                    this.gridAg.addColumnText("seqNr", terms["billing.invoices.householddeduction.seqnbr"], null);
                    this.gridAg.addColumnDate("payDate", terms["common.customer.invoices.paydate"], null, true);
                    this.gridAg.addColumnDate("appliedDate", terms["billing.invoices.householddeduction.applieddate"], null);
                    this.gridAg.addColumnDate("deniedDate", terms["billing.invoices.householddeduction.denieddate"], null);
                    this.gridAg.addColumnText("houseHoldTaxDeductionPercent", terms["common.percent"], null);
                    this.gridAg.addColumnIcon(null, "...", null, { icon: "fal fa-file-text", suppressFilter: true, showIcon: (row) => row.comment && row.comment.length > 0, onClick: this.showComment.bind(this), toolTip: terms["common.customer.customer.rot.comment"] });
                    break;
                case SoeHouseholdClassificationGroup.All:
                    exportKey = "common.all";
                    this.gridAg.addColumnSelect("householdStatus", terms["billing.invoices.householddeduction.status"], null, { displayField: "householdStatus", selectOptions: null, populateFilterFromGrid: true, enableRowGrouping: true });
                    this.gridAg.addColumnText("seqNr", terms["billing.invoices.householddeduction.seqnbr"], null, null, { enableRowGrouping: true });
                    this.gridAg.addColumnDate("payDate", terms["common.customer.invoices.paydate"], null, true, null, { enableRowGrouping: true });
                    this.gridAg.addColumnDate("appliedDate", terms["billing.invoices.householddeduction.applieddate"], null, true, null, { enableRowGrouping: true });
                    this.gridAg.addColumnDate("receivedDate", terms["billing.invoices.householddeduction.approved"], null, true, null, { enableRowGrouping: true });
                    this.gridAg.addColumnDate("deniedDate", terms["billing.invoices.householddeduction.denieddate"], null, true, null, { enableRowGrouping: true });
                    this.gridAg.addColumnText("houseHoldTaxDeductionPercent", terms["common.percent"], null, null, { enableRowGrouping: true });
                    this.gridAg.addColumnNumber("approvedAmount", terms["billing.invoices.householddeduction.approvedamount"], null, { enableHiding: false, decimals: 2, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                    this.gridAg.addColumnIcon("statusIcon", "...", null, { toolTipField: "householdStatus", showTooltipFieldInFilter: true, enableHiding: false });

                    this.gridAg.options.useGrouping(true, true, { keepColumnsAfterGroup: false, selectChildren: true });
                    break;
            }

            const events: GridEvent[] = [];
            /*events.push(new GridEvent(SoeGridOptionsEvent.IsRowSelectable, (rowNode) => {
                return this.isReceived ? rowNode.data && rowNode.data['selectable'] : true;
            }));*/
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: any) => {
                this.summarizeSelected();
            }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: any) => {
                this.summarizeSelected();
            }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: any[]) => {
                this.summarizeFiltered(rows);
            }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid(exportKey, true);
        });
    }

    public edit(row) {
        this.messagingService.publish(Constants.EVENT_OPEN_INVOICE, {
            id: row.invoiceId,
            name: this.terms["common.customerinvoice"] + " " + row.invoiceNr
        });
    }

    private openVoucher(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_VOUCHER, {
            id: row.voucherHeadId,
            name: this.terms["billing.invoices.householddeduction.voucher"] + " " + row.voucherNr
        });
    }

    public loadGridData(checkCredit = false) {

        this.progress.startLoadingProgress([() => {
            // Load data
            return this.invoiceService.getHouseholdTaxDeductionRows(this.classification, this.selectedTaxDeductionType).then((x) => {
                let hasCreditInvoices = false;
                this.householdRows = x;
                this.householdRows.forEach( (y) => {
                    y.appliedDate = CalendarUtility.convertToDate(y.appliedDate);
                    y.deniedDate = CalendarUtility.convertToDate(y.deniedDate);
                    y.payDate = CalendarUtility.convertToDate(y.payDate);
                    y.receivedDate = CalendarUtility.convertToDate(y.receivedDate);
                    if (y.fullyPayed) {
                        if (y.applied) {
                            if (y.received) {
                                if (y.amount !== y.approvedAmount) {
                                    y.householdStatus = this.terms["billing.invoices.householddeduction.partiallyapproved"];
                                    y['status'] = 6;
                                    y['statusIcon'] = 'fas fa-circle warningColor';
                                    y['selectable'] = false;
                                }
                                else {
                                    y.householdStatus = this.terms["billing.invoices.householddeduction.approved"];
                                    y['status'] = 4;
                                    y['statusIcon'] = 'fas fa-circle okColor';
                                    y["selectable"] = true;
                                }
                            }
                            else if (y.denied) {
                                y.householdStatus = this.terms["billing.invoices.householddeduction.denied"];
                                y['status'] = 5;
                                y['statusIcon'] = 'fas fa-circle errorColor';
                            }
                            else {
                                y.householdStatus = this.terms["billing.invoices.householddeduction.applied"];
                                y['status'] = 3;
                                y['statusIcon'] = 'fas fa-circle infoColor';
                            }
                        }
                        else {
                            y.householdStatus = this.terms["billing.invoices.householddeduction.paid"];
                            y['status'] = 2;
                            y['statusIcon'] = 'fas fa-circle mediumGrayColor';
                        }
                    }
                    else {
                        y.householdStatus = this.terms["billing.invoices.householddeduction.unpaid"];
                        y['status'] = 1;
                        y['statusIcon'] = 'fas fa-circle mediumGrayColor';
                    }

                    if (y.amount < 0)
                        hasCreditInvoices = true;
                });

                this.summarizeFiltered(null);
                this.summarizeSelected();

                this.setData(this.householdRows);

                if ((this.classification === SoeHouseholdClassificationGroup.Apply) && hasCreditInvoices && checkCredit) {
                    const keys: string[] = [
                        "core.warning",
                        "billing.invoices.householddeduction.creditwarning"
                    ];
                    return this.translationService.translateMany(keys).then((terms) => {
                        this.notificationService.showDialog(terms["core.warning"], terms["billing.invoices.householddeduction.creditwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    });
                }
            });
        }]);
    }

    private executeSaveFunction(option) {
        switch (option.id) {
            case HouseholdDeductionGridButtonFunctions.SaveReceived:
                this.saveReceived();
                break;
            case HouseholdDeductionGridButtonFunctions.SavePartiallyApproved:
                this.initSavePartiallyApproved();
                break;
            case HouseholdDeductionGridButtonFunctions.SaveDenied:
                this.saveDenied();
                break;
            case HouseholdDeductionGridButtonFunctions.WithdrawApplied:
                this.withdrawApplied();
                break;
        }
    }

    private executePrintFunction(option) {
        switch (option.id) {
            case HouseholdDeductionGridButtonFunctions.Print:
                this.initPrintReport();
                break;
            case HouseholdDeductionGridButtonFunctions.SaveXML:
                this.initCreateFile(false);
                break;
            case HouseholdDeductionGridButtonFunctions.EditAndSaveXML:
                this.initCreateFile(true);
                break;
        }

        this.updatePrintSetting(option.id);
    }

    public initSaveApplied() {
        const dict: any = [];

        const rows = this.gridAg.options.getSelectedRows();
        if (_.find(rows, r => r.amount < 0)) {
            const keys: string[] = [
                "billing.invoices.householddeduction.cantapplyheader",
                "billing.invoices.householddeduction.cantapply"
            ];
            return this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["core.warning"], terms["billing.invoices.householddeduction.cantapply"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            });
        }
        else {
            rows.forEach( (y: any) => {
                if (y.customerInvoiceRowId > 0)
                    dict.push(y.customerInvoiceRowId);
            });

            this.saveApplied(dict);
        }
    }

    public saveApplied(dict: any[]) {
        this.progress.startSaveProgress((completion) => {
            this.invoiceService.saveHouseholdTaxApplied(dict, this.bulkDate).then((result) => {
                if (result.success) {
                    const keys: string[] = [
                        "billing.invoices.householddeduction.appliedsuccessheader",
                        "billing.invoices.householddeduction.appliedsuccess"
                    ];
                    return this.translationService.translateMany(keys).then((terms) => {
                        this.notificationService.showDialog(terms["billing.invoices.householddeduction.appliedsuccessheader"], terms["billing.invoices.householddeduction.appliedsuccess"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                        completion.completed(null, null, true);
                    });
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {
            });
    }

    public saveReceived() {

        const selectedRows = this.gridAg.options.getSelectedRows();

        this.checkSelectedRows(selectedRows, true).then((id) => {
            if (id) {
                const dict: any = [];
                const invoiceNrs: any[] = [];
                let amount = 0;

                selectedRows.forEach(y => {
                    if (y.customerInvoiceRowId > 0) {
                        dict.push(y.customerInvoiceRowId);
                        invoiceNrs.push(y.invoiceNr)
                        amount += y.amount;
                    }
                });

                this.progress.startSaveProgress((completion) => {
                    this.invoiceService.saveHouseholdTaxReceived(dict, this.bulkDate).then((result) => {
                        if (result.success) {
                            const keys: string[] = [
                                "billing.invoices.householddeduction.recievedsuccessheader",
                                "billing.invoices.householddeduction.recievedsuccess",
                                "economy.accounting.voucher.new"
                            ];
                            return this.translationService.translateMany(keys).then((terms) => {
                                const modal = this.notificationService.showDialog(terms["billing.invoices.householddeduction.recievedsuccessheader"], terms["billing.invoices.householddeduction.recievedsuccess"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
                                modal.result.then(val => {
                                    if (val != null && val === true) {
                                        const message = { entityType: SoeEntityType.Voucher, date: this.bulkDate, amount: amount, ids: dict, nbrs: invoiceNrs, productId: id };
                                        this.messagingService.publish(Constants.EVENT_OPEN_HOUSEHOLD, message);
                                    }
                                });
                                completion.completed(null, null, true);
                            });
                        }
                        else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null)
                    .then(data => {
                        this.loadGridData();
                    }, error => {

                    });
            }
        });
    }

    private initSavePartiallyApproved() {
        let row: any;

        const rows = this.gridAg.options.getSelectedRows();

        if (rows.length > 1 || rows.length === 0)
            return;

        _.forEach(rows, (y: any) => {
            if (y.customerInvoiceRowId > 0)
                row = y;
        });

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Invoices/HouseholdDeduction/Dialogs/ApprovedAmount/ApprovedAmount.html"),
            controller: ApprovedAmountController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'sm',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                approvedAmount: () => { return row.amount },
            }
        });

        modal.result.then((result: any) => {
            if (result) {
                if (result.approvedAmount <= 0 || result.approvedAmount > row.amount)
                {
                    const keys: string[] = [
                        "core.warning",
                        "billing.invoices.householddeduction.amounterror"
                    ];

                    this.translationService.translateMany(keys).then((terms) => {
                        this.notificationService.showDialog(terms["core.warning"], terms["billing.invoices.householddeduction.amounterror"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    });
                }
                else {
                    this.savePartiallyApproved(row, result.approvedAmount, ((row.amount - result.approvedAmount) * -1), result.createInvoice);
                }
            }
        });
    }

    public savePartiallyApproved(row, amount, restAmount, createInvoice) {
        this.progress.startSaveProgress((completion) => {
            this.invoiceService.saveHouseholdTaxPartiallyApproved(row.customerInvoiceRowId, amount, this.bulkDate).then((result) => {
                if (result.success) {
                    const keys: string[] = [
                        "billing.invoices.householddeduction.recievedsuccessheader",
                        "billing.invoices.householddeduction.recievedsuccess",
                        "economy.accounting.voucher.new"
                    ];
                    return this.translationService.translateMany(keys).then((terms) => {
                        const modal = this.notificationService.showDialog(terms["billing.invoices.householddeduction.recievedsuccessheader"], terms["billing.invoices.householddeduction.recievedsuccess"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
                        modal.result.then(val => {
                            if (val != null && val === true) {
                                const message = { entityType: SoeEntityType.Voucher, date: this.bulkDate, amount: amount, ids: [row.customerInvoiceRowId], nbrs: [row.invoiceNr], productId: row.productId };
                                this.messagingService.publish(Constants.EVENT_OPEN_HOUSEHOLD, message);
                            }
                        });

                        if (createInvoice) 
                            this.messagingService.publish(Constants.EVENT_OPEN_HOUSEHOLD, { entityType: SoeEntityType.CustomerInvoice, id: row.invoiceId, rowId: row.customerInvoiceRowId, taxDeductionType: this.selectedTaxDeductionType, percent: row.houseHoldTaxDeductionPercent, amount: restAmount });

                        completion.completed(null, null, true);
                    });
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {

            });
    }

    public saveDenied() {
        let row: any;

        const rows = this.gridAg.options.getSelectedRows();

        if (rows.length > 1 || rows.length === 0)
            return;

        _.forEach(rows, (y: any) => {
            if (y.customerInvoiceRowId > 0)
                row = y;
        });

        this.progress.startSaveProgress((completion) => {
            this.invoiceService.saveHouseholdTaxDenied(row.invoiceId, row.customerInvoiceRowId, this.bulkDate).then((result) => {
                if (result.success) {
                    const keys: string[] = [
                        "billing.invoices.householddeduction.deniedsuccessheader",
                        "billing.invoices.householddeduction.deniedsuccess",
                        "common.customer.invoices.newcustomerinvoice"
                    ];
                    return this.translationService.translateMany(keys).then((terms) => {
                        const modal = this.notificationService.showDialog(terms["billing.invoices.householddeduction.deniedsuccessheader"], terms["billing.invoices.householddeduction.deniedsuccess"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
                        modal.result.then(val => {
                            if (val != null && val === true) {
                                const message = { entityType: SoeEntityType.CustomerInvoice, id: row.invoiceId, rowId: row.customerInvoiceRowId, taxDeductionType: this.selectedTaxDeductionType, percent: row.houseHoldTaxDeductionPercent };
                                this.messagingService.publish(Constants.EVENT_OPEN_HOUSEHOLD, message);
                            }
                        });
                        completion.completed(null, null, true);
                    });
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {

            });
    }

    public withdrawApplied() {
        let dict: any[] = [];
        this.gridAg.options.getSelectedRows().forEach((y: any) => {
            if (y.customerInvoiceRowId > 0)
                dict.push(y.customerInvoiceRowId);
        });
        this.progress.startSaveProgress((completion) => {
            this.invoiceService.saveHouseholdTaxWithdrawn(dict).then((result) => {
                if (result.success) {
                    return this.translationService.translate("billing.invoices.householddeduction.withdrawsuccess").then((term) => {
                        this.notificationService.showDialog(term, term, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                        completion.completed(null, null, true);
                    });
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {
            });
    }

    //public withdrawRecieved(row: any) {
    public withdrawRecieved() {
        const selectedRows = this.gridAg.options.getSelectedRows();
        if (selectedRows.length === 0 || selectedRows.length > 1)
            return;
        if (selectedRows[0].voucherHeadId > 0) {
            return this.translationService.translate("billing.invoices.householdtaxdeduction.reverseinfotext").then((term) => {
                this.progress.startDeleteProgress((completion) => {
                    this.invoiceService.withdrawReceived(selectedRows[0].customerInvoiceRowId).then((result) => {
                        if (result.success) {
                            if (selectedRows[0].voucherHeadId)
                                this.openVoucher(selectedRows[0]);
                            completion.completed(null);
                        }
                        else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null, term)
                    .then(data => {
                        this.loadGridData();
                    }, error => {
                    });
            });
        }
        else {
            this.progress.startDeleteProgress((completion) => {
                this.invoiceService.withdrawReceived(selectedRows[0].customerInvoiceRowId).then((result) => {
                    if (result.success) {
                        if (selectedRows[0].voucherHeadId)
                            this.openVoucher(selectedRows[0]);
                        completion.completed(null);
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }, null)
                .then(data => {
                    this.loadGridData();
                }, error => {
            });
        }
    }

    public initDeleteRow(row: any) {
        const keys: string[] = [
            "core.verifyquestion",
            "billing.invoices.householddeduction.deletequestion"
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["billing.invoices.householddeduction.deletequestion"].format(row.invoiceNr.toString()), SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val != null && val === true) {
                    this.deleteRow(row);
                }
            });
        });
    }

    public deleteRow(row: any) {
        this.progress.startDeleteProgress((completion) => {
            this.invoiceService.deleteHouseholdTaxDeductionRow(row.customerInvoiceRowId).then((result) => {
                if (result.success) {
                    completion.completed(row);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.loadGridData();
        });
    }

    private initCreateFile(editFile: boolean) {
        const selectedRows = this.gridAg.options.getSelectedRows();
        this.checkSelectedRows(selectedRows, false).then((id) => {
            if (id) {
                this.checkExistingSequenceNumber(SoeReportTemplateType.HouseholdTaxDeductionFile, selectedRows, editFile);
            }
        });
    }

    private initPrintReport() {
        const selectedRows = this.gridAg.options.getSelectedRows();

        this.checkSelectedRows(selectedRows, false).then((id) => {
            if (id) {
                const keys: string[] = [
                    "core.info",
                    "billing.invoices.householddeduction.printreportmessage",
                    "billing.invoices.householddeduction.printreportmessage.rotrut",
                    "billing.invoices.householddeduction.printreportmessage.green"
                ];
                this.translationService.translateMany(keys).then((terms) => {
                    
                    const green = selectedRows[0].houseHoldTaxDeductionType === TermGroup_HouseHoldTaxDeductionType.GREEN;
                    let message = terms["billing.invoices.householddeduction.printreportmessage"] + "\n";
                    message += green ? terms["billing.invoices.householddeduction.printreportmessage.green"] : terms["billing.invoices.householddeduction.printreportmessage.rotrut"];

                    const modal = this.notificationService.showDialog(terms["core.info"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        if (val != null && val === true) {
                            this.checkExistingSequenceNumber(SoeReportTemplateType.HousholdTaxDeduction, selectedRows, false);
                        }
                    });
                });
            }
        });
    }

    private checkSelectedRows(selectedRows: any[], checkUniqueProductId): ng.IPromise<number> {
        const deferral = this.$q.defer<number>();

        const uniqueTypes: number[] = _.uniq(selectedRows.map(a => a.houseHoldTaxDeductionType));
        const uniqueProductIds: number[] = _.uniq(selectedRows.map(a => a.productId));

        if (uniqueTypes.length > 1 || (checkUniqueProductId && (this.selectedTaxDeductionType === TermGroup_HouseHoldTaxDeductionType.ROT ? _.some(uniqueProductIds, (p) => p !== this.householdTaxDeductionProductId && p !== this.household50TaxDeductionProductId) : uniqueProductIds.length > 1))) {
            const keys: string[] = [
                "core.warning",
                "billing.invoices.householddeduction.sametypemessage"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["core.warning"], terms["billing.invoices.householddeduction.sametypemessage"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                deferral.resolve(undefined);
            });
        }
        else {
            deferral.resolve(uniqueProductIds[0]);
        }
        return deferral.promise;
    }

    private checkExistingSequenceNumber(templateType: SoeReportTemplateType, selectedRows: any[], editFile: boolean) {

        if (selectedRows.length === 0) {
            return 0;
        }

        const taxDeductionType = selectedRows[0].houseHoldTaxDeductionType;

        if (selectedRows.filter(r => r.seqNr && r.seqNr > 0).length > 0)
        {
            const keys: string[] = [
                "core.warning",
                "billing.invoices.householddeduction.existingseqnrmessage",
                "billing.invoices.householddeduction.sametypemessage"
            ];

            return this.translationService.translateMany(keys).then((terms) => {
                
                const modal = this.notificationService.showDialog(terms["core.warning"], terms["billing.invoices.householddeduction.existingseqnrmessage"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val != null && val === true) {
                        this.showSequenceNumberDialog(templateType, taxDeductionType, editFile);
                    }
                });
            });
        }
        else {
            this.showSequenceNumberDialog(templateType, taxDeductionType, editFile);
        }
    }

    private showSequenceNumberDialog(templateType: SoeReportTemplateType, taxDeductionType: TermGroup_HouseHoldTaxDeductionType, editFile: boolean) {
        const dict: any = [];
        const rows = this.gridAg.options.getSelectedRows();
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Invoices/HouseholdDeduction/Dialogs/SetSequenceNumber/SetSequenceNumber.html"),
            controller: SetSequenceNumberController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                taxDeductionType: () => { return taxDeductionType },
            }
        });

        modal.result.then((result: any) => {
            if (result) {
                if (editFile)
                    this.openEditFileDialog(result);
                else
                    this.printReport(templateType, taxDeductionType, result);
            }
        });
    }

    private openEditFileDialog(sequenceNumber: number) {
        const dict: any = [];
        const rows = this.gridAg.options.getSelectedRows();
        rows.forEach(y => {
            if (y.customerInvoiceRowId > 0) {
                dict.push(y.customerInvoiceRowId);
            }
        });

        this.progress.startLoadingProgress([() => {
            return this.invoiceService.getHouseHoldTaxFileForEdit(dict).then((x) => {
                const modal = this.modalInstance.open({
                    templateUrl: this.urlHelperService.getGlobalUrl("Billing/Invoices/HouseholdDeduction/Dialogs/EditHouseholdFile/EditHouseholdFile.html"),
                    controller: EditHouseholdFileController,
                    controllerAs: 'ctrl',
                    backdrop: 'static',
                    size: 'lg',
                    resolve: {
                        translationService: () => { return this.translationService },
                        coreService: () => { return this.coreService },
                        invoiceService: () => { return this.invoiceService },
                        rows: () => { return x },
                    }
                });

                modal.result.then((result: any) => {
                    if (result) {
                        this.downloadFile(result, sequenceNumber);
                    }
                });
            });
        }]);
    }

    private downloadFile(items: HouseholdTaxDeductionFileRowDTO[], sequenceNumber: number) {
        this.progress.startLoadingProgress([() => {
            return this.invoiceService.downloadHouseHoldTaxFile(items, this.selectedTaxDeductionType, sequenceNumber).then((x) => {
                HtmlUtility.openInSameTab(this.$window, x);

                this.$timeout(() => {
                    this.loadGridData(false);
                }, 1000);
            });
        }]);
    }

    private printReport(templateType: SoeReportTemplateType, taxDeductionType: TermGroup_HouseHoldTaxDeductionType, sequenceNumber: number) {
        const dict: number[] = [];
        const rows = this.gridAg.options.getSelectedRows();
        rows.forEach( y => {
            if (y.customerInvoiceRowId > 0) {
                dict.push(y.customerInvoiceRowId);
            }
        });

        if (templateType === SoeReportTemplateType.HousholdTaxDeduction) {
            const reportItem = new HouseholdTaxDeductionPrintDTO(dict);
            reportItem.sysReportTemplateTypeId = templateType;
            reportItem.sequenceNumber = sequenceNumber;
            reportItem.useGreen = taxDeductionType === TermGroup_HouseHoldTaxDeductionType.GREEN;

            this.requestReportService.printHouseholdTaxDeduction(reportItem).then(
                () => {

                    this.$timeout(() => {
                        this.loadGridData(false);
                    }, 1000);
                }
            );

        } else {

            this.reportService.getHouseholdTaxDeductionPrintUrl(
                dict, 
                this.householdReportTemplateId, 
                templateType, 
                sequenceNumber, 
                taxDeductionType === TermGroup_HouseHoldTaxDeductionType.GREEN
            ).then((x: string) => {
                HtmlUtility.openInSameTab(this.$window, x);

                this.$timeout(() => {
                    this.loadGridData(false);
                }, 1000);
            });
        }

    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.ProductHouseholdTaxDeduction, CompanySettingType.ProductHousehold50TaxDeduction];
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.householdTaxDeductionProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductHouseholdTaxDeduction);
            this.household50TaxDeductionProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductHousehold50TaxDeduction);
        });
    }

    protected loadUserSettings(): ng.IPromise<any> {
        const deferral = this.$q.defer<any>();

        const settingTypes: number[] = [UserSettingType.BillingInvoiceDefaultHouseholdTaxType, UserSettingType.BillingInvoiceDefaultHouseholdPrintButtonOption];

        this.coreService.getUserSettings(settingTypes).then(x => {
            const type = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingInvoiceDefaultHouseholdTaxType, 0, false);
            if (type) {
                this._selectedTaxDeductionType = type;
            }
            else {
                this._selectedTaxDeductionType = this.taxDeductionTypes[this.taxDeductionTypes.length - 1].id;
            }
            deferral.resolve();
            this.getDefaultHouseholdReport();

            if (this.isApplied) { 
                this.setSplitButtonFunctions();
                this.defaultPrintButtonOption = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingInvoiceDefaultHouseholdPrintButtonOption, 6, false);
            }
        });

        return deferral.promise;
    }

    private updateTaxDeductionTypeSetting() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.BillingInvoiceDefaultHouseholdTaxType, this.selectedTaxDeductionType).then((x) => {
            this.getDefaultHouseholdReport();
            this.loadGridData();
        });
    }

    private updatePrintSetting(option: number) {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.BillingInvoiceDefaultHouseholdPrintButtonOption, option);
    }

    private summarizeFiltered(x) {
        let filTotal = 0;
        _.forEach(x, (o: any) => {
            filTotal += o ? o.amount : 0;
        });
        this.$timeout(() => {
            this.filteredTotal = filTotal;
        });
    }
    private summarizeSelected() {
        let selTotal = 0;
        const rows = this.gridAg.options.getSelectedRows();
        rows.forEach(y => {
            selTotal += y ? y.amount : 0;
        });
        this.$timeout(() => {
            this.selectedTotal = selTotal;
        });
    }

    private filter() {
        this.filteredHouseholdRows = this.householdRows;
        if (this.selectedStatuses && this.selectedStatuses.length > 0) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, r => _.includes(_.map(this.selectedStatuses, 'id'), r.status));
        }
        if (this.selectedTypes && this.selectedTypes.length > 0) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, r => _.includes(_.map(this.selectedTypes, 'id'), r.houseHoldTaxDeductionType));
        }
        if (this.appliedDateFrom && this.appliedDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.appliedDate && row.appliedDate >= this.appliedDateFrom && row.appliedDate <= this.appliedDateTo));
        }

        if (this.appliedDateFrom && !this.appliedDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.appliedDate && row.appliedDate >= this.appliedDateFrom && row.appliedDate <= new Date()));
        }

        if (!this.appliedDateFrom && this.appliedDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.appliedDate && row.appliedDate >= new Date(1990, 1, 1) && row.appliedDate <= this.appliedDateTo));
        }

        if (this.receivedDateFrom && this.receivedDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.receivedDate && row.receivedDate >= this.receivedDateFrom && row.receivedDate <= this.receivedDateTo));
        }

        if (this.receivedDateFrom && !this.receivedDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.receivedDate && row.receivedDate >= this.receivedDateFrom && row.receivedDate <= new Date()));
        }

        if (!this.receivedDateFrom && this.receivedDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.receivedDate && row.receivedDate >= new Date(1990, 1, 1) && row.receivedDate <= this.receivedDateTo));
        }

        if (this.deniedDateFrom && this.deniedDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.deniedDate && row.deniedDate >= this.deniedDateFrom && row.deniedDate <= this.deniedDateTo));
        }

        if (this.deniedDateFrom && !this.deniedDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.deniedDate && row.deniedDate >= this.deniedDateFrom && row.deniedDate <= new Date()));
        }

        if (!this.deniedDateFrom && this.deniedDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.deniedDate && row.deniedDate >= new Date(1990, 1, 1) && row.deniedDate <= this.deniedDateTo));
        }

        if (this.paidDateFrom && this.paidDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.fullyPayed /*&& row.payDate*/ && row.payDate >= this.paidDateFrom && row.payDate <= this.paidDateTo));
        }

        if (this.paidDateFrom && !this.paidDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.fullyPayed /*&& row.payDate*/ && row.payDate >= this.paidDateFrom && row.payDate <= new Date()));
        }

        if (!this.paidDateFrom && this.paidDateTo) {
            this.filteredHouseholdRows = _.filter(this.filteredHouseholdRows, (row) => (row.fullyPayed /*&& row.payDate*/ && row.payDate >= new Date(1990, 1, 1) && row.payDate <= this.paidDateTo));
        }

        this.summarizeFiltered(null);
        this.summarizeSelected();

        this.setData(this.filteredHouseholdRows);
    }

    private showErrorMessage() {
        const keys: string[] = [
            "core.warning",
            "economy.inventory.inventories.adjustment.datemissing"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            this.notificationService.showDialog(terms["core.warning"], terms["economy.inventory.inventories.adjustment.datemissing"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            return;
        });
    }

    private loadHouseHoldRowInfo(invoiceId: number, customerInvoiceRowId: number): ng.IPromise<any> {
        return this.invoiceService.getHouseHoldTaxRowInfo(invoiceId, customerInvoiceRowId);
    }

    private showGreenApplyInfo(row) {
        this.loadHouseHoldRowInfo(row.invoiceId, row.customerInvoiceRowId).then( (result: IActionResult) => {
            const keys: string[] = [
                "core.info",
            ];
            return this.translationService.translateMany(keys).then((terms) => {
                let message = "";

                result.strings.forEach(s => {
                    if (s) {
                        message += s + "\n";
                    }
                });
                
                /*
                let message = "Antal timmar: \n";
                message += "Kostnad för installation: \n";
                message += "Övrig kostnad: \n";
                message += "Betalt belopp: \n";
                message += "Begärt belopp: \n";
                */
                this.notificationService.showDialog(terms["core.info"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
            });
        });
    }

    private showComment(row) {
        this.translationService.translate("common.customer.customer.rot.comment").then((term) => {
            this.notificationService.showDialog(term, row.comment, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
            return;
        });
    }

    private editDeductionInfo(row) {
        this.invoiceService.getHouseHoldTaxRowForEdit(row.customerInvoiceRowId).then((x) => {
            if (!x)
                return;

            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Billing/Invoices/HouseholdDeduction/Dialogs/EditHouseholdData/EditHouseholdData.html"),
                controller: EditHouseholdDataController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'md',
                resolve: {
                    translationService: () => { return this.translationService },
                    invoiceService: () => { return this.invoiceService },
                    row: () => { return x },
                }
            });

            modal.result.then((result: any) => {
                if (result) {
                    if (result.success) {
                        this.loadGridData();
                    }
                    else {
                        const keys: string[] = [
                            "core.warning",
                            "billing.invoices.householddeduction.editinfonotsaved"
                        ];
                        this.translationService.translateMany(keys).then((terms) => {
                            this.notificationService.showDialog(terms["core.warning"], terms["billing.invoices.householddeduction.editinfonotsaved"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                            return;
                        });
                    }
                }
            });
        });
    }
}
