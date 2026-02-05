import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Collections
    termsArray: any;

    //@ngInject
    constructor(private coreService: ICoreService,
        private translationService: ITranslationService,
        private $scope: ng.IScope,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Accounting.Currency", progressHandlerFactory, messagingHandlerFactory)

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Preferences_Currency, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {

        // Columns
        const keys: string[] = [
            "common.code",
            "common.name",
            "common.date",
            "economy.accounting.currency.rate",
            "economy.accounting.currency.source",
            "economy.accounting.currency.rateupdate",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.termsArray = terms;

            // Details
            this.gridAg.enableMasterDetail(true);
            this.gridAg.options.setDetailCellDataCallback((params) => {
                this.getCompCurrencyRates(params);

                this.$scope.$applyAsync(() => {
                    this.gridAg.detailOptions.enableRowSelection = false;
                    this.gridAg.detailOptions.sizeColumnToFit();
                });
            });

            this.gridAg.detailOptions.addColumnNumber("rateToBase", this.termsArray["economy.accounting.currency.rate"], null);
            this.gridAg.detailOptions.addColumnDate("date", this.termsArray["common.date"], null);
            this.gridAg.detailOptions.addColumnText("sourceName", this.termsArray["economy.accounting.currency.source"], null);
            this.gridAg.detailOptions.addColumnText("intervalTypeName", this.termsArray["economy.accounting.currency.rateupdate"], null);

            this.gridAg.detailOptions.enableFiltering = true;
            this.gridAg.detailOptions.enableGridMenu = true;

            this.gridAg.detailOptions.finalizeInitGrid();

            this.gridAg.addColumnText("code", terms["common.code"], 25);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.options.enableRowSelection = false;

            this.gridAg.finalizeInitGrid("economy.accounting.currency.currencies", true);
        });
    }

    private getCompCurrencyRates(params: any) {
        if (!params.data['rowsLoaded']) {
            return this.coreService.getCompCurrencyRates(params.data.currencyId).then((rows) => {
                params.data['rows'] = rows;
                params.data['rowsLoaded'] = true;
                params.successCallback(params.data['rows']);
            });
        }
        else {
            params.successCallback(params.data['rows']);
        }
    }

    public loadGridData() {
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.coreService.getCompCurrenciesGrid().then(data => {
                for (var row of data) {
                    row['expander'] = "";
                }
                this.setData(data);
            });
        }]);
    }
}
