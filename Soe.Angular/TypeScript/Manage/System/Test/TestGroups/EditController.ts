import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, TermGroup, SoeEntityState } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IReportService } from "../../../../Core/Services/ReportService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary } from "../../../../Util/Enumerations";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IUserService } from "../../../User/UserService";
import { ISystemService } from "../../SystemService";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { SysScheduledJobDTO } from "../../../../Common/Models/SysJobDTO";
import { CapabilitySettingDialogController } from "./Dialogs/CapabilitySettingDialogController";
import { TestCaseDialogController } from "./Dialogs/TestCaseDialogController";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { TestGroupSettingDialogController } from "./Dialogs/TestGroupSettingDialogController";
import { EditRecurrenceIntervalController } from "../../../../Common/Dialogs/EditRecurrenceInterval/EditRecurrenceIntervalController";
import { TestCaseSettingDTO, TestCaseGroupMappingDTO, TestCaseGroupDTO, TestCaseDTO, TestCaseSettingType, TestCaseType, TestCaseSettingTypeDTO } from "../Util"
import { Guid } from "../../../../Util/StringUtility";
import { IHttpService } from "../../../../Core/Services/HttpService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {


    private seleniumBrowserTypes: ISmallGenericType[];


    private testGroup: TestCaseGroupDTO;
    private testGroupId: number;
    private testGroupType: TestCaseType = TestCaseType.Selenium;
    private testTrackingGuid: Guid;
    private testTrackingResult: string;
    private testCases: any[];

    private browserStackUrl: string = undefined;
    private capabilities: any = {};
    private isNewGroup: boolean;
    private recurrenceInterval: string = null;//"{0} {1} {2} {3} {4}".format(Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED);
;

    private job: SysScheduledJobDTO;

    private testCaseButtons = new Array<ToolBarButtonGroup>();
    private testCaseGrid: ISoeGridOptionsAg;

    private testCaseGroupResultsGrid: ISoeGridOptionsAg;
    private testCaseResultsGrid: ISoeGridOptionsAg;


    private terms: { [index: string]: string; };
    private testTypes: ISmallGenericType[] = [];
    private settingTypes: TestCaseSettingTypeDTO[];


    //@ngInject
    constructor(
        private $uibModal,
        private $sce,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private translationService: ITranslationService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.testGroupId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.testCaseGrid = SoeGridOptionsAg.create("TestCases", this.$timeout);
        this.testCaseGrid.setMinRowsToShow(25);

        this.testCaseGroupResultsGrid = SoeGridOptionsAg.create("TestCaseGroupResults", this.$timeout);
        this.testCaseGroupResultsGrid.setMinRowsToShow(15);

        this.testCaseResultsGrid = SoeGridOptionsAg.create("TestCaseResults", this.$timeout);
        this.testCaseResultsGrid.setMinRowsToShow(15);

        this.flowHandler.start([{ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_System].readPermission;
        this.modifyPermission = response[Feature.Manage_System].modifyPermission;
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadSettingTypes(),
            this.loadTestTypes(),
            this.loadSeleniumBrowser(),
            this.loadTestGroup(),
        ]).then(() => {
            this.setupTestCaseGridColumns();
            this.setupTestCaseGroupResultsGrid();
            this.setupTestCaseResultsGrid();
            this.setSettings();
        })
    }

    //#region "Init"
    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "economy.supplier.invoice.changecompany", IconLibrary.FontAwesome, "fa-play", () => {
            this.runTests();
        },
        null,
        null
        )));
    }
    
    private runTests() {
        this.systemService.runTestCaseGroup(this.testGroupId);
    }

    private getTestCaseGroupResults() {
        this.systemService.getTestCaseGroupResults(this.testGroupId).then(data => {
            this.testCaseGroupResultsGrid.setData(data);
        })
    }

    private getTestCaseResults() {
        this.systemService.getTestCaseResultsByTestCaseGroupId(this.testGroupId).then(data => {
            this.testCaseResultsGrid.setData(data);
        })
    }

    private setupTestCaseGridColumns() {
        this.testCaseButtons.push(ToolBarUtility.createGroup(new ToolBarButton("", "common.dashboard.reload", IconLibrary.FontAwesome, "fa-sync", () => {
            this.loadTestCases()
        }, () => {
            return this.isNew;
        })));

        this.testCaseButtons.push(ToolBarUtility.createGroup(new ToolBarButton("", "common.dashboard.addtest", IconLibrary.FontAwesome, "fa-plus", () => {
            this.openTestCaseDialog()
        })));

        this.testCaseGrid.enableRowSelection = false;
        this.testCaseGrid.addColumnNumber("testCaseId", this.terms["common.number"], null);
        this.testCaseGrid.addColumnText("name", this.terms["common.name"], null);
        this.testCaseGrid.addColumnText("description", this.terms["common.name"], null);
        this.testCaseGrid.addColumnIcon(null, "", null, { icon: "iconEdit fal fa-trash", onClick: this.removeTestCase.bind(this) });
        if (this.testGroupType === 1) {
            this.testCaseGrid.addColumnIcon(null, "", null, { icon: "iconEdit fal fa-play", onClick: this.viewBrowserStackResult.bind(this) })
        }

        this.testCaseGrid.finalizeInitGrid();
    }

    private setupTestCaseGroupResultsGrid() {
        this.testCaseGroupResultsGrid.addColumnNumber("testCaseGroupResultId", "testCaseGroupResultId", null);
        this.testCaseGroupResultsGrid.addColumnNumber("testCaseGroupId", "testCaseGroupId", null);
        this.testCaseGroupResultsGrid.addColumnNumber("successPercent", "successPercent", null);
        this.testCaseGroupResultsGrid.addColumnDateTime("requestStarted", "requestStarted", null);
        this.testCaseGroupResultsGrid.addColumnDateTime("requestEnded", "requestEnded", null);
        this.testCaseGroupResultsGrid.addColumnDateTime("started", "started", null);
        this.testCaseGroupResultsGrid.addColumnDateTime("ended", "ended", null);
        this.testCaseGroupResultsGrid.addColumnTimeSpan("duration", "duration (ms)", null);
        this.testCaseGroupResultsGrid.addColumnText("message", "message", null);
        this.testCaseGroupResultsGrid.finalizeInitGrid();
    }

    private setupTestCaseResultsGrid() {
        this.testCaseResultsGrid.addColumnNumber("testCaseResultId", "testCaseResultId", null);
        this.testCaseResultsGrid.addColumnNumber("testCaseId", "testCaseId", null);
        this.testCaseResultsGrid.addColumnNumber("testCaseGroupId", "testCaseGroupId", null);
        this.testCaseResultsGrid.addColumnNumber("testCaseType", "testCaseType", null);
        this.testCaseResultsGrid.addColumnBool("success", "success", null, { enableEdit: false });
        this.testCaseResultsGrid.addColumnDateTime("requestStarted", "requestStarted", null);
        this.testCaseResultsGrid.addColumnDateTime("requestEnded", "requestEnded", null);
        this.testCaseResultsGrid.addColumnDateTime("started", "started", null);
        this.testCaseResultsGrid.addColumnDateTime("ended", "ended", null);
        this.testCaseResultsGrid.addColumnTimeSpan("duration", "duration (ms)", null);
        this.testCaseResultsGrid.addColumnText("message", "message", null);
        this.testCaseResultsGrid.addColumnText("machine", "machine", null);
        this.testCaseResultsGrid.finalizeInitGrid();
    }
    //#endregion

    //#region "API calls"

    protected showDetailedResult(row) {
        if (row.externalInformation)
            return true;
        else
            return false;
    }

    private loadTestTypes() {
        return this.coreService.getTermGroupContent(TermGroup.TestCaseType, true, false, true).then(x => {
            this.testTypes = x;
        });
    }

    private loadSeleniumBrowser(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SeleniumBrowser, true, false, true).then(x => {
            this.seleniumBrowserTypes = x;
        });
    }

    private loadSettingTypes() {
        //return this.coreService.getTermGroupContent(TermGroup.TestCaseSettingType, true, false, true).then(x => {
        //    this.settingTypes = x;
        //});
        return this.systemService.getTestCaseSettings().then((data) => {
            this.settingTypes = data;
        })
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.dashboard.reload",
            "common.description",
            "common.name",
            "common.number",
            "core.yes",
            "core.no",
            "manage.system.test.setting_seleniumtype",
            "manage.system.test.setting_seleniumbrowser",
            "manage.system.test.setting_starturl",
            "manage.system.test.setting_domain",
            "manage.system.test.setting_password",
            "manage.system.test.setting_username",
            "manage.system.test.setting_capabilities",
            "manage.system.test.testcasetype_selenium",
            "manage.system.test.testcasetype_unit"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadTestGroup() {
        if (this.testGroupId && this.testGroupId > 0) {
            this.isNewGroup = false;
            return this.systemService.getTestCaseGroup(this.testGroupId).then(data => {
                this.testGroup = data;
                this.testGroup.executeTime = this.testGroup.executeTime ? CalendarUtility.convertToDate(this.testGroup.executeTime) : undefined;
            }).then(() => {
                this.loadTestCases();
                this.setSettings();
            })
        }
        else {
            this.isNewGroup = true;
            this.testGroup = new TestCaseGroupDTO() //this.testGroupDTO();
        }
        this.testGroup.cron = this.testGroup.cron || this.recurrenceInterval;
    }

    private loadTestCases() {
        var promises: ng.IPromise<any>[] = [];

        for (let entry of this.testGroup.testCaseGroupMappingDTOs) {
            if (entry.state === 0) {
                promises.push(this.systemService.getTestCase(entry.testCaseId))
            }
        }

        Promise.all(promises).then(data => {
            this.testCaseGrid.setData(data);
        })
    }

    private save() {
        this.updateCapabilities();
        this.logAsJson(this.testGroup)
        this.progress.startSaveProgress((completion) => {
            this.systemService.saveTestCaseGroup(this.testGroup).then((result) => {
                if (result.success) {
                    this.testGroupId = result.integerValue;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.testGroup);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadTestGroup();
            }, error => {

            });

    }

    private delete() {
        if (this.testGroup.testCaseGroupId > 0) {
            var modal = this.notificationService.showDialogEx(this.terms["core.warning"], null, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val) {
                    this.testGroup.state = 2;
                    this.save();
                }
            });
        }
        this.messagingService.publish(Constants.EVENT_CLOSE_TAB, { guid: this.guid });
    }
    //#endregion

    //#region "Help methods"
    private setSettings() {
        if (!this.settingTypes) return
        _.forEach(this.testGroup.testCaseSettingDTOs, (setting) => {
            if (setting.state == SoeEntityState.Active) {
                const settingType = this.settingTypes.find(i => setting.type === i.type);
                setting.settingType = settingType;
                setting["name"] = settingType.name;

                if (settingType.isInt) {
                    setting["icon"] = "fal fa-hashtag"
                    setting["value"] = setting.intValue
                }
                else if (settingType.isBool) {
                    setting["icon"] = "fal fa-check-square"
                    setting["value"] = setting.boolValue == true ? this.terms["core.yes"] : this.terms["core.no"];
                }
                else if (settingType.isSelect) {
                    setting["icon"] = "fal fa-list"
                    const alt = settingType.alternatives.find(a => a.id === setting.intValue);
                    if (alt)
                        setting["value"] = alt.name;
                    else
                        setting["value"] = setting.intValue
                }
                else if (settingType.isString) {
                    setting["icon"] = "fal fa-text"
                    setting["value"] = setting.stringValue
                }
                else if (settingType.isJson) {
                    setting["icon"] = "fal fa-code"
                    setting["value"] = "<JSON>"
                }
            }
        });
    }

    private settingFilter(item: TestCaseSettingDTO) {
        return (item.state === 0)
    }

    private deleteTestGroupSetting(setting: any) {
        setting.state = 2;
        this.dirtyHandler.setDirty();
    }

    private updateCapabilities() {
        var value = this.capabilitiesJSON();
        var setting: TestCaseSettingDTO = this.testGroup.testCaseSettingDTOs.find(setting => setting.type === 7 && setting.state === 0);
        if (setting) {
            setting.stringValue = value;
        }
        else {
            setting = new TestCaseSettingDTO(this.testGroupId, undefined, TestCaseSettingType.Capabilities);
            setting.stringValue = this.capabilitiesJSON();
            if (setting.stringValue) this.testGroup.testCaseSettingDTOs.push(setting);
        }
    }

    private loadCapabilities(setting: any) {
        if (setting && setting.stringValue) {
            this.capabilities = JSON.parse(setting.stringValue)
        }
    }

    private deleteCapability(key) {
        delete this.capabilities[key]
        this.dirtyHandler.setDirty();
    }

    private capabilitiesJSON() {
        var value = JSON.stringify(this.capabilities)
        if (value === "" || value === "{}" || !value) return undefined
        else return value
    }

    private logAsJson(obj: any) {
        console.log(JSON.stringify(obj))
    }

    //#endregion

    //#region "Dialogs"
    private openTestCaseDialog() {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/System/Test/TestGroups/Dialogs/Views/TestCaseDialog.html"),
            controller: TestCaseDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.testCaseIds) {
                this.addTestCases(result.testCaseIds)
                this.dirtyHandler.setDirty();
            }
        });
    }

    private editTestGroupSetting(setting: TestCaseSettingDTO) {
        var result: any;
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/System/Test/TestGroups/Dialogs/Views/TestGroupSettingDialog.html"),
            controller: TestGroupSettingDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                settings: () => { return this.testGroup.testCaseSettingDTOs },
                settingTypes: () => { return this.settingTypes },
                setting: () => { return setting },
                testCaseGroupId: () => { return this.testGroupId },
            }
        }
        this.$uibModal.open(options).result.then((result: { setting: TestCaseSettingDTO }) => {
            if (result && result.setting) {
                if (!result.setting.testCaseSettingId || !setting) {
                    this.testGroup.testCaseSettingDTOs.push(result.setting);
                }
                else {
                    var existing = _.find(this.testGroup.testCaseSettingDTOs, { 'testCaseSettingId': result.setting.testCaseSettingId });
                    if (existing)
                        existing = result.setting;
                }
                this.setSettings();
                this.dirtyHandler.setDirty();
            }
        });
    }

    private editRecurrence() {
        var result: any;
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/EditRecurrenceInterval/EditRecurrenceInterval.html"),
            controller: EditRecurrenceIntervalController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                singleSelectTime: () => { return false },
                interval: () => { return this.testGroup.cron }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.interval) {
                this.testGroup.cron = result.interval;
                this.dirtyHandler.setDirty();
            }
        });
    }

    private editCapability(key: string) {
        var key: string;
        var result: any;
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/System/Test/TestGroups/Dialogs/Views/CapabilitySettingDialog.html"),
            controller: CapabilitySettingDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                capabilities: () => { return this.capabilities },
                key: () => { return key },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                //this.capabilities
                this.dirtyHandler.setDirty();
            }
        });
    }


    //#endregion

    //#region "TestCases"
    private addTestCases(ids: number[]) {
        for (let id of ids) {
            const found = this.testGroup.testCaseGroupMappingDTOs.some(el => el.testCaseId === id && el.state === 0);
            if (!found) {
                this.testGroup.testCaseGroupMappingDTOs.push(new TestCaseGroupMappingDTO(this.testGroupId, id));
            }
        }
        this.loadTestCases();
    }

    private removeTestCase(row: TestCaseDTO) {
        this.testGroup.testCaseGroupMappingDTOs.forEach((item, index) => {
            if (row.testCaseId === item.testCaseId && item.state === SoeEntityState.Active) {
                item.state = SoeEntityState.Deleted;
                this.loadTestCases();
                this.dirtyHandler.setDirty();
                return;
            }
        })
    }

    private viewBrowserStackResult(row) {
        //this.browserStackUrl = row.browserStackUrl;
        this.browserStackUrl = row.externalInformation;
    }

    private getBrowserStackUrl() {
        return this.$sce.trustAsResourceUrl(this.browserStackUrl);
    }

    //#endregion


}