export class GridKeypressDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            replace: true,
            priority: -200,
            require: '^uiGrid',
            scope: false,
            controller: GridKeypressContoller,
            compile: function () {
                return {
                    pre: function ($scope: any, $elm, $attrs, uiGridCtrl) {
                        $scope.uiGridCtrl = uiGridCtrl;
                    }
                }
            }
        };
    }
}

class GridKeypressContoller {

    //@ngInject
    constructor(private $scope, private $attrs, uiGridEditConstants) {
        var unregWatch = $scope.$watch('uiGridCtrl.cellNav', () => {
            var orig = $scope.uiGridCtrl.cellNav.handleKeyDown;
            $scope.uiGridCtrl.cellNav.handleKeyDown = (evt) => {
                var toRun = $attrs['gridKeypress'];//like directiveCtrl.onGridKeyPress. NO () in the end!!

                var controller = this.getControllerByStringPath($scope, toRun);//get the controller obj
                var func = this.getFunctionByStringPath(toRun);//get the function name
                var res = controller[func](evt);//call it in this way to maintain "this" in the called function.

                if (res === undefined)
                    return orig(evt);

                if (res === 'stopEdit') {//'stopEdit' means we want to handle it, but we need to end the cell edit before we navigate or IE will be sad, so we trigger editend here before we do the actual navigation. or well, really it is after, but it will be before when all timeouts are considered.
                    $scope.$broadcast(uiGridEditConstants.events.END_CELL_EDIT);//we broadcast insted of emit since we are on the grid scope and not on the cell-scope. Need to travel down, not up.
                }

                return null;
            }

            unregWatch();
        });
    }

    private getControllerByStringPath(o, s) {
        s = s.replace(/\[(\w+)\]/g, '.$1'); // convert indexes to properties
        s = s.replace(/^\./, '');           // strip a leading dot
        var a = s.split('.');
        for (var i = 0, n = a.length - 1; i < n; ++i) {
            var k = a[i];
            if (k in o) {
                o = o[k];
            } else {
                return null;
            }
        }
        return o;
    }

    private getFunctionByStringPath(s) {
        var a = s.split('.');
        return a[a.length - 1];
    }
}