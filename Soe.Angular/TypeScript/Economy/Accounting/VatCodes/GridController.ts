import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Filters
    accountFilterOptions: Array<any> = [];

    //@ngInject
    constructor(private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private $filter: ng.IFilterService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
    ) {
        super(gridHandlerFactory, "Economy.Accounting.VatCodes", progressHandlerFactory, messagingHandlerFactory);

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
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(true));
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(false); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Preferences_VoucherSettings_VatCodes, loadReadPermissions: true, loadModifyPermissions: true });
    }

    public setupGrid() {

        // Columns
        var keys: string[] = [
            "common.code",
            "common.name",
            "economy.accounting.vatcode.account",
            "economy.accounting.vatcode.purchasevataccount",
            "common.percentage",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            this.gridAg.addColumnText("code", terms["common.code"], 15, false);
            this.gridAg.addColumnText("name", terms["common.name"], 30);
            this.gridAg.addColumnText("account", terms["economy.accounting.vatcode.account"], 20, true);
            this.gridAg.addColumnText("purchaseVATAccount", terms["economy.accounting.vatcode.purchasevataccount"], 20, true);
            this.gridAg.addColumnNumber("percentString", terms["common.percentage"], 15, { enableHiding: true });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.finalizeInitGrid("economy.accounting.vatcode.vatcodes",true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData(false));
    }

    public loadGridData(useCache: boolean) {

        // Load data
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getVatCodes(useCache).then((x) => {
                // Format percent column
                var filter: Function = this.$filter("amount");
                _.forEach(x, (y) => {
                    y['percentString'] = filter(y['percent']);
                });
                return x;
            }).then(data => {
                this.setData(data);
            });

        }]);
    }

    protected loadLookups() {
        this.loadAccountDict();
    }

    private loadAccountDict() {
        this.accountingService.getAccountStdsDict(true).then((x) => {
            _.forEach(x, (y: any) => {
                this.accountFilterOptions.push({ value: y.name, label: y.name });
            });
        });
    }
}