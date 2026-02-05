import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { Feature, SoeFieldSettingType } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPreferencesService } from "../PreferencesService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private fieldSettingsType: SoeFieldSettingType;

    //Properties
    private usedCompanyIds: number[];

    //@ngInject
    constructor(
        private preferencesService: IPreferencesService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Accounting.CompanyGroup.CompanyAdministration", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.fieldSettingsType = soeConfig.fieldSettingsType ? soeConfig.fieldSettingsType : SoeFieldSettingType.Mobile;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_Preferences_FieldSettings, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    protected setupGrid() {
        // Columns
        const keys: string[] = [
            "core.function",
            "common.field",
            "manage.preferences.fieldsettings.rolesetting",
            "manage.preferences.fieldsettings.companysetting",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("formName", terms["core.function"], null);
            this.gridAg.addColumnText("fieldName", terms["common.field"], null);
            this.gridAg.addColumnText("companySettingsSummary", terms["manage.preferences.fieldsettings.companysetting"], null);
            this.gridAg.addColumnText("roleSettingsSummary", terms["manage.preferences.fieldsettings.rolesetting"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("manage.preferences.fieldsettings.fieldsettings", true);
        });
    }

    public loadGridData() {
        // Load data
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.preferencesService.getFieldSettings(this.fieldSettingsType).then(data => {
                // Add to grid
                this.setData(data);
            });
        }]);
    }
}
