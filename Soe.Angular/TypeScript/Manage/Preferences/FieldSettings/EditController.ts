import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IPreferencesService } from "../PreferencesService";
import { IFieldSettingDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    //Data
    private fieldSetting: IFieldSettingDTO;

    //Lookups
    private terms: { [index: string]: string; };
    private yesNoValues: ISmallGenericType[];

    // Grid
    protected soeGridOptions: ISoeGridOptionsAg;

    get selectedVisibleString() {
        if (!this.fieldSetting || !this.yesNoValues)
            return null;

        return this.getVisibleId(this.fieldSetting.companySetting.visible);
    }
    set selectedVisibleString(value: any) {
        if (value === 1)
            this.fieldSetting.companySetting.visible = true;
        else if (value === 0)
            this.fieldSetting.companySetting.visible = false;
        else
            this.fieldSetting.companySetting.visible = undefined;

        this.dirtyHandler.setDirty();
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private preferencesService: IPreferencesService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            //.onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {

        this.fieldSetting = parameters.dto;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.soeGridOptions = new SoeGridOptionsAg("RoleSettingsGrid", this.$timeout);

        this.flowHandler.start([{ feature: Feature.Manage_Preferences_FieldSettings_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_FieldSettings_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_FieldSettings_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    // LOOKUPS
    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadYesNoValues(),
            () => this.loadTerms()
        ]).then(() => {
            this.setupGrid();
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.role",
            "manage.preferences.fieldsettings.fieldshown",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadYesNoValues(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.YesNoDefault, true, false).then((x) => {
            this.yesNoValues = x;
        });
    }

    private setupGrid() {

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.setMinRowsToShow(this.fieldSetting.roleSettings.length + 1);

        this.soeGridOptions.addColumnText("roleName", this.terms["common.role"], null, { editable: false });
        this.soeGridOptions.addColumnSelect("visible", this.terms["manage.preferences.fieldsettings.fieldshown"], null, {
            editable: true, selectOptions: this.yesNoValues, enableHiding: false, dropdownIdLabel: "id", dropdownValueLabel: "name", displayField: "visibleName" });

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (row, colDef, newValue, oldValue) => { this.afterCellEdit(row, colDef, newValue, oldValue); }));
        this.soeGridOptions.subscribe(events);

        this.soeGridOptions.finalizeInitGrid();

        // Handle visible values
        _.forEach(this.fieldSetting.roleSettings, (setting) => {
            setting.visibleName = this.getVisibleString(setting.visible);
        });

        this.soeGridOptions.setData(this.fieldSetting.roleSettings);
    }

    protected filterValues(filter) {
        return _.orderBy(this.yesNoValues.filter(p => {
            return p.name.contains(filter);
        }), 'name');
    }

    protected allowNavigationFromTypeAhead(value, colDef) {
        if (!value)
            return true;
        const matched = _.some(this.yesNoValues, { 'name': value });
        if (matched)
            return true;

        return false;
    }

    private afterCellEdit(row: any, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;
        const item = _.find(this.yesNoValues, { 'name': newValue });
        if (item) {
            switch (item.id) {
                case 0:
                    row.visible = false;
                    break;
                case 1:
                    row.visible = true;
                    break;
                case 2:
                    row.visible = undefined;
                    break;
            }
            this.dirtyHandler.setDirty();
        }
    }

    private getVisibleString(value: boolean): string {
        if (value === true)
            return this.yesNoValues.find(x=> x.id === 1).name;
        else if (value === false)
            return this.yesNoValues.find(x => x.id === 0).name;
        else
            return this.yesNoValues.find(x => x.id === 2).name;
    }

    private getVisibleId(value: boolean): number {
        if (value === true)
            return this.yesNoValues.find(x => x.id === 1).id;
        else if (value === false)
            return this.yesNoValues.find(x => x.id === 0).id;
        else
            return this.yesNoValues.find(x => x.id === 2).id;
    }

    // ACTIONS
    public save() {
        this.progress.startSaveProgress((completion) => {
            this.preferencesService.saveFieldSetting(this.fieldSetting).then((result) => {
                if (result.success) {
                    completion.completed(Constants.EVENT_EDIT_SAVED);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
            }, error => {
        });
    }
}