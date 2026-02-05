import { IOrderService } from "./OrderService";
import { IToolbar } from "../../../Core/Handlers/Toolbar";
import { IconLibrary } from "../../../Util/Enumerations";
import { ToolBarButton, ToolBarUtility, ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { OrderUserReadyController } from "../Dialogs/OrderUserReady/OrderUserReadyController";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IOriginUserSmallDTO } from "../../../Scripts/TypeLite.Net4";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { OriginUserDTO } from "../../../Common/Models/InvoiceDTO";

export class OrderIsReadyHelper {

    private toolbarGroup: ToolBarButtonGroup;
    private nrOfReadyUsers = 0;
    private totalNrOfUsers = 0;
    public onlyChangeRowStateIfOwner = false;
    public userIsMember = false;
    public userIsMemberAndHasReadyMarked = false;
    public userIsOwner = false;

    constructor(
        private parentOrderEdit: any,
        private $uibModal,
        private orderService: IOrderService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private translationService: ITranslationService
    ) {
        
    }

    public addToolBarButtons(toolbar: IToolbar) {
        this.toolbarGroup = ToolBarUtility.createGroup();

        this.toolbarGroup.buttons.push(new ToolBarButton("", "billing.order.iam.ready", IconLibrary.FontAwesome, "fa-thumbs-up okColor", () => {
            this.updateIsReadyState();
        }, null, () => {
            return (!this.userIsMember || !this.userIsMemberAndHasReadyMarked);
        }));

        this.toolbarGroup.buttons.push(new ToolBarButton("", "billing.order.iam.notready", IconLibrary.FontAwesome, "fa-thumbs-down warningColor", () => {
            this.updateIsReadyState();
        }, null, () => {
            return (!this.userIsMember || this.userIsMemberAndHasReadyMarked);
        }));

        this.updateAdminButton();

        toolbar.addButtonGroup(this.toolbarGroup);
    }

    public invoicedLoaded(originUsers: IOriginUserSmallDTO[]) {
        this.userIsMemberAndHasReadyMarked = false;
        this.userIsMember = false;
        this.userIsOwner = false;
        this.nrOfReadyUsers = 0;
        this.totalNrOfUsers = originUsers.length;
        
        _.forEach(originUsers, (o) => {
            if (o.isReady) {
                this.nrOfReadyUsers = this.nrOfReadyUsers + 1;
            }

            if (CoreUtility.userId === o.userId) {
                this.userIsOwner = o.main;
                this.userIsMember = true;
                this.userIsMemberAndHasReadyMarked = o.isReady;
            }
        });

        this.updateAdminButton();
    }

    public invoiceNew(originUsers: OriginUserDTO[]) {
        this.userIsMemberAndHasReadyMarked = false;
        this.userIsMember = false;
        this.userIsOwner = false;
        this.nrOfReadyUsers = 0;
        this.totalNrOfUsers = originUsers.length;

        _.forEach(originUsers, (o) => {
            if (CoreUtility.userId === o.userId) {
                this.userIsOwner = o.main;
                this.userIsMember = true;
                this.userIsMemberAndHasReadyMarked = false;
            }
        });

        this.updateAdminButton();
    }

    private updateAdminButton() {
        if (this.toolbarGroup && this.parentOrderEdit.invoiceId) {
            this.toolbarGroup.deleteButton("isReadyAdmin");
            let color = "errorColor";
            if (this.nrOfReadyUsers === this.totalNrOfUsers) {
                color = "okColor";
            }
            else if (this.nrOfReadyUsers > 0) {
                color = "warningColor";
            }

            let adminButton = new ToolBarButton("", this.nrOfReadyUsers + "/" + this.totalNrOfUsers, IconLibrary.FontAwesome, "fa-users " + color, () => {
                this.showIsReadyDialog();
            }, null, () => {
                return false;
                });

            adminButton.idString = "isReadyAdmin";
            this.toolbarGroup.buttons.push(adminButton);
        }
    }
    
    private showIsReadyDialog() {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/OrderUserReady/OrderUserReady.html"),
            controller: OrderUserReadyController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                translationService: () => { return this.translationService },
                notificationService: () => { return this.notificationService },
                orderService: () => { return this.orderService },
                invoiceId: () => { return this.parentOrderEdit.invoiceId },
                invoiceNr: () => { return this.parentOrderEdit.invoice.invoiceNr },
                selectedUsers: () => { return this.parentOrderEdit.originUsers },
                allowAdminFunctions: () => { return this.userIsOwner },
                userIsMember: () => { return this.userIsMember },
                readOnly: () => { return this.parentOrderEdit.isLocked }
            }
        });

        modal.result.then(result => {
            this.nrOfReadyUsers = result.nrOfReadyUsers;
            this.updateAdminButton();
        });

        return modal;
    }

    private updateIsReadyState() {
        this.orderService.updateReadyState(this.parentOrderEdit.invoiceId, CoreUtility.userId);
        this.userIsMemberAndHasReadyMarked = !this.userIsMemberAndHasReadyMarked;
    }
}