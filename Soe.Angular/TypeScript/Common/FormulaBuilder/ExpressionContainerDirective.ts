import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { IWidget, ExpressionWidget, OperatorWidget, FormulaWidget } from "../Models/FormulaBuilderDTOs";
import { PriceRuleItemType } from "../../Util/CommonEnumerations";
import { PriceRuleDTO } from "../Models/PriceRuleDTO";

export class ExpressionContainerDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Common/FormulaBuilder/Views/expressionContainer.html"),
            scope: {
                isReadonly: '=',
                expWidgets: '=',
                opWidgets: '=',
                onDrop: '&'
            },
            restrict: 'E',
            replace: true,
            controller: ExpressionContainerController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class ExpressionContainerController {
    // Init parameters
    private isReadonly: boolean;
    private expWidgets: IWidget[];
    private opWidgets: IWidget[];
    private onDrop: Function;

    // Drag n drop
    private draggableOptions: any;

    //@ngInject
    constructor(private $timeout: ng.ITimeoutService, private $scope: ng.IScope) {
        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.isReadonly, (newval, oldval) => {
            this.enableDragDrop();
        }, true);
        this.$scope.$watch(() => this.expWidgets, (newval, oldval) => {
            this.enableDragDrop();
        }, true);
        this.$scope.$watch(() => this.opWidgets, (newval, oldval) => {
            this.enableDragDrop();
        }, true);
    }

    // DRAG N DROP

    private enableDragDrop() {
        if (this.isReadonly)
            return;

        if (!this.draggableOptions) {
            this.draggableOptions = {
                revert: "invalid",
                revertDuration: 0,
                helper: 'clone',
                opacity: 0.75,
                zIndex: 10000,
                stop: this.onExpressionDrop.bind(this),
            };
        }
        $(".expression-container > .widget-list > li > .widget-container").not('.ui-draggable').draggable(this.draggableOptions);
    }

    private onExpressionDrop(e, ui) {
        let containerIndex: number = this.isDroppedInFormulaContainer(ui);
        if (containerIndex < 0)
            return;

        let widget: IWidget = angular.element($(ui.helper).find('.widget')).scope()['widget'];
        if (widget) {
            this.$timeout(() => {
                let frmWidget = new FormulaWidget(this.isReadonly, widget.widgetName, widget.widgetWidth, widget.widgetWidthInFormula, widget.data ? widget.data.isStandby : false);
                frmWidget.priceRuleType = widget.priceRuleType;
                frmWidget.timeRuleType = widget.timeRuleType;
                frmWidget.isExpression = !!widget.isExpression;
                frmWidget.isOperator = !!widget.isOperator;
                frmWidget.isFormula = true;
                this.onDrop({ widget: frmWidget, containerIndex: containerIndex });
            });
        }
    }

    private isDroppedInFormulaContainer(ui): number {
        let inside: boolean = false;
        let containerIndex: number = -1;

        // Find formula container (look in current form/tab)
        var form = $(ui.helper).closest('form');
        if (form) {
            var containers = form.find('.formula-container');
            _.forEach(containers, container => {
                containerIndex++;
                // Get container boundaries
                var position = $(container).offset();
                if (position) {
                    var containerMinY = position.top;
                    var containerMaxY = position.top + $(container).height();
                    var containerMinX = position.left;
                    var containerMaxX = position.left + $(container).width();

                    // Get expression dropped position
                    var expPosition = ui.offset;
                    if (expPosition) {
                        var expTop = expPosition.top;
                        var expLeft = expPosition.left;

                        if (expTop >= containerMinY && expTop <= containerMaxY && expLeft >= containerMinX && expLeft <= containerMaxX) {
                            inside = true;
                            return false;
                        }
                    }
                }
            });
        }

        return inside ? containerIndex : -1;
    }
}