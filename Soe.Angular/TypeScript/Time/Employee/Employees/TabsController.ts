import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Constants } from "../../../Util/Constants";
import { Guid } from "../../../Util/StringUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { IEmployeeService } from "../EmployeeService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CreateFromEmployeeTemplateMode, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { RatioController } from "./RatioController";
import { MassUpdateEmployeeGridController } from "../MassUpdateEmployeeFields/GridController";
import { CreateFromTemplateDialogController } from "./Dialogs/CreateFromTemplate/CreateFromTemplateDialogController";
import { AddDocumentToAttestFlowController } from "../../../Common/Dialogs/AddDocumentToAttestFlow/AddDocumentToAttestFlowController";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class TabsController implements ICompositionTabsController {

    // Terms
    private terms: any;

    // Permission
    private ratiosPermission = false;
    private employmentPermission = false;
    private massUpdatePermission = false;

    // Company settings
    private useSimplifiedEmployeeRegistration = false;
    private hasEmployeeTemplates = false;
    private searchMode = false;

    private modalInstance: any;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModal,
        private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private employeeService: IEmployeeService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private $q: ng.IQService,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService) {

        this.modalInstance = $uibModal;

        const part = "time.employee.employee.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.employeeId)
            .onGetRowEditName(row => '{0} - {1}'.format(row.employeeNr, row.name))
            .onSetupTabs((tabHandler) => { this.setupTabs(tabHandler); })
            .onEdit(row => this.edit(row))
            .initialize("", part + "employees", part + "newemployee");

        // Subscribe to new event
        this.messagingService.subscribe(Constants.EVENT_SEARCH_EMPLOYEE, () => {
            this.searchMode = true;
            this.add(false);
        });
    }

    private $onInit() {
        const employeeId = HtmlUtility.getQueryParameterByName(this.$window.location, "employeeId")
        if (employeeId) {
            const employeeNr = HtmlUtility.getQueryParameterByName(this.$window.location, "employeenr");
            const row = { employeeId: employeeId, employeeNr: employeeNr };
            this.$timeout(() => { this.edit(row) });
        }
    }

    private setupTabs(tabHandler: ITabHandler) {
        tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
        tabHandler.enableAddTab(() => this.add(true));
        tabHandler.enableRemoveAll();

        this.$q.all([
            this.loadTerms(),
            this.loadReadOnlyPermissions(),
            this.loadCompanySettings()
        ]).then(() => {
            if (this.employmentPermission && this.ratiosPermission) {
                const keys: string[] = [];
                keys.push("time.employee.employee.ratios");
                this.translationService.translateMany(keys).then((terms) => {
                    tabHandler.addNewTab(terms["time.employee.employee.ratios"], new Guid(), RatioController, this.urlHelperService.getViewUrl("ratio.html"), null, false);
                });
            }
            if (this.massUpdatePermission) {
                const keys: string[] = [];
                keys.push("time.employee.massupdate");
                this.translationService.translateMany(keys).then((terms) => {
                    tabHandler.addNewTab(terms["time.employee.massupdate"], new Guid(), MassUpdateEmployeeGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), false, false, false);
                });
            }
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = ["time.employee.employee.ratios"];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [];
        featureIds.push(Feature.Time_Employee_Employees_Ratios);
        featureIds.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment);
        featureIds.push(Feature.Time_Employee_MassUpdateEmployeeFields);

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.ratiosPermission = x[Feature.Time_Employee_Employees_Ratios];
            this.employmentPermission = x[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment];
            this.massUpdatePermission = x[Feature.Time_Employee_MassUpdateEmployeeFields];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseSimplifiedEmployeeRegistration);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useSimplifiedEmployeeRegistration = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseSimplifiedEmployeeRegistration);
            if (this.useSimplifiedEmployeeRegistration)
                this.loadHasEmployeeTemplates();
        });
    }

    private loadHasEmployeeTemplates(): ng.IPromise<any> {
        return this.employeeService.hasEmployeeTemplates().then(x => {
            this.hasEmployeeTemplates = x;
        });
    }

    private edit(row: any) {
        this.tabs.addEditTab(row, EditController, { showInactive: !row.isActive });
    }

    private add(isManuallyNew: boolean) {
        if (isManuallyNew) {
            this.validateAdd().then(passed => {
                if (passed)
                    this.openAddTab(isManuallyNew);
            });
        } else {
            this.openAddTab(isManuallyNew);
        }
    }

    private validateAdd(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        this.employeeService.getEmployeeLicenseInfo().then(x => {
            const nrOfEmployeesOnLicense: number = x.field1;
            const maxNrOfEmployeesOnLicense: number = x.field2;
            if (nrOfEmployeesOnLicense >= maxNrOfEmployeesOnLicense) {
                const keys: string[] = [
                    "core.warning",
                    "time.employee.employee.licensewarning"];

                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["core.warning"], terms["time.employee.employee.licensewarning"].format(nrOfEmployeesOnLicense.toString(), maxNrOfEmployeesOnLicense.toString()), SOEMessageBoxImage.Forbidden);
                });

                this.tabs.setActiveTabIndex(0);
                deferral.resolve(false);
            } else {
                deferral.resolve(true);
            }
        });

        return deferral.promise;
    }

    private openAddTab(isManuallyNew: boolean) {
        if (isManuallyNew && this.useSimplifiedEmployeeRegistration && this.hasEmployeeTemplates) {
            // Open template dialog instead od normal edit page
            this.openCreateFromTemplate();
            // Reactivate grid
            this.tabs.setActiveTabIndex(0);
        } else {
            this.openEditPage(isManuallyNew);
        }
    }

    private openEditPage(isManuallyNew: boolean) {
        const parameters = {
            isManuallyNew: isManuallyNew,
            searchMode: false
        };

        if (this.searchMode) {
            this.searchMode = false;
            parameters.searchMode = true;
        }

        return this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"), parameters);
    }

    private openCreateFromTemplate() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Dialogs/CreateFromTemplate/CreateFromTemplateDialog.html"),
            controller: CreateFromTemplateDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                mode: () => { return CreateFromEmployeeTemplateMode.NewEmployee },
                createdWithEmployeeTemplateId: () => { return 0 },
                employeeId: () => { return 0 },
                numberAndName: () => { return '' }
            }
        }
        this.$uibModal.open(options).result.then(result => {
            if (result) {
                if (result.openEditPage)
                    this.openEditPage(true);
                else {
                    this.$scope.$broadcast(Constants.EVENT_RELOAD_GRID, null);

                    if (result.initSigning && result.userId && result.dataStorageId) {
                        this.initSigningDocument(result.userId, result.dataStorageId);
                    }
                }
            }
        }, (reason) => {
            // Cancelled
        });
    }

    private initSigningDocument(userId: number, recordId: number) {
        this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/AddDocumentToAttestFlow/Views/addDocumentToAttestFlow.html"),
            controller: AddDocumentToAttestFlowController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                recordId: () => { return recordId },
                endUserId: () => { return userId }
            }
        });
    }

    public tabs: ITabHandler;
}