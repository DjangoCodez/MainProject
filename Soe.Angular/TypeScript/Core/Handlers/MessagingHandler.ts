import { IMessagingService, ISubscription } from "../Services/MessagingService";
import { Guid } from "../../Util/StringUtility";
import { Constants } from "../../Util/Constants";

export interface IMessagingHandler {
    onSetDirty(callback: (message: any) => void): IMessagingHandler;
    onGridDataReloadRequired(callback: (message: any) => void): IMessagingHandler;
    onTabActivated(callback: (message: any) => void): IMessagingHandler;
    publishEvent(event, data);
    publishTabModified(guid: Guid, dirty: boolean);
    publishCloseTab(guid: Guid);
    publishReloadGrid(guid: Guid);
    publishActivateAddTab();
    publishEditRow(row: any, data?: any);
    publishOpenTab(message: any);
    publishSetTabGuid(guid: Guid, newGuid: Guid);
    publishSetTabLabel(guid: Guid, label: string, id?: any);
    publishSetTabLabelNew(guid: Guid, label?: string);
    publishTabActivated(guid: Guid);
    publishSetDirty(guid?: any);
    publishResizeWindow();
}

export class MessagingHandler implements IMessagingHandler {
    private setDirtySubscription: ISubscription;
    private setTabActivedSubscription: ISubscription;
    private reloadRequiredSubscription: ISubscription[]
    private dirtyMessageCallbacks: ((message: any) => void)[] = [];
    private reloadRequiredCallbacks: ((message: any) => void)[] = [];
    private tabActivedCallbacks: ((message: any) => void)[] = [];

    constructor(private messagingService: IMessagingService) { }

    onSetDirty(callback: (message: any) => void) {
        if (!this.setDirtySubscription) {
            this.setDirtySubscription = this.messagingService.subscribe(Constants.EVENT_SET_DIRTY, (x) => {
                _.each(this.dirtyMessageCallbacks, y => y(x));
            });
        }

        this.dirtyMessageCallbacks.push(callback);

        return this;
    }
    onGridDataReloadRequired(callback: (message: any) => void) {
        if (!this.reloadRequiredSubscription) {
            this.reloadRequiredSubscription = [
                this.messagingService.subscribe(Constants.EVENT_EDIT_SAVED, (x) => { _.each(this.reloadRequiredCallbacks, y => y(x)); }),
                this.messagingService.subscribe(Constants.EVENT_EDIT_ADDED, (x) => { _.each(this.reloadRequiredCallbacks, y => y(x)); }),
                this.messagingService.subscribe(Constants.EVENT_EDIT_DELETED, (x) => { _.each(this.reloadRequiredCallbacks, y => y(x)); }),
                this.messagingService.subscribe(Constants.EVENT_RELOAD_GRID, (x) => { _.each(this.reloadRequiredCallbacks, y => y(x)); })
            ];
        }

        this.reloadRequiredCallbacks.push(callback);

        return this;
    }

    onTabActivated(callback: (guid: any) => void) {
        if (!this.setTabActivedSubscription) {
            this.setTabActivedSubscription = this.messagingService.subscribe(Constants.EVENT_TAB_ACTIVATED, (x) => {
                _.each(this.tabActivedCallbacks, y => y(x));
            });
        }

        this.tabActivedCallbacks.push(callback);

        return this;
    }

    publishEvent(event, data) {
        this.messagingService.publish(event, data);
    }

    publishTabModified(guid: Guid, dirty: boolean) {
        this.messagingService.publish(Constants.EVENT_SET_TAB_MODIFIED, {
            guid: guid,
            dirty: dirty
        });
    }
    publishCloseTab(guid: Guid) {
        this.messagingService.publish(Constants.EVENT_CLOSE_TAB, guid);
    }
    publishReloadGrid(guid: Guid) {
        this.messagingService.publish(Constants.EVENT_RELOAD_GRID, guid);
    }
    publishActivateAddTab() {
        this.messagingService.publish(Constants.EVENT_ACTIVATE_ADD_TAB, null);
    }
    publishEditRow(row: any, data: any = null) {
        this.messagingService.publish(Constants.EVENT_EDIT, { row: row, data: data });
    }
    publishOpenTab(message: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
    }
    publishTabActivated(guid: Guid) {
        this.messagingService.publish(Constants.EVENT_TAB_ACTIVATED, guid);
    }
    publishSetDirty(guid: any = null) {
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, guid ? { guid: guid } : {});
    }
    publishResizeWindow() {
        this.messagingService.publish(Constants.EVENT_RESIZE_WINDOW, null);
    }

    publishSetTabGuid(guid: Guid, newGuid: Guid) {
        this.messagingService.publish(Constants.EVENT_SET_TAB_GUID, {
            guid: guid,
            newGuid: newGuid
        });
    }

    publishSetTabLabel(guid: Guid, label: string, id?: any) {
        this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
            guid: guid,
            label: label,
            id: id
        });
    }

    publishSetTabLabelNew(guid: Guid, label?: string) {
        this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
            guid: guid,
            label: label,
        });
    }
}

