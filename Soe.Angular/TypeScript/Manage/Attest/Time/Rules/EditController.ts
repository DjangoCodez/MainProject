import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { AttestRuleHeadDTO } from "../../../../Common/Models/AttestRuleHeadDTO";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { IFocusService } from "../../../../Core/Services/focusservice";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IAttestService } from "../../AttestService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, SoeModule } from "../../../../Util/CommonEnumerations";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { Constants } from "../../../../Util/Constants";
import { IRegistryService } from "../../../Registry/RegistryService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private attestRuleHeadId: number;
    private attestRuleHead: AttestRuleHeadDTO;

    // Lookups
    private dayTypes: SmallGenericType[];
    private scheduledJobHeads: SmallGenericType[];

    //@ngInject
    constructor(
        protected $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private attestService: IAttestService,
        private registryService: IRegistryService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.attestRuleHeadId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Attest_Time_AttestRules_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.attestRuleHeadId);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Attest_Time_AttestRules_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Attest_Time_AttestRules_Edit].modifyPermission;
    }

    // LOOKUPS

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadTerms()]).then(() => {
            this.$q.all([
                this.loadDayTypes(),
                this.loadScheduledJobs()
            ]);
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.all"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadDayTypes(): ng.IPromise<any> {
        this.dayTypes = [];
        return this.attestService.getDayTypesDict(true).then(x => {
            this.dayTypes = x;
            let dayTypeAll = _.find(this.dayTypes, d => d.id === 0);
            if (dayTypeAll)
                dayTypeAll.name = this.terms['common.all'];
        });
    }

    private loadScheduledJobs(): ng.IPromise<any> {
        this.scheduledJobHeads = [];
        return this.registryService.getScheduledJobHeadsDict(true, false, false).then(x => {
            this.scheduledJobHeads = x;
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.attestRuleHeadId) {
            return this.load();
        } else {
            this.new();
        }
    }

    private load(): ng.IPromise<any> {
        return this.attestService.getAttestRuleHead(this.attestRuleHeadId, true, true).then(x => {
            this.isNew = false;
            this.attestRuleHead = x;
            if (this.attestRuleHead.dayTypeId === null || this.attestRuleHead.dayTypeId === undefined)
                this.attestRuleHead.dayTypeId = 0;

            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.attestRuleHead.name);
        });
    }

    private new() {
        this.isNew = true;
        this.attestRuleHeadId = 0;
        this.attestRuleHead = new AttestRuleHeadDTO();
        this.attestRuleHead.attestRuleRows = [];
        this.attestRuleHead.isActive = true;
        this.attestRuleHead.dayTypeId = 0;
        this.attestRuleHead.module = SoeModule.Time;
    }

    // ACTIONS

    public save() {
        if (this.attestRuleHead.dayTypeId === 0)
            this.attestRuleHead.dayTypeId = null;

        this.progress.startSaveProgress((completion) => {
            this.attestService.saveAttestRule(this.attestRuleHead).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.attestRuleHeadId = result.integerValue;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.attestRuleHead);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.onLoadData();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.attestService.deleteAttestRule(this.attestRuleHeadId).then((result) => {
                if (result.success) {
                    completion.completed(this.attestRuleHead, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(false);
        });
    }

    protected copy() {
        if (!this.attestRuleHead)
            return;

        super.copy();

        this.isNew = true;
        this.attestRuleHeadId = 0;
        this.attestRuleHead.attestRuleHeadId = 0;
        this.attestRuleHead.name = "";
        this.attestRuleHead.created = null;
        this.attestRuleHead.createdBy = "";
        this.attestRuleHead.modified = null;
        this.attestRuleHead.modifiedBy = "";
        _.forEach(this.attestRuleHead.attestRuleRows, row => {
            row.attestRuleRowId = 0;
        });

        this.dirtyHandler.setDirty();
        this.focusService.focusByName("ctrl_attestRuleHead_name");
        this.translationService.translate("manage.attest.time.new_rule").then((term) => {
            this.messagingHandler.publishSetTabLabel(this.guid, term);
        });
    }

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.attestRuleHead) {
                if (!this.attestRuleHead.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }
}
