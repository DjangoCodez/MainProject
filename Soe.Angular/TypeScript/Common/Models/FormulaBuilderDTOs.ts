import { PriceRuleItemType, SoeTimeRuleOperatorType, PriceRuleValueType } from "../../Util/CommonEnumerations";

export interface IWidget {
    isReadonly: boolean;
    internalId: number;
    priceRuleType: PriceRuleItemType;
    priceRuleValueType: PriceRuleValueType;
    timeRuleType: SoeTimeRuleOperatorType;
    isExpression: boolean;
    isOperator: boolean;
    isFormula: boolean;
    hasSettings: boolean;
    widgetName: string;
    title: string;
    titleIcon: string;
    widgetClass: string;
    widgetWidth: number;
    widgetWidthInFormula: number;
    sort: number;
    data: any;
}

export class WidgetBase implements IWidget {
    isReadonly: boolean;
    internalId: number;
    priceRuleType: PriceRuleItemType;
    priceRuleValueType: PriceRuleValueType;
    timeRuleType: SoeTimeRuleOperatorType;
    isExpression: boolean;
    isOperator: boolean;
    isFormula: boolean;
    hasSettings: boolean;
    widgetName: string;
    title: string;
    titleIcon: string;
    widgetClass: string;
    widgetWidth: number;
    widgetWidthInFormula: number;
    sort: number;
    data: any;

    public get actualWidth(): number {
        return this.isFormula ? this.widgetWidthInFormula : this.widgetWidth;
    }
}

export class ExpressionWidget extends WidgetBase {
    constructor(widgetName: string, widgetWidth?: number, widgetWidthInFormula?: number, data?: any) {
        super();
        this.isExpression = true;
        this.widgetName = widgetName;
        this.widgetWidth = widgetWidth || 170;
        this.widgetWidthInFormula = widgetWidthInFormula ? widgetWidthInFormula : this.widgetWidth;
        if (data)
            this.data = data;
    }
}

export class OperatorWidget extends WidgetBase {
    constructor(widgetName: string, widgetWidth?: number, widgetWidthInFormula?: number, data?: any) {
        super();
        this.isOperator = true;
        this.widgetName = widgetName;
        this.widgetWidth = widgetWidth || 50;
        this.widgetWidthInFormula = widgetWidthInFormula ? widgetWidthInFormula : this.widgetWidth;
        if (data)
            this.data = data;
    }
}

export class FormulaWidget extends WidgetBase {
    constructor(isReadonly: boolean, widgetName: string, widgetWidth?: number, widgetWidthInFormula?: number, isStandby: boolean = false) {
        super();
        this.isReadonly = isReadonly;
        this.widgetName = widgetName;
        this.widgetWidth = widgetWidth;
        this.widgetWidthInFormula = widgetWidthInFormula ? widgetWidthInFormula : this.widgetWidth;

        if (isStandby) {
            if (!this.data)
                this.data = {};
            this.data.isStandby = true;
        }
    }
}