import { IUserSmallDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { SupplierService } from "../../../../../../Shared/Economy/Supplier/SupplierService";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../../../Util/SoeGridOptionsAg";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../../../../../Util/Enumerations";
import { AttestFlow_ReplaceUserReason, Feature } from "../../../../../../Util/CommonEnumerations";

export class AttestWorkFlowUserSelectorController {
    reasonMessage: string;
    private terms: { [index: string]: string; };
    loginNameColumn: uiGrid.IColumnDef;
    private checkableUsers: Checkable<IUserSmallDTO>[];
    private title: string;
    private hasChecked: boolean = false;

    private soeGridOptions: ISoeGridOptionsAg;


    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private result: any,
        private row: any,
        private reason: AttestFlow_ReplaceUserReason,
        private $timeout: ng.ITimeoutService,
        private supplierService: SupplierService,
        private notificationService: INotificationService,
        private $q: ng.IQService) {
        this.result = result;
        this.row = row;
    }

    private $onInit() {
        this.soeGridOptions = new SoeGridOptionsAg("AttestUsers", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.setMinRowsToShow(8);
        this.soeGridOptions.enableRowSelection = false;
        this.setupGrid().then(() => this.loadGridData())
    }


    protected setupGrid() {
        var keys: string[] = [
            "common.categories.selected",
            "common.name",
            "common.user",
            "core.info",
            "core.remove",
            "economy.supplier.invoice.attestusersmissing",
            "economy.supplier.invoice.attestuserremove",
            "economy.supplier.invoice.attestusertransfer",
            "core.attestflowtransfertoother",
            "core.attestflowtransfertootherwithreturn",
            "economy.supplier.invoice.attestusertransferwithreturn"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
            this.soeGridOptions.addColumnBool("checked", this.terms["common.categories.selected"], 30, { enableEdit: true, onChanged: this.rowSelected.bind(this), suppressFilter: true });
            this.soeGridOptions.addColumnText("entity.name", this.terms["common.name"], null, null, null);//empty tooltip changes what template we use, and we want to use the tooltip one
            this.soeGridOptions.addColumnText("entity.loginName", this.terms["common.user"], null, null, null);
            this.soeGridOptions.finalizeInitGrid();

            switch (this.reason) {
                case AttestFlow_ReplaceUserReason.Remove:
                    this.title = this.terms["core.remove"]
                    this.reasonMessage = this.terms["economy.supplier.invoice.attestuserremove"].format(this.row.name);
                    break;
                case AttestFlow_ReplaceUserReason.Transfer:
                    this.title = this.terms["core.attestflowtransfertoother"];
                    this.reasonMessage = this.terms["economy.supplier.invoice.attestusertransfer"];
                    break;
                case AttestFlow_ReplaceUserReason.TransferWithReturn:
                    this.reasonMessage = this.terms["economy.supplier.invoice.attestusertransferwithreturn"];
                    this.title = this.terms["core.attestflowtransfertootherwithreturn"]
                    break;
                default:
            }
        });
    }

    public loadGridData() {
        this.supplierService.getAttestWorkFlowUsersByAttestTransitionId(this.row.attestTransitionId)
            .then((data: IUserSmallDTO[]) => {
                this.checkableUsers = data.map(u => new Checkable(u)).filter(x => x.entity.userId !== this.row.userId);
                this.soeGridOptions.setData(this.checkableUsers);
                if (!this.checkableUsers || this.checkableUsers.length === 0)
                    this.notificationService.showDialog(this.terms["core.info"], this.terms["economy.supplier.invoice.attestusersmissing"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
            });
    }

    private rowSelected(item) {
        this.hasChecked = item.data.checked === true ? true : false;
        _.filter(this.checkableUsers, (u) => u.entity.userId !== item.data.entity.userId).forEach((u) => {
            if (u.checked)
                u.checked = false
        });
        this.soeGridOptions.setData(this.checkableUsers);
    }

    public cancel() {
        this.result.getPreviousResult = false;
        this.$uibModalInstance.close();
    }

    public ok() {
        var checked = _.find(this.checkableUsers, c => c.checked);
        if (checked) {
            this.result.selectedUser = checked.entity;
            this.$uibModalInstance.close(this.result);
        }
    }
}

class Checkable<T> {
    public entity: T;
    public checked: boolean;

    constructor(entity: T) {
        this.entity = entity;
        this.checked = false;
    }
}