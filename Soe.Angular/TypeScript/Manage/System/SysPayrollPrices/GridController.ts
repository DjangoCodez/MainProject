import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { SoeTimeCodeType, Feature, TermGroup_Languages, TermGroup, TermGroup_SysPayrollPrice, SoeEntityImageType, SoeEntityState } from "../../../Util/CommonEnumerations";
import { ITimeCodeGridDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";
import { ISystemService } from "../SystemService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SysPayrollPriceDTO } from "../../../Common/Models/SysPayrollPriceDTO";
import { SysPayrollPriceIntervalsController } from "./Dialogs/SysPayrollPriceIntervalsController";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    private terms: { [index: string]: string; };

    // Lookups
    private sysCountries: ISmallGenericType[];
    private types: ISmallGenericType[];
    private amountTypes: ISmallGenericType[];

    private sysCountryId: number = TermGroup_Languages.Swedish;
    private showAll: boolean = false;
    private unlocked: boolean = false;

    // Toolbar
    private toolbarInclude: any;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        private notificationService: INotificationService,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Manage.System.SysPayrollPrices", progressHandlerFactory, messagingHandlerFactory);

        this.toolbarInclude = urlHelperService.getViewUrl("gridHeader.html");

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
            .onBeforeSetUpGrid(() => this.loadLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    // SETUP

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Preferences_TimeSettings_TimeCodeWork, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupGrid() {
        this.gridAg.addColumnText("sysTermId", "SysTermId", 50);
        this.gridAg.addColumnSelect("typeName", this.terms["common.type"], 100, { displayField: "typeName", selectOptions: this.types, dropdownValueLabel: "name" });
        this.gridAg.addColumnText("code", this.terms["common.code"], 100);
        this.gridAg.addColumnText("name", this.terms["common.name"], null);
        this.gridAg.addColumnNumber("amount", this.terms["common.value"], 100, { decimals: 2 });
        this.gridAg.addColumnSelect("amountTypeName", this.terms["manage.system.syspayrollprices.amounttype"], 100, { displayField: "amountTypeName", selectOptions: this.amountTypes, dropdownValueLabel: "name" });
        this.gridAg.addColumnDate("fromDate", this.terms["common.fromdate"], 100);
        this.gridAg.addColumnIcon(null, "", null, { icon: 'fal fa-list-ol', toolTip: this.terms["manage.system.syspayrollprices.syspayrollprice.hasintervals"], showIcon: (row: SysPayrollPriceDTO) => { return row.showIntervals; }, suppressFilter: true, onClick: (row) => { this.showIntervals(row) } });
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this), false, () => { return this.unlocked });

        this.gridAg.finalizeInitGrid("manage.system.syspayrollprices.syspayrollprices", true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.edit", "core.edit", IconLibrary.FontAwesome, "fa-key",
            () => { this.initUnlock(); },
            () => { return this.unlocked; }
        )));

        this.toolbar.addInclude(this.toolbarInclude);
    }

    // SERVICE CALLS   

    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCountries(),
            this.loadTypes(),
            this.loadAmountTypes()
        ]);
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.delete",
            "core.edit",
            "common.type",
            "common.code",
            "common.name",
            "common.value",
            "common.fromdate",
            "manage.system.syspayrollprices.amounttype",
            "manage.system.syspayrollprices.syspayrollprice.hasintervals"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(false, true).then(x => {
            this.sysCountries = x;
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SysPayrollPriceType, true, false).then(x => {
            this.types = x;
        });
    }

    private loadAmountTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SysPayrollPriceAmountType, true, false).then(x => {
            this.amountTypes = x;
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.systemService.getSysPayrollPrices(this.sysCountryId, null, true, true, true, true, !this.showAll, null).then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    // EVENTS   

    private sysCountryChanged() {
        this.$timeout(() => {
            this.loadGridData();
        });
    }

    private showAllChanged() {
        this.$timeout(() => {
            this.loadGridData();
        });
    }

    private initUnlock() {
        var keys: string[] = [
            "manage.system.syspayrollprices.unlock.title",
            "manage.system.syspayrollprices.unlock.message",
            "manage.system.syspayrollprices.unlock.wrongpassword"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["manage.system.syspayrollprices.unlock.title"], terms["manage.system.syspayrollprices.unlock.message"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK, { showTextBox: true, textBoxType: 'password' });
            modal.result.then(val => {
                modal.result.then(result => {
                    if (result.result && result.textBoxValue) {
                        this.systemService.validatePasswordForSysPayrollPrice(result.textBoxValue).then(passed => {
                            if (passed) {
                                this.unlocked = true;
                                this.gridAg.options.refreshGrid();
                            } else {
                                this.notificationService.showDialogEx(terms["manage.system.syspayrollprices.unlock.title"], terms["manage.system.syspayrollprices.unlock.wrongpassword"], SOEMessageBoxImage.Custom, SOEMessageBoxButtons.OK, { customIcon: 'fa-lock-alt' });
                            }
                        });
                    }
                });
            });
        });
    }

    edit(row) {
        // Send message to TabsController        
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    private showIntervals(sysPayrollPrice: SysPayrollPriceDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/System/SysPayrollPrices/Dialogs/SysPayrollPriceIntervals.html"),
            controller: SysPayrollPriceIntervalsController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                sysPayrollPrice: () => { return sysPayrollPrice },
            }
        }
        this.$uibModal.open(options);
    }
}