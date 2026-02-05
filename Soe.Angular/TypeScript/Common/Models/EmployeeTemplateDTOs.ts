import { IEmployeeTemplateDisbursementAccountDTO, IEmployeeTemplateDTO, IEmployeeTemplateEmployeeAccountDTO, IEmployeeTemplateEmploymentPriceTypeDTO, IEmployeeTemplateGridDTO, IEmployeeTemplateGroupDTO, IEmployeeTemplateGroupRowDTO, IEmployeeTemplatePositionDTO, ISaveEmployeeFromTemplateHeadDTO, ISaveEmployeeFromTemplateRowDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEntityState, SoeEntityType, TermGroup_EmployeeTemplateGroupRowType, TermGroup_EmployeeTemplateGroupType } from "../../Util/CommonEnumerations";

export class EmployeeTemplateDTO implements IEmployeeTemplateDTO {
    actorCompanyId: number;
    code: string;
    created: Date;
    createdBy: string;
    description: string;
    employeeCollectiveAgreementId: number;
    employeeTemplateGroups: EmployeeTemplateGroupDTO[];
    employeeTemplateId: number;
    externalCode: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    title: string;

    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    public setTypes() {
        if (this.employeeTemplateGroups) {
            this.employeeTemplateGroups = this.employeeTemplateGroups.map(g => {
                const gObj: EmployeeTemplateGroupDTO = new EmployeeTemplateGroupDTO();
                angular.extend(gObj, g);
                gObj.setTypes();
                return gObj;
            });
        } else {
            this.employeeTemplateGroups = [];
        }
    }
}

export class EmployeeTemplateGridDTO implements IEmployeeTemplateGridDTO {
    code: string;
    description: string;
    employeeCollectiveAgreementName: string;
    employeeTemplateId: number;
    externalCode: string;
    name: string;
    state: SoeEntityState;

    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }
}

export class EmployeeTemplateGroupDTO implements IEmployeeTemplateGroupDTO {
    code: string;
    created: Date;
    createdBy: string;
    description: string;
    employeeTemplateGroupId: number;
    employeeTemplateGroupRows: EmployeeTemplateGroupRowDTO[];
    employeeTemplateId: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    newPageBefore: boolean;
    sortOrder: number;
    state: number;
    type: TermGroup_EmployeeTemplateGroupType;

    // Extensions
    tmpId: number;

    get identity(): number {
        return this.employeeTemplateGroupId ? this.employeeTemplateGroupId : this.tmpId;
    }

    get hasRows(): boolean {
        return this.employeeTemplateGroupRows.length > 0;
    }

    get hasRegistrationRows(): boolean {
        return this.employeeTemplateGroupRows.filter(r => !r.hideInRegistration).length > 0;
    }

    get hasEmploymentRegistrationRows(): boolean {
        return this.employeeTemplateGroupRows.filter(r => !r.hideInEmploymentRegistration).length > 0;
    }

    public setTypes() {
        if (this.employeeTemplateGroupRows) {
            this.employeeTemplateGroupRows = this.employeeTemplateGroupRows.map(r => {
                const rObj: EmployeeTemplateGroupRowDTO = new EmployeeTemplateGroupRowDTO();
                angular.extend(rObj, r);
                return rObj;
            });
        } else {
            this.employeeTemplateGroupRows = [];
        }
    }
}

export class EmployeeTemplateGroupRowDTO implements IEmployeeTemplateGroupRowDTO {
    comment: string;
    created: Date;
    createdBy: string;
    defaultValue: string;
    employeeTemplateGroupId: number;
    employeeTemplateGroupRowId: number;
    entity: SoeEntityType;
    format: string;
    hideInEmploymentRegistration: boolean;
    hideInRegistration: boolean;
    hideInReport: boolean;
    hideInReportIfEmpty: boolean;
    mandatoryLevel: number;
    modified: Date;
    modifiedBy: string;
    recordId: number;
    registrationLevel: number;
    row: number;
    spanColumns: number;
    startColumn: number;
    state: number;
    title: string;
    type: TermGroup_EmployeeTemplateGroupRowType;

    // Extensions
    tmpId: number;

    invalidPosition: boolean;

    get identity(): number {
        return this.employeeTemplateGroupRowId ? this.employeeTemplateGroupRowId : this.tmpId;
    }

    get isMandatory(): boolean {
        return this.mandatoryLevel > 0;
    }
    set isMandatory(value: boolean) {
        this.mandatoryLevel = value ? 1 : 0;
    }
}

export class SaveEmployeeFromTemplateHeadDTO implements ISaveEmployeeFromTemplateHeadDTO {
    hasExtraFields: boolean;
    date: Date;
    employeeId: number;
    employeeTemplateId: number;
    printEmploymentContract: boolean;
    rows: SaveEmployeeFromTemplateRowDTO[];
}

export class SaveEmployeeFromTemplateRowDTO implements ISaveEmployeeFromTemplateRowDTO {
    entity: SoeEntityType;
    extraValue: string;
    isExtraField: boolean;
    recordId: number;
    sort: number;
    startDate: Date;
    stopDate: Date;
    type: TermGroup_EmployeeTemplateGroupRowType;
    value: string;
}

export class EmployeeTemplateDisbursementAccountDTO implements IEmployeeTemplateDisbursementAccountDTO {
    accountNr: string;
    clearingNr: string;
    dontValidateAccountNr: boolean;
    method: number;
}

export class EmployeeTemplateEmployeeAccountDTO implements IEmployeeTemplateEmployeeAccountDTO {
    accountDimId: number;
    accountId: number;
    childAccountId: number;
    dateFrom: Date;
    dateFromString: string;
    dateTo: Date;
    dateToString: string;
    default: boolean;
    mainAllocation: boolean;
    subChildAccountId: number;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }
}

export class EmployeeTemplateEmploymentPriceTypeDTO implements IEmployeeTemplateEmploymentPriceTypeDTO {
    amount: number;
    fromDate: Date;
    fromDateString: string;
    payrollGroupAmount: number;
    payrollLevelId: number;
    payrollPriceTypeId: number;
    payrollPriceTypeName: string;

    // Extensions
    hasPayrollLevels: boolean;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}

export class EmployeeTemplatePositionDTO implements IEmployeeTemplatePositionDTO {
    default: boolean;
    positionId: number;
}

export class TemplateDesignerComponentOptions {
    model: string;
    isMultiple: boolean;
    recordId: number;
    systemRequired: boolean;
    required: boolean;
    readOnly: boolean;
    systemHideInRegistration: boolean;
    systemHideInEmploymentRegistration: boolean;
    invalid: boolean;
}

export class TemplateDesignerCheckBoxOptions extends TemplateDesignerComponentOptions {
}

export class TemplateDesignerDatePickerOptions extends TemplateDesignerComponentOptions {
}

export class TemplateDesignerSelectOptions extends TemplateDesignerComponentOptions {
    itemIdField: string;
    itemNameField: string;
    itemsName: string;
}

export class TemplateDesignerTextBoxOptions extends TemplateDesignerComponentOptions {
    alignRight: boolean;
    decimals: number;
    maxLength: number;
    numeric: boolean;
    isTime: boolean;
    placeholderKey: string;
}

export class TemplateDesignerTextAreaOptions extends TemplateDesignerComponentOptions {
    maxLength: number;
    rows: number;
}

export class TemplateDesignerComponentFormats {
}

export class TemplateDesignerTextAreaFormats extends TemplateDesignerComponentFormats {
    rows: number;
}
