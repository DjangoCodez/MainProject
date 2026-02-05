import { IInsight, IMatrixDefinition, IMatrixDefinitionColumn, IMatrixDefinitionColumnOptions, IMatrixField, IMatrixFieldOption, IMatrixLayoutColumn, IMatrixResult, System } from "../../Scripts/TypeLite.Net4";
import { MatrixDataType, MatrixFieldSetting, TermGroup_FixedInsights, TermGroup_InsightChartTypes, TermGroup_MatrixDateFormatOption, TermGroup_MatrixGroupAggOption } from "../../Util/CommonEnumerations";

export class MatrixResult implements IMatrixResult {
    jsonRows: System.Collections.Generic.IKeyValuePair[];
    key: number;
    matrixDefinition: MatrixDefinition;
    matrixDefinitions: IMatrixDefinition[];
    matrixFields: MatrixField[];

    public setTypes() {
        let obj = new MatrixDefinition();
        angular.extend(obj, this.matrixDefinition);
        this.matrixDefinition = obj;

        if (this.matrixFields) {
            this.matrixFields = this.matrixFields.map(f => {
                let fObj = new MatrixField();
                angular.extend(fObj, f);
                return fObj;
            });
        } else {
            this.matrixFields = [];
        }
    }
}

export class MatrixDefinition implements IMatrixDefinition {
    key: number;
    matrixDefinitionColumns: MatrixDefinitionColumn[];

    // Extensions
    title: string;

    public setTypes() {
        if (this.matrixDefinitionColumns) {
            this.matrixDefinitionColumns = this.matrixDefinitionColumns.map(x => {
                let obj = new MatrixDefinitionColumn();
                angular.extend(obj, x);
                obj.setTypes();
                return obj;
            });
        } else {
            this.matrixDefinitionColumns = [];
        }
    }
}

export class MatrixDefinitionColumn implements IMatrixDefinitionColumn {
    columnNumber: number;
    field: string;
    key: System.IGuid;
    matrixDataType: MatrixDataType;
    matrixLayoutColumn: IMatrixLayoutColumn;
    options: MatrixDefinitionColumnOptions;
    title: string;

    public setTypes() {
        let obj = new MatrixDefinitionColumnOptions();
        angular.extend(obj, this.options);
        this.options = obj;
    }

    public get hasOptions(): boolean {
        // Removed check if has changed... in case wanna set other standard options
        return !!this.options;
    }
}

export class MatrixDefinitionColumnOptions implements IMatrixDefinitionColumnOptions {
    alignLeft: boolean;
    alignRight: boolean;
    clearZero: boolean;
    changed: boolean;
    dateFormatOption: TermGroup_MatrixDateFormatOption;
    decimals: number;
    groupBy: boolean;
    groupOption: TermGroup_MatrixGroupAggOption;
    hidden: boolean;
    key: string;
    labelPostValue: string;
    minutesToDecimal: boolean;
    minutesToTimeSpan: boolean;
    formatTimeWithSeconds: boolean;
    formatTimeWithDays: boolean;
}

export class MatrixField implements IMatrixField {
    columnKey: System.IGuid;
    key: number;
    matrixDataType: MatrixDataType;
    matrixFieldOptions: MatrixFieldOption[];
    rowNumber: number;
    value: any;
}

export class MatrixFieldOption implements IMatrixFieldOption {
    matrixFieldSetting: MatrixFieldSetting;
    stringValue: string;
}

export class MatrixLayoutColumn implements IMatrixLayoutColumn {
    field: string;
    matrixDataType: MatrixDataType;
    options: MatrixDefinitionColumnOptions;
    sort: number;
    title: string;
    visible: boolean;

    public setTypes() {
        let obj = new MatrixDefinitionColumnOptions();
        angular.extend(obj, this.options);
        this.options = obj;
    }

    public get hasOptions(): boolean {
        return !!this.options && this.options.changed;
    }

    public get isHidden(): boolean {
        return !!this.options && this.options.hidden;
    }
}

export class Insight implements IInsight {
    defaultChartType: TermGroup_InsightChartTypes;
    insightId: number;
    name: string;
    possibleChartTypes: TermGroup_InsightChartTypes[];
    possibleColumns: MatrixLayoutColumn[];
    readOnly: boolean;

    // Extensions
    public get isCustom(): boolean {
        return this.insightId == TermGroup_FixedInsights.Custom;
    }

    public setTypes() {
        if (this.possibleColumns) {
            this.possibleColumns = this.possibleColumns.map(x => {
                let obj = new MatrixLayoutColumn();
                angular.extend(obj, x);
                return obj;
            });
        } else {
            this.possibleColumns = [];
        }
    }
}