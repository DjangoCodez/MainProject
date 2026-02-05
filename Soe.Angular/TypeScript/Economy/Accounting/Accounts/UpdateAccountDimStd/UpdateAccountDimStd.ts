import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IAccountingService } from "../../../../Shared/Economy/Accounting/AccountingService";

export class UpdateAccountDimStd {
    private accountStdTypes: SmallGenericType[] = [];
    private accountStdTypeId: number = 0;
    private progressBusy: boolean = false;


    //@ngInject
    constructor(
        private $uibModalInstance,
        private accountingService: IAccountingService,
        private notificationService: INotificationService,
    ) {
        this.progressBusy = true;
        accountingService.getSysAccountStdTypes().then(data => {
            this.accountStdTypes = data;
            this.progressBusy = false;
        })
    }

    private buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private buttonOkClick() {
        this.progressBusy = true;
        this.accountingService.importSysAccountStdType(this.accountStdTypeId).then(result => {
            this.progressBusy = false;

            if (result.success) {
                this.$uibModalInstance.close();
            } else {
                this.notificationService.showErrorDialog("", result.errorMessage, "");
            }
        })
    }
}