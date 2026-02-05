import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ISoeGridOptionsAg, SoeGridOptionsAg, TypeAheadOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IReportService } from "../../../Core/Services/ReportService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { DistributionCodeGridDTO, DistributionCodeHeadDTO, DistributionCodePeriodDTO } from "../../../Common/Models/DistributionCodeHeadDTO";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { NumberUtility } from "../../../Util/NumberUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature, TermGroup_AccountingBudgetType, TermGroup_AccountingBudgetSubType, TermGroup } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IFocusService } from "../../../Core/Services/focusservice";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private distributionCode: DistributionCodeHeadDTO;
    private distributionCodeHeadId: number;

    // Grids
    private distributioncodePeriodGridOptions: ISoeGridOptionsAg;
    private gridButtonGroups = new Array<ToolBarButtonGroup>();

    private gridFooterComponentUrl: string;
    private sumPeriods = 0;
    private sumPercent = 0;
    private diff = 0;

    // Lookups 
    private types: ISmallGenericType[];
    private subTypes: ISmallGenericType[];
    private openingHours: SmallGenericType[];
    private distributionCodes: DistributionCodeGridDTO[];
    private distributionCodesDict: ISmallGenericType[];
    private accountDims: AccountDimSmallDTO[];

    // Properties
    get numberOfPeriods() {
        return this.distributionCode.noOfPeriods;
    }
    set numberOfPeriods(item: any) {
        this.distributionCode.noOfPeriods = item;
        this.onPeriodChange();
    }

    private get hideToolbar(): boolean {
        return (!this.distributionCode || this.distributionCode.typeId !== TermGroup_AccountingBudgetType.AccountingBudget);
    }

    private get showOpeningHours(): boolean {
        return this.distributionCode && this.distributionCode.typeId !== TermGroup_AccountingBudgetType.AccountingBudget && this.distributionCode.subType === TermGroup_AccountingBudgetSubType.Day;
    }

    //@ngInject
    constructor(
        private accountingService: IAccountingService,
        private reportService: IReportService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private $timeout: ng.ITimeoutService,
        private notificationService: INotificationService,
        private focusService: IFocusService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private uiGridConstants: uiGrid.IUiGridConstants) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.gridFooterComponentUrl = urlHelperService.getViewUrl("gridFooter.html");

        this.setupSubGrids();
        this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.newperiod", "common.newperiod", IconLibrary.FontAwesome, "fal fa-plus", () => {
            this.addRow(0);
            this.calculateDistributionCode();
            this.distributioncodePeriodGridOptions.setData(this.distributionCode.periods);
        })));
    }

    // SETUP

    public onInit(parameters: any) {
        this.distributionCodeHeadId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Preferences_VoucherSettings_DistributionCodes_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Preferences_VoucherSettings_DistributionCodes_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Preferences_VoucherSettings_DistributionCodes_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    private setupSubGrids() {
        this.distributioncodePeriodGridOptions = new SoeGridOptionsAg("Economy.Accounting.DistributionCode.Periods", this.$timeout);
        this.distributioncodePeriodGridOptions.enableGridMenu = false;

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.distributioncodePeriodGridOptions.subscribe(events);

        var keys: string[] = [
            "common.periodnumber",
            "common.portionprocentage",
            "common.comment",
            "core.delete",
            "economy.accounting.distributioncode.diffValidation",
            "economy.accounting.distributioncode.sumperiods",
            "economy.accounting.distributioncode.sumpercent",
            "economy.accounting.distributioncode.diff",
            "economy.accounting.distributioncode.subtypetext",
            "economy.accounting.distributioncode.week",
            "common.week",
            "economy.accounting.salesbudget.subtyperow",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.distributioncodePeriodGridOptions.addColumnText(this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.number), terms["common.periodnumber"], null);
            var distributionCodeOptions = new TypeAheadOptionsAg();
            distributionCodeOptions.source = (filter) => this.filterDistributionCodes(filter);
            distributionCodeOptions.minLength = 0;
            distributionCodeOptions.delay = 0;
            distributionCodeOptions.displayField = "name"
            distributionCodeOptions.dataField = "name";
            distributionCodeOptions.useScroll = true;
            distributionCodeOptions.allowNavigationFromTypeAhead = () => { return true };
            let codeColumn = this.distributioncodePeriodGridOptions.addColumnTypeAhead(this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.parentToDistributionCodePeriodName), terms["economy.accounting.salesbudget.subtyperow"], 500, { typeAheadOptions: distributionCodeOptions, editable: true });
            codeColumn.hide = true; //initially hidden
            this.distributioncodePeriodGridOptions.addColumnText(this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.periodSubTypeName), terms["economy.accounting.distributioncode.subtypetext"], null, { editable: true });
            this.distributioncodePeriodGridOptions.addColumnNumber(this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.percent), terms["common.portionprocentage"] + " (%)", null, { decimals: 2, editable: true, ignoreFormatDecimals: true });
            this.distributioncodePeriodGridOptions.addColumnText(this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.comment), terms["common.comment"], null, { editable: true });

            this.distributioncodePeriodGridOptions.finalizeInitGrid();
        });

        //TODO: sub to these
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => {
            if (colDef.field === this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.parentToDistributionCodePeriodName)) {
                if (newValue !== oldValue) {
                    var distributionCode = _.find(this.distributionCodes, { name: newValue });
                    if (distributionCode) {
                        entity.parentToDistributionCodePeriodId = distributionCode.distributionCodeHeadId;
                        entity.parentToDistributionCodePeriodChanged = true;
                        this.dirtyHandler.setDirty();
                    }
                }
            } else if (colDef.field === this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.periodSubTypeName)) {
                if (newValue !== oldValue)
                    this.dirtyHandler.setDirty();
            } else {
                let cleanedPercent = this.cleanPercent(entity.percent);
                if (cleanedPercent)
                    entity.percent = cleanedPercent;
                this.calculateDistributionCode();
                this.dirtyHandler.setDirty();
            }
        }));
        this.distributioncodePeriodGridOptions.subscribe(events);
    }

    // SERVICE CALLS

    private onLoadData(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadOpeningHours(),
            () => this.loadDistributionCodes(),
            () => this.loadSubTypes(),
            () => this.loadTypes(),
            () => this.loadAccountDims(),
        ]).then(() => {
            if (this.distributionCodeHeadId > 0) {
                this.isNew = false;
                this.load();
            } else {
                this.new();
            }
        });
    }

    private loadOpeningHours(): ng.IPromise<any> {
        return this.accountingService.getOpeningHoursDict(false, true, true).then(x => {
            this.openingHours = x;
        });
    }

    private loadDistributionCodes(): ng.IPromise<any> {
        this.distributionCodesDict = [];
        return this.accountingService.getDistributionCodesForGrid().then((x) => {
            this.distributionCodes = x;
            //Have to leave itself out
            this.distributionCodesDict.push({ id: 0, name: " " });
            if (this.distributionCodeHeadId > 0) {
                _.forEach(x, (row) => {
                    if (row.distributionCodeHeadId != this.distributionCodeHeadId)
                        this.distributionCodesDict.push({ id: row.distributionCodeHeadId, name: row.name });
                });
            } else {
                _.forEach(x, (row) => {
                    this.distributionCodesDict.push({ id: row.distributionCodeHeadId, name: row.name });
                });
            }
        });
    }

    private loadSubTypes(): ng.IPromise<any> {
        this.subTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.AccountingBudgetSubType, true, false, true).then((x) => {
            //Sort subtypes - putting year - month last
            _.forEach(_.filter(x, (s) => s.id > 0), (subType) => {
                this.subTypes.push(subType);
            });
            this.subTypes.push(x[0]);
        });
    }

    private loadTypes(): ng.IPromise<any> {
        this.types = [];
        return this.coreService.getTermGroupContent(TermGroup.AccountingBudgetType, true, false).then((x) => {
            //Have to leave staffBudget out
            _.forEach(x, (row) => {
                if (row.id != TermGroup_AccountingBudgetType.StaffBudget && row.id != TermGroup_AccountingBudgetType.ProjectBudget)
                    this.types.push({ id: row.id, name: row.name });
            });
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, true, false, false, true).then(dims => {
            this.accountDims = dims;
        });
    }

    private load(): ng.IPromise<any> {
        return this.accountingService.getDistributionCode(this.distributionCodeHeadId).then(x => {
            this.distributionCode = x;
            this.dirtyHandler.clean();
            if (this.distributionCode.typeId === TermGroup_AccountingBudgetType.AccountingBudget) {
                this.distributioncodePeriodGridOptions.setData(this.distributionCode.periods);
                this.calculateDistributionCode();
            } else {
                _.forEach(this.distributionCode.periods, (p: DistributionCodePeriodDTO) => {
                    if (p.parentToDistributionCodePeriodId) {
                        var code = _.find(this.distributionCodes, c => c.distributionCodeHeadId === p.parentToDistributionCodePeriodId);
                        if (code)
                            p.parentToDistributionCodePeriodName = code.name;
                    }
                });

                this.setSubTypeNamesForExistingCode();
            }
            this.showDistributionCodeColumn();
        });
    }

    // ACTIONS

    private new() {
        this.isNew = true;
        this.distributionCodeHeadId = 0;
        this.distributionCode = new DistributionCodeHeadDTO();
        this.distributionCode.typeId = 1;
        this.distributionCode.noOfPeriods = 0;
        this.distributionCode.periods = [];
    }

    protected copy() {
        if (!this.distributionCode)
            return;

        this.isNew = true;
        this.distributionCodeHeadId = 0;
        this.distributionCode.distributionCodeHeadId = 0;
        this.distributionCode.created = null;
        this.distributionCode.createdBy = "";
        this.distributionCode.modified = null;
        this.distributionCode.modifiedBy = "";
        this.distributionCode.name = "";
        _.forEach(this.distributionCode.periods, period => {
            period.distributionCodePeriodId = 0;
        });

        this.dirtyHandler.setDirty();
        this.focusService.focusByName("ctrl_distributionCode_name");
        this.translationService.translate("economy.accounting.distributioncode.new_distributioncode").then((term) => {
            this.messagingHandler.publishSetTabLabel(this.guid, term);
        });
    }

    public delete() {
        // Check if code is used in either budget or in another distribution code
        if (this.distributionCode.isInUse) {
            var keys: string[] = [
                "core.warning",
                "economy.accounting.distributioncode.inuse"
            ];

            // Show dialog
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["core.warning"], terms["economy.accounting.distributioncode.inuse"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            });
            return;
        }

        // Delete code
        this.progress.startDeleteProgress((completion) => {
            this.accountingService.deleteDistributionCode(this.distributionCode.distributionCodeHeadId).then((result) => {
                if (result.success) {
                    completion.completed(this.distributionCode, true);
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

    public save() {
        //Clean fields before saving
        if (this.distributionCode.typeId === TermGroup_AccountingBudgetType.AccountingBudget) {
            this.distributionCode.openingHoursId = undefined;
            this.distributionCode.parentId = undefined;
            this.distributionCode.subType = undefined;
        }

        this.progress.startSaveProgress((completion) => {
            this.dirtyHandler.clean();
            this.accountingService.saveDistributionCode(this.distributionCode).then((result) => {
                if (result.success) {
                    // Clear cache
                    this.accountingService.getDistributionCodes(false, false);
                    this.accountingService.getDistributionCodes(true, false);

                    if (result.integerValue && result.integerValue > 0)
                        this.distributionCodeHeadId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.distributionCode);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.onLoadData();
            }, error => {

            });
    }

    // HELP-METHODS

    private cleanPercent(val) {
        if (!val)
            return 0;

        return val.round(2);
    }

    private getRestPercent() {
        var sum = 0;
        _.forEach(this.distributionCode.periods, (r) => {
            sum = NumberUtility.parseDecimal((sum + r.percent).toFixed(2));
        });
        return NumberUtility.parseDecimal((100 - sum).toFixed(2));
    }

    private filterDistributionCodes(filter) {
        return this.distributionCodesDict.filter(distributionCode => {
            return distributionCode.name.contains(filter);
        });
    }

    private setSubTypeNamesForExistingCode() {
        if (this.distributionCode) {
            switch (this.distributionCode.subType) {
                case TermGroup_AccountingBudgetSubType.Year:
                    _.forEach(this.distributionCode.periods, (period) => {
                        period.periodSubTypeName = CalendarUtility.getMonthName(period.number - 1).toUpperCaseFirstLetter();
                    });
                    break;
                case TermGroup_AccountingBudgetSubType.January:
                case TermGroup_AccountingBudgetSubType.February:
                case TermGroup_AccountingBudgetSubType.March:
                case TermGroup_AccountingBudgetSubType.April:
                case TermGroup_AccountingBudgetSubType.May:
                case TermGroup_AccountingBudgetSubType.June:
                case TermGroup_AccountingBudgetSubType.July:
                case TermGroup_AccountingBudgetSubType.August:
                case TermGroup_AccountingBudgetSubType.September:
                case TermGroup_AccountingBudgetSubType.October:
                case TermGroup_AccountingBudgetSubType.November:
                case TermGroup_AccountingBudgetSubType.December:
                    _.forEach(this.distributionCode.periods, (period) => {
                        period.periodSubTypeName = (period.number).toString() + ".";
                    });
                    break;
                case TermGroup_AccountingBudgetSubType.Week:
                    _.forEach(this.distributionCode.periods, (period) => {
                        period.periodSubTypeName = CalendarUtility.getDayName(period.number).toUpperCaseFirstLetter();
                    });
                    break;
                case TermGroup_AccountingBudgetSubType.Day:
                    if (this.distributionCode.openingHoursId > 0) {
                        //OpeningHour Connected to distributionCode
                        return this.accountingService.getOpeningHour(this.distributionCode.openingHoursId).then(x => {
                            var startTimeFrom = x.openingTime.getHours();
                            for (var i = 0; i < this.distributionCode.periods.length; i++) {
                                this.distributionCode.periods[i].periodSubTypeName = CalendarUtility.toFormattedTime(new Date(1900, 1, 1, i + startTimeFrom, 0, 0, 0), false) + "-" + CalendarUtility.toFormattedTime(new Date(1900, 1, 1, i + startTimeFrom + 1, 0, 0, 0), false);
                            }

                            this.distributioncodePeriodGridOptions.setData(this.distributionCode.periods);
                            this.calculateDistributionCode();
                        });
                    } else {
                        _.forEach(this.distributionCode.periods, (period) => {
                            period.periodSubTypeName = CalendarUtility.toFormattedTime(new Date(1900, 1, 1, period.number, 0, 0, 0), false) + "-" + CalendarUtility.toFormattedTime(new Date(1900, 1, 1, period.number + 1, 0, 0, 0), false);
                        });
                    }
                    break;
                default:
                    break;
            }
        }
        this.distributioncodePeriodGridOptions.setData(this.distributionCode.periods);
        this.calculateDistributionCode();
    }

    private showDistributionCodeColumn() {
        this.$timeout(() => {
            if ((this.distributionCode.typeId === TermGroup_AccountingBudgetType.AccountingBudget || this.distributionCode.typeId === TermGroup_AccountingBudgetType.ProjectBudget) || this.distributionCode.subType === TermGroup_AccountingBudgetSubType.Day) {
                if (this.distributionCode.typeId === TermGroup_AccountingBudgetType.AccountingBudget)
                    this.distributioncodePeriodGridOptions.hideColumn(this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.periodSubTypeName));
                this.distributioncodePeriodGridOptions.hideColumn(this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.parentToDistributionCodePeriodName));
                this.distributioncodePeriodGridOptions.sizeColumnToFit();
            } else {
                this.distributioncodePeriodGridOptions.showColumn(this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.periodSubTypeName));
                this.distributioncodePeriodGridOptions.showColumn(this.interfacePropertyToString((o: DistributionCodePeriodDTO) => o.parentToDistributionCodePeriodName));
                this.distributioncodePeriodGridOptions.sizeColumnToFit();
            }
        });
    }

    private addRow(percent: number): void {
        this.distributionCode.periods.push(new DistributionCodePeriodDTO(this.distributionCode.periods.length + 1, percent));
    }

    private initSetupPeriods(nbrOfPeriods: number) {
        this.distributionCode.noOfPeriods = nbrOfPeriods;

        this.distributionCodesDict = [];
        this.distributionCodesDict.push({ id: 0, name: " " });
    }

    private setUpYear() {
        this.initSetupPeriods(12);

        // Filter distribution codes
        _.forEach(_.filter(this.distributionCodes, (c) => c.typeId === this.distributionCode.typeId), (d) => {
            if (d.typeOfPeriodId > TermGroup_AccountingBudgetSubType.Year && d.typeOfPeriodId < TermGroup_AccountingBudgetSubType.Week) {
                this.distributionCodesDict.push({ id: d.distributionCodeHeadId, name: d.name });
            } else {
                if (this.distributionCode.parentId && this.distributionCode.parentId === d.distributionCodeHeadId)
                    this.distributionCode.parentId = undefined;
            }
        });

        this.onSubTypeChanged(this.distributionCode.noOfPeriods);
    }

    private setUpYearWeek(weeks: number) {
        this.initSetupPeriods(weeks);

        // Filter distribution codes
        _.forEach(_.filter(this.distributionCodes, (c) => c.typeId === this.distributionCode.typeId), (d) => {
            if (d.typeOfPeriodId === TermGroup_AccountingBudgetSubType.Week) {
                this.distributionCodesDict.push({ id: d.distributionCodeHeadId, name: d.name });
            } else {
                if (this.distributionCode.parentId && this.distributionCode.parentId === d.distributionCodeHeadId)
                    this.distributionCode.parentId = undefined;
            }
        });

        this.onSubTypeChanged(this.distributionCode.noOfPeriods);
    }

    private setUpMonth(nbrOfPeriodsInMonth: number) {
        this.initSetupPeriods(nbrOfPeriodsInMonth);

        // Filter distribution codes
        _.forEach(_.filter(this.distributionCodes, (c) => c.typeId === this.distributionCode.typeId), (d) => {
            if (d.typeOfPeriodId === TermGroup_AccountingBudgetSubType.Day) {
                this.distributionCodesDict.push({ id: d.distributionCodeHeadId, name: d.name });
            } else {
                if (this.distributionCode.parentId && this.distributionCode.parentId === d.distributionCodeHeadId)
                    this.distributionCode.parentId = undefined;
            }
        });

        this.onSubTypeChanged(this.distributionCode.noOfPeriods);
    }

    private setUpWeek() {
        this.initSetupPeriods(7);

        // Filter distribution codes
        _.forEach(_.filter(this.distributionCodes, (c) => c.typeId === this.distributionCode.typeId), (d) => {
            if (d.typeOfPeriodId === TermGroup_AccountingBudgetSubType.Day) {
                this.distributionCodesDict.push({ id: d.distributionCodeHeadId, name: d.name });
            } else {
                if (this.distributionCode.parentId && this.distributionCode.parentId === d.distributionCodeHeadId)
                    this.distributionCode.parentId = undefined;
            }
        });

        this.onSubTypeChanged(this.distributionCode.noOfPeriods);
    }

    private setUpDay() {
        this.distributionCode.noOfPeriods = 24;

        // Filter distribution codes
        this.distributionCodesDict = [];

        this.onSubTypeChanged(this.distributionCode.noOfPeriods);
    }

    private interfacePropertyToString = (property: (object: any) => void) => {
        var chaine = property.toString();
        var arr = chaine.match(/[\s\S]*{[\s\S]*\.([^\.; ]*)[ ;\n]*}/);
        return arr[1];
    };

    private calculateDistributionCode() {
        this.$timeout(() => {
            this.sumPeriods = this.distributionCode.periods.length;
            this.sumPercent = this.calculatePeriodSumPercent();
            this.diff = this.calculatePeriodDiffPercent();
        }, 10)
    }

    private calculatePeriodSumPercent() {
        var sum = 0;
        for (var i = 0; i < this.distributionCode.periods.length; i++) {
            sum = NumberUtility.parseDecimal((sum + this.distributionCode.periods[i].percent).toFixed(2));
        }
        return sum;
    }

    private calculatePeriodDiffPercent() {
        return NumberUtility.parseDecimal((this.calculatePeriodSumPercent() - 100).toFixed(2));
    }

    // EVENTS

    private typeChanged(row) {
        if (this.distributionCode) {
            this.distributionCode.subType = undefined;
            this.distributionCode.parentId = undefined;
            this.distributionCode.periods = [];
            this.distributioncodePeriodGridOptions.clearColumnDefs(); // This actually just clears the data

            // Set column visibility
            this.showDistributionCodeColumn();
        }
    }

    private onSubTypeChanged(nbrOfPeriods) {
        if (this.distributionCode) {
            this.distributionCode.periods = new Array<DistributionCodePeriodDTO>();

            var percent = 100 / nbrOfPeriods;
            this.distributionCode.periods.length = 0;
            var totSum = 0;
            var subTypeName: string = "";
            for (var i = 0; i < nbrOfPeriods; i++) {
                switch (this.distributionCode.subType) {
                    case TermGroup_AccountingBudgetSubType.Year:
                        subTypeName = CalendarUtility.getMonthName(i).toUpperCaseFirstLetter();
                        break;
                    case TermGroup_AccountingBudgetSubType.YearWeek:
                        subTypeName = this.terms["common.week"] + " " + (i + 1).toString();
                        break;
                    case TermGroup_AccountingBudgetSubType.January:
                    case TermGroup_AccountingBudgetSubType.February:
                    case TermGroup_AccountingBudgetSubType.March:
                    case TermGroup_AccountingBudgetSubType.April:
                    case TermGroup_AccountingBudgetSubType.May:
                    case TermGroup_AccountingBudgetSubType.June:
                    case TermGroup_AccountingBudgetSubType.July:
                    case TermGroup_AccountingBudgetSubType.August:
                    case TermGroup_AccountingBudgetSubType.September:
                    case TermGroup_AccountingBudgetSubType.October:
                    case TermGroup_AccountingBudgetSubType.November:
                    case TermGroup_AccountingBudgetSubType.December:
                        subTypeName = (i + 1).toString() + ".";
                        break;
                    case TermGroup_AccountingBudgetSubType.Week:
                        subTypeName = CalendarUtility.getDayName(i + 1).toUpperCaseFirstLetter();
                        break;
                    case TermGroup_AccountingBudgetSubType.Day:
                        subTypeName = CalendarUtility.toFormattedTime(new Date(1900, 1, 1, i, 0, 0, 0), false) + "-" + CalendarUtility.toFormattedTime(new Date(1900, 1, 1, i + 1, 0, 0, 0), false);
                        break;
                }

                if (i === nbrOfPeriods - 1) {
                    this.distributionCode.periods.push(new DistributionCodePeriodDTO(this.distributionCode.periods.length + 1, this.getRestPercent(), subTypeName));
                } else {
                    var cleandPercent = this.cleanPercent(percent);
                    this.distributionCode.periods.push(new DistributionCodePeriodDTO(this.distributionCode.periods.length + 1, cleandPercent, subTypeName));
                    totSum = totSum + cleandPercent;
                }
            }
            this.distributioncodePeriodGridOptions.setData(this.distributionCode.periods)
            this.calculateDistributionCode();
        }
    }

    private subTypeChanged() {
        this.$timeout(() => {
            let date = this.distributionCode.fromDate ? this.distributionCode.fromDate : CalendarUtility.getDateToday();

            var selectedSubType: number = this.distributionCode.subType;
            switch (selectedSubType) {
                case TermGroup_AccountingBudgetSubType.Year:
                    this.setUpYear();
                    break;
                case TermGroup_AccountingBudgetSubType.YearWeek:
                    this.setUpYearWeek(date.weeksInYear());
                    break;
                case TermGroup_AccountingBudgetSubType.January:
                    this.setUpMonth(31);
                    break;
                case TermGroup_AccountingBudgetSubType.February:
                    this.setUpMonth(new Date(date.year(), 2, 0).getDate());
                    break;
                case TermGroup_AccountingBudgetSubType.March:
                    this.setUpMonth(31);
                    break;
                case TermGroup_AccountingBudgetSubType.April:
                    this.setUpMonth(30);
                    break;
                case TermGroup_AccountingBudgetSubType.May:
                    this.setUpMonth(31);
                    break;
                case TermGroup_AccountingBudgetSubType.June:
                    this.setUpMonth(30);
                    break;
                case TermGroup_AccountingBudgetSubType.July:
                    this.setUpMonth(31);
                    break;
                case TermGroup_AccountingBudgetSubType.August:
                    this.setUpMonth(31);
                    break;
                case TermGroup_AccountingBudgetSubType.September:
                    this.setUpMonth(30);
                    break;
                case TermGroup_AccountingBudgetSubType.October:
                    this.setUpMonth(31);
                    break;
                case TermGroup_AccountingBudgetSubType.November:
                    this.setUpMonth(30);
                    break;
                case TermGroup_AccountingBudgetSubType.December:
                    this.setUpMonth(31);
                    break;
                case TermGroup_AccountingBudgetSubType.Week:
                    this.setUpWeek();
                    break;
                case TermGroup_AccountingBudgetSubType.Day:
                    this.setUpDay();
                    break;
                default:
                    break;
            }

            // Empty opening hours if type != day
            if (selectedSubType != TermGroup_AccountingBudgetSubType.Day && this.distributionCode)
                this.distributionCode.openingHoursId = undefined;

            // Set column visibility
            this.showDistributionCodeColumn();
        });
    }

    private onOpeningHourChanged(start, nbrOfHours) {
        if (this.distributionCode) {
            this.distributionCode.periods = new Array<DistributionCodePeriodDTO>();

            var percent = 100 / nbrOfHours;

            this.distributionCode.periods.length = 0;
            var totSum = 0;
            var subTypeName: string = "";
            for (var i = start; i < nbrOfHours + start; i++) {
                subTypeName = CalendarUtility.toFormattedTime(new Date(1900, 1, 1, i, 0, 0, 0), false) + "-" + CalendarUtility.toFormattedTime(new Date(1900, 1, 1, i + 1, 0, 0, 0), false);

                if (i === (nbrOfHours + start) - 1) {
                    this.distributionCode.periods.push(new DistributionCodePeriodDTO(this.distributionCode.periods.length + 1, this.getRestPercent(), subTypeName));
                } else {
                    var cleandPercent = this.cleanPercent(percent);
                    this.distributionCode.periods.push(new DistributionCodePeriodDTO(this.distributionCode.periods.length + 1, cleandPercent, subTypeName));
                    totSum = totSum + +cleandPercent;
                }
            }
            this.distributioncodePeriodGridOptions.setData(this.distributionCode.periods);
        }
    }

    private openingHoursChanged() {
        this.$timeout(() => {
            if (this.distributionCode && this.distributionCode.openingHoursId) {
                return this.accountingService.getOpeningHour(this.distributionCode.openingHoursId).then(x => {
                    var timeFrom: Date = x.openingTime;
                    var timeTo: Date = x.closingTime;
                    var hours = Math.abs(timeTo.getTime() - timeFrom.getTime()) / (3600 * 1000);
                    //Create periods from times
                    this.distributionCode.noOfPeriods = hours;
                    this.onOpeningHourChanged(timeFrom.getHours(), hours);
                });
            }
        });
    }

    private dateChanged() {
        this.$timeout(() => {
            this.subTypeChanged();
        });
    }

    private onPeriodChange() {
        if (this.distributionCode) {
            // Clear
            this.distributionCode.periods = [];
            var percent = 100 / this.distributionCode.noOfPeriods;
            var totSum = 0;
            for (var i = 0; i < this.distributionCode.noOfPeriods; i++) {
                if (i === this.distributionCode.noOfPeriods - 1) {
                    this.addRow(this.cleanPercent(((100 - totSum) * 100.0) / 100.0));
                } else {
                    var cleandPercent = this.cleanPercent(percent);
                    this.addRow(cleandPercent);
                    totSum = totSum + +cleandPercent;
                }
            }
            this.calculateDistributionCode();
            this.distributioncodePeriodGridOptions.setData(this.distributionCode.periods);
        }
    }

    private standardDistributionCodeChanged(code) {
        if (this.distributionCode && this.distributionCode.periods) {
            var distributionCode = _.find(this.distributionCodes, (d) => d.distributionCodeHeadId === code);
            if (distributionCode) {
                _.forEach(this.distributionCode.periods, (period: DistributionCodePeriodDTO) => {
                    if (!period.parentToDistributionCodePeriodChanged) {
                        period.parentToDistributionCodePeriodId = distributionCode.distributionCodeHeadId;
                        period.parentToDistributionCodePeriodName = distributionCode.name;
                        this.distributioncodePeriodGridOptions.refreshRows(period);
                    }
                });
            }
        }
    }

    private afterCellEdit(row: any, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        if (colDef.field === 'percent') {
            var percent: number = NumberUtility.parseDecimal(newValue);
            row.percent = percent;
        }
    }

    // VALIDATION

    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.distributionCode) {
                if (!this.distributionCode.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (this.distributionCode.typeId > 3 && (!this.distributionCode.accountDimId || this.distributionCode.accountDimId === 0)) {
                    mandatoryFieldKeys.push("common.accountdim");
                }
                if (this.distributionCode.periods) {
                    var sum = 0;
                    for (var i = 0; i < this.distributionCode.periods.length; i++) {
                        sum = NumberUtility.parseDecimal((sum + this.distributionCode.periods[i].percent).toFixed(2));
                    }

                    if (sum !== 100) {
                        validationErrorKeys.push("economy.accounting.distributioncode.diffValidation");
                    }
                }
            }
        });
    }
}