import { EditControllerBase } from "../../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ISysCompDBDTO } from "../../../../Scripts/TypeLite.Net4";
import { ToolBarButtonGroup } from "../../../../Util/ToolBarUtility";
import { ISystemService } from "../../SystemService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { Feature } from "../../../../Util/CommonEnumerations";

export class EditController extends EditControllerBase {

    // Data       
    sysCompDBDTO: ISysCompDBDTO;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();
    private syscompDBDTOId: number = 0;
    public parameters: any;

    //@ngInject
    constructor(
        $uibModal,
        coreService: ICoreService,
        private systemService: ISystemService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService) {

        super("Manage.SysCompany.SysCompDB.Edit",
            Feature.Manage_System,
            $uibModal,
            translationService,
            messagingService,
            coreService,
            notificationService,
            urlHelperService);
    }

    public init() {
        this.syscompDBDTOId = this.parameters.id || 0;
        this.$q.all([this.load()])
            .then(() => {
                this.setupToolBar();
            });
    }

    // SETUP

    private setupToolBar() {
        super.stopProgress();
    }

    protected startLoad() { }

    private setSysCompDBDTOId(id: number) {
        this.syscompDBDTOId = id;
    }

    // SERVICE CALLS

    private load(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.syscompDBDTOId > 0) {
            this.systemService.getSysCompDB(this.syscompDBDTOId)
                .then((x) => {
                    this.sysCompDBDTO = x;
                    this.isNew = false;
                    deferral.resolve();
                });
        } else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    // ACTIONS

    private save() {
        this.startSave();
        this.systemService.saveSysCompDB(this.sysCompDBDTO).then((result) => {
            if (result.success) {
                if (!this.syscompDBDTOId) {
                    this.setSysCompDBDTOId(result.integerValue);
                }
                return this.systemService.saveSysCompDB(this.sysCompDBDTO);
            }
            return result;
        },
            error => {
                this.failedSave(error.message);
            }).then(result => {
                if (result.success) {
                    this.completedSave(this.sysCompDBDTO);
                    this.closeMe(false);
                } else {
                    this.failedSave(result.errorMessage);
                }
            },
            error => this.failedSave(error.message));
    }

    protected delete() {
        this.failedDelete("Delete not allowed");
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.syscompDBDTOId = 0;
        this.sysCompDBDTO = ({} as ISysCompDBDTO);
    }

    // VALIDATION

    protected validate() {
        if (this.sysCompDBDTO) {
            if (!this.sysCompDBDTO.name) {
                this.mandatoryFieldKeys.push("common.name");
            }
        }
    }
}
