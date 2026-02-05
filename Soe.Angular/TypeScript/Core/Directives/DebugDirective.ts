export class DebugDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            template:
                '<i class="debug-icon fal fa-debug" data-ng-dblclick="directiveCtrl.doubleClick()"></i>',
            scope: {
                onDebug: '&',
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
            },
            restrict: 'E',
            replace: true,
            controller: DebugController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class DebugController {
    onDebug: () => any;

    private doubleClickCount: number = 0;
    public doubleClick() {
        this.doubleClickCount++;
        if (this.doubleClickCount >= 2) {
            if (this.onDebug)
                this.onDebug();
            this.doubleClickCount = 0;
        }
    }
}