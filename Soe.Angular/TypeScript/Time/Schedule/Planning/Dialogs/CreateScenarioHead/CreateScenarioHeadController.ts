import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { TimeScheduleScenarioHeadDTO, TimeScheduleScenarioEmployeeDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { EmployeeAccountDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { TermGroup_TimeSchedulePlanningViews, TermGroup_TimeScheduleScenarioHeadSourceType, TermGroup, Feature, TimeSchedulePlanningDisplayMode } from "../../../../../Util/CommonEnumerations";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EditController as EmployeeEditController } from "../../../../Employee/Employees/EditController";
import { Constants } from "../../../../../Util/Constants";
import { CreateVacantEmployeesDialogController } from "../../../../Employee/Employees/Dialogs/CreateVacantEmployees/CreateVacantEmployeesDialogController";

export class CreateScenarioHeadController {

    // Terms
    private terms: { [index: string]: string; };
    private fromScenarioLabel: string;
    private sourceTypeName: string;
    private offsetText: string;
    private lengthDiffText: string;

    // Permissions
    private modifyEmployeePermission: boolean = false;
    private createVacantPermission: boolean = false;

    // Lookups
    private sourceTypes: ISmallGenericType[] = [];
    private allEmployees: EmployeeListDTO[] = [];
    private filteredEmployees: TimeScheduleScenarioEmployeeDTO[] = [];
    private progress: IProgressHandler;

    // Flags
    private isNew: boolean = false;
    private isValidToSave: boolean = false;
    private firstSelect: boolean = true;

    // Properties
    private scenarioHead: TimeScheduleScenarioHeadDTO;
    private selectedAccountIds: number[] = [];
    private selectedEmployees: TimeScheduleScenarioEmployeeDTO[] = [];
    private initialSelectedEmployeeIds: number[] = [];
    private length: number;

    private dateOptions = {
        minDate: null,
        maxDate: null
    };

    private get isScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Day || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Schedule;
    }

    private get isTemplateView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateSchedule;
    }

    private get isScenarioView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioComplete;
    }

    private _fromSchedule: boolean;
    private get fromSchedule(): boolean {
        return this._fromSchedule;
    }
    private set fromSchedule(value: boolean) {
        this._fromSchedule = value;
        this._fromTemplate = false;
        this._fromScenario = false;
        this.dateFunction = 0;
        this.scenarioHead.sourceType = value ? TermGroup_TimeScheduleScenarioHeadSourceType.Schedule : TermGroup_TimeScheduleScenarioHeadSourceType.Empty;
        this.setSourceTypeName();
        this.filterEmployees();
    }

    private _fromTemplate: boolean;
    private get fromTemplate(): boolean {
        return this._fromTemplate;
    }
    private set fromTemplate(value: boolean) {
        this._fromTemplate = value;
        this._fromSchedule = false;
        this._fromScenario = false;
        this.dateFunction = 0;
        this.scenarioHead.sourceType = value ? TermGroup_TimeScheduleScenarioHeadSourceType.Template : TermGroup_TimeScheduleScenarioHeadSourceType.Empty;
        this.setSourceTypeName();
        this.filterEmployees();
    }

    private _fromScenario: boolean;
    private get fromScenario(): boolean {
        return this._fromScenario;
    }
    private set fromScenario(value: boolean) {
        this._fromScenario = value;
        this._fromSchedule = false;
        this._fromTemplate = false;
        this.dateFunction = 0;
        if (value) {
            this.scenarioHead.sourceType = TermGroup_TimeScheduleScenarioHeadSourceType.Scenario;
            this.scenarioHead.sourceDateFrom = this.scenarioHead.dateFrom = this.scenarioDateFrom;
            this.scenarioHead.sourceDateTo = this.scenarioHead.dateTo = this.scenarioDateTo;
        } else {
            this.scenarioHead.sourceType = TermGroup_TimeScheduleScenarioHeadSourceType.Empty;
            this.scenarioHead.sourceDateFrom = this.scenarioHead.dateFrom = this.dateFrom;
            this.scenarioHead.sourceDateTo = this.scenarioHead.dateTo = this.dateTo;
        }

        this.calculateLength();
        this.setSourceTypeName();
        this.filterEmployees();
    }

    private _includeAbsence: boolean = true;
    private get includeAbsence(): boolean {
        return this._includeAbsence;
    }
    private set includeAbsence(value: boolean) {
        this._includeAbsence = value;
    }

    private _showAllEmployees: boolean;
    private get showAllEmployees(): boolean {
        return this._showAllEmployees;
    }
    private set showAllEmployees(value: boolean) {
        this._showAllEmployees = value;
        this.filterEmployees();
    }

    private get showAccounts(): boolean {
        return this.useAccountHierarchy && this.accountDim.accounts.length !== 1;
    }

    private _dateFunction = 0;
    private get dateFunction(): number {
        return this._dateFunction;
    }
    private set dateFunction(value: number) {
        this._dateFunction = value;

        switch (value) {
            case 0:
            case 1:
                this.dateOptions = {
                    minDate: null,
                    maxDate: null
                };
                break;
            case 2:
                this.dateOptions = {
                    minDate: this.scenarioHead.sourceDateFrom,
                    maxDate: this.scenarioHead.sourceDateTo
                };
                break;
        }

        // Reset dates from source
        this.scenarioHead.dateFrom = this.scenarioHead.sourceDateFrom;
        this.scenarioHead.dateTo = this.scenarioHead.sourceDateTo;
        this.dateFromChanged();
    }

    // Grid
    private gridHandlerAcc: EmbeddedGridController;
    private gridHandlerEmp: EmbeddedGridController;

    private modalInstance: any;

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private sharedScheduleService: IScheduleService,
        progressHandlerFactory: IProgressHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private viewDefinition: TermGroup_TimeSchedulePlanningViews,
        private useVacant: boolean,
        private useAccountHierarchy: boolean,
        private accountHierarchyId: string,
        private accountDim: AccountDimSmallDTO,
        private filteredAccountIds: number[],
        private filteredCategoryIds: number[],
        private sourceEmployees: EmployeeListDTO[],
        private includeSecondaryCategoriesOrAccounts: boolean,
        private currentScenarioHeadId: number,
        scenarioHead: TimeScheduleScenarioHeadDTO,
        private scenarioName: string,
        private scenarioDateFrom: Date,
        private scenarioDateTo: Date,
        private dateFrom: Date,
        private dateTo: Date) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.modalInstance = $uibModal;

        this.scenarioHead = new TimeScheduleScenarioHeadDTO();
        if (scenarioHead) {
            angular.extend(this.scenarioHead, scenarioHead);
            this.selectedAccountIds = this.scenarioHead.accounts.map(a => a.accountId);
            this.selectedEmployees = this.scenarioHead.employees;
            this.initialSelectedEmployeeIds = this.scenarioHead.employees.map(e => e.employeeId);
        } else {
            this.isNew = true;
            this.scenarioHead.sourceDateFrom = this.scenarioHead.dateFrom = this.dateFrom;
            this.scenarioHead.sourceDateTo = this.scenarioHead.dateTo = this.dateTo;
            if (this.isScheduleView) {
                this._fromSchedule = true;
                this.scenarioHead.sourceType = TermGroup_TimeScheduleScenarioHeadSourceType.Schedule;
            } else if (this.isTemplateView) {
                this._fromTemplate = true;
                this.scenarioHead.sourceType = TermGroup_TimeScheduleScenarioHeadSourceType.Template;
            } else if (this.isScenarioView) {
                this._showAllEmployees = true;
                this.scenarioHead.sourceType = TermGroup_TimeScheduleScenarioHeadSourceType.Empty;
            }
        }
        this.calculateLength();

        if (this.useAccountHierarchy) {
            this.gridHandlerAcc = new EmbeddedGridController(gridHandlerFactory, "CreateScenarioHead.Accounts");
            this.gridHandlerAcc.gridAg.options.enableGridMenu = false;
            this.gridHandlerAcc.gridAg.options.setMinRowsToShow(15);
        }

        this.gridHandlerEmp = new EmbeddedGridController(gridHandlerFactory, "CreateScenarioHead.Employees");
        this.gridHandlerEmp.gridAg.options.enableGridMenu = false;
        this.gridHandlerEmp.gridAg.options.setMinRowsToShow(15);

        this.progress.startLoadingProgress([() => {
            return this.loadLookups().then(() => {
                this.setSourceTypeName();
                this.setupGrids();

                if (this.useAccountHierarchy) {
                    this.setAccount();
                    this.gridHandlerAcc.gridAg.setData(this.accountDim.accounts);
                    this.selectAccounts();
                }

                this.filterEmployees();
            });
        }]);
    }

    private calculateLength() {
        this.length = this.scenarioHead.dateTo.diffDays(this.scenarioHead.dateFrom) + 1;
    }

    private setupGrids() {
        if (this.useAccountHierarchy) {
            this.gridHandlerAcc.gridAg.addColumnText("name", this.accountDim.name, null);
            this.gridHandlerAcc.gridAg.options.setStandardSubscriptions((rows: any[]) => this.onAccountGridRowSelected(rows));
            this.gridHandlerAcc.gridAg.finalizeInitGrid(null, true, "account-totals-grid");
            if (this.firstSelect) {
                this.selectedAccountIds = this.gridHandlerAcc.gridAg.options.getSelectedIds('accountId');
                this.filterEmployees();
                this.setIsValidToSave();
            }
        }

        this.gridHandlerEmp.gridAg.addColumnText("employeeNumberAndName", this.terms["common.employee"], null);
        if (this.isNew) {
            this.gridHandlerEmp.gridAg.addColumnIcon(null, null, null, { icon: 'fal fa-user-slash errorColor', showIcon: (row: TimeScheduleScenarioEmployeeDTO) => row.needsReplacement, toolTip: this.terms["time.schedule.planning.scenario.needsreplacement"] });
            this.gridHandlerEmp.gridAg.addColumnSelect("replacementEmployeeNumberAndName", this.terms["time.schedule.planning.scenario.replacewith"], null, {
                editable: true,
                selectOptions: [],
                displayField: "replacementEmployeeNumberAndName",
                dropdownIdLabel: "employeeId",
                dropdownValueLabel: "numberAndName",
                dynamicSelectOptions: {
                    idField: "employeeId",
                    displayField: "numberAndName",
                    options: () => this.allEmployees
                },
            });
        }
        this.gridHandlerEmp.gridAg.options.setStandardSubscriptions((rows: any[]) => this.onEmployeeGridRowSelected(rows));
        this.gridHandlerEmp.gridAg.finalizeInitGrid(null, true, "employee-totals-grid");

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.gridHandlerEmp.gridAg.options.subscribe(events);
    }

    // SERVICE CALLS

    private loadLookups(): ng.IPromise<any> {
        let deferral = this.$q.defer<any>();

        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions(),
            this.loadSourceTypes(),
            this.loadEmployees()
        ]).then(() => {
            deferral.resolve();
        });

        return deferral.promise;
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.time.day",
            "core.time.days",
            "core.time.week",
            "core.time.weeks",
            "core.time.year",
            "core.time.years",
            "core.shorter",
            "core.longer",
            "core.warning",
            "common.employee",
            "time.schedule.planning.scenario.deleteemployee.warning",
            "time.schedule.planning.scenario.needsreplacement",
            "time.schedule.planning.scenario.new.originatefrom.scenario",
            "time.schedule.planning.scenario.replacewith",
            "time.schedule.planning.scenario.selectaccounts"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.fromScenarioLabel = '{0} ({1})'.format(this.terms["time.schedule.planning.scenario.new.originatefrom.scenario"], this.scenarioName);
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const features: number[] = [];
        features.push(Feature.Time_Employee_Employees);
        features.push(Feature.Time_Employee_Employees_Create_Vacant_Employees);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.modifyEmployeePermission = x[Feature.Time_Employee_Employees];
            this.createVacantPermission = x[Feature.Time_Employee_Employees_Create_Vacant_Employees];
        });
    }

    private loadSourceTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeScheduleScenarioHeadSourceType, false, false).then(x => {
            this.sourceTypes = x;
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        return this.sharedScheduleService.getEmployeesForPlanning(null, null, true, false, false, false, false, this.scenarioHead.dateFrom, this.scenarioHead.dateTo, this.includeSecondaryCategoriesOrAccounts, TimeSchedulePlanningDisplayMode.Admin).then(x => {
            this.allEmployees = x;
        });
    }

    private selectAccounts() {
        if (this.isNew) {
            this.selectedAccountIds = this.accountDim.accounts.map(a => a.accountId);
            this.accountDim.accounts.forEach(acc => {
                const row = _.find(this.accountDim.accounts, e => e.accountId === acc.accountId);
                if (row)
                    this.gridHandlerAcc.gridAg.options.selectRow(row, true);
            });
        } else {
            this.selectedAccountIds = this.scenarioHead.accounts.map(a => a.accountId);
            this.scenarioHead.accounts.forEach(acc => {
                const row = _.find(this.accountDim.accounts, e => e.accountId === acc.accountId);
                if (row)
                    this.gridHandlerAcc.gridAg.options.selectRow(row, true);
            });
        }
        if (this.firstSelect) {
            this.selectedAccountIds = this.gridHandlerAcc.gridAg.options.getSelectedIds('accountId');
            this.filterEmployees();
            this.setIsValidToSave();
        }

        this.$timeout(() => {
            this.firstSelect = false;
        });
    }

    private selectEmployees() {
        this.selectedEmployees.forEach(row => {
            let emp = _.find(this.filteredEmployees, e => e.employeeId === row.employeeId);
            if (emp)
                this.gridHandlerEmp.gridAg.options.selectRow(emp, true);
        });
    }

    // HELP-METHODS

    private setSourceTypeName() {
        let type = _.find(this.sourceTypes, s => s.id == this.scenarioHead.sourceType);
        this.sourceTypeName = type ? type.name : '';
    }

    private setOffsetText() {
        this.offsetText = '';

        let days: number = this.scenarioHead.sourceOffsetDays;
        if (days === 0)
            return;

        // Years
        if (this.scenarioHead.dateFrom.getMonth() === this.scenarioHead.sourceDateFrom.getMonth() && this.scenarioHead.dateFrom.getDate() === this.scenarioHead.sourceDateFrom.getDate()) {
            let years: number = this.scenarioHead.dateFrom.getFullYear() - this.scenarioHead.sourceDateFrom.getFullYear();
            if (years !== 0) {
                this.offsetText = `${years} ${(Math.abs(years) === 1 ? this.terms["core.time.year"] : this.terms["core.time.years"]).toLocaleLowerCase()}`;
                return;
            }
        }

        // Weeks
        if (days % 7 === 0) {
            let weeks: number = days / 7;
            if (weeks !== 0) {
                this.offsetText = `${weeks} ${(Math.abs(weeks) === 1 ? this.terms["core.time.week"] : this.terms["core.time.weeks"]).toLocaleLowerCase()}`;
                return;
            }
        }

        // Days
        this.offsetText = `${days} ${(Math.abs(days) === 1 ? this.terms["core.time.day"] : this.terms["core.time.days"]).toLocaleLowerCase()}`;
    }

    private setLengthDiffText() {
        this.lengthDiffText = '';
        const sourceLength = this.scenarioHead.sourceDateTo.diffDays(this.scenarioHead.sourceDateFrom) + 1;
        const targetLength = this.scenarioHead.dateTo.diffDays(this.scenarioHead.dateFrom) + 1;
        const diffDays = targetLength - sourceLength;

        if (diffDays !== 0) {
            this.lengthDiffText = `${Math.abs(diffDays)} ${(Math.abs(diffDays) === 1 ? this.terms["core.time.day"] : this.terms["core.time.days"]).toLocaleLowerCase()} `;

            if (diffDays > 0) {
                this.lengthDiffText += `${this.terms["core.longer"].toLocaleLowerCase()}`;
            } else if (diffDays < 0) {
                this.lengthDiffText += `${this.terms["core.shorter"].toLocaleLowerCase()}`;
            }
        }
    }

    private setAccount() {
        // Set user account to current user setting
        if (this.useAccountHierarchy && this.accountHierarchyId && this.accountDim && this.accountDim.accounts) {
            const userAccountIds: number[] = this.accountHierarchyId.split('-').map(Number);

            if (userAccountIds.length > 0 && _.first(userAccountIds) !== 0) {
                // Filter accounts based on current user setting
                let userAccountId = _.last(userAccountIds);
                let account = _.find(this.accountDim.accounts, a => a.accountId === userAccountId);
                if (account) {
                    // Leaf node selected (eg. Butik)
                    this.accountDim.accounts = _.filter(this.accountDim.accounts, a => a.accountId === userAccountId);
                } else {
                    // If parent of leaf node selected (eg. Region), get leaf accounts (eg. Butiker)
                    if (_.some(this.accountDim.accounts, a => a.parentAccountId === userAccountId))
                        this.accountDim.accounts = _.filter(this.accountDim.accounts, a => a.parentAccountId === userAccountId);

                    // If above parent of leaf node selected (eg. Kedja) show all
                }
            }
        }
    }

    private reloadEmployees(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([() => this.loadEmployees()]).then(() => {
            this.filterEmployees();
        });
    }

    private filterEmployees() {
        this.filteredEmployees = [];

        let validEmployeeIds: number[] = this.getValidEmployeeIds();
        let filtered = _.filter(this.allEmployees, e => _.includes(validEmployeeIds, e.employeeId) || (this.useAccountHierarchy && e.hidden));
        if (!this.showAllEmployees)
            filtered = _.filter(filtered, e => _.includes(this.sourceEmployees.map(se => se.employeeId), e.employeeId) || (this.useAccountHierarchy && e.hidden));
        filtered.forEach(e => {
            this.filteredEmployees.push(e.convertToTimeScheduleScenarioEmployeeDTO());
        });

        // Filter employees on account
        if (this.useAccountHierarchy) {
            let validAccountEmployeeIds: number[] = [];
            this.filteredEmployees.forEach(emp => {
                let origEmp = _.find(this.allEmployees, e => e.employeeId === emp.employeeId);
                if (origEmp) {
                    if (origEmp.hidden || NumberUtility.intersect(origEmp.accounts.map(a => a.accountId), this.selectedAccountIds).length > 0)
                        validAccountEmployeeIds.push(emp.employeeId);
                }
            });
            this.filteredEmployees = _.filter(this.filteredEmployees, e => _.includes(validAccountEmployeeIds, e.employeeId));
        }

        this.sourceEmployees.forEach(employee => {
            if (!_.includes(this.filteredEmployees.map(e => e.employeeId), employee.employeeId)) {
                let replacement: TimeScheduleScenarioEmployeeDTO = employee.convertToTimeScheduleScenarioEmployeeDTO();
                replacement.needsReplacement = true;
                this.filteredEmployees.push(replacement);
            }
        });

        this.gridHandlerEmp.gridAg.setData(this.filteredEmployees);
        this.selectEmployees();
    }

    private getValidEmployeeIds(): number[] {
        let validEmployeeIds: number[] = [];

        if (this.useAccountHierarchy) {
            // Filter employees based on account hierarchy and filtered accounts
            this.allEmployees.forEach(employee => {
                if (employee.hidden) {
                    validEmployeeIds.push(employee.employeeId);
                } else {
                    if (employee.hasEmployment(this.scenarioHead.dateFrom, this.scenarioHead.dateTo, true) && employee.accounts) {
                        employee.accounts.forEach(empAccount => {
                            if (this.isEmployeeAccountValid(empAccount)) {
                                validEmployeeIds.push(employee.employeeId);
                                return false;
                            }
                        });
                    }
                }
            });
        } else {
            // Filter employees based on filtered categories
            this.allEmployees.forEach(employee => {
                if (employee.hidden) {
                    validEmployeeIds.push(employee.employeeId);
                } else {
                    if (employee.hasEmployment(this.scenarioHead.dateFrom, this.scenarioHead.dateTo, true)) {
                        this.filteredCategoryIds.forEach(categoryId => {
                            // Check if employee has current category
                            let empCategory = _.find(employee.categoryRecords, c => c.categoryId === categoryId);
                            if (empCategory) {
                                // Check dates
                                if ((!empCategory.dateFrom || empCategory.dateFrom.isSameOrBeforeOnDay(this.scenarioHead.dateTo)) && (!empCategory.dateTo || empCategory.dateTo.isSameOrAfterOnDay(this.scenarioHead.dateFrom))) {
                                    validEmployeeIds.push(employee.employeeId);
                                    return false;
                                }
                            }
                        });
                    }
                }
            });
        }

        return validEmployeeIds;
    }

    private isEmployeeAccountValid(empAccount: EmployeeAccountDTO): boolean {
        // Check account
        if (_.includes(this.filteredAccountIds, empAccount.accountId)) {
            // Check date interval
            if (empAccount.dateFrom.isSameOrBeforeOnDay(this.scenarioHead.dateTo) && (!empAccount.dateTo || empAccount.dateTo.isSameOrAfterOnDay(this.scenarioHead.dateFrom))) {
                // Check children
                if (!empAccount.children || empAccount.children.length === 0) {
                    // No children, parent was valid so it's OK
                    return true;
                } else {
                    // Recursively check each child account
                    // If one is valid it's OK
                    let childValid: boolean = false;
                    empAccount.children.forEach(childAccount => {
                        if (this.isEmployeeAccountValid(childAccount)) {
                            childValid = true;
                            return false;   // Exit loop
                        }
                    });
                    if (childValid)
                        return true;
                }
            }
        }

        return false;
    }

    private setIsValidToSave() {
        this.isValidToSave = false;

        if (this.useAccountHierarchy && this.selectedAccountIds.length === 0)
            return;

        if (this.filteredEmployees.length === 0 || this.selectedEmployees.length === 0)
            return;

        // All missing employees that are selected needs to be replaced
        if (_.filter(this.selectedEmployees, e => e.needsReplacement && !e.replacementEmployeeId).length > 0)
            return;

        // Same employee cannot exist twice (as replacement)
        let employeeIds: number[] = [];
        employeeIds = employeeIds.concat(_.filter(this.selectedEmployees, e => !e.needsReplacement).map(e => e.employeeId));
        employeeIds = employeeIds.concat(_.filter(this.selectedEmployees, e => e.needsReplacement).map(e => e.replacementEmployeeId));
        if (_.uniq(employeeIds).length !== this.selectedEmployees.length)
            return;

        this.isValidToSave = true;
    }

    // EVENTS

    private dateFromChanged() {
        this.$timeout(() => {
            if (this.dateFunction === 2 && this.scenarioHead.dateFrom < this.scenarioHead.sourceDateFrom)
                this.scenarioHead.dateFrom = this.scenarioHead.sourceDateFrom;

            this.scenarioHead.dateTo = this.scenarioHead.dateFrom ? this.scenarioHead.dateFrom.addDays(this.length - 1).endOfDay() : null;
            if (this.dateFunction === 2 && this.scenarioHead.dateTo > this.scenarioHead.sourceDateTo)
                this.scenarioHead.dateTo = this.scenarioHead.sourceDateTo;

            this.setOffsetText();
            this.setLengthDiffText();

            this.allEmployees = [];
            this.filteredEmployees = [];
            this.gridHandlerEmp.gridAg.clearData();

            this.reloadEmployees();
        });
    }

    private dateToChanged() {
        this.$timeout(() => {
            if (this.dateFunction === 2 && this.scenarioHead.dateTo > this.scenarioHead.sourceDateTo)
                this.scenarioHead.dateTo = this.scenarioHead.sourceDateTo;

            this.scenarioHead.dateTo = this.scenarioHead.dateTo.endOfDay();
            
            this.setLengthDiffText();

            this.allEmployees = [];
            this.filteredEmployees = [];
            this.gridHandlerEmp.gridAg.clearData();

            this.reloadEmployees();
        });
    }

    private onAccountGridRowSelected(rows: any[]) {
        if (!this.firstSelect) {
            this.$scope.$applyAsync();
            this.selectedAccountIds = this.gridHandlerAcc.gridAg.options.getSelectedIds('accountId');
            this.filterEmployees();
            this.setIsValidToSave();
        }
    }

    private onEmployeeGridRowSelected(rows: any[]) {
        this.$scope.$applyAsync();
        let selectedEmployeeIds: number[] = this.gridHandlerEmp.gridAg.options.getSelectedIds('employeeId');
        this.selectedEmployees = _.filter(this.filteredEmployees, e => _.includes(selectedEmployeeIds, e.employeeId));
        this.setIsValidToSave();
    }

    private afterCellEdit(row: TimeScheduleScenarioEmployeeDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        this.$scope.$applyAsync();
        if (newValue === oldValue)
            return;

        if (colDef.field === 'replacementEmployeeNumberAndName' && newValue) {
            let emp = _.find(this.allEmployees, e => e.numberAndName === newValue);
            if (emp) {
                row.replacementEmployeeId = emp.employeeId;
                row.replacementEmployeeNumberAndName = emp.numberAndName;
                this.setIsValidToSave();
            }
        }
    }

    private newEmployee() {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Views/edit.html"),
            controller: EmployeeEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        };
        let modal = this.modalInstance.open(options);

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                isManuallyNew: true,
            });
        });

        modal.result.then(result => {
            if (result.modified) {
                this.reloadEmployees();
            }
        }, (reason) => { });
    }

    private openCreateVacantEmployees() {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Dialogs/CreateVacantEmployees/CreateVacantEmployeesDialog.html"),
            controller: CreateVacantEmployeesDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {

            }
        }
        this.modalInstance.open(options).result.then(result => {
            this.reloadEmployees();
        }, (reason) => { });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private initOk() {
        if (this.initialSelectedEmployeeIds.every(e => _.includes(this.selectedEmployees.map(se => se.employeeId), e))) {
            this.ok();
        } else {
            // If employees were removed, show warning
            const modal = this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["time.schedule.planning.scenario.deleteemployee.warning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.ok();
                };
            }, (reason) => {
                // User cancelled
            });
        }
    }

    private ok() {
        let result: any = {};
        result.scenarioHead = this.scenarioHead;
        if (this.useAccountHierarchy)
            result.accountIds = this.selectedAccountIds;
        result.employees = this.selectedEmployees;
        result.includeAbsence = this.includeAbsence;
        result.dateFunction = this.dateFunction;

        this.$uibModalInstance.close(result);
    }
}
