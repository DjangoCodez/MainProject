import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { Feature, TermGroup } from "../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { InformationGridDTO } from "../../../../Common/Models/InformationDTOs";
import { ISystemService } from "../../SystemService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Lookups
    private severities: ISmallGenericType[];

    private selectedCount: number = 0;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Manage.System.Admin.SysInformation", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadSeverities())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    // SETUP

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupGrid() {
        var keys: string[] = [
            "core.edit",
            "core.document.folder",
            "core.document.validfrom",
            "core.document.validto",
            "common.subject",
            "common.messages.needsconfirmation",
            "core.informationmenu.companyinformation.shorttext",
            "core.informationmenu.companyinformation.severity",
            "core.informationmenu.companyinformation.showinweb",
            "core.informationmenu.companyinformation.showinmobile",
            "core.informationmenu.companyinformation.showinterminal",
            "core.informationmenu.companyinformation.notify",
            "core.informationmenu.companyinformation.deletewarning.title",
            "core.informationmenu.companyinformation.deletewarning.message",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnText("folder", terms["core.document.folder"], 100, true);
            this.gridAg.addColumnText("subject", terms["common.subject"], null, true);
            this.gridAg.addColumnText("shortText", terms["core.informationmenu.companyinformation.shorttext"], null, true);
            this.gridAg.addColumnSelect("severityName", terms["core.informationmenu.companyinformation.severity"], 100, { displayField: "severityName", selectOptions: this.severities });
            this.gridAg.addColumnDateTime("validFrom", terms["core.document.validfrom"], 75);
            this.gridAg.addColumnDateTime("validTo", terms["core.document.validto"], 75);
            this.gridAg.addColumnIcon(null, null, 25, { icon: "fal fa-check-square", toolTip: terms["common.messages.needsconfirmation"], showIcon: (i: InformationGridDTO) => { return i.needsConfirmation } })
            this.gridAg.addColumnIcon(null, null, 25, { icon: "fal fa-desktop", toolTip: terms["core.informationmenu.companyinformation.showinweb"], showIcon: (i: InformationGridDTO) => { return i.showInWeb } })
            this.gridAg.addColumnIcon(null, null, 25, { icon: "fal fa-mobile-android-alt", toolTip: terms["core.informationmenu.companyinformation.showinmobile"], showIcon: (i: InformationGridDTO) => { return i.showInMobile } })
            this.gridAg.addColumnIcon(null, null, 25, { icon: "fal fa-window-alt", toolTip: terms["core.informationmenu.companyinformation.showinterminal"], showIcon: (i: InformationGridDTO) => { return i.showInTerminal } })
            this.gridAg.addColumnIcon("notifyIcon", null, 25, { toolTip: terms["core.informationmenu.companyinformation.notify"], showIcon: (i: InformationGridDTO) => { return i.notify } })
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rowNode) => {
                this.$timeout(() => {
                    this.selectedCount = this.gridAg.options.getSelectedCount();
                });
            }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("manage.system.admin.sysinformation", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData(), false);

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.delete", "core.delete", IconLibrary.FontAwesome, "fa-times",
            () => { this.initDelete(); },
            () => { return this.selectedCount === 0 },
            () => { return !this.modifyPermission })));
    }

    // SERVICE CALLS   

    private loadSeverities(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InformationSeverity, false, false, true).then((x => {
            this.severities = x;
        }));
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.systemService.getSysInformationGrids().then(x => {
                _.forEach(x, y => {
                    y['notifyIcon'] = y.notificationSent ? 'fal fa-bell-on' : 'fal fa-bell';
                });
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    private initDelete() {
        var ids: number[] = this.gridAg.options.getSelectedIds("informationId");
        if (ids && ids.length > 0) {
            let keys = [
                "core.informationmenu.companyinformation.deletewarning.title",
                "core.informationmenu.companyinformation.deletewarning.message"
            ]

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["core.informationmenu.companyinformation.deletewarning.title"], terms["core.informationmenu.companyinformation.deletewarning.message"].format(ids.length.toString()), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val)
                        this.deleteInformations(ids);
                });
            });
        }
    }

    private deleteInformations(informationIds: number[]) {
        return this.progress.startWorkProgress((completion) => {
            this.systemService.deleteSysInformations(informationIds).then(result => {
                if (result.success)
                    completion.completed(null, true);
                else {
                    completion.failed(result.errorMessage);
                }
            });
        }, null).then(data => {
            this.reloadData();
        }, error => {
        });
    }

    // EVENTS   

    edit(row) {
        // Send message to TabsController        
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
}