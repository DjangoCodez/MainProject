import { IMessagingService } from "../../Core/Services/MessagingService";
import { AngularJsLegacyType, Feature } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";

export class LegacyController {

    private type: AngularJsLegacyType;
    private feature: Feature;

    //@ngInject
    constructor(private messagingService: IMessagingService) {
        this.type = ajsLegacy.type;
        this.feature = ajsLegacy.feature;
    }

    $onInit() {
        switch (this.type) {
            case AngularJsLegacyType.RightMenu_Document:
                this.messagingService.publish(Constants.EVENT_SHOW_DOCUMENT_MENU, ajsLegacy.data);
                break;
            case AngularJsLegacyType.RightMenu_Help:
                this.messagingService.publish(Constants.EVENT_SHOW_HELP_MENU, { feature: this.feature });
                break;
            case AngularJsLegacyType.RightMenu_Information:
                this.messagingService.publish(Constants.EVENT_SHOW_INFORMATION_MENU, null);
                break;
            case AngularJsLegacyType.RightMenu_Message:
                this.messagingService.publish(Constants.EVENT_SHOW_MESSAGE_MENU, ajsLegacy.data);
                break;
            case AngularJsLegacyType.RightMenu_Report:
                this.messagingService.publish(Constants.EVENT_SHOW_REPORT_MENU, { showFavorites: true });
                break;
        }
    }
}