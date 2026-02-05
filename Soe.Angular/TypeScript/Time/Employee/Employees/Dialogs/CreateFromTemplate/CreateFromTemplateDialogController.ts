import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IEmployeeService } from "../../../EmployeeService";
import { IEmploymentTypeSmallDTO, ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { EmployeeTemplateDTO, EmployeeTemplateGroupRowDTO, SaveEmployeeFromTemplateHeadDTO, SaveEmployeeFromTemplateRowDTO } from "../../../../../Common/Models/EmployeeTemplateDTOs";
import { IValidationSummaryHandlerFactory } from "../../../../../Core/Handlers/validationsummaryhandlerfactory";
import { IValidationSummaryHandler } from "../../../../../Core/Handlers/ValidationSummaryHandler";
import { CompanySettingType, SoeEntityType, TermGroup_EmployeeTemplateGroupRowType, TermGroup_ExtraFieldType } from "../../../../../Util/CommonEnumerations";
import { EmployeeCollectiveAgreementDTO } from "../../../../../Common/Models/EmployeeCollectiveAgreementDTOs";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { CreateFromEmployeeTemplateMode, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { TemplateDesignerHelper } from "../../../EmployeeTemplates/Directives/TemplateDesigner/TemplateDesignerHelper";
import { ExtraFieldRecordDTO } from "../../../../../Common/Models/ExtraFieldDTO";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class CreateFromTemplateDialogController {

    // Terms
    private terms: { [index: string]: string };

    // Company settings
    private useAccountHierarchy = false;
    private useAnnualLeave = false;
    private forceSocialSecNbr = false;
    private setNextEmployeeNumberAutomatically = false;
    private dontAllowIdenticalSSN = false;

    // Data
    private employeeTemplates: ISmallGenericType[];
    private employeeTemplate: EmployeeTemplateDTO;
    private collectiveAgreement: EmployeeCollectiveAgreementDTO;
    private employmentTypes: IEmploymentTypeSmallDTO[];
    private employee: any;
    private extraFieldRecords: ExtraFieldRecordDTO[];

    // Properties
    private employeeTemplateId: number;
    private employmentStopDateExistsInTemplate = false;
    private disbursementAccountExistsInTemplate = false;
    private employeeAccountExistsInTemplate = false;

    private get isNewEmployeeMode(): boolean {
        return this.mode === CreateFromEmployeeTemplateMode.NewEmployee;
    }

    private get isNewEmploymentMode(): boolean {
        return this.mode === CreateFromEmployeeTemplateMode.NewEmployment;
    }

    private _printEmploymentContract = true;
    private get printEmploymentContract(): boolean {
        return this._printEmploymentContract;
    }
    private set printEmploymentContract(value: boolean) {
        this._printEmploymentContract = value;
        if (!value)
            this.initSigning = false;
    }

    private initSigning = true;

    // Flags
    private saveInProgress = false;

    // Handlers
    private templateDesignerHelper: TemplateDesignerHelper;
    private validationHandler: IValidationSummaryHandler;
    public progress: IProgressHandler;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        private mode: CreateFromEmployeeTemplateMode,
        private createdWithEmployeeTemplateId: number,
        private employeeId: number,
        private numberAndName: string) {

        if (validationSummaryHandlerFactory)
            this.validationHandler = validationSummaryHandlerFactory.create();

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        let queue = [];
        queue.push(this.loadTerms());
        if (this.isNewEmployeeMode)
            queue.push(this.loadCompanySettings());

        queue.push(this.loadEmploymentTypes());

        this.$q.all(queue).then(() => {
            this.loadEmployeeTemplates().then(() => {
                if (this.isNewEmploymentMode && this.createdWithEmployeeTemplateId) {
                    this.employeeTemplateId = this.createdWithEmployeeTemplateId;
                    this.loadEmployeeTemplate();
                }
            });
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.missingmandatoryfield",
            "common.accounting",
            "time.employee.employee.paymentmethodmandatory",
            "time.employee.disbursementmethodisunknown"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.UseAnnualLeave);
        settingTypes.push(CompanySettingType.TimeForceSocialSecNbr);
        settingTypes.push(CompanySettingType.TimeSetNextFreePersonNumberAutomatically);
        settingTypes.push(CompanySettingType.DontAllowIdenticalSSN);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.useAnnualLeave = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAnnualLeave);
            this.forceSocialSecNbr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeForceSocialSecNbr);
            this.setNextEmployeeNumberAutomatically = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSetNextFreePersonNumberAutomatically);
            this.dontAllowIdenticalSSN = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.DontAllowIdenticalSSN);
        });
    }

    private loadEmployeeTemplates(): ng.IPromise<any> {
        return this.employeeService.getEmployeeTemplatesDict(false).then(x => {
            this.employeeTemplates = x;
        });
    }

    private loadEmployeeTemplate(): ng.IPromise<any> {
        return this.employeeService.getEmployeeTemplate(this.employeeTemplateId).then(x => {
            this.employeeTemplate = x;
            if (this.employeeTemplate) {
                this.templateDesignerHelper = new TemplateDesignerHelper(this.$q, this.translationService, this.coreService, this.employeeService, this.useAccountHierarchy, this.forceSocialSecNbr, this.employeeTemplate, false, this.mode, () => { });

                this.loadCollectiveAgreement();

                if (this.templateDesignerHelper.isNewEmployeeMode && this.setNextEmployeeNumberAutomatically)
                    this.getNextEmployeeNumber();

                // Check if certain fields exists in template
                this.employeeTemplate.employeeTemplateGroups.forEach(g => {
                    const empStopDate = g.employeeTemplateGroupRows.find(r => r.type === TermGroup_EmployeeTemplateGroupRowType.EmploymentStopDate);

                    if (this.templateDesignerHelper.isNewEmployeeMode) {
                        const disbursementAccount = g.employeeTemplateGroupRows.find(r => r.type === TermGroup_EmployeeTemplateGroupRowType.DisbursementAccount);
                        const employeeAccount = g.employeeTemplateGroupRows.find(r => r.type === TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount);

                        if (empStopDate && !empStopDate.hideInRegistration)
                            this.employmentStopDateExistsInTemplate = true;
                        if (disbursementAccount && !disbursementAccount.hideInRegistration)
                            this.disbursementAccountExistsInTemplate = true;
                        if (employeeAccount && !employeeAccount.hideInRegistration)
                            this.employeeAccountExistsInTemplate = true;
                    } else if (this.templateDesignerHelper.isNewEmploymentMode) {
                        if (empStopDate && !empStopDate.hideInEmploymentRegistration)
                            this.employmentStopDateExistsInTemplate = true;
                    }
                });
                this.checkExtraFieldAccountLinkable();
                this.loadExtraFieldRecords().then(() => {
                    this.setExistingExtraFieldRecords();
                });
            }
        });
    }

    private loadEmploymentTypes(): ng.IPromise<any> {
        return this.employeeService.getEmploymentEmploymentTypes(CoreUtility.languageId).then(x => {
            // Remove unknown
            this.employmentTypes = x.filter(y => y.id !== 0 && y.active);
        });
    }

    private loadCollectiveAgreement(): ng.IPromise<any> {
        return this.employeeService.getEmployeeCollectiveAgreement(this.employeeTemplate.employeeCollectiveAgreementId).then(x => {
            this.collectiveAgreement = x;
        });
    }

    private loadExtraFieldRecord(gRow: EmployeeTemplateGroupRowDTO, accountId: number) {
        const options = this.templateDesignerHelper.getComponentOptionsByType(gRow);
        if (options && options.model) {
            this.coreService.getExtraFieldRecord(gRow.recordId, accountId, SoeEntityType.Account).then(x => {
                this.employee[options.model] = x ? x.value : undefined;
            });
        }
    }

    private loadExtraFieldRecords(): ng.IPromise<any> {
        return this.coreService.getExtraFieldWithRecords(this.employeeId, SoeEntityType.Employee, CoreUtility.languageId, 0, 0).then(x => {
            this.extraFieldRecords = x;
            _.forEach(this.extraFieldRecords, (r) => {
                if (r.extraFieldType === TermGroup_ExtraFieldType.Date)
                    r.dateData = CalendarUtility.convertToDate(r.dateData);
            })
        });
    }

    private getNextEmployeeNumber() {
        this.employeeService.getLastUsedEmployeeSequenceNumber().then(number => {
            this.employee.employeeNr = (number + 1).toString();
        });
    }

    private validateSocialSecExists(socialSec: string) {
        if (!socialSec)
            return;

        this.employeeService.validateEmployeeSocialSecNumberNotExists(socialSec, this.employeeId).then(result => {
            if (!result.success) {
                if (!this.dontAllowIdenticalSSN) {
                    this.translationService.translate("time.employee.employee.socialsecnumber.exists.title").then(term => {
                        this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Warning);
                    });
                } else {
                    this.translationService.translate("time.employee.employee.socialsecnumber.exists.companysetting").then(term => {
                        this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Warning);
                    });
                }
            }
        });
    }

    // EVENTS

    private templateChanged() {
        this.$timeout(() => {
            this.loadEmployeeTemplate();
        });
    }

    private modelChanged(model: string, value) {
        if (model === 'socialSec') {
            this.validateSocialSecExists(value);
        } else if (model === 'hierarchicalAccount') {
            this.checkExtraFieldAccountLinkable();
        }
    }

    private openEditPage() {
        this.$uibModalInstance.close({ openEditPage: true });
    }

    private ok() {
        this.saveInProgress = true;

        const dto = this.convertToSaveDTO();

        if (this.templateDesignerHelper.isNewEmploymentMode && this.employeeId)
            dto.employeeId = this.employeeId;

        this.progress.startSaveProgress((completion) => {
            this.employeeService.saveEmployeeFromTemplate(dto).then(res => {
                if (res.success) {
                    completion.completed(null, null, true);
                    this.$uibModalInstance.close({ dto: dto, initSigning: this.initSigning, employeeId: res.integerValue, userId: res.integerValue2, dataStorageId: res.decimalValue, errorNumber: res.errorNumber, errorMessage: res.errorMessage });
                } else {
                    completion.failed(res.errorMessage);
                    this.saveInProgress = false;
                }
            }, error => {
                completion.failed(error.message);
                this.saveInProgress = false;
            });
        }, null).then(data => {
            this.saveInProgress = false;
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    // HELP-METHODS

    private checkExtraFieldAccountLinkable() {
        if (!this.employee || !this.employeeAccountExistsInTemplate)
            return;

        const accountDimId = this.getEmployeeAccountAccountDimId();
        if (!accountDimId)
            return;

        const accountId = this.getEmployeeAccountAccountId();
        if (!accountId)
            return;

        const accountChildId = this.getEmployeeAccountChildAccountId();

        this.employeeTemplate.employeeTemplateGroups.forEach(g => {
            // Check if any fields should get default value from extra field on account
            g.employeeTemplateGroupRows.forEach(gRow => {
                if (this.templateDesignerHelper.isFieldExtraFieldAccountLinkable(gRow.type) && gRow.entity === SoeEntityType.AccountDim && gRow.recordId)
                    this.loadExtraFieldRecord(gRow, accountChildId ? accountChildId : accountId);
            });
        });
    }

    private setExistingExtraFieldRecords() {
        if (!this.extraFieldRecords || this.extraFieldRecords.length === 0)
            return;

        // For all employee extra fields in the template, check if there is an existing record on the employee.
        // In that case, use existing value instead of default value from template.
        this.employeeTemplate.employeeTemplateGroups.forEach(g => {
            g.employeeTemplateGroupRows.filter(r => r.type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee &&
                !r.hideInEmploymentRegistration).forEach(gRow => {
                    let existing = this.extraFieldRecords.find(e => e.extraFieldRecordId !== 0 && e.extraFieldId === gRow.recordId);
                    if (existing) {
                        const options = this.templateDesignerHelper.getComponentOptionsByType(gRow);
                        if (options && options.model) {
                            switch (existing.extraFieldType) {
                                case TermGroup_ExtraFieldType.FreeText:
                                    this.employee[options.model] = existing.strData;
                                    break;
                                case TermGroup_ExtraFieldType.Integer:
                                case TermGroup_ExtraFieldType.YesNo:
                                case TermGroup_ExtraFieldType.SingleChoice:
                                    this.employee[options.model] = existing.intData;
                                    break;
                                case TermGroup_ExtraFieldType.Decimal:
                                    this.employee[options.model] = existing.decimalData;
                                    break;
                                case TermGroup_ExtraFieldType.Checkbox:
                                    this.employee[options.model] = existing.boolData;
                                    break;
                                case TermGroup_ExtraFieldType.Date:
                                    this.employee[options.model] = existing.dateData;
                                    break;
                            }
                        }
                    }

                });
        });
    }

    private getHierarchicalAccountObject() {
        if (this.employee && this.employee.hierarchicalAccount)
            return JSON.parse(this.employee.hierarchicalAccount);

        return null;
    }

    private getEmployeeAccountAccountDimId(): number {
        const json = this.getHierarchicalAccountObject();
        return json ? json.accountDimId : 0;
    }

    private getEmployeeAccountAccountId(): number {
        const json = this.getHierarchicalAccountObject();
        return json ? json.accountId : 0;
    }

    private getEmployeeAccountChildAccountId(): number {
        const json = this.getHierarchicalAccountObject();
        return json ? json.childAccountId : 0;
    }

    private getTitleByType(type: TermGroup_EmployeeTemplateGroupRowType, id: number = 0): string {
        let title = '';

        if (this.employeeTemplate && this.employeeTemplate.employeeTemplateGroups) {
            this.employeeTemplate.employeeTemplateGroups.forEach(group => {
                if (group.employeeTemplateGroupRows) {
                    // For normal fields there is a one to one link between the field and the type. Type fetched from getTypeByFieldName().
                    // Field name starts with 'ctrl_employee_' then the type name.
                    // For extra fields the field name is 'ctrl_employee_extraFieldAccount_NNN' or 'ctrl_employee_extraFieldEmployee_NNN' where NNN is the id of the row.
                    // So the type is the same for all extra fields of the same entity, and we need to match on id as well.
                    if (type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount || type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee) {
                        group.employeeTemplateGroupRows.forEach(row => {
                            if (row.type === type && row.employeeTemplateGroupRowId === id) {
                                title = row.title;
                                return false;
                            }
                        });
                        // Exit outer loop if title found
                        if (title) return false;
                    } else {
                        const row = group.employeeTemplateGroupRows.find(r => r.type === type);
                        if (row) {
                            title = row.title;
                            return false;
                        }
                    }
                }
            });
        }

        return title;
    }

    private convertToSaveDTO(): SaveEmployeeFromTemplateHeadDTO {
        const dto = new SaveEmployeeFromTemplateHeadDTO();
        dto.employeeTemplateId = this.employeeTemplateId;
        dto.date = this.employee.employmentStartDate;
        dto.printEmploymentContract = this.printEmploymentContract;
        dto.rows = [];

        const fieldNames = Object.getOwnPropertyNames(this.employee);
        fieldNames.forEach(fieldName => {
            const type = this.templateDesignerHelper.getTypeByFieldName(fieldName);
            if (type !== TermGroup_EmployeeTemplateGroupRowType.Unknown) {
                const row = new SaveEmployeeFromTemplateRowDTO();
                row.type = type;
                row.value = this.employee[fieldName];

                if (row.type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount) {
                    row.entity = SoeEntityType.Account;
                    row.recordId = this.getRecordId(fieldName);
                } else if (row.type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee) {
                    row.entity = SoeEntityType.Employee;
                    row.recordId = this.getRecordId(fieldName);
                }

                // Only add fields that contains any value
                if (typeof row.value !== 'undefined')
                    dto.rows.push(row);
            }
        });

        return dto;
    }

    private getRecordId(fieldName: string): number {
        const elem = document.querySelectorAll(`[model="ctrl.employee.${fieldName}"]`);
        if (elem && elem.length > 0) {
            const recordId = elem[0].getAttribute('record-id');
            if (recordId) {
                return parseInt(recordId);
            }
        }

        return 0;
    }

    // VALIDATION

    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.employeeTemplate) {
                const errors = this['edit'].$error;

                if (!this.employee.employmentStartDate)
                    mandatoryFieldKeys.push("common.startdate");

                if (errors['mandatoryEmploymentStopDate'])
                    mandatoryFieldKeys.push("common.stopdate");

                if (errors.required) {
                    _.forEach(errors.required, req => {
                        const type = this.templateDesignerHelper.getTypeByFieldName(req['$name']);
                        if (type !== TermGroup_EmployeeTemplateGroupRowType.Unknown) {
                            let id: number = 0;
                            if (type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount || type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee) {
                                id = parseInt(req['$name'].substring(req['$name'].lastIndexOf('_') + 1));
                            }

                            const title = this.getTitleByType(type, id);
                            if (title)
                                validationErrorStrings.push(this.terms["core.missingmandatoryfield"] + " " + title.toLocaleLowerCase());
                        } else if (req['$name'] === 'ctrl_selectedAccount') {
                            validationErrorStrings.push(this.terms["core.missingmandatoryfield"] + " " + this.terms["common.accounting"].toLocaleLowerCase());
                        }
                    });
                }

                if (errors['mandatoryDisbursementMethod'])
                    validationErrorStrings.push(this.terms["time.employee.employee.paymentmethodmandatory"]);
                if (errors['validDisbursementMethod'])
                    validationErrorStrings.push(this.terms["time.employee.disbursementmethodisunknown"]);
            }
        });
    }
}
