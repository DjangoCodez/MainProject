import { ShiftDTO, TemplateScheduleShiftDTO, TemplateScheduleEmployeeDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { TimeScheduleTemplateHeadSmallDTO } from "../../../../../Common/Models/timescheduletemplatedtos";
import { IEmployeeSchedulePlacementGridViewDTO } from "../../../../../Scripts/TypeLite.Net4";
import { TemplateScheduleModes, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService } from "../../../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { StringUtility, Guid } from "../../../../../Util/StringUtility";
import { StringKeyValue } from "../../../../../Common/Models/StringKeyValue";
import { TermGroup_ShiftHistoryType, SoeScheduleWorkRules, ActionResultSave, TermGroup, TermGroup_TemplateScheduleActivateFunctions } from "../../../../../Util/CommonEnumerations";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { RecalculateTimeStatusDialogController } from "../../../../Dialogs/RecalculateTimeStatus/RecalculateTimeStatusDialogController";
import { EmployeeAccountDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { IFocusService } from "../../../../../Core/Services/focusservice";
import { ActiveScheduleControlDialogController } from "../../../../Dialogs/ActiveScheduleControl/ActiveScheduleControlDialogController";
import { ActivateScheduleControlDTO } from "../../../../../Common/Models/EmployeeScheduleDTOs";

export class TemplateScheduleController {

    // Terms
    private terms: any;
    private title: string;
    private workTimeText: string;
    private placementExistsInfo: string;

    // Data
    private shifts: ShiftDTO[];
    private templateShifts: TemplateScheduleShiftDTO[];
    private employeeTemplates: TimeScheduleTemplateHeadSmallDTO[];
    private placement: IEmployeeSchedulePlacementGridViewDTO;
    private placementFunctions: SmallGenericType[];
    private employeeAccounts: EmployeeAccountDTO[] = [];

    // Properties
    private get isTemplateGroup(): boolean {
        return !!(this.templateHead && this.templateHead.timeScheduleTemplateGroupId);
    }

    private _copyFromEmployeeId: any;
    private get copyFromEmployeeId(): any {
        return this._copyFromEmployeeId;
    }
    private set copyFromEmployeeId(id: any) {
        var emp = _.find(this.employees, e => e.employeeId === id);
        if (emp) {
            this._copyFromEmployeeId = { id: emp.employeeId, numberAndName: emp.numberAndName };
            this.$timeout(() => {
                this.loadTemplateHeadsForEmployee();
            });
        } else {
            this._copyFromEmployeeId = undefined;
            this.employeeTemplates = [];
        }
    }

    private _copyFromTemplateHeadId: number;
    private get copyFromTemplateHeadId(): number {
        return this._copyFromTemplateHeadId;
    }
    private set copyFromTemplateHeadId(id: number) {
        var head = _.find(this.employeeTemplates, t => t.timeScheduleTemplateHeadId === id);
        if (head) {
            this._copyFromTemplateHeadId = id;
            this.loadTemplateHeadForEmployee(head);
        } else {
            this._copyFromTemplateHeadId = undefined;
        }
    }
    private setsourceId(id: number) {
        this._copyFromEmployeeId = { id: null, numberAndName: '' };
        this.focusService.focusByName("ctrl_copyFromEmployeeId");
    }
    private weekInCycle: number = 1;
    private totalWorkTimeMinutes: number;

    private placementFunction: TermGroup_TemplateScheduleActivateFunctions;
    private placementStartDate: Date;
    private placementStopDate: Date;
    private placementPreliminary: boolean = false;

    // Flags
    private executing: boolean = false;
    private placementExistsOnCurrentTemplate: boolean = false;
    private activateExpanderOpen: boolean = false;
    private useAccountingFromSourceSchedule: boolean = true;

    // Validation
    private mandatoryFieldKeys: string[] = [];
    private validationErrorKeys: string[] = [];
    private validationErrorStrings: string[] = [];

    private progress: IProgressHandler;

    private modalInstance: any;
    private edit: ng.IFormController;

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        progressHandlerFactory: IProgressHandlerFactory,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private mode: TemplateScheduleModes,
        private useAccountHierarchy: boolean,
        private useStopDate: boolean,
        private skipWorkRules: boolean,
        private employees: TemplateScheduleEmployeeDTO[],
        private hiddenEmployeeId: number,
        private selectedPeriodFrom: Date,
        private selectedPeriodTo: Date,
        private templateHead: TimeScheduleTemplateHeadSmallDTO,
        private employee: EmployeeListDTO,
        private startDate: Date,
        private nbrOfWeeks: number,
        private placementDefaultPreliminary: boolean,
        private focusService: IFocusService,
        private placementHidePreliminary: boolean) {

        this.modalInstance = $uibModal;
        this.progress = progressHandlerFactory.create();

        this.$q.all([
            this.loadTerms(),
            this.loadTemplateHead(),
        ]).then(() => {
            this.loadPlacement().then(() => {
                this.setup();
            });
        });
    }

    // SETUP

    private setup() {
        this.activateExpanderOpen = (this.mode === TemplateScheduleModes.Activate);
        // Make it possible to save without changing anything
        if (this.mode === TemplateScheduleModes.New || this.mode === TemplateScheduleModes.Activate)
            this.edit.$dirty = true;

        if (this.mode === TemplateScheduleModes.Activate && this.placementDefaultPreliminary)
            this.placementPreliminary = true;

        if (this.templateHead) {
            this.startDate = this.templateHead.startDate;

            if (this.templateHead.noOfDays !== this.selectedPeriodTo.addSeconds(1).diffDays(this.selectedPeriodFrom)) {
                // Number of days in existing template is not the same as visible days in schedule view.
                this.selectedPeriodTo = this.selectedPeriodFrom.addDays(this.templateHead.noOfDays).addSeconds(-1);
            }

            this.setNbrOfWeeksFromTemplate();
            if (!this.templateHead.firstMondayOfCycle)
                this.setFirstMondayOfCycle();
            this.setWeekInCycle();
            this.checkExistingPlacement();
        } else {
            this.templateHead = new TimeScheduleTemplateHeadSmallDTO();
            this.templateHead.startDate = this.startDate;
            this.templateHead.noOfDays = this.nbrOfWeeks * 7;
            this.setFirstMondayOfCycle();
            this.loadFunctions();
        }

        this.getEmployeeAccounts();
        this.copyFromEmployeeId = this.employee.employeeId;

        this.resetShifts();
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.weekshort",
            "time.schedule.planning.contextmenu.newtemplate",
            "time.schedule.planning.contextmenu.edittemplate",
            "time.schedule.planning.templateschedule.activate",
            "time.schedule.planning.templateschedule.activate.progressmessage"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            if (this.mode === TemplateScheduleModes.New)
                this.title = this.terms["time.schedule.planning.contextmenu.newtemplate"];
            else if (this.mode === TemplateScheduleModes.Edit)
                this.title = this.terms["time.schedule.planning.contextmenu.edittemplate"];
            else if (this.mode === TemplateScheduleModes.Activate)
                this.title = this.terms["time.schedule.planning.templateschedule.activate"];
        });
    }

    private loadPlacement(): ng.IPromise<any> {
        return this.sharedScheduleService.getLastPlacementForEmployee(this.employee.employeeId, this.templateHead && this.templateHead.timeScheduleTemplateHeadId ? this.templateHead.timeScheduleTemplateHeadId : 0).then(x => {
            this.placement = x;
        });
    }

    private checkExistingPlacement() {
        this.sharedScheduleService.getTimeScheduleTemplate(this.templateHead.timeScheduleTemplateHeadId, true, false).then(x => {
            this.placementExistsOnCurrentTemplate = x && x.employeeSchedules && x.employeeSchedules.length > 0;
            this.loadFunctions();

            this.translationService.translate("time.schedule.planning.templateschedule.placementexists").then(term => {
                this.placementExistsInfo = StringUtility.ToBr(term);
            });
        });
    }

    private getEmployeeAccounts() {
        if (this.useAccountHierarchy) {
            this.scheduleService.getEmployeeAccounts([this.employee.employeeId], this.startDate, this.startDate.addDays(this.nbrOfWeeks * 7)).then(x => {
                this.employeeAccounts = x;
            });
        }
    }

    private loadTemplateHead(): ng.IPromise<any> {
        let deferral = this.$q.defer();

        if (this.templateHead && this.templateHead.timeScheduleTemplateHeadId) {
            this.scheduleService.getTimeScheduleTemplateHeadSmall(this.templateHead.timeScheduleTemplateHeadId).then(x => {
                this.templateHead = x;
                if (this.templateHead.noOfDays === 0)
                    this.templateHead.noOfDays = 7;

                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadTemplateHeadsForEmployee() {
        this.employeeTemplates = [];
        if (!this.copyFromEmployeeId)
            return;

        // For now, if we copy from same same employee, do not exclude templates with multiple accounts.
        // Should have a better solution where we check if the employee has the account for each day.
        let sameEmployee: boolean = (this.employee.employeeId === this.copyFromEmployeeId.id);
        let excludeMultipleAccounts: boolean = (this.useAccountHierarchy && this.useAccountingFromSourceSchedule && !sameEmployee);

        this.sharedScheduleService.getTimeScheduleTemplateHeadsForEmployee(this.copyFromEmployeeId.id, null, null, false, excludeMultipleAccounts).then(x => {
            this.employeeTemplates = x;

            _.forEach(this.employeeTemplates, t => {
                t['description'] = "{0}, {1} {2}, {3}".format(t.startDate.toFormattedDate(), (t.noOfDays / 7).toString(), this.terms["common.weekshort"], t.name);
                if (t.accountName)
                    t['description'] += " ({0})".format(t.accountName);
            });

            if (this.useAccountHierarchy) {
                let accountIds = _.uniq(this.employeeAccounts.map(e => e.accountId));
                this.employeeTemplates = _.filter(this.employeeTemplates, t => !t.accountId || _.includes(accountIds, t.accountId));
            }

            // Add empty
            var emptyTemplate = new TimeScheduleTemplateHeadSmallDTO();
            emptyTemplate.timeScheduleTemplateHeadId = 0;
            emptyTemplate['description'] = '';
            emptyTemplate.noOfDays = this.nbrOfWeeks * 7;
            this.employeeTemplates.splice(0, 0, emptyTemplate);

            // If current template is in list, select it (happens when editing a template).
            // This will cause the whole template to be loaded, not just the visible part.
            if (this.templateHead.timeScheduleTemplateHeadId && _.includes(_.map(this.employeeTemplates, t => t.timeScheduleTemplateHeadId), this.templateHead.timeScheduleTemplateHeadId))
                this.copyFromTemplateHeadId = this.templateHead.timeScheduleTemplateHeadId;
            else {
                if (this.isTemplateGroup)
                    this.loadTemplateHeadForEmployee(this.templateHead);
                else
                    this.copyFromTemplateHeadId = 0;
            }
        });
    }

    private loadTemplateHeadForEmployee(sourceTemplate: TimeScheduleTemplateHeadSmallDTO) {
        this.shifts = [];

        if (sourceTemplate && sourceTemplate.timeScheduleTemplateHeadId) {
            if (sourceTemplate.noOfDays === 0)
                sourceTemplate.noOfDays = 7;

            this.progress.startLoadingProgress([() => this.sharedScheduleService.getTimeScheduleTemplateHeadForEmployee(this.selectedPeriodFrom, this.selectedPeriodTo, sourceTemplate.timeScheduleTemplateHeadId).then(x => {
                var guids: StringKeyValue[] = [];

                this.shifts = x.map(s => {
                    var obj = new ShiftDTO();
                    angular.extend(obj, s);
                    obj.fixDates();
                    obj.startTime = obj.actualStartTime;
                    obj.stopTime = obj.actualStopTime;

                    // Clear ids
                    obj.timeScheduleTemplateBlockId = 0;
                    obj.tempTimeScheduleTemplateBlockId = 0;
                    obj.timeScheduleTemplatePeriodId = 0;
                    obj.break1Id = 0;
                    obj.break1Link = null;
                    obj.break2Id = 0;
                    obj.break2Link = null;
                    obj.break3Id = 0;
                    obj.break3Link = null;
                    obj.break4Id = 0;
                    obj.break4Link = null;
                    obj.isModified = true;

                    // Set target employee
                    obj.employeeId = this.employee.employeeId;

                    // Every unique source Guid will get one new target Guid
                    // Linked source shifts will still be linked as targets, but with a new Guid
                    if (obj.link) {
                        var newGuidItem: StringKeyValue = _.find(guids, g => g.key === obj.link);
                        if (newGuidItem) {
                            obj.link = newGuidItem.value;
                        } else {
                            var newGuid: string = Guid.newGuid().toString();
                            guids.push(new StringKeyValue(obj.link, newGuid));
                            obj.link = newGuid;
                        }
                    } else {
                        obj.link = Guid.newGuid().toString();
                    }

                    return obj;
                });

                this.templateHead.noOfDays = sourceTemplate.noOfDays;
                this.setNbrOfWeeksFromTemplate();
                this.resetShifts();
            })]);
        } else {
            this.resetShifts();
        }
    }

    // EVENTS

    private startDateChanged() {
        this.$timeout(() => {
            this.setFirstMondayOfCycle();
            this.getEmployeeAccounts();
        });
    }

    private nbrOfWeeksChanged() {
        this.$timeout(() => {
            this.setNbrOfDaysOnTemplate();
            this.validateWeekInCycle();
            this.setFirstMondayOfCycle();

            // Remove shifts outside of template
            _.pullAll(this.shifts, _.filter(this.shifts, s => s.dayNumber > this.templateHead.noOfDays));
            this.resetShifts();
            this.getEmployeeAccounts();
        });
    }

    private weekInCycleChanged() {
        this.$timeout(() => {
            this.validateWeekInCycle();
            this.setFirstMondayOfCycle();
        });
    }

    private validateWeekInCycle() {
        if (this.weekInCycle <= 0)
            this.weekInCycle = 1;
        if (this.weekInCycle > this.nbrOfWeeks)
            this.weekInCycle = this.nbrOfWeeks;
    }

    private placementFunctionChanged() {
        this.$timeout(() => {
            if (this.placementFunction == TermGroup_TemplateScheduleActivateFunctions.NewPlacement) {
                if (this.placementExistsOnCurrentTemplate) {
                    this.placementStartDate = this.placement.employeeScheduleStopDate.addDays(1);
                    this.placementStopDate = null;
                } else {
                    this.placementStartDate = this.selectedPeriodFrom;
                    this.placementStopDate = this.selectedPeriodTo;
                }
            } else if (this.placementFunction == TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate) {
                this.placementStartDate = null;
                this.placementStopDate = (this.placementExistsOnCurrentTemplate ? this.placement.employeeScheduleStopDate : null);
            }
        });
    }

    private useAccountingFromSourceScheduleChanged() {
        this.$timeout(() => {
            if (this.copyFromEmployeeId)
                this.loadTemplateHeadsForEmployee();
        });
    }

    // ACTIONS

    private openRecalculateTimeStatusDialog(activateMode: boolean) {
        this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/RecalculateTimeStatus/Views/RecalculateTimeStatusDialog.html"),
            controller: RecalculateTimeStatusDialogController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            keyboard: false,
            size: 'xl',
            resolve: {
                activateMode: () => { return activateMode; },
                items: () => { return []; },
                selectedFunction: () => { return null; },
                selectedHeadId: () => { return 0; },
                selectedPeriodId: () => { return 0; },
                startDate: () => { return null; },
                stopDate: () => { return null; },
                preliminary: () => { return false; },
            }
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private close(action: string) {
        this.$uibModalInstance.close({ success: true, action: action });
    }

    private initSave() {
        this.executing = true;

        this.validateSave().then(result => {
            if (result) {
                // Do not evaluate work rules for hidden employee
                if (this.employee.employeeId === this.hiddenEmployeeId) {
                    this.save(true, false);
                } else {
                    this.validateWorkRules(TermGroup_ShiftHistoryType.TemplateScheduleSave).then(resultWorkRules => {
                        if (resultWorkRules)
                            this.save(true, false);
                        else
                            this.executing = false;
                    });
                }
            } else {
                this.executing = false;
            }
        });
    }

    private validateSave(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        // Check if any placement exists on current start date
        if (this.templateHead.startDate) {
            this.scheduleService.hasEmployeeSchedule(this.employee.employeeId, this.templateHead.startDate).then(result => {
                if (result) {
                    var keys: string[] = [
                        "core.warning",
                        "time.schedule.planning.templateschedule.placementexistsendafter"
                    ];

                    this.translationService.translateMany(keys).then(terms => {
                        var modal = this.notificationService.showDialogEx(terms["core.warning"], terms["time.schedule.planning.templateschedule.placementexistsendafter"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            if (val)
                                deferral.resolve(true);
                        }, (reason) => {
                            deferral.resolve(false);
                        });
                    });
                } else {
                    deferral.resolve(true);
                }
            });
        } else {
            deferral.resolve(false);
        }

        return deferral.promise;
    }

    private validateWorkRules(action: TermGroup_ShiftHistoryType): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var rules: SoeScheduleWorkRules[] = null;
        if (this.skipWorkRules) {
            // The following rules should always be evaluated
            rules = [];
            rules.push(SoeScheduleWorkRules.OverlappingShifts);
        }

        if (!this.shifts || this.shifts.length === 0)
            deferral.resolve(true);
        else {
            this.progress.startWorkProgress((completion) => {
                this.sharedScheduleService.evaluatePlannedShiftsAgainstWorkRules(this.shifts, rules, this.employee.employeeId, true, null).then(result => {
                    completion.completed(null, true);
                    this.notificationService.showValidateWorkRulesResult(action, result, this.employee.employeeId).then(passed => {
                        deferral.resolve(passed);
                    });
                });
            }, null);
        }

        return deferral.promise;
    }

    private save(saveTemplate: boolean, activateTemplate: boolean) {
        if (activateTemplate && this.placement && this.placement.employeeScheduleStopDate && this.placement.employeeScheduleStopDate > this.placementStopDate) {
            this.scheduleService.controlActivation(this.employee.employeeId, this.placement.employeeScheduleStartDate, this.placement.employeeScheduleStopDate, this.placementStartDate, this.placementStopDate, false).then(control => {
                if (!control.hasWarnings) {
                    this.saveTimeScheduleTemplateAndPlacement(control, saveTemplate, true);
                }
                else {
                    var modal = this.modalInstance.open({
                        templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/ActiveScheduleControl/Views/ActiveScheduleControlDialog.html"),
                        controller: ActiveScheduleControlDialogController,
                        controllerAs: 'ctrl',
                        bindToController: true,
                        backdrop: 'static',
                        keyboard: true,
                        size: 'xl',
                        windowClass: 'fullsize-modal',
                        scope: this.$scope,
                        resolve: {
                            control: () => { return control; },
                            activateDate: () => { return this.placementStopDate; }
                        }
                    });

                    modal.result.then(val => {
                        control.createResult();
                        this.saveTimeScheduleTemplateAndPlacement(control, saveTemplate, true);
                    }, (reason) => {
                        // Cancelled
                    });
                }
            });
        }
        else {
            this.saveTimeScheduleTemplateAndPlacement(null, saveTemplate, activateTemplate);
        }
    }

    private saveTimeScheduleTemplateAndPlacement(control: ActivateScheduleControlDTO, saveTemplate: boolean, activateTemplate: boolean) {
        this.progress.startWorkProgress((completion) => {
            this.scheduleService.saveTimeScheduleTemplateAndPlacement(saveTemplate, activateTemplate, control, this.shifts ? this.shifts : [], this.templateHead.timeScheduleTemplateHeadId ? this.templateHead.timeScheduleTemplateHeadId : 0, this.templateHead.noOfDays, this.templateHead.startDate, this.templateHead.stopDate, this.templateHead.firstMondayOfCycle, this.placementStartDate, this.placementStopDate, this.startDate, this.templateHead.simpleSchedule, false, this.placementPreliminary, this.templateHead.locked, this.employee.employeeId, null, this.useAccountingFromSourceSchedule).then(result => {
                completion.completed(null, true);
                if (result.success) {
                    this.close('save');
                } else {
                    var key: string;
                    if (result.successNumber == ActionResultSave.SaveTemplateSchedule_TemplateSaved)
                        key = "time.schedule.planning.templateschedule.save.error.templatesaved";
                    else
                        key = "time.schedule.planning.templateschedule.save.error";

                    this.translationService.translate(key).then(term => {
                        this.executing = false;
                        this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                    });
                }
            }).catch(reason => {
                completion.failed(reason);
            });
        }, null, activateTemplate ? this.terms["time.schedule.planning.templateschedule.activate.progressmessage"] : '');
    }

    private initDelete() {
        this.executing = true;

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.schedule.planning.templateschedule.deleteverification"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialogEx(terms["core.warning"], terms["time.schedule.planning.templateschedule.deleteverification"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val)
                    this.delete();
            }, (reason) => {
                this.executing = false;
            });
        });
    }

    private delete() {
        if (!this.templateHead.timeScheduleTemplateHeadId)
            return;

        this.scheduleService.deleteTimeScheduleTemplateHead(this.templateHead.timeScheduleTemplateHeadId).then(result => {
            if (result.success) {
                this.close('delete');
            } else {
                this.translationService.translate("time.schedule.planning.templateschedule.deleteerror").then(term => {
                    this.executing = false;
                    this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                });
            }
        });
    }

    private initActivate() {
        this.executing = true;

        if (this.placementFunction === TermGroup_TemplateScheduleActivateFunctions.NewPlacement && this.placementStartDate) {
            this.scheduleService.hasEmployeeSchedule(this.employee.employeeId, this.placementStartDate).then(exists => {
                if (exists) {
                    var keys: string[] = [
                        "core.warning",
                        "time.schedule.planning.templateschedule.placementexistsendafter"
                    ];

                    this.translationService.translateMany(keys).then(terms => {
                        var modal = this.notificationService.showDialogEx(terms["core.warning"], terms["time.schedule.planning.templateschedule.placementexistsendafter"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            this.activate();
                        }, (reason) => {
                            this.executing = false;
                        });
                    });
                }
                else {
                    this.activate();
                }
            });
        }
        else {
            this.activate();
        }
    }

    private activate() {
        // Do not evaluate work rules for hidden employee
        if (this.employee.employeeId === this.hiddenEmployeeId) {
            this.save(false, true);
        } else {
            this.validateWorkRules(TermGroup_ShiftHistoryType.TemplateScheduleActivate).then(result => {
                if (result)
                    this.save(false, true);
                else
                    this.executing = false;
            });
        }
    }

    // HELP-METHODS

    private setFirstMondayOfCycle() {
        if (!this.templateHead || !this.templateHead.startDate || !this.weekInCycle) {
            this.templateHead.firstMondayOfCycle = null;
            return;
        }

        let date = this.templateHead.startDate.beginningOfWeek().addWeeks(-(this.weekInCycle - 1));
        this.templateHead.firstMondayOfCycle = date;
    }

    private setWeekInCycle() {
        if (!this.templateHead || !this.templateHead.startDate || !this.templateHead.firstMondayOfCycle) {
            this.weekInCycle = 1;
            return;
        }

        this.weekInCycle = this.templateHead.startDate.beginningOfWeek().diffDays(this.templateHead.firstMondayOfCycle) / 7 + 1;
        while (this.weekInCycle > (this.templateHead.noOfDays / 7)) {
            this.weekInCycle -= (this.templateHead.noOfDays / 7);
        }
    }

    private get startDateIsFirstMondayOfCycle(): boolean {
        return this.templateHead && this.templateHead.startDate && this.templateHead.firstMondayOfCycle && this.templateHead.startDate.isSameDayAs(this.templateHead.firstMondayOfCycle);
    }

    private setNbrOfWeeksFromTemplate() {
        var nbrOfDays = this.templateHead.noOfDays;
        var nbr = nbrOfDays / 7;
        if (nbr < 1)
            nbr = 1;

        this.nbrOfWeeks = nbr;
    }

    private setNbrOfDaysOnTemplate() {
        var nbr = this.nbrOfWeeks;
        if (nbr < 1)
            nbr = this.nbrOfWeeks = 1;
        this.templateHead.noOfDays = (nbr * 7);
        this.selectedPeriodTo = this.selectedPeriodFrom.addDays(this.templateHead.noOfDays).addSeconds(-1);
    }

    private loadFunctions(): ng.IPromise<any> {
        this.placementFunctions = [];

        return this.coreService.getTermGroupContent(TermGroup.TemplateScheduleActivateFunctions, false, true).then(x => {
            this.placementFunctions = x;

            if (!this.placementExistsOnCurrentTemplate)
                _.pull(this.placementFunctions, _.find(this.placementFunctions, f => f.id == TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate));

            // If placement exists, set default action to 'Change stop date', otherwise 'New placement'
            this.placementFunction = (this.placementExistsOnCurrentTemplate ? TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate : TermGroup_TemplateScheduleActivateFunctions.NewPlacement);
            this.placementFunctionChanged();
        });
    }

    private rotateShifts(down: boolean) {
        if (!this.shifts || this.shifts.length === 0)
            return;

        var increaseBy: number = (down ? 7 : -7);

        _.forEach(_.orderBy(this.shifts, s => s.actualStartTime), shift => {
            this.rotateShift(shift, increaseBy);
        });

        var nbrOfDays: number = this.nbrOfWeeks * 7;

        if (down) {
            //  Move the last week up to 1
            _.forEach(_.filter(this.shifts, s => s.dayNumber > nbrOfDays), shift => {
                this.rotateShift(shift, -nbrOfDays);
            });
        } else {
            //  Move the first week down after last
            _.forEach(_.filter(this.shifts, s => s.dayNumber < 1), shift => {
                this.rotateShift(shift, nbrOfDays);
            });
        }

        this.resetShifts();
        this.edit.$dirty = true;
    }

    private rotateShift(shift: ShiftDTO, nbrOfDays: number) {
        shift.startTime = shift.startTime.addDays(nbrOfDays);
        shift.stopTime = shift.stopTime.addDays(nbrOfDays);
        shift.actualStartTime = shift.actualStartTime.addDays(nbrOfDays);
        shift.actualStopTime = shift.actualStopTime.addDays(nbrOfDays);
        shift.dayNumber += nbrOfDays;

        if (shift.break1TimeCodeId)
            shift.break1StartTime = shift.break1StartTime.addDays(nbrOfDays);
        if (shift.break2TimeCodeId)
            shift.break2StartTime = shift.break2StartTime.addDays(nbrOfDays);
        if (shift.break3TimeCodeId)
            shift.break3StartTime = shift.break3StartTime.addDays(nbrOfDays);
        if (shift.break4TimeCodeId)
            shift.break4StartTime = shift.break4StartTime.addDays(nbrOfDays);
    }

    private resetShifts() {
        this.templateShifts = [];
        if (!this.templateHead)
            return;

        let startDate: Date = this.templateHead.firstMondayOfCycle ? this.templateHead.firstMondayOfCycle : this.templateHead.startDate;
        if (!startDate)
            return;

        // Group shifts on day number (one row per day)
        // New and modified shifts
        _.forEach(_.sortBy(_.map(_.uniqBy(this.shifts, s => s.dayNumber), s => s.dayNumber)), dayNumber => {
            this.templateShifts.push(TemplateScheduleShiftDTO.convertShiftsToDTO(_.filter(this.shifts, s => s.dayNumber === dayNumber), startDate, this.templateHead.noOfDays));
        });

        // Fill up with empty shifts
        for (var i = 0; i < this.templateHead.noOfDays; i++) {
            if (_.filter(this.shifts, s => s.dayNumber === i + 1).length === 0) {
                var date = startDate.addDays(i);
                var dto = new TemplateScheduleShiftDTO();
                dto.startTime = date;
                dto.stopTime = date;
                dto.dayOfWeek = date.dayOfWeek();
                dto.weekNbr = Math.floor(i / 7) + 1;
                this.templateShifts.push(dto);
            }
        }

        _.forEach(this.templateShifts, shift => {
            shift['sortDayOfWeek'] = shift.startTime.sortOnMonday();
        });

        this.templateShifts = _.orderBy(this.templateShifts, ['weekNbr', 'sortDayOfWeek', 'startTime']);

        this.totalWorkTimeMinutes = _.sumBy(this.templateShifts, s => s.duration);
    }

    // VALIDATION

    private validate() {
        var errors = this['edit'].$error;

        if (this.templateHead) {
            if (!this.templateHead.startDate)
                this.mandatoryFieldKeys.push("time.schedule.planning.templateschedule.startdate");

            if (errors['locked'])
                this.validationErrorKeys.push("time.schedule.planning.templateschedule.locked");

            if (errors['stopDateBeforeStartDate'])
                this.validationErrorKeys.push("time.schedule.planning.templateschedule.stopdatebeforestartdate");

            if (errors['stopDateBeforePlacementStopDate'])
                this.validationErrorKeys.push("time.schedule.planning.templateschedule.stopdatebeforeplacementstopdate");

            if (errors['missingPlacementDates'])
                this.validationErrorKeys.push("time.schedule.planning.templateschedule.missingplacementdates");

            if (errors['placementStopDateBeforePlacementStartDate'])
                this.validationErrorKeys.push("time.schedule.planning.templateschedule.placementstopdatebeforeplacementstartdate");

            if (errors['missingPlacementStopDate'])
                this.validationErrorKeys.push("time.schedule.planning.templateschedule.missingplacementstopdate");

            if (errors['placementStopDateBeforeEmployeeScheduleStartDate'])
                this.validationErrorKeys.push("time.schedule.planning.templateschedule.placementstopdatebeforeemployeeschedulestartdate");
        }
    }

    private showValidationError() {
        this.mandatoryFieldKeys = [];
        this.validationErrorKeys = [];
        this.validationErrorStrings = [];
        this.validate();

        var keys: string[] = [];

        if (this.mandatoryFieldKeys.length > 0 || this.validationErrorKeys.length > 0 || this.validationErrorStrings.length > 0) {
            keys.push("error.unabletosave_title");

            // Mandatory fields
            if (this.mandatoryFieldKeys.length > 0) {
                keys.push("core.missingmandatoryfield");
                _.forEach(this.mandatoryFieldKeys, (key) => {
                    keys.push(key);
                });
            }

            // Other messages
            if (this.validationErrorKeys.length > 0) {
                _.forEach(this.validationErrorKeys, (key) => {
                    keys.push(key);
                });
            }
        }

        if (keys.length > 0) {
            this.translationService.translateMany(keys).then((terms) => {
                var message: string = "";

                // Mandatory fields
                if (this.mandatoryFieldKeys.length > 0) {
                    _.forEach(this.mandatoryFieldKeys, (key) => {
                        message = message + terms["core.missingmandatoryfield"] + " " + terms[key].toLocaleLowerCase() + ".\\n";
                    });
                }

                // Other messages
                if (this.validationErrorKeys.length > 0) {
                    _.forEach(this.validationErrorKeys, (key) => {
                        message = message + terms[key] + ".\\n";
                    });
                }

                // Predefined messages
                if (this.validationErrorStrings.length > 0) {
                    _.forEach(this.validationErrorStrings, (str) => {
                        message = message + str + ".\\n";
                    });
                }

                this.notificationService.showDialog(terms["error.unabletosave_title"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        }
    }
}
