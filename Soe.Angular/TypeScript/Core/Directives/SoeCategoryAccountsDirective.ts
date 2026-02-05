import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";
import { IMessagingService } from "../Services/MessagingService";
import { ICoreService } from "../Services/CoreService";
import { SoeCategoryType } from "../../Util/CommonEnumerations";

export class SoeCategoryAccountsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var elem = DirectiveHelper.createTemplateElement(
                    '<uib-accordion>' +
                    '<soe-accordion label-key="common.categories" is-open="true">' +
                    '<div data-ng-repeat="categoryAccount in directiveCtrl.categoryAccounts">' +
                    '<div class="col-sm-11">' +
                    '<div class="input-group" data-ng-class="{\'margin-small-top\': !$first}">' +
                    '<soe-select model="categoryAccount.categoryId" items="directiveCtrl.categories" options="item.id as item.name for item in items"></soe-select>' +
                    '<span class="input-group-btn">' +
                    '<button type="button" class="btn btn-sm btn-default iconDelete fal fa-times" data-l10n-bind data-l10n-bind-title="' + '' + 'core.delete' + '' + '" data-ng-click="$event.stopPropagation();directiveCtrl.removeItem(categoryAccount)"></button>' +
                    '</span>' +
                    '</div>' +
                    '</div>' +
                    '<div class="col-sm-1" style="padding-left: 0px;">' +
                    '<button data-ng-if="$first" type="button" class="btn btn-sm btn-default fal fa-plus" style="height: 30px; width: 30px;" data-ng-click="$event.stopPropagation();directiveCtrl.addItem(null)"></button>' +
                    '</div>' +
                    '</div>' +
                    '</soe-accordion>' +
                    '<uib-accordion>',
                    attrs);

                DirectiveHelper.applyAttributes([elem], attrs, "directiveCtrl");

                return elem.outerHTML;
            },
            scope: {
                form: '=',
                params: '=',
                categoryAccounts: '=',
                onChange: '&'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            controller: CategoryAccountsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class CategoryAccountsController {
    private categoryAccounts: [{}];
    private onChange: Function;
    private categories: any;
    private params: any;
    private accountId = 0;

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService) {
    }

    public $onInit() {
        this.accountId = this.params["accountId"];
        this.loadCategories();
        this.loadCategoryAccounts();
    }

    private loadCategories() {
        this.coreService.getCategoriesDict(SoeCategoryType.Employee, true).then((x) => {
            this.categories = x;
        });
    }

    private loadCategoryAccounts() {
        this.coreService.getCategoryAccountsByAccount(this.accountId, false).then((x) => {
            this.categoryAccounts = x;
            if (this.categoryAccounts && this.categoryAccounts.length === <number>0)
                this.addItem(null);
        });
    }

    private removeItem(item) {
        if (this.categoryAccounts) {
            if (item)
                this.categoryAccounts.splice(this.categoryAccounts.indexOf(item), 1);
            else
                this.categoryAccounts.splice(-1, 1);

            if (this.categoryAccounts.length === <number>0)
                this.addItem(null);
        }

        if (this.onChange)
            this.onChange();
    }

    private addItem(item) {
        if (this.categoryAccounts) {
            if (item)
                this.categoryAccounts.push(item);
            else
                this.categoryAccounts.push({ "categoryId": 0 });
        }

        if (this.onChange)
            this.onChange();
    }
}