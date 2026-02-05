import { IMessagingHandler } from "../Handlers/messaginghandler";
import { IGridControllerFlowHandler } from "../Handlers/controllerflowhandler";
import { IProgressHandler } from "../Handlers/progresshandler";
import { IToolbar } from "../Handlers/Toolbar";
import { IGridHandlerAg } from "../Handlers/gridhandlerag";
import { IGridHandler } from "../Handlers/gridhandler";
import { IGridHandlerFactory } from "../Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../Handlers/progresshandlerfactory";
import { GridEvent } from "../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../Util/Enumerations";
import { Guid } from "../../Util/StringUtility";
import { IContextMenuHandler } from "../Handlers/ContextMenuHandler";
import { ToolBarButtonGroup } from "../../Util/ToolBarUtility";

export class GridControllerBase2Ag {
    private onTabActivetedAndModifiedCallback: (() => void);
    private idColumnNameOnEdit: string;
    private nameColumnNameOnEdit: string;
    protected messagingHandler: IMessagingHandler;
    protected parameters: any;
    protected isHomeTab: boolean;
    protected doubleClickToEdit = true;
    protected flowHandler: IGridControllerFlowHandler;

    // Context menu
    protected contextMenuHandler: IContextMenuHandler;

    public progress: IProgressHandler;
    public toolbar: IToolbar;
    public gridAg: IGridHandlerAg;
    public grid: IGridHandler;
    public modifyPermission: boolean;
    public readPermission: boolean;
    public isDirty = false;
    public guid: Guid;
    public idProperty: string;
    public modifiedData: number[] = [];
    public sortMenuButtons = new Array<ToolBarButtonGroup>();

    constructor(gridHandlerFactory: IGridHandlerFactory, gridName: string, progressHandlerFactory?: IProgressHandlerFactory,
        messagingHandlerFactory?: IMessagingHandlerFactory) {
        this.gridAg = gridHandlerFactory.create(gridName, "agGrid") as IGridHandlerAg;

        // Grid events
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.rowDoubleClicked(row); }));
        this.gridAg.options.subscribe(events);

        if (progressHandlerFactory) {
            this.progress = progressHandlerFactory.create();
        }

        if (messagingHandlerFactory) {
            this.messagingHandler = messagingHandlerFactory.create();
        }
    }

    private setDirty(x: any = null) {
        if (x && x.data && x.data[this.idProperty] && !_.includes(this.modifiedData, x.data[this.idProperty]))
            this.modifiedData.push(x.data[this.idProperty]);
        this.isDirty = true;
    }

    public onTabActivated(callback: () => void) {
        if (callback) {
            this.messagingHandler.onTabActivated((tabGuid) => {
                if (tabGuid == this.guid)
                    callback();
            });
        }
    }

    public onTabActivetedAndModified(callback: () => void) {
        this.onTabActivetedAndModifiedCallback = callback;
        if (this.onTabActivetedAndModifiedCallback && this.messagingHandler) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.setDirty(x); });
            this.messagingHandler.onTabActivated((tabGuid) => this.tabActived(tabGuid));
        }
    }

    private tabActived(tabGuid: Guid) {
        if (tabGuid == this.guid) {
            if (this.isDirty && this.onTabActivetedAndModifiedCallback) {
                this.onTabActivetedAndModifiedCallback();
                this.isDirty = false;
            }
        }
    }

    // GRID EVENTS

    public rowDoubleClicked(row) {
        if (this.doubleClickToEdit)
            this.edit(row);
    }

    public edit(row) {
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission)) {
            if (this.idColumnNameOnEdit && this.nameColumnNameOnEdit)
                this.messagingHandler.publishEditRow(row, this.gridAg.options.getFilteredRows().map(r => ({ id: r[this.idColumnNameOnEdit], name: r[this.nameColumnNameOnEdit] })));
            else if (this.idColumnNameOnEdit)
                this.messagingHandler.publishEditRow({ row: row, ids: _.map(this.gridAg.options.getFilteredRows(), this.idColumnNameOnEdit) });
            else
                this.messagingHandler.publishEditRow(row);
        }
    }

    protected setData(data: any) {
        this.isDirty = false;
        this.gridAg.setData(data);
        this.messagingHandler.publishResizeWindow();
    }

    protected setIdColumnNameOnEdit(idColumnName: string) {
        this.idColumnNameOnEdit = idColumnName;
    }

    protected useRecordNavigatorInEdit(idColumnName: string, nameColumnName: string) {
        this.idColumnNameOnEdit = idColumnName;
        this.nameColumnNameOnEdit = nameColumnName;
    }

    private getContextMenuOptions() {
        return this.contextMenuHandler ? this.contextMenuHandler.getContextMenuOptions() : undefined;
    }

    protected findNextRow(row): { rowIndex: number, rowNode: any } {
        const result = this.gridAg.options.getNextRow(row);

        return !!result.rowNode ? result : null;
    }

    protected findPreviousRow(row): { rowIndex: number, rowNode: any } {
        const result = this.gridAg.options.getPreviousRow(row);

        return !!result.rowNode ? result : null;
    }
}
