import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { NotificationService } from "../../../Core/Services/NotificationService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, TermGroup_TimePeriodType, CompanySettingType, TermGroup, TermGroup_MonthlyWorkTimeCalculationType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { IPayrollService } from "../../Payroll/PayrollService";
import { ITimeService } from "../../Time/TimeService";
import { PayrollGroupDTO, ForaColletiveAgrementDTO } from "../../../Common/Models/PayrollGroupDTOs";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { CoreUtility } from "../../../Util/CoreUtility";
import { TimePeriodHeadGridDTO } from "../../../Common/Models/TimePeriodHeadDTO";

export declare type SettingType = boolean | string | number | Date;

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };
    private daysLabel: string;

    // Company settings
    private useOverTimeCompensation: boolean = false;
    private useException2to6InWorkingAgreement: boolean = false;
    private useTravelCompansation: boolean = false;
    private useWorkTimeShiftCompensation: boolean = false;
    private useVacationRights: boolean = false;
    private useGrossNetTimeInStaffing: boolean = false;
    private usePayroll: boolean = false;

    // Lookups
    private timePeriods: TimePeriodHeadGridDTO[] = [];
    private payrollReportsPersonalCategories: SmallGenericType[] = [];
    private payrollReportsWorkTimeCategories: SmallGenericType[] = [];
    private payrollReportsSalaryTypes: SmallGenericType[] = [];
    private payrollFormulas: SmallGenericType[] = [];
    private foraCollectiveAgreements: ForaColletiveAgrementDTO[] = [];
    private kpaAgreementTypes: ISmallGenericType[] = [];
    private kpaBelongings: ISmallGenericType[] = [];
    private skandiaPensionTypes: ISmallGenericType[] = [];
    private skandiaPensionCategories: ISmallGenericType[] = [];
    private gtpAgreementNumbers: ISmallGenericType[] = [];
    private monthlyWorkTimeCalculationTypes: ISmallGenericType[] = [];
    private foraCategories: ISmallGenericType[] = [];
    private bygglosenSalaryType: ISmallGenericType[] = [];

    // Data
    private payrollGroupId: number;
    private payrollGroup: PayrollGroupDTO;
    private payrollPriceFormulas: ISmallGenericType[] = [];
    private jobStatuses: ISmallGenericType[] = [];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private timeService: ITimeService,
        private payrollService: IPayrollService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: NotificationService,
        private focusService: IFocusService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.payrollGroupId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Employee_PayrollGroups_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_PayrollGroups_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_PayrollGroups_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.payrollGroupId, recordId => {
            if (recordId !== this.payrollGroupId) {
                this.payrollGroupId = recordId;
                this.onLoadData();
            }
        });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadTimePeriods(),
            this.loadPayrollPriceFormulas(),
            this.loadJobStatuses(),
            this.loadPayrollReportsPersonalCategories(),
            this.loadPayrollReportsWorkTimeCategories(),
            this.loadPayrollReportsPayrollExportSalaryTypes(),
            this.loadKpaAgreementTypes(),
            this.loadKpaBelongings(),
            this.loadSkandiaPensionTypes(),
            this.loadSkandiaPensionCategories(),
            this.loadGtpAgreementNumbers(),
            this.loadForaCategories(),
            this.loadBygglosenSalaryTypes(),
            this.loadMonthlyWorkTimeCalculationTypes()
        ]).then(() => {
            this.setupForaColletiveAgreements();
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.payrollGroupId) {
            return this.loadPayrollGroup();
        } else {
            this.new();
        }
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.time.days",
            "time.employee.payrollgroup.pricetype.duplicatefromdate",
            "time.employee.payrollgroup.payrollgroup"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.daysLabel = this.terms["core.time.days"].toLocaleLowerCase();
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.PayrollAgreementUseOverTimeCompensation);
        settingTypes.push(CompanySettingType.PayrollAgreementUseException2to6InWorkingAgreement);
        settingTypes.push(CompanySettingType.PayrollAgreementUseTravelCompansation);
        settingTypes.push(CompanySettingType.PayrollAgreementUseWorkTimeShiftCompensation);
        settingTypes.push(CompanySettingType.PayrollAgreementUseVacationRights);
        settingTypes.push(CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing);
        settingTypes.push(CompanySettingType.UsePayroll);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useOverTimeCompensation = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollAgreementUseOverTimeCompensation);
            this.useException2to6InWorkingAgreement = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollAgreementUseException2to6InWorkingAgreement);
            this.useTravelCompansation = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollAgreementUseTravelCompansation);
            this.useWorkTimeShiftCompensation = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollAgreementUseWorkTimeShiftCompensation);
            this.useVacationRights = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollAgreementUseVacationRights);
            this.useGrossNetTimeInStaffing = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing);
            this.usePayroll = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UsePayroll);
        });
    }

    private loadTimePeriods(): ng.IPromise<any> {
        this.timePeriods = [];
        return this.timeService.getTimePeriodHeadsForGrid(TermGroup_TimePeriodType.Payroll, false, false, false).then(x => {
            this.timePeriods = x;
        });
    }

    private loadPayrollPriceFormulas(): ng.IPromise<any> {
        this.payrollPriceFormulas = [];

        return this.payrollService.getPayrollPriceFormulasDict(true).then(x => {
            this.payrollPriceFormulas = x;
        });
    }

    private loadJobStatuses(): ng.IPromise<any> {
        this.jobStatuses = [];

        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsJobStatus, true, false, false).then(x => {
            this.jobStatuses = x;
        });
    }

    private loadPayrollReportsPersonalCategories(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsPersonalCategory, true, true).then(x => {
            this.payrollReportsPersonalCategories = x;
        });
    }

    private loadPayrollReportsWorkTimeCategories(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsWorkTimeCategory, true, true).then(x => {
            this.payrollReportsWorkTimeCategories = x;
        });
    }

    private loadPayrollReportsPayrollExportSalaryTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsSalaryType, true, true).then(x => {
            this.payrollReportsSalaryTypes = x;
        });
    }
    
    private loadBygglosenSalaryTypes(): ng.IPromise<any> {
        this.bygglosenSalaryType = [];

        return this.coreService.getTermGroupContent(TermGroup.BygglosenSalaryType, true, true).then(x => {
            this.bygglosenSalaryType = x;
        });
    }

    private loadKpaAgreementTypes(): ng.IPromise<any> {
        this.kpaAgreementTypes = [];

        return this.coreService.getTermGroupContent(TermGroup.KPAAgreementType, true, true).then(x => {
            this.kpaAgreementTypes = x;
        });
    }

    private loadKpaBelongings(): ng.IPromise<any> {
        this.kpaBelongings = [];

        return this.coreService.getTermGroupContent(TermGroup.KPABelonging, true, true).then(x => {
            this.kpaBelongings = x;
        });
    }

    private loadSkandiaPensionTypes(): ng.IPromise<any> {
        this.skandiaPensionTypes = [];

        return this.coreService.getTermGroupContent(TermGroup.SkandiaPensionType, true, true).then(x => {
            this.skandiaPensionTypes = x;
        });
    }

    private loadSkandiaPensionCategories(): ng.IPromise<any> {
        this.skandiaPensionCategories = [];

        return this.coreService.getTermGroupContent(TermGroup.SkandiaPensionCategory, true, true).then(x => {
            this.skandiaPensionCategories = x;
        });
    }
    private loadGtpAgreementNumbers(): ng.IPromise<any> {
        this.gtpAgreementNumbers = [];

        return this.coreService.getTermGroupContent(TermGroup.GTPAgreementNumber, true, true, true).then(x => {
            x.forEach(y => {
                this.gtpAgreementNumbers.push({ id: y.id, name: y.id > 0 ? "({0}) {1}".format(y.id.toString(), y.name) : y.name });
            });
        });
    }

    private loadForaCategories(): ng.IPromise<any> {
        this.foraCategories = [];

        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsAFACategory, true, true).then(x => {
            this.foraCategories = x;
        });
    }

    private loadMonthlyWorkTimeCalculationTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.MonthlyWorkTimeCalculationType, false, false, true).then(x => {
            this.monthlyWorkTimeCalculationTypes = x;
        });
    }

    private setupForaColletiveAgreements() {
        this.foraCollectiveAgreements = ForaColletiveAgrementDTO.getForColletiveAgrements();
    }

    // SERVICE CALLS

    private loadPayrollGroup(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollGroup(this.payrollGroupId, true, true, true, true, false, true, true, true).then(x => {
                this.isNew = false;
                this.payrollGroup = x;

                if (!this.payrollGroup.monthlyWorkTimeCalculationType)
                    this.payrollGroup.monthlyWorkTimeCalculationType = TermGroup_MonthlyWorkTimeCalculationType.Divisor;

                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.employee.payrollgroup.payrollgroup"] + ' ' + this.payrollGroup.name);
            });
        }]);
    }

    protected copy() {
        super.copy();
        this.isNew = true;
        this.payrollGroupId = 0;
        this.payrollGroup.payrollGroupId = 0;
    }

    private new() {
        this.isNew = true;

        this.payrollGroupId = 0;
        this.payrollGroup = new PayrollGroupDTO();
        this.payrollGroup.actorCompanyId = CoreUtility.actorCompanyId;
        this.payrollGroup.isActive = true;
        this.payrollGroup.accounts = [];
        this.payrollGroup.payrollProducts = [];
        this.payrollGroup.priceFormulas = [];
        this.payrollGroup.priceTypes = [];
        this.payrollGroup.reports = [];
        this.payrollGroup.reportIds = [];
        this.payrollGroup.settings = [];
        this.payrollGroup.vacations = [];

        this.payrollGroup.monthlyWorkTimeCalculationType = TermGroup_MonthlyWorkTimeCalculationType.Divisor;

        this.focusService.focusById("ctrl_payrollgroup_name");
    }

    // ACTIONS

    private initSave() {
        this.save();
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.payrollService.savePayrollGroup(this.payrollGroup).then(result => {
                if (result.success) {
                    if (this.payrollGroupId == 0) {
                        if (this.navigatorRecords) {
                            this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.payrollGroup.name));
                            this.toolbar.setSelectedRecord(result.integerValue);
                        } else {
                            this.reloadNavigationRecords(result.integerValue);
                        }

                    }
                    this.payrollGroupId = result.integerValue;
                    this.payrollGroup.payrollGroupId = this.payrollGroupId;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.payrollGroup);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData();
        }, error => {
        });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.payrollService.getPayrollGroupsGrid(false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.payrollGroupId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.payrollGroupId) {
                    this.payrollGroupId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.payrollService.deletePayrollGroup(this.payrollGroupId).then(result => {
                if (result.success) {
                    completion.completed(this.payrollGroup, true);
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

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.payrollGroup) {
                var errors = this['edit'].$error;
                if (!this.payrollGroup.name)
                    mandatoryFieldKeys.push("common.name");

                if (!this.payrollGroup.timePeriodHeadId)
                    mandatoryFieldKeys.push("time.employee.payrollgroup.timeperiod");

                if (errors['vacationGroup'])
                    mandatoryFieldKeys.push("time.employee.payrollgroup.vacationgroup");

                if (errors['priceTypePeriodFromDate'])
                    validationErrorKeys.push("time.employee.payrollgroup.pricetype.duplicatefromdate");

                if (errors['uniquePriceTypesWithLevel']) {
                    validationErrorKeys.push("time.employee.payrollgroup.pricetype.duplicatepricetypewithlevel");
                }
               
                if (errors['uniquePriceTypes']) {
                    validationErrorKeys.push("time.employee.payrollgroup.pricetype.duplicatepricetype");
                }
            }
        });
    }
}