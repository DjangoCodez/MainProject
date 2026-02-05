import { MatrixDefinitionColumnOptions } from "../../../../../Common/Models/MatrixResultDTOs";
import { MatrixColumnSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { AnalysisMode, MatrixDataType, TermGroup_MatrixDateFormatOption, TermGroup_MatrixGroupAggOption } from "../../../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../../../Util/CoreUtility";

export class MatrixColumnOptionsController {

    // Data
    private column: MatrixColumnSelectionDTO

    // Properties
    private get isAnalysis(): boolean {
        return this.mode === AnalysisMode.Analysis;
    }
    private get isInsights(): boolean {
        return this.mode === AnalysisMode.Insights;
    }

    private _align: number;
    private set align(value: number) {
        this._align = value;
        if (value === MatrixColumnOptionsAlignment.Left)
            this.setAlignLeft();
        else
            this.setAlignRight();
    }
    private get align(): number {
        return this._align;
    }

    private _showTimeAs: number;
    private set showTimeAs(value: number) {
        this._showTimeAs = value;
        if (value == MatrixColumnOptionsShowTimeAs.Minutes)
            this.setShowTimeAsMinutes();
        else if (value == MatrixColumnOptionsShowTimeAs.HHMM)
            this.setShowTimeAsHHMM();
        else
            this.setShowTimeAsHHDD();
    }
    private get showTimeAs(): number {
        return this._showTimeAs;
    }

    private get isDecimal(): boolean {
        return this.column.matrixDataType === MatrixDataType.Decimal;
    }

    private get isDate(): boolean {
        return this.column.matrixDataType === MatrixDataType.Date;
    }

    private get isTime(): boolean {
        return this.column.matrixDataType === MatrixDataType.Time;
    }

    private get isNumeric(): boolean {
        return this.column.matrixDataType === MatrixDataType.Integer || this.isDecimal;
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private mode: AnalysisMode,
        private dateFormats: SmallGenericType[],
        private groupAggOptions: SmallGenericType[],
        column: MatrixColumnSelectionDTO) {

        this.column = CoreUtility.cloneDTO(column);
    }

    public $onInit() {
        if (!this.column.options)
            this.setDefaultProperties();
        else
            this.initProperties();
    }

    private setDefaultProperties() {
        this.column.options = new MatrixDefinitionColumnOptions();
        this.setDefaultAlignmentBasedOnDataType();
        if (!this.column.options.dateFormatOption)
            this.column.options.dateFormatOption = TermGroup_MatrixDateFormatOption.DateLong;
        this.showTimeAs = MatrixColumnOptionsShowTimeAs.Minutes;
        if (this.isDecimal)
            this.column.options.decimals = 2;
        if (!this.column.options.groupOption) this.column.options.groupOption = TermGroup_MatrixGroupAggOption.Sum;
    }

    private initProperties() {
        if (!this.column.options.alignRight && !this.column.options.alignLeft)
            this.setDefaultAlignmentBasedOnDataType();
        else
            this.align = this.column.options.alignRight ? MatrixColumnOptionsAlignment.Right : MatrixColumnOptionsAlignment.Left;

        if (!this.column.options.dateFormatOption)
            this.column.options.dateFormatOption = TermGroup_MatrixDateFormatOption.DateLong;

        if (this.isDecimal && !this.column.options.decimals)
            this.column.options.decimals = 2;

        if (this.column.options.minutesToTimeSpan)
            this._showTimeAs = 2;
        else if (this.column.options.minutesToDecimal)
            this._showTimeAs = 3;
        else
            this._showTimeAs = 1;

        if (!this.column.options.groupOption)  this.column.options.groupOption = TermGroup_MatrixGroupAggOption.Sum;
    }

    private setDefaultAlignmentBasedOnDataType() {
        this.align = this.isNumeric || this.isTime ? MatrixColumnOptionsAlignment.Right : MatrixColumnOptionsAlignment.Left;
    }

    // EVENTS

    private ok() {
        this.column.options.changed = true;
        this.$uibModalInstance.close({ column: this.column });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    // HELP-METHODS

    private setAlignLeft() {
        this.column.options.alignLeft = true;
        this.column.options.alignRight = false;
    }

    private setAlignRight() {
        this.column.options.alignLeft = false;
        this.column.options.alignRight = true;
    }

    private setShowTimeAsMinutes() {
        this.column.options.minutesToTimeSpan = false;
        this.column.options.minutesToDecimal = false;
    }

    private setShowTimeAsHHMM() {
        this.column.options.minutesToTimeSpan = true;
        this.column.options.minutesToDecimal = false;
    }

    private setShowTimeAsHHDD() {
        this.column.options.minutesToTimeSpan = false;
        this.column.options.minutesToDecimal = true;
    }
}

export enum MatrixColumnOptionsAlignment {
    Left = 1,
    Right = 2
}

export enum MatrixColumnOptionsShowTimeAs {
    Minutes = 1,
    HHMM = 2,
    HHDD = 3
}