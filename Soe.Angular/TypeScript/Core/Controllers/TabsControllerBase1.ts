import { ITranslationService } from "../Services/TranslationService";
import { IUrlHelperService } from "../Services/UrlHelperService";
import { IMessagingService } from "../Services/MessagingService";
import { INotificationService } from "../Services/NotificationService";
import { Guid } from "../../Util/StringUtility";
import { HtmlUtility } from "../../Util/HtmlUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../Util/Enumerations";
import { Constants } from "../../Util/Constants";

export class ActionTab {
    constructor(public iconToolTip: string, public disable: boolean, private $window: ng.IWindowService, private blurOnSelect: boolean = true) {
    }
    public selected() {
        if (this.blurOnSelect)
            HtmlUtility.blurActiveElement(this.$window);
    }
}

export class Tab1 {
    public isDirty: boolean;
    public index: number;

    constructor(private $scope: ng.IScope, public label: string, public identifier: any, public controller: any, public templateUrl: string, public parameters: any, public canClose: boolean, public iconToolTip: string, public disable: boolean, private notificationService: INotificationService) {
        this.parameters = this.parameters || {};
        this.parameters.guid = Guid.newGuid();
    }

    public getGuid() {
        return this.parameters.guid;
    }

    public selected() {
        if (this.$scope)
            this.$scope.$broadcast('onTabActivated', this.getGuid());
    }

    private doubleClickCount: number = 0;
    public doubleClick() {
        this.doubleClickCount++;
        if (this.doubleClickCount >= 2) {
            this.notificationService.showDialog("Debug", "Parameters\n" + angular.toJson(this.parameters), SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
            this.doubleClickCount = 0;
        }
    }
}

export class TabsControllerBase1 {
    protected tabs: Tab1[] = [];

    protected entityNameSingle: string;     // The translated name of one entity
    protected entityNameMultiple: string;   // The translated name of multiple entities
    protected entityNameNew: string;        // The translated name of new entity
    private activeTabIndex: number;

    private closeToolTipText: string;
    private closeAllText: string;

    private addEnabled: boolean;
    private removeAllEnabled: boolean;

    public addTab: ActionTab;
    public removeAllTab: ActionTab;

    //@ngInject
    constructor(private $state: angular.ui.IStateService,
        private $stateParams: angular.ui.IStateParamsService,
        protected $window: ng.IWindowService,
        protected $timeout: ng.ITimeoutService,
        protected translationService: ITranslationService,
        protected urlHelperService: IUrlHelperService,
        protected messagingService: IMessagingService,
        protected notificationService: INotificationService,
        protected $scope: ng.IScope) {
    }

    protected enableAddTab() {
        this.addTab = new ActionTab(this.entityNameNew, true, this.$window);
    }

    protected enableRemoveAll() {
        this.removeAllTab = new ActionTab(this.closeAllText, true, this.$window, false);
    }

    protected addHomeTab(controller: any, parameters: any = { isHomeTab: true }, templateUrl: string = this.urlHelperService.getCoreViewUrl("grid1.html"), activeTab: boolean = false) {
        this.addNewTab(this.entityNameMultiple, null, controller, templateUrl, parameters, false, activeTab);
    }

    protected addHomeTabEx(controller: any, parameters: any = { isHomeTab: true }, activeTab: boolean = false) {
        this.addNewTab(this.entityNameMultiple, null, controller, this.urlHelperService.getCoreViewUrl("grid1.html"), parameters, false, activeTab);
    }

    protected addCreateNewTab(controller: any, templateUrl: string = this.urlHelperService.getViewUrl("edit.html"), parameters: any = null) {
        this.addNewTab(this.entityNameNew, null, controller, templateUrl, parameters, true, true);
    }

    protected addEditTab(label: string, tabIdentifier: any, controller: any, parameters: any, templateUrl: string = this.urlHelperService.getViewUrl("edit.html"), excludeEntityName: boolean = false) {
        this.addNewTab(excludeEntityName ? `${label || ""}` : `${this.entityNameSingle} ${label || ""}`, tabIdentifier, controller, templateUrl, parameters, true, true);
    }

    protected addNewTab(label: string, tabIdentifier: any, controller: any, templateUrl: string, parameters: any = null, canClose: boolean = true, activeTab: boolean = false, disable: boolean = false) {
        if (typeof (controller) !== "function")
            throw new Error("Controller must be a function/class");

        var tab = new Tab1(this.$scope, label, tabIdentifier, controller, templateUrl, parameters, canClose, this.closeToolTipText, disable, this.notificationService);
        var newIndex = this.tabs.length > 0 ? _.max(this.tabs.map(t => t.index)) + 1 : 0; // Needs a new unique index
        tab.index = newIndex;

        this.tabs.push(tab);
        if (activeTab) {
            this.$timeout(() => this.setActiveTabIndex(newIndex));
            this.$timeout(() => tab.selected());
        }
    }

    protected getEditIdentifier(row: any): any {
        throw new Error("Must implement protected getEditIdentifier(row: any): any");
    }
    protected getEditName(data: any): string {
        throw new Error("Must implement protected getEditName(data: any): string");
    }

    protected initialize(entityNameSingleKey: string, entityNameMultipleKey: string, entityNameNewKey: string) {

        // Translate
        var keys: string[] = [
            entityNameSingleKey,
            entityNameMultipleKey,
            entityNameNewKey,
            "core.close",
            "core.close_all_tabs"
        ];

        this.translationService.translateMany(keys.filter(k => !!k)).then((terms) => {
            this.closeToolTipText = terms["core.close"];
            this.closeAllText = terms["core.close_all_tabs"];

            this.entityNameSingle = entityNameSingleKey ? terms[entityNameSingleKey] : '';
            this.entityNameMultiple = entityNameMultipleKey ? terms[entityNameMultipleKey] : '';
            this.entityNameNew = entityNameNewKey ? terms[entityNameNewKey] : '';

            this.setupTabs();

            for (var key in this.$stateParams) {
                if (this.$stateParams.hasOwnProperty(key) && this.$stateParams[key]) {
                    this.edit(this.$stateParams);
                    break;
                }
            }
        });

        // Subscribe to add tab events
        this.messagingService.subscribe(Constants.EVENT_ACTIVATE_ADD_TAB, () => {
            if (this.addTab)
                this.addTab.disable = false;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_DISABLE_ADD_TAB, () => {
            if (this.addTab)
                this.addTab.disable = true;
        }, this.$scope);

        // Subscribe to close tab event
        this.messagingService.subscribe(Constants.EVENT_CLOSE_TAB, (guid) => {
            var tab = _.find(this.tabs, t => t.getGuid() === guid);
            this.removeTab(tab);
        }, this.$scope);

        // Subscribe to edit event
        this.messagingService.subscribe(Constants.EVENT_EDIT, (obj) => {
            var row = obj.row;
            var data = obj.data;
            var identifier = this.getEditIdentifier(row);
            var activeTab = _.find(this.tabs, tab => tab.identifier === identifier);
            if (activeTab) {
                this.activeTabIndex = activeTab.index;
            } else {
                this.edit(row, data);
            }
        }, this.$scope);

        // Subscribe to modified (dirty) event
        this.messagingService.subscribe(Constants.EVENT_SET_TAB_MODIFIED, (params) => {
            this.setDirtyFlag(params.guid, params.dirty);
        }, this.$scope);

        // Subscribe to new event
        this.messagingService.subscribe(Constants.EVENT_EDIT_NEW, (params) => {
            this.setTabLabel(params.guid, params.label ? params.label : this.entityNameNew);
            this.setTabIdentifier(params.guid, undefined);
        }, this.$scope);

        // Subscribe to set tab label event
        this.messagingService.subscribe(Constants.EVENT_SET_TAB_LABEL, (params) => {
            this.setTabLabel(params.guid, params.label);
            if (params.id)
                this.setTabIdentifier(params.guid, params.id);
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_DISABLE_TAB, (params) => {
            this.setTabDisable(params.identifier, params.disable);
        }, this.$scope);

        // Subscribe to added event
        this.messagingService.subscribe(Constants.EVENT_EDIT_ADDED, (params) => {
            this.setTabLabel(params.guid, `${this.entityNameSingle} ${this.getEditName(params.data)}`);
            this.setTabIdentifier(params.guid, this.getEditIdentifier(params.data));
        }, this.$scope);

        // Subscribe to set tab label event
        this.messagingService.subscribe(Constants.EVENT_OPEN_TAB, (tabMessage: TabMessage) => {
            var forceNewTab = (tabMessage.parameters && tabMessage.parameters.forceNewTab);
            var activeTab = _.find(this.tabs, tab => tab.identifier === tabMessage.tabIdentifier);
            if (activeTab && !forceNewTab) {
                this.activeTabIndex = activeTab.index;
            } else {
                this.addNewTab(tabMessage.label, tabMessage.tabIdentifier, tabMessage.controller, tabMessage.templateUrl, tabMessage.parameters, true, true);
            }
        }, this.$scope);

        // Subscribe to saved event
        this.messagingService.subscribe(Constants.EVENT_EDIT_SAVED, (params) => {
            this.setDirtyFlag(params.guid, false);
        }, this.$scope);
    }

    private setDirtyFlag(guid: Guid, dirty: boolean) {
        var tab = this.getTabByGuid(guid);
        if (tab)
            tab.isDirty = dirty;
    }

    protected setTabLabel(guid: Guid, label: string) {
        var tab = this.getTabByGuid(guid);
        if (tab)
            tab.label = label;
    }

    protected setTabDisable(identifier: any, disable: boolean) {
        var tab = this.getTabByIdentifier(identifier);
        if (tab)
            tab.disable = disable;
    }

    protected setTabIdentifier(guid: Guid, identifier: any) {
        var tab = this.getTabByGuid(guid);
        if (tab) {
            tab.identifier = identifier;
        }
    }

    private getTabByGuid(guid: Guid): Tab1 {
        return _.find(this.tabs, t => t.parameters.guid === guid);
    }

    private getTabByIdentifier(identifier: any): Tab1 {
        return _.find(this.tabs, t => t.identifier === identifier);
    }

    // SETUP

    protected setupTabs() { }

    // ACTIONS

    protected add() { }
    protected edit(row: any, data: any = null) { }

    private openEditIfIdExists() {
        this.edit(this.$stateParams);
    }

    public canRemoveAllTabs(): boolean {
        return !!_.find(this.tabs, x => x.canClose);
    }

    public removeAllTabs() {
        var dirtyTab = _.find(this.tabs, x => x.isDirty && x.canClose);
        if (dirtyTab) {
            this.notificationService.showConfirmOnClose(true).then(close => {
                if (close) {
                    this.closeAllTabs();
                } else {
                    this.setActiveTabIndex(this.tabs.indexOf(dirtyTab));
                }
            });
        } else {
            this.closeAllTabs();
        }
    }

    private closeTab(tab: Tab1) {
        this.activeTabIndex = 0;
        _.remove(this.tabs, tab);
    }

    private closeAllTabs() {
        _.remove(this.tabs, tab => tab.canClose);
        this.activeTabIndex = 0;
    }

    protected removeTab(tab: Tab1) {
        if (!tab)
            return;

        if (tab.isDirty) {
            this.notificationService.showConfirmOnClose(false).then(close => {
                if (close)
                    this.closeTab(tab);
            });
        } else {
            this.closeTab(tab);
        }
    }

    protected setActiveTabIndex(index: number) {
        this.$timeout(() => this.activeTabIndex = index);
    }
}

export class TabMessage {
    constructor(public label: string, public tabIdentifier: any, public controller: any, public parameters: any, public templateUrl: string) { }
}