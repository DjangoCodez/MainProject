import { EmployeeTemplateEmploymentPriceTypeDTO } from "../../../../../../../Common/Models/EmployeeTemplateDTOs";
import { PayrollGroupPriceTypeDTO, PayrollGroupPriceTypePeriodDTO } from "../../../../../../../Common/Models/PayrollGroupDTOs";
import { PayrollLevelDTO } from "../../../../../../../Common/Models/PayrollLevelDTO";
import { ICoreService } from "../../../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../../../../Util/CalendarUtility";
import { IPayrollService } from "../../../../../../Payroll/PayrollService";

export class EmploymentPriceTypesFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/EmploymentPriceTypes/EmploymentPriceTypes.html'),
            scope: {
                model: '=',
                date: '=',
                payrollGroupId: '=',
                isEditMode: '=',
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

    // Init parameters
    private model: string;
    private date: Date;
    private payrollGroupId: number;
    private isEditMode: boolean;
    private onChange: Function;

    // Data
    private payrollGroupPriceTypes: PayrollGroupPriceTypeDTO[];
    private priceTypes: EmployeeTemplateEmploymentPriceTypeDTO[] = [];
    private payrollLevels: PayrollLevelDTO[];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private payrollService: IPayrollService,
        private coreService: ICoreService) {
    }

    $onInit() {
        this.$q.all([
            this.loadPayrollGroupPriceTypes(),
            this.loadPayrollLevels()
        ]).then(() => {
            this.setupPriceTypes();
            if (this.model) {
                this.setModel();
            }

            this.setupWatches();
        });
    }

    private setupWatches() {
        this.$scope.$watch(() => this.date, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.dateChanged();
        });
    }

    // SERVICE CALLS

    private loadPayrollGroupPriceTypes(): ng.IPromise<any> {
        return this.payrollService.getPayrollGroupPriceTypes(this.payrollGroupId || 0, true).then(x => {
            this.payrollGroupPriceTypes = x;
        });
    }

    private loadPayrollLevels(): ng.IPromise<any> {
        return this.payrollService.getPayrollLevels().then(x => {
            this.payrollLevels = x;
            const empty: PayrollLevelDTO = new PayrollLevelDTO();
            empty.payrollLevelId = 0;
            empty.name = empty.description = empty.nameAndDesc = '';
            this.payrollLevels.splice(0, 0, empty);
        });
    }

    // EVENTS

    private dateChanged() {
        this.updateAllPeriodAmounts();
    }

    private fromDateChanged(priceType: EmployeeTemplateEmploymentPriceTypeDTO) {
        this.$timeout(() => {
            this.updatePeriodAmount(priceType);
        });
    }

    private payrollLevelChanged(priceType: EmployeeTemplateEmploymentPriceTypeDTO) {
        this.$timeout(() => {
            this.updatePeriodAmount(priceType);
        });
    }

    private setDirty() {
        if (this.onChange) {
            this.$timeout(() => {
                this.onChange({ jsonString: this.getJsonFromModel() });
            });
        }
    }

    // HELP-METHODS

    private getDate(priceType: EmployeeTemplateEmploymentPriceTypeDTO): Date {
        if (priceType.fromDate)
            return priceType.fromDate;
        if (this.date)
            return this.date;

        return CalendarUtility.getDateToday();
    }

    private setupPriceTypes() {
        _.forEach(_.orderBy(this.payrollGroupPriceTypes, p => p.sort), payrollGroupPriceType => {
            let existingPriceType = this.priceTypes.find(t => t.payrollPriceTypeId === payrollGroupPriceType.payrollPriceTypeId);
            if (existingPriceType) {
                existingPriceType.hasPayrollLevels = true;
                existingPriceType.payrollLevelId = 0;
            } else {
                const priceType: EmployeeTemplateEmploymentPriceTypeDTO = new EmployeeTemplateEmploymentPriceTypeDTO();
                priceType.fromDate = this.date;
                priceType.payrollPriceTypeId = payrollGroupPriceType.payrollPriceTypeId;
                priceType.payrollPriceTypeName = payrollGroupPriceType.priceTypeName;
                priceType.payrollLevelId = payrollGroupPriceType.payrollLevelId;
                priceType.amount = priceType.payrollGroupAmount = this.getPeriodAmount(payrollGroupPriceType, this.getDate(priceType));
                priceType['levels'] = this.getPayrollLevelsForPriceType(priceType);
                this.priceTypes.push(priceType);
            }
        });
    }

    private getPeriodAmount(payrollGroupPriceType: PayrollGroupPriceTypeDTO, date: Date): number {
        let payrollGroupPeriod: PayrollGroupPriceTypePeriodDTO;
        if (payrollGroupPriceType && payrollGroupPriceType.periods && payrollGroupPriceType.periods.length > 0)
            payrollGroupPeriod = _.orderBy(_.filter(payrollGroupPriceType.periods, p => !p.fromDate || p.fromDate.isSameOrBeforeOnDay(date)), p => p.fromDate, 'desc')[0];

        return payrollGroupPeriod ? payrollGroupPeriod.amount : 0;
    }

    private updatePeriodAmount(priceType: EmployeeTemplateEmploymentPriceTypeDTO) {
        const payrollGroupPriceType = this.payrollGroupPriceTypes.find(p => p.payrollPriceTypeId === priceType.payrollPriceTypeId && ((priceType.payrollLevelId && p.payrollLevelId === priceType.payrollLevelId) || (!priceType.payrollLevelId && !p.payrollLevelId)));
        priceType.amount = priceType.payrollGroupAmount = this.getPeriodAmount(payrollGroupPriceType, this.getDate(priceType));
        this.setDirty();
    }

    private updateAllPeriodAmounts() {
        _.forEach(_.orderBy(this.payrollGroupPriceTypes, p => p.sort), payrollGroupPriceType => {
            let priceType = this.priceTypes.find(t => t.payrollPriceTypeId === payrollGroupPriceType.payrollPriceTypeId);
            if (priceType) {
                priceType.fromDate = this.date;
                priceType.amount = priceType.payrollGroupAmount = this.getPeriodAmount(payrollGroupPriceType, this.getDate(priceType));
            }
        });
    }

    private getPayrollLevelsForPriceType(priceType: EmployeeTemplateEmploymentPriceTypeDTO): PayrollLevelDTO[] {
        let levelIds = this.payrollGroupPriceTypes.filter(p => p.payrollPriceTypeId === priceType.payrollPriceTypeId).map(p => p.payrollLevelId);
        // Include the empty one
        levelIds.push(0);

        return this.payrollLevels.filter(l => _.includes(levelIds, l.payrollLevelId));
    }

    private getJsonFromModel(): string {
        this.priceTypes.forEach(p => {
            p.fromDateString = p.fromDate ? p.fromDate.toDateTimeString() : undefined;
        });
        return JSON.stringify(this.priceTypes);
    }

    private setModel() {
        this.priceTypes = JSON.parse(this.model).map(p => {
            const obj = new EmployeeTemplateEmploymentPriceTypeDTO();
            angular.extend(obj, p);
            obj.fixDates();
            return obj;
        });
    }
}
