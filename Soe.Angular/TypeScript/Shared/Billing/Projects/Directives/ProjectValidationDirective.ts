import { ProjectDTO } from "../../../../Common/Models/ProjectDTO";

export class ProjectValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                allprojects: "=",
                newPricelist: "=",
                pricelistName: "=",
                existingPricelists: "=",
                useExisting: "=",
                pricelistTypeId: "=",
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        // Customer
                        var project: ProjectDTO = ngModelController.$modelValue;

                        var allProjects: any[] = scope["allprojects"];
                        if (project.projectId && project.projectId > 0)
                            allProjects = _.filter(allProjects, p => p.projectId !== project.projectId);
                        if (project.number) {
                            var number: string = project.number.toString();
                        }
                            var numberInUse: boolean = project.number ? _.filter(allProjects, p => p.number === number).length > 0 : false;
                            ngModelController.$setValidity("numberInUse", !numberInUse);

                            var invalidDates: boolean = project.startDate && project.stopDate ? project.startDate > project.stopDate : false;
                            ngModelController.$setValidity("invalidDates", !invalidDates);
                    }
                }, true);
                scope.$watchGroup(['allProjects', 'newPricelist', 'pricelistName', 'existingPricelists', 'useExisting', 'pricelistTypeId'], (newValues, oldValues, scope) => {

                    //Price lists
                    var newPricelist: boolean = scope["newPricelist"];
                    var useExisting: boolean = scope["useExisting"];

                    var pricelistName: string = scope["pricelistName"];
                    var nameIsEmpty: boolean = newPricelist ? !pricelistName || pricelistName.trim() === '' : false;
                    ngModelController.$setValidity("nameIsEmpty", !nameIsEmpty);

                    var allProjects: any[] = scope["existingPricelists"];
                    var nameInUse: boolean = nameIsEmpty === false ? _.filter(allProjects, p => p.name === pricelistName).length > 0 : false;
                    ngModelController.$setValidity("nameInUse", !nameInUse);

                    var pricelistTypeId: number = scope["pricelistTypeId"];
                    var invalidPricelist: boolean = useExisting ? !pricelistTypeId || pricelistTypeId < 1 : false;
                    ngModelController.$setValidity("invalidPricelist", !invalidPricelist);
                });
            }
        }
    }
}


