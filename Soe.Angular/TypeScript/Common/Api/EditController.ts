import { EditControllerBase2 } from "../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../Util/CommonEnumerations";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { ApiSettingDTO } from "../Models/ApiSettingDTO";
import { IApiService } from "./ApiService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Init parameters
    private feature: Feature;

    // Terms
    private terms: { [index: string]: string; };

    // Settings
    private settings: ApiSettingDTO[];

    private _selectedSetting: ApiSettingDTO;
    private get selectedSetting(): ApiSettingDTO {
        return this._selectedSetting;
    }
    private set selectedSetting(setting: ApiSettingDTO) {
        this._selectedSetting = setting;

        _.filter(this.settings, s => s.guid !== setting.guid).forEach(s => s.isEditing = false);
    }

    // Filter
    private filterName: string;
    private filterDescription: string;

    private get footerInfo(): string {
        if (!this.terms || !this.settings)
            return '';

        let total = this.settings.length;
        let filtered = this.filteredSettings.length;

        if (total === filtered)
            return "{0} {1}".format(this.terms["core.aggrid.totals.total"], total.toString());
        else
            return "{0} {1} ({2} {3})".format(this.terms["core.aggrid.totals.total"], total.toString(), this.terms["core.aggrid.totals.filtered"], filtered.toString());
    }

    private get filteredSettings(): ApiSettingDTO[] {
        return _.filter(this.settings, s =>
            (!this.filterName || s.name.contains(this.filterName)) &&
            (!this.filterDescription || s.description.contains(this.filterDescription)));
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private apiService: IApiService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.feature = soeConfig.feature;
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.loadTerms())
            .onLoadData(() => this.loadSettings());
    }

    // SETUP

    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: this.feature, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[this.feature].readPermission;
        this.modifyPermission = response[this.feature].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {

    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.aggrid.totals.total",
            "core.aggrid.totals.filtered",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadSettings(): ng.IPromise<any> {
        return this.apiService.getApiSettings().then(x => {
            this.settings = x;
        });
    }

    // EVENTS

    private valueChanged(setting: ApiSettingDTO) {
        this.$timeout(() => {
            setting.isModified = true;
            setting.isEditing = false;
            this.setModified();
        });
    }

    // ACTIONS

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.apiService.saveApiSettings(_.filter(this.settings, s => s.isModified)).then(result => {
                if (result.success) {
                    completion.completed();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadSettings();
            }, error => {

            });
    }

    // HELP-METHODS

    private setModified() {
        this.dirtyHandler.setDirty();
    }
}