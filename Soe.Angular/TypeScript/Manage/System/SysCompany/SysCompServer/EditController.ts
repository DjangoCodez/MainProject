import { EditControllerBase } from "../../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ISysCompServerDTO } from "../../../../Scripts/TypeLite.Net4";
import { ToolBarButtonGroup } from "../../../../Util/ToolBarUtility";
import { ISystemService } from "../../SystemService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { Feature, TermGroup } from "../../../../Util/CommonEnumerations";

export class EditController extends EditControllerBase {

    // Data       
    syscompServerDTO: ISysCompServerDTO;
    templateTypes: any;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();
    private syscompServerDTOId: number = 0;
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

        super("Time.Employee.SysCompServerDTO.Edit",
            Feature.Manage_System,
            $uibModal,
            translationService,
            messagingService,
            coreService,
            notificationService,
            urlHelperService);
    }

    public init() {
        this.syscompServerDTOId = this.parameters.id || 0;
        this.$q.all([this.load(), this.loadTemplateTypes()])
            .then(() => {
                this.setupToolBar();
            });
    }

    // SETUP

    private setupToolBar() {
        super.stopProgress();
    }

    protected startLoad() { }

    private setSysCompServerDTOId(id: number) {
        this.syscompServerDTOId = id;
    }

    // SERVICE CALLS

    private loadTemplateTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AttestWorkFlowType, false, false).then(templateTypes => {
            this.templateTypes = templateTypes;
        });
    }

    private load(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.syscompServerDTOId > 0) {
            this.systemService.getSysCompServer(this.syscompServerDTOId)
                .then(x => {
                    this.syscompServerDTO = x;
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
        this.systemService.saveSysCompServer(this.syscompServerDTO).then((result) => {
            if (result.success) {
                if (!this.syscompServerDTOId) {
                    this.setSysCompServerDTOId(result.integerValue);
                }
                return this.systemService.saveSysCompServer(this.syscompServerDTO);
            }
            return result;
        },
            error => {
                this.failedSave(error.message);
            }).then(result => {
                if (result.success) {
                    this.completedSave(this.syscompServerDTO);
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
        this.syscompServerDTOId = 0;
        this.syscompServerDTO = ({} as ISysCompServerDTO);
    }

    // VALIDATION

    protected validate() {
        if (this.syscompServerDTO) {
            if (!this.syscompServerDTO.name) {
                this.mandatoryFieldKeys.push("common.name");
            }
        }
    }
}
