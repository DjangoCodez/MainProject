import { ITranslationService } from "../Services/TranslationService";

export class DateFilterFactory {
    //@ngInject
    public static create(translationService: ITranslationService, uibDatepickerConfig: angular.ui.bootstrap.IDatepickerConfig, uibDatepickerPopupConfig: angular.ui.bootstrap.IDatepickerPopupConfig): ng.IDirective {
        return {
            template: '<div class="input-group input-group-sm" style="z-index: 1">' +
                '<input title="{{ctrl.colFilter.term|date:\'shortDate\'}}" placeholder="{{ctrl.placeholder}}" clear-text="{{ctrl.clearText}}" close-text={{ctrl.closeText}} current-text={{ctrl.currentText}} datepicker-append-to-body="true" type="text" class="form-control dateFilter" uib-datepicker-popup="{{datepickerConfig.format}}" data-ng-model="ctrl.colFilter.term" data-ng-model-options="{allowInvalid: allowInvalid}" data-ng-disabled="disabled" is-open="isOpen" />' +
                '<span class="input-group-btn ui-grid-datepicker-button">' +
                '<button type="button" class="btn btn-default dateFilterButton" data-ng-hide="disabled" data-ng-click="isOpen=true"><i class="fal fa-calendar-alt"></i></button>' +
                '</span>' +
                '</div>',
            scope: {
                colFilter: "=",
                isFrom: "="
            },
            restrict: 'E',
            replace: true,
            controller: DateFilterController,
            controllerAs: 'ctrl',
            bindToController: true
        }
    }
}

class DateFilterController {
    scope: ng.IScope;
    placeholder: string;
    colFilter: any;
    isFrom: any;
    clearText: string;
    closeText: string;
    currentText: string;

    //@ngInject
    constructor(private translationService: ITranslationService) {
    }

    public $onInit() {
        var keys: string[] = [
            "common.from",
            "common.to",
            "core.datepicker.clear",
            "core.datepicker.close",
            "core.datepicker.current"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            this.placeholder = this.isFrom ? terms["common.from"] : terms["common.to"];
            this.clearText = terms["core.datepicker.clear"];
            this.closeText = terms["core.datepicker.close"];
            this.currentText = terms["core.datepicker.current"];
        });
    }
}