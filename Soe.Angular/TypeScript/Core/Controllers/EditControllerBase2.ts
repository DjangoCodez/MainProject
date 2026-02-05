import { IProgressHandler } from "../Handlers/progresshandler";
import { IToolbar } from "../Handlers/Toolbar";
import { IDirtyHandler } from "../Handlers/DirtyHandler";
import { IValidationSummaryHandler } from "../Handlers/validationsummaryhandler";
import { IMessagingHandler } from "../Handlers/messaginghandler";
import { IEditControllerFlowHandler } from "../Handlers/controllerflowhandler";
import { IUrlHelperService } from "../Services/UrlHelperService";
import { IProgressHandlerFactory } from "../Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../Handlers/messaginghandlerfactory";
import { Guid } from "../../Util/StringUtility";
import { ToolBarButton } from "../../Util/ToolBarUtility";
import { Constants } from "../../Util/Constants";
import { ISmallGenericType } from "../../Scripts/TypeLite.Net4";

export class EditControllerBase2 {
    private tabActivatedCallback: (() => void);
    private tabDeActivatedCallback: (() => void);
    public watchUnRegisterCallbacks: (() => void)[] = []; 

    public progress: IProgressHandler;
    public toolbar: IToolbar;
    public navigatorRecords: ISmallGenericType[];
    public dirtyHandler: IDirtyHandler;

    public showInfoMessage = false;
    public infoMessage: string;
    public infoButtons: ToolBarButton[] = [];

    protected validationHandler: IValidationSummaryHandler;
    protected messagingHandler: IMessagingHandler;
    protected flowHandler: IEditControllerFlowHandler;
    protected isTabActivated: boolean;

    public saveInProgress = false;
    public isNew = true;
    public deleteButtonTemplateUrl: string;
    public saveButtonTemplateUrl: string;
    public modifyPermission: boolean;
    public readOnlyPermission: boolean;
    public documentsPermission: boolean;
    
    public guid: Guid;

    constructor(urlHelperService?: IUrlHelperService,
        progressHandlerFactory?: IProgressHandlerFactory,
        validationSummaryHandlerFactory?: IValidationSummaryHandlerFactory,
        messagingHandlerFactory?: IMessagingHandlerFactory) {
        if (urlHelperService) {
            this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
            this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");
        }

        if (progressHandlerFactory) {
            this.progress = progressHandlerFactory.create();
        }

        if (validationSummaryHandlerFactory) {
            this.validationHandler = validationSummaryHandlerFactory.create();
        }

        if (messagingHandlerFactory) {
            this.messagingHandler = messagingHandlerFactory.create().onSetDirty(x => {
                if (x && x.guid) {
                    if(x.guid === this.guid)
                        this.dirtyHandler.setDirty();
                }
                else {
                    this.dirtyHandler.setDirty();
                }
            });
        }
    }

    private tabActivated(guid: string) {
        this.isTabActivated = guid === this.guid;

        if (this.tabActivatedCallback && this.isTabActivated) {
            this.tabActivatedCallback();
        }
        else if (this.tabDeActivatedCallback && !this.isTabActivated) {
            this.unRegisterWatches();
            this.tabDeActivatedCallback();
        }
    }

    protected setTabCallbacks(onTabActivCallback: () => void, onTabDeActivatedCallback: () => void) {
        this.messagingHandler.onTabActivated((tabGuid) => this.tabActivated(tabGuid));
        this.tabActivatedCallback = onTabActivCallback;
        this.tabDeActivatedCallback = onTabDeActivatedCallback;
    }

    protected copy() {
        this.isNew = true;
        const newGuid = Guid.newGuid();
        this.messagingHandler.publishSetTabLabelNew(this.guid);
        this.messagingHandler.publishSetTabGuid(this.guid, newGuid);
        this.guid = newGuid;
    }

    protected closeMe(reloadGrid: boolean) {
        // Send messages to TabsController
        this.messagingHandler.publishCloseTab(this.guid);
        if (reloadGrid) {
            this.messagingHandler.publishReloadGrid(this.guid);
        }
    }

    protected unRegisterWatches() {
        _.forEach(this.watchUnRegisterCallbacks, (callBack) => {
            callBack();
        });
        this.watchUnRegisterCallbacks = [];
    }

    protected getSaveEvent(): string {
        return this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED;
    }
}