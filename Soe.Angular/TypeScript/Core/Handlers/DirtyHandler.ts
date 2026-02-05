import { IMessagingService } from "../Services/MessagingService";
import { Guid } from "../../Util/StringUtility";
import { Constants } from "../../Util/Constants";

export interface IDirtyHandler {
    setDirty();
    clean();
    setSecondaryEvent(event: string);
    isDirty;
}

export class DirtyHandler implements IDirtyHandler {
    private dirty: boolean = false;
    private secondaryEvent: string;

    constructor(private messagingService: IMessagingService, private guid: Guid) { }

    setDirty() {
        this.isDirty = true;
    }
    clean() {
        this.isDirty = false;
    }
    setSecondaryEvent(event: string) {
        this.secondaryEvent = event;
    }

    get isDirty() {
        return this.dirty;
    }
    set isDirty(dirty: boolean) {
        this.dirty = dirty;
        this.messagingService.publish(Constants.EVENT_SET_TAB_MODIFIED, {
            guid: this.guid,
            dirty: dirty
        });

        if (this.secondaryEvent) {
            this.messagingService.publish(this.secondaryEvent, {
                guid: this.guid,
                dirty: dirty
            });
        }
    }
}
