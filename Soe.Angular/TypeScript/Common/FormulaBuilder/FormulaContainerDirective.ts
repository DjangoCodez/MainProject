import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { IWidget, FormulaWidget } from "../Models/FormulaBuilderDTOs";
import { Guid } from "../../Util/StringUtility";

export class FormulaContainerDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Common/FormulaBuilder/Views/formulaContainer.html"),
            scope: {
                isReadonly: '=',
                labelKey: '@',
                frmWidgets: '=',
                formulaValid: '=',
                formulaError: '=',
                showSecondaryContainer: '@',
                secondaryLabelKey: '@',
                secondaryFrmWidgets: '=',
                secondaryFormulaValid: '=',
                secondaryFormulaError: '=',
                onChange: '&',
                onValidate: '&'
            },
            restrict: 'E',
            replace: true,
            controller: FormulaContainerController,
            controllerAs: 'ctrl',
            bindToController: true,
            link: function (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes, ctrl: FormulaContainerController) {
                // Need to set unique guid on form, used in drag n drop, otherwise it will fail if multiple edit pages are open in same tab view
                let guid: Guid = Guid.newGuid();
                ctrl.guid = guid;
                $(element).closest("form").attr("guid", <string>guid);
            }
        };
    }
}

class FormulaContainerController {
    // Init parameters
    private isReadonly: boolean;
    private frmWidgets: IWidget[];
    private formulaValid: string;
    private formulaError: string;
    private showSecondaryContainer: boolean;
    private secondaryFrmWidgets: IWidget[];
    private secondaryFormulaValid: string;
    private secondaryFormulaError: string;
    private onChange: Function;
    private onValidate: Function;

    // Drag n drop
    private _guid: Guid;
    public set guid(guid: Guid) {
        this._guid = guid;
        this.enableDragDrop();
        this.enableSecondaryDragDrop();
    }
    public get guid(): Guid {
        return this._guid;
    }

    private sortableOptions: any;
    private secondarySortableOptions: any;

    //@ngInject
    constructor(private $timeout: ng.ITimeoutService, private $scope: ng.IScope) {
        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.isReadonly, (newval, oldval) => {
            this.enableDragDrop();
            this.enableSecondaryDragDrop();
        }, true);
        this.$scope.$watch(() => this.frmWidgets, (newval, oldval) => {
            this.enableDragDrop();
        }, true);
        this.$scope.$watch(() => this.secondaryFrmWidgets, (newval, oldval) => {
            this.enableSecondaryDragDrop();
        }, true);
    }

    // DRAG N DROP

    private getFormElement() {
        let form = $("form[guid='" + this.guid + "']");
        return form.length > 0 ? $(form[0]) : null;
    }

    private enableDragDrop() {
        if (this.isReadonly)
            return;

        let form = this.getFormElement();
        if (!form)
            return;

        if (!this.sortableOptions) {
            this.sortableOptions = {
                containment: form.find(".main-widget-list")[0],
                handle: '.panel-heading',
                'ui-floating': true,
                stop: this.onMove.bind(this)
            };
        }
        form.find(".sortable-main").sortable(this.sortableOptions);
    }

    private enableSecondaryDragDrop() {
        if (this.isReadonly)
            return;

        let form = this.getFormElement();
        if (!form)
            return;

        if (!this.secondarySortableOptions) {
            this.secondarySortableOptions = {
                containment: form.find(".secondary-widget-list")[0],
                handle: '.panel-heading',
                'ui-floating': true,
                stop: this.onMoveSecondary.bind(this)
            };
        }
        form.find(".sortable-secondary").sortable(this.secondarySortableOptions);
    }

    private onMove(e, ui) {
        this.resortWidgets(0);
        this.validateRule(0);
        this.onChange();
    }

    private onMoveSecondary(e, ui) {
        this.resortWidgets(1);
        this.validateRule(1);
        this.onChange();
    }

    private resortWidgets(containerIndex: number) {
        let form = this.getFormElement();
        if (!form)
            return;

        var listElements = form.find(containerIndex === 0 ? ".main-widget-list" : ".secondary-widget-list").children();
        var sort: number = 0;

        _.forEach(listElements, element => {
            let widgetElem = $(element).find(".widget-container");
            let widget = this.getWidgetFromElement(widgetElem, containerIndex);
            if (widget)
                widget.sort = ++sort;
        });
    }

    // EVENTS

    private removeFormulaWidget(widget, containerIndex: number) {
        if (containerIndex === 0)
            _.pull(this.frmWidgets, widget);
        else
            _.pull(this.secondaryFrmWidgets, widget);

        this.validateRule(containerIndex);
        this.onChange();
    }

    // VALIDATION

    private validateRule(containerIndex: number) {
        this.$timeout(() => {
            if (this.onValidate)
                this.onValidate({ widgets: containerIndex === 0 ? this.frmWidgets : this.secondaryFrmWidgets, containerIndex: containerIndex });
        });
    }

    // HELP-METHODS

    private getWidgetFromElement(widgetElem, containerIndex: number): IWidget {
        if (widgetElem) {
            let internalId: number = parseInt(widgetElem.attr("id"), 10);
            if (internalId)
                return _.find(containerIndex === 0 ? this.frmWidgets : this.secondaryFrmWidgets, w => w.internalId == internalId);
        }

        return null;
    }
}