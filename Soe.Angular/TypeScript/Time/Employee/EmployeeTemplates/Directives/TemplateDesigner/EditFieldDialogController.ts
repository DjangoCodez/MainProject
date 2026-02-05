import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { EmployeeTemplateGroupRowDTO, TemplateDesignerComponentOptions, TemplateDesignerTextAreaFormats } from "../../../../../Common/Models/EmployeeTemplateDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SoeEntityType, TemplateDesignerComponent, TermGroup_EmployeeTemplateGroupRowType } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ExtraFieldGridDTO } from "../../../../../Common/Models/ExtraFieldDTO";
import { TemplateDesignerHelper } from "./TemplateDesignerHelper";
import { IFocusService } from "../../../../../Core/Services/focusservice";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";

export class EditFieldDialogController {

    private field: EmployeeTemplateGroupRowDTO;
    private fieldOptions: TemplateDesignerComponentOptions;
    private extraFields: ExtraFieldGridDTO[] = [];
    private selectedAccountDimId: number;
    private accountExtraFields: ExtraFieldGridDTO[] = [];

    private isNew: boolean;

    private textAreaRows = 3;

    private _selectedFieldType: ISmallGenericType;
    get selectedFieldType() {
        return this._selectedFieldType;
    }
    set selectedFieldType(item: ISmallGenericType) {
        this._selectedFieldType = item;
        if (item) {
            this.field.type = item.id;
        } else {
            this.field.type = undefined;
        }
    }

    private _linkExtraField: boolean;
    get linkExtraField() {
        return this._linkExtraField;
    }
    set linkExtraField(value: boolean) {
        this._linkExtraField = value;
        this.field.entity = SoeEntityType.AccountDim;
    }

    private get isCheckBoxType(): boolean {
        if (this.field && this.field.type) {
            switch (this.field.type) {
                case TermGroup_EmployeeTemplateGroupRowType.IsSecondaryEmployment:
                case TermGroup_EmployeeTemplateGroupRowType.ControlTaskPartnerInCloseCompany:
                case TermGroup_EmployeeTemplateGroupRowType.ControlTaskBenefitAsPension:
                case TermGroup_EmployeeTemplateGroupRowType.AFAParttimePensionCode:
                case TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDateIsLeaveOfAbsence:
                case TermGroup_EmployeeTemplateGroupRowType.GTPExcluded:
                case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentIgnore:
                    return true;
            }
        }

        return false;
    }

    private get isSystemRequired(): boolean {
        if (this.field && this.field.type)
            return this.templateDesignerHelper.isFieldSystemRequired(this.field.type);

        return false;
    }

    private get isSystemHideInReport(): boolean {
        if (this.field && this.field.type) {
            switch (this.field.type) {
                case TermGroup_EmployeeTemplateGroupRowType.DisbursementAccount:
                case TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount:
                case TermGroup_EmployeeTemplateGroupRowType.EmploymentPriceTypes:
                    return true;
            }
        }

        return false;
    }

    private get isReadOnly(): boolean {
        if (this.field && this.field.type) {
            switch (this.field.type) {
                case TermGroup_EmployeeTemplateGroupRowType.Name:
                case TermGroup_EmployeeTemplateGroupRowType.ZipCity:
                case TermGroup_EmployeeTemplateGroupRowType.DisbursementMethod:
                case TermGroup_EmployeeTemplateGroupRowType.DisbursementAccountNr:
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
                case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount:
                    return true;
            }
        }

        return false;
    }

    private get isExtraFieldAccountLinkable(): boolean {
        if (this.field && this.field.type)
            return this.templateDesignerHelper.isFieldExtraFieldAccountLinkable(this.field.type);

        return false;
    }

    private get showExtraFieldSelector(): boolean {
        return (this.field.type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee || this.field.type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount);
    }

    private get isTextAreaComponent(): boolean {
        const component = this.templateDesignerHelper.getComponentByType(this.field);
        return component && component === TemplateDesignerComponent.TextArea;
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private focusService: IFocusService,
        private coreService: ICoreService,
        private templateDesignerHelper: TemplateDesignerHelper,
        private fieldNames: ISmallGenericType[],
        private accountDims: AccountDimSmallDTO[],
        field: EmployeeTemplateGroupRowDTO) {

        this.isNew = !field;

        this.field = new EmployeeTemplateGroupRowDTO();
        angular.extend(this.field, field);

        this.selectedFieldType = this.fieldNames.find(f => f.id === this.field.type);

        if (!this.isNew && this.field.entity && (this.field.type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee || this.field.type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount))
            this.loadExtraFields();

        this.setSystemFieldProperties();

        if (this.isTextAreaComponent && this.field.format) {
            const formats: TemplateDesignerTextAreaFormats = JSON.parse(this.field.format);
            if (formats && formats.rows)
                this.textAreaRows = formats.rows;
        }

        if (this.isExtraFieldAccountLinkable && this.field.entity === SoeEntityType.AccountDim && this.field.recordId) {
            this.linkExtraField = true;
            this.setExtraField(0);
        }

        this.$timeout(() => {
            this.focusService.focusByName(this.isNew ? "ctrl_selectedFieldType" : "ctrl_field_title");
        }, 200);
    }

    // SERVICE CALLS

    private loadExtraFields(): ng.IPromise<any> {
        return this.coreService.getExtraFields(this.field.entity).then(x => {
            this.extraFields = x;
        });
    }

    private loadAccountExtraFields(): ng.IPromise<any> {
        return this.coreService.getExtraFieldsGrid(SoeEntityType.Account, false, SoeEntityType.AccountDim, this.selectedAccountDimId).then(x => {
            this.accountExtraFields = x;
        });
    }

    // EVENTS

    private typeChanged() {
        this.$timeout(() => {
            this.setSystemFieldProperties();

            if (this.field.type) {
                if (this.field.type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee) {
                    this.field.entity = SoeEntityType.Employee;
                    this.loadExtraFields().then(() => {
                        this.focusService.focusByName("ctrl_field_recordId");
                    });
                }
                else if (this.field.type === TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount) {
                    this.field.entity = SoeEntityType.Account;
                    this.loadExtraFields().then(() => {
                        this.focusService.focusByName("ctrl_field_recordId");
                    });
                } else {
                    this.field.title = this.fieldNames.find(f => f.id === this.field.type)?.name;
                    this.focusService.focusByName("ctrl_field_title");
                }
            }
        });
    }

    private extraFieldChanged() {
        this.$timeout(() => {
            if (this.field.recordId) {
                this.field.title = this.extraFields.find(f => f.extraFieldId === this.field.recordId)?.text;
            }
        });
    }

    private accountDimChanged() {
        this.$timeout(() => {
            this.loadAccountExtraFields();
        });
    }

    private initDelete() {
        const keys: string[] = [
            "time.employee.employeetemplate.designer.tools.deletefield",
            "time.employee.employeetemplate.designer.tools.deletefield.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["time.employee.employeetemplate.designer.tools.deletefield"], terms["time.employee.employeetemplate.designer.tools.deletefield.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
                if (val)
                    this.delete();
            }, () => {
                // User cancelled
            });
        });
    }

    private delete() {
        this.$uibModalInstance.close({ delete: true });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        if (this.isTextAreaComponent) {
            const formats = new TemplateDesignerTextAreaFormats();
            formats.rows = this.textAreaRows;

            this.field.format = JSON.stringify(formats);
        }

        if (this.isExtraFieldAccountLinkable && !this.linkExtraField) {
            // Remove any previously selected linked extra field
            this.field.entity = undefined;
            this.field.recordId = undefined;
        }

        this.$uibModalInstance.close({ field: this.field });
    }

    // HELP-METHODS

    private setSystemFieldProperties() {
        this.fieldOptions = this.templateDesignerHelper.getComponentOptionsByType(this.field);

        if (this.isSystemRequired)
            this.field.isMandatory = true;

        if (this.isSystemHideInReport)
            this.field.hideInReport = true;

        if (this.isReadOnly) {
            this.field.hideInRegistration = true;
            this.field.hideInEmploymentRegistration = true;
        }

        if (this.fieldOptions.systemHideInEmploymentRegistration)
            this.field.hideInEmploymentRegistration = true;

        if (this.fieldOptions.systemHideInRegistration)
            this.field.hideInRegistration = true;
    }

    private setExtraField(dimIdx: number) {
        if (this.accountDims.length < dimIdx + 1)
            return;

        this.selectedAccountDimId = this.accountDims[dimIdx].accountDimId;

        this.loadAccountExtraFields().then(() => {
            let foundit = (this.accountExtraFields && this.accountExtraFields.filter(f => f.accountDimId === this.selectedAccountDimId && f.extraFieldId === this.field.recordId).length > 0);
            if (!foundit)
                this.setExtraField(++dimIdx);
        });
    }
}
