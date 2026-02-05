import { IGridControllerBase } from "./GridControllerBase";
import { IEditControllerBase } from "./EditControllerBase";
import { ITranslationService } from "../Services/TranslationService";
import { IUrlHelperService } from "../Services/UrlHelperService";
import { IMessagingService } from "../Services/MessagingService";
import { INotificationService } from "../Services/NotificationService";
import { HtmlUtility } from "../../Util/HtmlUtility";
import { Guid } from "../../Util/StringUtility";
import { Constants } from "../../Util/Constants";

export interface ITabsControllerBase {
    setupTabs(): void;
    edit(id: number, label: string): void;
    createGridController(): IGridControllerBase;
    createEditController(id: number): IEditControllerBase;
}

export class TabsControllerBase {
    protected tabs: Tab[] = [];
    protected idField: string;              // The field name of the entitys primary key
    protected labelField: string;           // The field name of the text to be displayed in the tabs label
    protected entityNameSingle: string;     // The translated name of one entity
    protected entityNameMultiple: string;   // The translated name of multiple entities
    protected entityNameNew: string;        // The translated name of new entity
    private activeTabIndex: number;

    //@ngInject
    constructor(private $state: angular.ui.IStateService,
        private $stateParams: angular.ui.IStateParamsService,
        protected $window: ng.IWindowService,
        protected $timeout: ng.ITimeoutService,
        protected translationService: ITranslationService,
        protected urlHelperService: IUrlHelperService,
        protected messagingService: IMessagingService,
        protected notificationService: INotificationService) {
    }

    protected initialize(idField: string, labelField: string, entityNameSingleKey: string, entityNameMultipleKey: string, entityNameNewKey: string) {
        this.idField = idField;
        this.labelField = labelField;

        // Translate
        var keys: string[] = [entityNameSingleKey, entityNameMultipleKey, entityNameNewKey];
        this.translationService.translateMany(keys.filter(k => !!k)).then((terms) => {
            this.entityNameSingle = entityNameSingleKey ? terms[entityNameSingleKey] : '';
            this.entityNameMultiple = entityNameMultipleKey ? terms[entityNameMultipleKey] : '';
            this.entityNameNew = entityNameNewKey ? terms[entityNameNewKey] : '';

            this.setupTabs();
            this.checkStateParams();
        });

        // Subscribe to add tab events
        this.messagingService.subscribe(Constants.EVENT_ACTIVATE_ADD_TAB, (guid) => {
            this.activateAddTab();
        });
        this.messagingService.subscribe(Constants.EVENT_DISABLE_ADD_TAB, (guid) => {
            this.disableAddTab();
        });

        // Subscribe to close tab event
        this.messagingService.subscribe(Constants.EVENT_CLOSE_TAB, (guid) => {
            var tab = this.findRemovableTabByGuid(guid);
            if (tab) {
                this.remove(tab);
            }
        });

        // Subscribe to edit event
        this.messagingService.subscribe(Constants.EVENT_EDIT, (obj) => {
            var row = obj.row;
            // If tab is already opened, activate it, otherwise open record in new tab
            var tab = this.findRemovableTab(row['' + idField + '']);
            if (tab) {
                this.activeTabIndex = tab.index;
            } else {
                this.edit(row['' + this.idField + ''], this.entityNameSingle + " " + row['' + this.labelField + '']);
            }
        });

        // Subscribe to modified (dirty) event
        this.messagingService.subscribe(Constants.EVENT_SET_TAB_MODIFIED, (params) => {
            this.setTabDirty(params);
        });

        // Subscribe to new event
        this.messagingService.subscribe(Constants.EVENT_EDIT_NEW, (params) => {
            this.setTabLabelOnNew(params);
        });

        // Subscribe to set tab label event
        this.messagingService.subscribe(Constants.EVENT_SET_TAB_LABEL, (params) => {
            this.setTabLabel(params);
        });

        // Subscribe to added event
        this.messagingService.subscribe(Constants.EVENT_EDIT_ADDED, (params) => {
            this.setTabLabelOnSave(params);
        });

        // Subscribe to saved event
        this.messagingService.subscribe(Constants.EVENT_EDIT_SAVED, (params) => {
            this.setTabLabelOnSave(params);
        });
    }

    // SETUP

    protected setupTabs() {
    }

    // ACTIONS

    protected edit(id: number, label: string) {
    }

    protected checkStateParams() {
        // Open edit page if record id is specified on query string
        var id = this.$stateParams['' + this.idField + ''];
        if (id && parseInt(id, 10)) {
            this.edit(parseInt(id, 10), this.entityNameSingle);
        }
    }

    protected tabDblClicked(tab) {
        // Double click twice on tab label to display record id
        if (tab.isRemovableTab) {
            if (!tab.dblClicked) {
                tab.dblClicked = true;
                return;
            }
            tab.dblClicked = false;

            // Toggle between ID and label
            if (tab.label !== tab.id) {
                tab.tmpLabel = tab.label;
                tab.setLabel(tab.id);
            } else if (tab.tmpLabel) {
                tab.setLabel(tab.tmpLabel);
            }
        }
    }

    // TABS


    protected createHomeTab(gridController: IGridControllerBase, viewName?: string, isHomeTab?: boolean, index?: number, label?: string): Tab {
        gridController.isHomeTab = isHomeTab != null ? isHomeTab : true;
        var tab: Tab = viewName ?
            new CustomGridHomeTab(this.urlHelperService, gridController, viewName) :
            new HomeTab(this.urlHelperService, gridController);

        tab.setLabel(label != null ? label : this.entityNameMultiple);
        (<any>tab).index = index != null ? index : 0;
        return tab;
    }

    protected createHomeEditTab(editController: IEditControllerBase, viewName?: string, index?: number): HomeEditTab {
        if (!viewName)
            viewName = "edit.html";

        const tab: HomeEditTab = new HomeEditTab(this.urlHelperService, editController, viewName);
        tab.setLabel(this.entityNameMultiple);
        tab.index = index != null ? index : 0;
        return tab;
    }

    protected createAddTab(index?: number): AddTab {
        const tab = new AddTab(this.$window, this.entityNameNew, () => { this.edit(0, this.entityNameNew); });
        tab.index = index != null ? index : 1;
        return tab;
    }

    protected createRemoveAllTab(index?: number): RemoveAllTab {
        var tab = new RemoveAllTab(this.$window, this.translationService, () => { this.removeAll(); });
        tab.index = index != null ? index : 2;
        return tab;
    }

    protected addNewTab(id: number, label: string, editController: IEditControllerBase, url: string = "") {
        // Insert new tab last (before add and remove all tabs)
        var nrOfOptionalTabs = this.getNrOfOptionalTabs();
        var tab = new RemovableTab(this.translationService, this.urlHelperService, editController, id, label, (t) => { this.remove(t) }, url);
        var index = this.tabs.length - nrOfOptionalTabs;
        this.tabs.splice(index, 0, tab);

        var newIndex = _.max(this.tabs.map(t => t.index)) + 1; // Needs a new unique index
        tab.index = newIndex;

        this.$timeout(() => { this.activeTabIndex = newIndex; }, 100);

        this.enableRemoveAllTab();
        HtmlUtility.blurActiveElement(this.$window);
    }

    protected getNrOfOptionalTabs(): number {
        var nrOfOptionalTabs: number = 0;
        if (this.hasAddTab())
            nrOfOptionalTabs++;
        if (this.hasRemoveAllTab())
            nrOfOptionalTabs++;
        return nrOfOptionalTabs;
    }

    protected hasAddTab(): boolean {
        var count: number = 0;
        _.forEach(this.tabs, (tab: Tab) => {
            if (tab instanceof AddTab) {
                count++;
            }
        });
        return count > 0;
    }

    protected hasRemoveAllTab() {
        var count: number = 0;
        _.forEach(this.tabs, (tab: Tab) => {
            if (tab instanceof RemoveAllTab) {
                count++;
            }
        });
        return count > 0;
    }

    protected activateAddTab() {
        _.forEach(this.tabs, (tab: Tab) => {
            if (tab instanceof AddTab) {
                tab.disable = false;
            }
        });
    }

    protected disableAddTab() {
        _.forEach(this.tabs, (tab: Tab) => {
            if (tab instanceof AddTab) {
                tab.disable = true;
            }
        });
    }

    protected enableRemoveAllTab() {
        // Enable remove all tab if at least one removable tab exists
        var count: number = 0;

        _.forEach(this.tabs, (tab: Tab) => {
            if (tab instanceof RemovableTab) {
                count++;
            }
        });

        _.forEach(this.tabs, (tab: Tab) => {
            if (tab instanceof RemoveAllTab) {
                tab.disable = count === 0;
            }
        });
    }

    protected remove(tab) {
        if (tab.isDirty) {
            // Show warning dialog
            this.notificationService.showConfirmOnClose(false).then(close => {
                if (close)
                    this.removeTabs([tab]);
            });
        } else {
            this.removeTabs([tab]);
        }
    }

    protected removeAll() {
        var tabsToRemove = new Array<RemovableTab>();
        var hasDirty: boolean = false;

        _.forEach(this.tabs, (tab: Tab) => {
            if (tab instanceof RemovableTab) {
                tabsToRemove.push(tab);
                if (tab.isDirty)
                    hasDirty = true;
            }
        });

        if (hasDirty) {
            // Show warning dialog
            this.notificationService.showConfirmOnClose(true).then(close => {
                if (close) {
                    this.removeTabs(tabsToRemove);
                } else {
                    // Activate home tab
                    this.tabs[0].active = true;
                }
            });
        } else {
            this.removeTabs(tabsToRemove);
        }
    }

    private removeTabs(tabs: RemovableTab[]) {
        // Activate home tab
        this.$timeout(() => { this.activeTabIndex = 0; }, 100);

        // Force the splice to next loop after the home tab is selected.
        // Otherwise the next tab in tabset will automatically be active,
        // and if that is the addTab a new tab will be created
        this.$timeout(() => {
            _.forEach(tabs, (tab: RemovableTab) => {
                var tabIndex = this.tabs.indexOf(tab);
                if (this.tabs[tabIndex] instanceof RemovableTab) {
                    this.tabs.splice(tabIndex, 1);
                    this.enableRemoveAllTab();
                }
            });
        });
    }

    protected findRemovableTab(id: number): Tab {
        const tab = _.find(this.tabs, (y) => {
            if (y instanceof RemovableTab) {
                return (<RemovableTab>y).id === id;
            }
            return false;
        });
        return tab;
    }

    protected findTabByGuid(guid: Guid): Tab {
        const tab = _.find(this.tabs, (y) => {
            return (<Tab>y).guid === guid;
        });
        return tab;
    }

    protected findRemovableTabByGuid(guid: Guid): Tab {
        const tab = _.find(this.tabs, (y) => {
            if (y instanceof RemovableTab) {
                return (<RemovableTab>y).guid === guid;
            }
            return false;
        });
        return tab;
    }

    private setTabDirty(params) {
        const tab = this.findTabByGuid(params.guid);
        if (tab) {
            tab.setDirty(params.dirty);
        }
    }

    private setTabLabel(params) {
        const tab = this.findTabByGuid(params.guid);
        if (tab) {
            tab.setLabel(params.label);
        }
    }

    private setTabLabelOnNew(params) {
        var tab = this.findRemovableTabByGuid(params.guid);
        if (tab) {
            tab.setLabel(params.label ? params.label : this.entityNameNew);
        }
    }

    private setTabLabelOnSave(params) {
        var tab = this.findRemovableTabByGuid(params.guid);
        if (tab) {
            tab.setLabel(this.entityNameSingle + " " + params.data['' + this.labelField + '']);
        }
    }
}

export class Tab {
    public guid: string;
    public label: string = "";
    public iconToolTip: string = "";
    public isDirty: boolean = false;
    public index: number;

    constructor(public active: boolean = false, public disable: boolean = false) {
        this.guid = Guid.newGuid();
    }

    public setLabel(label: string) {
        this.label = label;
    }

    public setIconToolTip(toolTip: string) {
        this.iconToolTip = toolTip;
    }

    public setDirty(dirty: boolean) {
        this.isDirty = dirty;
    }

    public blurActiveTab($window) {
        // Unselecting tab after click
        HtmlUtility.blurActiveElement($window);
    }
}

export class CustomGridHomeTab extends Tab {
    constructor(private urlHelperService: IUrlHelperService, private gridController: IGridControllerBase, private viewName: string) {
        super(true);
    }
    isHomeTab: boolean = this.gridController.isHomeTab;
    templateUrl = this.urlHelperService.getViewUrl(this.viewName);
    controller = this.gridController;
}

export class HomeTab extends Tab {
    constructor(private urlHelperService: IUrlHelperService, private gridController: IGridControllerBase) {
        super(true);
    }

    isHomeTab: boolean = this.gridController.isHomeTab;
    templateUrl = this.urlHelperService.getCoreViewUrl("grid.html");
    controller = this.gridController;
}

export class HomeEditTab extends Tab {
    constructor(private urlHelperService: IUrlHelperService, private editController: IEditControllerBase, private viewName: string) {
        super(true);

        this.editController.guid = this.guid;
    }

    isHomeTab: boolean = true;
    templateUrl = this.urlHelperService.getViewUrl(this.viewName);
    controller = this.editController;
}

export class AddTab extends Tab {
    constructor(private $window, toolTip: string, private callback: () => void) {
        // Disabled as default
        // Will be enabled when permissions are loaded
        super(false, true);
        super.setIconToolTip(toolTip);
    }

    isAddTab: boolean = true;

    public selected() {
        this.callback();
        this.blurActiveTab(this.$window);
    }
}

export class RemovableTab extends Tab {
    constructor(private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private editController: IEditControllerBase,
        public id,
        label: string,
        private removeTabCallback: (tab: Tab) => void,
        private url: string = "") {
        super(true);

        super.setLabel(label);
        translationService.translate("core.close").then((term) => {
            super.setIconToolTip(term);
        });

        this.editController.guid = this.guid;
    }

    isRemovableTab: boolean = true;
    templateUrl = this.url === "" ? this.urlHelperService.getViewUrl("edit.html") : this.url;
    controller = this.editController;

    public remove() {
        this.removeTabCallback(this);
    }
}

export class RemoveAllTab extends Tab {
    constructor(private $window, private translationService: ITranslationService,
        private callback: () => void) {
        super(false, true);

        translationService.translate("core.close_all_tabs").then((term) => {
            super.setIconToolTip(term);
        });
    }

    isRemoveAllTab: boolean = true;

    public selected() {
        this.callback();
        this.blurActiveTab(this.$window);
    }
}