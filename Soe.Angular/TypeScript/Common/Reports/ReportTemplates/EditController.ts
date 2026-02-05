import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { ReportTemplateDTO } from "../../../Common/Models/ReportDTOs";
import { TermGroup, SoeEntityState, SoeModule, SoeReportTemplateType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ISmallGenericType, IActionResult } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { CoreUtility } from "../../../Util/CoreUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { IconLibrary } from "../../../Util/Enumerations";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };
    
    // Data
    private reportTemplateId: number;
    private reportTemplate: ReportTemplateDTO;
    private templateData: any;
    private sysReportTemplateTypes: ISmallGenericType[] = [];
    private groupingsAndSortings: ISmallGenericType[] = [];
    private changeTemplate: boolean = false;
    private isSystem: boolean = false;
    private module: number;
    private selectableCountries = [];
    private selectedCountries = [];
    private selectableExportTypes = [];
    private selectedExportTypes = [];

    private reportsWithGroups = [
        SoeReportTemplateType.BalanceReport,
        SoeReportTemplateType.ResultReport,
        SoeReportTemplateType.ResultReportV2,
    ]

    private get isEconomyModule() {
        return this.module === SoeModule.Economy;
    }

    private get showConnectReportGroup() {
        return this.isEconomyModule && this.reportTemplate && !!this.reportTemplateId &&
            this.reportsWithGroups.indexOf(this.reportTemplate.sysReportTemplateTypeId) > -1;
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $window,
        private reportService: IReportService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {    
        this.reportTemplateId = parameters.id;
        this.guid = parameters.guid;
        this.module = soeConfig.module;
        this.isSystem = soeConfig.isSys;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[soeConfig.feature].readPermission;
        this.modifyPermission = response[soeConfig.feature].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.report.reporttemplate.connecttoreportgroups", "common.report.reporttemplate.connecttoreportgroups", IconLibrary.FontAwesome, "fal fa-link", () => {
            this.openReportGroups();
        }, null, () => {
            return !this.modifyPermission || !this.reportTemplate.isSystemReport || !this.showConnectReportGroup;
        })));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.download", "common.report.reporttemplate.downloadtemplate", IconLibrary.FontAwesome, "fal fa-download", () => {
            this.downloadTemplate();
        }, null, () => {
            return !this.modifyPermission || !this.reportTemplate || this.reportTemplate.sysReportTypeId > 1 || this.reportTemplateId === 0;
            })));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.fileupload.upload", "common.report.reporttemplate.uploadtemplate", IconLibrary.FontAwesome, "fal fa-upload", () => {
            this.uploadTemplate();
        }, null, () => {
            return !this.modifyPermission || !this.reportTemplate || this.reportTemplate.sysReportTypeId > 1;
        })));
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadExportTypes(),
            this.loadCountries(),
            this.loadSysReportTemplateTypes(),
            this.loadSortingsAndGroupings(),
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.reportTemplateId) {
            return this.load();
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
 
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadSysReportTemplateTypes(): ng.IPromise<any> {
        return this.reportService.getSysReportTemplateTypesForModule(this.module).then(x => {
            this.sysReportTemplateTypes = _.sortBy(x, t => t.name);
        });
    }

    private loadExportTypes(): ng.IPromise<any> {
        return this.reportService.getExportTypes().then((result: ISmallGenericType[]) => {
            this.selectableExportTypes.length = 0;
            _.forEach(result, (exportType: ISmallGenericType) => {
                this.selectableExportTypes.push({
                    id: exportType.id,
                    label: exportType.name
                });
            });
        });
    }

    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(false, true).then((result: ISmallGenericType[]) => {
            this.selectableCountries.length = 0;
            _.forEach(result, (country: ISmallGenericType) => {
                this.selectableCountries.push({
                    id: country.id,
                    label: country.name
                });
            });
        });
    }

    private loadSortingsAndGroupings() {
        return this.coreService.getTermGroupContent(TermGroup.ReportGroupAndSortingTypes, false, false).then(x => {
            this.groupingsAndSortings = x;
        });
    }

    private load(): ng.IPromise<any> {
        return this.reportService.getReportTemplate(this.reportTemplateId, this.isSystem).then(x => {
            this.isNew = false;
            this.reportTemplate = x;
            this.selectedCountries.length = 0;
            if (this.reportTemplate.sysCountryIds) {
                _.forEach(this.reportTemplate.sysCountryIds, (sysCountryId) => {
                    this.selectedCountries.push({
                        id: sysCountryId,
                    });
                });
            }

            this.selectedExportTypes.length = 0;
            if (this.reportTemplate.validExportTypes) {
                _.forEach(this.reportTemplate.validExportTypes, (exportType) => {
                    this.selectedExportTypes.push({
                        id: exportType,
                    });
                });
            }
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.reportTemplate.name);
        });
    }

    private new() {
        this.isNew = true;
        this.reportTemplateId = 0;
        this.reportTemplate = new ReportTemplateDTO();
        this.reportTemplate.module = this.module;
        this.reportTemplate.state = SoeEntityState.Active;
        this.reportTemplate.isSystemReport = false;
    }

    // ACTIONS

    private initSave() {
        if (this.validateSave())
            this.save();
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.reportService.saveReportTemplate(this.reportTemplate, this.templateData, this.isSystem).then(result => {
                if (result.success) {
                    this.reportTemplateId = result.integerValue;
                    this.reportTemplate.reportTemplateId = this.reportTemplateId;
                    this.templateData = null;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.reportTemplate);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData();
        }, error => {
        });
    }

    private delete() {
        if (!this.isSystem) {
            this.progress.startDeleteProgress((completion) => {
                this.reportService.deleteReportTemplate(this.reportTemplate).then(result => {
                    if (result.success) {
                        completion.completed(this.reportTemplate);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }).then(x => {
                super.closeMe(true);
            });
        }
    }

    // EVENTS

    protected selectedExportTypeChanged() {
        this.reportTemplate.validExportTypes = [];
        _.forEach(this.selectedExportTypes, (exportFormat) => {
            this.reportTemplate.validExportTypes.push(exportFormat.id);
        });
        this.dirtyHandler.setDirty();
    }

    protected selectedCountriesChanged() {
        this.reportTemplate.sysCountryIds = [];
        _.forEach(this.selectedCountries, (country) => {
            this.reportTemplate.sysCountryIds.push(country.id);
        });
        this.dirtyHandler.setDirty();
    }

    private openReportGroups() {
        HtmlUtility.openInNewTab(this.$window, "/soe/economy/distribution/reports/reportgroupmapping/?report=" + this.reportTemplate.reportTemplateId);
    }

    public uploadTemplate() {
        this.translationService.translate("core.fileupload.choosefiletoimport").then((term) => {
            var url = CoreUtility.apiPrefix + Constants.WEBAPI_REPORT_REPORTTEMPLATE_UPLOAD;
            var modal = this.notificationService.showFileUpload(url, term, true, true, false, true);
            modal.result.then(res => {
                let result: IActionResult = res.result;
                if (result.success) {
                    this.templateData = result.value;
                    this.reportTemplate.fileName = result.value2;
                    this.dirtyHandler.setDirty();
                }
            }, error => {
            });
        });
    }

    public downloadTemplate() {
        if (soeConfig.isSys)
            HtmlUtility.openInSameTab(this.$window, "/soe/common/distribution/systemplates/edit/download/?systemplate=" + this.reportTemplate.reportTemplateId);
        else
            HtmlUtility.openInSameTab(this.$window, "/soe/common/distribution/templates/edit/download/?template=" + this.reportTemplate.reportTemplateId);
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    private setSelectedExportFormat() {
        if (!this.reportTemplate)
            return;

        this.selectedExportTypes = _.filter(this.selectableExportTypes, f => _.includes(this.reportTemplate.validExportTypes, f.id));
    }

    // VALIDATION

    private validateSave(): boolean {
        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.reportTemplate) {
                if (!this.reportTemplate.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}