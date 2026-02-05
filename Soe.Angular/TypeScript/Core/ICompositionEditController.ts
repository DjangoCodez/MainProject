import { IToolbar } from './Handlers/Toolbar';
import { IProgressHandler } from './Handlers/ProgressHandler';
import { IDirtyHandler } from './Handlers/DirtyHandler';
import { Guid } from '../Util/StringUtility';

export interface ICompositionEditController {
    // Called from TabControllerDirective
    onInit(parameters: any);

    guid: Guid;

    toolbar: IToolbar;
    progress: IProgressHandler;
    dirtyHandler: IDirtyHandler;

    isNew: boolean;
    modifyPermission: boolean;

    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
}