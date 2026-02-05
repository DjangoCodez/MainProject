import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Feature, SoePaymentExportCancelledStates, TermGroup, UserSettingType, SettingMainType, TermGroup_PaymentTransferStatus, SoePaymentStatus } from "../../../Util/CommonEnumerations";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { SettingsUtility } from "../../../Util/SettingsUtility";



export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //
    public exportType: number;
    private paymentMethods: any[];

    // Collections
    private termsArray: any;
    private allItemsSelectionDict: any[];
    private paymentTransferStatus: any[];

    // Subgrid rows
    private payments: any;
    private type: number;
    private toolbarInclude: any;

    //Permission
    private lbModifyPermission = false;
    private pgModifyPermission = false;
    private sepaModifyPermission = false;
    private cfpModifyPermission = false;

    // Flags
    private setupComplete = false;

    //DDL
    private exportTypes: any[] = [];

    private _selectedExportType: any;
    get selectedExportType() {
        return this._selectedExportType;
    }
    set selectedExportType(item: any) {
        this._selectedExportType = item;
        this.UpdateSelection();
    }

    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        if (this.setupComplete)
            this.updateItemsSelection();
    }

    //@ngInject
    constructor(
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
    ) {
        super(gridHandlerFactory, "Economy.Export.Payments", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded((response) => {
                this.onAllPermissionsLoaded(response);
                this.messagingHandler.publishActivateAddTab();
            })
            .onDoLookUp(() => this.onDoLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadGridData(() => this.loadGridData())
        
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.loadExportTypes();
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData());
        this.toolbarInclude = this.urlHelperService.getGlobalUrl("economy/export/payments/views/gridHeader.html");
        this.toolbar.addInclude(this.toolbarInclude);
    }

    onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.lbModifyPermission = response[Feature.Economy_Export_Payments_LB].modifyPermission;
        this.pgModifyPermission = response[Feature.Economy_Export_Payments_PG].modifyPermission
        this.sepaModifyPermission = response[Feature.Economy_Export_Payments_SEPA].modifyPermission;
        this.cfpModifyPermission = response[Feature.Economy_Export_Payments_Cfp].modifyPermission;
        this.modifyPermission = response[Feature.Economy_Export_Payments].modifyPermission;
        this.readPermission = response[Feature.Economy_Export_Payments].readPermission;
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.type = parameters.type;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start([{ feature: Feature.Economy_Export_Payments, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Export_Payments_LB, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Export_Payments_PG, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Export_Payments_SEPA, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Export_Payments_Cfp, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "economy.supplier.payment.payment",
            "economy.export.payments.username",
            "economy.export.payments.exportdate",
            "economy.export.payments.filename",
            "economy.export.payments.numberofpayments",
            "economy.export.payments.cancelled",
            "economy.export.payments.download",
            "economy.export.payments.cancelpayment",
            "economy.export.payments.segnr",
            "economy.export.payments.paymentnr",
            "economy.export.payments.amount",
            "economy.export.payments.paydate",
            "economy.export.payments.status",
            "core.created",
            "core.createdby",
            "common.message"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            //Details
            this.gridAg.enableMasterDetail(true);
            this.gridAg.options.setDetailCellDataCallback((params) => {
                this.getPaymentDetails(params)
            })
            this.gridAg.detailOptions.addColumnText("seqNr", terms["economy.export.payments.segnr"], 25);
            this.gridAg.detailOptions.addColumnText("paymentNr", terms["economy.export.payments.paymentnr"], 25);
            this.gridAg.detailOptions.addColumnText("amount", terms["economy.export.payments.amount"], 25);
            this.gridAg.detailOptions.addColumnDate("payDate", terms["economy.export.payments.paydate"], 25);
            this.gridAg.detailOptions.addColumnText("statusName", terms["economy.export.payments.status"], 25);
            this.gridAg.detailOptions.addColumnText("statusMsg", terms["common.message"], 25);
            this.gridAg.detailOptions.addColumnIcon("statusIcon", terms["economy.export.payments.status"], null, { suppressSorting: false, toolTipField: "statusName", showTooltipFieldInFilter: true });
            this.gridAg.detailOptions.addColumnEdit(terms["common.edit"], this.edit.bind(this), true);
            
            this.gridAg.detailOptions.finalizeInitGrid();

            //Master
            this.termsArray = terms;
            this.gridAg.addColumnText("createdBy", terms["core.createdby"], null);
            this.gridAg.addColumnDate("created", terms["core.created"], null);
            this.gridAg.addColumnText("filename", terms["economy.export.payments.filename"], null);
            this.gridAg.addColumnText("numberOfPayments", terms["economy.export.payments.numberofpayments"], null);
            this.gridAg.addColumnText("aggPaymentStatus", terms["economy.export.payments.status"], null);
            this.gridAg.addColumnIcon(null, terms["economy.export.payments.download"], null, { icon: "fal fa-download", onClick: this.doDownload.bind(this), showIcon: this.showDownloadIcon.bind(this) });

            this.gridAg.addColumnIcon("transferStateIcon", terms["economy.export.payments.status"], null, { suppressSorting: false, enableHiding: true, toolTipField: "transferStateIconText", showTooltipFieldInFilter: true });

            this.gridAg.addColumnDelete(terms["economy.export.payments.cancelpayment"], this.doCancel.bind(this), false, this.showCancelIcon.bind(this));

            this.gridAg.finalizeInitGrid("economy.export.payment.paymentexport", true);
        });
    }

    public getPaymentDetails(params) {
        const paymentRows = params["data"].paymentRows;
        _.forEach(paymentRows, r => {
            this.setDetailRowStatusIcon(r);
        })

        params.successCallback(paymentRows)
    }

    public loadGridData() {
        if (!this.selectedExportType || this.selectedExportType == 0) {
            return;
        }
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getPaymentExports(this._selectedExportType, this._allItemsSelection).then((data:any[]) => {
                for (const row of data) {
                    row["expander"] = "";
                    row["aggPaymentStatus"] = this.rowPaymentStatus(row["paymentRows"]);
                    this.setTransferIcon(row);
                }
                this.setData(data);
            });
        }]);
    }

    private setTransferIcon(row: any) {
        switch (<number>row.transferStatus) {
            case TermGroup_PaymentTransferStatus.PendingTransfer:
                row.transferStateIcon = "fal fa-exchange warningColor";
                row.transferStateIconText = this.paymentTransferStatus.find(x => x.id == TermGroup_PaymentTransferStatus.PendingTransfer)?.name ?? "";
                break;
            case TermGroup_PaymentTransferStatus.Transfered:
                row.transferStateIcon = "fal fa-cloud-upload warningColor";
                row.transferStateIconText = this.paymentTransferStatus.find(x => x.id == TermGroup_PaymentTransferStatus.Transfered)?.name ?? "";
                break;
            case TermGroup_PaymentTransferStatus.Completed:
                row.transferStateIcon = "fal fa-cloud-upload okColor";
                row.transferStateIconText = this.paymentTransferStatus.find(x => x.id == TermGroup_PaymentTransferStatus.Completed)?.name ?? "";
                break;
            case TermGroup_PaymentTransferStatus.Pending:
            case TermGroup_PaymentTransferStatus.PartlyRejected:
                row.transferStateIcon = "fal fa-siren-on errorColor";
                row.transferStateIconText = row.transferMsg;
                break;
            case TermGroup_PaymentTransferStatus.AvaloError:
            case TermGroup_PaymentTransferStatus.SoftoneError:
            case TermGroup_PaymentTransferStatus.BankError:
                row.transferStateIcon = "fal fa-exclamation-triangle errorColor";
                row.transferStateIconText = "Error";
                break;
            default:
                row.transferStateIcon = "";
                row.transferStateIconState = "";
                break;
        }
    }

    private setDetailRowStatusIcon(row: any) {
        switch (<number>row.status) {
            case SoePaymentStatus.Error:
                row.statusIcon = "fal fa-exclamation-triangle errorColor";
                break;
            default:
                row.statusIcon = "";
                break;
        }
    }

    public onDoLookups() {
        return this.$q.all([
            this.loadPaymentMethods(),
            this.loadUserSettings(),
            this.loadSelectionTypes(),
            this.loadPaymentTransferStatus(),
        ]).then(() => {
            this.setupComplete = true;
        });
    }

    protected loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.PaymentExportType, UserSettingType.ExportedPaymentsAllItemsSelection];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.selectedExportType = SettingsUtility.getIntUserSetting(x, UserSettingType.PaymentExportType, 0, false);
            this.allItemsSelection = SettingsUtility.getIntUserSetting(x, UserSettingType.ExportedPaymentsAllItemsSelection, 1, false);
        });
    }

    private loadPaymentMethods(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PaymentMethod, false, false, true).then( (x: any[]) => {
            this.paymentMethods = x;
        });
    }

    private loadPaymentTransferStatus(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PaymentTransferStatus, false, false).then((x: any[]) => {
            this.paymentTransferStatus = x;
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    public UpdateSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.PaymentExportType, this.selectedExportType).then((x) => {
            this.loadGridData();
        });
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.ExportedPaymentsAllItemsSelection, this.allItemsSelection)
        this.loadGridData();
    }

    private loadExportTypes()
    {
        if (this.lbModifyPermission)
        {
            this.exportTypes.push(this.paymentMethods[0]);
        }

        if (this.pgModifyPermission)
        {
            this.exportTypes.push(this.paymentMethods[1]);
        }

        if (this.sepaModifyPermission)
        {
            this.exportTypes.push(this.paymentMethods[3]);
            this.exportTypes.push(this.paymentMethods[9]);
        }

        if (this.cfpModifyPermission)
        {
            this.exportTypes.push(this.paymentMethods[8]);
        }

        if (this.lbModifyPermission || this.sepaModifyPermission) {
            this.exportTypes.push(this.paymentMethods[11]);
        }
    }

    public rowPaymentStatus(rows)
    {
        const flags = [], output = [], l = rows.length;
        for (let i = 0; i < l; i++) {
            if (flags[rows[i].statusName]) continue;
            flags[rows[i].statusName] = true;
            output.push(rows[i].statusName);
        }
        return output.sort().join(', ');
    }
    

    protected showCancelIcon(row) {
        const errorStatuses = [97, 98.99]; //BankError, AvaloError, SoftOneError
        return ((row.cancelledState === SoePaymentExportCancelledStates.Active && row.transferStatus === 0) || errorStatuses.some(x => x === row.transferStatus));
    }

    protected showDownloadIcon(row) {
        return true;
    }

    private doDownload(row) {
        let guid: string = row.filename.substring(0, row.filename.lastIndexOf("."));
        guid = guid.substring(guid.lastIndexOf("_") + 1);

        let uri = window.location.protocol + "//" + window.location.host;
        uri = uri + "/soe/economy/export/payments/default.aspx" + "?c=" + CoreUtility.actorCompanyId + "&r=" + CoreUtility.roleId + "&type=" + this.type + "&exportfile=" + guid + "&paymentExportId=" + row.paymentExportId;

        window.open(uri, '_blank');
    }
    private doCancel(row) {
        this.progress.startWorkProgress((completion) => {
            this.accountingService.cancelPaymentExport(row.paymentExportId).then((result) => {
                if (result.success) {
                    completion.completed(true);
                    this.loadGridData();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        })
    }
}