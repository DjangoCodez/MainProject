import { ICoreService } from "../../../../Core/Services/CoreService"
import { ITranslationService } from "../../../../Core/Services/TranslationService"
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SoeModule } from "../../../../Util/CommonEnumerations"
import { DashboardSettingsController, UserGaugeHeadDTO } from "../DashboardSettings/DashboardSettingsController";

export class DashboardSelectionController {
    private heads: UserGaugeHeadDTO[] = [];
    private filter: string;

    //@ngInject
    public constructor(
        private $uibModalInstance,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private module: SoeModule
    ) {
        this.load();
    }

    private load(): ng.IPromise<any> {
        const keys = [
            "common.dashboard.header.time",
            "common.dashboard.header.economy",
            "common.dashboard.header.billing",
            "common.start.module.manage",
        ]
        return this.coreService.getUserGaugeHeads().then(data => {
            this.heads = data.sort((h1, h2) => h1.priority - h2.priority);

            return this.translationService.translateMany(keys).then(terms => {
                let modules = [
                    { name: "", id: SoeModule.None },
                    { name: terms["common.dashboard.header.time"], id: SoeModule.Time },
                    { name: terms["common.dashboard.header.economy"], id: SoeModule.Economy },
                    { name: terms["common.dashboard.header.billing"], id: SoeModule.Billing },
                    { name: terms["common.start.module.manage"], id: SoeModule.Manage },
                ]
                this.heads.forEach(h => {
                    h.moduleName = modules.find(m => m.id === h.module)?.name || ""
                })

            })
        })
    }

    private edit(head: any) {
        let modal = DashboardSettingsController.openAsDialog(this.$uibModal, this.urlHelperService, this.module, head.userGaugeHeadId);
        modal.result.then(() => {
            this.load();
        })
    }

    private new() {
        let modal = DashboardSettingsController.openAsDialog(this.$uibModal, this.urlHelperService, this.module);
        modal.result.then(() => {
            this.load();
        })
    }

    private close(head: any) {
        this.$uibModalInstance.close(head);
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    public static openAsDialog(uibModal: any, urlHelperService: IUrlHelperService, module: SoeModule) {
        var modal = uibModal.open({
            templateUrl: urlHelperService.getGlobalUrl("Common/Dashboard/Dialogs/DashboardSelection/DashboardSelection.html"),
            controller: DashboardSelectionController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                module: () => module,
            }
        });

        return modal;
    }
}