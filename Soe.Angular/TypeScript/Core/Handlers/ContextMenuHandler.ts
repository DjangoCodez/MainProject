
// https://github.com/Templarian/ui.bootstrap.contextMenu

export interface IContextMenuHandler {
    clearContextMenuItems();
    addContextMenuItem(label: string, icon: string, click: ($itemScope, $event, modelValue) => void, enabled: () => void);
    addContextMenuSeparator();
    getContextMenuOptions(): any[];
    hasContextMenuOptions(): boolean;
    lastItemIsSeparator(): boolean;
}

export class ContextMenuHandler implements IContextMenuHandler {

    public contextMenuOptions: any[];

    constructor() {

    }

    private initContextMenu() {
        if (!this.contextMenuOptions)
            this.contextMenuOptions = [];
    }

    public clearContextMenuItems() {
        this.contextMenuOptions = [];
    }

    public addContextMenuItem(label: string, icon: string, click: ($itemScope, $event, modelValue) => void, enabled: () => void) {
        this.initContextMenu();

        var item = ['<span><i class="fal fa-fw ' + icon + '"></i>' + label + '</span>', click, enabled];
        this.contextMenuOptions.push(item);
    }

    public addContextMenuSeparator() {
        this.initContextMenu();

        // Do not add a separator as first item, and not two in a row
        if (this.hasContextMenuOptions() && !this.lastItemIsSeparator())
            this.contextMenuOptions.push(null);
    }

    public getContextMenuOptions(): any[] {
        if (this.lastItemIsSeparator())
            this.contextMenuOptions = _.dropRight(this.contextMenuOptions);

        return this.contextMenuOptions;
    }

    public hasContextMenuOptions(): boolean {
        return this.contextMenuOptions && this.contextMenuOptions.length > 0;
    }

    public lastItemIsSeparator(): boolean {
        return _.last(this.contextMenuOptions) === null;
    }
}
