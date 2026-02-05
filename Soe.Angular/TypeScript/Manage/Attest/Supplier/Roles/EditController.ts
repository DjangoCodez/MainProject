import { EditControllerBase } from "../../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../../Util/ToolBarUtility";
import { IAttestService } from "../../AttestService";
import { Feature } from "../../../../Util/CommonEnumerations";

export class EditController extends EditControllerBase {

    // Data       

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    // Lookups

    //@ngInject
    constructor(
        private attestRoleId: number,
        $uibModal,
        coreService: ICoreService,
        private attestService: IAttestService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants) {

        super("Manage.Attest.Supplier.Roles.Edit", Feature.Manage_Attest_Supplier_AttestRoles_Edit, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);
    }

    // SETUP

    protected setupLookups() {
        this.lookups = 1;
        this.load();
    }

    private setupToolBar() {
        if (this.setupDefaultToolBar()) {
            //Do nothing for now
        }
    }

    // LOOKUPS

    private load() {

        this.lookupLoaded();
    }

    // EVENTS

    protected lookupLoaded() {
        super.lookupLoaded();
        if (this.lookups <= 0) {
            this.setupToolBar();
        }
    }

    // ACTIONS

    private save() {

    }

    protected delete() {

    }

    // HELP-METHODS

    private new() {

    }

    // VALIDATION

    protected validate() {

    }
}
