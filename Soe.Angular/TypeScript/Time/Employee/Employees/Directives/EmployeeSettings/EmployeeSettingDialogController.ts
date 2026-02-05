import { EmployeeSettingDTO, EmployeeSettingTypeDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { IValidationSummaryHandler } from "../../../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../../../Core/Handlers/validationsummaryhandlerfactory";
import { TermGroup_EmployeeSettingType } from "../../../../../Util/CommonEnumerations";

export class EmployeeSettingDialogController {

    private setting: EmployeeSettingDTO;
    private filteredTypes: EmployeeSettingTypeDTO[];
    private selectedType: EmployeeSettingTypeDTO;

    private validationHandler: IValidationSummaryHandler;
    private dialogform: ng.IFormController;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private groupTypes: EmployeeSettingTypeDTO[],
        private types: EmployeeSettingTypeDTO[],
        private settings: EmployeeSettingDTO[],
        setting: EmployeeSettingDTO,
        private isNew: boolean) {

        this.validationHandler = validationSummaryHandlerFactory.create();

        this.setting = new EmployeeSettingDTO();
        angular.extend(this.setting, setting);
        if (this.isNew) {
            //this.setting.validFromDate = CalendarUtility.getDateToday();
            if (this.groupTypes.length === 1) {
                this.setting.employeeSettingGroupType = this.groupTypes[0].employeeSettingGroupType;
                this.filterTypes();
            }
        } else {
            this.filterTypes();
            this.setSelectedType();
        }
    }

    private groupTypeChanged() {
        this.$timeout(() => {
            this.filterTypes();
            if (this.filteredTypes.length === 1) {
                this.setting.employeeSettingType = this.filteredTypes[0].employeeSettingType;
                this.typeChanged();
            }
        });
    }

    private typeChanged() {
        this.$timeout(() => {
            this.setSelectedType();
            if (this.selectedType)
                this.setting.dataType = this.selectedType.dataType;      
            this.setting.clearData();
        });
    }

    private setSelectedType() {
        this.selectedType = this.types.find(t => t.employeeSettingType === this.setting.employeeSettingType);
    }

    private filterTypes() {
        this.filteredTypes = [];
        this.types.forEach(t => {
            // Filter on specified group
            if (t.employeeSettingGroupType === this.setting.employeeSettingGroupType && t.employeeSettingType !== TermGroup_EmployeeSettingType.None) {
                this.filteredTypes.push(t);
            }
        });
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ setting: this.setting });
    }

    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            const errors = this['dialogform'].$error;

            if (errors['groupMandatory'])
                mandatoryFieldKeys.push("common.group");
            if (errors['typeMandatory'])
                mandatoryFieldKeys.push("common.type");
            if (errors['validDates'])
                validationErrorKeys.push("error.invaliddaterange");
            if (errors['validFromDate'])
                validationErrorKeys.push("time.employee.employeesetting.duplicatefromdate");
        });
    }
}
