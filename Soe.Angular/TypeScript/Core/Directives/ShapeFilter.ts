import { ITranslationService } from "../Services/TranslationService";

export class ShapeFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: '<select ng-model="colFilter.term" style="width: 100%" ng-style="{background: colFilter.term}">' +
                '<option value="" style="background: white;"></option>' +
                '<option ng-repeat="color in ctrl.colors" ng-style="{background: color}" value="{{color}}">&nbsp;</option>' +
                '</select>',
            restrict: 'E',
            replace: true,
            controller: ShapeFilterController,
            controllerAs: 'ctrl',
            bindToController: true
        }
    }
}

class ShapeFilterController {
    public colors: string[];

    //@ngInject
    constructor($scope: any, translationService: ITranslationService) {
        $scope.colFilter.condition = (term, value, row, column) => term === value;
        var field = $scope.col.field;
        this.colors = $scope.col.grid.rows.map((row) => row.entity[field]).filter((value, index, self) => value && self.indexOf(value) === index);
    }
}