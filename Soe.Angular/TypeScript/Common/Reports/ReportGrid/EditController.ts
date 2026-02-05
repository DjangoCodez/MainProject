import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Feature, TermGroup, TermGroup_ReportExportType, SoeModule, SoeReportTemplateType, TermGroup_ReportExportFileType, SoeReportType, TermGroup_ReportSettingType, SettingDataType } from "../../../Util/CommonEnumerations";
import { IReportService } from "../../../Core/Services/ReportService";
import { ISmallGenericType, IReportDTO, IReportSettingDTO } from "../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { Guid } from "../../../Util/StringUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SysReportTemplateViewGridDTO } from "../../Models/ReportDTOs";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { IHttpService } from "../../../Core/Services/httpservice";
import { IMessagingService } from "../../../Core/Services/MessagingService";

interface IInitParameter {
    id: number,
    guid: string
}
interface IModalInitParameter extends IInitParameter {
    modal: ng.ui.bootstrap.IModalInstanceService,
    sysReportType: SoeReportType,
    module: SoeModule,
    feature: Feature,
    reportTemplateId: number,
    isCompanyTemplate: boolean
}

type SaveReportResult = {
    success: boolean;
    integerValue: number;
    message: string;
};

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private reportId: number;
    private sysReportType: SoeReportType;
    private module: number;
    private feature: number;
    private userTemplateFeature: number;
    private userTemplatePermission: boolean;
    private sysTemplatePermission: boolean;

    private userReportTemplates: { reportTemplateId: number, reportNr: number, name: string, description: string, combinedDisplayName: string }[] = [];
    private sysReportTemplates: SysReportTemplateViewGridDTO[];
    private exportTypes: ISmallGenericType[];
    private exportFileTypes: ISmallGenericType[];
    private settingTypes: ISmallGenericType[];
    private showSettings = false;

    private visibleOptionalFields: { [key: string]: boolean } = {
        "exportFileType": false,
        "includeAllHistoricalData": false,
        "includeBudget": false,
        "noOfYearsBackinPreviousYear": false,
        "getDetailedInformation": false,
        "showInAccountingReports": false,
        "projectOverviewExtendedInfo": false,
        "stockInventoryExcludeZeroQuantity": false,
        "excludeItemsWithZeroQuantityForSpecificDate": false,
        "hidePriceAndRowSum": false,
        "groupedInvoiceByTaxDeduction": false,
        "groupedOfferByTaxDeduction": false,
    };

    private settingValues: { [key: number]: any } = {
        3: false
    };
   
    private selectableRoles: ISmallGenericType[];
    private selectedRoles: ISmallGenericType[] = [];

    private report: IReportDTO = <IReportDTO>{};
    private selectedUserReportTemplateId: number = 0;
    private selectedSysReportTemplateId: number = 0;
    private selectedUserReportTemplate: any = undefined;
    private selectedSysReportTemplate: SysReportTemplateViewGridDTO = undefined;

    private groups: ISmallGenericType[];
    private enableGroupingSorting = () => this.groups && this.groups.length > 0;

    private isModal: boolean = false;
    private modal: ng.ui.bootstrap.IModalInstanceService;
    private reportCompanies: ISmallGenericType[];
    private sysCompanyReports: ISmallGenericType[];
    private newGroupsHeadings: boolean;
    private selectedImportCompanyId: number;
    private selectedImportReportId: number;
    private isGroupMapping: boolean;

    private get isAnalysis(): boolean {
        return this.sysReportType && this.sysReportType === SoeReportType.Analysis;
    }

    //@ngInject
    constructor(
        private $window: ng.IWindowService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private reportService: IReportService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private reportDataService: IReportDataService,
        httpService: IHttpService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded(this.onAllPermissionsLoaded.bind(this))
            .onDoLookUp(this.onDoLookups.bind(this))
            .onLoadData(this.onLoadData.bind(this));

        this.$scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters: IModalInitParameter) => {
            //Pre-configure when opened as a modal
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.sysReportType = parameters.sysReportType;
            this.module = parameters.module;
            this.feature = parameters.feature;

            //Possible to set initial selected report template (but will be overwritten once a report is downloaded)
            if (parameters.isCompanyTemplate)
                this.selectedUserReportTemplateId = parameters.reportTemplateId;
            else
                this.selectedSysReportTemplateId = parameters.reportTemplateId;

            this.onInit(parameters);
        });
    }

    public onInit(parameters: IInitParameter) {
        this.reportId = parameters.id;
        this.guid = parameters.guid;
        //set properties from soeConfig if not already set
        this.module = this.module || soeConfig.module;
        this.feature = this.feature || soeConfig.feature;
        this.userTemplateFeature = this.getUserTemplateFeature();
        
        this.flowHandler.start([{ feature: this.feature, loadReadPermissions: true, loadModifyPermissions: true }, { feature: this.userTemplateFeature, loadReadPermissions: true, loadModifyPermissions: true }]);
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.selectableRoles = [];        
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[this.feature].readPermission;
        this.modifyPermission = response[this.feature].modifyPermission;

        this.userTemplatePermission = response[this.userTemplateFeature].readPermission;
        this.sysTemplatePermission = CoreUtility.isSupportAdmin;
    }

    private onDoLookups(): ng.IPromise<any> {
        let queue = [];

        if (!this.isAnalysis)
            queue.push(() => this.loadUserReportTemplates());       

        queue.push(() => this.loadSysReportTemplates());
        queue.push(() => this.loadExportFileTypes());
        queue.push(() => this.loadGroupsAndSorts());
        queue.push(() => this.loadRoles());
        queue.push(() => this.loadSettingTypes());
        queue.push(() => this.getCompaniesByLicense(soeConfig.licenseNr, false));

        return this.progress.startLoadingProgress(queue);
    }

    private loadUserReportTemplates(): ng.IPromise<any> {
        return this.reportService.getUserReportTemplatesForModule(this.module).then(x => {
            this.userReportTemplates = x;
            this.selectedUserReportTemplate = this.selectedUserReportTemplateId !== 0 ? _.find(this.userReportTemplates, r => r.reportTemplateId == this.selectedUserReportTemplateId) : undefined;
        });
    }

    private loadSysReportTemplates(): ng.IPromise<any> {
        return this.reportService.getSysReportTemplatesForModule(this.module, true).then(x => {
            this.sysReportTemplates = x;
            this.selectedSysReportTemplate = this.selectedSysReportTemplateId !== 0 ? _.find(this.sysReportTemplates, r => r.sysReportTemplateId == this.selectedSysReportTemplateId) : undefined;
        });
    }

    private loadExportFileTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ReportExportFileType, false, false).then(x => {
            this.exportFileTypes = x;
        });
    }

    private loadSettingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ReportSettingType, false, false).then(x => {
            this.settingTypes = x;
        });
    }

    private loadGroupsAndSorts(): ng.IPromise<any> {
        return this.reportDataService.getGroupsAndSorts(this.reportId, this.feature).then(x => {
            this.groups = x;
        });
    }

    private loadRoles(): ng.IPromise<any> {
        return this.coreService.getCompanyRolesDict(false, false).then(x => {
            this.selectableRoles = x;
        });
    }

    private loadExportTypes(sysReportTemplateId: number, userReportTemplateId: number): ng.IPromise<any> {
        return this.reportService.getReportExportTypes(sysReportTemplateId, userReportTemplateId, this.isAnalysis ? SoeReportType.Analysis : SoeReportType.CrystalReport).then(x => {
            this.exportTypes = x;
        });
    }

    private getCompaniesByLicense(licenseNr: string, onlytemplate: boolean): ng.IPromise<any> {
        this.reportCompanies = [];
        this.reportService.getCompaniesByLicense(licenseNr, onlytemplate).then(x => {
            _.forEach(x, (y: any) => {
                this.reportCompanies.push({ id: y.actorCompanyId, name: y.name })
            });
        });

        return this.reportService.getGlobalTemplateCompanies().then(x => {
            _.forEach(x, (y: any) => {
                this.reportCompanies.push({ id: y.actorCompanyId, name: y.name + "(" + "Global" + ")" })
            });
        });
    }

    private GetSysReportTemplateType(): boolean {
        this.isGroupMapping = false;
        this.reportService.GetSysReportTemplateType(this.report).then(x => {
            if (x != null) {
                if (x.groupMapping)
                    this.isGroupMapping = true;
            }
        });

        return this.isGroupMapping;
    }

    private getReportsByTemplateTypeDict(): ng.IPromise<any> {
        return this.reportService.GetReportsByTemplateTypeDict(this.selectedImportCompanyId, this.report.sysReportTemplateTypeId, true, false, true, false).then(x => {
            _.forEach(x, (y: any) => {
                this.sysCompanyReports.push({ id: y.id, name: y.name })
            });
        });
    }

    private getUserTemplateFeature() {
        switch (this.module) {
            case SoeModule.Billing:
                return Feature.Billing_Distribution_Templates_Edit_Download;
            case SoeModule.Economy:
                return Feature.Economy_Distribution_Templates_Edit_Download;
            case SoeModule.Time:
                return Feature.Time_Distribution_Templates_Edit_Download;
        }
    }

    private getsettingName(type: number): string {
        return this.settingTypes.find(x => x.id == type).name;
    }

    private onLoadData(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => {
                this.isNew = !this.reportId;

                if (this.isNew) {
                    this.createNewReport();
                    return this.$q.resolve();
                }

                return this.reportService.getReportForPrint(this.reportId, false, true, true, true, true).then(report => {
                    this.report = report;

                    this.selectedUserReportTemplate = this.report.standard ? undefined : _.find(this.userReportTemplates, r => r.reportTemplateId == this.report.reportTemplateId);
                    this.selectedSysReportTemplate = this.report.standard ? _.find(this.sysReportTemplates, r => r.sysReportTemplateId == this.report.reportTemplateId) : undefined;

                    this.mapSelectedRoles();
                    this.evaluateExportFileType(this.report.exportType);
                    this.evaluateSelectedReportTemplate();
                    this.evaluateOptionalProperties();
                    this.GetSysReportTemplateType();
                    this.loadSettingsToView();

                    this.dirtyHandler.clean();
                })
            }
        ]);
    }

    private loadSettingsToView() {
        if (this.report?.settings) {
            const allowedSettings = [
                TermGroup_ReportSettingType.ProjectOverviewExtendedInfo,
                TermGroup_ReportSettingType.StockInventoryExcludeZeroQuantity,
                TermGroup_ReportSettingType.ExcludeItemsWithZeroQuantityForSpecificDate,
                TermGroup_ReportSettingType.HidePriceAndRowSum,
                TermGroup_ReportSettingType.GroupedInvoiceByTaxDeduction,
                TermGroup_ReportSettingType.GroupedOfferByTaxDeduction,
            ];

            this.report.settings.forEach(s => {
                if (allowedSettings.includes(s.type)) {
                    this.settingValues[s.type] = s.boolData;
                }
            })
        }
    }

    private populateReportSettingsForSave() {
        if (!this.report.settings) {
            this.report.settings = [];
        }

        const settingTypes = [
            TermGroup_ReportSettingType.ProjectOverviewExtendedInfo,
            TermGroup_ReportSettingType.StockInventoryExcludeZeroQuantity,
            TermGroup_ReportSettingType.ExcludeItemsWithZeroQuantityForSpecificDate,
            TermGroup_ReportSettingType.HidePriceAndRowSum,
            TermGroup_ReportSettingType.GroupedInvoiceByTaxDeduction,
            TermGroup_ReportSettingType.GroupedOfferByTaxDeduction,
        ];

        settingTypes.forEach(type => {
            const fieldKey = this.getFieldKey(type);
            if (!this.visibleOptionalFields[fieldKey]) return;

            const currentSetting = this.report.settings.find(s => s.type === type);
            const viewValue = this.settingValues[type];

            if (currentSetting) {
                if (currentSetting.value !== viewValue) {
                    this.setSettingValue(currentSetting, viewValue);
                }
            } else if (viewValue) {
                this.report.settings.push(
                    this.createSettingObject(type, SettingDataType.Boolean, true)
                );
            }
        });
    }

    private getFieldKey(type: TermGroup_ReportSettingType): string {
        switch (type) {
            case TermGroup_ReportSettingType.ProjectOverviewExtendedInfo:
                return "projectOverviewExtendedInfo";
            case TermGroup_ReportSettingType.StockInventoryExcludeZeroQuantity:
                return "stockInventoryExcludeZeroQuantity";
            case TermGroup_ReportSettingType.ExcludeItemsWithZeroQuantityForSpecificDate:
                return "excludeItemsWithZeroQuantityForSpecificDate";
            case TermGroup_ReportSettingType.HidePriceAndRowSum:
                return "hidePriceAndRowSum";
            case TermGroup_ReportSettingType.GroupedInvoiceByTaxDeduction:
                return "groupedInvoiceByTaxDeduction";
            case TermGroup_ReportSettingType.GroupedOfferByTaxDeduction:
                return "groupedOfferByTaxDeduction";
            default:
                return "";
        }
    }



    private createSettingObject(type: TermGroup_ReportSettingType, dataType:SettingDataType, value:any):any {
        let object = {
            dataTypeId:dataType,
            type: type,
            value: null,
            intData: null,
            boolData: false,
            strData: null,    
            reportId: this.report.reportId,
            reportSettingId: 0
        }

        this.setSettingValue(object, value);

        return object;
    }

    private setSettingValue(settingObject:IReportSettingDTO, value: any) {
        if (settingObject.dataTypeId === SettingDataType.Boolean) {
            settingObject.boolData = value;
        }
        else if (settingObject.dataTypeId === SettingDataType.Integer) {
            settingObject.intData = value;
        }
        else if (settingObject.dataTypeId === SettingDataType.String) {
            settingObject.strData = value;
        }
    }

    private createNewReport() {
        this.report = <IReportDTO>{
            exportType: TermGroup_ReportExportType.Pdf,
            module: this.module
        };

        this.evaluateSelectedReportTemplate();
        this.evaluateExportFileType(this.report.exportType);

        this.selectedRoles = _.filter(this.selectableRoles, r => r.id === CoreUtility.roleId);
        this.onRolesSelected();
    }

    private openReportGroup() {
        HtmlUtility.openInNewTab(this.$window, "/soe/economy/distribution/reports/reportgroupmapping/?report=" + this.report.reportId);
    }

    private save() {
        return this.progress.startSaveProgress(completion => {
            this.report.isNewGroupsAndHeaders = this.newGroupsHeadings;
            this.report.importCompanyId = this.selectedImportCompanyId;
            this.report.importReportId = this.selectedImportReportId;

            this.populateReportSettingsForSave()
            
            this.reportService.saveReport(this.report).then(result => {
                if (result.success) {
                    this.reportId = result.integerValue;
                    this.messagingService.publish(Constants.EVENT_RELOAD_REPORT_SETTINGS, null);
                    completion.completed(null, { needReload: true });
                } else {
                    completion.failed(result.errorMessage);
                }
            }, (error) => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            if (this.isModal)
                this.closeModal(true);
            else if (data.needReload)
                this.onLoadData();
        });
    }

    private delete() {
        return this.progress.startDeleteProgress(completion => {
            this.reportService.deleteReport(this.report.reportId).then(
                () => {
                    completion.completed({});
                    if (this.isModal) {
                        this.closeModal(true);
                    }
                    else {
                        this.closeMe(true);
                    }
                },
                (error) => {
                    completion.failed(error.message);
                });
        });
    }

    public downloadSysTemplate() {
        if (this.report.reportTemplateId !== 0)
            HtmlUtility.openInSameTab(this.$window, "/soe/common/distribution/systemplates/edit/download/?systemplate=" + this.report.reportTemplateId);
    }

    public downloadUserTemplate() {
        if (this.report.reportTemplateId !== 0)
            HtmlUtility.openInSameTab(this.$window, "/soe/common/distribution/templates/edit/download/?template=" + this.report.reportTemplateId);
    }

    private onUserReportTemplateChanged(newTemplate) {
        this.report.standard = false;
        this.report.reportTemplateId = newTemplate.reportTemplateId;

        if (newTemplate) {
            this.selectedSysReportTemplate = undefined;
        }
        this.setNumberAndNameIfNeeded(() => this.userReportTemplates.find(t => t.reportTemplateId === newTemplate.reportTemplateId))
        this.loadExportTypes(0, newTemplate.reportTemplateId);

        this.dirtyHandler.setDirty();
    }

    private onSysReportTemplateChanged(newTemplate) {
        this.report.standard = true;
        this.report.reportTemplateId = newTemplate.sysReportTemplateId;

        if (newTemplate) {
            this.selectedUserReportTemplate = undefined;
        }
        this.setNumberAndNameIfNeeded(() => this.sysReportTemplates.find(t => t.sysReportTemplateId === newTemplate.sysReportTemplateId))
        this.loadExportTypes(newTemplate.sysReportTemplateId, 0);

        this.dirtyHandler.setDirty();
    }

    private onCompanyChanged(item) {
        this.selectedImportCompanyId = item;

        if (item > 0) {
            return this.progress.startLoadingProgress([
                () => this.reportService.GetReportsByTemplateTypeDict(this.selectedImportCompanyId, this.report.sysReportTemplateTypeId, true, false, true, false).then(x => {
                    this.sysCompanyReports = [];
                    _.forEach(x, (y: any) => {
                        this.sysCompanyReports.push({ id: y.id, name: y.name })
                    });
                })
            ]);

        }
    }

    private onSysCompanyReportIdChanged(item) {
        this.selectedImportReportId = item;
    }

    private setNumberAndNameIfNeeded(getDefaultNameForTemplate: () => { reportNr: number, name: string, description: string }) {
        if (!this.report.reportNr)
            this.report.reportNr = (getDefaultNameForTemplate() || { reportNr: null }).reportNr;
        if (!this.report.name)
            this.report.name = (getDefaultNameForTemplate() || { name: '' }).name;
        if (!this.report.description)
            this.report.description = (getDefaultNameForTemplate() || { description: '' }).description;
        if (this.report.name && this.report.name.length > 100)
            this.report.name = this.report.name.substring(0, 100);
    }

    private evaluateSelectedReportTemplate() {
        if (this.selectedUserReportTemplate) this.onUserReportTemplateChanged(this.selectedUserReportTemplate);
        if (this.selectedSysReportTemplate) this.onSysReportTemplateChanged(this.selectedSysReportTemplate);
    }

    private evaluateExportFileType(exportType: TermGroup_ReportExportType) {
        this.visibleOptionalFields["exportFileType"] = exportType === TermGroup_ReportExportType.File;

        if (!this.visibleOptionalFields["exportFileType"]) {
            this.report.exportFileType = TermGroup_ReportExportFileType.Unknown;
        }
    }

    private onRolesSelected() {
        this.report.roleIds = this.selectedRoles.map(r => r.id);
        this.dirtyHandler.setDirty();
    }

    private closeModal(modified: boolean) {
        if (this.isModal) {
            if (this.reportId) {
                this.modal.close({ modified: modified, id: this.reportId });
            } else {
                this.modal.dismiss();
            }
        }
    }

    private mapSelectedRoles() {
        this.selectedRoles = [];
        if (this.report.roleIds) {
            this.report.roleIds.forEach(id => {
                let role = this.selectableRoles.find(r => r.id === id);
                if (role)
                    this.selectedRoles.push(role);
            });
        }
    }

    private evaluateOptionalProperties() {
        this.evaluateExportFileType(this.report.exportType);
        this.visibleOptionalFields["includeAllHistoricalData"] = this.isConsideredResultReport();
        this.visibleOptionalFields["includeBudget"] = this.isConsideredResultReport();
        this.visibleOptionalFields["noOfYearsBackinPreviousYear"] = this.isConsideredResultReport();
        this.visibleOptionalFields["nrOfDecimals"] = this.isConsideredAccountingReport();
        this.visibleOptionalFields["showRowsByAccount"] = this.isConsideredAccountingReport();
        this.visibleOptionalFields["getDetailedInformation"] = this.isConsideredDetailedInformationReport();
        this.visibleOptionalFields["showInAccountingReports"] = this.isConsideredAccountingReport();
        this.visibleOptionalFields["connectReportGroups"] = this.isConsideredConnectReportGroups(); 

        if (this.report.sysReportTemplateTypeId == SoeReportTemplateType.ProjectTransactionsReport) {
            this.showSettings = true;
            this.visibleOptionalFields["projectOverviewExtendedInfo"] = true;
        } else if (this.report.sysReportTemplateTypeId == SoeReportTemplateType.StockInventoryReport) {
            this.showSettings = true;
            this.visibleOptionalFields["stockInventoryExcludeZeroQuantity"] = true;
        } else if (this.report.sysReportTemplateTypeId == SoeReportTemplateType.StockSaldoListReport) {
            this.showSettings = true;
            this.visibleOptionalFields["excludeItemsWithZeroQuantityForSpecificDate"] = true;
        } else if (this.report.sysReportTemplateTypeId == SoeReportTemplateType.BillingOffer) {
            this.showSettings = true;
            this.visibleOptionalFields["hidePriceAndRowSum"] = true;
            this.visibleOptionalFields["groupedOfferByTaxDeduction"] = true;
        } else if (this.report.sysReportTemplateTypeId == SoeReportTemplateType.BillingInvoice) {
            this.showSettings = true;
            this.visibleOptionalFields["groupedInvoiceByTaxDeduction"] = true;
        }
    }

    private isConsideredResultReport() {
        return this.matchAnySysReportTemplateType(
            SoeReportTemplateType.ResultReport,
            SoeReportTemplateType.ResultReportV2);
    }

    private isConsideredConnectReportGroups() {
        const test = this.matchAnySysReportTemplateType(
            SoeReportTemplateType.ResultReportV2);
        return test;
    }

    private isConsideredDetailedInformationReport() {
        return this.matchAnySysReportTemplateType(
            SoeReportTemplateType.ResultReport,
            SoeReportTemplateType.BalanceReport,
            SoeReportTemplateType.PayrollAccountingReport,
            SoeReportTemplateType.PayrollVacationAccountingReport,
            SoeReportTemplateType.TimeEmployeeSchedule,
            SoeReportTemplateType.ResultReportV2,
            SoeReportTemplateType.BillingInvoice,
            SoeReportTemplateType.BillingInvoiceInterest,
            SoeReportTemplateType.BillingInvoiceReminder,
            SoeReportTemplateType.TimeMonthlyReport,
            SoeReportTemplateType.VerticalTimeTrackerAnalysis);
    }

    private isConsideredAccountingReport() {
        return this.matchAnySysReportTemplateType(
            SoeReportTemplateType.ResultReport,
            SoeReportTemplateType.BalanceReport);
    }

    private matchAnySysReportTemplateType(...types: SoeReportTemplateType[]): boolean {
        return this.report.sysReportTemplateTypeId
            && types.some(v => v === <SoeReportTemplateType>this.report.sysReportTemplateTypeId);
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (!this.report.reportNr)
                mandatoryFieldKeys.push("common.report.report.reportNumber");

            if (!this.report.name)
                mandatoryFieldKeys.push("common.name");

            if (this.selectedRoles.length === 0)
                mandatoryFieldKeys.push("common.report.report.roles");
        });
    }
}