import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeNavigationMenuDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<div class="btn-group ">' +
                    '<button type="button" class="btn btn-default fal fa-chevron-double-left" data-ng-disabled="directiveCtrl.isDoubleLeftButtonDisabled" data-ng-click="directiveCtrl.doubleLeftButtonClick()"</button>' +
                    '<button type="button" class="btn btn-default fal fa-chevron-left" data-ng-disabled="directiveCtrl.isLeftButtonDisabled" data-ng-click="directiveCtrl.leftButtonClick()" </button>' +
                    '<button type="button" class="btn btn-default fal fa-chevron-right" data-ng-disabled="directiveCtrl.isRightButtonDisabled" data-ng-click="directiveCtrl.rightButtonClick()" </button>' +
                    '<button type="button" class="btn btn-default fal fa-chevron-double-right" data-ng-disabled="directiveCtrl.isDoubleRightButtonDisabled" data-ng-click="directiveCtrl.doubleRightButtonClick()" </button>' +
                    '</div>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                DirectiveHelper.applyAttributes([elem], attrs, "directiveCtrl");

                return elem.outerHTML;
            },
            scope: {
                form: '=',
                ids: '=',
                currentId: '=?',
                navigateTo: '&',
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            controller: NavigationMenuController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class NavigationMenuController {

    ids: number[];
    _currentId: number;
    navigateTo: (id: any) => void;
    isDoubleLeftButtonDisabled: boolean = false;
    isLeftButtonDisabled: boolean = false;
    isDoubleRightButtonDisabled: boolean = false;
    isRightButtonDisabled: boolean = false;

    get currentId() {
        return this._currentId;
    }
    set currentId(id: number) {
        this._currentId = id;
        if (this.ids && this._currentId)
            this.validateButtons(_.indexOf(this.ids, this._currentId));
    }

    //@ngInject
    constructor() {

    }

    private doubleLeftButtonClick() {
        if (this.ids && this.ids.length > 0) {
            this.notifyParent(0);
        }
    }

    private leftButtonClick() {
        if (this.ids && this.ids.length > 0) {
            var index: number = _.indexOf(this.ids, this.currentId);
            if (index >= 0)
                this.notifyParent(index - 1);
        }
    }

    private rightButtonClick() {
        if (this.ids && this.ids.length > 0) {
            var index: number = _.indexOf(this.ids, this.currentId);
            if (index < (this.ids.length - 1))
                this.notifyParent(index + 1);
        }
    }

    private doubleRightButtonClick() {
        if (this.ids && this.ids.length > 0) {
            this.notifyParent(this.ids.length - 1);
        }
    }

    private validateButtons(index: number) {

        this.isDoubleLeftButtonDisabled = false;
        this.isLeftButtonDisabled = false;
        this.isDoubleRightButtonDisabled = false;
        this.isRightButtonDisabled = false;


        if (index === 0) {
            this.isDoubleLeftButtonDisabled = true;
            this.isLeftButtonDisabled = true;
        }

        if (index === (this.ids.length - 1)) {
            this.isDoubleRightButtonDisabled = true;
            this.isRightButtonDisabled = true;
        }
    }

    private notifyParent(index: number) {

        this.currentId = this.ids[index];
        this.navigateTo({ id: this.currentId }); //notify parent
    }
}