import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { AttestEmployeeDayDTO, AttestEmployeeDayTimeStampDTO, SaveAttestEmployeeDayDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ShiftDTO } from "../../../Common/Models/TimeSchedulePlanningDTOs";
import { TranslationService } from "../../../Core/Services/TranslationService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TimeStampEntryType, Feature, TermGroup_TimeDeviationCauseType, CompanySettingType, UserSettingType, TermGroup_TimeStampEntryOriginType } from "../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ITimeService } from "../../Time/TimeService";
import { AccountSmallDTO, AccountDTO } from "../../../Common/Models/AccountDTO";
import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { Constants } from "../../../Util/Constants";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IEmployeeService } from "../../Employee/EmployeeService";
import { TimeStampDetailsController } from "./Dialogs/TimeStampDetails/TimeStampDetailsController";
import { TimeDeviationCauseGridDTO } from "../../../Common/Models/TimeDeviationCauseDTOs";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { IAccountingService as ISharedAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { NumberUtility } from "../../../Util/NumberUtility";
import { TimeStampEntryDTO, TimeStampEntryExtendedDTO } from "../../../Common/Models/TimeStampDTOs";
import { TimeStampAdditionsDialogController } from "./Dialogs/TimeStampAdditionsDialog/TimeStampAdditionsDialogController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";

export class TimeStampDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AttestEmployee/Views/TimeStamp.html'),
            scope: {
                model: '=',
                isModal: '=?',
                onChange: '&',
                onSelectedTimeStampChanged: '&',
                onResizeNeeded: '&',
                isDirty: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: TimeStampController,
            controllerAs: 'ctrl',
            bindToController: true,
        };
    }
}

export class TimeStampController {

    // Init parameters
    private model: AttestEmployeeDayDTO;
    private isModal: boolean;
    private onChange: Function;
    private onSelectedTimeStampChanged: Function;
    private isModified: boolean;
    private onResizeNeeded: Function;
    private isDirty: boolean;

    // Terms
    private terms: { [index: string]: string; };
    private accountingLabel: string;
    private accountingLabel2: string;
    private terminalAccountingLabel: string;
    private progressMessage: string = '';

    // Company settings
    private useAccountHierarchy = false;
    private defaultEmployeeAccountDimId = 0;
    private useTimeScheduleTypeFromTime = false;
    private possibilityToRegisterAdditionsInTerminal = false;

    // User settings
    private accountHierarchyId: string;
    //private userAccountIds: number[];
    //private userAccountId: number;
    private validAccountIds: number[] = [];
    private selectableEmployeeShiftAccountIds: number[] = [];

    // Permissions
    private get isReadonly(): boolean {
        return !this.editPermission || this.model.isReadonly;
    }
    private isEmployeeCurrentUser = false;
    private editPermission = false;
    private editTimePermission = false;
    private editOthersPermission = false;
    private editOthersTimePermission = false;
    private editTimeStampsWithOutComment = false;
    private discardBreakEvaluationPermission = false;
    private createTimeStampsAccourdingToSchedulePermission = false;

    // AttestRole settings
    private hasAttestByEmployeeAccount = false;

    // Terminal settings
    private accountDimId2 = 0;

    // Data
    private deviationCauseStandardId = 0;
    private deviationCauses: TimeDeviationCauseGridDTO[] = [];
    private employeeChilds: ISmallGenericType[] = [];
    private accountDim: AccountDimSmallDTO;
    private accounts: AccountSmallDTO[] = [];
    private accountDim2: AccountDimSmallDTO;
    private accounts2: AccountSmallDTO[] = [];
    private terminalAccountDim: AccountDimSmallDTO;
    private terminalAccounts: AccountDTO[] = [];
    private defaultTerminalAccountId: number;
    private employeeAccountIds: number[];

    // Flags
    private setFocusInTime = false;
    private executing = false;
    private enableCreateTimeStampsFromSchedule = false;

    // Properties
    private selectedTimeStamp: AttestEmployeeDayTimeStampDTO;
    private idCounter = 0;
    private discardBreakEvaluation = false;
    private discardBreakEvaluationDisabled = false;
    private showEmployeeChild = false;
    private showAccountDim = false;
    private showAccountDim2 = false;

    private progress: IProgressHandler;

    //@ngInject
    constructor(
        private urlHelperService: IUrlHelperService,
        private translationService: TranslationService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private progressHandlerFactory: IProgressHandlerFactory,
        private coreService: ICoreService,
        private timeService: ITimeService,
        private employeeService: IEmployeeService,
        private sharedAccountingService: ISharedAccountingService,
        private $uibModal,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService
    ) {
        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadUserAndCompanySettings(),
            this.loadValidAccounts(),
            this.getIsEmployeeCurrentUser()
        ]).then(() => {
            let queue = [];
            queue.push(this.loadModifyPermissions());

            if (this.useAccountHierarchy && this.defaultEmployeeAccountDimId) {
                queue.push(this.loadSelectableEmployeeShiftAccountIds());
                queue.push(this.loadEmployeeAccountIds());
                queue.push(this.loadTerminalAccountDim());
            }

            this.$q.all(queue).then(() => {
                if (this.useAccountHierarchy) {
                    this.setDefaultTerminalAccount();
                    this.filterTerminalAccounts();

                    // Add selectableEmployeeShiftAccountIds to validAccountIds
                    this.validAccountIds = this.validAccountIds.concat(this.selectableEmployeeShiftAccountIds)
                }
                this.setEnableCreateTimeStampsFromSchedule();

                this.$q.all([this.loadEmployeeChilds()]).then(() => {
                    this.$q.all([
                        this.loadStandardDeviationCause(),
                        this.loadDeviationCauses(),
                        this.loadAccountDim(1),
                        this.loadAccountDim(2),
                    ]).then(() => {
                        this.discardBreakEvaluation = this.model.alwaysDiscardBreakEvaluation || this.model.discardedBreakEvaluation;
                        this.discardBreakEvaluationDisabled = this.model.alwaysDiscardBreakEvaluation;
                        this.setupWatchers();
                        this.setShowEmployeeChild();
                        _.forEach(this.model.timeStampEntrys, timeStamp => {
                            this.setSpecifyChild(timeStamp);
                            this.setAccountId2FromExtended(timeStamp);
                            this.setTerminalAccountName(timeStamp);
                        });
                    });
                });
            });
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.selectedTimeStamp, () => {
            // TimeStamp entry selected, notify schedule GUI to mark it as selected
            if (!this.setFocusInTime && this.onSelectedTimeStampChanged)
                this.onSelectedTimeStampChanged({ timeStampEntry: this.selectedTimeStamp });
        });

        this.$scope.$on('TimeStampSelected', (event, id) => {
            // TimeStamp selected in schedule GUI
            // Select stamp in time for that entry
            this.setFocusInTime = true;
            this.$timeout(() => {
                this.selectedTimeStamp = _.find(this.model.timeStampEntrys, t => t.identifier == id);
                this.$timeout(() => {
                    this.setFocusInTime = false;
                });
            })
        });

        this.$scope.$on(Constants.EVENT_ATTESTEMPLOYEE_CONTENT_DAY_CHANGED, (event, data) => {
            // Will be triggered inside grid
            if (this.model.employeeId === data.employeeId && this.model.date.isSameDayAs(data.date)) {
                _.forEach(this.model.timeStampEntrys, timeStamp => {
                    this.setSpecifyChild(timeStamp);
                    this.setAccountId2FromExtended(timeStamp);
                    this.setTerminalAccountName(timeStamp);
                    this.setFilteredAccountsOnAllTimeStamps(1);
                    this.setFilteredAccountsOnAllTimeStamps(2);
                });
            }
        });
        const subscription = this.messagingService.subscribe(Constants.EVENT_ATTESTEMPLOYEE_CONTENT_DAY_CHANGED, (data: { employeeId: number, date: Date }) => {
            // Will be triggered in dialog
            if (this.model.employeeId === data.employeeId && this.model.date.isSameDayAs(data.date)) {
                _.forEach(this.model.timeStampEntrys, timeStamp => {
                    this.setSpecifyChild(timeStamp);
                    this.setAccountId2FromExtended(timeStamp);
                    this.setTerminalAccountName(timeStamp);
                    this.setFilteredAccountsOnAllTimeStamps(1);
                    this.setFilteredAccountsOnAllTimeStamps(2);
                });
            }
        });

        this.$scope.$on('$destroy', () => {
            subscription.unsubscribe();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.saving",
            "common.accounting",
            "time.time.attest.timestamps.in",
            "time.time.attest.timestamps.out",
            "time.time.attest.break",
            "time.time.attest.timestamps.invaliddate",
            "time.time.attest.timestamps.invaliddate.message",
            "time.time.attest.timestamps.save.invalid",
            "time.time.attest.timestamps.save.invalid.commentmandatory",
            "time.time.attest.timestamps.unauthorizedtime",
            "time.time.attest.timestamps.unauthorizedtime.message",
            "time.time.attest.timestamps.save.invalid.commentmandatory",
            "time.time.attest.timestamps.unlockday",
            "time.time.attest.timestamps.unlockday.tooltip",
            "time.time.attest.timestamps.unlockday.error",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.accountingLabel = this.accountingLabel2 = this.terms["common.accounting"];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);
        settingTypes.push(CompanySettingType.UseTimeScheduleTypeFromTime);
        settingTypes.push(CompanySettingType.PossibilityToRegisterAdditionsInTerminal);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            if (this.useAccountHierarchy)
                this.getHasAttestByEmployeeAccount();
            this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
            this.useTimeScheduleTypeFromTime = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseTimeScheduleTypeFromTime);
            this.possibilityToRegisterAdditionsInTerminal = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PossibilityToRegisterAdditionsInTerminal);
        });
    }

    private loadUserAndCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(UserSettingType.AccountHierarchyId);

        return this.coreService.getUserAndCompanySettings(settingTypes).then(x => {
            this.accountHierarchyId = SettingsUtility.getStringUserSetting(x, UserSettingType.AccountHierarchyId, '0');
        });
    }

    private getHasAttestByEmployeeAccount(): ng.IPromise<any> {
        return this.timeService.getHasAttestByEmployeeAccount(this.model.date).then(result => {
            this.hasAttestByEmployeeAccount = result;
        });
    }

    private loadValidAccounts(): ng.IPromise<any> {
        return this.coreService.getAccountIdsFromHierarchyByUser(this.model.date, this.model.date, false, false, false, true, false, false, this.model.employeeId).then(x => {
            this.validAccountIds = x;
        });
    }

    private loadSelectableEmployeeShiftAccountIds(): ng.IPromise<any> {
        return this.coreService.getSelectableEmployeeShiftAccountIds(this.model.employeeId, this.model.date).then(x => {
            this.selectableEmployeeShiftAccountIds = x;
        });
    }

    private getIsEmployeeCurrentUser(): ng.IPromise<any> {
        return this.timeService.isEmployeeCurrentUser(this.model.employeeId).then(x => {
            this.isEmployeeCurrentUser = x;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const features: number[] = [];
        features.push(Feature.Time_Time_Attest_TimeStamp_EditTimeStampEntries);
        features.push(Feature.Time_Time_Attest_TimeStamp_EditTimeStampEntryTime);
        features.push(Feature.Time_Time_Attest_TimeStamp_EditOthersTimeStampEntries);
        features.push(Feature.Time_Time_Attest_TimeStamp_EditOthersTimeStampEntryTime);
        features.push(Feature.Time_Time_Attest_TimeStamp_EditTimeStampWithOutComment);
        features.push(Feature.Time_Time_Attest_TimeStamp_DiscardBreakEvaluation);
        features.push(Feature.Time_Time_Attest_TimeStamp_CreateTimeStampsAccourdingToSchedule);
        features.push(Feature.Time_Time_AttestUser_TimeStamp_EditTimeStampWithOutComment);

        return this.coreService.hasModifyPermissions(features).then(x => {
            this.editPermission = x[Feature.Time_Time_Attest_TimeStamp_EditTimeStampEntries] || (x[Feature.Time_Time_Attest_TimeStamp_EditOthersTimeStampEntries] && !this.isEmployeeCurrentUser);
            this.editTimePermission = this.editPermission || (x[Feature.Time_Time_Attest_TimeStamp_EditOthersTimeStampEntryTime] && !this.isEmployeeCurrentUser);
            this.editTimeStampsWithOutComment = (this.isEmployeeCurrentUser && x[Feature.Time_Time_AttestUser_TimeStamp_EditTimeStampWithOutComment]) || (!this.isEmployeeCurrentUser && x[Feature.Time_Time_Attest_TimeStamp_EditTimeStampWithOutComment]);
            this.discardBreakEvaluationPermission = x[Feature.Time_Time_Attest_TimeStamp_DiscardBreakEvaluation];
            this.createTimeStampsAccourdingToSchedulePermission = this.editTimePermission && x[Feature.Time_Time_Attest_TimeStamp_CreateTimeStampsAccourdingToSchedule];
        });
    }

    private loadTerminalAccountDim(): ng.IPromise<any> {
        return this.sharedAccountingService.getAccountDimSmall(this.defaultEmployeeAccountDimId, true, false, true, true).then(x => {
            this.terminalAccountDim = x;
            if (this.terminalAccountDim)
                this.terminalAccountingLabel = this.terminalAccountDim.name;
        })
    }

    private loadEmployeeAccountIds(): ng.IPromise<any> {
        return this.employeeService.getEmployeeAccountIds(this.model.employeeId, this.model.date).then(x => {
            this.employeeAccountIds = x;
        });
    }

    private loadStandardDeviationCause(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCauseStandardIdFromPrio(this.model.employeeId, this.model.date).then(x => {
            this.deviationCauseStandardId = x;
        });
    }

    private loadDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCausesByEmployeeGroupGrid(this.model.employeeGroupId, false, false).then(x => {
            this.deviationCauses = x;

            // If employee does not have any children, remove causes where children is mandatory
            if (this.employeeChilds.length === 0)
                this.deviationCauses = _.filter(this.deviationCauses, t => !t.specifyChild);
        });
    }

    private loadEmployeeChilds(): ng.IPromise<any> {
        return this.employeeService.getEmployeeChildsDict(this.model.employeeId, false).then(x => {
            this.employeeChilds = x;
        });
    }

    private loadAccountDim(dimNr: number): ng.IPromise<any> {
        let deferral = this.$q.defer();

        this.timeService.getTimeTerminalAccountDim(0, dimNr).then(x => {
            if (dimNr === 1) {
                this.accountDim = x;
            } else {
                this.accountDim2 = x;
            }

            if (x?.accountDimId) {
                if (dimNr === 1) {
                    this.accountingLabel = x.name;
                    this.showAccountDim = true;
                } else {
                    this.accountingLabel2 = x.name;
                    this.showAccountDim2 = true;
                }

                this.loadAccounts(dimNr).then(() => {
                    deferral.resolve();
                });
            } else {
                deferral.resolve();
            }
        });

        return deferral.promise;
    }

    private loadAccounts(dimNr: number): ng.IPromise<any> {
        return this.timeService.getAccountsSmall(dimNr == 1 ? this.accountDim.accountDimId : this.accountDim2.accountDimId, 0, true, true).then(x => {
            // Add empty account
            let acc: AccountSmallDTO = new AccountSmallDTO();
            acc.accountId = 0;
            acc.number = acc.name = '';

            if (dimNr === 1) {
                this.accounts = x;
                this.accounts.splice(0, 0, acc);
            } else {
                this.accounts2 = x;
                this.accounts2.splice(0, 0, acc);
            }

            this.setFilteredAccountsOnAllTimeStamps(dimNr);
        });
    }

    private saveTimeStamps = _.debounce(() => {
        this.validateSaveTimeStamps().then(passed => {
            if (passed) {
                this.progress.startSaveProgress((completion) => {
                    return this.timeService.saveTimeStampEntries(this.model.timeStampEntrys, this.model.date, this.discardBreakEvaluation, this.model.employeeId).then(result => {
                        if (result.success) {
                            let hasAbsence: boolean = false;
                            _.forEach(_.filter(this.model.timeStampEntrys, e => e.timeDeviationCauseId), entry => {
                                const cause = _.find(this.deviationCauses, d => d.timeDeviationCauseId === entry.timeDeviationCauseId);
                                if (cause && cause.type === TermGroup_TimeDeviationCauseType.Absence) {
                                    hasAbsence = true;
                                    return false;
                                }
                            });
                            completion.completed(null, true);
                            this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_ROWSANDWARNINGS_RELOAD, { date: hasAbsence ? null : this.model.date, fromModal: this.isModal });
                            this.isModified = false;
                            this.isDirty = false;
                        } else {
                            completion.completed(null, true);
                            this.translationService.translate('core.savefailed').then(term => {
                                this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                            });
                        }

                        this.progressMessage = '';
                    });
                }, null);
            } else {                
                this.progressMessage = '';
            }
        });
    }, 500, { leading: true, trailing: false });

    private validateSaveTimeStamps(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        this.$timeout(() => {
            // Check mandatory comment on manually adjusted stamps
            if (!this.editTimeStampsWithOutComment) {
                if (_.filter(this.model.timeStampEntrys, t => t.manuallyAdjusted && (!t.note || t.note.trim().length === 0)).length > 0) {
                    this.notificationService.showDialogEx(this.terms["time.time.attest.timestamps.save.invalid"], this.terms["time.time.attest.timestamps.save.invalid.commentmandatory"], SOEMessageBoxImage.Forbidden);
                    deferral.resolve(false);
                    return;
                }
            }

            if (this.validateTimeForCurrentUser()) {
                deferral.resolve(true);
            } else {
                this.employeeService.getDefaultEmployeeAccountDimName().then(name => {
                    const modal = this.notificationService.showDialogEx(this.terms["time.time.attest.timestamps.unauthorizedtime"], this.terms["time.time.attest.timestamps.unauthorizedtime.message"].format(name), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        deferral.resolve(val);
                    }, (reason) => {
                        deferral.resolve(false);
                    });
                });
            }
        });

        return deferral.promise;
    }

    private unlockDay() {
        const modal = this.notificationService.showDialog(this.terms["time.time.attest.timestamps.unlockday"], this.terms["time.time.attest.timestamps.unlockday.tooltip"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val) {
                const items: SaveAttestEmployeeDayDTO[] = [];
                const item = new SaveAttestEmployeeDayDTO();
                item.date = this.model.date;
                item.timeBlockDateId = this.model.timeBlockDateId;
                items.push(item);

                this.executing = true;
                this.progressMessage = this.terms["core.saving"];

                this.timeService.unlockDay(items, this.model.employeeId).then((result) => {
                    if (result.success) {
                        this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_ROWSANDWARNINGS_RELOAD, { date: null, fromModal: this.isModal });
                    } else {
                        this.notificationService.showDialogEx(this.terms["time.time.attest.timestamps.unlockday.error"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                    this.isModified = false;
                    this.executing = false;
                    this.progressMessage = '';
                });
            }
        });
    }

    // EVENTS

    private resizeContainer() {
        if (this.onResizeNeeded) {
            this.$timeout(() => {
                this.onResizeNeeded();
            });
        }
    }

    private addTimeStamp() {
        let lastTimeStamp: AttestEmployeeDayTimeStampDTO;
        if (this.model.timeStampEntrys && this.model.timeStampEntrys.length > 0)
            lastTimeStamp = _.last(_.orderBy(this.model.timeStampEntrys, t => t.time));

        const lastStampWasIn = lastTimeStamp && lastTimeStamp.type === TimeStampEntryType.In;
        let type: TimeStampEntryType = lastStampWasIn ? TimeStampEntryType.Out : TimeStampEntryType.In;
        let time: Date = this.model.date.mergeTime(lastTimeStamp ? lastTimeStamp.time : CalendarUtility.getDateNow());
        let timeStamp: AttestEmployeeDayTimeStampDTO = this.createNewTimeStamp(type, time);
        this.model.timeStampEntrys.push(timeStamp);

        this.selectedTimeStamp = timeStamp;

        this.resizeContainer();
        this.setDirty(timeStamp);
    }

    private fromSchedule() {
        this.progress.startWorkProgress((completion) => {
            return this.timeService.createTimeStampsAccourdingToSchedule(this.model.timeScheduleTemplatePeriodId, this.model.date, this.model.employeeId, this.model.employeeGroupId).then(entries => {
                let hasExisting: boolean = this.model.timeStampEntrys.length > 0;
                _.forEach(entries, entry => {
                    this.model.timeStampEntrys.push(this.createNewTimeStampFromEntry(entry));
                });
                if (hasExisting)
                    this.model.timeStampEntrys = _.orderBy(this.model.timeStampEntrys, ['time', 'timeStampEntryId']);

                this.resizeContainer();
                this.setDirty();

                completion.completed(null, true);
            });
        });
    }

    private deleteTimeStamp(timeStamp: AttestEmployeeDayTimeStampDTO) {
        _.pull(this.model.timeStampEntrys, timeStamp);

        this.resizeContainer();
        this.setDirty();
    }

    private changeType(timeStamp: AttestEmployeeDayTimeStampDTO) {
        if (!this.editPermission || timeStamp.isReadonly)
            return;

        timeStamp.type = (timeStamp.type === TimeStampEntryType.In ? TimeStampEntryType.Out : TimeStampEntryType.In);
        this.setDirty(timeStamp);
    }

    private dateChanged(timeStamp: AttestEmployeeDayTimeStampDTO) {
        let oldTime: Date = timeStamp.time;
        this.$timeout(() => {
            if (!oldTime.isSameDayAs(timeStamp.time)) {
                // Can only set date to one day before or one day after current date
                let invalidDate: boolean = false;
                if (timeStamp.time.isAfterOnDay(this.model.date.addDays(1))) {
                    invalidDate = true;
                    timeStamp.time = this.model.date.addDays(1);
                } else if (timeStamp.time.isBeforeOnDay(this.model.date.addDays(-1))) {
                    invalidDate = true;
                    timeStamp.time = this.model.date.addDays(-1);
                }

                timeStamp.time = timeStamp.time.mergeTime(oldTime);

                this.setDirty(timeStamp);

                if (invalidDate)
                    this.notificationService.showDialogEx(this.terms["time.time.attest.timestamps.invaliddate"], this.terms["time.time.attest.timestamps.invaliddate.message"].format(this.model.date.addDays(-1).toFormattedDate(), this.model.date.addDays(1).toFormattedDate()), SOEMessageBoxImage.Forbidden);
            }
        });
    }

    private timeChanged(timeStamp: AttestEmployeeDayTimeStampDTO) {
        let oldTime: Date = timeStamp.time;
        this.$timeout(() => {
            if (!oldTime.isSameMinuteAs(timeStamp.time))
                this.setDirty(timeStamp);
        });
    }

    private deviationCauseChanged(timeStamp: AttestEmployeeDayTimeStampDTO) {
        this.$timeout(() => {
            let newCause = _.find(this.deviationCauses, c => c.timeDeviationCauseId === timeStamp.timeDeviationCauseId);
            timeStamp.employeeChildId = (newCause?.specifyChild && this.employeeChilds.length > 0) ? this.employeeChilds[0].id : null;

            this.setShowEmployeeChild();
            this.setSpecifyChild(timeStamp);

            this.setDirty(timeStamp);
        });
    }

    private secondaryAccountChanged(timeStamp: AttestEmployeeDayTimeStampDTO) {
        this.$timeout(() => {
            this.setExtendedFromAccountId2(timeStamp);
            this.setDirty(timeStamp);
        });
    }

    private openTimeStampAdditions(timeStamp: AttestEmployeeDayTimeStampDTO) {
        if (!timeStamp)
            return;

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/AttestEmployee/Dialogs/TimeStampAdditionsDialog/TimeStampAdditionsDialog.html"),
            controller: TimeStampAdditionsDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            resolve: {
                timeStamp: () => { return timeStamp },
                isMyTime: () => { return !!this.model.isMyTime },
            }
        }
        this.$uibModal.open(options);
    }

    private showTimeStampDetails(timeStamp: AttestEmployeeDayTimeStampDTO) {
        if (!timeStamp?.timeStampEntryId)
            return;

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/AttestEmployee/Dialogs/TimeStampDetails/TimeStampDetails.html"),
            controller: TimeStampDetailsController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                timeStampEntryId: () => { return timeStamp.timeStampEntryId }
            }
        }
        this.$uibModal.open(options).result.then(result => {
            console.log(result);
        });
    }

    // HELP-METHODS

    private getActualShifts(): ShiftDTO[] {
        return this.model.shifts ? _.filter(this.model.shifts, s => !s.actualStartTime.isSameMinuteAs(s.actualStopTime)) : [];
    }

    private getIntersectingShifts(timeFrom: Date, timeTo: Date): ShiftDTO[] {
        let shifts: ShiftDTO[] = [];

        let dayShifts = this.getActualShifts();
        _.forEach(dayShifts, shift => {
            // Note!
            // This shortens the existing shifts in start and end to make sure same time does not overlap.
            // Eg: Shift stop 08:00 does not overlap shift start 08:00.
            if (CalendarUtility.isRangesOverlapping(shift.actualStartTime.addSeconds(1), shift.actualStopTime.addSeconds(-1), timeFrom, timeTo)) {
                shifts.push(shift);
            }
        });

        return shifts;
    }

    private hasShiftWithUserAccount(excludeAccountId: number = 0) {
        let hasSchedule = false;
        if (this.validAccountIds.length > 0) {
            let dayShifts = this.getActualShifts();
            _.forEach(dayShifts, shift => {
                if (shift.accountId && shift.accountId !== excludeAccountId && this.validAccountIds.indexOf(shift.accountId) >= 0) {
                    hasSchedule = true;
                    return hasSchedule;
                }
            });
        }
        return hasSchedule;
    }

    private getTypeToolTip(timeStamp: AttestEmployeeDayTimeStampDTO) {
        if (!this.terms)
            return '';

        if (timeStamp.type === TimeStampEntryType.In)
            return this.terms["time.time.attest.timestamps.in"];
        else
            return timeStamp.isBreak ? this.terms["time.time.attest.break"] : this.terms["time.time.attest.timestamps.out"];
    }

    private setShowEmployeeChild() {
        this.showEmployeeChild = false;

        if (this.model.timeStampEntrys.length === 0)
            return;

        // First check if there are any existing children among the time stamps
        if (this.model.timeStampEntrys.filter(e => e.employeeChildId).length > 0) {
            this.showEmployeeChild = true;
            return;
        }

        // Next, check if there are any selected deviation causes that requires children
        const childCauseIds = this.getChildDeviationCauseIds();
        const deviationCauseIds = _.uniq(_.map(this.model.timeStampEntrys, e => e.timeDeviationCauseId));
        for (let deviationCauseId of deviationCauseIds) {
            if (childCauseIds.includes(deviationCauseId)) {
                this.showEmployeeChild = true;
                break;
            }
        }
    }

    private setSpecifyChild(timeStamp: AttestEmployeeDayTimeStampDTO) {
        const childCauseIds = this.getChildDeviationCauseIds();
        timeStamp.specifyChild = timeStamp.timeDeviationCauseId && _.includes(childCauseIds, timeStamp.timeDeviationCauseId);
    }

    private getChildDeviationCauseIds(): number[] {
        const causes = _.filter(this.deviationCauses, c => c.specifyChild);
        if (causes.length > 0)
            return _.map(causes, c => c.timeDeviationCauseId);

        return [];
    }

    private setTerminalAccountName(timeStamp: AttestEmployeeDayTimeStampDTO) {
        if (timeStamp.timeTerminalAccountId) {
            let acc = _.find(this.terminalAccounts, a => a.accountId === timeStamp.timeTerminalAccountId);
            if (acc) {
                timeStamp.timeTerminalAccountName = acc.name;
            } else {
                this.sharedAccountingService.getAccountName(timeStamp.timeTerminalAccountId).then(name => {
                    timeStamp.timeTerminalAccountName = name;
                });
            }
        }
    }

    private setAccountId2FromExtended(timeStamp: AttestEmployeeDayTimeStampDTO) {
        if (!timeStamp.extended)
            timeStamp.extended = [];

        // Secondary account is stored in extended.
        // But it is easier to handle here if on entry level.
        // Check if extended has any account and set it on entry.
        let accountExtended = timeStamp.extended.find(e => e.accountId);
        timeStamp.accountId2 = accountExtended?.accountId;
        timeStamp.accountId2FromExtendedId = accountExtended?.timeStampEntryExtendedId;
    }

    private setExtendedFromAccountId2(timeStamp: AttestEmployeeDayTimeStampDTO) {
        if (!timeStamp.extended)
            timeStamp.extended = [];

        if (timeStamp.accountId2) {
            if (timeStamp.accountId2FromExtendedId) {
                // Update existing
                let accountExtended = timeStamp.extended.find(e => e.timeStampEntryExtendedId === timeStamp.accountId2FromExtendedId);
                if (accountExtended)
                    accountExtended.accountId = timeStamp.accountId2;
            } else {
                // Add new
                // First check if there are any other extended accounts (should only be one)
                let accountExtended = timeStamp.extended.find(e => e.accountId);
                if (accountExtended)
                    accountExtended.accountId = timeStamp.accountId2;
                else {
                    accountExtended = new TimeStampEntryExtendedDTO();
                    accountExtended.accountId = timeStamp.accountId2;
                    timeStamp.extended.push(accountExtended);
                }

            }
        } else if (timeStamp.accountId2FromExtendedId) {
            // Remove
            let accountExtended = timeStamp.extended.find(e => e.timeStampEntryExtendedId === timeStamp.accountId2FromExtendedId);
            if (accountExtended)
                accountExtended.accountId = undefined;
        }
    }

    private setFilteredAccountsOnAllTimeStamps(dimNr: number) {
        _.forEach(this.model.timeStampEntrys, timeStamp => {
            this.setFilteredAccounts(timeStamp, dimNr);
        });
    }

    private setFilteredAccountsFromGui(timeStamp: AttestEmployeeDayTimeStampDTO, dimNr: number) {
        this.$timeout(() => {
            this.setFilteredAccounts(timeStamp, dimNr);
        });
    }

    private setFilteredAccounts(timeStamp: AttestEmployeeDayTimeStampDTO, dimNr: number) {
        const hasTerminalAccount: boolean = !!(timeStamp && timeStamp.timeTerminalAccountId);

        let terminalDimIsParent = false;
        if (dimNr === 1) {
            terminalDimIsParent = this.accountDim && this.terminalAccountDim && this.accountDim.parentAccountDimId && this.accountDim.parentAccountDimId === this.terminalAccountDim.accountDimId;
            timeStamp['filteredAccounts'] = hasTerminalAccount && terminalDimIsParent ? _.filter(this.accounts, a => !a.parentAccountId || a.parentAccountId === timeStamp.timeTerminalAccountId) : this.accounts;
        } else {
            terminalDimIsParent = this.accountDim2 && this.terminalAccountDim && this.accountDim2.parentAccountDimId && this.accountDim2.parentAccountDimId === this.terminalAccountDim.accountDimId;
            timeStamp['filteredAccounts2'] = hasTerminalAccount && terminalDimIsParent ? _.filter(this.accounts2, a => !a.parentAccountId || a.parentAccountId === timeStamp.timeTerminalAccountId) : this.accounts2;
        }
    }

    private getOriginType(): TermGroup_TimeStampEntryOriginType {
        return (this.isEmployeeCurrentUser ? TermGroup_TimeStampEntryOriginType.WebByEmployee : TermGroup_TimeStampEntryOriginType.WebByAdmin);
    }

    private createNewTimeStamp(type: TimeStampEntryType, time: Date): AttestEmployeeDayTimeStampDTO {
        const timeStamp: AttestEmployeeDayTimeStampDTO = new AttestEmployeeDayTimeStampDTO();
        timeStamp.tmpId = ++this.idCounter;
        timeStamp.type = type;
        timeStamp.time = time;
        timeStamp.originType = this.getOriginType();
        timeStamp.timeDeviationCauseId = this.deviationCauseStandardId;
        if (this.useAccountHierarchy && _.includes(this.terminalAccounts.map(a => a.accountId), this.defaultTerminalAccountId))
            timeStamp.timeTerminalAccountId = this.defaultTerminalAccountId;
        this.setFilteredAccounts(timeStamp, 1);
        this.setFilteredAccounts(timeStamp, 2);
        timeStamp.manuallyAdjusted = true;
        timeStamp.isModified = true;

        return timeStamp;
    }

    private createNewTimeStampFromEntry(entry: TimeStampEntryDTO): AttestEmployeeDayTimeStampDTO {
        const timeStamp: AttestEmployeeDayTimeStampDTO = new AttestEmployeeDayTimeStampDTO();
        timeStamp.tmpId = ++this.idCounter;
        timeStamp.type = entry.type;
        timeStamp.time = entry.time;
        timeStamp.originType = this.getOriginType();
        timeStamp.timeDeviationCauseId = entry.timeDeviationCauseId;
        if (this.useAccountHierarchy) {
            const terminalAccountIds = this.terminalAccounts.map(a => a.accountId);
            if (_.includes(terminalAccountIds, entry.timeTerminalAccountId))
                timeStamp.timeTerminalAccountId = entry.timeTerminalAccountId;
            else if (_.includes(terminalAccountIds, this.defaultTerminalAccountId))
                timeStamp.timeTerminalAccountId = this.defaultTerminalAccountId;
        }
        this.setFilteredAccounts(timeStamp, 1);
        this.setFilteredAccounts(timeStamp, 2);
        timeStamp.accountId = entry.accountId;
        timeStamp.timeScheduleTypeId = entry.timeScheduleTypeId;
        timeStamp.timeScheduleTypeName = entry.timeScheduleTypeName;
        timeStamp.manuallyAdjusted = true;
        timeStamp.isModified = true;

        return timeStamp;
    }

    private filterTerminalAccounts() {
        let existingAccountIds: number[] = _.uniq(_.map(_.filter(this.model.timeStampEntrys, e => e.timeTerminalAccountId), e => e.timeTerminalAccountId));
        this.terminalAccounts = _.filter(this.terminalAccountDim.accounts, a => _.includes(this.employeeAccountIds, a.accountId));
        _.forEach(existingAccountIds, id => {
            if (!_.includes(this.terminalAccounts.map(a => a.accountId), id)) {
                let account = _.find(this.terminalAccountDim.accounts, a => a.accountId === id);
                if (account)
                    this.terminalAccounts.push(account);
            }
        });

        // Add empty account
        let acc: AccountDTO = new AccountDTO();
        acc.accountId = 0;
        acc.numberName = '';
        this.terminalAccounts.splice(0, 0, acc);
    }

    private setDefaultTerminalAccount() {
        // Set selected account to current user setting
        if (this.terminalAccountDim?.accounts) {
            if (NumberUtility.intersect(_.map(this.terminalAccountDim.accounts, a => a.accountId), this.validAccountIds).length > 0) {
                // Set account to the one that the user is working on
                let userAccountIds: number[] = this.accountHierarchyId ? this.accountHierarchyId.split('-').map(Number) : [];
                if (userAccountIds.length > 0)
                    this.defaultTerminalAccountId = _.last(userAccountIds);
                else if (this.validAccountIds.length > 0)
                    this.defaultTerminalAccountId = _.last(this.validAccountIds);
            } else if (this.model.employeeId) {
                // User working on a higher level, get account from employee
                this.employeeService.getDefaultEmployeeAccountId(this.model.employeeId, this.model.date).then(employeeAccountId => {
                    if (employeeAccountId && _.includes(_.map(this.terminalAccountDim.accounts, a => a.accountId), employeeAccountId))
                        this.defaultTerminalAccountId = employeeAccountId;
                });
            }
        }
    }

    private setEnableCreateTimeStampsFromSchedule() {
        if (this.useAccountHierarchy) {
            let hasSchedule: boolean = NumberUtility.intersect(_.map(this.getActualShifts(), s => s.accountId), this.validAccountIds).length > 0;
            let hasTimeStamps: boolean = NumberUtility.intersect(_.map(this.model.timeStampEntrys, e => e.timeTerminalAccountId), this.validAccountIds).length > 0;
            this.enableCreateTimeStampsFromSchedule = hasSchedule && !hasTimeStamps;
        } else {
            this.enableCreateTimeStampsFromSchedule = this.model.timeStampEntrys.length === 0;
        }
    }

    private validateTimeForCurrentUser(): boolean {
        let isValid: boolean = true;

        if (this.useAccountHierarchy && this.getActualShifts().length > 0) {
            let prevEntry: AttestEmployeeDayTimeStampDTO;
            _.forEach(_.orderBy(this.model.timeStampEntrys, e => e.time), entry => {
                if (entry.isModified) {
                    // If entry is modified, always check it's time for intersecting schedule
                    if (!this.entryHasValidAccount(entry.time, entry.time))
                        isValid = false;
                }

                if (isValid && prevEntry && (prevEntry.isModified || entry.isModified) && prevEntry.type === TimeStampEntryType.In && entry.type === TimeStampEntryType.Out) {
                    if (!this.entryHasValidAccount(prevEntry.time, entry.time))
                        isValid = false;
                }

                prevEntry = entry;
            });
        }

        return isValid;
    }

    private entryHasValidAccount(timeFrom: Date, timeTo: Date): boolean {
        let isValid: boolean = true;
        let shiftAccountIds = _.map(_.filter(this.getIntersectingShifts(timeFrom, timeTo), s => s.accountId), s => s.accountId);
        if (shiftAccountIds.length > 0) {
            // Check if time stamp is within a schedule that current user is not allowed to edit
            if (NumberUtility.intersect(this.validAccountIds, shiftAccountIds).length === 0)
                isValid = false;
        }
        return isValid;
    }

    private showUnlockDay(): boolean {
        if (!this.useAccountHierarchy || this.hasAttestByEmployeeAccount)
            return false;

        //never show at the same time as save button
        if (!this.isReadonly)
            return false;

        //must have at least one attested transactions
        if (!this.model.hasNoneInitialTransactions)
            return false;

        //must have min one timeTerminalAccounts (butiker)
        const timeTerminalAccountIds = _.uniq(_.map(_.filter(this.model.timeStampEntrys, t => t.timeTerminalAccountId && t.timeTerminalAccountId > 0), t => t.timeTerminalAccountId));
        if (!timeTerminalAccountIds || timeTerminalAccountIds.length < 1)
            return false;

        //must have min one valid timeTerminalAccounts (butiker) or min one valid account on schedule
        const userTimeTerminalAccountIds = NumberUtility.intersect(_.map(this.model.timeStampEntrys, e => e.timeTerminalAccountId), this.validAccountIds);
        if ((!userTimeTerminalAccountIds || userTimeTerminalAccountIds.length === 0) && !this.hasShiftWithUserAccount())
            return false;

        return true;
    }

    private setDirty(timeStamp?: AttestEmployeeDayTimeStampDTO) {
        this.isModified = true;

        if (timeStamp) {
            timeStamp.manuallyAdjusted = true;
            timeStamp.isModified = true;
        }

        if (this.onChange)
            this.onChange();

        this.setEnableCreateTimeStampsFromSchedule();
    }
}

