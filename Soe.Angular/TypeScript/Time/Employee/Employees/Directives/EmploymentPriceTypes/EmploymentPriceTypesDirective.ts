import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmploymentDTO, EmploymentPriceTypeDTO, EmploymentPriceTypePeriodDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { PayrollGroupPriceTypeDTO, PayrollGroupPriceTypePeriodDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { EmploymentPriceTypePeriodDialogController } from "./EmploymentPriceTypePeriodDialogController";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { SoeEntityType, TermGroup_TrackChangesAction } from "../../../../../Util/CommonEnumerations";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { TrackChangesDTO } from "../../../../../Common/Models/TrackChangesDTO";
import { PayrollLevelDTO } from "../../../../../Common/Models/PayrollLevelDTO";

export class EmploymentPriceTypesDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/EmploymentPriceTypes/Views/EmploymentPriceTypes.html'),
            scope: {
                employment: '=',
                employmentPriceTypes: '=',
                payrollGroupPriceTypes: '=',
                payrollLevels: '=',
                readOnly: '=',
                changeDate: '=?',
                isValid: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: EmploymentPriceTypesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmploymentPriceTypesController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private employment: EmploymentDTO;
    private priceTypes: EmploymentPriceTypeDTO[] = [];
    private employmentPriceTypes: EmploymentPriceTypeDTO[];
    private payrollGroupPriceTypes: PayrollGroupPriceTypeDTO[];
    private payrollLevels: PayrollLevelDTO[];
    private selectedPriceType: EmploymentPriceTypeDTO;
    private changeDate?: Date;

    // Flags
    private readOnly: boolean;
    private allPeriodsExpanded: boolean = false;
    private isValid: boolean = false;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService) {
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {

        this.$scope.$watchCollection(() => this.employmentPriceTypes, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.$timeout(() => {
                    this.mergePriceTypes(this.changeDate, false);
                });
            }
        });
        this.$scope.$watchCollection(() => this.payrollGroupPriceTypes, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.$timeout(() => {
                    this.mergePriceTypes(this.changeDate, !!oldVal);
                });
            }
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [];

        keys.push("common.by");
        keys.push("common.from");
        keys.push("common.to");
        keys.push("time.employee.employmentpricetype.history");
        keys.push("time.employee.employmentpricetype.change.insert.pricetype");
        keys.push("time.employee.employmentpricetype.change.insert.pricetypeperiod");
        keys.push("time.employee.employmentpricetype.change.update.pricetype");
        keys.push("time.employee.employmentpricetype.change.update.pricetypeperiod");
        keys.push("time.employee.employmentpricetype.change.delete.pricetype");
        keys.push("time.employee.employmentpricetype.change.delete.pricetypeperiod");

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTrackChanges(entity: SoeEntityType, recordId: number, includeChildren: boolean) {
        if (!recordId)
            return;

        this.coreService.getTrackChanges(entity, recordId, includeChildren).then((changes: TrackChangesDTO[]) => {
            var msg: string = '';
            _.forEach(changes, change => {
                switch (change.action) {
                    case TermGroup_TrackChangesAction.Insert:
                        msg += change.entity == SoeEntityType.EmploymentPriceType ? this.terms["time.employee.employmentpricetype.change.insert.pricetype"] : this.terms["time.employee.employmentpricetype.change.insert.pricetypeperiod"];
                        if (change.created)
                            msg += ' {0}'.format(change.created.toFormattedDateTime());
                        if (change.createdBy)
                            msg += ' {0} {1}'.format(this.terms["common.by"], change.createdBy);
                        msg += '\n';
                        break;
                    case TermGroup_TrackChangesAction.Update:
                        msg += change.entity == SoeEntityType.EmploymentPriceType ? this.terms["time.employee.employmentpricetype.change.update.pricetype"] : this.terms["time.employee.employmentpricetype.change.update.pricetypeperiod"];
                        if (change.created)
                            msg += ' {0}'.format(change.created.toFormattedDateTime());
                        if (change.createdBy)
                            msg += ' {0} {1}'.format(this.terms["common.by"], change.createdBy);
                        msg += ', ';
                        msg += '{0} {1} {2} {3}'.format(this.terms["common.from"].toLocaleLowerCase(), change.fromValue, this.terms["common.to"].toLocaleLowerCase(), change.toValue);
                        msg += '\n';
                        break;
                    case TermGroup_TrackChangesAction.Delete:
                        msg += change.entity == SoeEntityType.EmploymentPriceType ? this.terms["time.employee.employmentpricetype.change.delete.pricetype"] : this.terms["time.employee.employmentpricetype.change.delete.pricetypeperiod"];
                        if (change.created)
                            msg += ' {0}'.format(change.created.toFormattedDateTime());
                        if (change.createdBy)
                            msg += ' {0} {1}'.format(this.terms["common.by"], change.createdBy);
                        msg += '\n';
                        break;
                }
            });

            this.notificationService.showDialogEx(this.terms["time.employee.employmentpricetype.history"], msg, SOEMessageBoxImage.Information);
        });
    }

    // EVENTS

    private expandAllPeriods() {
        _.forEach(this.priceTypes, priceType => {
            priceType['expanded'] = true;
        });
        this.allPeriodsExpanded = true;
    }

    private collapseAllPeriods() {
        _.forEach(this.priceTypes, priceType => {
            priceType['expanded'] = false;
        });
        this.allPeriodsExpanded = false;
    }

    private showHistory(priceType: EmploymentPriceTypeDTO) {
        this.loadTrackChanges(SoeEntityType.EmploymentPriceType, priceType.employmentPriceTypeId, true);
    }

    private showPeriodHistory(period: EmploymentPriceTypePeriodDTO) {
        this.loadTrackChanges(SoeEntityType.EmploymentPriceTypePeriod, period.employmentPriceTypePeriodId, false);
    }

    private editPeriod(period: EmploymentPriceTypePeriodDTO) {

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/EmploymentPriceTypes/Views/EmploymentPriceTypePeriodDialog.html"),
            controller: EmploymentPriceTypePeriodDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                period: () => { return period },
                payrollLevels: () => { return this.getSelectableLevels(); },
                payrollPriceTypeId: () => { return this.selectedPriceType.payrollPriceTypeId; },
                payrollGroupPriceTypes: () => { return this.payrollGroupPriceTypes; }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.period) {
                let originalPriceType = this.getOriginalPriceType(true);
                if (!period) {
                    // Add new period to the original collection
                    period = new EmploymentPriceTypePeriodDTO();
                    period.fromDate = result.period.fromDate;
                    period.amount = result.period.amount;
                    period.payrollLevelId = result.period.payrollLevelId;

                    if (originalPriceType) {
                        period.employmentPriceTypeId = originalPriceType.employmentPriceTypeId;
                        originalPriceType.periods.push(period);
                    }
                } else {
                    // Update period on original price type
                    if (originalPriceType) {
                        var originalPeriod = _.find(originalPriceType.periods, p => p.employmentPriceTypePeriodId === result.period.employmentPriceTypePeriodId)
                        if (originalPeriod) {
                            originalPeriod.fromDate = result.period.fromDate;
                            originalPeriod.amount = result.period.amount;
                            period.payrollLevelId = result.period.payrollLevelId;
                        }
                    }
                }

                this.setPayrollLevelName(period);

                this.changeDate = result.period.fromDate;
                this.mergePriceTypes(this.changeDate, false);

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deletePeriod(priceType: any, period: EmploymentPriceTypePeriodDTO) {
        var originalPriceType = this.getOriginalPriceType(false);
        if (originalPriceType) {
            var originalPeriod = _.find(originalPriceType.periods, p => p.employmentPriceTypePeriodId === period.employmentPriceTypePeriodId)
            if (originalPeriod && priceType.payrollPriceTypeId == originalPriceType.payrollPriceTypeId)
                _.pull(originalPriceType.periods, originalPeriod);
        }

        this.mergePriceTypes(this.changeDate, false);

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private mergePriceTypes = _.debounce((date: Date, payrollGroupChanged: boolean) => {
        if (!date)
            date = CalendarUtility.getDateToday();
        this.priceTypes = [];

        // Loop through existing employment price types
        _.forEach(this.employmentPriceTypes, employmentPriceType => {
            let payrollGroupPriceType: PayrollGroupPriceTypeDTO = _.find(this.payrollGroupPriceTypes, p => p.payrollPriceTypeId === employmentPriceType.payrollPriceTypeId);
            if (payrollGroupPriceType) {
                // If employee has its own periods add it, it will be merged below
                if (employmentPriceType.periods && employmentPriceType.periods.length > 0 && payrollGroupPriceType.showOnEmployee) {
                    employmentPriceType.sort = payrollGroupPriceType.sort;
                    this.priceTypes.push(employmentPriceType);
                }
            } else if (employmentPriceType.periods && employmentPriceType.periods.length > 0) {
                if (payrollGroupChanged) {
                    // Price type does not exist in current payroll group
                    // If employee has its own periods "close" it by setting amount to zero
                    let period: EmploymentPriceTypePeriodDTO = new EmploymentPriceTypePeriodDTO();

                    if (this.employment.dateTo >= date) {
                        period.fromDate = date;
                        period.amount = 0;
                        employmentPriceType.isPayrollGroupPriceType = false;
                        employmentPriceType.payrollGroupAmount = 0;
                        employmentPriceType.periods.push(period);
                    }
                }
                this.priceTypes.push(employmentPriceType);
            }
        });

        // Merge employment price types with payroll group price types
        _.forEach(this.payrollGroupPriceTypes, payrollGroupPriceType => {
            let payrollGroupPeriod: PayrollGroupPriceTypePeriodDTO;
            if (payrollGroupPriceType.periods && payrollGroupPriceType.periods.length > 0) {
                payrollGroupPeriod = _.orderBy(_.filter(payrollGroupPriceType.periods, p => !p.fromDate || p.fromDate.isSameOrBeforeOnDay(date)), p => p.fromDate, 'desc')[0];
            }

            // If price type is already added from employment, only update the PayrollGroupAmount field
            let existing: EmploymentPriceTypeDTO = _.find(this.priceTypes, p => p.employmentPriceTypeId && p.employmentPriceTypeId !== 0 && p.payrollPriceTypeId === payrollGroupPriceType.payrollPriceTypeId);
            if (existing) {

                let existingPeriod: EmploymentPriceTypePeriodDTO;
                if (existing.periods && existing.periods.length > 0) {
                    existingPeriod = _.orderBy(_.filter(existing.periods, p => !p.fromDate || p.fromDate.isSameOrBeforeOnDay(date)), p => p.fromDate, 'desc')[0];
                }

                if (payrollGroupPriceType.priceTypeLevel && payrollGroupPriceType.priceTypeLevel.hasLevels && existingPeriod && existingPeriod.payrollLevelId && existingPeriod.payrollLevelId !== payrollGroupPriceType.payrollLevelId)
                    return; //Continue to next 

                existing.sort = payrollGroupPriceType.sort;
                existing.currentAmount = existingPeriod ? existingPeriod.amount : 0;
                existing.payrollGroupAmount = payrollGroupPeriod ? payrollGroupPeriod.amount : 0;

            } else if (payrollGroupPriceType.showOnEmployee) {

                if (payrollGroupPriceType.priceTypeLevel && payrollGroupPriceType.priceTypeLevel.hasLevels) {
                    let newlyAdded: EmploymentPriceTypeDTO = _.find(this.priceTypes, p => p.employmentPriceTypeId === 0 && p.payrollPriceTypeId === payrollGroupPriceType.payrollPriceTypeId);
                    if (newlyAdded) {
                        let newlyAddedExistingPeriod: EmploymentPriceTypePeriodDTO;
                        if (newlyAdded.periods && newlyAdded.periods.length > 0) {
                            newlyAddedExistingPeriod = _.orderBy(_.filter(newlyAdded.periods, p => !p.fromDate || p.fromDate.isSameOrBeforeOnDay(date)), p => p.fromDate, 'desc')[0];

                            if (newlyAddedExistingPeriod && newlyAddedExistingPeriod.payrollLevelId === payrollGroupPriceType.payrollLevelId) {
                                newlyAdded.payrollGroupAmount = payrollGroupPeriod ? payrollGroupPeriod.amount : 0;
                                newlyAdded.currentAmount = newlyAddedExistingPeriod ? newlyAddedExistingPeriod.amount : 0;
                            }
                        }
                        return;
                    }
                }

                let newType: EmploymentPriceTypeDTO = new EmploymentPriceTypeDTO();
                newType.employmentPriceTypeId = 0;
                newType.employmentId = 0;
                newType.payrollPriceTypeId = payrollGroupPriceType.payrollPriceTypeId;
                newType.code = payrollGroupPriceType.priceTypeCode;
                newType.name = payrollGroupPriceType.priceTypeName;
                newType.sort = payrollGroupPriceType.sort;
                newType.isPayrollGroupPriceType = true;

                if (payrollGroupPriceType.priceTypeLevel && payrollGroupPriceType.priceTypeLevel.hasLevels) {
                    newType.levelIsMandatory = payrollGroupPriceType.priceTypeLevel.levelIsMandatory;
                    newType.readOnly = false;
                    if (!payrollGroupPriceType.payrollLevelId || payrollGroupPriceType.payrollLevelId === 0)
                        newType.payrollGroupAmount = payrollGroupPeriod ? payrollGroupPeriod.amount : 0;
                }
                else {
                    newType.readOnly = payrollGroupPriceType.readOnlyOnEmployee;
                    newType.payrollGroupAmount = payrollGroupPeriod ? payrollGroupPeriod.amount : 0;
                }

                this.priceTypes.push(newType);
            }
        });
        this.priceTypes.sort((a, b) => a.sort - b.sort);
        _.forEach(this.priceTypes, priceType => {
            _.forEach(priceType.periods, period => {
                this.setPayrollLevelName(period);
            });
        });

        const wageTypeRowIndex = this.getSelectedPriceTypeRowIndex();
        this.selectedPriceType = this.priceTypes && this.priceTypes.length > 0 ? _.orderBy(this.priceTypes, p => p.sort)[wageTypeRowIndex] : null;
        this.validate();
        // GUI does not update when moving between employees
        this.$scope.$apply();
    }, 250, { leading: false, trailing: true });

    private getSelectedPriceTypeRowIndex(): number {
        var wageTypeRowIndex = 0;
        if (this.selectedPriceType != null && typeof (this.selectedPriceType) != "undefined") {
            if (typeof (this.selectedPriceType.periods) == "undefined") {
                const priceTypeObj = this.priceTypes.find(p => p.payrollPriceTypeId === this.selectedPriceType.payrollPriceTypeId);
                wageTypeRowIndex = typeof (priceTypeObj?.periods) != "undefined" ? _.findIndex(this.priceTypes, p => p.payrollPriceTypeId === this.selectedPriceType.payrollPriceTypeId) : 0;
            }
            else {
                wageTypeRowIndex = (typeof (this.selectedPriceType.periods) != "undefined" && this.selectedPriceType.periods.length != 0)
                    ? _.findIndex(this.priceTypes, p => p.sort === this.selectedPriceType.sort) : 0;
            }
        }
        return wageTypeRowIndex;
    }

    private getOriginalPriceType(addIfNotExists: boolean): EmploymentPriceTypeDTO {
        // Get selected price type from originally bound collection
        var priceType;
        if (this.selectedPriceType.employmentPriceTypeId)
            priceType = _.find(this.employmentPriceTypes, p => p.employmentPriceTypeId && p.employmentPriceTypeId === this.selectedPriceType.employmentPriceTypeId);
        if (!priceType)
            priceType = _.find(this.employmentPriceTypes, p => p.payrollPriceTypeId === this.selectedPriceType.payrollPriceTypeId);

        if (!priceType && addIfNotExists)
            priceType = this.addEmploymentPriceType();

        return priceType;
    }

    private addEmploymentPriceType(): EmploymentPriceTypeDTO {
        if (!this.selectedPriceType)
            return;

        var priceType: EmploymentPriceTypeDTO = new EmploymentPriceTypeDTO();
        priceType.employmentPriceTypeId = 0;
        priceType.employmentId = this.employment.employmentId;
        priceType.payrollPriceTypeId = this.selectedPriceType.payrollPriceTypeId;
        priceType.payrollGroupAmount = this.selectedPriceType.payrollGroupAmount;
        priceType.code = this.selectedPriceType.code;
        priceType.name = this.selectedPriceType.name;
        priceType.sort = this.selectedPriceType.sort;
        priceType.periods = [];
        if (!this.employmentPriceTypes)
            this.employmentPriceTypes = [];
        this.employmentPriceTypes.push(priceType);

        return priceType;
    }

    private getSelectableLevels(): PayrollLevelDTO[] {
        let levels: PayrollLevelDTO[] = [];
        if (this.usePayrollLevels() && this.selectedPriceType) {

            let priceType = _.find(this.payrollGroupPriceTypes, p => p.payrollPriceTypeId === this.selectedPriceType.payrollPriceTypeId);
            if (priceType && priceType.priceTypeLevel && priceType.priceTypeLevel.hasLevels) {
                _.forEach(priceType.priceTypeLevel.selectableLevelIds, (id) => {
                    let level = _.find(this.payrollLevels, p => p.payrollLevelId === id);
                    if (level && level.payrollLevelId != 0)
                        levels.push(level);
                })
            }
        }
        return levels;
    }

    private usePayrollLevels(): boolean {
        return _.size(this.payrollLevels) > 0
    }

    private setPayrollLevelName(period: EmploymentPriceTypePeriodDTO) {
        let payrollLevel = _.find(this.payrollLevels, p => p.payrollLevelId === period.payrollLevelId);
        if (payrollLevel)
            period.payrollLevelName = payrollLevel.name;
    }


    private validate() {
        this.isValid = true;
        if (this.usePayrollLevels()) {
            _.forEach(this.priceTypes, priceType => {
                if (priceType.levelIsMandatory) {
                    this.isValid = false;
                    return;
                }
            });
        }
    }
}