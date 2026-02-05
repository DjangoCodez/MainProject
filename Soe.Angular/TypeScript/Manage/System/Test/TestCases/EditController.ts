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
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { ISystemService } from "../../SystemService";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { CapabilitySettingDialogController } from "../TestGroups/Dialogs/CapabilitySettingDialogController";
import { TestGroupSettingDialogController } from "../TestGroups/Dialogs/TestGroupSettingDialogController";
import { EditRecurrenceIntervalController } from "../../../../Common/Dialogs/EditRecurrenceInterval/EditRecurrenceIntervalController";
import { TestCaseSettingDTO, TestCaseDTO, TestCaseSettingType, TestCaseSettingTypeDTO } from "../Util"
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";


export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private seleniumBrowserTypes: ISmallGenericType[];
    private testCase: TestCaseDTO;
    private testCaseId: number;
    private name: string;
    private description: string;
    private loadingTestCases: boolean;
    private capabilities: any = {};
    private isNewCase: boolean;
    private recurrenceInterval: string = "{0} {1} {2} {3} {4}".format(Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED);

    private terms: { [index: string]: string; };
    private testTypes: ISmallGenericType[] = [];
    private settingTypes: TestCaseSettingTypeDTO[];

    private testCaseResultsGrid: ISoeGridOptionsAg;


    //@ngInject
    constructor(
        private $uibModal,
        private $timeout,
        private $sce,
        private $q: ng.IQService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
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

    //#region "Init"
    public onInit(parameters: any) {
        this.testCaseId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true }]);

        this.testCaseResultsGrid = SoeGridOptionsAg.create("TestCaseResults", this.$timeout);
        this.testCaseResultsGrid.setMinRowsToShow(15);
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
            this.loadTestCase(),
        ]).then(() => {
            this.setSettings();
            this.setupTestCaseResultsGrid();
        })
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
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

    private loadTestTypes() {
        return this.coreService.getTermGroupContent(TermGroup.TestCaseType, true, false, true).then(x => {
            this.testTypes = x;
        });
    }

    private loadSeleniumBrowser(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SeleniumBrowser, true, false, true).then(x => {
            this.seleniumBrowserTypes = x;
            console.log(x);
        });
    }

    private loadSettingTypes() {
        return this.systemService.getTestCaseSettings().then(x => {
            this.settingTypes = x;
        });
    }

    private getTestCaseResults() {
        this.systemService.getTestCaseResultsByTestCaseId(this.testCaseId).then(data => {
            this.testCaseResultsGrid.setData(data);
        })
    }

    //remove
    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.dashboard.reload",
            "common.description",
            "common.name",
            "common.number",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadTestCase() {
        if (this.testCaseId && this.testCaseId > 0) {
            this.isNewCase = false;
            return this.systemService.getTestCase(this.testCaseId).then(data => {
                console.log(data)
                this.testCase = data;
                this.name = data.name;
                this.description = data.description;
            }).then(() => {
                this.setSettings();
            })
        }
        else {
            this.isNewCase = true;
            this.testCase = new TestCaseDTO() //this.testGroupDTO();
        }
    }

    private save() {
        this.updateCapabilities();
        this.progress.startSaveProgress((completion) => {
            this.systemService.saveTestCase(this.testCase).then((result) => {
                if (result.success) {
                    this.testCaseId = result.integerValue;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.testCase);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        },
            this.guid).then(data => {
                this.dirtyHandler.clean();
                this.loadTestCase();
            }, error => {

            });
    }

    private delete() {
        if (this.testCase.testCaseId > 0) {
            var modal = this.notificationService.showDialogEx(this.terms["core.warning"], null, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val) {
                    this.testCase.state = 2;
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
        _.forEach(this.testCase.testCaseSettingDTOs, (setting) => {
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

    private settingFilter(item) {
        return (item.state === 0)
    }

    private deleteTestCaseSetting(setting: any) {
        setting.state = 2;
        this.dirtyHandler.setDirty();
    }

    private updateCapabilities() {
        var value = this.capabilitiesJSON();
        var setting: TestCaseSettingDTO = this.testCase.testCaseSettingDTOs.find(setting => setting.type === 7 && setting.state === 0);
        if (setting) {
            setting.stringValue = value;
        }
        else {
            setting = new TestCaseSettingDTO(undefined, this.testCaseId, TestCaseSettingType.Capabilities);
            setting.stringValue = this.capabilitiesJSON();
            if (setting.stringValue) this.testCase.testCaseSettingDTOs.push(setting);
        }
    }

    private loadCapabilities(setting: any) {
        if (setting && setting.stringValue) {
            console.log('he')
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

    //#endregion

    //#region "Dialogs"

    private editTestCaseSetting(setting: TestCaseSettingDTO) {
        var result: any;
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/System/Test/TestGroups/Dialogs/Views/TestGroupSettingDialog.html"),
            controller: TestGroupSettingDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                settings: () => { return this.testCase.testCaseSettingDTOs },
                settingTypes: () => { return this.settingTypes },
                setting: () => { return setting },
                testCaseGroupId: () => { return this.testCaseId },
            }
        }
        this.$uibModal.open(options).result.then((result: { setting: TestCaseSettingDTO }) => {
            if (result && result.setting) {
                if (!result.setting.testCaseSettingId || !setting) {
                    this.testCase.testCaseSettingDTOs.push(result.setting);
                }
                else {
                    var existing = _.find(this.testCase.testCaseSettingDTOs, { 'testCaseSettingId': result.setting.testCaseSettingId });
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
                interval: () => { return this.recurrenceInterval }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.interval) {
                this.recurrenceInterval = result.interval;
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

}