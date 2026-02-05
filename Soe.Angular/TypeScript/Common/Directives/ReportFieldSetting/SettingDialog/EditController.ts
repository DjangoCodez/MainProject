import { IHttpService } from "angular";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/validationsummaryhandlerfactory";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { IReportDataService } from "../../../../Core/RightMenu/ReportMenu/ReportDataService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IReportService } from "../../../../Core/Services/reportservice";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { Feature, SoeReportSettingFieldType, SoeReportSettingFieldMetaData } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { values } from "lodash";

interface IFieldSetting {
    visible: boolean;
    defaultValue: boolean | number | string | undefined;
    forceDefaultValue?: boolean;
}

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    private params: any;
    private modal: ng.ui.bootstrap.IModalInstanceService;
    private feature: Feature = Feature.None;
    private labelKey: string = '';
    private fieldType: SoeReportSettingFieldType;
    private settingValues: any[];

    private model: IFieldSetting = <IFieldSetting>{};

    get isCheckbox(): boolean {
        return <SoeReportSettingFieldType>this.fieldType == SoeReportSettingFieldType.Boolean;
    }
    get isNumber(): boolean {
        return <SoeReportSettingFieldType>this.fieldType == SoeReportSettingFieldType.Number;
    }
    get isText(): boolean {
        return <SoeReportSettingFieldType>this.fieldType == SoeReportSettingFieldType.String;
    }

    //@ngInject
    constructor(
        private $window: ng.IWindowService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private urlHelperService: IUrlHelperService,
        private progressHandlerFactory: IProgressHandlerFactory,
        private validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private translationService: ITranslationService,
        private reportService: IReportService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private reportDataService: IReportDataService, private httpService: IHttpService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        if (this.urlHelperService) {
            this.saveButtonTemplateUrl = this.urlHelperService.getCoreComponent("saveButtonComposition.html");
        }

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded(this.onAllPermissionsLoaded.bind(this))

        this.$scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            this.params = parameters;
            this.modal = parameters.modal;
            this.feature = parameters.feature || Feature.None;
            this.fieldType = <SoeReportSettingFieldType>parameters.fieldType;
            this.labelKey = (parameters.labelKey && this.translationService.translateInstant(parameters.labelKey)) || '';
            this.settingValues = parameters.settingValues || [];
            this.onInit();
        });
    }

    public onInit() {
        this.flowHandler.start({ feature: this.feature, loadReadPermissions: true, loadModifyPermissions: true });
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.setSettingValues();
    }

    private setSettingValues() {
        this.model.visible = this.settingValues.find(s => s.type === SoeReportSettingFieldMetaData.IsVisible)?.value === 'true' ?? false;

        if (this.isCheckbox) {
            const val = this.settingValues.find(s => s.type === SoeReportSettingFieldMetaData.DefaultValue)?.value;
            this.model.defaultValue = (val === 'true');
        }
        else if (this.isNumber)
            this.model.defaultValue = parseInt(this.settingValues.find(s => s.type === SoeReportSettingFieldMetaData.DefaultValue)?.value) ?? null;
        else if (this.isText)
            this.model.defaultValue = this.settingValues.find(s => s.type === SoeReportSettingFieldMetaData.DefaultValue)?.value ?? '';

        this.model.forceDefaultValue = this.settingValues.find(s => s.type === SoeReportSettingFieldMetaData.ForceDefaultValue)?.value === 'true' ?? false;
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[this.feature].readPermission;
        this.modifyPermission = response[this.feature].modifyPermission;
    }

    private closeModal(isModified: boolean) {
        if (isModified) {
            this.modal.close([
                {
                    type: SoeReportSettingFieldMetaData.IsVisible,
                    value: !!this.model.visible,
                },
                {
                    type: SoeReportSettingFieldMetaData.DefaultValue,
                    value: this.model.defaultValue,
                },
                {
                    type: SoeReportSettingFieldMetaData.ForceDefaultValue,
                    value: !!this.model.forceDefaultValue,
                }
            ]);
        }
        else {
            this.modal.dismiss();
        }
    }

    private save() {
        this.closeModal(true);
    }

    private delete() {
        this.closeModal(false);
    }
}