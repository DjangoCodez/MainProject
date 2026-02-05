import { SoeModule, SoeEntityState, WildCard, TermGroup_AttestRuleRowLeftValueType, TermGroup_AttestRuleRowRightValueType } from "../../Util/CommonEnumerations";
import { IAttestRuleHeadDTO, IAttestRuleRowDTO, IAttestRuleHeadGridDTO } from "../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "./SmallGenericType";

export class AttestRuleHeadDTO implements IAttestRuleHeadDTO {
    actorCompanyId: number;
    attestRuleHeadId: number;
    attestRuleRows: AttestRuleRowDTO[];
    created: Date;
    createdBy: string;
    dayTypeCompanyId: number;
    dayTypeCompanyName: string;
    dayTypeId: number;
    dayTypeName: string;
    description: string;
    employeeGroupIds: number[];
    isSelected: boolean;
    modified: Date;
    modifiedBy: string;
    module: SoeModule;
    name: string;
    scheduledJobHeadId: number;
    state: SoeEntityState;

    get isActive(): boolean {
        return (this.state === SoeEntityState.Active);
    }
    set isActive(active: boolean) {
        this.state = (active ? SoeEntityState.Active : SoeEntityState.Inactive);
    }
}

export class AttestRuleHeadGridDTO implements IAttestRuleHeadGridDTO {
    attestRuleHeadId: number;
    dayTypeName: string;
    description: string;
    employeeGroupNames: string;
    name: string;
    state: SoeEntityState;

    get isActive(): boolean {
        return (this.state === SoeEntityState.Active);
    }
    set isActive(active: boolean) {
        this.state = (active ? SoeEntityState.Active : SoeEntityState.Inactive);
    }
}

export class AttestRuleRowDTO implements IAttestRuleRowDTO {
    attestRuleHeadId: number;
    attestRuleRowId: number;
    comparisonOperator: WildCard;
    comparisonOperatorString: string;
    created: Date;
    createdBy: string;
    leftValueId: number;
    leftValueIdName: string;
    leftValueType: TermGroup_AttestRuleRowLeftValueType;
    leftValueTypeName: string;
    minutes: number;
    modified: Date;
    modifiedBy: string;
    rightValueId: number;
    rightValueIdName: string;
    rightValueType: TermGroup_AttestRuleRowRightValueType;
    rightValueTypeName: string;
    showLeftValueId: boolean;
    showRightValueId: boolean;
    state: SoeEntityState;

    constructor() {
    }

    public setLeftValueTypeName(leftOperators: SmallGenericType[]) {
        let leftType = _.find(leftOperators, o => o.id === this.leftValueType);
        this.leftValueTypeName = leftType ? leftType.name : '';
    }

    public setLeftValueIdName(timeCodes:SmallGenericType[], payrollProducts:SmallGenericType[], invoiceProducts:SmallGenericType[]) {
        this.leftValueIdName = '';
        if (this.leftValueId && this.leftValueType) {
            if (this.leftValueType == TermGroup_AttestRuleRowLeftValueType.TimeCode) {
                let timeCode = _.find(timeCodes, t => t.id === this.leftValueId);
                this.leftValueIdName = timeCode ? timeCode.name : '';
            } else if (this.leftValueType == TermGroup_AttestRuleRowLeftValueType.PayrollProduct) {
                let prod = _.find(payrollProducts, p => p.id === this.leftValueId);
                this.leftValueIdName = prod ? prod.name : '';
            } else if (this.leftValueType == TermGroup_AttestRuleRowLeftValueType.InvoiceProduct) {
                let prod = _.find(invoiceProducts, p => p.id === this.leftValueId);
                this.leftValueIdName = prod ? prod.name : '';
            }
        }
    }

    public setComparisonOperatorString(comparisonOperators: SmallGenericType[]) {
        let comparisonOperator = _.find(comparisonOperators, o => o.id === this.comparisonOperator);
        this.comparisonOperatorString = comparisonOperator ? comparisonOperator.name : '';
    }

    public setRightValueTypeName(rightOperators: SmallGenericType[]) {
        let rightType = _.find(rightOperators, o => o.id === this.rightValueType);
        this.rightValueTypeName = rightType ? rightType.name : '';
    }

    public setRightValueIdName(timeCodes: SmallGenericType[], payrollProducts: SmallGenericType[], invoiceProducts: SmallGenericType[]) {
        this.rightValueIdName = '';
        if (this.rightValueId && this.rightValueType) {
            if (this.rightValueType == TermGroup_AttestRuleRowRightValueType.TimeCode) {
                let timeCode = _.find(timeCodes, t => t.id === this.rightValueId);
                this.rightValueIdName = timeCode ? timeCode.name : '';
            } else if (this.rightValueType == TermGroup_AttestRuleRowRightValueType.PayrollProduct) {
                let prod = _.find(payrollProducts, p => p.id === this.rightValueId);
                this.rightValueIdName = prod ? prod.name : '';
            } else if (this.rightValueType == TermGroup_AttestRuleRowRightValueType.InvoiceProduct) {
                let prod = _.find(invoiceProducts, p => p.id === this.rightValueId);
                this.rightValueIdName = prod ? prod.name : '';
            }
        }
    }

    public get leftValueTypeIsTimeCode(): boolean {
        return this.leftValueType == TermGroup_AttestRuleRowLeftValueType.TimeCode;
    }

    public get leftValueTypeIsPayrollProduct(): boolean {
        return this.leftValueType == TermGroup_AttestRuleRowLeftValueType.PayrollProduct;
    }

    public get leftValueTypeIsInvoiceProduct(): boolean {
        return this.leftValueType == TermGroup_AttestRuleRowLeftValueType.InvoiceProduct;
    }

    public get rightValueTypeIsTimeCode(): boolean {
        return this.rightValueType == TermGroup_AttestRuleRowRightValueType.TimeCode;
    }

    public get rightValueTypeIsPayrollProduct(): boolean {
        return this.rightValueType == TermGroup_AttestRuleRowRightValueType.PayrollProduct;
    }

    public get rightValueTypeIsInvoiceProduct(): boolean {
        return this.rightValueType == TermGroup_AttestRuleRowRightValueType.InvoiceProduct;
    }
}