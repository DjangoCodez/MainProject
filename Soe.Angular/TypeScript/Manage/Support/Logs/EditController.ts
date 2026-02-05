import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ISupportService } from "../SupportService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature } from "../../../Util/CommonEnumerations";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";
import { HtmlUtility } from "../../../Util/HtmlUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private sysLogId: number;
    private sysLog: any;

    // Flags
    private taskWatchAccordionInitiallyOpen: boolean = false;
    private userInfoAccordionInitiallyOpen: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private supportService: ISupportService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private $window,
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
        this.sysLogId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Support_Logs_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Support_Logs_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Support_Logs_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.download", "common.download", IconLibrary.FontAwesome, "fal fa-download", () => {
            this.downloadSysLog();
        }, null, () => {
            return (!this.sysLog)
        })));
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.sysLogId) {
            return this.load();
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

    private load(): ng.IPromise<any> {
        return this.supportService.getSysLog(this.sysLogId).then(x => {
            this.sysLog = x;
            this.sysLog.date = new Date(this.sysLog.date);
            this.sysLog.dateStr = CalendarUtility.toFormattedDateAndTime(this.sysLog.date)
            this.taskWatchAccordionInitiallyOpen = this.sysLog.taskWatchLogId > 0;
            this.userInfoAccordionInitiallyOpen = this.sysLog.licenseId > 0 || this.sysLog.actorCompanyId > 0 || this.sysLog.roleId > 0;
            this.dirtyHandler.clean();
        });
    }

    // ACTIONS

    private downloadSysLog() {
        if (this.sysLog)
            HtmlUtility.openInSameTab(this.$window, "/soe/manage/support/logs/edit/download/?sysLogId=" + this.sysLog.sysLogId);
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }
}