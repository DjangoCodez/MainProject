import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private startupFilter: any = {
        isActive: ['true']
    };
    private gridName: string;

    private sysCountries: ISmallGenericType[];

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private reportService: IReportService,
        private translationService: ITranslationService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Common.Reports.ReportTemplates", progressHandlerFactory, messagingHandlerFactory);

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
            .onBeforeSetUpGrid(() => this.loadCountries())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false));
    }

    // SETUP

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.gridName = soeConfig.isSys ? "common.report.sysreporttemplate.reporttemplates" : "common.report.userreporttemplate.reporttemplates";

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(false, true).then(x => {
            this.sysCountries = x;
        });
    }

    private setupGrid() {
        var keys: string[] = [
            "common.number",
            "common.name",
            "common.description",
            "common.typename",
            "common.country",
            "core.edit",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnNumber("reportNr", terms["common.number"], 50);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnText("sysReportTemplateTypeName", terms["common.typename"], null, true);
            this.gridAg.addColumnSelect("sysCountryIds", terms["common.country"], null, { selectOptions: this.sysCountries, displayField: "countryNames", dropdownValueLabel: "name", dropdownIdLabel: "id" });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.finalizeInitGrid(this.gridName, true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }
        
    // SERVICE CALLS   

    public loadGridData(useCache: boolean) {
        if (soeConfig.isSys)
            this.getSysReportTemplates();
        else
            this.getUserReportTemplates();
    }

    private getSysReportTemplates() {
        this.progress.startLoadingProgress([() => {
            return this.reportService.getSysReportTemplatesForModule(soeConfig.module, false).then((data) => {
                _.forEach(data, (r) => {
                    r['countryNames'] = r.sysCountryIds ? r.sysCountryIds.map(x => this.sysCountries.find(s => s.id == x).name || "") : "";
                });
                this.setData(data);
            })
        }]);
    }

    private getUserReportTemplates() {
        this.progress.startLoadingProgress([() => {
            return this.reportService.getUserReportTemplatesForModule(soeConfig.module).then((data) => {
                _.forEach(data, (r) => {
                    r['countryNames'] = r.sysCountryIds ? r.sysCountryIds.map(x => this.sysCountries.find(s => s.id == x).name || "") : "";
                });
                this.setData(data);
            })
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }

    // EVENTS   

    edit(row) {
        // Send message to TabsController        
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
}