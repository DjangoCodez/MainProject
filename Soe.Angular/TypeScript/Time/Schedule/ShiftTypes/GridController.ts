import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IScheduleService } from "../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { Feature, TermGroup, CompanySettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { AccountDimDTO } from "../../../Common/Models/AccountDimDTO";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ISmallGenericType, IActionResult } from "../../../Scripts/TypeLite.Net4";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, IconLibrary } from "../../../Util/Enumerations";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ShiftTypeGridDTO } from "../../../Common/Models/ShiftTypeDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    private terms: any;

    // Company settings
    private useAccountHierarchy: boolean = false;

    // Toolbar
    private toolbarInclude: any;    
    private _showShiftTypesWithInactivatedAccounts: boolean = false;
    get showShiftTypesWithInactivatedAccounts() {
        return this._showShiftTypesWithInactivatedAccounts;
    }
    set showShiftTypesWithInactivatedAccounts(item: boolean) {
        this._showShiftTypesWithInactivatedAccounts = item;
        this.setData(this.getfilteredData())
    }

    private shiftTypeAccountDim: AccountDimDTO;
    private timeScheduleTemplateBlockTypes: ISmallGenericType[];
    private timeScheduleTypes: ISmallGenericType[];
    private shiftTypes: ShiftTypeGridDTO[];

    // Flags
    private isOrder: boolean = false;
    private timeScheduleTypeVisible: boolean = false;
    private timeScheduleTemplateBlockTypeVisible: boolean = false;
    private hasSelectedRows: boolean = false;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Time.Schedule.ShiftTypes", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('shiftTypeId', 'name');

        this.toolbarInclude = this.urlHelperService.getViewUrl("gridHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onBeforeSetUpGrid(() => this.loadShiftTypeAccountDim())
            .onBeforeSetUpGrid(() => this.loadTimeScheduleTemplateBlockTypes())
            .onBeforeSetUpGrid(() => this.loadTimeScheduleTypes())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = this.parameters.guid;

        this.isOrder = soeConfig.type == "order" ? true : false;

        if (this.isHomeTab) {
            this.onTabActivetedAndModified(() => { this.loadGridData(false) });
        }

        this.flowHandler.start({ feature: this.isOrder ? Feature.Billing_Preferences_InvoiceSettings_ShiftType : Feature.Time_Preferences_ScheduleSettings_ShiftType, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData(false));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.delete", "core.delete", IconLibrary.FontAwesome, "fa-times", () => { this.delete(); },
            () => { return !this.hasSelectedRows },
            () => { return !this.modifyPermission }
        )));
        this.toolbar.addInclude(this.toolbarInclude);
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.delete",
            "core.deleteselectedwarning",
            "core.edit",
            "common.accounting",
            "common.active",
            "common.categories",
            "common.code",
            "common.color",
            "common.customer.invoices.noshifttype",
            "common.description",
            "common.name",
            "common.number",
            "common.skills.skills",
            "common.type",
            "time.schedule.scheduletype.scheduletype",
            "time.schedule.shifttype.linkedtoaccount",
            "time.schedule.shifttype.externalcode"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadShiftTypeAccountDim(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypeAccountDim(false).then(x => {
            this.shiftTypeAccountDim = x;
        });
    }

    private setUpGrid() {
        this.gridAg.options.enableRowSelection = true;
        this.gridAg.options.useGrouping(false, false, { keepColumnsAfterGroup: true, selectChildren: false });
        this.gridAg.options.groupHideOpenParents = true;

        this.gridAg.addColumnSelect("timeScheduleTemplateBlockTypeName", this.terms["common.type"], 80, { displayField: "timeScheduleTemplateBlockTypeName", selectOptions: this.timeScheduleTemplateBlockTypes, enableHiding: true, editable: false, enableRowGrouping: true });
        this.gridAg.addColumnText("name", this.terms["common.name"], null);
        this.gridAg.addColumnText("externalCode", this.terms["time.schedule.shifttype.externalcode"], 80, true);
        this.gridAg.addColumnText("description", this.terms["common.description"], null, true);
        this.gridAg.addColumnText("needsCode", this.shiftTypeAccountDim ? this.terms["common.number"] : this.terms["common.code"], 80, true);
        if (this.timeScheduleTypeVisible)
            this.gridAg.addColumnSelect("timeScheduleTypeName", this.terms["time.schedule.scheduletype.scheduletype"], 120, { displayField: "timeScheduleTypeName", selectOptions: this.timeScheduleTypes, enableHiding: true, editable: false, enableRowGrouping: true });
        if (!this.useAccountHierarchy)
            this.gridAg.addColumnText("categoryNames", this.terms["common.categories"], null, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("skillNames", this.terms["common.skills.skills"], null, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("accountingStringAccountNames", this.terms["common.accounting"], null, true, { enableRowGrouping: true });
        this.gridAg.addColumnSelect("color", this.terms["common.color"], 70, {
            populateFilterFromGrid: true, toolTipField: "color", selectOptions: null, shape: Constants.SHAPE_RECTANGLE, shapeValueField: "color", colorField: "color", enableHiding: true, ignoreTextInFilter: true, suppressFilter: true,
        });
        if (this.shiftTypeAccountDim)
            this.gridAg.addColumnIcon("isLinked", null, null, { toolTip: this.terms["time.schedule.shifttype.linkedtoaccount"], icon: "fal fa-link", showIcon: this.showLinkedIcon.bind(this), toolTipField: "billingIconMessage", enableResizing: false, suppressSorting: false, enableHiding: true });
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        this.gridAg.options.subscribe(events);


        this.gridAg.finalizeInitGrid("time.schedule.shifttype.shifttype", true);
    }

    protected showLinkedIcon(row: any): boolean {
        return row && row.accountId;
    }

    private loadTimeScheduleTemplateBlockTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeScheduleTemplateBlockType, false, false).then((x) => {
            this.timeScheduleTemplateBlockTypes = x;
        });
    }

    private loadTimeScheduleTypes(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTypesDict(false, true).then((x) => {
            this.timeScheduleTypes = x;
            if (_.size(this.timeScheduleTypes) > 1)
                this.timeScheduleTypeVisible = true;
        });
    }

    public loadGridData(useCache: boolean = true) {
        this.hasSelectedRows = false;
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getShiftTypesGrid(false, false, false, true, true, true, true, true, useCache).then((x) => {
                _.forEach(x, (y: any) => {
                    if (y.color && y.color.length === 9)
                        y.color = "#" + y.color.substring(3);
                });
                this.shiftTypes = x;                
                this.setData(this.getfilteredData());
            });
        }]);
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteShiftTypes(this.gridAg.options.getSelectedIds('shiftTypeId')).then(result => {
                if (result.success)
                    completion.completed(null, !result.errorMessage, result.errorMessage);
                else
                    completion.failed(result.errorMessage);
                return result;
            }, error => {
                completion.failed(error.message);
            }).then((result: IActionResult) => {
                if (result.booleanValue)
                    this.loadGridData(false);
            });
        }, null, this.terms["core.deleteselectedwarning"])
            .catch((reason) => { });
    }

    private getfilteredData(): ShiftTypeGridDTO[] {
        if (this.showShiftTypesWithInactivatedAccounts)
            return this.shiftTypes;
        else
            return _.filter(this.shiftTypes, (shiftType) => (!shiftType.accountIsNotActive))
       
    }
    // EVENTS

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.hasSelectedRows = this.gridAg.options.getSelectedCount() > 0;
        });
    }
}
