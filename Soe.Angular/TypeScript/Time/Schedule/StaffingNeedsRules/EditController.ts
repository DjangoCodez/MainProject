import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISoeGridOptions, SoeGridOptions } from "../../../Util/SoeGridOptions";
import { NumberUtility } from "../../../Util/NumberUtility";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IScheduleService } from "../ScheduleService";
import { IconLibrary } from "../../../Util/Enumerations";
import { CompanySettingType, Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    rule: any;
    rows: any = [];
    locationGroups: any = [];
    units: any = [];
    dayTypes: any = [];
    lastCellEdited: any;
    private staffingNeedsRuleId: number = 0;
    isDirty: boolean;
    terms: any = [];
    private accounts: AccountDTO[];


    // Company settings
    private useAccountsHierarchy: boolean;

    // Subgrid
    protected rowGridOptions: ISoeGridOptions;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();
    protected sortMenuButtons = new Array<ToolBarButtonGroup>();

    // Lookups

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private $timeout: ng.ITimeoutService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private messagingService: IMessagingService,
        private translationService: ITranslationService,
        private scheduleService: IScheduleService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.initRowsGrid();

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.loadLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.staffingNeedsRuleId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_NeedsSettings_Rules, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_NeedsSettings_Rules].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_NeedsSettings_Rules].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.staffingNeedsRuleId, recordId => {
            if (recordId !== this.staffingNeedsRuleId) {
                this.staffingNeedsRuleId = recordId;
                this.onLoadData();
            }
        });
    }


    // SETUP
    private load(): ng.IPromise<any> {
        return this.scheduleService.getStaffingNeedsRule(this.staffingNeedsRuleId).then((x) => {
            this.isNew = false;
            this.rule = x;
            this.rows = x.rows;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.staffingneedsrule.staffingneedsrule"] + ' ' + this.rule.name);

            if (this.rule.accountId > 0) {
                // Insert empty
                const account: AccountDTO = new AccountDTO();
                account.accountId = 0;
                account.name = "";
                this.accounts.splice(0, 0, account);
            }

            this.setupRows();
        });
    }

    private onLoadData() {
        if (this.staffingNeedsRuleId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    // SETUP
    protected loadLookups(): ng.IPromise<any> {
        return this.loadTerms().then(() => {
            return this.$q.all([
                this.loadCompanySettings(),
                this.loadLocationGroups(),
                this.loadUnits(),
                this.loadDayTypes(),
                this.setupRowsGrid(),
                this.loadAccountStringIdsByUserFromHierarchy()]);
        });
    }

    private setupToolBar() {
        if (this.modifyPermission) {
            this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
                this.addRow();
            })));

            const group = ToolBarUtility.createSortGroup(
                () => { this.rowGridOptions.sortFirst(); this.messagingService.publish(Constants.EVENT_SET_DIRTY, {}); },
                () => { this.rowGridOptions.sortUp(); this.messagingService.publish(Constants.EVENT_SET_DIRTY, {}); },
                () => { this.rowGridOptions.sortDown(); this.messagingService.publish(Constants.EVENT_SET_DIRTY, {}); },
                () => { this.rowGridOptions.sortLast(); this.messagingService.publish(Constants.EVENT_SET_DIRTY, {}); }
            );
            this.sortMenuButtons.push(group);
        }
    }

    private initRowsGrid() {
        this.rowGridOptions = new SoeGridOptions("Time.Schedule.StaffingNeedsRules.Rows", this.$timeout, this.uiGridConstants);
        this.rowGridOptions.enableGridMenu = false;
        this.rowGridOptions.showGridFooter = false;
        this.rowGridOptions.setMinRowsToShow(10);
    }

    private setupRowsGrid() {

        this.rowGridOptions.addColumnNumber("sort", "", "10%");
        this.rowGridOptions.addColumnSelect("dayId", this.terms["common.day"], null, this.dayTypes, false, true, "dayName", "dayId", "name", "grid_dayTypeChanged");
        const colDef = this.rowGridOptions.addColumnNumber("value", this.terms["common.value"], "20%");
        colDef.enableCellEdit = true;

        _.forEach(this.rowGridOptions.getColumnDefs(), (def: uiGrid.IColumnDef) => {
            def.enableFiltering = false;
            def.enableSorting = false;
            def.enableColumnMenu = false;
        });

        if (this.modifyPermission)
            this.rowGridOptions.addColumnDelete(this.terms["core.delete"], "deleteRow");

        this.setupToolBar();
    }

    // LOOKUPS

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(), true, false, false, true).then(x => {
            this.accounts = x;
        });
    }

    private loadLocationGroups() {
        this.scheduleService.getStaffingNeedsLocationGroupsDict(false, false).then((x) => {
            this.locationGroups = x;
        });
    }

    private loadUnits() {
        this.coreService.getTermGroupContent(TermGroup.StaffingNeedsRuleUnit, false, true).then((x) => {
            this.units = x;
        });
    }

    private loadDayTypes() {
        this.scheduleService.getDayTypesAndWeekdays().then((x) => {
            let i: number = 1;
            _.forEach(x, (y: any) => {
                this.dayTypes.push({ dayId: i, dayTypeId: y.dayTypeId, weekdayNr: y.weekdayNr, name: y.name })
                i += 1;
            });
        });
    }

    private loadTerms() {
        const keys: string[] = [
            "common.day",
            "common.value",
            "core.delete",
            "time.schedule.staffingneedsrule.staffingneedsrule"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    // ACTIONS
    public save() {
        this.checkRows(true);
        this.rule.rows = this.rows;
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveStaffingNeedsRule(this.rule).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.staffingNeedsRuleId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.rule.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.staffingNeedsRuleId = result.integerValue;
                        this.rule.staffingNeedsRuleId = result.integerValue;

                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.rule);
                    }

                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.load();
            }, error => {

            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.scheduleService.getStaffingNeedsRules().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.staffingNeedsRuleId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.staffingNeedsRuleId) {
                    this.staffingNeedsRuleId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteStaffingNeedsRule(this.rule.staffingNeedsRuleId).then((result) => {
                if (result.success) {
                    completion.completed(this.rule, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    // HELP-METHODS

    private checkRows(forSaving: boolean) {
        if (this.rows) {
            _.forEach(this.rows, row => {
                row.value = forSaving ? parseFloat(row.value.toString().replace(',', '.')) : row.value.toString().replace('.', ',');
            });
        }
    }

    protected copy() {
        super.copy();
        this.isNew = true;
        this.staffingNeedsRuleId = 0;
        this.rule.staffingNeedsRuleId = 0;
        if (this.rule.rows) {
            _.forEach(this.rule.rows, row => {
                row.staffingNeedsRuleRowId = 0;
                row.staffingNeedsRuleId = 0;
            });
        }
    }

    private new() {
        this.isNew = true;
        this.staffingNeedsRuleId = 0;
        this.rule = {};
        this.rows = [];
    }

    private addRow() {
        const maxCount: number = NumberUtility.max(this.rows, "sort");
        const row = {
            sort: maxCount + 1,
            dayId: 0,
            dayTypeId: 0,
            weekday: "",
            dayName: "",
            value: 0,
        };

        this.rows.push(row);
        this.resetRows(row);
    }

    protected deleteRow(row) {
        this.dirtyHandler.setDirty();
        this.rowGridOptions.deleteRow(row);
        this.rowGridOptions.reNumberRows();
        this.resetRows(null);
    }

    protected grid_dayTypeChanged(row) {
        const obj = (_.filter(this.dayTypes, { dayId: row.dayId }))[0];
        if (obj) {
            row.dayTypeId = obj["dayTypeId"];
            row.weekday = obj["weekdayNr"];
            row.dayName = obj["name"];
        }
    }

    private setupRows() {
        if (this.rows) {
            _.forEach(this.rows, (x: any) => {
                if (x.dayTypeId !== null) {
                    let obj = (_.filter(this.dayTypes, { dayTypeId: x.dayTypeId }))[0];
                    if (obj) {
                        x["dayId"] = obj["dayId"];
                    }
                } else if (x.weekDay !== null) {
                    let obj = (_.filter(this.dayTypes, { weekdayNr: x.weekday }))[0];
                    if (obj !== null) {
                        x["dayId"] = obj["dayId"];

                    }
                }
            });

            this.resetRows(null);
        }
    }

    private resetRows(rowItem: any) {
        this.rows = _.sortBy(this.rows, 'sort');
        this.rowGridOptions.setData(this.rows);

        if (rowItem) {
            this.rowGridOptions.scrollToFocus(rowItem, 1);
        }
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.rule) {
                // Mandatory fields
                if (!this.rule.dayId)
                    mandatoryFieldKeys.push("common.day");
                if (!this.rule.value)
                    mandatoryFieldKeys.push("common.value");
            }
        });
    }

    grid: IGridHandler;
}
