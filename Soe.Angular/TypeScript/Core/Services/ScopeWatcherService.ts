import { IScopeTreeTraverserService } from "./ScopeTreeTraverserService";

export interface IScopeWatcherService {
    suspendWatchers(scope: ng.IScope): void;
    resumeWatchers(scope: ng.IScope): void;
}
//based on https://www.monterail.com/blog/2015/story-of-angular-watchers-toggler-directive
export class ScopeWatcherService implements IScopeWatcherService {
    //@ngInject
    constructor(private scopeTreeTraverserService: IScopeTreeTraverserService, private $parse: ng.IParseService) {
    }

    public suspendWatchers(scope: ng.IScope): void {

        this.scopeTreeTraverserService.depthFirst(scope, (currScope) => {
            const currWatchers: Array<any> = currScope['$$watchers'];
            if (!!currWatchers && currWatchers.length > 0) {
                currScope['bk_watchers'] = currWatchers;
                currScope['$$watchers'] = [];
                currScope['$watch'] = <any>this.mockScopeWatch(currScope);
            }
        });
    }

    public resumeWatchers(scope: ng.IScope): void {
        this.scopeTreeTraverserService.depthFirst(scope, (currScope) => {
            const prevWatchers: Array<any> = currScope['bk_watchers'];
            if (!!prevWatchers && prevWatchers.length > 0) {
                currScope['$$watchers'] = prevWatchers;
                if (currScope.hasOwnProperty('$watch')) {
                    delete currScope.$watch;
                }
                delete currScope['bk_watchers'];
            }
        });
    }

    private mockScopeWatch(scope: ng.IScope): (watchExpression, listener, objectEquality, prettyPrintExpression) => () => void {
        return (watchExpression, listener, objectEquality, prettyPrintExpression) => {
            const watchers: Array<any> = scope['bk_watchers'];
            const length = watchers.unshift({
                fn: angular.isFunction(listener) ? listener : angular.noop,
                last: void 0,
                get: this.$parse(watchExpression),
                exp: prettyPrintExpression || watchExpression,
                eq: !!objectEquality
            });

            return () => {
                watchers.splice(length - 1, 1);
            }
        };
    }
}