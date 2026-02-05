import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ISystemService } from "../SystemService";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SelectDateController } from "../../../Common/Dialogs/SelectDate/SelectDateController";
import { CoreUtility } from "../../../Util/CoreUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { Constants } from "../../../Util/Constants";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private modalInstance: any;
    public files: any[] = [];

    //@ngInject
    constructor(
        private systemService: ISystemService,
        private $q: ng.IQService,
        private $uibModal,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Manage.System.CommodityCodes", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_System].readPermission;
        this.modifyPermission = response[Feature.Manage_System].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }
   

    public setupGrid() {
        this.gridAg.options.enableRowSelection = false;

        const keys: string[] = [
            "manage.system.commoditycode.code",
            "manage.system.commoditycode.description",
            "manage.system.commoditycode.useotherqualifier",
            "manage.system.commoditycode.startdate",
            "manage.system.commoditycode.enddate",
            "manage.system.commoditycode.import",
            "manage.system.commoditycode.commoditycodes",
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("code", terms["manage.system.commoditycode.code"], 15);
            this.gridAg.addColumnText("text", terms["manage.system.commoditycode.description"], 30);
            this.gridAg.addColumnBool("useOtherQuantity", terms["manage.system.commoditycode.useotherqualifier"], 15, false);
            this.gridAg.addColumnDate("startDate", terms["manage.system.commoditycode.startdate"], 15);
            this.gridAg.addColumnDate("endDate", terms["manage.system.commoditycode.enddate"], 15);
            
            this.gridAg.finalizeInitGrid("manage.system.commodityCode.commoditycodes", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData(), false);
        var groupAdd = ToolBarUtility.createGroup(new ToolBarButton("", "manage.system.commoditycode.import", IconLibrary.FontAwesome, "fa-upload", () => this.uploadFiles()));
        this.toolbar.addButtonGroup(groupAdd);
    }

    private uploadFiles() {

        const keys: string[] = [
            "manage.system.commoditycode.fileuploadnotsuccess",
            "manage.system.commoditycode.fileuploadsuccess",
            "core.succeeded",
            "core.error",
            "core.fileupload.choosefiletoimport"
        ];

        return this.translationService.translateMany(keys).then((terms) => {

            this.showSelectDateDialog(this.getStartingDayOfYear()).then((selectedDate: Date) => {
                if (selectedDate) {

                    this.files = [];
                    const url = CoreUtility.apiPrefix + Constants.WEBAPI_CORE_FILES_UPLOAD_COMMODITYCODES + selectedDate.getFullYear();
                    const modal = this.notificationService.showFileUpload(url, terms["core.fileupload.choosefiletoimport"], true, true, true, true);
                    modal.result.then(res => {
                        let filesNotUploaded: string = "";
                        var allSuccess = true;
                        _.forEach(res.result, result => {
                            if (result.success)
                                this.files.push(result);
                            else {
                                allSuccess = false;
                                filesNotUploaded += filesNotUploaded.length > 0 ? "\n " + (result.stringValue + ":" + result.errorMessage) : (result.stringValue + ":" + result.errorMessage);
                            }
                        });


                        if (filesNotUploaded.length > 0)
                            this.notificationService.showDialog(terms["core.error"], terms["manage.system.commoditycode.fileuploadnotsuccess"] + "\n" + filesNotUploaded, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                        if (allSuccess) {
                            this.notificationService.showDialog(terms["core.succeeded"], terms["manage.system.commodityCode.fileuploadsuccess"], SOEMessageBoxImage.OK, SOEMessageBoxButtons.OK);
                            this.loadGridData();
                        }
                    }, error => {
                    });

                }
            });
        });
    }

    private getStartingDayOfYear(): Date {
        return new Date(new Date().getFullYear(),0,1);
    }

    private showSelectDateDialog(defaultDate: Date): ng.IPromise<Date> {
        const deferral = this.$q.defer<Date>();

        this.translationService.translate("common.customer.invoices.selectorder").then((term) => {
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectDate", "SelectDate.html"),
                controller: SelectDateController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    title: () => { return term },
                    defaultDate: () => { return defaultDate }
                }
            });

            modal.result.then(result => {
                if (result && result.selectedDate) {
                    deferral.resolve(result.selectedDate);
                }
                else {
                    deferral.resolve(undefined);
                }
            });
        });

        return deferral.promise;
    }

    // Load data
    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.systemService.getCommodityCodes(CoreUtility.languageId).then(data => {                
                this.setData(data);
            });
        }]);
    }
}
