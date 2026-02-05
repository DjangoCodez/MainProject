import { TabMessage } from "../Controllers/TabsControllerBase1";
import { ITranslationService } from "../Services/TranslationService";
import { IUrlHelperService } from "../Services/UrlHelperService";
import { INotificationService } from "../Services/NotificationService";
import { IMessagingService } from "../Services/MessagingService";
import { Guid } from "../../Util/StringUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../Util/Enumerations";
import { HtmlUtility } from "../../Util/HtmlUtility";
import { Constants } from "../../Util/Constants";

export interface ITabHandler {
    initialize(entityNameSingleKey: string, entityNameMultipleKey: string, entityNameNewKey: string): ITabHandler;

    onSetupTabs(callback: (tabHandler: ITabHandler) => void): ITabHandler;
    onEdit(callback: (row: any, data?: any) => void): ITabHandler;
    onGetRowIdentifier(callback: (row: any) => string): ITabHandler;
    onGetRowEditName(callback: (row: any) => string): ITabHandler;

    addHomeTab(controller: any, parameters?: any, templateUrl?: string, tabHeader?: string);
    addCreateNewTab(controller: any, templateUrl?: string, parameters?: any, title?: string, activateTab?: boolean);
    addEditTab(row: any, controller: any, parameters?: any, templateUrl?: string, title?: string, activateTab?: boolean);
    addNewTab(label: string, tabIdentifier: any, controller: any, templateUrl: string, parameters?: any, canClose?: boolean, activeTab?: boolean, disabled?: boolean);
    enableAddTab(callback: () => void);
    enableRemoveAll();

    getTabByIdentifier(id: any);
    getTabByParameters(id: any, type: any);
    getIndexOf(tab: any);
    setActiveTab(tab: any);
    setActiveTabIndex(index: number);
    setMaxNbrOfTabs(max: number);
}

export class TabHandler implements ITabHandler {
    private tabs: TabHandlerTab[] = [];
    private onSetupTabsHandlers: ((tabHandler: ITabHandler) => void)[] = [];
    private onEditHandlers: ((row: any, data?: any) => void)[] = [];
    private onGetRowIdentifierHandler: (row: any) => string;
    private onGetRowEditNameHandler: (row: any) => string;
    private closeToolTipText: string;
    private closeAllText: string;
    private entityNameSingle: string;     // The translated name of one entity
    private entityNameMultiple: string;   // The translated name of multiple entities
    private entityNameNew: string;        // The translated name of new entity
    private maxNbrOfTabs: number;

    private activeTabIndex: number = -1;
    public addTab: TabHandlerActionTab;
    public removeAllTab: TabHandlerActionTab;

    constructor(private translationService: ITranslationService,
        private $stateParams: angular.ui.IStateParamsService, private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService, private messagingService: IMessagingService,
        private $timeout: ng.ITimeoutService, private $window: ng.IWindowService) {
    }

    initialize(entityNameSingleKey: string, entityNameMultipleKey: string, entityNameNewKey: string) {
        if (!this.onGetRowIdentifierHandler) {
            throw "Have to call onGetRowIdentifier"
        }
        if (!this.onGetRowEditNameHandler) {
            throw "Have to call onGetRowEditName"
        }

        const keys: string[] = [
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
            this.entityNameNew = entityNameMultipleKey ? terms[entityNameNewKey] : '';

            _.each(this.onSetupTabsHandlers, x => x(this));

            for (var key in this.$stateParams) {
                if (this.$stateParams.hasOwnProperty(key) && this.$stateParams[key]) {
                    this.edit(this.$stateParams);
                    break;
                }
            }
        });

        this.setUpSubscriptions();

        return this;
    }
    onSetupTabs(callback: (tabHandler: ITabHandler) => void): ITabHandler {
        this.onSetupTabsHandlers.push(callback);
        return this;
    }
    onEdit(callback: (row: any) => void): ITabHandler {
        this.onEditHandlers.push(callback);
        return this;
    }
    onGetRowIdentifier(callback: (row: any) => string): ITabHandler {
        this.onGetRowIdentifierHandler = callback;
        return this;
    }
    onGetRowEditName(callback: (row: any) => string): ITabHandler {
        this.onGetRowEditNameHandler = callback;
        return this;
    }

    addHomeTab(controller: any, parameters: any = null, templateUrl: string = null, tabHeader: string = null) {
        if (!parameters)
            parameters = { isHomeTab: true };
        this.addNewTab((tabHeader ? tabHeader : this.entityNameMultiple), null, controller, templateUrl ? templateUrl : this.urlHelperService.getCoreViewUrl("gridComposition.html"), parameters, false, (parameters && parameters.activeTab != undefined) ? parameters.activeTab : true);
    }
    addCreateNewTab(controller: any, templateUrl: string = this.urlHelperService.getViewUrl("edit.html"), parameters: any = null, title: string = null, activateTab: boolean = true) {
        this.addNewTab(title ? title : this.entityNameNew, Guid.newGuid(), controller, templateUrl, parameters, true, activateTab);
    }
    addEditTab(row: any, controller: any, parameters?: any, templateUrl: string = this.urlHelperService.getViewUrl("edit.html"), title: string = null, activateTab: boolean = true) {
        if (!parameters) {
            parameters = {};
        }
        if (!parameters.id) {
            parameters.id = this.onGetRowIdentifierHandler(row);
        }
        this.addNewTab(`${title ? title : this.entityNameSingle} ${this.onGetRowEditNameHandler(row) || ""}`, this.onGetRowIdentifierHandler(row), controller, templateUrl, parameters, true, activateTab);
    }
    addNewTab(label: string, tabIdentifier: any, controller: any, templateUrl: string, parameters: any = null, canClose = true, activeTab = false, disabled = false) {
        if (typeof (controller) !== "function")
            throw new Error("Controller must be a function/class");

        const tab = new TabHandlerTab(label, tabIdentifier, controller, templateUrl, parameters, canClose, this.closeToolTipText, this.notificationService, this.messagingService);
        tab.disable = disabled;
        var newIndex = this.tabs.length > 0 ? _.max(this.tabs.map(t => t.index)) + 1 : 0; // Needs a new unique index
        tab.index = newIndex;
        this.tabs.push(tab);

        this.checkMaxNbrOfTabs();

        if (activeTab) {
            this.$timeout(() => this.setActiveTabIndex(newIndex));
        }
    }
    enableAddTab(callback: () => void) {
        this.addTab = new TabHandlerActionTab(this.entityNameNew, true, this.$window, true, callback);
    }
    enableRemoveAll() {
        this.removeAllTab = new TabHandlerActionTab(this.closeAllText, true, this.$window, false, () => { this.removeAllTabs(); });
    }
    removeTab(tab: TabHandlerTab) {
        if (!tab)
            return;

        if (tab.isDirty) {
            this.notificationService.showConfirmOnClose(false).then(close => {
                if (close) {
                    this.activeTabIndex = 0;
                    _.remove(this.tabs, tab);
                    this.checkMaxNbrOfTabs();
                }
            });
        } else {
            this.activeTabIndex = 0;
            _.remove(this.tabs, tab);
            this.checkMaxNbrOfTabs();
        }
    }
    canRemoveAllTabs(): boolean {
        return !!_.find(this.tabs, x => x.canClose);
    }
    removeAllTabs() {
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

    private edit(row: any, data: any = null) {
        _.each(this.onEditHandlers, x => x(row, data));
    }
    public getTabByIdentifier(id: any) {
        return _.find(this.tabs, { identifier: id });
    }
    public getTabByParameters(id: any, type: any) {
        let tab = null;
        _.forEach(this.tabs, (x) => {
            if (x.parameters.id == id && x.parameters.type == type) {
                tab = x;
            }
        });
        return tab;
    }
    public getIndexOf(tab: any) {
        return this.tabs.indexOf(tab);
    }
    public setActiveTabIndex(index: number) {
        this.$timeout(() => this.activeTabIndex = index);
    }
    public setActiveTab(tab: any) {
        var index = this.getIndexOf(tab);
        if (index >= 0) {
            this.setActiveTabIndex(index);
        }
    }
    public setMaxNbrOfTabs(max: number) {
        this.maxNbrOfTabs = max;
    }

    private setDirtyFlag(guid: Guid, dirty: boolean) {
        const tab = this.getTabByGuid(guid);
        if (tab)
            this.$timeout(() => tab.isDirty = dirty);
    }
    private getTabByGuid(guid: Guid): TabHandlerTab {
        return _.find(this.tabs, t => t.parameters.guid === guid);
    }
    private setTabGuid(guid: Guid, newGuid: Guid) {
        const tab = this.getTabByGuid(guid);
        if (tab)
            tab.setGuid(newGuid);
    }
    private setTabLabel(guid: Guid, label: string) {
        const tab = this.getTabByGuid(guid);
        if (tab)
            tab.label = label;
    }
    private setTabIdentifier(guid: Guid, identifier: any) {
        const tab = this.getTabByGuid(guid);
        if (tab) {
            tab.identifier = identifier;
        }
    }
    private setTabDisable(identifier: string, disable: boolean) {
        const tab = this.getTabByIdentifier(identifier);
        if (tab)
            tab.disable = disable;
    }
    private setUpSubscriptions() {
        // Subscribe to remove add tab event
        this.messagingService.subscribe(Constants.EVENT_ACTIVATE_ADD_TAB, () => {
            if (this.addTab) {
                if (!this.maxNbrOfTabs || this.tabs.length < this.maxNbrOfTabs)
                    this.addTab.disable = false;
            }
        });

        // Subscribe to close tab event
        this.messagingService.subscribe(Constants.EVENT_CLOSE_TAB, (guid) => {
            var t = _.find(this.tabs, tab => tab.getGuid() === guid);
            this.$timeout(() => this.removeTab(t));
        });

        // Subscribe to edit event
        this.messagingService.subscribe(Constants.EVENT_EDIT, (obj) => {
            const row = obj.row;
            const data = obj.data;
            const identifier = this.onGetRowIdentifierHandler(row);
            const activeTab = _.find(this.tabs, tab => tab.identifier === identifier);
            if (activeTab) {
                this.setActiveTabIndex(this.tabs.indexOf(activeTab));
            } else {
                this.edit(row, data);
            }
        });

        // Subscribe to modified (dirty) event
        this.messagingService.subscribe(Constants.EVENT_SET_TAB_MODIFIED, (params) => {
            this.setDirtyFlag(params.guid, params.dirty);
        });

        // Subscribe to new event
        this.messagingService.subscribe(Constants.EVENT_EDIT_NEW, (params) => {
            this.setTabLabel(params.guid, params.label ? params.label : this.entityNameNew);
            this.setTabIdentifier(params.guid, undefined);
        });

        // Subscribe to set tab guid event
        this.messagingService.subscribe(Constants.EVENT_SET_TAB_GUID, (params) => {
            this.setTabGuid(params.guid, params.newGuid);
        });

        // Subscribe to set tab label event
        this.messagingService.subscribe(Constants.EVENT_SET_TAB_LABEL, (params) => {
            this.setTabLabel(params.guid, params.label);
            if (params.id)
                this.setTabIdentifier(params.guid, params.id);
        });

        // Subscribe to added event
        this.messagingService.subscribe(Constants.EVENT_EDIT_ADDED, (params) => {
            this.setTabLabel(params.guid, `${this.entityNameSingle} ${this.onGetRowEditNameHandler(params.data)}`);
            this.setTabIdentifier(params.guid, this.onGetRowIdentifierHandler(params.data));
        });

        // Subscribe to set tab label event
        this.messagingService.subscribe(Constants.EVENT_OPEN_TAB, (tabMessage: TabMessage) => {
            var parameters = tabMessage.parameters;
            if (parameters && parameters.forceNewTab) {
                this.addNewTab(tabMessage.label, tabMessage.tabIdentifier, tabMessage.controller, tabMessage.templateUrl, tabMessage.parameters, true, true);
            } else {
                var activeTab = _.find(this.tabs, tab => tab.identifier === tabMessage.tabIdentifier);
                if (activeTab) {
                    this.setActiveTabIndex(this.tabs.indexOf(activeTab));
                } else {
                    this.addNewTab(tabMessage.label, tabMessage.tabIdentifier, tabMessage.controller, tabMessage.templateUrl, tabMessage.parameters, true, true);
                }
            }
        });

        // Subscribe to saved event
        this.messagingService.subscribe(Constants.EVENT_EDIT_SAVED, (params) => {
            this.setTabDisable(params.guid, false);
        });

        this.messagingService.subscribe(Constants.EVENT_DISABLE_TAB, (params) => {
            this.setTabDisable(params.identifier, params.disable);
        });
    }
    private closeAllTabs() {
        _.remove(this.tabs, tab => tab.canClose);
        this.checkMaxNbrOfTabs();
        this.setActiveTabIndex(0);
    }

    private checkMaxNbrOfTabs() {
        if (this.addTab) {
            this.addTab.hidden = (this.maxNbrOfTabs && this.tabs.length >= this.maxNbrOfTabs);
        }
    }
}

export class TabHandlerTab {
    public isDirty: boolean;
    public disable: boolean;
    public index: number;

    constructor(public label: string, public identifier: any, public controller: any, public templateUrl: string, public parameters: any, public canClose: boolean, public iconToolTip: string, private notificationService: INotificationService, private messagingService: IMessagingService) {
        this.parameters = this.parameters || {};
        this.parameters.guid = Guid.newGuid();
    }

    public getGuid() {
        return this.parameters.guid;
    }

    public setGuid(guid: Guid) {
        this.parameters.guid = guid;
    }

    public selected() {
        this.messagingService.publish(Constants.EVENT_TAB_ACTIVATED, this.getGuid());
    }

    private doubleClickCount = 0;
    public doubleClick() {
        this.doubleClickCount++;
        if (this.doubleClickCount >= 2) {
            this.notificationService.showDialog("Debug", "Parameters\n" + angular.toJson(this.parameters), SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
            this.doubleClickCount = 0;
        }
    }
}

export class TabHandlerActionTab {
    public hidden = false;

    constructor(public iconToolTip: string, public disable: boolean, private $window: ng.IWindowService, private blurOnSelect: boolean = true, private callback: () => void) {
    }
    public selected() {
        if (this.blurOnSelect)
            HtmlUtility.blurActiveElement(this.$window);
    }

    public onSelected() {
        this.callback();
    }
}
