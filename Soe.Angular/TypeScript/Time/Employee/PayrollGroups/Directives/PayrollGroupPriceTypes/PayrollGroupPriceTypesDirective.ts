import { PayrollGroupPriceTypeDTO, PayrollGroupPriceTypePeriodDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { PayrollLevelDTO } from "../../../../../Common/Models/PayrollLevelDTO";
import { PayrollPriceTypeDTO } from "../../../../../Common/Models/PayrollPriceTypeDTOs";
import { PayrollGroupPriceTypePeriodDialogController } from "./PayrollGroupPriceTypePeriodDialogController";
import { PayrollGroupPriceTypeDialogController } from "./PayrollGroupPriceTypeDialogController";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";


export class PayrollGroupPriceTypesDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/PayrollGroups/Directives/PayrollGroupPriceTypes/Views/PayrollGroupPriceTypes.html'),
            scope: {
                payrollGroupId: '=',
                priceTypes: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: PayrollGroupPriceTypesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollGroupPriceTypesController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private payrollGroupId: number;
    private priceTypes: PayrollGroupPriceTypeDTO[];
    private selectedPriceType: PayrollGroupPriceTypeDTO;
    private payrollPriceTypes: PayrollPriceTypeDTO[] = [];
    private payrollLevels: PayrollLevelDTO[] = [];

    // Flags
    private readOnly: boolean;
    private allPeriodsExpanded: boolean = false;
    private payrollLevelVisible: boolean = false;

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
        private coreService: ICoreService,
        private payrollService: IPayrollService) {        
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadPayrollPriceTypes(),
            this.loadPayrollLevels()
        ]).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {        
        this.$scope.$watchCollection(() => this.priceTypes, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.selectedPriceType = this.priceTypes.length ? this.priceTypes[0] : null;
            }
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadPayrollPriceTypes(): ng.IPromise<any> {
        return this.payrollService.getPayrollPriceTypes().then(x => {
            this.payrollPriceTypes = x;
        });
    }

    private loadPayrollLevels(): ng.IPromise<any> {
        return this.payrollService.getPayrollLevels().then(x => {            
            this.payrollLevels = x;            
            if (_.size(this.payrollLevels) > 0) {
                this.payrollLevelVisible = true;
                let empty: PayrollLevelDTO = new PayrollLevelDTO();
                empty.payrollLevelId = 0;
                empty.name = "";
                empty.description = "";
                this.payrollLevels.push(empty);
            }
        });
    }

    // EVENTS

    private sortFirst() {
        if (this.selectedPriceType && this.selectedPriceType.sort > 1) {
            // Move row to the top
            this.selectedPriceType.sort = -1;

            this.reNumberRows();
        }
    }

    private sortPrev() {
        if (this.selectedPriceType && this.selectedPriceType.sort > 1) {
            // Get previous row
            var prevRow = _.find(this.priceTypes, p => p.sort === this.selectedPriceType.sort - 1);
            // Move row up
            if (prevRow) {
                this.multiplyRowNr();
                // Move current row before previous row
                this.selectedPriceType.sort -= 19;

                this.reNumberRows();
            }
        }
    }

    private sortNext() {
        if (this.selectedPriceType && this.selectedPriceType.sort < this.priceTypes.length) {
            // Get next row
            var nextRow = _.head(_.sortBy(_.filter(this.priceTypes, p => p.sort > this.selectedPriceType.sort), 'sort'));
            // Move row down
            if (nextRow) {
                this.multiplyRowNr();
                // Move current row after next row                    
                this.selectedPriceType.sort = nextRow.sort + 5;

                this.reNumberRows();
            }
        }
    }

    private sortLast() {
        if (this.selectedPriceType && this.selectedPriceType.sort < this.priceTypes.length) {
            // Move row to the bottom
            this.selectedPriceType.sort = _.max(_.map(this.priceTypes, p => p.sort)) + 2;

            this.reNumberRows();
        }
    }

    private reNumberRows() {
        var i: number = 1;

        _.forEach(_.orderBy(this.priceTypes, 'sort'), p => {
            p.sort = i++;
        });

        this.setAsDirty();
    }

    private multiplyRowNr() {
        _.forEach(this.priceTypes, p => {
            p.sort *= 10;
        });
    }

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

    private editPriceType(priceType: PayrollGroupPriceTypeDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/PayrollGroups/Directives/PayrollGroupPriceTypes/Views/PayrollGroupPriceTypeDialog.html"),
            controller: PayrollGroupPriceTypeDialogController,
            controllerAs: "ctrl",
            size: 'sm',
            resolve: {
                payrollLevelVisible: () => { return this.payrollLevelVisible },
                priceType: () => { return priceType },
                payrollPriceTypes: () => { return this.getAvailablePayrollPriceTypes(priceType) },
                payrollLevels: () => { return this.getAvailablePayrollLevels() },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.priceType) {

                //OBS:TODO check if combination pricetype and level is valid
                
                if (!priceType) {
                    // Add new priceType to the original collection
                    priceType = new PayrollGroupPriceTypeDTO();
                    priceType.periods = [];
                    priceType.sort = _.max(_.map(this.priceTypes, p => p.sort)) + 1;
                    this.priceTypes.push(priceType);
                    this.$timeout(() => {
                        this.selectedPriceType = priceType;
                    });
                }
                
                priceType.payrollPriceTypeId = result.priceType.payrollPriceTypeId;
                priceType.payrollLevelId = result.priceType.payrollLevelId;
                priceType.showOnEmployee = result.priceType.showOnEmployee;
                priceType.readOnlyOnEmployee = result.priceType.readOnlyOnEmployee;

                // Set code and name
                let payrollPriceType = _.find(this.payrollPriceTypes, p => p.payrollPriceTypeId === priceType.payrollPriceTypeId);
                if (payrollPriceType) {
                    priceType.priceTypeCode = payrollPriceType.code;
                    priceType.priceTypeName = payrollPriceType.name;
                }
                let payrollLevel = _.find(this.payrollLevels, p => p.payrollLevelId === priceType.payrollLevelId);
                if (payrollLevel) {
                    priceType.payrollLevelName = payrollLevel.name;                    
                }

                this.setAsDirty();
            }
        });
    }

    private deletePriceType(priceType: PayrollGroupPriceTypeDTO) {
        _.pull(this.priceTypes, priceType);

        this.reNumberRows();
    }

    private editPeriod(period: PayrollGroupPriceTypePeriodDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/PayrollGroups/Directives/PayrollGroupPriceTypes/Views/PayrollGroupPriceTypePeriodDialog.html"),
            controller: PayrollGroupPriceTypePeriodDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                period: () => { return period },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.period && this.selectedPriceType) {
                if (!period) {
                    // Add new period to the original collection
                    period = new PayrollGroupPriceTypePeriodDTO();
                    if (!this.selectedPriceType.periods)
                        this.selectedPriceType.periods = [];
                    this.selectedPriceType.periods.push(period);
                    this.selectedPriceType['expanded'] = true;
                }

                period.fromDate = result.period.fromDate;
                period.amount = result.period.amount;

                this.setAsDirty();
            }
        });
    }

    private deletePeriod(period: PayrollGroupPriceTypePeriodDTO) {
        if (this.selectedPriceType && this.selectedPriceType.periods)
            _.pull(this.selectedPriceType.periods, period);

        this.setAsDirty();
    }

    // HELP-METHIDS

    private isOkToAddPayrollGroupPriceType(payrollGroupPriceTypeDTO: PayrollGroupPriceTypeDTO): boolean {
        if (!payrollGroupPriceTypeDTO)
            return false;
        if (!this.payrollLevelVisible)
            return true;

        //Todo
    }
  
    private getAvailablePayrollPriceTypes(payrollGroupPriceType: PayrollGroupPriceTypeDTO): PayrollPriceTypeDTO[] {
        let priceTypes: PayrollPriceTypeDTO[] = [];

        if (this.payrollLevelVisible) {                       
            _.forEach(this.payrollPriceTypes, ppt => {
                let payrollGroupPriceTypesForCurrentPriceType = _.filter(this.priceTypes, p => p.payrollPriceTypeId === ppt.payrollPriceTypeId && (!p.payrollLevelId || _.includes(_.map(this.payrollLevels, pl => pl.payrollLevelId), p.payrollLevelId)));                                
                if (_.size(payrollGroupPriceTypesForCurrentPriceType) < _.size(this.payrollLevels) )
                    priceTypes.push(ppt);                
            }); 

        } else {
            priceTypes = _.filter(this.payrollPriceTypes, p => !_.includes(_.map(this.priceTypes, pt => pt.payrollPriceTypeId), p.payrollPriceTypeId));                        
        }    

        if (payrollGroupPriceType) {
            let payrollPriceType = _.find(this.payrollPriceTypes, p => p.payrollPriceTypeId === payrollGroupPriceType.payrollPriceTypeId);
            if (payrollPriceType) {
                priceTypes.push(payrollPriceType);
            }
        }

        return priceTypes;
    }
    private getAvailablePayrollLevels(): PayrollLevelDTO[] {        
        return this.payrollLevels;
    }
    private setAsDirty() {
        if (this.onChange)
            this.onChange();
    }
}