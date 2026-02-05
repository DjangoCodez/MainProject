import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Feature, ImportPaymentType, SettingMainType, SoeEntityState, SoeOriginType, TermGroup, TermGroup_PaymentTransferStatus, UserSettingType } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { PaymentImportDTO } from "../../../Common/Models/PaymentImportDTO";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private paymentMethodFilterOptions = [];
    private paymentTypeFilterOptions = [];

    private paymentMethods = [];
    private paymentTypes = [];

    private importTypeId: number;

    // Selection
    protected allItemsSelectionDict: any[];

    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        this.updateItemsSelection();
    }

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService) {

        super(gridHandlerFactory, "Economy.Import.Payments", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded((response) => {
                this.onAllPermissionsLoaded(response)
            })
            .onBeforeSetUpGrid(() => this.onDoLookups())
            .onSetUpGrid(() => this.setUpGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {

        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.importTypeId = parameters.importType;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start([{ feature: Feature.Economy_Import_Payments, loadReadPermissions: true, loadModifyPermissions: true } ])
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
    }

    onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Economy_Import_Payments].modifyPermission;
        this.readPermission = response[Feature.Economy_Import_Payments].readPermission;
        if (this.modifyPermission) {
            this.messagingHandler.publishActivateAddTab();
        }
    }

    private setUpGrid() {
        const translationKeys: string[] = [
            "economy.import.payments.importdate",
            "economy.import.payments.syspaymenttype",
            "economy.import.payments.type",
            "economy.import.payments.totalamount",
            "economy.import.payments.numberofpayments",
            "economy.import.payment.importBatchId",
            "core.createdby",
            "common.status",
            "core.created",
            "core.edit",
            "economy.import.payment.label",
            "economy.import.payment.paiddate"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            // hide selection
            this.gridAg.options.enableRowSelection = false;

            this.gridAg.addColumnText("batchId", terms["economy.import.payment.importBatchId"], 75, true);
            this.gridAg.addColumnDate("importDate", this.importTypeId === ImportPaymentType.CustomerPayment ? terms["economy.import.payment.paiddate"] : terms["economy.import.payments.importdate"], null, true);
            this.gridAg.addColumnSelect("paymentMethodName", terms["economy.import.payments.type"], null, { selectOptions: this.paymentMethodFilterOptions, displayField: "paymentMethodName" });
            this.gridAg.addColumnNumber("totalAmount", terms["economy.import.payments.totalamount"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.addColumnText("numberOfPayments", terms["economy.import.payments.numberofpayments"], null);
            this.gridAg.addColumnText("createdBy", terms["core.createdby"], null, true);
            this.gridAg.addColumnDateTime("created", terms["core.created"], null, true);
            this.gridAg.addColumnText("statusName", terms["common.status"], null, true);
            this.gridAg.addColumnText("paymentLabel", terms["economy.import.payment.label"], null, true, { hide: true, enableHiding: true });
            this.gridAg.addColumnIcon("transferStateIcon", terms["economy.export.payments.status"], null, { suppressSorting: false, enableHiding: true, toolTipField: "transferStateIconText", showTooltipFieldInFilter: true });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.getColumnDefs().forEach(col => {
                var cellcls: string = col.cellClass ? col.cellClass.toString() : "";
                col.cellClass = (grid: any) => {
                    if (grid.data['state'] === SoeEntityState.Inactive)
                        return cellcls + " closedRow";
                    else
                        return cellcls;
                }
            });

            this.gridAg.finalizeInitGrid("economy.import.payment.payments", true);
        });
    }

    private loadGridData() {
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {

            return this.accountingService.getPaymentImports(this.importTypeId, this.allItemsSelection).then((data: PaymentImportDTO[]) => {
                for (const row of data) {
                    this.setTransferIcon(row);
                    for (var pt in this.paymentTypes) {

                        if (this.paymentTypes[pt].id === row.sysPaymentTypeId) {
                            row.typeName = this.paymentTypes[pt].name;
                        }
                    }
                    for (var pm in this.paymentMethods) {

                        if (this.paymentMethods[pm].id === row.type) {
                            row.paymentMethodName = this.paymentMethods[pm].name;
                        }
                    }
                    //Fix dates
                    if (row.importDate)
                        row.importDate = new Date(row.importDate);

                };
                this.setData(data);
            });
        }]);
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadUserSettings(), this.loadSelectionTypes(), this.loadPaymentMethods(), this.loadPaymentTypes()]).then(() => this.loadGridData());
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        const type = this.importTypeId === ImportPaymentType.CustomerPayment ? UserSettingType.CustomerPaymentImportAllItemsSelection : UserSettingType.SupplierPaymentImportAllItemsSelection;
        settingTypes.push(type);
        return this.coreService.getUserSettings(settingTypes).then(x => {
            this._allItemsSelection = SettingsUtility.getIntUserSetting(x, type, 1, false);
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    private loadPaymentMethods(): ng.IPromise<any> {
        if (this.importTypeId === ImportPaymentType.CustomerPayment) {
            return this.accountingService.getPaymentMethods(SoeOriginType.CustomerPayment, false).then((x) => {
                this.paymentMethods = x;
                _.forEach(x, (y: any) => {
                    this.paymentMethodFilterOptions.push({ value: y.name, label: y.name })
                });
            });
        }
        else {
            return this.accountingService.getPaymentMethods(SoeOriginType.SupplierPayment, false).then((x) => {
                this.paymentMethods = x;
                _.forEach(x, (y: any) => {
                    this.paymentMethodFilterOptions.push({ value: y.name, label: y.name })
                });
            });
        }
    }

    private loadPaymentTypes(): ng.IPromise<any> {
        return this.accountingService.getSysPaymentTypeDict().then((x) => {
            this.paymentTypes = x;
            _.forEach(x, (y: any) => {
                this.paymentTypeFilterOptions.push({ value: y.name, label: y.name })
            });
        });
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, this.importTypeId === ImportPaymentType.CustomerPayment ? UserSettingType.CustomerPaymentImportAllItemsSelection : UserSettingType.SupplierPaymentImportAllItemsSelection, this.allItemsSelection).then((x) => {
            this.loadGridData();
        });
    }

    private setTransferIcon(row: any) {
        switch (<number>row.transferStatus) {
            case TermGroup_PaymentTransferStatus.Transfered:
                row.transferStateIcon = "fal fa-cloud-download warningColor";
                break;
            case TermGroup_PaymentTransferStatus.Completed:
                row.transferStateIcon = "fal fa-cloud-download okColor";
                break;
            case TermGroup_PaymentTransferStatus.AvaloError:
            case TermGroup_PaymentTransferStatus.SoftoneError:
            case TermGroup_PaymentTransferStatus.BankError:
                row.transferStateIcon = "fal fa-exclamation-triangle errorColor";
                row.transferStateIconText = row.transferMsg;
                break;
            default:
                row.transferStateIcon = "";
                row.transferStateIconState = "";
                break;
        }
    }
    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });
}
