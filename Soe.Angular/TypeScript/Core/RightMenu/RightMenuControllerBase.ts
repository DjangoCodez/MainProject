import { IMessagingService } from "../Services/MessagingService";
import { Constants } from "../../Util/Constants";

export class RightMenuControllerBase {

    protected positionIndex: any;
    protected topPosition: number;
    protected menuType: RightMenuType;

    // Flags
    protected showMenu = false;
    protected fullscreen = false;

    constructor(
        protected $timeout: ng.ITimeoutService,
        protected messagingService: IMessagingService,
        menuType: RightMenuType) {

        this.menuType = menuType;

        this.messagingService.subscribe(Constants.EVENT_OPENING_RIGHT_MENU, (data: any) => {
            if (this.showMenu && data.source) {
                let source: RightMenuType = data.source;
                if (source !== this.menuType)
                    this.toggleShowMenu();
            }
        });
    }

    protected setTopPosition() {
        this.topPosition = 38 * (parseInt(this.positionIndex, 10) + 1);
    }

    protected toggleShowMenu() {
        this.showMenu = !this.showMenu;

        if (this.showMenu)
            this.messagingService.publish(Constants.EVENT_OPENING_RIGHT_MENU, { source: this.menuType });

        this.triggerResize();
    }

    protected toggleFullscreen(): ng.IPromise<any> {
        this.fullscreen = !this.fullscreen;
        return this.triggerResize();
    }

    protected triggerResize(): ng.IPromise<any> {
        let eventObject = jQuery.Event("resize");
        return this.$timeout(() => {
            $(window).trigger(eventObject);
        });
    }
}

export enum RightMenuType {
    Help = 1,
    Academy = 2,
    Message = 3,
    Report = 4,
    Document = 5,
    Information = 6
}