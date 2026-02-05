import { IGridHandler } from "./Handlers/gridhandler";
import { IProgressHandler } from "./Handlers/progresshandler";
import { IToolbar } from "./Handlers/Toolbar";

export interface ICompositionGridController {
    // Called from TabControllerDirective
    onInit(parameters: any);
    edit(row: any);
    //save();

    grid: IGridHandler;
    progress: IProgressHandler;
    toolbar: IToolbar;
    modifyPermission?: boolean;
    readOnlyPermission?: boolean;
}
