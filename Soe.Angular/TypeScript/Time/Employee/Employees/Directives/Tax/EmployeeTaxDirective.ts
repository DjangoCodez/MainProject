import { IUrlHelperService, UrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { EmployeeTaxSEDTO, EmployeeUnionFeeDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup_EmployeeTaxAdjustmentType, TermGroup, TermGroup_EmployeeTaxSalaryDistressAmountType, TermGroup_EmployeeTaxEmploymentTaxType, TermGroup_EmployeeTaxType } from "../../../../../Util/CommonEnumerations";
import { CoreService } from "../../../../../Core/Services/CoreService";
import { TranslationService } from "../../../../../Core/Services/TranslationService";
import { NotificationService } from "../../../../../Core/Services/NotificationService";
import { EmployeeService } from "../../../EmployeeService";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { UnionFeeDialogController } from "./UnionFeeDialogController";
import { copy } from "angular";

export class EmployeeTaxDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/Tax/Views/EmployeeTax.html'),
            scope: {
                employeeId: '=',
                employeeTax: '=',
                unionFees: '=',
                readOnly: '=',
                enableCsrInquiry: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeTaxController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeTaxController {

    // Init parameters
    private employeeId: number;
    private unionFees: EmployeeUnionFeeDTO[];
    private readOnly: boolean;
    private enableCsrInquiry: boolean;

    // Events
    private onChange: Function;

    // Data
    private years: SmallGenericType[] = [];
    private employeeTax: EmployeeTaxSEDTO;
    private types: ISmallGenericType[];
    private adjustmentTypes: ISmallGenericType[];
    private sinkTypes: ISmallGenericType[];
    private salaryDistressAmountTypes: ISmallGenericType[];
    private employmentTaxTypes: ISmallGenericType[];
    private employmentAbroadCodes: ISmallGenericType[];
    private unionFeesDict: ISmallGenericType[];

    // Terms
    private terms: { [index: string]: string; };
    private taxRateLabel: string = '20-40';
    private percentLabel: string = "%";
    private get adjustmentValueLabel(): string {
        if (this.employeeTax && this.employeeTax.adjustmentType === TermGroup_EmployeeTaxAdjustmentType.PercentTax)
            return '%';
        else if (this.terms)
            return this.terms["common.amount"];

        return '';
    }
    private schoolYouthInfo: string;
    private csrInquiryText: string;

    // Properties
    private _year: number;
    private get year(): number {
        return this._year;
    }
    private set year(item: number) {
        this._year = item;
        this.loadEmployeeTax();
        if (item != null) {
            if (item < 2026)
                this.showFirstSecondEmployee = true;
            else
                this.showFirstSecondEmployee = false;
        }
    }

    private schoolYouthLimitUsed: number = 0;
    private schoolYouthLimitRemaining: number = 0;

    // Flags
    private showUnionFee: boolean = false;
    private showCopyFromLastYearWarning: boolean = false;
    private showFirstSecondEmployee: boolean = false;

    //@ngInject
    constructor(
        private $uibModal,
        private coreService: CoreService,
        private urlHelperService: UrlHelperService,
        private translationService: TranslationService,
        private notificationService: NotificationService,
        private employeeService: EmployeeService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadYears(true),
            this.loadTypes(),
            this.loadAdjustmentTypes(),
            this.loadSinkTypes(),
            this.loadSalaryDistressAmountTypes(),
            this.loadEmploymentTaxTypes(),
            this.loadEmploymentAbroadCodes(),
            this.loadUnionFeesDict()]).then(() => {
            });

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.employeeId, (oldVal, newVal) => {
            if (oldVal !== newVal) {
                this.loadYears(true);
            }
        });

        this.$scope.$on('reloadYears', (e, year) => {
            this.loadYears(false).then(() => {
                this.year = year;
            });
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.amount",
            "time.employee.tax.year.noinfo",
            "time.employee.tax.schoolyouth.info",
            "time.employee.tax.csrinquiry.done",
            "time.employee.tax.csrinquiry.notdone"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.schoolYouthInfo = this.terms["time.employee.tax.schoolyouth.info"];
        });
    }

    private loadYears(selectCurrentYear: boolean): ng.IPromise<any> {
        this.years = [];

        if (!this.employeeId)
            this.employeeId = 0;

        return this.employeeService.getEmployeeTaxYears(this.employeeId).then((x: number[]) => {

            // Add previous, current and next year if they don't exists
            let emptyYears: number[] = [];
            let currentYear: number = new Date().getFullYear();
            if (!_.includes(x, currentYear - 1)) {
                x.push(currentYear - 1);
                emptyYears.push(currentYear - 1);
            }
            if (!_.includes(x, currentYear)) {
                x.push(currentYear);
                emptyYears.push(currentYear);
            }
            if (!_.includes(x, currentYear + 1)) {
                x.push(currentYear + 1);
                emptyYears.push(currentYear + 1);
            }

            x = x.sort((a, b) => a - b);
            _.forEach(x, year => {
                let name: string = year.toString();
                if (_.includes(emptyYears, year))
                    name += " ({0})".format(this.terms["time.employee.tax.year.noinfo"]);
                this.years.push(new SmallGenericType(year, name));
            });

            // Select current year
            if (selectCurrentYear)
                this.year = currentYear;
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeTaxType, true, true).then(x => {
            this.types = x;
        });
    }

    private loadAdjustmentTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeTaxAdjustmentType, true, true).then(x => {
            this.adjustmentTypes = x;
        });
    }

    private loadSinkTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeTaxSinkType, true, true).then(x => {
            this.sinkTypes = x;
        });
    }

    private loadSalaryDistressAmountTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeTaxSalaryDistressAmountType, true, true).then(x => {
            this.salaryDistressAmountTypes = x;
        });
    }

    private loadEmploymentTaxTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeTaxEmploymentTaxType, true, true).then(x => {
            this.employmentTaxTypes = x;
            this.setDefaultEmploymentTaxType();
        });
    }

    private loadEmploymentAbroadCodes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeTaxEmploymentAbroadCode, true, true).then(x => {
            this.employmentAbroadCodes = x;
        });
    }

    private loadUnionFeesDict(): ng.IPromise<any> {
        this.unionFeesDict = [];
        return this.employeeService.getUnionFeesDict(false).then(x => {
            this.unionFeesDict = x;
            if (this.unionFeesDict.length > 0)
                this.showUnionFee = true;
        });
    }

    private loadEmployeeTax() {
        if (!this.year)
            return;

        if (!this.employeeId) {
            this.new(null);
        } else {
            this.employeeService.getEmployeeTaxByYear(this.employeeId, this.year).then(x => {
                this.employeeTax = x;

                if (!this.employeeTax) {
                    // Load previous year to copy info from
                    this.employeeService.getEmployeeTaxByYear(this.employeeId, this.year - 1).then(y => {
                        this.new(y);
                    });
                } else {
                    this.showCopyFromLastYearWarning = false;
                    this.setCsrInquiryText();
                    this.setDefaultEmploymentTaxType();
                    this.GetSchoolYouthLimitUsed();
                }
            });
        }
    }

    private GetSchoolYouthLimitUsed() {
        if (this.employeeTax && this.employeeTax.type == TermGroup_EmployeeTaxType.SchoolYouth) {
            let date: Date = new Date(this.employeeTax.year, 11, 31, 0, 0, 0, 0);
            this.employeeService.calculateSchoolYouthLimitUsed(this.employeeId, date).then(x => {
                this.schoolYouthLimitUsed = x;
                this.CalculateSchoolYouthLimitRemaining()
            });
        } else {
            this.schoolYouthLimitUsed = 0;
            this.schoolYouthLimitRemaining = 0;
        }
    }

    private CalculateSchoolYouthLimitRemaining() {
        if (this.employeeTax) {
            let date: Date = new Date(this.employeeTax.year, 11, 31, 0, 0, 0, 0);
            this.employeeService.calculateSchoolYouthLimitRemaining(this.employeeTax.schoolYouthLimitInitial ? this.employeeTax.schoolYouthLimitInitial : 0, this.schoolYouthLimitUsed, date).then(x => {
                this.schoolYouthLimitRemaining = x;
            });
        }
    }


    private new(copyFrom: EmployeeTaxSEDTO) {
        this.employeeTax = new EmployeeTaxSEDTO();
        this.employeeTax.employeeId = this.employeeId;
        this.employeeTax.year = this.year;

        if (copyFrom) {
            this.employeeTax.mainEmployer = copyFrom.mainEmployer;
            this.employeeTax.type = copyFrom.type;

            this.employeeTax.salaryDistressReservedAmount = copyFrom.salaryDistressReservedAmount;
            this.employeeTax.salaryDistressAmountType = copyFrom.salaryDistressAmountType;
            this.employeeTax.salaryDistressAmount = copyFrom.salaryDistressAmount;
            this.employeeTax.salaryDistressCase = copyFrom.salaryDistressCase;

            this.showCopyFromLastYearWarning = true;

            if (this.onChange)
                this.onChange();
        } else {
            this.employeeTax.mainEmployer = true;
        }

        this.setDefaultEmploymentTaxType();
        this.setCsrInquiryText();
        this.schoolYouthLimitUsed = 0;
        this.schoolYouthLimitRemaining = 0;
    }

    // EVENTS

    private csrInquiry() {
        this.employeeService.csrInquiry(this.employeeId, this.year).then(response => {
            if (response.errorMessage) {
                this.translationService.translate('time.employee.tax.csrinquiry.error.title').then(term => {
                    this.notificationService.showDialogEx(term, response.errorMessage, SOEMessageBoxImage.Error);
                });
            } else {
                let selectedYear = this.year;
                this.loadYears(false).then(() => {
                    this.year = selectedYear;
                    this.loadEmployeeTax();
                });
            }
        });
    }

    private distressAmountTypeChanged(item: any) {
        if (item != TermGroup_EmployeeTaxSalaryDistressAmountType.FixedAmount)
            this.employeeTax.salaryDistressAmount = 0;
    }

    // HELP-METHODS

    private setDefaultEmploymentTaxType() {
        if (this.employeeTax && !this.employeeTax.employmentTaxType && this.employeeTax.employmentTaxType !== 0)
            this.employeeTax.employmentTaxType = TermGroup_EmployeeTaxEmploymentTaxType.EmploymentTax;
    }

    private editUnionFee(unionFee: EmployeeUnionFeeDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/Tax/Views/UnionFeeDialog.html"),
            controller: UnionFeeDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                unionFees: () => { return this.unionFeesDict },
                unionFee: () => { return unionFee },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.unionFee) {
                if (!this.unionFees)
                    this.unionFees = [];

                if (!unionFee) {
                    // Add new union fee to the original collection
                    unionFee = new EmployeeUnionFeeDTO();
                    this.updateUnionFee(unionFee, result.unionFee);
                    this.unionFees.push(unionFee);
                } else {
                    // Update original union fee
                    var originalUnionFee = _.find(this.unionFees, u => u.employeeUnionFeeId === unionFee.employeeUnionFeeId);
                    if (originalUnionFee)
                        this.updateUnionFee(originalUnionFee, result.unionFee);
                }

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private updateUnionFee(unionFee: EmployeeUnionFeeDTO, input: EmployeeUnionFeeDTO) {
        let fee = _.find(this.unionFeesDict, u => u.id === input.unionFeeId);
        let feeName: string = fee ? fee.name : '';

        unionFee.unionFeeId = input.unionFeeId;
        unionFee.unionFeeName = feeName
        unionFee.fromDate = input.fromDate;
        unionFee.toDate = input.toDate;
    }

    private deleteUnionFee(unionFee: EmployeeUnionFeeDTO) {
        _.pull(this.unionFees, unionFee);

        if (this.onChange)
            this.onChange();
    }

    private setCsrInquiryText() {
        if (this.employeeTax && this.employeeTax.csrExportDate) {
            this.csrInquiryText = this.terms["time.employee.tax.csrinquiry.done"];
            if (this.employeeTax.csrImportDate)
                this.csrInquiryText += " " + this.employeeTax.csrImportDate.toFormattedDate();
        } else
            this.csrInquiryText = this.terms["time.employee.tax.csrinquiry.notdone"];
    }
}