import { FieldOrPredicate } from "../../SoeGridOptionsAg";
import { ICellEditorParams } from "./ICellEditorComp";
import { InputCellEditor } from "./InputCellEditor";
import { ObjectFieldHelper } from "../../ObjectFieldHelper";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export interface IDateCellEditorParams {
    isDisabled: FieldOrPredicate;
    dateFormat: string;
    minDate: Date;
    maxDate: Date;
}

declare type Params = ICellEditorParams & IDateCellEditorParams;

export class DateCellEditor extends InputCellEditor {
    private $scope: ng.IScope;
    private $isOpenWatch: () => void;
    private fixedDateFormat: string;
    private minDate: Date;
    private maxDate: Date;

    public init?(params: ICellEditorParams): void {
        super.init(params);

        this.fixedDateFormat = this.fixDateFormat((<Params>this.params).dateFormat);
        this.minDate = (<Params>this.params).minDate;
        this.maxDate = (<Params>this.params).maxDate;

        let { isDisabled, node } = <Params>this.params;

        if (ObjectFieldHelper.IsEvaluatedTrue(node.data, isDisabled)) {
            this.cellElement.setAttribute("disabled", "disabled");
        }
    }

    // Gets called once after GUI is attached to DOM.
    // Useful if you want to focus or highlight a component
    // (this is not possible when the element is not attached)
    public afterGuiAttached?(): void {
        super.afterGuiAttached();

        // Want to use the DateTimePicker in angular-bootstrap (uib-datepicker-popup directive).
        // Invoke and apply Angular for this element only. 
        const grid = angular.element(this.cellElement).closest("[ag-grid]");
        grid.injector().invoke(/*@ngInject*/($compile: ng.ICompileService, $locale: any, translationService: ITranslationService) => {
            this.$scope = grid.scope().$new(true);
            this.$scope['selectedDate'] = this.params.value;
            this.$scope['isOpen'] = true;
            this.$scope['options'] = {
                minDate: this.minDate,
                maxDate: this.maxDate,
            }

            this.$isOpenWatch = this.$scope.$watch("isOpen", (newValue: boolean) => {
                if (!newValue) {
                    const { api } = this.params;
                    api.stopEditing();
                }
            });

            const format = this.fixedDateFormat || $locale.DATETIME_FORMATS.shortDate;

            this.cellElement.setAttribute("parse-date", '');
            this.cellElement.setAttribute("uib-datepicker-popup", format);
            this.cellElement.setAttribute("datepicker-options", "options");
            this.cellElement.setAttribute("is-open", "isOpen");
            this.cellElement.setAttribute("ng-model", "selectedDate");
            this.cellElement.setAttribute("datepicker-append-to-body", "true");
            this.cellElement.setAttribute("close-on-date-selection", "true");
            this.cellElement.setAttribute("ng-change", "changedDate()");
            this.cellElement.setAttribute("on-open-focus", "false");

            var keys: string[] = [
                "core.datepicker.current",
                "core.datepicker.clear",
                "core.datepicker.close"
            ];

            translationService.translateMany(keys).then((terms) => {
                this.cellElement.setAttribute("current-text", terms["core.datepicker.current"]);
                this.cellElement.setAttribute("clear-text", terms["core.datepicker.clear"]);
                this.cellElement.setAttribute("close-text", terms["core.datepicker.close"]);
            });

            $compile(this.cellElement)(this.$scope);
        });
    }

    public getValue(): any {
        return this.$scope['selectedDate'];
    }

    // Gets called once by grid after editing is finished
    // if your editor needs to do any cleanup, do it here
    public destroy?(): void {
        super.destroy();

        //Need to clean angular stuff in an apply, otherwise it's trying to clean up the watcher in a destroyed scope next digest cycle .
        this.$scope.$applyAsync((s: ng.IScope) => {
            this.$isOpenWatch();
            this.$scope.$destroy();
            delete this.$scope;
            delete this.$isOpenWatch;
        });
    }

    // Gets called once after initialised.
    // If you return true, the editor will appear in a popup
    isPopup?(): boolean {
        return false;
    }

    private fixDateFormat(dateFormat: string) {
        return !!dateFormat
            ? dateFormat.replace("mm", "MM")
            : null;
    }
}