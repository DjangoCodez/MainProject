import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IOrderService } from "../../Orders/OrderService";
import { OriginUserDTO } from "../../../../Common/Models/InvoiceDTO";
import { SoeGridOptionsAg, ISoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { INotificationService } from "../../../../Core/Services/NotificationService";

export class OrderUserReadyController {
    private soeGridOptions: ISoeGridOptionsAg;
    private users: any[] = [];
    private sendMessage: boolean = false;
    private rowsSelected: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private orderService: IOrderService,
        private invoiceId: number,
        private invoiceNr: string,
        private allowAdminFunctions: boolean,
        private userIsMember: boolean,
        private readOnly: boolean,
        private selectedUsers: any[]) {

        this.soeGridOptions = new SoeGridOptionsAg("Common.Dialogs.SelectUsers", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.setMinRowsToShow(8);
    }

    $onInit() {
        this.$timeout(() => {
            this.setupGrid();

            this.loadUsersReadyState().then(() => {
                //this.setSelectedUsers();
            });

        });
    }

    private setupGrid() {

        // Columns
        const keys: string[] = [
            "common.username",
            "common.name",
            //"common.main",
            "common.date",
            //"billing.order.selectusers.responsible",
            "billing.order.ready",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];
        this.soeGridOptions.setStandardSubscriptions((rows:any[]) => this.onGridRowSelected(rows) );
        this.translationService.translateMany(keys).then((terms) => {
            this.soeGridOptions.addColumnText("name", terms["common.name"], null);
            this.soeGridOptions.addColumnText("readyStatus", terms["billing.order.ready"], null);
            this.soeGridOptions.addColumnDateTime("readyDate", terms["common.date"], null);

            this.soeGridOptions.addTotalRow("#totals-grid", {
                filtered: terms["core.aggrid.totals.filtered"],
                total: terms["core.aggrid.totals.total"]
            });

            this.soeGridOptions.finalizeInitGrid();
        });
    }

    private onGridRowSelected(rows: any[]) {
        this.rowsSelected = (Array.isArray(rows) && rows.length > 0);
        this.$scope.$applyAsync();
    }

    private loadUsersReadyState(): ng.IPromise<any> {

        return this.translationService.translateMany(['core.yes', 'core.no']).then((terms) => {
            return this.orderService.getOriginUsers(this.invoiceId).then((users: OriginUserDTO[]) => {
                
                _.forEach(users, (user) => {
                    user["readyStatus"] = user["readyDate"] ? terms["core.yes"] : terms["core.no"];
                });

                this.users = users;
                this.soeGridOptions.setData(this.users);
            });
        });
    }

    private sendReminder() {
        const selectedUsersIds = this.soeGridOptions.getSelectedIds("userId");
        if (selectedUsersIds.length > 0) {
            this.orderService.sendReminderForReadyState(this.invoiceId, this.invoiceNr, selectedUsersIds).then((result) => {
                if (result.success) {
                    this.translationService.translate("common.sent").then((term) => {
                        const modal = this.notificationService.showDialog(term, term, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                    });
                }
            })
        }
    }

    private clearState() {
        const selectedUsersIds = this.soeGridOptions.getSelectedIds("userId");
        if (selectedUsersIds.length > 0) {
            this.orderService.clearReadyState(this.invoiceId, selectedUsersIds).then(() => {
                this.loadUsersReadyState();
            });
        }
    }

    public allowRowFunctions() {
        return this.rowsSelected && !this.readOnly;
    }

    buttonOkClick() {
        let readyUsers = this.users.filter(u => u.readyDate);
        this.$uibModalInstance.close({ nrOfReadyUsers: readyUsers.length});
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}