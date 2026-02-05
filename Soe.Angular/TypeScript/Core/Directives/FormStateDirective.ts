export class FormStateDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            scope: {
                formState: '=',
            },
            link: function ($scope, elem, attrs, form: ng.IFormController) {
                var w = $scope.$watch(x => form.$pristine, (newValue, oldValue, scope) => {
                    if (newValue === oldValue)
                        return;

                    $scope['formState'] = !newValue;
                });

                var w2 = $scope.$watch("formState", (newValue, oldValue, scope) => {
                    if (newValue === oldValue)
                        return;

                    if (newValue)
                        form.$setDirty();
                    else
                        form.$setPristine();
                });

                var o = $scope.$on("$destroy", () => {
                    w();
                    w2();
                    o();
                });
            },
            restrict: 'A',
            require: 'form'
        };
    }
}