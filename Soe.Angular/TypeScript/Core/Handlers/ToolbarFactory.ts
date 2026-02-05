import { Toolbar, IToolbar } from "./Toolbar";
import { ToolBarUtility } from "../../Util/ToolBarUtility";
import { IGridHandler } from "./GridHandler";
import { IGridHandlerAg } from "./GridHandlerAg";

export interface IToolbarFactory {
    createEmpty(): IToolbar;
    createDefaultGridToolbar(grid: IGridHandler | IGridHandlerAg, onReload: () => void, showSave?: boolean, onSave?: () => void, disableSave?: () => boolean): IToolbar;
    createDefaultEditToolbar(showCopy: boolean, onCopy?: () => void, disableCopy?: () => boolean): IToolbar;
}

export class ToolbarFactory implements IToolbarFactory {
    constructor() {
    }

    createEmpty(): IToolbar {
        return new Toolbar();
    }

    createDefaultGridToolbar(grid: IGridHandler | IGridHandlerAg, onReload: () => void, showSave?: boolean, onSave?: () => void, disableSave?: () => boolean): IToolbar {
        var toolbar = new Toolbar();

        var group = ToolBarUtility.createGroup();
        group.buttons.push(ToolBarUtility.createClearFiltersButton(() => { grid.clearFilters(); }));
        if (onReload) {
            group.buttons.push(ToolBarUtility.createReloadDataButton(onReload));
        }
        toolbar.addButtonGroup(group);

        if (showSave) {
            toolbar.addButtonGroup(ToolBarUtility.createGroup(ToolBarUtility.createSaveButton(onSave, disableSave)));
        }

        return toolbar;
    }

    createDefaultEditToolbar(showCopy: boolean, onCopy: () => void, disableCopy: () => boolean = undefined): IToolbar {
        var toolbar = new Toolbar();

        if (showCopy === true) {
            toolbar.addButtonGroup(ToolBarUtility.createGroup(ToolBarUtility.createCopyButton(onCopy, disableCopy)));
        }

        return toolbar;
        //CopyFunction was not working correctly
        //return Toolbar.createDefault(showCopy, onCopy, disableCopy);
    }
}