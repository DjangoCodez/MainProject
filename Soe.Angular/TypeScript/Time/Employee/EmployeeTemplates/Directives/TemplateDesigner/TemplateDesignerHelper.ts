import { IQService } from "angular";
import { EmployeeCollectiveAgreementDTO } from "../../../../../Common/Models/EmployeeCollectiveAgreementDTOs";
import { EmployeeTemplateDTO, EmployeeTemplateGroupRowDTO, TemplateDesignerCheckBoxOptions, TemplateDesignerComponentOptions, TemplateDesignerDatePickerOptions, TemplateDesignerSelectOptions, TemplateDesignerTextAreaOptions, TemplateDesignerTextBoxOptions } from "../../../../../Common/Models/EmployeeTemplateDTOs";
import { ExtraFieldGridDTO } from "../../../../../Common/Models/ExtraFieldDTO";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SoeEntityType, TemplateDesignerComponent, TermGroup, TermGroup_EmployeeTemplateGroupRowType, TermGroup_ExtraFieldType } from "../../../../../Util/CommonEnumerations";
import { CreateFromEmployeeTemplateMode } from "../../../../../Util/Enumerations";
import { IEmployeeService } from "../../../EmployeeService";

export class TemplateDesignerHelper {

    // Terms
    private terms: { [index: string]: string };

    // Lookups
    private collectiveAgreement: EmployeeCollectiveAgreementDTO;
    private extraFieldTypes: ISmallGenericType[] = [];
    private extraFieldsAccount: ExtraFieldGridDTO[] = [];
    public extraFieldsEmployee: ExtraFieldGridDTO[] = [];

    // Properties
    public multiFieldIdCounter = 0;
    public accordionCounter = 0;
    public openAccordions: number[] = [];

    public get isNewEmployeeMode(): boolean {
        return this.mode === CreateFromEmployeeTemplateMode.NewEmployee;
    }

    public get isNewEmploymentMode(): boolean {
        return this.mode === CreateFromEmployeeTemplateMode.NewEmployment;
    }

    constructor(private $q: IQService, private translationService: ITranslationService, private coreService: ICoreService, private employeeService: IEmployeeService, private useAccountHierarchy: boolean, private forceSocialSecNbr: boolean, private employeeTemplate: EmployeeTemplateDTO, private isEditMode: boolean, private mode: CreateFromEmployeeTemplateMode, initializedCallback: () => void) {
        this.$q.all([
            this.loadTerms(),
            this.loadCollectiveAgreement(),
            this.loadExtraFieldTypes(),
            this.loadExtraFieldsAccount(),
            this.loadExtraFieldsEmployee()
        ]).then(() => {
            if (initializedCallback)
                initializedCallback();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys = [
            "core.edit",
            "core.time.placeholder.hoursminutes",
            "time.employee.employeetemplate.grouprow.hideinregistration",
            "time.employee.employeetemplate.grouprow.hideinemploymentregistration",
            "time.employee.employeetemplate.grouprow.hideinreport",
            "time.employee.employeetemplate.grouprow.hideinreportifempty"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadCollectiveAgreement(): ng.IPromise<any> {
        return this.employeeService.getEmployeeCollectiveAgreement(this.employeeTemplate.employeeCollectiveAgreementId).then(x => {
            this.collectiveAgreement = x;
        });
    }

    private loadExtraFieldTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ExtraFieldTypes, false, false).then((x) => {
            this.extraFieldTypes = x;
        });
    }

    private loadExtraFieldsAccount(): ng.IPromise<any> {
        return this.coreService.getExtraFields(SoeEntityType.Account, true).then(x => {
            this.extraFieldsAccount = x;
        });
    }

    private loadExtraFieldsEmployee(): ng.IPromise<any> {
        return this.coreService.getExtraFields(SoeEntityType.Employee, true).then(x => {
            this.extraFieldsEmployee = x;
        });
    }

    public getComponentByType(gRow: EmployeeTemplateGroupRowDTO): TemplateDesignerComponent {
        switch (gRow.type) {
            case TermGroup_EmployeeTemplateGroupRowType.IsSecondaryEmployment:
            case TermGroup_EmployeeTemplateGroupRowType.ControlTaskPartnerInCloseCompany:
            case TermGroup_EmployeeTemplateGroupRowType.ControlTaskBenefitAsPension:
            case TermGroup_EmployeeTemplateGroupRowType.AFAParttimePensionCode:
            case TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDateIsLeaveOfAbsence:
            case TermGroup_EmployeeTemplateGroupRowType.GTPExcluded:
            case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentIgnore:
                return TemplateDesignerComponent.CheckBox;
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate:
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentStopDate:
            case TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDate:
                return TemplateDesignerComponent.DatePicker;
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentType:
            case TermGroup_EmployeeTemplateGroupRowType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment:
            case TermGroup_EmployeeTemplateGroupRowType.ExperienceAgreedOrEstablished:
            case TermGroup_EmployeeTemplateGroupRowType.PayrollFormula:
            case TermGroup_EmployeeTemplateGroupRowType.DisbursementMethod:
            case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsPersonalCategory:
            case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkTimeCategory:
            case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsSalaryType:
            case TermGroup_EmployeeTemplateGroupRowType.AFACategory:
            case TermGroup_EmployeeTemplateGroupRowType.AFASpecialAgreement:
            case TermGroup_EmployeeTemplateGroupRowType.CollectumITPPlan:
            case TermGroup_EmployeeTemplateGroupRowType.KPABelonging:
            case TermGroup_EmployeeTemplateGroupRowType.KPAEndCode:
            case TermGroup_EmployeeTemplateGroupRowType.KPAAgreementType:
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryFormula:
            case TermGroup_EmployeeTemplateGroupRowType.GTPAgreementNumber:
                return TemplateDesignerComponent.Select;
            case TermGroup_EmployeeTemplateGroupRowType.WorkTasks:
            case TermGroup_EmployeeTemplateGroupRowType.SpecialConditions:
                return TemplateDesignerComponent.TextArea;
            case TermGroup_EmployeeTemplateGroupRowType.GeneralText:
                if (this.isEditMode)
                    return TemplateDesignerComponent.TextArea;
                else
                    return TemplateDesignerComponent.Instruction;
            case TermGroup_EmployeeTemplateGroupRowType.FirstName:
            case TermGroup_EmployeeTemplateGroupRowType.LastName:
            case TermGroup_EmployeeTemplateGroupRowType.Name:
            case TermGroup_EmployeeTemplateGroupRowType.SocialSec:
            case TermGroup_EmployeeTemplateGroupRowType.EmployeeNr:
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentWorkTimeWeek:
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentPercent:
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentFullTimeWorkWeek:
            case TermGroup_EmployeeTemplateGroupRowType.PrimaryEmploymentWorkTimeWeek:
            case TermGroup_EmployeeTemplateGroupRowType.TotalEmploymentWorkTimeWeek:
            case TermGroup_EmployeeTemplateGroupRowType.ExperienceMonths:
            case TermGroup_EmployeeTemplateGroupRowType.VacationDaysPayed:
            case TermGroup_EmployeeTemplateGroupRowType.VacationDaysUnpayed:
            case TermGroup_EmployeeTemplateGroupRowType.VacationDaysAdvance:
            case TermGroup_EmployeeTemplateGroupRowType.TaxRate:
            case TermGroup_EmployeeTemplateGroupRowType.Address:
            case TermGroup_EmployeeTemplateGroupRowType.AddressRow:
            case TermGroup_EmployeeTemplateGroupRowType.AddressRow2:
            case TermGroup_EmployeeTemplateGroupRowType.ZipCode:
            case TermGroup_EmployeeTemplateGroupRowType.City:
            case TermGroup_EmployeeTemplateGroupRowType.ZipCity:
            case TermGroup_EmployeeTemplateGroupRowType.Telephone:
            case TermGroup_EmployeeTemplateGroupRowType.Email:
            case TermGroup_EmployeeTemplateGroupRowType.DisbursementAccountNr:
            case TermGroup_EmployeeTemplateGroupRowType.Department:
            case TermGroup_EmployeeTemplateGroupRowType.SubstituteFor:
            case TermGroup_EmployeeTemplateGroupRowType.SubstituteForDueTo:
            case TermGroup_EmployeeTemplateGroupRowType.ExternalCode:
            case TermGroup_EmployeeTemplateGroupRowType.WorkPlace:
            case TermGroup_EmployeeTemplateGroupRowType.CompanyName:
            case TermGroup_EmployeeTemplateGroupRowType.CompanyOrgNr:
            case TermGroup_EmployeeTemplateGroupRowType.CompanyAddress:
            case TermGroup_EmployeeTemplateGroupRowType.CompanyAddressRow:
            case TermGroup_EmployeeTemplateGroupRowType.CompanyAddressRow2:
            case TermGroup_EmployeeTemplateGroupRowType.CompanyZipCode:
            case TermGroup_EmployeeTemplateGroupRowType.CompanyCity:
            case TermGroup_EmployeeTemplateGroupRowType.CompanyZipCity:
            case TermGroup_EmployeeTemplateGroupRowType.CompanyTelephone:
            case TermGroup_EmployeeTemplateGroupRowType.CompanyEmail:
            case TermGroup_EmployeeTemplateGroupRowType.CityAndDate:
            case TermGroup_EmployeeTemplateGroupRowType.SignatureEmployee:
            case TermGroup_EmployeeTemplateGroupRowType.SignatureEmployer:
            case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkPlaceNumber:
            case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsCFARNumber:
            case TermGroup_EmployeeTemplateGroupRowType.ControlTaskWorkPlaceSCB:
            case TermGroup_EmployeeTemplateGroupRowType.AFAWorkplaceNr:
            case TermGroup_EmployeeTemplateGroupRowType.CollectumAgreedOnProduct:
            case TermGroup_EmployeeTemplateGroupRowType.CollectumCostPlace:
            case TermGroup_EmployeeTemplateGroupRowType.KPARetirementAge:
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreementArea:
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenAllocationNumber:
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenMunicipalCode:
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenProfessionCategory:
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryType:
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenWorkPlaceNumber:
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenLendedToOrgNr:
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreedHourlyPayLevel:
            case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentAddress:
            case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentCity:
            case TermGroup_EmployeeTemplateGroupRowType.TaxTinNumber:
            case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCode:
            case TermGroup_EmployeeTemplateGroupRowType.TaxBirthPlace:
            case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCodeBirthPlace:
            case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCodeCitizen:
                return TemplateDesignerComponent.TextBox;
            case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount:
            case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee: {
                let extraField: ExtraFieldGridDTO;
                switch (gRow.type) {
                    case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount:
                        extraField = this.extraFieldsAccount.find(f => f.extraFieldId === gRow.recordId);
                        break;
                    case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee:
                        extraField = this.extraFieldsEmployee.find(f => f.extraFieldId === gRow.recordId);
                        break;
                }
                if (extraField) {
                    const fieldType = this.extraFieldTypes.find(f => f.id === extraField.type);
                    if (fieldType) {
                        switch (fieldType.id as TermGroup_ExtraFieldType) {
                            case TermGroup_ExtraFieldType.FreeText:
                            case TermGroup_ExtraFieldType.Integer:
                            case TermGroup_ExtraFieldType.Decimal:
                                return TemplateDesignerComponent.TextBox;
                            case TermGroup_ExtraFieldType.YesNo:
                            case TermGroup_ExtraFieldType.SingleChoice:
                                return TemplateDesignerComponent.Select;
                            case TermGroup_ExtraFieldType.Checkbox:
                                return TemplateDesignerComponent.CheckBox;
                            case TermGroup_ExtraFieldType.Date:
                                return TemplateDesignerComponent.DatePicker;
                        }
                    }
                }
                return TemplateDesignerComponent.TextBox;
            }
            case TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount:
                return TemplateDesignerComponent.EmployeeAccount;
            case TermGroup_EmployeeTemplateGroupRowType.DisbursementAccount:
                return TemplateDesignerComponent.DisbursementAccount;
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentPriceTypes:
                return TemplateDesignerComponent.EmploymentPriceTypes;
            case TermGroup_EmployeeTemplateGroupRowType.Position:
                return TemplateDesignerComponent.Position;
            default:
                return TemplateDesignerComponent.TextBox;
        }
    }

    public getComponentOptionsByType(gRow: EmployeeTemplateGroupRowDTO): TemplateDesignerComponentOptions {
        const options = new TemplateDesignerComponentOptions();

        if (this.isFieldSystemRequired(gRow.type))
            options.systemRequired = true;

        switch (gRow.type) {
            case TermGroup_EmployeeTemplateGroupRowType.FirstName:
                options.model = 'firstName';
                options.required = true;
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.LastName:
                options.model = 'lastName';
                options.required = true;
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.Name:
                options.model = 'name';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 200;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.SocialSec:
                options.model = 'socialSec';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 15;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.EmployeeNr:
                options.model = 'employeeNr';
                options.required = true;
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 50;
                (options as TemplateDesignerTextBoxOptions).alignRight = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate:
                options.model = 'employmentStartDate';
                options.required = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentStopDate:
                options.model = 'employmentStopDate';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentType:
                options.model = 'employmentType';
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.employmentTypes';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentWorkTimeWeek:
                options.model = 'employmentWorkTimeWeekFormatted';
                (options as TemplateDesignerTextBoxOptions).isTime = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentPercent:
                options.model = 'employmentPercent';
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                (options as TemplateDesignerTextBoxOptions).decimals = 2;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.IsSecondaryEmployment:
                options.model = 'isSecondaryEmployment';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment:
                options.model = 'excludeFromWorkTimeWeekCalculationOnSecondaryEmployment';
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.excludeFromWorkTimeWeekCalculationItems';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.PrimaryEmploymentWorkTimeWeek:
                options.model = 'primaryEmploymentWorkTimeWeekFormatted';
                (options as TemplateDesignerTextBoxOptions).isTime = true;
                (options as TemplateDesignerTextBoxOptions).readOnly = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentFullTimeWorkWeek:
                options.model = 'employmentFullTimeWorkWeekFormatted';
                (options as TemplateDesignerTextBoxOptions).isTime = true;
                (options as TemplateDesignerTextBoxOptions).readOnly = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.TotalEmploymentWorkTimeWeek:
                options.model = 'totalEmploymentWorkTimeWeekFormatted';
                (options as TemplateDesignerTextBoxOptions).isTime = true;
                (options as TemplateDesignerTextBoxOptions).readOnly = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.ExperienceMonths:
                options.model = 'experienceMonths';
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                (options as TemplateDesignerTextBoxOptions).decimals = 0;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.ExperienceAgreedOrEstablished:
                options.model = 'experienceAgreedOrEstablished';
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.experiences';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.VacationDaysPayed:
                options.model = 'vacationDaysPayed';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                (options as TemplateDesignerTextBoxOptions).decimals = 0;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.VacationDaysUnpayed:
                options.model = 'vacationDaysUnpayed';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                (options as TemplateDesignerTextBoxOptions).decimals = 0;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.VacationDaysAdvance:
                options.model = 'vacationDaysAdvance';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                (options as TemplateDesignerTextBoxOptions).decimals = 0;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.TaxRate:
                options.model = 'taxRate';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                (options as TemplateDesignerTextBoxOptions).decimals = 0;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.EmploymentPriceTypes:
                options.model = 'employmentPriceTypes';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.PayrollFormula:
                options.model = `payrollFormula_${this.getMultiFieldId(gRow)}`;
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.payrollPriceFormulas';
                options.isMultiple = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.Address:
                options.model = 'address';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 50;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.AddressRow:
                options.model = 'addressRow';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.AddressRow2:
                options.model = 'addressRow2';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.ZipCode:
                options.model = 'zipCode';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.City:
                options.model = 'city';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.ZipCity:
                options.model = 'zipCity';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.Telephone:
                options.model = 'telephone';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.Email:
                options.model = 'email';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.DisbursementMethod:
                options.model = 'disbursementMethod';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.disbursementMethods';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.DisbursementAccountNr:
                options.model = 'disbursementAccountNr';
                (options as TemplateDesignerTextBoxOptions).maxLength = 200;
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.DisbursementAccount:
                options.model = 'disbursementAccount';
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount:
                options.model = 'hierarchicalAccount';
                if (this.useAccountHierarchy)
                    options.required = true;
                options.systemHideInRegistration = !this.useAccountHierarchy;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.Position:
                options.model = 'position';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.WorkTasks:
                options.model = 'workTasks';
                (options as TemplateDesignerTextAreaOptions).rows = 3;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.Department:
                options.model = 'department';
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.SpecialConditions:
                options.model = 'specialConditions';
                (options as TemplateDesignerTextAreaOptions).rows = 3;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.SubstituteFor:
                options.model = 'substituteFor';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.SubstituteForDueTo:
                options.model = 'substituteForDueTo';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.ExternalCode:
                options.model = 'externalCode';
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.WorkPlace:
                options.model = 'workPlace';
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CompanyName:
                options.model = 'companyName';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CompanyOrgNr:
                options.model = 'companyOrgNr';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CompanyAddress:
                options.model = 'companyAddress';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CompanyAddressRow:
                options.model = 'companyAddressRow';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CompanyAddressRow2:
                options.model = 'companyAddressRow2';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CompanyZipCode:
                options.model = 'companyZipCode';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CompanyCity:
                options.model = 'companyCity';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CompanyZipCity:
                options.model = 'companyZipCity';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CompanyTelephone:
                options.model = 'companyTelephone';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CompanyEmail:
                options.model = 'companyEmail';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CityAndDate:
                options.model = 'cityAndDate';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.SignatureEmployee:
                options.model = 'signatureEmployee';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.SignatureEmployer:
                options.model = 'signatureEmployer';
                options.readOnly = true;
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsPersonalCategory:
                options.model = 'payrollStatisticsPersonalCategory';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.payrollReportsPersonalCategories';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkTimeCategory:
                options.model = 'payrollStatisticsWorkTimeCategory';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.payrollReportsWorkTimeCategories';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsSalaryType:
                options.model = 'payrollStatisticsSalaryType';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.payrollReportsPayrollExportSalaryTypes';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkPlaceNumber:
                options.model = 'payrollStatisticsWorkPlaceNumber';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                break
            case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsCFARNumber:
                options.model = 'payrollStatisticsCFARNumber';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.ControlTaskWorkPlaceSCB:
                options.model = 'controlTaskWorkPlaceSCB';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.ControlTaskPartnerInCloseCompany:
                options.model = 'controlTaskPartnerInCloseCompany';
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.ControlTaskBenefitAsPension:
                options.model = 'controlTaskBenefitAsPension';
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.AFACategory:
                options.model = 'afaCategory';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.payrollReportsAFACategories';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.AFASpecialAgreement:
                options.model = 'afaSpecialAgreement';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.payrollReportsAFASpecialAgreements';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.AFAWorkplaceNr:
                options.model = 'afaWorkplaceNr';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.AFAParttimePensionCode:
                options.model = 'afaParttimePensionCode';
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CollectumITPPlan:
                options.model = 'collectumITPPlan';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.payrollReportsCollectumITPplans';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CollectumCostPlace:
                options.model = 'collectumCostPlace';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CollectumAgreedOnProduct:
                options.model = 'collectumAgreedOnProduct';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDateIsLeaveOfAbsence:
                options.model = 'collectumCancellationDateIsLeaveOfAbsence';
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDate:
                options.model = 'collectumCancellationDate';
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.KPARetirementAge:
                options.model = 'kpaRetirementAge';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                (options as TemplateDesignerTextBoxOptions).decimals = 0;
                (options as TemplateDesignerTextBoxOptions).maxLength = 2;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.KPABelonging:
                options.model = 'kpaBelonging';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.kpaBelongings';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.KPAEndCode:
                options.model = 'kpaEndCode';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.kpaEndCodes';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.KPAAgreementType:
                options.model = 'kpaAgreementType';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.kpaAgreementTypes';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreementArea:
                options.model = 'bygglosenAgreementArea';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 10;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenAllocationNumber:
                options.model = 'bygglosenAllocationNumber';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 10;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenMunicipalCode:
                options.model = 'bygglosenMunicipalCode';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 10;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryFormula:
                options.model = 'bygglosenSalaryFormula';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.payrollPriceFormulas';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryType:
                options.model = 'bygglosenSalaryType';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.bygglosenSalaryType';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenProfessionCategory:
                options.model = 'bygglosenProfessionCategory';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 10;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenWorkPlaceNumber:
                options.model = 'bygglosenWorkPlaceNumber';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenLendedToOrgNr:
                options.model = 'bygglosenLendedToOrgNr';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 10;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreedHourlyPayLevel:
                options.model = 'bygglosenAgreedHourlyPayLevel';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).numeric = true;
                (options as TemplateDesignerTextBoxOptions).decimals = 2;
                (options as TemplateDesignerTextBoxOptions).maxLength = 10;
                break;

            case TermGroup_EmployeeTemplateGroupRowType.GTPAgreementNumber:
                options.model = 'gtpAgreementNumber';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.gtpAgreementNumbers';
                break;
            case TermGroup_EmployeeTemplateGroupRowType.GTPExcluded:
                options.model = 'gtpExcluded';
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentAddress:
                options.model = 'agiPlaceOfEmploymentAddress';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentCity:
                options.model = 'agiPlaceOfEmploymentCity';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 100;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentIgnore:
                options.model = 'agiPlaceOfEmploymentIgnore';
                options.systemHideInEmploymentRegistration = true;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.TaxTinNumber:
                options.model = 'taxTinNumber';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 50;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCode:
                options.model = 'taxCountryCode';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 5;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.TaxBirthPlace:
                options.model = 'taxBirthPlace';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 50;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCodeBirthPlace:
                options.model = 'taxCountryCodeBirthPlace';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 50;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCodeCitizen:
                options.model = 'taxCountryCodeCitizen';
                options.systemHideInEmploymentRegistration = true;
                (options as TemplateDesignerTextBoxOptions).maxLength = 50;
                break;
            case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount:
            case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee: {
                let extraField: ExtraFieldGridDTO;
                switch (gRow.type) {
                    case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount:
                        options.model = `extraFieldAccount_${this.getMultiFieldId(gRow)}`;
                        options.isMultiple = true;
                        extraField = this.extraFieldsAccount.find(f => f.extraFieldId === gRow.recordId);
                        break;
                    case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee:
                        options.model = `extraFieldEmployee_${this.getMultiFieldId(gRow)}`;
                        options.isMultiple = true;
                        extraField = this.extraFieldsEmployee.find(f => f.extraFieldId === gRow.recordId);
                        break;
                }
                if (extraField) {
                    const fieldType = this.extraFieldTypes.find(f => f.id === extraField.type);
                    if (fieldType) {
                        switch (fieldType.id as TermGroup_ExtraFieldType) {
                            case TermGroup_ExtraFieldType.FreeText:
                                break;
                            case TermGroup_ExtraFieldType.Integer:
                                (options as TemplateDesignerTextBoxOptions).numeric = true;
                                (options as TemplateDesignerTextBoxOptions).decimals = 0;
                                break;
                            case TermGroup_ExtraFieldType.Decimal:
                                (options as TemplateDesignerTextBoxOptions).numeric = true;
                                (options as TemplateDesignerTextBoxOptions).decimals = 2;
                                break;
                            case TermGroup_ExtraFieldType.YesNo:
                                (options as TemplateDesignerSelectOptions).itemsName = 'ctrl.yesNoItems';
                                break;
                            case TermGroup_ExtraFieldType.Checkbox:
                                break;
                            case TermGroup_ExtraFieldType.Date:
                                break;
                            case TermGroup_ExtraFieldType.SingleChoice:
                                (options as TemplateDesignerSelectOptions).itemIdField = 'extraFieldValueId';
                                (options as TemplateDesignerSelectOptions).itemNameField = 'value';
                                (options as TemplateDesignerSelectOptions).itemsName = `ctrl.getExtraFieldEmployeeItems(${extraField.extraFieldId})`;
                                break;
                        }
                    }
                }

                break;
            }
            case TermGroup_EmployeeTemplateGroupRowType.GeneralText:
                options.model = `generalText_${this.getMultiFieldId(gRow)}`;
                options.systemHideInEmploymentRegistration = true;
                options.isMultiple = true;
                break;
        }

        return options;
    }

    private getMultiFieldId(gRow: EmployeeTemplateGroupRowDTO): number {
        return gRow.employeeTemplateGroupRowId ? gRow.employeeTemplateGroupRowId : ++this.multiFieldIdCounter;
    }

    public getTypeByFieldName(fieldName: string): TermGroup_EmployeeTemplateGroupRowType {
        fieldName = fieldName.replace('ctrl_employee_', '');

        switch (fieldName) {
            case 'firstName':
                return TermGroup_EmployeeTemplateGroupRowType.FirstName;
            case 'lastName':
                return TermGroup_EmployeeTemplateGroupRowType.LastName;
            case 'socialSec':
                return TermGroup_EmployeeTemplateGroupRowType.SocialSec;
            case 'employeeNr':
                return TermGroup_EmployeeTemplateGroupRowType.EmployeeNr;
            case 'employmentStartDate':
                return TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate;
            case 'employmentStopDate':
                return TermGroup_EmployeeTemplateGroupRowType.EmploymentStopDate;
            case 'employmentType':
                return TermGroup_EmployeeTemplateGroupRowType.EmploymentType;
            case 'employmentWorkTimeWeek':
                return TermGroup_EmployeeTemplateGroupRowType.EmploymentWorkTimeWeek;
            case 'employmentPercent':
                return TermGroup_EmployeeTemplateGroupRowType.EmploymentPercent;
            case 'isSecondaryEmployment':
                return TermGroup_EmployeeTemplateGroupRowType.IsSecondaryEmployment;
            case 'excludeFromWorkTimeWeekCalculationOnSecondaryEmployment':
                return TermGroup_EmployeeTemplateGroupRowType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment;
            case 'primaryEmploymentWorkTimeWeek':
                return TermGroup_EmployeeTemplateGroupRowType.PrimaryEmploymentWorkTimeWeek;
            case 'employmentFullTimeWorkWeek':
                return TermGroup_EmployeeTemplateGroupRowType.EmploymentFullTimeWorkWeek;
            case 'totalEmploymentWorkTimeWeek':
                return TermGroup_EmployeeTemplateGroupRowType.TotalEmploymentWorkTimeWeek;
            case 'experienceMonths':
                return TermGroup_EmployeeTemplateGroupRowType.ExperienceMonths;
            case 'experienceAgreedOrEstablished':
                return TermGroup_EmployeeTemplateGroupRowType.ExperienceAgreedOrEstablished;
            case 'vacationDaysPayed':
                return TermGroup_EmployeeTemplateGroupRowType.VacationDaysPayed;
            case 'vacationDaysUnpayed':
                return TermGroup_EmployeeTemplateGroupRowType.VacationDaysUnpayed;
            case 'vacationDaysAdvance':
                return TermGroup_EmployeeTemplateGroupRowType.VacationDaysAdvance;
            case 'taxRate':
                return TermGroup_EmployeeTemplateGroupRowType.TaxRate;
            case 'employmentPriceTypes':
                return TermGroup_EmployeeTemplateGroupRowType.EmploymentPriceTypes;
            case 'address':
                return TermGroup_EmployeeTemplateGroupRowType.Address;
            case 'addressRow':
                return TermGroup_EmployeeTemplateGroupRowType.AddressRow;
            case 'addressRow2':
                return TermGroup_EmployeeTemplateGroupRowType.AddressRow2;
            case 'zipCode':
                return TermGroup_EmployeeTemplateGroupRowType.ZipCode;
            case 'city':
                return TermGroup_EmployeeTemplateGroupRowType.City;
            case 'telephone':
                return TermGroup_EmployeeTemplateGroupRowType.Telephone;
            case 'email':
                return TermGroup_EmployeeTemplateGroupRowType.Email;
            case 'disbursementMethod':
                return TermGroup_EmployeeTemplateGroupRowType.DisbursementMethod;
            case 'disbursementAccountNr':
                return TermGroup_EmployeeTemplateGroupRowType.DisbursementAccountNr;
            case 'disbursementAccount':
                return TermGroup_EmployeeTemplateGroupRowType.DisbursementAccount;
            case 'hierarchicalAccount':
                return TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount;
            case 'position':
                return TermGroup_EmployeeTemplateGroupRowType.Position;
            case 'workTasks':
                return TermGroup_EmployeeTemplateGroupRowType.WorkTasks;
            case 'department':
                return TermGroup_EmployeeTemplateGroupRowType.Department;
            case 'specialConditions':
                return TermGroup_EmployeeTemplateGroupRowType.SpecialConditions;
            case 'substituteFor':
                return TermGroup_EmployeeTemplateGroupRowType.SubstituteFor;
            case 'substituteForDueTo':
                return TermGroup_EmployeeTemplateGroupRowType.SubstituteForDueTo;
            case 'externalCode':
                return TermGroup_EmployeeTemplateGroupRowType.ExternalCode;
            case 'workPlace':
                return TermGroup_EmployeeTemplateGroupRowType.WorkPlace;
            case 'payrollStatisticsPersonalCategory':
                return TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsPersonalCategory;
            case 'payrollStatisticsWorkTimeCategory':
                return TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkTimeCategory;
            case 'payrollStatisticsSalaryType':
                return TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsSalaryType;
            case 'payrollStatisticsWorkPlaceNumber':
                return TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkPlaceNumber;
            case 'payrollStatisticsCFARNumber':
                return TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsCFARNumber;
            case 'controlTaskWorkPlaceSCB':
                return TermGroup_EmployeeTemplateGroupRowType.ControlTaskWorkPlaceSCB;
            case 'controlTaskPartnerInCloseCompany':
                return TermGroup_EmployeeTemplateGroupRowType.ControlTaskPartnerInCloseCompany;
            case 'controlTaskBenefitAsPension':
                return TermGroup_EmployeeTemplateGroupRowType.ControlTaskBenefitAsPension;
            case 'afaCategory':
                return TermGroup_EmployeeTemplateGroupRowType.AFACategory;
            case 'afaSpecialAgreement':
                return TermGroup_EmployeeTemplateGroupRowType.AFASpecialAgreement;
            case 'afaWorkplaceNr':
                return TermGroup_EmployeeTemplateGroupRowType.AFAWorkplaceNr;
            case 'afaParttimePensionCode':
                return TermGroup_EmployeeTemplateGroupRowType.AFAParttimePensionCode;
            case 'kpaRetirementAge':
                return TermGroup_EmployeeTemplateGroupRowType.KPARetirementAge;
            case 'kpaBelonging':
                return TermGroup_EmployeeTemplateGroupRowType.KPABelonging;
            case 'kpaEndCode':
                return TermGroup_EmployeeTemplateGroupRowType.KPAEndCode;
            case 'kpaAgreementType':
                return TermGroup_EmployeeTemplateGroupRowType.KPAAgreementType;
            case 'bygglosenAgreementArea':
                return TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreementArea;
            case 'bygglosenAllocationNumber':
                return TermGroup_EmployeeTemplateGroupRowType.BygglosenAllocationNumber;
            case 'bygglosenSalaryFormula':
                return TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryFormula;
            case 'bygglosenMunicipalCode':
                return TermGroup_EmployeeTemplateGroupRowType.BygglosenMunicipalCode;
            case 'bygglosenProfessionCategory':
                return TermGroup_EmployeeTemplateGroupRowType.BygglosenProfessionCategory;
            case 'bygglosenSalaryType':
                return TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryType;
            case 'bygglosenWorkPlaceNumber':
                return TermGroup_EmployeeTemplateGroupRowType.BygglosenWorkPlaceNumber;
            case 'bygglosenLendedToOrgNr':
                return TermGroup_EmployeeTemplateGroupRowType.BygglosenLendedToOrgNr;
            case 'bygglosenAgreedHourlyPayLevel':
                return TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreedHourlyPayLevel;
            case 'gtpAgreementNumber':
                return TermGroup_EmployeeTemplateGroupRowType.GTPAgreementNumber;
            case 'gtpExcluded':
                return TermGroup_EmployeeTemplateGroupRowType.GTPExcluded;
            case 'agiPlaceOfEmploymentAddress':
                return TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentAddress;
            case 'agiPlaceOfEmploymentCity':
                return TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentCity;
            case 'agiPlaceOfEmploymentIgnore':
                return TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentIgnore;
            case 'taxTinNumber':
                return TermGroup_EmployeeTemplateGroupRowType.TaxTinNumber;
            case 'taxCountryCode':
                return TermGroup_EmployeeTemplateGroupRowType.TaxCountryCode;
            case 'taxBirthPlace':
                return TermGroup_EmployeeTemplateGroupRowType.TaxBirthPlace;
            case 'taxCountryCodeBirthPlace':
                return TermGroup_EmployeeTemplateGroupRowType.TaxCountryCodeBirthPlace;
            case 'taxCountryCodeCitizen':
                return TermGroup_EmployeeTemplateGroupRowType.TaxCountryCodeCitizen;
        }

        if (fieldName.startsWith('payrollFormula'))
            return TermGroup_EmployeeTemplateGroupRowType.PayrollFormula;
        else if (fieldName.startsWith('generalText'))
            return TermGroup_EmployeeTemplateGroupRowType.GeneralText;
        else if (fieldName.startsWith('extraFieldAccount'))
            return TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount;
        else if (fieldName.startsWith('extraFieldEmployee'))
            return TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee;

        return TermGroup_EmployeeTemplateGroupRowType.Unknown;
    }

    public get systemRequiredFields(): TermGroup_EmployeeTemplateGroupRowType[] {
        const fields: TermGroup_EmployeeTemplateGroupRowType[] = [];

        fields.push(TermGroup_EmployeeTemplateGroupRowType.FirstName);
        fields.push(TermGroup_EmployeeTemplateGroupRowType.LastName);
        if (this.forceSocialSecNbr)
            fields.push(TermGroup_EmployeeTemplateGroupRowType.SocialSec);
        fields.push(TermGroup_EmployeeTemplateGroupRowType.EmployeeNr);
        fields.push(TermGroup_EmployeeTemplateGroupRowType.Email);
        fields.push(TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate);
        fields.push(TermGroup_EmployeeTemplateGroupRowType.DisbursementAccount);
        if (this.useAccountHierarchy)
            fields.push(TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount);

        return fields;
    }

    public isFieldSystemRequired(field: TermGroup_EmployeeTemplateGroupRowType): boolean {
        return _.includes(this.systemRequiredFields, field);
    }

    public get extraFieldAccountLinkableFields(): TermGroup_EmployeeTemplateGroupRowType[] {
        const fields: TermGroup_EmployeeTemplateGroupRowType[] = [];

        if (this.useAccountHierarchy) {
            fields.push(TermGroup_EmployeeTemplateGroupRowType.Department);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.WorkPlace);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkPlaceNumber);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsCFARNumber);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.ControlTaskWorkPlaceSCB);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.AFAWorkplaceNr);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.CollectumAgreedOnProduct);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.CollectumCostPlace);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.KPARetirementAge);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreementArea);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.BygglosenAllocationNumber);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.BygglosenMunicipalCode);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentAddress);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentCity);
            fields.push(TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentIgnore);
        }

        return fields;
    }

    public isFieldExtraFieldAccountLinkable(field: TermGroup_EmployeeTemplateGroupRowType): boolean {
        return _.includes(this.extraFieldAccountLinkableFields, field);
    }

    public createRow(): HTMLDivElement {
        const row = document.createElement('div');
        row.classList.add('row');
        return row;
    }

    public createCol(width: number, offset = 0): HTMLDivElement {
        const col = document.createElement('div');
        col.classList.add('col-sm-' + width * 3);
        if (offset > 0)
            col.classList.add('col-sm-offset-' + offset * 3);
        return col;
    }

    public createSpan(text = '') {
        const span = document.createElement('span');
        if (text)
            span.innerHTML = text;
        return span;
    }

    public createPageBreakIconRow() {
        const row = this.createRow();
        const col = this.createCol(4);
        row.appendChild(col);

        const icon = this.createIcon('fa-page-break', false);
        col.appendChild(icon);

        return row;
    }

    private createIcon(iconName: string, clickable: boolean, additionalClasses: string[] = []) {
        const icon = document.createElement('div');
        icon.classList.add('fal');
        icon.classList.add(iconName);

        if (clickable)
            icon.classList.add('link');

        if (additionalClasses.length > 0) {
            additionalClasses.forEach(cls => {
                icon.classList.add(cls);
            });
        }

        return icon;
    }

    public createGroupPlaceholder(id: number, label: string) {
        const div = document.createElement('div');
        div.id = `group_${this.employeeTemplate.employeeTemplateId}_${id}`;

        if (this.isEditMode)
            div.classList.add('group-placeholder');

        const uib = document.createElement('uib-accordion');
        uib.setAttribute('close-others', 'false');

        const accordion = document.createElement('soe-accordion');
        this.accordionCounter++;
        this[`accordion${this.accordionCounter}isopen`] = _.includes(this.openAccordions, this.accordionCounter);
        accordion.setAttribute('is-open', `ctrl.accordion${this.accordionCounter}isopen`);

        accordion.setAttribute('label-value', `'${label}'`);
        if (this.isEditMode) {
            accordion.setAttribute('data-ng-click', `ctrl.selectGroup(${id});`);
            accordion.setAttribute('data-ng-dblclick', `$event.stopPropagation(); ctrl.doubleClickToEdit(${id}, true);`);
        }
        uib.appendChild(accordion);
        div.appendChild(uib);

        if (this.isEditMode) {
            const editIcon = this.createIcon('fa-pen', true, ['group-placeholder-icon', 'iconEdit']);
            editIcon.setAttribute('data-ng-if', `ctrl.selectedGroupId === ${id};`)
            editIcon.setAttribute('data-ng-click', `ctrl.editGroup(${id});`);
            div.appendChild(editIcon);
        }

        return div;
    }

    public createFieldPlaceholder(groupId: number, id: number, hideInRegistration: boolean, hideInEmploymentRegistration: boolean, hideInReport: boolean, hideInReportIfEmpty: boolean, addIconMargin: boolean = false): HTMLDivElement {
        const div = document.createElement('div');
        div.id = `field_${this.employeeTemplate.employeeTemplateId}_${id}`
        div.setAttribute('groupId', groupId.toString());

        if (this.isEditMode) {
            div.classList.add('field-placeholder');

            const editIcon = this.createIcon('fa-pen', true, [addIconMargin ? 'field-placeholder-icon-with-margin' : 'field-placeholder-icon', 'iconEdit']);
            editIcon.setAttribute('data-ng-if', `ctrl.selectedFieldId === ${id};`)
            editIcon.setAttribute('data-ng-click', `ctrl.editField(${id});`);
            editIcon.title = this.terms["core.edit"];
            div.appendChild(editIcon);

            let iconCounter = 1;
            if (hideInRegistration)
                iconCounter++;
            if (hideInEmploymentRegistration)
                iconCounter++;
            if (hideInReport)
                iconCounter++;
            if (hideInReportIfEmpty)
                iconCounter++;

            if (hideInRegistration) {
                const icon = document.createElement('div');
                icon.classList.add(addIconMargin ? 'field-placeholder-icon-with-margin' : 'field-placeholder-icon');
                icon.classList.add(this.iconCounterToClass(iconCounter--));
                icon.classList.add('fal');
                icon.classList.add('fa-user-slash');
                icon.title = this.terms["time.employee.employeetemplate.grouprow.hideinregistration"];
                div.appendChild(icon);
            }

            if (hideInEmploymentRegistration) {
                const icon = document.createElement('div');
                icon.classList.add(addIconMargin ? 'field-placeholder-icon-with-margin' : 'field-placeholder-icon');
                icon.classList.add(this.iconCounterToClass(iconCounter--));
                icon.classList.add('fal');
                icon.classList.add('fa-users-slash');
                icon.title = this.terms["time.employee.employeetemplate.grouprow.hideinemploymentregistration"];
                div.appendChild(icon);
            }

            if (hideInReport) {
                const icon = document.createElement('div');
                icon.classList.add(addIconMargin ? 'field-placeholder-icon-with-margin' : 'field-placeholder-icon');
                icon.classList.add(this.iconCounterToClass(iconCounter--));
                icon.classList.add('fal');
                icon.classList.add('fa-print-slash');
                icon.title = this.terms["time.employee.employeetemplate.grouprow.hideinreport"];
                div.appendChild(icon);
            }

            if (hideInReportIfEmpty) {
                const icon = document.createElement('div');
                icon.classList.add(addIconMargin ? 'field-placeholder-icon-with-margin' : 'field-placeholder-icon');
                icon.classList.add(this.iconCounterToClass(iconCounter--));
                icon.classList.add('fal');
                icon.classList.add('fa-file');
                icon.title = this.terms["time.employee.employeetemplate.grouprow.hideinreportifempty"];
                div.appendChild(icon);
            }
        }

        return div;
    }

    private iconCounterToClass(iconCounter: number): string {
        if (iconCounter === 2)
            return 'second';
        else if (iconCounter === 3)
            return 'third';
        else if (iconCounter === 4)
            return 'fourth';
        else if (iconCounter === 5)
            return 'fifth';

        return '';
    }

    public createCheckBox(groupId: number, id: number, label: string, hideInRegistration: boolean, hideInEmploymentRegistration: boolean, hideInReport: boolean, hideInReportIfEmpty: boolean, options?: TemplateDesignerCheckBoxOptions) {
        const div = this.createFieldPlaceholder(groupId, id, hideInRegistration, hideInEmploymentRegistration, hideInReport, hideInReportIfEmpty);

        const elem = document.createElement('soe-checkbox');
        this.setCommonAttributes(elem, groupId, id, false);

        elem.setAttribute('label', `${label}`);
        elem.setAttribute('indiscreet', 'true');
        elem.setAttribute('inline', 'true');
        elem.setAttribute('on-changing', 'ctrl.setDirty();');

        if (options) {
            this.setCommonOptions(div, elem, options);
        }

        div.appendChild(elem);

        return div;
    }

    public createDatePicker(groupId: number, id: number, label: string, isMandatory: boolean, hideInRegistration: boolean, hideInEmploymentRegistration: boolean, hideInReport: boolean, hideInReportIfEmpty: boolean, options?: TemplateDesignerDatePickerOptions) {
        const div = this.createFieldPlaceholder(groupId, id, hideInRegistration, hideInEmploymentRegistration, hideInReport, hideInReportIfEmpty);

        const elem = document.createElement('soe-datepicker');
        this.setCommonAttributes(elem, groupId, id, isMandatory);

        // Special requirement for employmentStopDate
        // Required if not employment type is permanent
        if (options.model === 'employmentStopDate') {
            // This will make the * appear
            // Actual validation is done in CreateFromTemplateValidationDirective
            elem.setAttribute('is-required', 'ctrl.employee.employmentStopDateRequired');
        }

        elem.setAttribute('label-value', `'${label} ${this.isEditMode && isMandatory ? '*' : ''}'`);
        elem.setAttribute('label-value-indiscreet', 'true');
        elem.setAttribute('on-change', 'ctrl.setDirty();');

        if (options) {
            this.setCommonOptions(div, elem, options);
        }

        div.appendChild(elem);

        return div;
    }

    public createInstruction(groupId: number, id: number, text: string, label?: string) {
        const div = this.createFieldPlaceholder(groupId, id, false, false, false, false);

        if (label) {
            div.appendChild(this.createLabel(label));
        }

        const elem = document.createElement('soe-instruction');
        elem.setAttribute('model', `'${text}'`);
        elem.setAttribute('inline', 'true');
        elem.setAttribute('full-width', 'true');

        div.appendChild(elem);

        return div;
    }

    public createLabel(label: string) {
        const elem = document.createElement('soe-label');
        elem.setAttribute('label-value', `'${label}'`);
        elem.setAttribute('label-value-indiscreet', 'true');

        return elem;
    }

    public createSelect(groupId: number, id: number, label: string, isMandatory: boolean, hideInRegistration: boolean, hideInEmploymentRegistration: boolean, hideInReport: boolean, hideInReportIfEmpty: boolean, options?: TemplateDesignerSelectOptions) {
        const div = this.createFieldPlaceholder(groupId, id, hideInRegistration, hideInEmploymentRegistration, hideInReport, hideInReportIfEmpty);

        const elem = document.createElement('soe-select');
        this.setCommonAttributes(elem, groupId, id, isMandatory);

        elem.setAttribute('label-value', `'${label} ${this.isEditMode && isMandatory ? '*' : ''}'`);
        elem.setAttribute('label-value-indiscreet', 'true');
        elem.setAttribute('on-changing', `ctrl.selectChanged('${options.model}');`);

        if (options) {
            this.setCommonOptions(div, elem, options);

            if (options.itemsName) {
                let itemIdField = options.itemIdField || 'id';
                let itemNameField = options.itemNameField || 'name';
                elem.setAttribute('options', `item.${itemIdField} as item.${itemNameField} for item in items`);
                elem.setAttribute('items', options.itemsName);
            }
        }

        div.appendChild(elem);

        return div;
    }

    public createTextArea(groupId: number, id: number, label: string, isMandatory: boolean, hideInRegistration: boolean, hideInEmploymentRegistration: boolean, hideInReport: boolean, hideInReportIfEmpty: boolean, options?: TemplateDesignerTextAreaOptions) {
        const div = this.createFieldPlaceholder(groupId, id, hideInRegistration, hideInEmploymentRegistration, hideInReport, hideInReportIfEmpty, true);

        const elem = document.createElement('soe-textarea');
        this.setCommonAttributes(elem, groupId, id, isMandatory);

        if (this.isEditMode)
            elem.classList.add('margin-small-top');

        elem.setAttribute('label-value', `'${label} ${this.isEditMode && isMandatory ? '*' : ''}'`);
        elem.setAttribute('label-value-indiscreet', 'true');
        //elem.setAttribute('update-on', 'blur');
        elem.setAttribute('max-length', '1500');
        elem.setAttribute('show-length', 'true');
        elem.setAttribute('no-trim', 'true');
        elem.setAttribute('on-change', `ctrl.setDirty('${options.model}');`);

        if (options) {
            this.setCommonOptions(div, elem, options);

            if (options.maxLength)
                elem.setAttribute('max-length', options.maxLength.toString());
            if (options.rows)
                elem.setAttribute('rows', options.rows.toString());
        }

        div.appendChild(elem);

        return div;
    }

    public createTextBox(groupId: number, id: number, label: string, isMandatory: boolean, hideInRegistration: boolean, hideInEmploymentRegistration: boolean, hideInReport: boolean, hideInReportIfEmpty: boolean, options?: TemplateDesignerTextBoxOptions) {
        const div = this.createFieldPlaceholder(groupId, id, hideInRegistration, hideInEmploymentRegistration, hideInReport, hideInReportIfEmpty);

        const elem = document.createElement('soe-textbox');
        this.setCommonAttributes(elem, groupId, id, isMandatory);

        const mandatoryIcon = this.isEditMode && isMandatory ? '*' : '';
        const timeInfo = options && options.isTime ? '(' + this.terms["core.time.placeholder.hoursminutes"] + ')' : '';

        elem.setAttribute('label-value', `'${label} ${mandatoryIcon} ${timeInfo}'`);
        elem.setAttribute('label-value-indiscreet', 'true');
        elem.setAttribute('update-on', 'blur');
        elem.setAttribute('on-change', `ctrl.textBoxChanged('${options.model}');`);

        if (options) {
            this.setCommonOptions(div, elem, options);

            if (options.alignRight) {
                elem.setAttribute('input-class', 'text-right');
                elem.setAttribute('input-class-condition', 'true');
            }

            if (options.maxLength)
                elem.setAttribute('max-length', options.maxLength.toString());

            if (options.numeric) {
                elem.setAttribute('numeric', 'true');
                if (options.decimals)
                    elem.setAttribute('decimals', options.decimals.toString());
            }

            if (options.isTime) {
                elem.setAttribute('is-time', 'true');
            }

            if (options.placeholderKey)
                elem.setAttribute('placeholder-key', options.placeholderKey);
        }

        div.appendChild(elem);

        return div;
    }

    public createEmployeeAccount(groupId: number, id: number, label: string, isMandatory: boolean, hideInRegistration: boolean, hideInEmploymentRegistration: boolean, hideInReport: boolean, hideInReportIfEmpty: boolean, options?: TemplateDesignerComponentOptions) {
        const div = this.createFieldPlaceholder(groupId, id, hideInRegistration, hideInEmploymentRegistration, hideInReport, hideInReportIfEmpty);

        const elem = document.createElement('employee-account-component');
        this.setCommonAttributes(elem, groupId, id, false);

        elem.setAttribute('employment-start-date', 'ctrl.employee.employmentStartDate');
        elem.setAttribute('set-default', `${this.isEditMode}`);
        elem.setAttribute('on-change', 'ctrl.employeeAccountChanged(jsonString);');

        if (options) {
            this.setCommonOptions(div, elem, options);
        }

        div.appendChild(elem);

        return div;
    }

    public createDisbursementAccount(groupId: number, id: number, label: string, isMandatory: boolean, hideInRegistration: boolean, hideInEmploymentRegistration: boolean, hideInReport: boolean, hideInReportIfEmpty: boolean, options?: TemplateDesignerComponentOptions) {
        const div = this.createFieldPlaceholder(groupId, id, hideInRegistration, hideInEmploymentRegistration, hideInReport, hideInReportIfEmpty);

        const elem = document.createElement('disbursement-account-component');
        this.setCommonAttributes(elem, groupId, id, false);

        elem.setAttribute('social-sec', 'ctrl.employee.socialSec');
        elem.setAttribute('force-social-sec-nbr', `${this.forceSocialSecNbr}`);
        elem.setAttribute('on-change', 'ctrl.disbursementAccountChanged(jsonString);');

        if (options) {
            this.setCommonOptions(div, elem, options);
        }

        div.appendChild(elem);

        return div;
    }

    public createEmploymentPriceTypes(groupId: number, id: number, label: string, isMandatory: boolean, hideInRegistration: boolean, hideInEmploymentRegistration: boolean, hideInReport: boolean, hideInReportIfEmpty: boolean, options?: TemplateDesignerComponentOptions) {
        const div = this.createFieldPlaceholder(groupId, id, hideInRegistration, hideInEmploymentRegistration, hideInReport, hideInReportIfEmpty);

        const elem = document.createElement('employment-price-types-component');
        this.setCommonAttributes(elem, groupId, id, false);

        elem.setAttribute('date', 'ctrl.employee.employmentStartDate');
        elem.setAttribute('payroll-group-id', `${this.collectiveAgreement.payrollGroupId}`);
        elem.setAttribute('on-change', 'ctrl.employmentPriceTypesChanged(jsonString);');

        if (options) {
            this.setCommonOptions(div, elem, options);
        }

        div.appendChild(elem);

        return div;
    }

    public createPosition(groupId: number, id: number, label: string, isMandatory: boolean, hideInRegistration: boolean, hideInEmploymentRegistration: boolean, hideInReport: boolean, hideInReportIfEmpty: boolean, options?: TemplateDesignerComponentOptions) {
        const div = this.createFieldPlaceholder(groupId, id, hideInRegistration, hideInEmploymentRegistration, hideInReport, hideInReportIfEmpty);

        const elem = document.createElement('position-component');
        this.setCommonAttributes(elem, groupId, id, false);

        elem.setAttribute('on-change', 'ctrl.positionChanged(jsonString);');

        if (options) {
            this.setCommonOptions(div, elem, options);
        }

        div.appendChild(elem);

        return div;
    }

    private setCommonAttributes(elem: HTMLElement, groupId: number, id: number, required: boolean) {
        elem.setAttribute('is-edit-mode', `${this.isEditMode}`);
        if (this.isEditMode) {
            elem.setAttribute('data-ng-click', `ctrl.selectField(${groupId}, ${id});`);
            elem.setAttribute('data-ng-dblclick', `$event.stopPropagation(); ctrl.doubleClickToEdit(${id}, false);`);
        } else {
            if (required)
                elem.setAttribute('required', 'true');
        }
    }

    private setCommonOptions(placeholder: HTMLDivElement, elem: HTMLElement, options: TemplateDesignerComponentOptions) {
        if (options.model)
            elem.setAttribute('model', `ctrl.employee.${options.model}`);

        if (options.isMultiple && options.recordId)
            elem.setAttribute('record-id', options.recordId.toString());

        if (options.readOnly)
            elem.setAttribute('is-readonly', 'true');

        if (options.invalid) {
            elem.classList.add('has-error');
            elem.classList.add('has-feedback');
            placeholder.classList.add('invalid-position');
        }
    }
}