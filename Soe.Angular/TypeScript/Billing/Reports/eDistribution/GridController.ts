import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature, SoeOriginType, TermGroup } from "../../../Util/CommonEnumerations";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { Constants } from "../../../Util/Constants";
import { IMessagingService } from "../../../Core/Services/MessagingService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private distributionItems: any;
    private terms: any = [];
    protected distributionTypes: ISmallGenericType[] = [];
    protected distributionStatusTypes: ISmallGenericType[] = [];
    protected originTypes: ISmallGenericType[] = [];
    protected allItemsSelectionDict: any[];

    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        this.reloadGridFromFilter();
    }

    private _distributionTypeSelection: ISmallGenericType;
    get distributionTypeSelection() {
        return this._distributionTypeSelection
    }
    set distributionTypeSelection(value: any) {
        this._distributionTypeSelection = value;
        this.reloadGridFromFilter();
    }

    private _originTypeSelection: ISmallGenericType;
    get originTypeSelection() {
        return this._originTypeSelection
    }
    set originTypeSelection(value: any) {
        this._originTypeSelection = value;
        this.reloadGridFromFilter();
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        protected messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
    ) {
        super(gridHandlerFactory, "Billing.Distribution.EDistribution", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.onBeforeSetUpGrid())

            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start({ feature: Feature.Billing_Distribution_Reports, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private loadOriginTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.OriginType,false,false).then(x => {
            this.originTypes = x.filter(x => x.id === SoeOriginType.Offer || x.id === SoeOriginType.Order || x.id === SoeOriginType.CustomerInvoice
                || x.id === SoeOriginType.Purchase);

            
            this._originTypeSelection = this.originTypes.filter(x => x.id == 0)[0];
            this.translationService.translate("common.all").then(term => {
                this.originTypes.unshift({ id: 0, name: term });
                this._originTypeSelection = this.originTypes.find(x => x.id == 0);
            });
        });
    }

    private loadDistributionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EdistributionTypes, true, false).then(x => {
            this.distributionTypes = x;
            this._distributionTypeSelection = this.distributionTypes.find(x => x.id == 0);
        });
    }

    private loadDistributionStatusTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EDistributionStatusType, true, false).then(x => {
            this.distributionStatusTypes = x;
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.GridDateSelectionType, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
            this._allItemsSelection = 1;
        });
    }

    private onBeforeSetUpGrid(): ng.IPromise<any> {
        return this.$q.all([this.loadDistributionTypes(), this.loadDistributionStatusTypes(), this.loadSelectionTypes(), this.loadOriginTypes()]);
    }

    private setIds(row: any) {
        let ids: any = [];

        if (row.originTypeId == SoeOriginType.Offer || row.originTypeId == SoeOriginType.CustomerInvoice || row.originTypeId == SoeOriginType.Purchase)
            ids = new Array(row.originId);
        else if (row.originTypeId == SoeOriginType.Order)
            ids = _.map(this.gridAg.options.getFilteredRows(), 'seqNr');

        return ids;
    }

    private openOrderOrOfferOrCustomerInvoice(row: any) {
        let data: any = {};
        data = {
            originId: row.originId,
            name: row.originTypeName + " " + row.seqNr,
            status: row.status,
            seqNr: row.seqNr,
            ids: this.setIds(row),
            originType: row.originTypeId
        };

        if (row.originTypeId == SoeOriginType.Order) {
            this.messagingService.publish(Constants.EVENT_OPEN_ORDER, data);
        }
        else if (row.originTypeId == SoeOriginType.CustomerInvoice) {
            this.messagingService.publish(Constants.EVENT_OPEN_INVOICE, data);
        }
        else if (row.originTypeId == SoeOriginType.Offer) {
            this.messagingService.publish(Constants.EVENT_OPEN_OFFER, data);
        }
        else if (row.originTypeId == SoeOriginType.Purchase) {
            this.messagingService.publish(Constants.EVENT_OPEN_PURCHASE, data);
        }
    }

    public setupGrid() {

        // Columns
        const keys: string[] = [
            "common.message",
            "billing.invoices.householddeduction.seqnbr",
            "common.customer.customer.customerorsuppliernr",
            "common.customer.customer.customerorsuppliername",
            "common.type",
            "common.status",
            "common.messages.sentby",
            "common.messages.sentdate",
            "common.order",
            "common.offer"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.gridAg.addColumnText("originTypeName", terms["common.type"], 30);
            this.gridAg.addColumnText("seqNr", terms["billing.invoices.householddeduction.seqnbr"], 15, true, { buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => true, callback: this.openOrderOrOfferOrCustomerInvoice.bind(this) } });
            this.gridAg.addColumnText("customerName", terms["common.customer.customer.customerorsuppliername"], 80);
            this.gridAg.addColumnText("customerNr", terms["common.customer.customer.customerorsuppliernr"], 60);
            this.gridAg.addColumnSelect("typeName", terms["common.type"], 30,
                {
                    selectOptions: this.distributionTypes,
                    enableHiding: false,
                    editable: false,
                    displayField: "typeName",
                    dropdownIdLabel: "id",
                    dropdownValueLabel: "name",
                });
            this.gridAg.addColumnSelect("status", terms["common.status"], 30,
                {
                    selectOptions: this.distributionStatusTypes,
                    enableHiding: false,
                    editable: false,
                    displayField: "statusName",
                    dropdownIdLabel: "id",
                    dropdownValueLabel: "name",
                });

            this.gridAg.addColumnText("createdBy", terms["common.messages.sentby"], 40);
            this.gridAg.addColumnDateTime("created", terms["common.messages.sentdate"], 40);
            this.gridAg.addColumnText("message", terms["common.message"], null,false, { toolTipField: "message" });

            this.gridAg.finalizeInitGrid("billing.distribution.edistribution.distribution", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
    }

    public loadGridData() {
        if (this.distributionTypeSelection && this.allItemsSelection) {
            this.progress.startLoadingProgress([() => {
                return this.reportService.getEDistributionItems(this.originTypeSelection.id, this.distributionTypeSelection.id, this.allItemsSelection).then((data) => {
                    this.distributionItems = data;
                    this.setData(data)
                });
            }]);
        }
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });
}