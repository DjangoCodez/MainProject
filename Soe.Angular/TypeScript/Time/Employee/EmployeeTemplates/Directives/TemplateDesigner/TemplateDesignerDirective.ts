import angular, { IPromise } from "angular";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { EmployeeTemplateDTO, EmployeeTemplateGroupDTO, EmployeeTemplateGroupRowDTO, TemplateDesignerSelectOptions, TemplateDesignerTextAreaFormats, TemplateDesignerTextAreaOptions, TemplateDesignerTextBoxOptions } from "../../../../../Common/Models/EmployeeTemplateDTOs";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IEmploymentTypeSmallDTO, IExtraFieldValueDTO, ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { CompanySettingType, TemplateDesignerComponent, TermGroup, TermGroup_EmployeeTemplateGroupRowType, TermGroup_EmploymentType } from "../../../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { CreateFromEmployeeTemplateMode, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { IEmployeeService } from "../../../EmployeeService";
import { EditFieldDialogController } from "./EditFieldDialogController";
import { EditGroupDialogController } from "./EditGroupDialogController";
import { TemplateDesignerHelper } from "./TemplateDesignerHelper";

export class TemplateDesignerDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/EmployeeTemplates/Directives/TemplateDesigner/TemplateDesigner.html'),
            scope: {
                employeeTemplate: '=',
                employee: '=?',
                isEditMode: '=',
                mode: '=',
                readOnly: '=',
                hasInvalidPosition: '=?',
                hasRemainingSystemRequiredFields: '=?',
                onChange: '&',
                onModelChange: '&',
                onRendered: '&'
            },
            restrict: 'E',
            replace: true,
            controller: TemplateDesignerController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class TemplateDesignerController {
    // Bindings
    private employeeTemplate: EmployeeTemplateDTO;
    private employee: any;
    private isEditMode: boolean;
    private mode: CreateFromEmployeeTemplateMode;
    private readOnly: boolean;
    private hasInvalidPosition: boolean;
    private hasRemainingSystemRequiredFields: boolean;
    private onChange: Function;
    private onModelChange: Function;
    private onRendered: Function;

    private templateDesignerHelper: TemplateDesignerHelper;

    // Terms
    private terms: { [index: string]: string };
    private groupInfoNew: string;
    private groupInfoSelected: string;
    private fieldInfoNew: string;
    private fieldInfoSelected: string;

    // Company settings
    private useAccountHierarchy = false;
    private forceSocialSecNbr = false;

    // Lookups
    private groupTypes: ISmallGenericType[];
    private fieldNames: ISmallGenericType[];
    private yesNoItems: ISmallGenericType[];
    private accountDims: AccountDimSmallDTO[];
    private employmentTypes: IEmploymentTypeSmallDTO[];
    private excludeFromWorkTimeWeekCalculationItems: ISmallGenericType[];
    private payrollPriceFormulas: ISmallGenericType[];
    private bygglosenSalaryType: ISmallGenericType[];
    private experiences: ISmallGenericType[];
    private payrollReportsPersonalCategories: ISmallGenericType[];
    private payrollReportsWorkTimeCategories: ISmallGenericType[];
    private payrollReportsPayrollExportSalaryTypes: ISmallGenericType[];
    private payrollReportsAFACategories: ISmallGenericType[];
    private payrollReportsAFASpecialAgreements: ISmallGenericType[];
    private payrollReportsCollectumITPplans: ISmallGenericType[];
    private kpaBelongings: ISmallGenericType[];
    private kpaEndCodes: ISmallGenericType[];
    private kpaAgreementTypes: ISmallGenericType[];
    private gtpAgreementNumbers: ISmallGenericType[];

    // Properties
    private tmpIdCounter = 0;
    private selectedGroup: EmployeeTemplateGroupDTO;
    private selectedGroupId: number;
    private selectedField: EmployeeTemplateGroupRowDTO;
    private selectedFieldId: number;

    private get selectedGroupName(): string {
        if (!this.selectedGroup)
            return '';

        return this.selectedGroup.name;
    }

    private get selectedFieldName(): string {
        if (!this.selectedField)
            return '';

        const typeName = this.getFieldNameByType(this.selectedField.type);
        if (this.selectedField.title !== typeName)
            return `${this.selectedField.title} (${typeName})`;
        else
            return this.selectedField.title;
    }

    private remainingSystemRequiredFields: TermGroup_EmployeeTemplateGroupRowType[] = [];
    private remainingSystemRequiredFieldNames: string;

    private hasAccordions = false;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $compile: ng.ICompileService,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        private payrollService: IPayrollService) {

        this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings()]).then(() => {
                this.$q.all([
                    this.loadGroupTypes(),
                    this.loadFieldNames(),
                    this.loadYesNoItems(),
                    this.loadAccountDims(),
                    this.loadEmploymentTypes(),
                    this.loadExcludeFromWorkTimeWeekCalculationItems(),
                    this.loadPayrollPriceFormulas(),
                    this.loadBygglosenSalaryType(),
                    this.loadPayrollReportsPersonalCategories(),
                    this.loadPayrollReportsWorkTimeCategories(),
                    this.loadPayrollReportsPayrollExportSalaryTypes(),
                    this.loadPayrollReportsAFACategories(),
                    this.loadPayrollReportsAFASpecialAgreements(),
                    this.loadPayrollReportsCollectumITPplans(),
                    this.loadKpaBelongings(),
                    this.loadKpaEndCodes(),
                    this.loadKpaAgreementTypes(),
                    this.loadGtpAgreementNumbers()
                ]).then(() => {
                    this.setupExperiences();
                    this.setupWatchers();
                })
            });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.employeeTemplate, (newVal, oldVal) => {
            this.employee = {};
            if (this.employeeTemplate)
                this.templateDesignerHelper = new TemplateDesignerHelper(this.$q, this.translationService, this.coreService, this.employeeService, this.useAccountHierarchy, this.forceSocialSecNbr, this.employeeTemplate, this.isEditMode, this.mode, () => { this.helperInitialized(); });
            else
                this.clearDesigner();
        });
    }

    private helperInitialized() {
        this.renderDesign(false, false);
    }

    private setupExperiences() {
        this.experiences = [];
        this.experiences.push({ id: 1, name: this.terms["time.employee.employment.experience.agreed"] });
        this.experiences.push({ id: 0, name: this.terms["time.employee.employment.experience.established"] });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys = [
            "time.employee.employeetemplate.designer.tools.groupinfo.new",
            "time.employee.employeetemplate.designer.tools.groupinfo.selected",
            "time.employee.employeetemplate.designer.tools.fieldinfo.new",
            "time.employee.employeetemplate.designer.tools.fieldinfo.selected",
            "time.employee.employment.experience.agreed",
            "time.employee.employment.experience.established"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
            this.groupInfoNew = this.terms["time.employee.employeetemplate.designer.tools.groupinfo.new"];
            this.groupInfoSelected = this.terms["time.employee.employeetemplate.designer.tools.groupinfo.selected"];
            this.fieldInfoNew = this.terms["time.employee.employeetemplate.designer.tools.fieldinfo.new"];
            this.fieldInfoSelected = this.terms["time.employee.employeetemplate.designer.tools.fieldinfo.selected"];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.TimeForceSocialSecNbr);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.forceSocialSecNbr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeForceSocialSecNbr);
        });
    }

    private loadGroupTypes(): IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeTemplateGroupType, false, false, true).then(x => {
            this.groupTypes = x;
        });
    }

    private loadFieldNames(): IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeTemplateGroupRowType, false, false).then(x => {
            this.fieldNames = x;

            if (!this.useAccountHierarchy)
                this.fieldNames = this.fieldNames.filter(f => f.id !== TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount && f.id !== TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount);
        });
    }

    private loadYesNoItems(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.YesNo, false, false).then(x => {
            this.yesNoItems = x;
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.coreService.getAccountDimsSmall(false, true, false, false, false, false).then(x => {
            this.accountDims = x;
        });
    }

    private loadEmploymentTypes(): ng.IPromise<any> {
        return this.employeeService.getEmploymentEmploymentTypes(CoreUtility.languageId).then(x => {
            // Remove unknown
            this.employmentTypes = x.filter(y => y.id !== 0 && y.active);
        });
    }

    private loadExcludeFromWorkTimeWeekCalculationItems(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ExcludeFromWorkTimeWeekCalculationItems, false, false, true).then(x => {
            this.excludeFromWorkTimeWeekCalculationItems = x;
        });
    }

    private loadPayrollPriceFormulas(): ng.IPromise<any> {
        return this.payrollService.getPayrollPriceFormulasDict(true).then(x => {
            this.payrollPriceFormulas = x;
        });
    }
    private loadBygglosenSalaryType(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.BygglosenSalaryType, true, true).then((x) => {
            this.bygglosenSalaryType = x;
        });
    }
    private loadPayrollReportsPersonalCategories(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsPersonalCategory, true, true).then((x) => {
            this.payrollReportsPersonalCategories = x;
        });
    }

    private loadPayrollReportsWorkTimeCategories(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsWorkTimeCategory, true, true).then((x) => {
            this.payrollReportsWorkTimeCategories = x;
        });
    }

    private loadPayrollReportsPayrollExportSalaryTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsSalaryType, true, true).then((x) => {
            this.payrollReportsPayrollExportSalaryTypes = x;
        });
    }

    private loadPayrollReportsAFACategories(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsAFACategory, true, true).then((x) => {
            this.payrollReportsAFACategories = x;
        });
    }

    private loadPayrollReportsAFASpecialAgreements(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsAFASpecialAgreement, true, true).then((x) => {
            this.payrollReportsAFASpecialAgreements = x;
        });
    }

    private loadPayrollReportsCollectumITPplans(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsCollectumITPplan, true, true).then((x) => {
            this.payrollReportsCollectumITPplans = x;
        });
    }

    private loadKpaBelongings(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.KPABelonging, true, true).then(x => {
            this.kpaBelongings = x;
        });
    }

    private loadKpaEndCodes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.KPAEndCode, true, true).then(x => {
            this.kpaEndCodes = x;
        });
    }

    private loadKpaAgreementTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.KPAAgreementType, true, true).then(x => {
            this.kpaAgreementTypes = x;
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

    // DESIGNER

    private getAttachmentPoint() {
        if (this.employeeTemplate?.employeeTemplateId) {
            const elem = $(`[template-id=${this.employeeTemplate.employeeTemplateId}]`);
            if (elem && elem.length > 0)
                return $(elem[0]).find('.designer');
        }

        return null;
    }

    private clearDesigner(): ng.IPromise<any> {
        return this.$timeout(() => {
            const attachmentPoint = this.getAttachmentPoint();
            if (attachmentPoint)
                attachmentPoint.empty();
        });
    }

    private renderDesign(keepSelected: boolean, setDirty: boolean) {
        this.validateFieldPositions();
        this.rememberOpenAccordions();

        this.clearDesigner().then(() => {
            const attachmentPoint = this.getAttachmentPoint();

            this.templateDesignerHelper.accordionCounter = 0;
            this.templateDesignerHelper.multiFieldIdCounter = 0;

            // Reseed multiFieldIdCounter based on max EmployeeTemplateGroupRowId used in current template
            if (this.employeeTemplate?.employeeTemplateGroups) {
                this.employeeTemplate.employeeTemplateGroups.forEach(g => {
                    let maxId = Math.max(...g.employeeTemplateGroupRows.map(r => r.employeeTemplateGroupRowId));
                    if (maxId > this.templateDesignerHelper.multiFieldIdCounter)
                        this.templateDesignerHelper.multiFieldIdCounter = maxId + 1000;
                });
            }

            this.remainingSystemRequiredFields = this.templateDesignerHelper.systemRequiredFields;
            if (this.employeeTemplate.employeeTemplateId)
                this.checkSystemRequiredField(null);

            if (this.employeeTemplate?.employeeTemplateGroups) {
                _.orderBy(this.employeeTemplate.employeeTemplateGroups, g => g.sortOrder).forEach(group => {
                    if (group.identity && (this.isEditMode || (this.templateDesignerHelper.isNewEmployeeMode && group.hasRegistrationRows) || (this.templateDesignerHelper.isNewEmploymentMode && group.hasEmploymentRegistrationRows))) {
                        // Group
                        if (this.isEditMode && group.newPageBefore)
                            attachmentPoint.append(this.templateDesignerHelper.createPageBreakIconRow());

                        const groupRow = this.templateDesignerHelper.createRow();

                        const groupCol = this.templateDesignerHelper.createCol(4);
                        groupRow.appendChild(groupCol);

                        const groupAccordion = this.templateDesignerHelper.createGroupPlaceholder(group.identity, group.name);
                        groupCol.appendChild(groupAccordion);

                        // Group description
                        if (group.description) {
                            const descRow = this.templateDesignerHelper.createRow();
                            const descCol = this.templateDesignerHelper.createCol(4, 0);
                            descRow.appendChild(descCol);

                            const instruction = this.templateDesignerHelper.createInstruction(group.identity, --this.tmpIdCounter, group.description);
                            descCol.appendChild(instruction);

                            groupAccordion.children[0].children[0].appendChild(descRow);
                        }

                        // Fields
                        let hasFields = false;
                        let currentRowNr = 0;
                        let currentColNr = 1;
                        let row: HTMLDivElement;
                        if (group.employeeTemplateGroupRows) {
                            _.orderBy(group.employeeTemplateGroupRows, ['row', 'startColumn']).forEach(gRow => {
                                if (gRow.identity && (this.isEditMode || (this.templateDesignerHelper.isNewEmployeeMode && !gRow.hideInRegistration) || (this.templateDesignerHelper.isNewEmploymentMode && !gRow.hideInEmploymentRegistration))) {
                                    while (currentRowNr < gRow.row) {
                                        row = this.templateDesignerHelper.createRow();
                                        currentRowNr++;
                                        currentColNr = 1;
                                        groupAccordion.children[0].children[0].appendChild(row);
                                    }

                                    const colWidth = _.max([_.min([gRow.spanColumns, 4]), 1]);    // Valid range between 1 and 4
                                    let colOffset = 0;
                                    if (currentColNr !== gRow.startColumn)
                                        colOffset = gRow.startColumn - currentColNr;
                                    const col = this.templateDesignerHelper.createCol(colWidth, colOffset);
                                    currentColNr += (colWidth + colOffset);
                                    row.appendChild(col);

                                    const component = this.templateDesignerHelper.getComponentByType(gRow);
                                    const options = this.templateDesignerHelper.getComponentOptionsByType(gRow);
                                    if (gRow.recordId)
                                        options.recordId = gRow.recordId;
                                    if (gRow.invalidPosition)
                                        options.invalid = true;

                                    this.checkSystemRequiredField(gRow.type);

                                    if ((options?.systemHideInRegistration && this.templateDesignerHelper.isNewEmployeeMode) ||
                                        (options?.systemHideInEmploymentRegistration && this.templateDesignerHelper.isNewEmploymentMode))
                                        return;

                                    hasFields = true;

                                    let fieldPlaceholder: HTMLDivElement;

                                    switch (component) {
                                        case TemplateDesignerComponent.CheckBox:
                                            if (gRow.defaultValue && gRow.defaultValue.toString().toLowerCase() === 'true')
                                                this.employee[options.model] = true;
                                            fieldPlaceholder = this.templateDesignerHelper.createCheckBox(group.identity, gRow.identity, gRow.title, gRow.hideInRegistration, gRow.hideInEmploymentRegistration, gRow.hideInReport, gRow.hideInReportIfEmpty, options);
                                            break;
                                        case TemplateDesignerComponent.DatePicker:
                                            this.employee[options.model] = CalendarUtility.convertToDate(gRow.defaultValue);
                                            fieldPlaceholder = this.templateDesignerHelper.createDatePicker(group.identity, gRow.identity, gRow.title, gRow.isMandatory || options.systemRequired, gRow.hideInRegistration, gRow.hideInEmploymentRegistration, gRow.hideInReport, gRow.hideInReportIfEmpty, options);
                                            break;
                                        case TemplateDesignerComponent.Instruction:
                                            this.employee[options.model] = gRow.defaultValue;
                                            fieldPlaceholder = this.templateDesignerHelper.createInstruction(group.identity, gRow.identity, gRow.defaultValue, gRow.title);
                                            break;
                                        case TemplateDesignerComponent.Select:
                                            if (gRow.defaultValue)
                                                this.employee[options.model] = parseInt(gRow.defaultValue);
                                            fieldPlaceholder = this.templateDesignerHelper.createSelect(group.identity, gRow.identity, gRow.title, gRow.isMandatory || options.systemRequired, gRow.hideInRegistration, gRow.hideInEmploymentRegistration, gRow.hideInReport, gRow.hideInReportIfEmpty, (options as TemplateDesignerSelectOptions));
                                            break;
                                        case TemplateDesignerComponent.TextArea:
                                            if (gRow.defaultValue)
                                                this.employee[options.model] = gRow.defaultValue;
                                            if (gRow.format) {
                                                const formats: TemplateDesignerTextAreaFormats = JSON.parse(gRow.format);
                                                if (formats?.rows)
                                                    (options as TemplateDesignerTextAreaOptions).rows = formats.rows;
                                            }
                                            fieldPlaceholder = this.templateDesignerHelper.createTextArea(group.identity, gRow.identity, gRow.title, gRow.isMandatory || options.systemRequired, gRow.hideInRegistration, gRow.hideInEmploymentRegistration, gRow.hideInReport, gRow.hideInReportIfEmpty, (options as TemplateDesignerTextAreaOptions));
                                            break;
                                        case TemplateDesignerComponent.TextBox:
                                            if (gRow.defaultValue)
                                                this.employee[options.model] = gRow.defaultValue;
                                            if (options.model === 'socialSec')
                                                (options as TemplateDesignerTextBoxOptions).placeholderKey = 'time.employee.employee.socialsecplaceholder';
                                            fieldPlaceholder = this.templateDesignerHelper.createTextBox(group.identity, gRow.identity, gRow.title, gRow.isMandatory || options.systemRequired, gRow.hideInRegistration, gRow.hideInEmploymentRegistration, gRow.hideInReport, gRow.hideInReportIfEmpty, (options as TemplateDesignerTextBoxOptions));
                                            break;
                                        case TemplateDesignerComponent.EmployeeAccount:
                                            if (gRow.defaultValue)
                                                this.employee[options.model] = gRow.defaultValue;
                                            fieldPlaceholder = this.templateDesignerHelper.createEmployeeAccount(group.identity, gRow.identity, gRow.title, gRow.isMandatory || options.systemRequired, gRow.hideInRegistration, gRow.hideInEmploymentRegistration, gRow.hideInReport, gRow.hideInReportIfEmpty, options);
                                            break;
                                        case TemplateDesignerComponent.DisbursementAccount:
                                            if (gRow.defaultValue)
                                                this.employee[options.model] = gRow.defaultValue;
                                            fieldPlaceholder = this.templateDesignerHelper.createDisbursementAccount(group.identity, gRow.identity, gRow.title, gRow.isMandatory || options.systemRequired, gRow.hideInRegistration, gRow.hideInEmploymentRegistration, gRow.hideInReport, gRow.hideInReportIfEmpty, options);
                                            break;
                                        case TemplateDesignerComponent.EmploymentPriceTypes:
                                            if (gRow.defaultValue)
                                                this.employee[options.model] = gRow.defaultValue;
                                            fieldPlaceholder = this.templateDesignerHelper.createEmploymentPriceTypes(group.identity, gRow.identity, gRow.title, gRow.isMandatory || options.systemRequired, gRow.hideInRegistration, gRow.hideInEmploymentRegistration, gRow.hideInReport, gRow.hideInReportIfEmpty, options);
                                            break;
                                        case TemplateDesignerComponent.Position:
                                            if (gRow.defaultValue)
                                                this.employee[options.model] = gRow.defaultValue;
                                            fieldPlaceholder = this.templateDesignerHelper.createPosition(group.identity, gRow.identity, gRow.title, gRow.isMandatory || options.systemRequired, gRow.hideInRegistration, gRow.hideInEmploymentRegistration, gRow.hideInReport, gRow.hideInReportIfEmpty, options);
                                            break;
                                        default:
                                            col.appendChild(this.templateDesignerHelper.createSpan(`Component for field type ${gRow.type} not supported yet`));
                                    }

                                    if (fieldPlaceholder)
                                        col.appendChild(fieldPlaceholder);
                                }
                            });
                        }

                        if (hasFields || this.isEditMode) {
                            attachmentPoint.append(groupRow);
                            this.hasAccordions = true;
                        } else {
                            this.templateDesignerHelper.accordionCounter--;
                        }
                    }
                });
            }

            this.$compile(attachmentPoint)(this.$scope);

            this.$timeout(() => {
                if (!this.isEditMode && this.templateDesignerHelper.openAccordions.length === 0)
                    this.expandAll();

                if (keepSelected) {
                    this.selectGroup(this.selectedGroupId);
                    this.selectField(this.selectedGroupId, this.selectedFieldId);
                } else {
                    this.selectedGroup = undefined;
                    this.selectedGroupId = undefined;
                    this.selectedField = undefined;
                    this.selectedFieldId = undefined;
                }

                if (setDirty)
                    this.setDirty();

                if (this.onRendered)
                    this.onRendered();
            });
        });
    }

    // DESIGNER TOOLS

    private doubleClickToEdit(id: number, isGroup: boolean) {
        if (isGroup)
            this.editGroup(id);
        else
            this.editField(id);
    }

    private editGroup(id: number) {
        let group = id !== 0 ? this.employeeTemplate.employeeTemplateGroups.find(g => g.identity === id) : null;

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/EmployeeTemplates/Directives/TemplateDesigner/EditGroupDialog.html"),
            controller: EditGroupDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                types: () => { return this.groupTypes },
                group: () => { return group }
            }
        }
        this.$uibModal.open(options).result.then(result => {
            if (result) {
                if (result.group) {
                    if (!group) {
                        // Add new group
                        group = new EmployeeTemplateGroupDTO();
                        group.tmpId = --this.tmpIdCounter;
                        this.updateGroup(group, result.group);
                        group.sortOrder = this.getMaxGroupSortOrder() + 1;
                        group.employeeTemplateGroupRows = [];

                        if (!this.employeeTemplate.employeeTemplateGroups)
                            this.employeeTemplate.employeeTemplateGroups = [];

                        this.employeeTemplate.employeeTemplateGroups.push(group);
                        this.selectGroup(group.identity);
                    } else {
                        // Update existing group
                        this.updateGroup(group, result.group);
                    }

                    this.renderDesign(true, true);
                } else if (result.delete) {
                    this.deleteGroup();
                }
            }
        });
    }

    private updateGroup(group: EmployeeTemplateGroupDTO, input: EmployeeTemplateGroupDTO) {
        group.type = input.type;
        group.code = input.code;
        group.name = input.name;
        group.description = input.description;
        group.newPageBefore = input.newPageBefore;
    }

    private initDeleteGroup() {
        const keys: string[] = [
            "time.employee.employeetemplate.designer.tools.deletegroup",
            "time.employee.employeetemplate.designer.tools.deletegroup.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["time.employee.employeetemplate.designer.tools.deletegroup"], terms["time.employee.employeetemplate.designer.tools.deletegroup.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
                if (val)
                    this.deleteGroup();
            }, () => {
                // User cancelled
            });
        });
    }

    private deleteGroup() {
        const idx = this.employeeTemplate.employeeTemplateGroups.indexOf(this.selectedGroup);
        this.employeeTemplate.employeeTemplateGroups.splice(idx, 1);
        this.unselectFields();
        this.unselectGroups();

        this.renderDesign(false, true);
    }

    private editField(id: number) {
        if (!this.selectedGroup)
            return;
        if (!this.selectedGroup.employeeTemplateGroupRows)
            this.selectedGroup.employeeTemplateGroupRows = [];

        let field = this.selectedGroup.employeeTemplateGroupRows.find(r => r.identity === id);

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/EmployeeTemplates/Directives/TemplateDesigner/EditFieldDialog.html"),
            controller: EditFieldDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                templateDesignerHelper: () => { return this.templateDesignerHelper },
                fieldNames: () => { return this.fieldNames },
                accountDims: () => { return this.accountDims },
                field: () => { return field }
            }
        }
        this.$uibModal.open(options).result.then(result => {
            if (result) {
                if (result.field) {
                    if (!field) {
                        // Add new field
                        field = new EmployeeTemplateGroupRowDTO();
                        field.tmpId = --this.tmpIdCounter;
                        this.updateField(field, result.field);
                        field.spanColumns = (field.type === TermGroup_EmployeeTemplateGroupRowType.DisbursementAccount ||
                            field.type === TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount ||
                            field.type === TermGroup_EmployeeTemplateGroupRowType.EmploymentPriceTypes ||
                            field.type === TermGroup_EmployeeTemplateGroupRowType.Position ? 4 : 1);
                        field.startColumn = 1;
                        field.row = this.getMaxFieldRows(this.selectedGroup.identity) + 1;

                        this.selectedGroup.employeeTemplateGroupRows.push(field);
                        this.selectField(this.selectedGroupId, field.identity);
                    } else {
                        // Update existing field
                        this.updateField(field, result.field);
                    }

                    this.renderDesign(true, true);
                } else if (result.delete) {
                    this.deleteField();
                }
            }
        });
    }

    private updateField(field: EmployeeTemplateGroupRowDTO, input: EmployeeTemplateGroupRowDTO) {
        field.type = input.type;
        field.title = input.title;
        field.mandatoryLevel = input.mandatoryLevel;
        field.format = input.format;
        field.hideInRegistration = input.hideInRegistration;
        field.hideInEmploymentRegistration = input.hideInEmploymentRegistration;
        field.hideInReport = input.hideInReport;
        field.hideInReportIfEmpty = input.hideInReportIfEmpty;
        field.entity = input.entity;
        field.recordId = input.recordId;
    }

    private initDeleteField() {
        const keys: string[] = [
            "time.employee.employeetemplate.designer.tools.deletefield",
            "time.employee.employeetemplate.designer.tools.deletefield.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["time.employee.employeetemplate.designer.tools.deletefield"], terms["time.employee.employeetemplate.designer.tools.deletefield.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
                if (val)
                    this.deleteField();
            }, () => {
                // User cancelled
            });
        });
    }

    private deleteField() {
        const idx = this.selectedGroup.employeeTemplateGroupRows.indexOf(this.selectedField);
        this.selectedGroup.employeeTemplateGroupRows.splice(idx, 1);
        this.unselectFields();

        this.removeEmptyFieldRows(this.selectedGroup.employeeTemplateGroupId);

        this.renderDesign(true, true);
    }

    // DESIGNER EVENTS

    // GROUP

    private expandAll() {
        this.setIsOpenOnAccordions(true);
    }

    private collapseAll() {
        this.setIsOpenOnAccordions(false);
    }

    private setIsOpenOnAccordions(open: boolean) {
        const designer = this.getAttachmentPoint();
        const accordions = designer.find('soe-accordion');
        for (let i = 1; i <= accordions.length; i++) {
            this[`accordion${i}isopen`] = open;
        }
    }

    private rememberOpenAccordions() {
        this.templateDesignerHelper.openAccordions = [];
        const designer = this.getAttachmentPoint();
        const accordions = designer.find('soe-accordion');
        for (let i = 1; i <= accordions.length; i++) {
            if (this[`accordion${i}isopen`])
                this.templateDesignerHelper.openAccordions.push(i);
        }
    }

    private selectGroup(id: number) {
        if (this.selectedGroupId !== id)
            this.unselectFields();

        this.unselectGroups();

        this.selectedGroup = this.employeeTemplate.employeeTemplateGroups.find(g => g.identity === id);
        if (this.selectedGroup) {
            this.selectedGroupId = id;

            const elem = this.getGroupPlaceholder(id);
            if (elem)
                elem.classList.add('selected');
        }
    }

    private unselectGroups() {
        this.selectedGroup = undefined;
        this.selectedGroupId = 0;

        const designer = this.getAttachmentPoint();
        if (designer) {
            const divs = designer.find('.group-placeholder');
            _.forEach(divs, div => {
                div.classList.remove('selected');
            });
        }
    }

    private moveGroupFirst() {
        if (this.selectedGroup.sortOrder > 1) {
            this.selectedGroup.sortOrder = 0;
            this.resortGroups();
            this.renderDesign(true, true);
        }
    }

    private moveGroupUp() {
        if (this.selectedGroup.sortOrder > 1) {
            this.selectedGroup.sortOrder -= 1.5;
            this.resortGroups();
            this.renderDesign(true, true);
        }
    }

    private moveGroupDown() {
        const maxSortOrder = this.getMaxGroupSortOrder();
        if (this.selectedGroup.sortOrder < maxSortOrder) {
            this.selectedGroup.sortOrder += 1.5;
            this.resortGroups();
            this.renderDesign(true, true);
        }
    }

    private moveGroupLast() {
        const maxSortOrder = this.getMaxGroupSortOrder();
        if (this.selectedGroup.sortOrder < maxSortOrder) {
            this.selectedGroup.sortOrder = maxSortOrder + 1;
            this.resortGroups();
            this.renderDesign(true, true);
        }
    }

    private resortGroups() {
        if (this.employeeTemplate?.employeeTemplateGroups) {
            let sortOrder = 1;
            _.orderBy(this.employeeTemplate.employeeTemplateGroups, g => g.sortOrder).forEach(group => {
                group.sortOrder = sortOrder++;
            });
        }
    }

    // FIELD

    private selectField(groupId: number, id: number) {
        this.unselectFields();

        const group = this.employeeTemplate.employeeTemplateGroups.find(g => g.identity === groupId);
        if (group) {
            if (this.selectedGroupId !== groupId)
                this.selectGroup(groupId);

            const field = group.employeeTemplateGroupRows.find(r => r.identity === id);
            if (field) {
                this.selectedField = field;
                this.selectedFieldId = id;

                const elem = this.getFieldPlaceholder(id);
                if (elem)
                    elem.classList.add('selected');
            }
        }
    }

    private unselectFields() {
        this.selectedField = undefined;
        this.selectedFieldId = 0;

        const designer = this.getAttachmentPoint();
        if (designer) {
            const divs = designer.find('.field-placeholder');
            _.forEach(divs, div => {
                div.classList.remove('selected');
            });
        }
    }

    private setFieldSize(size: number) {
        if (this.selectedField.startColumn + size < 6) {
            this.selectedField.spanColumns = size;
            this.renderDesign(true, true);
        }
    }

    private get canMoveUp(): boolean {
        return this.selectedField.row > 1 || this.getOtherFieldsOnRow().length > 0;
    }

    private get canMoveDown(): boolean {
        return this.selectedField.row < this.getMaxFieldRows(this.selectedGroupId) || this.getOtherFieldsOnRow().length > 0;
    }

    private moveFieldFirst() {
        if (this.canMoveUp) {
            let fields = this.getGroupFields(this.selectedGroupId);
            fields.forEach(f => {
                f.row++;
            });

            this.selectedField.row = 1;
            this.renumberColumns(this.selectedGroupId, this.selectedField.row);
            this.removeEmptyFieldRows(this.selectedGroupId);
            this.renderDesign(true, true);
        }
    }

    private moveFieldUp(insert: boolean) {
        if (!this.canMoveUp)
            return;

        this.selectedField.row--;

        if (insert) {
            if (this.placeFieldOnSameRow()) {
                this.renumberColumns(this.selectedGroupId, this.selectedField.row);
                this.removeEmptyFieldRows(this.selectedGroupId);
                this.renderDesign(true, true);
            } else {
                this.moveFieldUp(false);
            }
        } else {
            let fields = this.getGroupFields(this.selectedGroupId);
            let otherFields = this.getOtherFieldsOnRow();
            if (otherFields.length > 0) {
                // Move all other rows down
                fields.filter(f => f.row >= this.selectedField.row && f.identity !== this.selectedField.identity).forEach(f => {
                    f.row++;
                });
            } else {
                // Swap place with row above
                fields.filter(f => f.row === this.selectedField.row && f.identity !== this.selectedField.identity).forEach(f => {
                    f.row++;
                });
            }
            this.removeEmptyFieldRows(this.selectedGroupId);
            this.renderDesign(true, true);
        }
    }

    private moveFieldDown(insert: boolean) {
        if (!this.canMoveDown)
            return;

        this.selectedField.row++;

        if (insert) {
            if (this.placeFieldOnSameRow()) {
                this.renumberColumns(this.selectedGroupId, this.selectedField.row);
                this.removeEmptyFieldRows(this.selectedGroupId);
                this.renderDesign(true, true);
            } else {
                this.moveFieldDown(false);
            }
        } else {
            let fields = this.getGroupFields(this.selectedGroupId);
            let otherFields = this.getOtherFieldsOnRow();
            if (otherFields.length > 0) {
                // Move all other rows down
                fields.filter(f => f.row >= this.selectedField.row && f.identity !== this.selectedField.identity).forEach(f => {
                    f.row++;
                });
            } else {
                // Swap place with row below
                fields.filter(f => f.row === this.selectedField.row && f.identity !== this.selectedField.identity).forEach(f => {
                    f.row--;
                });
            }
            this.removeEmptyFieldRows(this.selectedGroupId);
            this.renderDesign(true, true);
        }
    }

    private moveFieldLast() {
        if (this.canMoveDown) {
            this.selectedField.row = this.getMaxFieldRows(this.selectedGroupId) + 1;
            this.renumberColumns(this.selectedGroupId, this.selectedField.row);
            this.removeEmptyFieldRows(this.selectedGroupId);
            this.renderDesign(true, true);
        }
    }

    private moveFieldLeft() {
        if (this.selectedField.startColumn > 1) {
            let otherFields = this.getOtherFieldsOnRow();
            let moveSteps = 0;
            otherFields.filter(f => f.startColumn < this.selectedField.startColumn && f.startColumn + f.spanColumns - 1 >= this.selectedField.startColumn - 1).forEach(f => {
                f.startColumn += this.selectedField.spanColumns;
                moveSteps += f.spanColumns;
            });

            if (moveSteps === 0)
                moveSteps = 1;

            this.selectedField.startColumn -= moveSteps;
            this.renumberColumns(this.selectedGroupId, this.selectedField.row);
            this.renderDesign(true, true);
        }
    }

    private moveFieldRight() {
        if (this.selectedField.startColumn + this.selectedField.spanColumns < 5) {
            let otherFields = this.getOtherFieldsOnRow();
            let moveSteps = 0;
            otherFields.filter(f => f.startColumn > this.selectedField.startColumn && f.startColumn <= this.selectedField.startColumn + this.selectedField.spanColumns).forEach(f => {
                f.startColumn -= this.selectedField.spanColumns;
                moveSteps += f.spanColumns;
            });

            if (moveSteps === 0)
                moveSteps = 1;

            this.selectedField.startColumn += moveSteps;
            this.renumberColumns(this.selectedGroupId, this.selectedField.row);
            this.renderDesign(true, true);
        }
    }

    private placeFieldOnSameRow(): boolean {
        let otherFields = this.getOtherFieldsOnRow();
        if (otherFields.filter(f => f.startColumn === this.selectedField.startColumn).length > 0) {
            let wantedColumn = _.max(otherFields.map(f => f.startColumn + f.spanColumns));
            if (wantedColumn + this.selectedField.spanColumns < 6) {
                this.selectedField.startColumn = wantedColumn;
                return true;
            } else {
                return false;
            }
        }

        return true;
    }

    // HELP-METHODS

    private getGroupPlaceholder(id: number) {
        return document.getElementById(`group_${this.employeeTemplate.employeeTemplateId}_${id}`);
    }

    private getGroupElem(id: number) {
        const divElem = this.getGroupPlaceholder(id);
        return divElem && divElem.children.length > 0 ? divElem.children[0] : null;
    }

    private getFieldPlaceholder(id: number) {
        return document.getElementById(`field_${this.employeeTemplate.employeeTemplateId}_${id}`);
    }

    private getFieldElem(id: number) {
        const divElem = this.getFieldPlaceholder(id);
        if (divElem && divElem.children.length > 0) {
            if (divElem.children.length === 1)
                return divElem.children[0];

            // If current field is selected, the edit icon will be the first child
            if (divElem.children.length > 1 && (divElem.children[0].classList.contains('field-placeholder-icon') || divElem.children[0].classList.contains('field-placeholder-icon-with-margin')))
                return divElem.children[divElem.children.length - 1];
        }

        return null;
    }

    private getFieldNameByType(type: TermGroup_EmployeeTemplateGroupRowType): string {
        return this.fieldNames.find(f => f.id === type)?.name;
    }

    private getMaxGroupSortOrder(): number {
        let sortOrder = 0;

        if (this.employeeTemplate?.employeeTemplateGroups) {
            this.employeeTemplate.employeeTemplateGroups.forEach(group => {
                if (group.sortOrder > sortOrder)
                    sortOrder = group.sortOrder;
            });
        }

        return sortOrder;
    }

    private getGroupFields(groupId: number): EmployeeTemplateGroupRowDTO[] {
        if (this.employeeTemplate?.employeeTemplateGroups) {
            const group = this.employeeTemplate.employeeTemplateGroups.find(g => g.identity === groupId);
            if (group?.employeeTemplateGroupRows) {
                return group.employeeTemplateGroupRows;
            }
        }

        return [];
    }

    private getMaxFieldRows(groupId: number): number {
        let row = 0;

        this.getGroupFields(groupId).forEach(gRow => {
            if (gRow.row > row)
                row = gRow.row;
        });

        return row;
    }

    private getRowFields(groupId: number, row: number): EmployeeTemplateGroupRowDTO[] {
        return _.orderBy(this.getGroupFields(groupId).filter(r => r.row === row), f => f.startColumn);
    }

    private getOtherFieldsOnRow(): EmployeeTemplateGroupRowDTO[] {
        return _.orderBy(this.getRowFields(this.selectedGroupId, this.selectedField.row).filter(r => r.identity !== this.selectedField.identity), f => f.startColumn);
    }

    private renumberColumns(groupId: number, row: number) {
        let fields = this.getRowFields(groupId, row);
        let prevField: EmployeeTemplateGroupRowDTO;
        let targetColumn = 1;
        fields.forEach(field => {
            if (prevField)
                targetColumn = prevField.startColumn + prevField.spanColumns;

            if (targetColumn > 4)
                targetColumn = 4;

            field.startColumn = targetColumn;
            prevField = field;
        });
    }

    private renumberRows(groupId: number) {
        let fields = this.getGroupFields(groupId);

        let minRowNr = 1;
        fields.forEach(gRow => {
            if (gRow.row < minRowNr)
                minRowNr = gRow.row;
        });

        if (minRowNr !== 1) {
            fields.forEach(gRow => {
                gRow.row -= (minRowNr - 1);
            });
        }
    }

    private removeEmptyFieldRows(groupId: number) {
        this.renumberRows(groupId);

        const maxRows = this.getMaxFieldRows(groupId);

        if (this.employeeTemplate?.employeeTemplateGroups) {
            const group = this.employeeTemplate.employeeTemplateGroups.find(g => g.identity === groupId);
            if (group?.employeeTemplateGroupRows) {
                let row = 1;
                while (row <= maxRows) {
                    if (group.employeeTemplateGroupRows.filter(r => r.row === row).length === 0) {
                        _.filter(group.employeeTemplateGroupRows, r => r.row > row).forEach(gRow => {
                            gRow.row--;
                        });
                    }
                    row++;
                }
            }
        }
    }

    private setDefaultValuesFromModel() {
        if (this.employeeTemplate?.employeeTemplateGroups) {
            this.employeeTemplate.employeeTemplateGroups.forEach(group => {
                if (group.employeeTemplateGroupRows) {
                    group.employeeTemplateGroupRows.forEach(gRow => {
                        const elem = this.getFieldElem(gRow.identity);
                        if (elem) {
                            const model = elem.getAttribute('model');
                            if (model) {
                                const value = this.employee[model.replace('ctrl.employee.', '')];
                                if (value !== undefined) {
                                    gRow.defaultValue = value;
                                }
                            }
                        }
                    });
                }
            });
        }
    }

    getExtraFieldEmployeeItems(extraFieldId: number): IExtraFieldValueDTO[] {
        let extraField = this.templateDesignerHelper.extraFieldsEmployee.find(f => f.extraFieldId === extraFieldId);
        return extraField?.extraFieldValues ?? [];
    }

    // COMPONENT EVENTS

    private setDirty(model: string = '') {
        this.$timeout(() => {
            this.setDefaultValuesFromModel();
            if (this.onChange)
                this.onChange();

            if (model && this.onModelChange)
                this.onModelChange({ model: model, value: this.employee[model] });
        });
    }

    private textBoxChanged(model: string) {
        let notify = false;
        this.$timeout(() => {
            if (model === 'socialSec') {
                notify = true;
            } else if (model === 'employmentWorkTimeWeekFormatted') {
                const span = CalendarUtility.parseTimeSpan(this.employee['employmentWorkTimeWeekFormatted']);
                this.employee['employmentWorkTimeWeek'] = CalendarUtility.timeSpanToMinutes(span);
            } else if (model === 'primaryEmploymentWorkTimeWeekFormatted') {
                const span = CalendarUtility.parseTimeSpan(this.employee['primaryEmploymentWorkTimeWeekFormatted']);
                this.employee['primaryEmploymentWorkTimeWeek'] = CalendarUtility.timeSpanToMinutes(span);
            } else if (model === 'employmentFullTimeWorkWeekFormatted') {
                const span = CalendarUtility.parseTimeSpan(this.employee['employmentFullTimeWorkWeekFormatted']);
                this.employee['employmentFullTimeWorkWeek'] = CalendarUtility.timeSpanToMinutes(span);
            } else if (model === 'totalEmploymentWorkTimeWeekFormatted') {
                const span = CalendarUtility.parseTimeSpan(this.employee['totalEmploymentWorkTimeWeekFormatted']);
                this.employee['totalEmploymentWorkTimeWeek'] = CalendarUtility.timeSpanToMinutes(span);
            }

            this.setDirty(notify ? model : '');
        });
    }

    private selectChanged(model: string) {
        this.$timeout(() => {
            // Special requirement for employmentStopDate
            // Required if not employment type is permanent
            if (model === 'employmentType') {
                // This will make the * appear
                // Actual validation is done in CreateFromTemplateValidationDirective
                const employmentType = this.employmentTypes.find(e => e.id === this.employee['employmentType']);
                const isPermanent = employmentType?.type === TermGroup_EmploymentType.SE_Permanent;
                this.employee.employmentStopDateRequired = !isPermanent;
            }
            this.setDirty();
        });
    }

    private employeeAccountChanged(jsonString: string) {
        this.employee['hierarchicalAccount'] = jsonString;
        this.setDirty('hierarchicalAccount');
    }

    private disbursementAccountChanged(jsonString: string) {
        this.employee['disbursementAccount'] = jsonString;
        this.setDirty();
    }

    private employmentPriceTypesChanged(jsonString: string) {
        this.employee['employmentPriceTypes'] = jsonString;
        this.setDirty();
    }

    private positionChanged(jsonString: string) {
        this.employee['position'] = jsonString;
        this.setDirty();
    }

    // VALIDATION

    private checkSystemRequiredField(field: TermGroup_EmployeeTemplateGroupRowType) {
        if (field) {
            if (_.includes(this.remainingSystemRequiredFields, field))
                this.remainingSystemRequiredFields.splice(this.remainingSystemRequiredFields.indexOf(field), 1);
        }

        this.remainingSystemRequiredFieldNames = '';
        this.remainingSystemRequiredFields.forEach(f => {
            const fieldName = this.fieldNames.find(n => n.id === f);
            if (fieldName)
                this.remainingSystemRequiredFieldNames += fieldName.name + '<br/>';
        });

        this.hasRemainingSystemRequiredFields = this.remainingSystemRequiredFields.length > 0;
    }

    private validateFieldPositions() {
        this.hasInvalidPosition = false;

        if (this.employeeTemplate?.employeeTemplateGroups) {
            _.orderBy(this.employeeTemplate.employeeTemplateGroups, g => g.sortOrder).forEach(group => {
                if (group.employeeTemplateGroupRows) {
                    const groupedRows = _.groupBy(_.orderBy(group.employeeTemplateGroupRows, r => r.startColumn), g => g.row);
                    _.forEach(groupedRows, row => {
                        let currentRow = 1;
                        let currentCol = 1;
                        _.forEach(row, field => {
                            field.invalidPosition = false;

                            if (field.row === currentRow) {
                                if (field.startColumn < currentCol || (field.startColumn + field.spanColumns - 1) > 4) {
                                    field.invalidPosition = true;
                                    this.hasInvalidPosition = true;
                                }
                            } else {
                                currentRow = field.row;
                            }
                            currentCol = field.startColumn + field.spanColumns;
                        });
                    });
                }
            });
        }
    }
}