import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { Feature, TermGroup } from "../../../../../Util/CommonEnumerations";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";

export class AccountingPriorityDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Products/Products/Directives/Views/AccountingPriority.html'),
            scope: {
                productAccountPriorityRows: '=',
                readOnly: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: AccountingPriorityDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class AccountingPriorityDirectiveController extends GridControllerBase2Ag implements ICompositionGridController {
    // Setup
    private productAccountPriorityRows: any[] = [];
    private readOnly: boolean;

    // Collections
    accountingPrios: any[] = [];
    accountDims: any[] = [];

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Products.Products.Views.AccountingPriority", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            //.onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.onBeforeSetUpGrid())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.afterSetup());

        this.doubleClickToEdit = false;

        this.onInit({});
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.flowHandler.start({ feature: Feature.Billing_Product_Products_Edit, loadReadPermissions: true, loadModifyPermissions: true });
    }
    private setupWatchers() {
        this.$scope.$watch(() => this.productAccountPriorityRows, () => {
            this.loadGridData();
        });
    }

    private onBeforeSetUpGrid(): ng.IPromise<any> {
        return this.$q.all([
            this.loadAccountingPriority()
        ]);
    }

    private afterSetup() {
        this.setupWatchers();
    }

    private setupGrid() {
        const keys: string[] = [
            "billing.products.products.dimname",
            "billing.products.products.priority"
        ];

        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.enableFiltering = false;
        this.gridAg.options.setMinRowsToShow(5);

        this.translationService.translateMany(keys).then(terms => {

            this.gridAg.addColumnText("dimName", terms["billing.products.products.dimname"], 100);
            this.gridAg.addColumnSelect("prioNr", terms["billing.products.products.priority"], null, { editable: true, enableColumnMenu: false, selectOptions: this.accountingPrios, displayField: "prioName", dropdownIdLabel: "id", dropdownValueLabel: "name" });

            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("", false);
        });
    }

    public loadGridData() {
        this.gridAg.setData(this.productAccountPriorityRows);
    }

    
    // Lookups
    private loadAccountingPriority(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceProductAccountingPrio, true, false).then(x => {
            this.accountingPrios = x;
        });
    }

    private afterCellEdit(row: any, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        console.log("newValue", colDef.field, newValue);

        if (colDef.field === "prioNr") {
            const obj = this.accountingPrios.find(x=> x.name === newValue);
            if (obj) {
                row.prioNr = obj["id"];
                row.prioName = obj["name"];
                console.log("prioChanged dirty");
                this.messagingHandler.publishSetDirty();
            }
        }
    }

    // Actions
    
}
