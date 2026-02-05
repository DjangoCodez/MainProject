import { ICoreService } from "../../../../Core/Services/CoreService"
import { ITranslationService } from "../../../../Core/Services/TranslationService"
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IUserGaugeDTO, IUserGaugeHeadDTO } from "../../../../Scripts/TypeLite.Net4";
import { SoeModule } from "../../../../Util/CommonEnumerations";
import { SmallGenericType } from "../../../Models/SmallGenericType";


export class UserGaugeHeadDTO implements IUserGaugeHeadDTO {
    actorCompanyId: number;
    module: SoeModule;
    moduleName: string;
    name: string;
    description: string;
    priority: number;
    userGaugeHeadId: number;
    userGauges: IUserGaugeDTO[];
    userId: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;

    constructor() {
        this.priority = null;
        this.module = SoeModule.None;
    }
}


export class DashboardSettingsController {
    private modules: SmallGenericType[] = [];
    private head: IUserGaugeHeadDTO;

    //@ngInject
    public constructor(
        private $uibModalInstance,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private userGaugeHeadId: number,
        private module: SoeModule,
    ) {
        this.doLookups();
    }

    private doLookups() {
        let lookups = [
            this.loadModules()
        ]

        if (this.userGaugeHeadId && this.userGaugeHeadId > 0) {
            lookups.push(this.loadUserGaugeHead());
        } else {
            this.new();
        }

        return this.$q.all(lookups)
    }

    private loadModules(): ng.IPromise<any> {
        const keys = [
            "common.dashboard.header.time",
            "common.dashboard.header.economy",
            "common.dashboard.header.billing",
            "common.start.module.manage",
        ]
        return this.translationService.translateMany(keys).then(terms => {
            this.modules = [
                { name: "", id: SoeModule.None },
                { name: terms["common.dashboard.header.time"], id: SoeModule.Time },
                { name: terms["common.dashboard.header.economy"], id: SoeModule.Economy },
                { name: terms["common.dashboard.header.billing"], id: SoeModule.Billing },
                { name: terms["common.start.module.manage"], id: SoeModule.Manage },
            ]
        })
    }

    private loadUserGaugeHead(): ng.IPromise<any> {
        return this.coreService.getUserGaugeHead(this.userGaugeHeadId).then(data => {
            this.head = data;
        })
    }

    private new() {
        this.head = { ...new UserGaugeHeadDTO, module: this.module, priority: 0 }
    }

    private ok() {
        this.coreService.saveUserGaugeHead(this.head).then(data => {
            this.$uibModalInstance.close(data.integerValue)
        })
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    public static openAsDialog(uibModal: any, urlHelperService: IUrlHelperService, module: SoeModule, userGaugeHeadId?: number) {
        let modal = uibModal.open({
            templateUrl: urlHelperService.getGlobalUrl("Common/Dashboard/Dialogs/DashboardSettings/DashboardSettings.html"),
            controller: DashboardSettingsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                module: () => module,
                userGaugeHeadId: () => userGaugeHeadId || 0,
            }
        });

        return modal;
    }
}