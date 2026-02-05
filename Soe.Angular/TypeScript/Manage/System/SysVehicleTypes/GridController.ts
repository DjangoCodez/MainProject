import { GridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { ISystemService } from "../SystemService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IActionResult } from "../../../Scripts/TypeLite.Net4";

export class GridController extends GridControllerBase {

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private systemService: ISystemService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants) {

        super("Manage.System.SysVehicleTypes", "manage.system.sysvehicletypes", Feature.Manage_System, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    protected setupCustomToolBar() {
        if (super.setupDefaultToolBar()) {
            // Import
            this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("", "manage.system.sysvehicletype.import", IconLibrary.FontAwesome, "fa-download", () => {
                // Open file dialog
                this.translationService.translate("manage.system.sysvehicletype.fileuploadtitle").then((term) => {
                    var url = CoreUtility.apiPrefix + Constants.WEBAPI_MANAGE_SYSTEM_SYS_VEHICLE_TYPE;
                    var modal = this.notificationService.showFileUpload(url, term, true, true, false, false, false);
                    modal.result.then(res => {
                        let result: IActionResult = res.result;
                        if (result.success) {
                            super.reloadData();
                        } else {
                            this.failedSave(result.errorMessage);
                        }
                    });
                });
            })));
        }
    }

    protected setupGrid() {

        // Columns
        var keys: string[] = [
            "core.filename",
            "manage.system.sysvehicletype.manufacturingyearshort",
            "manage.system.sysvehicletype.datefrom",
            "manage.system.sysvehicletype.created",
            "manage.system.sysvehicletype.createdby",
            "core.delete"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            super.addColumnText("filename", terms["core.filename"], null);
            super.addColumnNumber("manufacturingYear", terms["manage.system.sysvehicletype.manufacturingyearshort"], "15%");
            super.addColumnDate("dateFrom", terms["manage.system.sysvehicletype.datefrom"], "15%", false);
            super.addColumnDate("created", terms["manage.system.sysvehicletype.created"], "15%", false);
            super.addColumnText("createdBy", terms["manage.system.sysvehicletype.createdby"], "20%");
            super.addColumnDelete(terms["core.delete"]);
        });
    }

    public loadGridData() {
        this.systemService.getSysVehicleTypes().then((x) => {
            _.forEach(x, (y) => {
                y.dateFrom = CalendarUtility.toFormattedDate(y.dateFrom);
                y.created = CalendarUtility.toFormattedDate(y.created);
            });
            super.gridDataLoaded(x);
        });
    }

    protected initDeleteRow(row) {
        var id: number = row['sysVehicleTypeId'];

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "manage.system.sysvehicletype.deletewarning"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var message: string = terms["manage.system.sysvehicletype.deletewarning"].format(row.filename);
            var modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val) {
                    this.startDelete();
                    this.systemService.deleteSysVehicleType(id).then((result) => {
                        if (result.success) {
                            this.completedDelete(null);
                        }
                        else {
                            this.failedDelete(result.errorMessage);
                        }
                    }, error => {
                        this.failedDelete(error.message);
                    });
                }
            });
        });
    }
}


