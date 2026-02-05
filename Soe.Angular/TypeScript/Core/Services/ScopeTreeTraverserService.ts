export type VisitorFunction = ((scope: ng.IScope) => void) | ((scope: ng.IScope) => boolean);

export interface IScopeTreeTraverserService {
    depthFirst(scope: ng.IScope, visitor: VisitorFunction): void;
}

export class ScopeTreeTraverserService implements IScopeTreeTraverserService {
    public depthFirst(scope: ng.IScope, visitor: VisitorFunction): void {
        if (visitor(scope) === true) return;
        this.traverseSiblings(scope, visitor);
        this.traverseChildren(scope, visitor);
    }

    private traverseSiblings(scope: ng.IScope, visitor: VisitorFunction) {
        while (!!(scope = scope['$$nextSibling'])) {
            if (visitor(scope) === true) return;
            this.traverseChildren(scope, visitor);
        }
    }

    private traverseChildren(scope: ng.IScope, visitor: VisitorFunction) {
        while (!!(scope = scope['$$childHead'])) {
            if (visitor(scope) === true) return;
            this.traverseSiblings(scope, visitor);
        }
    }
}