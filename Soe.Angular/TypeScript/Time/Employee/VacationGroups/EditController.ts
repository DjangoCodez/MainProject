import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISmallGenericType, IVacationGroupDTO, IVacationGroupSEDayTypeDTO } from "../../../Scripts/TypeLite.Net4";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { IEmployeeService } from "../EmployeeService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { VacationGroupDTO, VacationGroupSEDTO } from "../../../Common/Models/VacationGroupDTO";
import { Feature, TermGroup, TermGroup_SysPayrollPrice, TermGroup_VacationGroupType, TermGroup_VacationGroupCalculationType, TermGroup_VacationGroupRemainingDaysRule, TermGroup_VacationGroupVacationSalaryPayoutRule, TermGroup_VacationGroupVacationHandleRule, TermGroup_VacationGroupVacationDaysHandleRule, TermGroup_VacationGroupVacationAbsenceCalculationRule, TermGroup_VacationGroupYearEndRemainingDaysRule, SoeVacationGroupDayType } from "../../../Util/CommonEnumerations";
import { ITimeService } from "../../Time/timeservice";
import { AccountSmallDTO } from "../../../Common/Models/AccountDTO";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { IFocusService } from "../../../Core/Services/focusservice";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    vacationGroupTypes: ISmallGenericType[];
    dayRangesWithManualFromDate: ISmallGenericType[];
    dayRanges: ISmallGenericType[];
    allCalculationTypes: ISmallGenericType[];
    calculationTypes: ISmallGenericType[];
    timeDeviationCauses: ISmallGenericType[];
    vacationHandleRules: ISmallGenericType[];
    vacationDaysHandleRules: ISmallGenericType[];
    allVacationDaysHandleRules: ISmallGenericType[];
    remainingDaysRules: ISmallGenericType[];
    allRemainingDaysRules: ISmallGenericType[];
    guaranteeAmountMaxNbrOfDaysRules: ISmallGenericType[];
    absenceCalculationRules: ISmallGenericType[];
    allAbsenceCalculationRules: ISmallGenericType[];
    salaryPayoutRules: ISmallGenericType[];
    variablePayoutRules: ISmallGenericType[];
    yearEndRemainingDaysRules: ISmallGenericType[];
    yearEndOverdueDaysRules: ISmallGenericType[];
    yearEndVacationVariableRules: ISmallGenericType[];
    nbrOfValidAdditionalVacationDays: ISmallGenericType[];
    monthNames: ISmallGenericType[];
    payrollPriceTypes: ISmallGenericType[];
    payrollPriceFormulas: ISmallGenericType[];
    dayTypes: ISmallGenericType[] = [];
    selectedDayTypes: any[] = [];
    sysPayrollPriceVacationDayPercent: number;
    sysPayrollPriceVacationDayAdditionPercent: number;
    handelsGuaranteeAmountHours: number;
    selectedVacationGroupId: number = 0;
    vacationSalaryPayoutRuleSelectorLabel: any;
    selectedFromDateId: number = 0;
    selectedFromDate: Date = null;
    selectedEarningYearAmountFromDateId: number = 0;
    selectedEarningYearAmountFromDate: Date = null;
    latestVacationYearEnd: string = '';
    useEarningYearVariableAmountFromDate: boolean = false;
    selectedEarningYearVariableAmountDateId: number = 0;
    selectedRemainingDaysPayoutMonthId: number = 0;
    accountStds: AccountSmallDTO[];

    //Visibility fields
    useAdditionalVacationDaysVisibility: boolean = false;
    additionalVacationDays1Visibility: boolean = false;
    additionalVacationDays2Visibility: boolean = false;
    additionalVacationDays3Visibility: boolean = false;
    vacationHandleRuleVisibility: boolean = false;
    vacationDaysHandleRuleVisibility: boolean = false;
    vacationDaysGrossUseFiveDaysPerWeekVisibility: boolean = false;
    remainingDaysRuleVisibility: boolean = false;
    useMaxRemainingDaysVisibility: boolean = false;
    remainingDaysPayoutMonthVisibility: boolean = false;
    remainingDaysRectangleVisibility: boolean = false;
    earningYearAmountFromDateVisibility: boolean = false;
    earningYearVariableAmountFromDateVisibility: boolean = false;
    monthlySalaryFormulaVisibility: boolean = false;
    hourlySalaryFormulaVisibility: boolean = false;
    vacationDayPercentVisibility: boolean = false;
    vacationDayAdditionPercentVisibility: boolean = false;
    vacationVariablePercentVisibility: boolean = false;
    vacationDayPercentPriceTypeVisibility: boolean = false;
    vacationDayAdditionPercentPriceTypeVisibility: boolean = false;
    vacationVariablePercentPriceTypeVisibility: boolean = false;
    useGuaranteeAmountVisibility: boolean = false;
    vacationAbsenceCalculationRuleVisibility: boolean = false;
    vacationSalaryPayoutRuleVisibility: boolean = false;
    vacationSalaryPayoutDaysVisibility: boolean = false;
    vacationSalaryPayoutMonthVisibility: boolean = false;
    vacationSalaryPayoutRectangleVisibility: boolean = false;
    vacationVariablePayoutRuleVisibility: boolean = false;
    vacationVariablePayoutDaysVisibility: boolean = false;
    vacationVariablePayoutMonthVisibility: boolean = false;
    vacationVariablePayoutRectangleVisibility: boolean = false;
    showAccordions: boolean = true;

    //Readonly fields
    vacationDayPercentReadOnly: boolean = true;
    remainingDaysRuleReadOnly: boolean = true;
    vacationSalaryPayoutRuleReadOnly: boolean = true;
    vacationVariablePayoutRuleReadOnly: boolean = true;
    vacationDayAdditionPercentReadOnly: boolean = true;
    vacationVariablePercentReadOnly: boolean = true;

    // Data
    vacationGroup: IVacationGroupDTO;
    vacationGroupId: number;
    //Terms
    terms: { [index: string]: string; };

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        private timeService: ITimeService,
        private translationService: ITranslationService,
        private focusService: IFocusService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }


    public onInit(parameters: any) {
        this.vacationGroupId = parameters.id;
        if (this.vacationGroupId > 0) {
            this.isNew = false
            this.selectedVacationGroupId = this.vacationGroupId
        }
        else {
            this.isNew = true
        }

        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Employee_VacationGroups, loadReadPermissions: true, loadModifyPermissions: true }]);
        this.navigatorRecords = parameters.navigatorRecords;
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_VacationGroups].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_VacationGroups].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.vacationGroupId, recordId => {
            if (recordId !== this.vacationGroupId) {
                this.vacationGroupId = recordId;
                this.selectedVacationGroupId = recordId;
                this.onLoadData();
            }
        });
    }

    // LOOKUPS

    protected doLookups(): ng.IPromise<any> {
        this.setupMonths();
        this.setupMonthNames();
        this.setupNbrOfValidAdditionalVacationDays();
        return this.progress.startLoadingProgress([() => {
            return this.$q.all([
                this.loadTerms(),
                this.loadSelectionTypes(),
                this.loadCalculationTypes(),
                this.loadTimeDeviationCauses(),
                this.loadVacationHandleRules(),
                this.loadVacationDaysHandleRules(),
                this.loadRemainingDaysRules(),
                this.loadGuaranteeAmountMaxNbrOfDaysRules(),
                this.loadAbsenceCalculationRules(),
                this.loadAbsenceSalaryPayoutRules(),
                this.loadYearEndOverdueDaysRules(),
                this.loadYearEndRemainingDaysRules(),
                this.loadYearEndVacationVariableRules(),
                this.loadPayrollPriceTypes(),
                this.loadPayrollPriceFormulas(),
                this.loadSysPayrollPriceVacationDayPercent(),
                this.loadSysPayrollPriceVacationDayAdditionPercent(),
                this.loadSysPayrollPriceHandelsGuaranteeAmount(),
                this.loadAccountStds(),
                this.loadDayTypes()
            ]);
        }]);
    }

    private onLoadData() {
        if (this.vacationGroupId > 0) {
            return this.progress.startLoadingProgress([
                () => this.loadVacationGroup()
            ]);
        } else {
            this.new();
        }

    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.employee.vacationgroup.salarypayoutruleselectorlabel1",
            "time.employee.vacationgroup.salarypayoutruleselectorlabel2",
            "common.name",
            "core.edit",
            "core.yes",
            "core.no"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadVacationGroup(): ng.IPromise<any> {
        if (this.vacationGroupId > 0) {
            return this.employeeService.getVacationGroup(this.selectedVacationGroupId).then(x => {
                this.vacationGroup = x;
                
                if (this.vacationGroup.latesVacationYearEnd)
                    this.latestVacationYearEnd = CalendarUtility.toFormattedDate(this.vacationGroup.latesVacationYearEnd);

                this.filterCalculationTypesByVacationGroupType();
                this.filterVacationDaysHandleRulesByVacationGroupType();
                this.filterVacationAbsenceCalculationRulesByVacationGroupType();

                if (this.vacationGroup.fromDate) {
                    if (this.vacationGroup.fromDate.getDate() === 1) {
                        this.selectedFromDateId = this.vacationGroup.fromDate.getMonth() + 1;
                    } else {
                        this.selectedFromDateId = 0;
                        this.selectedFromDate = new Date(CalendarUtility.getDateToday().getFullYear(), this.vacationGroup.fromDate.getMonth(), this.vacationGroup.fromDate.getDate());
                    }
                }
                if (this.vacationGroup.vacationGroupSE == null) {
                    this.vacationGroup.vacationGroupSE = new VacationGroupSEDTO();
                    this.vacationGroup.vacationGroupSE.vacationGroupId = this.selectedVacationGroupId;
                }
                if (this.vacationGroup.vacationGroupSE.earningYearVariableAmountFromDate != null) {
                    let fromDate: Date = new Date(this.vacationGroup.vacationGroupSE.earningYearVariableAmountFromDate.toString());
                    this.selectedEarningYearVariableAmountDateId = fromDate.getMonth() + 1;
                    this.useEarningYearVariableAmountFromDate = true;
                }
                if (this.vacationGroup.vacationGroupSE.earningYearAmountFromDate) {
                    if (this.vacationGroup.vacationGroupSE.earningYearAmountFromDate.getDate() === 1) {
                        this.selectedEarningYearAmountFromDateId = this.vacationGroup.vacationGroupSE.earningYearAmountFromDate.getMonth() + 1;
                    } else {
                        this.selectedEarningYearAmountFromDateId = 0;
                        this.selectedEarningYearAmountFromDate = new Date(CalendarUtility.getDateToday().getFullYear(), this.vacationGroup.vacationGroupSE.earningYearAmountFromDate.getMonth(), this.vacationGroup.vacationGroupSE.earningYearAmountFromDate.getDate());
                    }
                }
                if (this.vacationGroup.vacationGroupSE.remainingDaysPayoutMonth == null)
                    this.selectedRemainingDaysPayoutMonthId = 1
                else
                    this.selectedRemainingDaysPayoutMonthId = this.vacationGroup.vacationGroupSE.remainingDaysPayoutMonth;
                
                if (this.vacationGroup.vacationGroupSE.replacementTimeDeviationCauseId == null)
                    this.vacationGroup.vacationGroupSE.replacementTimeDeviationCauseId = 0;

                if (this.vacationGroup.vacationGroupSE.vacationGroupSEDayTypes) {
                    this.selectedDayTypes = [];
                    this.vacationGroup.vacationGroupSE.vacationGroupSEDayTypes.forEach(w => {
                        this.selectedDayTypes.push({ id: w.dayTypeId });
                    });
                }
                this.setFieldVisibility();
                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.vacationGroup.name);
            });
        } else {
            this.new();
        }
    }

    private loadAccountStds(): ng.IPromise<any> {
        this.accountStds = [];
        return this.timeService.getAccountsSmall(0, 0, true).then(x => {
            this.accountStds = x;
        });
    }

    private loadDayTypes(): ng.IPromise<any> {
        return this.employeeService.getDayTypesDict(false).then(x => {
            this.dayTypes = x;
        });
    }
    private loadSelectionTypes(): ng.IPromise<any> {
        this.vacationGroupTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupType, false, true).then(x => {
            this.vacationGroupTypes = x;
        });
    }

    private loadCalculationTypes(): ng.IPromise<any> {
        this.calculationTypes = [];
        this.allCalculationTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupCalculationType, false, true).then(x => {
            this.allCalculationTypes = x;
            this.calculationTypes = x;
        });
    }

    private loadTimeDeviationCauses(): ng.IPromise<any> {
        this.timeDeviationCauses = [];
        return this.employeeService.getTimeDeviationCausesGrid().then(x => {
            this.timeDeviationCauses = x;
        });
    }

    private loadVacationHandleRules(): ng.IPromise<any> {
        this.vacationHandleRules = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupVacationHandleRule, false, true).then(x => {
            this.vacationHandleRules = x;
        });
    }

    private loadVacationDaysHandleRules(): ng.IPromise<any> {
        this.vacationDaysHandleRules = [];
        this.allVacationDaysHandleRules = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupVacationDaysHandleRule, false, true).then(x => {
            this.vacationDaysHandleRules = x;
            this.allVacationDaysHandleRules = x;
        });
    }

    private loadRemainingDaysRules(): ng.IPromise<any> {
        this.remainingDaysRules = [];
        this.allRemainingDaysRules = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupRemainingDaysRule, false, true).then(x => {
            this.remainingDaysRules = x;
            this.allRemainingDaysRules = x;
        });
    }

    private loadGuaranteeAmountMaxNbrOfDaysRules(): ng.IPromise<any> {
        this.guaranteeAmountMaxNbrOfDaysRules = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupGuaranteeAmountMaxNbrOfDaysRule, false, true).then(x => {
            this.guaranteeAmountMaxNbrOfDaysRules = x;
        });
    }

    private loadAbsenceCalculationRules(): ng.IPromise<any> {
        this.absenceCalculationRules = [];
        this.allAbsenceCalculationRules = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupVacationAbsenceCalculationRule, false, true).then(x => {
            this.absenceCalculationRules = x;
            this.allAbsenceCalculationRules = x;
        });
    }

    private loadAbsenceSalaryPayoutRules(): ng.IPromise<any> {
        this.salaryPayoutRules = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupVacationSalaryPayoutRule, false, true).then(x => {
            this.salaryPayoutRules = x;
            this.variablePayoutRules = x;
        });
    }

    private loadYearEndRemainingDaysRules(): ng.IPromise<any> {
        this.yearEndRemainingDaysRules = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupYearEndRemainingDaysRule, false, true).then(x => {
            this.yearEndRemainingDaysRules = x;
        });
    }

    private loadYearEndOverdueDaysRules(): ng.IPromise<any> {
        this.yearEndOverdueDaysRules = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupYearEndOverdueDaysRule, false, true).then(x => {
            this.yearEndOverdueDaysRules = x;
        });
    }

    private loadYearEndVacationVariableRules(): ng.IPromise<any> {
        this.yearEndVacationVariableRules = [];
        return this.coreService.getTermGroupContent(TermGroup.VacationGroupYearEndVacationVariableRule, false, true).then(x => {
            this.yearEndVacationVariableRules = x;
        });
    }

    private loadPayrollPriceTypes(): ng.IPromise<any> {
        this.payrollPriceTypes = [];
        return this.employeeService.getPayrollPriceTypesDict(true).then(x => {
            this.payrollPriceTypes = x;
        });
    }

    private loadPayrollPriceFormulas() {
        this.payrollPriceFormulas = [];
        return this.employeeService.getPayrollPriceFormulasDict(false).then(x => {
            this.payrollPriceFormulas = x;
        });
    }

    private loadSysPayrollPriceVacationDayPercent() {
        this.sysPayrollPriceVacationDayPercent = 0;
        return this.employeeService.getSysPayrollPriceAmount(TermGroup_SysPayrollPrice.SE_Vacation_VacationDayPercent, null).then(x => {
            this.sysPayrollPriceVacationDayPercent = x * 100;

        });
    }

    private loadSysPayrollPriceVacationDayAdditionPercent() {
        this.sysPayrollPriceVacationDayAdditionPercent = 0;
        return this.employeeService.getSysPayrollPriceAmount(TermGroup_SysPayrollPrice.SE_Vacation_VacationDayAdditionPercent, null).then(x => {
            this.sysPayrollPriceVacationDayAdditionPercent = x * 100;

        });
    }

    private loadSysPayrollPriceHandelsGuaranteeAmount() {
        this.handelsGuaranteeAmountHours = 0;
        return this.employeeService.getSysPayrollPriceAmount(TermGroup_SysPayrollPrice.SE_Vacation_HandelsGuaranteeAmount, null).then(x => {
            this.handelsGuaranteeAmountHours = x;
        });
    }

    private setupNbrOfValidAdditionalVacationDays() {
        this.nbrOfValidAdditionalVacationDays = [];

        for (var i = 1; i <= 3; i++) {
            this.nbrOfValidAdditionalVacationDays.push({ id: i, name: i.toString() });
        }
    }

    private setupMonths() {
        this.dayRangesWithManualFromDate = [];
        this.dayRanges = [];
        this.translationService.translate("time.employee.vacationgroup.manualfromdate").then((term) => {
            this.dayRangesWithManualFromDate.push({ id: 0, name: term });
            for (var i = 1; i <= 12; i++) {
                var days = new Date(1900, i - 1, 0).getDate();
                var fromMonthName = new Date(1900, i - 1, 0).toLocaleString(CoreUtility.language, { month: "long" });
                var toMonthName = new Date(1900, i, 0).toLocaleString(CoreUtility.language, { month: "long" });
                var displayValue = "1 " + toMonthName + " - " + days + " " + fromMonthName;
                this.dayRangesWithManualFromDate.push({ id: i, name: displayValue });
                this.dayRanges.push({ id: i, name: displayValue });
            }
        });
    }

    private setupMonthNames() {
        this.monthNames = [];
        for (var i = 1; i <= 12; i++) {
            var monthName = new Date(1900, i, 0).toLocaleString(CoreUtility.language, { month: "long" });
            this.monthNames.push({ id: i, name: monthName });
        }
    }

    private typeChanged(item: any) {
        this.vacationGroup.type = item;
        this.setFieldVisibility();
        this.filterCalculationTypesByVacationGroupType();
        this.filterVacationAbsenceCalculationRulesByVacationGroupType();
        this.vacationGroup.vacationDaysPaidByLaw = 0;
        var today = new Date();
        if (item == TermGroup_VacationGroupType.EarningYearIsVacationYear) {
            this.selectedFromDateId = 1;
            this.vacationGroup.fromDate = new Date(1900, 1, 0);
            this.vacationGroup.realDateFrom = new Date(today.getFullYear(), 1, 1);
            this.vacationGroup.realDateFrom = this.vacationGroup.realDateFrom.addYears(1).addDays(-1);
            if (this.isNew) {
                this.vacationGroup.vacationDaysPaidByLaw = 25;
                this.vacationGroup.vacationGroupSE.calculationType = 21;
                this.vacationGroup.vacationGroupSE.vacationHandleRule = 1;
                this.vacationGroup.vacationGroupSE.vacationDaysHandleRule = 1;
                this.vacationGroup.vacationGroupSE.remainingDaysRule = 1;
                this.selectedRemainingDaysPayoutMonthId = 2;
                this.selectedEarningYearAmountFromDateId = 2;
                this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule = 1;
                this.vacationGroup.vacationGroupSE.yearEndRemainingDaysRule = 3;
                this.setFieldVisibility();
                this.filterRemainingDaysRulesByCalculationType();
                this.filterVacationDaysHandleRulesByVacationGroupType();
            }
        } else if (item == TermGroup_VacationGroupType.EarningYearIsBeforeVacationYear) {
            this.selectedFromDateId = 4;
            this.vacationGroup.fromDate = new Date(1900, 4, 0);
            this.vacationGroup.realDateFrom = new Date(today.getFullYear(), 4, 1);
            this.vacationGroup.realDateFrom = this.vacationGroup.realDateFrom.addYears(1).addDays(-1);
            if (this.isNew) {
                this.vacationGroup.vacationGroupSE.vacationHandleRule = 1;
                this.vacationGroup.vacationDaysPaidByLaw = 25;
                this.vacationGroup.vacationGroupSE.calculationType = 11;

                this.selectedRemainingDaysPayoutMonthId = 5;
                this.selectedEarningYearAmountFromDateId = 5;
                this.vacationGroup.vacationGroupSE.yearEndRemainingDaysRule = 3;
                this.setFieldVisibility();
                this.filterRemainingDaysRulesByCalculationType();
                this.filterVacationDaysHandleRulesByVacationGroupType();
            }
        } else if (this.vacationGroup.fromDate == null) {
            this.selectedFromDateId = 1;
            this.vacationGroup.fromDate = new Date(1900, 1, 0);
            this.vacationGroup.realDateFrom = new Date(today.getFullYear(), 1, 1);
            this.vacationGroup.realDateFrom = this.vacationGroup.realDateFrom.addYears(1).addDays(-1);
        }
    }

    private calculationTypeChanged(item: any) {
        this.$timeout(() => {
            this.vacationGroup.vacationGroupSE.calculationType = item;
            this.setFieldVisibility();
            this.filterRemainingDaysRulesByCalculationType();
            this.filterVacationDaysHandleRulesByVacationGroupType();
        });
    }

    private vacationHandleRuleChanged(item: any) {
        this.$timeout(() => {
            this.setFieldVisibility();
        });
    }

    private remaingDaysRuleChanged(item: any) {
        this.$timeout(() => {
            this.setFieldVisibility();
        });
    }

    private useAdditionalDaysChanged(item: any) {
        this.$timeout(() => {
            this.additionalVacationDays1Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && (this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 1);
            this.additionalVacationDays2Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && (this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 2);
            this.additionalVacationDays3Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && (this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 3);
        });
    }

    private useGrossDays(item: any) {
        this.$timeout(() => {
            this.setFieldVisibility();
        });
    }

    private useMaxRemainingDays(item: any) {
        this.$timeout(() => {
            this.setFieldVisibility();
        });
    }

    private useEarningYearVariableAmountFromDateChanging(item: any) {
        this.$timeout(() => {
            this.setFieldVisibility();
        });
    }

    private useGuaranteeAmountFromDateChanging(item: any) {
        this.$timeout(() => {
            this.setFieldVisibility();
        });
    }

    private useOwnGuaranteeAmountChanged() {
        this.$timeout(() => {
            if (this.vacationGroup.vacationGroupSE.useOwnGuaranteeAmount) {
                this.focusService.focusByName("ctrl_vacationGroup_vacationGroupSE_ownGuaranteeAmount");
            } else
                this.vacationGroup.vacationGroupSE.ownGuaranteeAmount = null;
        });
    }

    private salaryPayoutRulesChanging(item: any) {
        this.$timeout(() => {
            this.setFieldVisibility();
        });
    }

    private additionalDaysChanged(item: any) {
        this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays = item;
        this.additionalVacationDays1Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && (this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 1);
        this.additionalVacationDays2Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && (this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 2);
        this.additionalVacationDays3Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && (this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 3);
    }

    private showUsefilluptovacationdayspaidbylawrule(): boolean {

        return this.vacationGroup?.vacationGroupSE?.remainingDaysRule == TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw &&
            (this.vacationGroup?.vacationGroupSE?.yearEndRemainingDaysRule == TermGroup_VacationGroupYearEndRemainingDaysRule.Over20DaysSaved || this.vacationGroup?.vacationGroupSE?.yearEndRemainingDaysRule == TermGroup_VacationGroupYearEndRemainingDaysRule.Saved);
    }

    private monthChanged(item: any) {

    }

    // ACTIONS

    private save() {
        this.emtyFieldsBeforeSave();
        if (this.selectedFromDateId === 0) {
            if (this.selectedFromDate)
                this.vacationGroup.fromDate = new Date(1900, this.selectedFromDate.getMonth(), this.selectedFromDate.getDate());
        }
        else
            this.vacationGroup.fromDate = new Date(1900, this.selectedFromDateId - 1, 1);

        if (this.selectedEarningYearAmountFromDateId === 0) {
            if (this.selectedEarningYearAmountFromDate)
                this.vacationGroup.vacationGroupSE.earningYearAmountFromDate = new Date(1900, this.selectedEarningYearAmountFromDate.getMonth(), this.selectedEarningYearAmountFromDate.getDate());
            else
                this.vacationGroup.vacationGroupSE.earningYearAmountFromDate = new Date(1900, 1, 1);
        }
        else
            this.vacationGroup.vacationGroupSE.earningYearAmountFromDate = new Date(1900, this.selectedEarningYearAmountFromDateId - 1, 1);

        if (!this.showUsefilluptovacationdayspaidbylawrule && this.vacationGroup?.vacationGroupSE != null)
            this.vacationGroup.vacationGroupSE.useFillUpToVacationDaysPaidByLawRule = false;

        this.vacationGroup.vacationGroupSE.earningYearVariableAmountFromDate = new Date(1900, this.selectedEarningYearVariableAmountDateId - 1, 1);
        this.vacationGroup.vacationGroupSE.remainingDaysPayoutMonth = this.selectedRemainingDaysPayoutMonthId;

        this.progress.startSaveProgress((completion) => {
            this.employeeService.saveVacationGroup(this.vacationGroup).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.vacationGroupId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.vacationGroup.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.vacationGroupId = result.integerValue;
                        this.vacationGroup.vacationGroupId = result.integerValue;
                        this.selectedVacationGroupId = this.vacationGroupId;

                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.vacationGroup);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.onLoadData();
            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.employeeService.getVacationGroups(true, false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.vacationGroupId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.vacationGroupId) {
                    this.vacationGroupId = recordId;
                    this.selectedVacationGroupId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    protected delete() {
        if (this.selectedVacationGroupId > 0) {
            this.progress.startDeleteProgress((completion) => {
                this.employeeService.deleteVacationGroup(this.selectedVacationGroupId).then((result) => {
                    if (result.success) {
                        completion.completed(this.vacationGroup, true);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }).then(x => {
                super.closeMe(true);
            });
        }
    }

    // HELP-METHODS  
    protected copy() {  
       super.copy();  
       this.isNew = true;  
       this.vacationGroupId = 0;  
       this.vacationGroup.vacationGroupSE.vacationGroupId = 0;  
       this.vacationGroup.vacationGroupId = 0;  
       this.vacationGroup.name = "";  
       this.focusService.focusByName("ctrl_vacationGroup_name"); 
    }

    private new() {
        this.isNew = true;
        //Create new empty vacationgroupDTO
        this.vacationGroupId = 0;
        this.vacationGroup = new VacationGroupDTO();
        this.vacationGroup.vacationGroupSE = new VacationGroupSEDTO();
        this.vacationGroup.type = 1;
        this.selectedFromDateId = 1;
        //Default values when adding new one
        this.vacationGroup.vacationGroupSE.calculationType = 1;
        this.vacationGroup.vacationGroupSE.remainingDaysPayoutMonth = 1;
        this.vacationGroup.vacationGroupSE.vacationAbsenceCalculationRule = 1;

        this.vacationGroup.vacationGroupSE.yearEndRemainingDaysRule = 1;
        this.vacationGroup.vacationGroupSE.yearEndOverdueDaysRule = 1;
        this.vacationGroup.vacationGroupSE.yearEndVacationVariableRule = 1;

        this.vacationGroup.vacationGroupSE.vacationDayAdditionPercent = this.sysPayrollPriceVacationDayPercent;
        this.vacationGroup.vacationGroupSE.vacationDayPercent = this.sysPayrollPriceVacationDayPercent;

        this.vacationGroup.vacationGroupSE.vacationSalaryPayoutMonth = 1;
        this.vacationGroup.vacationGroupSE.vacationVariablePayoutMonth = 1;

        this.setFieldVisibility();

        this.dirtyHandler.isDirty = false;
    }

    private setFieldVisibility() {
        var vacationGroupType: number = this.vacationGroup.type;
        // Show fields based on VacationGroupType and CalculationType
        this.showAccordions = true;
        switch (vacationGroupType) {
            case TermGroup_VacationGroupType.NoCalculation:
                // NoCalculation
                // No fields visible
                this.showAccordions = false;
                break;
            case TermGroup_VacationGroupType.DirectPayment:
                //DirectPayment

                this.useAdditionalVacationDaysVisibility = false;
                this.additionalVacationDays1Visibility = false;
                this.additionalVacationDays2Visibility = false;
                this.additionalVacationDays3Visibility = false;
                this.vacationHandleRuleVisibility = false;
                this.vacationDaysHandleRuleVisibility = false;
                this.vacationDaysGrossUseFiveDaysPerWeekVisibility = false;
                this.remainingDaysRuleVisibility = false;
                this.useMaxRemainingDaysVisibility = false;
                this.remainingDaysPayoutMonthVisibility = false;
                this.remainingDaysRectangleVisibility = false;
                this.earningYearAmountFromDateVisibility = false;
                this.earningYearVariableAmountFromDateVisibility = false;
                this.monthlySalaryFormulaVisibility = false;
                this.hourlySalaryFormulaVisibility = false;
                this.vacationDayPercentVisibility = true;
                this.vacationDayAdditionPercentVisibility = false;
                this.vacationVariablePercentVisibility = false;
                this.vacationDayPercentPriceTypeVisibility = false;
                this.vacationDayAdditionPercentPriceTypeVisibility = false;
                this.vacationVariablePercentPriceTypeVisibility = false;
                this.useGuaranteeAmountVisibility = false;
                this.vacationAbsenceCalculationRuleVisibility = true;
                this.vacationSalaryPayoutRuleVisibility = false;
                this.vacationSalaryPayoutDaysVisibility = false;
                this.vacationSalaryPayoutMonthVisibility = false;
                this.vacationSalaryPayoutRectangleVisibility = false;
                this.vacationVariablePayoutRuleVisibility = false;
                this.vacationVariablePayoutDaysVisibility = false;
                this.vacationVariablePayoutMonthVisibility = false;
                this.vacationVariablePayoutRectangleVisibility = false;
                if (this.vacationGroup.vacationGroupSE) {
                    switch (this.vacationGroup.vacationGroupSE.calculationType) {
                        case TermGroup_VacationGroupCalculationType.DirectPayment_AccordingToVacationLaw:
                            // Read only, get value from SysPayrollPrice
                            this.vacationDayPercentReadOnly = true;
                            this.vacationGroup.vacationGroupSE.vacationDayPercent = this.sysPayrollPriceVacationDayPercent;
                            break;
                        case TermGroup_VacationGroupCalculationType.DirectPayment_AccordingToCollectiveAgreement:
                            // Enabled, mandatory
                            this.vacationDayPercentReadOnly = false;
                            break;
                    }
                }
                break;
            case TermGroup_VacationGroupType.EarningYearIsBeforeVacationYear:
                // EarningYearIsBeforeVacationYear
                // Specific for each calculation type
                switch (this.vacationGroup.vacationGroupSE.calculationType) {
                    case TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToVacationLaw:
                        // PercentCalculation_AccordingToVacationLaw

                        this.useAdditionalVacationDaysVisibility = false;
                        this.additionalVacationDays1Visibility = false;
                        this.additionalVacationDays2Visibility = false;
                        this.additionalVacationDays3Visibility = false;
                        this.vacationHandleRuleVisibility = false;
                        this.vacationDaysHandleRuleVisibility = false;
                        this.vacationDaysGrossUseFiveDaysPerWeekVisibility = false;
                        this.remainingDaysRuleVisibility = true;
                        this.remainingDaysRuleReadOnly = true;
                        this.vacationGroup.vacationGroupSE.remainingDaysRule = TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw;
                        this.useMaxRemainingDaysVisibility = false;
                        this.remainingDaysPayoutMonthVisibility = true;
                        this.remainingDaysRectangleVisibility = true;
                        this.earningYearAmountFromDateVisibility = true;
                        if (!this.isNew && this.vacationGroup.vacationGroupSE.earningYearAmountFromDate == null && this.vacationGroup.fromDate != null) {
                            let fromDate: Date = new Date(this.vacationGroup.fromDate.toString());
                            this.selectedEarningYearAmountFromDateId = fromDate.getMonth() + 1;
                        }
                        this.earningYearVariableAmountFromDateVisibility = false;
                        this.monthlySalaryFormulaVisibility = false;
                        this.hourlySalaryFormulaVisibility = false;
                        this.vacationDayPercentVisibility = true;
                        // Read only, get value from SysPayrollPrice
                        this.vacationDayPercentReadOnly = true;
                        this.vacationGroup.vacationGroupSE.vacationDayPercent = this.sysPayrollPriceVacationDayPercent;
                        this.vacationDayAdditionPercentVisibility = false;
                        this.vacationVariablePercentVisibility = false;
                        this.vacationDayPercentPriceTypeVisibility = false;
                        this.vacationDayAdditionPercentPriceTypeVisibility = false;
                        this.vacationVariablePercentPriceTypeVisibility = false;
                        this.useGuaranteeAmountVisibility = false;
                        this.vacationAbsenceCalculationRuleVisibility = true;
                        this.vacationSalaryPayoutRuleVisibility = true;
                        this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule = TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation;
                        this.vacationSalaryPayoutRuleReadOnly = true;
                        this.vacationSalaryPayoutRuleSelectorLabel = this.terms["time.employee.vacationgroup.salarypayoutruleselectorlabel1"];
                        this.vacationSalaryPayoutDaysVisibility = false;
                        this.vacationSalaryPayoutMonthVisibility = false;
                        this.vacationSalaryPayoutRectangleVisibility = false;
                        this.vacationVariablePayoutRuleVisibility = false;
                        this.vacationVariablePayoutDaysVisibility = false;
                        this.vacationVariablePayoutMonthVisibility = false;
                        this.vacationVariablePayoutRectangleVisibility = false;
                        break;

                    case TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToCollectiveAgreement:
                        // PercentCalculation_AccordingToCollectiveAgreement

                        this.useAdditionalVacationDaysVisibility = false;
                        this.additionalVacationDays1Visibility = false;
                        this.additionalVacationDays2Visibility = false;
                        this.additionalVacationDays3Visibility = false;
                        this.vacationHandleRuleVisibility = true;
                        this.vacationDaysHandleRuleVisibility = this.vacationGroup.vacationGroupSE.vacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Days;
                        this.vacationDaysGrossUseFiveDaysPerWeekVisibility = this.vacationDaysHandleRuleVisibility && this.vacationGroup.vacationGroupSE.vacationDaysHandleRule == TermGroup_VacationGroupVacationDaysHandleRule.Gross;
                        this.remainingDaysRuleVisibility = true;
                        this.remainingDaysRuleReadOnly = false;
                        this.useMaxRemainingDaysVisibility = false;
                        this.remainingDaysPayoutMonthVisibility = this.vacationGroup.vacationGroupSE.remainingDaysRule == TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw;
                        this.remainingDaysRectangleVisibility = true;
                        this.earningYearAmountFromDateVisibility = true;
                        if (!this.isNew && this.vacationGroup.vacationGroupSE.earningYearAmountFromDate == null && this.vacationGroup.fromDate != null) {
                            let fromDate: Date = new Date(this.vacationGroup.fromDate.toString());
                            this.selectedEarningYearAmountFromDateId = fromDate.getMonth() + 1;
                        }
                        this.earningYearVariableAmountFromDateVisibility = false;
                        this.monthlySalaryFormulaVisibility = false;
                        this.hourlySalaryFormulaVisibility = true
                        this.vacationDayPercentVisibility = true;
                        this.vacationDayPercentReadOnly = false;
                        this.vacationDayAdditionPercentVisibility = false;
                        this.vacationVariablePercentVisibility = false;
                        this.vacationDayPercentPriceTypeVisibility = false;
                        this.vacationDayAdditionPercentPriceTypeVisibility = false;
                        this.vacationVariablePercentPriceTypeVisibility = false;
                        this.useGuaranteeAmountVisibility = true;
                        this.vacationAbsenceCalculationRuleVisibility = true;
                        this.vacationSalaryPayoutRuleVisibility = true;
                        this.vacationSalaryPayoutRuleReadOnly = false;
                        this.vacationSalaryPayoutRuleSelectorLabel = this.terms["time.employee.vacationgroup.salarypayoutruleselectorlabel1"];
                        this.vacationSalaryPayoutDaysVisibility = this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule == TermGroup_VacationGroupVacationSalaryPayoutRule.PartlyPayoutBeforeVacation;
                        this.vacationSalaryPayoutMonthVisibility = this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule != TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation;
                        this.vacationSalaryPayoutRectangleVisibility = this.vacationSalaryPayoutDaysVisibility || this.vacationSalaryPayoutMonthVisibility;
                        this.vacationVariablePayoutRuleVisibility = false;
                        this.vacationVariablePayoutDaysVisibility = false;
                        this.vacationVariablePayoutMonthVisibility = false;
                        this.vacationVariablePayoutRectangleVisibility = false;
                        break;

                    case TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw:
                        // VacationDayAddition_AccordingToVacationLaw

                        this.useAdditionalVacationDaysVisibility = false;
                        this.additionalVacationDays1Visibility = false;
                        this.additionalVacationDays2Visibility = false;
                        this.additionalVacationDays3Visibility = false;
                        this.vacationHandleRuleVisibility = false;
                        this.vacationDaysHandleRuleVisibility = false;
                        this.vacationDaysGrossUseFiveDaysPerWeekVisibility = false;
                        this.remainingDaysRuleVisibility = true;
                        this.remainingDaysRuleReadOnly = true;
                        this.useMaxRemainingDaysVisibility = false;
                        this.remainingDaysPayoutMonthVisibility = true;
                        this.remainingDaysRectangleVisibility = true;
                        this.earningYearAmountFromDateVisibility = true;
                        this.earningYearVariableAmountFromDateVisibility = false;
                        if (!this.isNew && this.vacationGroup.vacationGroupSE.earningYearAmountFromDate == null && this.vacationGroup.fromDate != null) {
                            let fromDate: Date = new Date(this.vacationGroup.fromDate.toString());
                            this.selectedEarningYearAmountFromDateId = fromDate.getMonth() + 1;
                        }
                        this.monthlySalaryFormulaVisibility = true;
                        this.hourlySalaryFormulaVisibility = false;
                        this.vacationDayPercentVisibility = false;
                        this.vacationDayAdditionPercentVisibility = true;
                        // Read only, get value from SysPayrollPrice
                        this.vacationDayAdditionPercentReadOnly = true;
                        this.vacationGroup.vacationGroupSE.vacationDayAdditionPercent = this.sysPayrollPriceVacationDayAdditionPercent;
                        this.vacationVariablePercentVisibility = true;
                        this.vacationVariablePercentReadOnly = true;
                        this.vacationGroup.vacationGroupSE.vacationVariablePercent = this.sysPayrollPriceVacationDayPercent;
                        this.vacationDayPercentPriceTypeVisibility = false;
                        this.vacationDayAdditionPercentPriceTypeVisibility = false;
                        this.vacationVariablePercentPriceTypeVisibility = false;
                        this.useGuaranteeAmountVisibility = false;
                        this.vacationAbsenceCalculationRuleVisibility = false;
                        this.vacationSalaryPayoutRuleVisibility = true;
                        this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule = TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation;
                        this.vacationSalaryPayoutRuleReadOnly = true;
                        this.vacationSalaryPayoutRuleSelectorLabel = this.terms["time.employee.vacationgroup.salarypayoutruleselectorlabel2"];
                        this.vacationSalaryPayoutDaysVisibility = false;
                        this.vacationSalaryPayoutMonthVisibility = false;
                        this.vacationSalaryPayoutRectangleVisibility = false;
                        if (this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId == null)
                            this.vacationVariablePayoutRuleVisibility = false;
                        else
                            this.vacationVariablePayoutRuleVisibility = this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId != 0;
                        this.vacationGroup.vacationGroupSE.vacationVariablePayoutRule = TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation;
                        this.vacationVariablePayoutRuleReadOnly = true;
                        this.vacationVariablePayoutDaysVisibility = false;
                        this.vacationVariablePayoutMonthVisibility = false;
                        this.vacationVariablePayoutRectangleVisibility = false;
                        break;

                    case TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement:
                        // VacationDayAddition_AccordingToCollectiveAgreement

                        this.useAdditionalVacationDaysVisibility = true;
                        this.additionalVacationDays1Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 1;
                        this.additionalVacationDays2Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 2;
                        this.additionalVacationDays3Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 3;
                        this.vacationHandleRuleVisibility = true;
                        this.vacationDaysHandleRuleVisibility = this.vacationGroup.vacationGroupSE.vacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Days;
                        this.vacationDaysGrossUseFiveDaysPerWeekVisibility = this.vacationDaysHandleRuleVisibility && this.vacationGroup.vacationGroupSE.vacationDaysHandleRule == TermGroup_VacationGroupVacationDaysHandleRule.Gross;
                        this.remainingDaysRuleVisibility = true;
                        this.remainingDaysRuleReadOnly = false;
                        this.useMaxRemainingDaysVisibility = false;
                        this.remainingDaysPayoutMonthVisibility = this.vacationGroup.vacationGroupSE.remainingDaysRule === TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw;
                        this.remainingDaysRectangleVisibility = true;
                        this.earningYearAmountFromDateVisibility = true;
                        this.earningYearVariableAmountFromDateVisibility = true;
                        if (!this.isNew && this.vacationGroup.vacationGroupSE.earningYearAmountFromDate == null && this.vacationGroup.fromDate != null) {
                            let fromDate: Date = new Date(this.vacationGroup.fromDate.toString());
                            this.selectedEarningYearAmountFromDateId = fromDate.getMonth() + 1;
                        }
                        this.monthlySalaryFormulaVisibility = true;
                        this.hourlySalaryFormulaVisibility = false;
                        this.vacationDayPercentVisibility = false;
                        this.vacationDayAdditionPercentVisibility = false;
                        this.vacationVariablePercentVisibility = false;
                        this.vacationDayPercentPriceTypeVisibility = true;
                        this.vacationDayAdditionPercentPriceTypeVisibility = true;
                        this.vacationVariablePercentPriceTypeVisibility = true;
                        this.useGuaranteeAmountVisibility = true;
                        this.vacationAbsenceCalculationRuleVisibility = true;
                        this.vacationSalaryPayoutRuleVisibility = true;
                        this.vacationSalaryPayoutRuleReadOnly = false;
                        this.vacationSalaryPayoutRuleSelectorLabel = this.terms["time.employee.vacationgroup.salarypayoutruleselectorlabel2"];
                        this.vacationSalaryPayoutDaysVisibility = this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule == TermGroup_VacationGroupVacationSalaryPayoutRule.PartlyPayoutBeforeVacation;
                        this.vacationSalaryPayoutMonthVisibility = this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule != TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation;
                        this.vacationSalaryPayoutRectangleVisibility = this.vacationSalaryPayoutDaysVisibility || this.vacationSalaryPayoutMonthVisibility;
                        if (this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId == null)
                            this.vacationVariablePayoutRuleVisibility = false;
                        else
                            this.vacationVariablePayoutRuleVisibility = this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId != 0;
                        this.vacationVariablePayoutRuleReadOnly = false;
                        this.vacationVariablePayoutDaysVisibility = this.vacationVariablePayoutRuleVisibility && this.vacationGroup.vacationGroupSE.vacationVariablePayoutRule == TermGroup_VacationGroupVacationSalaryPayoutRule.PartlyPayoutBeforeVacation;
                        this.vacationVariablePayoutMonthVisibility = this.vacationVariablePayoutRuleVisibility && this.vacationGroup.vacationGroupSE.vacationVariablePayoutRule != TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation;
                        this.vacationVariablePayoutRectangleVisibility = this.vacationVariablePayoutDaysVisibility || this.vacationVariablePayoutMonthVisibility;
                        break;
                }
                break;

            case TermGroup_VacationGroupType.EarningYearIsVacationYear:
                // EarningYearIsVacationYear

                // Specific for each calculation type
                switch (this.vacationGroup.vacationGroupSE.calculationType) {
                    case TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_ABAgreement:
                        // ABAgreement

                        this.useAdditionalVacationDaysVisibility = true;
                        this.additionalVacationDays1Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 1;
                        this.additionalVacationDays2Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 2;
                        this.additionalVacationDays3Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 3;
                        this.vacationHandleRuleVisibility = true;
                        this.vacationDaysHandleRuleVisibility = this.vacationGroup.vacationGroupSE.vacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Days;
                        this.vacationDaysGrossUseFiveDaysPerWeekVisibility = this.vacationDaysHandleRuleVisibility && this.vacationGroup.vacationGroupSE.vacationDaysHandleRule == TermGroup_VacationGroupVacationDaysHandleRule.Gross;
                        this.remainingDaysRuleVisibility = true;
                        this.remainingDaysRuleReadOnly = false;
                        this.useMaxRemainingDaysVisibility = this.vacationGroup.vacationGroupSE.remainingDaysRule == TermGroup_VacationGroupRemainingDaysRule.AllRemainingDaysSavedToYear1;
                        this.remainingDaysPayoutMonthVisibility = this.vacationGroup.vacationGroupSE.remainingDaysRule == TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw;
                        this.remainingDaysRectangleVisibility = this.useMaxRemainingDaysVisibility || this.remainingDaysPayoutMonthVisibility;
                        this.earningYearAmountFromDateVisibility = false;
                        this.earningYearVariableAmountFromDateVisibility = true;
                        if (!this.isNew && this.vacationGroup.vacationGroupSE.earningYearAmountFromDate == null && this.vacationGroup.fromDate != null) {
                            let fromDate: Date = new Date(this.vacationGroup.fromDate.toString());
                            this.selectedEarningYearAmountFromDateId = fromDate.getMonth() + 1;
                        }
                        this.monthlySalaryFormulaVisibility = true;
                        this.hourlySalaryFormulaVisibility = false;
                        this.vacationDayPercentVisibility = false;
                        this.vacationDayAdditionPercentVisibility = false;
                        this.vacationVariablePercentVisibility = false;
                        this.vacationDayPercentPriceTypeVisibility = true;
                        this.vacationDayAdditionPercentPriceTypeVisibility = true;
                        this.vacationVariablePercentPriceTypeVisibility = true;
                        this.useGuaranteeAmountVisibility = true;
                        this.vacationAbsenceCalculationRuleVisibility = true;
                        this.vacationSalaryPayoutRuleVisibility = true;
                        this.vacationSalaryPayoutRuleReadOnly = false;
                        this.vacationSalaryPayoutRuleSelectorLabel = this.terms["time.employee.vacationgroup.salarypayoutruleselectorlabel1"];
                        this.vacationSalaryPayoutDaysVisibility = this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule == TermGroup_VacationGroupVacationSalaryPayoutRule.PartlyPayoutBeforeVacation;
                        this.vacationSalaryPayoutMonthVisibility = this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule != TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation;
                        this.vacationSalaryPayoutRectangleVisibility = this.vacationSalaryPayoutDaysVisibility || this.vacationSalaryPayoutMonthVisibility;
                        if (this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId == null)
                            this.vacationVariablePayoutRuleVisibility = false;
                        else
                            this.vacationVariablePayoutRuleVisibility = this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId != 0;
                        this.vacationVariablePayoutRuleReadOnly = false;
                        this.vacationVariablePayoutDaysVisibility = this.vacationVariablePayoutRuleVisibility && this.vacationGroup.vacationGroupSE.vacationVariablePayoutRule == TermGroup_VacationGroupVacationSalaryPayoutRule.PartlyPayoutBeforeVacation;
                        this.vacationVariablePayoutMonthVisibility = this.vacationVariablePayoutRuleVisibility && this.vacationGroup.vacationGroupSE.vacationVariablePayoutRule != TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation;
                        this.vacationVariablePayoutRectangleVisibility = this.vacationVariablePayoutDaysVisibility || this.vacationVariablePayoutMonthVisibility;
                        break;

                    case TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition:
                        // VacationDayAddition

                        this.useAdditionalVacationDaysVisibility = true;
                        this.additionalVacationDays1Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 1;
                        this.additionalVacationDays2Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 2;
                        this.additionalVacationDays3Visibility = this.vacationGroup.vacationGroupSE.useAdditionalVacationDays && this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays >= 3;
                        this.vacationHandleRuleVisibility = true;
                        this.vacationDaysHandleRuleVisibility = this.vacationGroup.vacationGroupSE.vacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Days;
                        this.vacationDaysGrossUseFiveDaysPerWeekVisibility = this.vacationDaysHandleRuleVisibility && this.vacationGroup.vacationGroupSE.vacationDaysHandleRule == TermGroup_VacationGroupVacationDaysHandleRule.Gross;
                        this.remainingDaysRuleVisibility = true;
                        this.remainingDaysRuleReadOnly = false;
                        this.useMaxRemainingDaysVisibility = 1 > 2; //Override TS transpiler
                        this.remainingDaysPayoutMonthVisibility = this.vacationGroup.vacationGroupSE.remainingDaysRule == TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw;
                        this.remainingDaysRectangleVisibility = this.useMaxRemainingDaysVisibility || this.remainingDaysPayoutMonthVisibility;
                        this.earningYearAmountFromDateVisibility = false;
                        this.earningYearVariableAmountFromDateVisibility = true;
                        if (!this.isNew && this.vacationGroup.vacationGroupSE.earningYearAmountFromDate == null && this.vacationGroup.fromDate != null) {
                            let fromDate: Date = new Date(this.vacationGroup.fromDate.toString());
                            this.selectedEarningYearAmountFromDateId = fromDate.getMonth() + 1;
                        }
                        this.monthlySalaryFormulaVisibility = true;
                        this.hourlySalaryFormulaVisibility = false;
                        this.vacationDayPercentVisibility = false;
                        this.vacationDayAdditionPercentVisibility = false;
                        this.vacationVariablePercentVisibility = false;
                        this.vacationDayPercentPriceTypeVisibility = true;
                        this.vacationDayAdditionPercentPriceTypeVisibility = true;
                        this.vacationVariablePercentPriceTypeVisibility = true;
                        this.useGuaranteeAmountVisibility = true;
                        this.vacationAbsenceCalculationRuleVisibility = true;
                        this.vacationSalaryPayoutRuleVisibility = true;
                        this.vacationSalaryPayoutRuleReadOnly = false;
                        this.vacationSalaryPayoutRuleSelectorLabel = this.terms["time.employee.vacationgroup.salarypayoutruleselectorlabel2"];
                        this.vacationSalaryPayoutDaysVisibility = this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule == TermGroup_VacationGroupVacationSalaryPayoutRule.PartlyPayoutBeforeVacation;
                        this.vacationSalaryPayoutMonthVisibility = this.vacationGroup.vacationGroupSE.vacationSalaryPayoutRule != TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation;
                        this.vacationSalaryPayoutRectangleVisibility = this.vacationSalaryPayoutDaysVisibility || this.vacationSalaryPayoutMonthVisibility;
                        if (this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId == null)
                            this.vacationVariablePayoutRuleVisibility = false;
                        else
                            this.vacationVariablePayoutRuleVisibility = this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId != 0;
                        this.vacationVariablePayoutRuleReadOnly = false;
                        this.vacationVariablePayoutDaysVisibility = this.vacationVariablePayoutRuleVisibility && this.vacationGroup.vacationGroupSE.vacationVariablePayoutRule == TermGroup_VacationGroupVacationSalaryPayoutRule.PartlyPayoutBeforeVacation;
                        this.vacationVariablePayoutMonthVisibility = this.vacationVariablePayoutRuleVisibility && this.vacationGroup.vacationGroupSE.vacationVariablePayoutRule != TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation;
                        this.vacationVariablePayoutRectangleVisibility = this.vacationVariablePayoutDaysVisibility || this.vacationVariablePayoutMonthVisibility;
                        break;
                }
                break;
        }
    }

    // VALIDATION
    private emtyFieldsBeforeSave() {
        // hidden values will be cleared

        this.vacationGroup.vacationGroupSE.useAdditionalVacationDays = this.useAdditionalVacationDaysVisibility == true ? this.vacationGroup.vacationGroupSE.useAdditionalVacationDays : false;
        this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays = this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays ? this.vacationGroup.vacationGroupSE.nbrOfAdditionalVacationDays : 0;
        this.vacationGroup.vacationGroupSE.additionalVacationDaysFromAge1 = (!this.vacationGroup.vacationGroupSE.useAdditionalVacationDays) ? null : this.vacationGroup.vacationGroupSE.additionalVacationDaysFromAge1;
        this.vacationGroup.vacationGroupSE.additionalVacationDays1 = (!this.vacationGroup.vacationGroupSE.useAdditionalVacationDays) ? null : this.vacationGroup.vacationGroupSE.additionalVacationDays1;
        this.vacationGroup.vacationGroupSE.additionalVacationDaysFromAge2 = (!this.vacationGroup.vacationGroupSE.useAdditionalVacationDays) ? null : this.vacationGroup.vacationGroupSE.additionalVacationDaysFromAge2;
        this.vacationGroup.vacationGroupSE.additionalVacationDays2 = (!this.vacationGroup.vacationGroupSE.useAdditionalVacationDays) ? null : this.vacationGroup.vacationGroupSE.additionalVacationDays2;
        this.vacationGroup.vacationGroupSE.additionalVacationDaysFromAge3 = (!this.vacationGroup.vacationGroupSE.useAdditionalVacationDays) ? null : this.vacationGroup.vacationGroupSE.additionalVacationDaysFromAge3;
        this.vacationGroup.vacationGroupSE.additionalVacationDays3 = (!this.vacationGroup.vacationGroupSE.useAdditionalVacationDays) ? null : this.vacationGroup.vacationGroupSE.additionalVacationDays3;

        if (this.vacationGroup.vacationGroupSE.vacationDaysHandleRule != null)
            this.vacationGroup.vacationGroupSE.vacationDaysGrossUseFiveDaysPerWeek = (this.vacationGroup.vacationGroupSE.vacationDaysHandleRule != TermGroup_VacationGroupVacationDaysHandleRule.Gross) ? false : this.vacationGroup.vacationGroupSE.vacationDaysGrossUseFiveDaysPerWeek;

        this.vacationGroup.vacationGroupSE.useMaxRemainingDays = !this.useMaxRemainingDaysVisibility ? false : this.vacationGroup.vacationGroupSE.useMaxRemainingDays;
        this.vacationGroup.vacationGroupSE.maxRemainingDays = !this.vacationGroup.vacationGroupSE.useMaxRemainingDays ? null : this.vacationGroup.vacationGroupSE.maxRemainingDays;
        this.vacationGroup.vacationGroupSE.remainingDaysPayoutMonth = !this.vacationGroup.vacationGroupSE.useMaxRemainingDays ? null : (this.vacationGroup.vacationGroupSE.remainingDaysPayoutMonth);
        this.vacationGroup.vacationGroupSE.useFillUpToVacationDaysPaidByLawRule = this.vacationGroup.vacationGroupSE.remainingDaysRule != TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw ? false : this.vacationGroup.vacationGroupSE.useFillUpToVacationDaysPaidByLawRule;

        this.vacationGroup.vacationGroupSE.monthlySalaryFormulaId = !this.monthlySalaryFormulaVisibility ? null : (this.vacationGroup.vacationGroupSE.monthlySalaryFormulaId != 0 ? this.vacationGroup.vacationGroupSE.monthlySalaryFormulaId : null);
        this.vacationGroup.vacationGroupSE.hourlySalaryFormulaId = !this.hourlySalaryFormulaVisibility ? null : (this.vacationGroup.vacationGroupSE.hourlySalaryFormulaId != 0 ? this.vacationGroup.vacationGroupSE.hourlySalaryFormulaId : null);
        this.vacationGroup.vacationGroupSE.vacationDayPercent = !this.vacationDayPercentVisibility ? null : this.vacationGroup.vacationGroupSE.vacationDayPercent;
        this.vacationGroup.vacationGroupSE.vacationDayAdditionPercent = (this.vacationDayAdditionPercentReadOnly || !this.vacationDayAdditionPercentVisibility) ? null : this.vacationGroup.vacationGroupSE.vacationDayAdditionPercent;
        this.vacationGroup.vacationGroupSE.vacationVariablePercent = !this.vacationVariablePercentVisibility ? null : this.vacationGroup.vacationGroupSE.vacationVariablePercent;
        this.vacationGroup.vacationGroupSE.vacationDayPercentPriceTypeId = !this.vacationDayPercentPriceTypeVisibility ? null : (this.vacationGroup.vacationGroupSE.vacationDayPercentPriceTypeId != 0 ? this.vacationGroup.vacationGroupSE.vacationDayPercentPriceTypeId : null);
        this.vacationGroup.vacationGroupSE.vacationDayAdditionPercentPriceTypeId = !this.vacationDayAdditionPercentPriceTypeVisibility ? null : (this.vacationGroup.vacationGroupSE.vacationDayAdditionPercentPriceTypeId != 0 ? this.vacationGroup.vacationGroupSE.vacationDayAdditionPercentPriceTypeId : null);
        this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId = !this.vacationVariablePercentPriceTypeVisibility ? null : (this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId != 0 ? this.vacationGroup.vacationGroupSE.vacationVariablePercentPriceTypeId : null);
        if (!this.useGuaranteeAmountVisibility)
            this.vacationGroup.vacationGroupSE.useGuaranteeAmount = false;

        this.vacationGroup.vacationGroupSE.guaranteeAmountAccordingToHandels = !this.vacationGroup.vacationGroupSE.useGuaranteeAmount ? false : this.vacationGroup.vacationGroupSE.guaranteeAmountAccordingToHandels;
        this.vacationGroup.vacationGroupSE.guaranteeAmountEmployedNbrOfYears = !this.vacationGroup.vacationGroupSE.useGuaranteeAmount ? null : this.vacationGroup.vacationGroupSE.guaranteeAmountEmployedNbrOfYears;
        this.vacationGroup.vacationGroupSE.guaranteeAmountPerDayPriceTypeId = !this.vacationGroup.vacationGroupSE.useGuaranteeAmount ? null : (this.vacationGroup.vacationGroupSE.guaranteeAmountPerDayPriceTypeId != 0 ? this.vacationGroup.vacationGroupSE.guaranteeAmountPerDayPriceTypeId : null);
        this.vacationGroup.vacationGroupSE.guaranteeAmountJuvenile = !this.vacationGroup.vacationGroupSE.useGuaranteeAmount ? false : this.vacationGroup.vacationGroupSE.guaranteeAmountJuvenile;
        this.vacationGroup.vacationGroupSE.guaranteeAmountJuvenileAgeLimit = !this.vacationGroup.vacationGroupSE.guaranteeAmountJuvenile ? null : this.vacationGroup.vacationGroupSE.guaranteeAmountJuvenileAgeLimit;
        this.vacationGroup.vacationGroupSE.guaranteeAmountJuvenilePerDayPriceTypeId = !this.vacationGroup.vacationGroupSE.guaranteeAmountJuvenile ? null : (this.vacationGroup.vacationGroupSE.guaranteeAmountJuvenilePerDayPriceTypeId != 0 ? this.vacationGroup.vacationGroupSE.guaranteeAmountJuvenilePerDayPriceTypeId : null);

        this.vacationGroup.vacationGroupSE.useOwnGuaranteeAmount = (!this.vacationGroup.vacationGroupSE.useGuaranteeAmount || !this.vacationGroup.vacationGroupSE.guaranteeAmountAccordingToHandels) ? false : this.vacationGroup.vacationGroupSE.useOwnGuaranteeAmount;
        this.vacationGroup.vacationGroupSE.ownGuaranteeAmount = (!this.vacationGroup.vacationGroupSE.useOwnGuaranteeAmount) ? null : this.vacationGroup.vacationGroupSE.ownGuaranteeAmount;

        this.vacationGroup.vacationGroupSE.vacationSalaryPayoutDays = !this.vacationSalaryPayoutDaysVisibility ? null : this.vacationGroup.vacationGroupSE.vacationSalaryPayoutDays;
        this.vacationGroup.vacationGroupSE.vacationSalaryPayoutMonth = !this.vacationSalaryPayoutMonthVisibility ? null : (this.vacationGroup.vacationGroupSE.vacationSalaryPayoutMonth != null ? this.vacationGroup.vacationGroupSE.vacationSalaryPayoutMonth : null);

        this.vacationGroup.vacationGroupSE.vacationVariablePayoutDays = !this.vacationVariablePayoutDaysVisibility ? null : this.vacationGroup.vacationGroupSE.vacationVariablePayoutDays;
        this.vacationGroup.vacationGroupSE.vacationVariablePayoutMonth = !this.vacationVariablePayoutMonthVisibility ? null : (this.vacationGroup.vacationGroupSE.vacationVariablePayoutMonth != null ? this.vacationGroup.vacationGroupSE.vacationVariablePayoutMonth : null);
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.vacationGroup) {
                // Mandatory fields
                if (this.selectedFromDateId === 0 && !this.selectedFromDate) {
                    mandatoryFieldKeys.push("time.employee.vacationgroup.fromdate");
                }
                if (this.vacationGroup.vacationGroupSE) {
                    if (this.selectedEarningYearAmountFromDateId === 0 && !this.selectedEarningYearAmountFromDate) {
                        mandatoryFieldKeys.push("time.employee.vacationgroup.earningyearamountfromdate");
                    }
                }
            }
        });
    }

    private filterCalculationTypesByVacationGroupType() {
        var vacationGroupType: number = this.vacationGroup.type;
        var calcType;
        this.calculationTypes = [];
        switch (vacationGroupType) {
            case TermGroup_VacationGroupType.NoCalculation:
                break;
            case TermGroup_VacationGroupType.DirectPayment:
                calcType = _.find(this.allCalculationTypes, { id: TermGroup_VacationGroupCalculationType.DirectPayment_AccordingToVacationLaw });
                this.calculationTypes.push({ id: calcType.id, name: calcType.name });
                calcType = _.find(this.allCalculationTypes, { id: TermGroup_VacationGroupCalculationType.DirectPayment_AccordingToCollectiveAgreement });
                this.calculationTypes.push({ id: calcType.id, name: calcType.name });
                break;
            case TermGroup_VacationGroupType.EarningYearIsBeforeVacationYear:
                calcType = _.find(this.allCalculationTypes, { id: TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToVacationLaw });
                this.calculationTypes.push({ id: calcType.id, name: calcType.name });
                calcType = _.find(this.allCalculationTypes, { id: TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToCollectiveAgreement });
                this.calculationTypes.push({ id: calcType.id, name: calcType.name });
                calcType = _.find(this.allCalculationTypes, { id: TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw });
                this.calculationTypes.push({ id: calcType.id, name: calcType.name });
                calcType = _.find(this.allCalculationTypes, { id: TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement });
                this.calculationTypes.push({ id: calcType.id, name: calcType.name });
                break;
            case TermGroup_VacationGroupType.EarningYearIsVacationYear:
                calcType = _.find(this.allCalculationTypes, { id: TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_ABAgreement });
                this.calculationTypes.push({ id: calcType.id, name: calcType.name });
                calcType = _.find(this.allCalculationTypes, { id: TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition });
                this.calculationTypes.push({ id: calcType.id, name: calcType.name });
                break;
        }
    }

    private filterVacationDaysHandleRulesByVacationGroupType() {

        var vacationGroupType: number = this.vacationGroup.type;
        var calcType;
        this.vacationDaysHandleRules = [];

        switch (vacationGroupType) {
            case TermGroup_VacationGroupType.DirectPayment:
            case TermGroup_VacationGroupType.EarningYearIsBeforeVacationYear:
                calcType = _.find(this.allVacationDaysHandleRules, { id: TermGroup_VacationGroupVacationDaysHandleRule.Gross });
                this.vacationDaysHandleRules.push({ id: calcType.id, name: calcType.name });
                calcType = _.find(this.allVacationDaysHandleRules, { id: TermGroup_VacationGroupVacationDaysHandleRule.Net });
                this.vacationDaysHandleRules.push({ id: calcType.id, name: calcType.name });
                if (this.isNew)
                    this.vacationGroup.vacationGroupSE.vacationDaysHandleRule = 2;
                break;
            case TermGroup_VacationGroupType.EarningYearIsVacationYear:
                var calculationType: number = this.vacationGroup.vacationGroupSE.calculationType;
                switch (calculationType) {
                    case TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_ABAgreement:
                        calcType = _.find(this.allVacationDaysHandleRules, { id: TermGroup_VacationGroupVacationDaysHandleRule.VacationFactor });
                        this.vacationDaysHandleRules.push({ id: calcType.id, name: calcType.name });
                        calcType = _.find(this.allVacationDaysHandleRules, { id: TermGroup_VacationGroupVacationDaysHandleRule.Gross });
                        this.vacationDaysHandleRules.push({ id: calcType.id, name: calcType.name });
                        calcType = _.find(this.allVacationDaysHandleRules, { id: TermGroup_VacationGroupVacationDaysHandleRule.Net });
                        this.vacationDaysHandleRules.push({ id: calcType.id, name: calcType.name });
                        break;
                    case TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition:
                        calcType = _.find(this.allVacationDaysHandleRules, { id: TermGroup_VacationGroupVacationDaysHandleRule.VacationCoefficient });
                        this.vacationDaysHandleRules.push({ id: calcType.id, name: calcType.name });
                        calcType = _.find(this.allVacationDaysHandleRules, { id: TermGroup_VacationGroupVacationDaysHandleRule.Gross });
                        this.vacationDaysHandleRules.push({ id: calcType.id, name: calcType.name });
                        calcType = _.find(this.allVacationDaysHandleRules, { id: TermGroup_VacationGroupVacationDaysHandleRule.Net });
                        this.vacationDaysHandleRules.push({ id: calcType.id, name: calcType.name });
                        if (this.isNew)
                            this.vacationGroup.vacationGroupSE.vacationDaysHandleRule = 2;
                        break;
                }
                break;
        }

    }

    private filterVacationAbsenceCalculationRulesByVacationGroupType() {

        var vacationGroupType: number = this.vacationGroup.type;
        var calcRule;
        this.absenceCalculationRules = [];

        switch (vacationGroupType) {
            case TermGroup_VacationGroupType.DirectPayment:
                calcRule = _.find(this.allAbsenceCalculationRules, { id: TermGroup_VacationGroupVacationAbsenceCalculationRule.Actual });
                this.absenceCalculationRules.push({ id: calcRule.id, name: calcRule.name });
                calcRule = _.find(this.allAbsenceCalculationRules, { id: TermGroup_VacationGroupVacationAbsenceCalculationRule.PerHour });
                this.absenceCalculationRules.push({ id: calcRule.id, name: calcRule.name });
                break;
            case TermGroup_VacationGroupType.EarningYearIsBeforeVacationYear:
            case TermGroup_VacationGroupType.EarningYearIsVacationYear:
                calcRule = _.find(this.allAbsenceCalculationRules, { id: TermGroup_VacationGroupVacationAbsenceCalculationRule.Actual });
                this.absenceCalculationRules.push({ id: calcRule.id, name: calcRule.name });
                calcRule = _.find(this.allAbsenceCalculationRules, { id: TermGroup_VacationGroupVacationAbsenceCalculationRule.PerDay });
                this.absenceCalculationRules.push({ id: calcRule.id, name: calcRule.name });
                calcRule = _.find(this.allAbsenceCalculationRules, { id: TermGroup_VacationGroupVacationAbsenceCalculationRule.PerHour });
                this.absenceCalculationRules.push({ id: calcRule.id, name: calcRule.name });
                break;
        }

    }

    private filterRemainingDaysRulesByCalculationType() {

        if (this.vacationGroup.vacationGroupSE.calculationType == null)
            return;

        var calcRule;
        this.remainingDaysRules = [];

        var calculationType: number = this.vacationGroup.vacationGroupSE.calculationType;
        switch (calculationType) {
            case TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToVacationLaw:
            case TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw:
                calcRule = _.find(this.allRemainingDaysRules, { id: TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw });
                this.remainingDaysRules.push({ id: calcRule.id, name: calcRule.name });
                break;
            case TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToCollectiveAgreement:
            case TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement:
            case TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition:
                calcRule = _.find(this.allRemainingDaysRules, { id: TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw });
                this.remainingDaysRules.push({ id: calcRule.id, name: calcRule.name });
                calcRule = _.find(this.allRemainingDaysRules, { id: TermGroup_VacationGroupRemainingDaysRule.AllRemainingDaysSaved });
                this.remainingDaysRules.push({ id: calcRule.id, name: calcRule.name });
                break;
            case TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_ABAgreement:
                calcRule = _.find(this.allRemainingDaysRules, { id: TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw });
                this.remainingDaysRules.push({ id: calcRule.id, name: calcRule.name });
                calcRule = _.find(this.allRemainingDaysRules, { id: TermGroup_VacationGroupRemainingDaysRule.AllRemainingDaysSaved });
                this.remainingDaysRules.push({ id: calcRule.id, name: calcRule.name });
                calcRule = _.find(this.allRemainingDaysRules, { id: TermGroup_VacationGroupRemainingDaysRule.AllRemainingDaysSavedToYear1 });
                this.remainingDaysRules.push({ id: calcRule.id, name: calcRule.name });
                break;
        }
    }

    private dayTypesComplete() {
        this.dirtyHandler.isDirty = true;
        let loadedDayTypes = this.vacationGroup.vacationGroupSE.vacationGroupSEDayTypes;
        this.vacationGroup.vacationGroupSE.vacationGroupSEDayTypes = [];
        if (this.selectedDayTypes.length > 0) {
            this.selectedDayTypes.forEach(s => {
                let loadedDayType = _.find(loadedDayTypes, { dayTypeId: s.id });
                if (loadedDayType != undefined) {
                    this.vacationGroup.vacationGroupSE.vacationGroupSEDayTypes.push(loadedDayType);
                } else {
                    let selectedDayType = _.find(this.dayTypes, { id: s.id });
                    let dayType: IVacationGroupSEDayTypeDTO = {
                        vacationGroupSEDayTypeId: 0,
                        dayTypeId: selectedDayType.id, 
                        vacationGroupSEId: this.vacationGroup.vacationGroupSE.vacationGroupSEId,
                        type: SoeVacationGroupDayType.VacationFiveDaysPerWeek,
                    }
                    this.vacationGroup.vacationGroupSE.vacationGroupSEDayTypes.push(dayType);
                }
            });
        }
        
    }
}
