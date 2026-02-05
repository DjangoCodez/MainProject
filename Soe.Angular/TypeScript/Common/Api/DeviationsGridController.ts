import { ITranslationService } from "../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../Core/ICompositionGridController";
import { Feature } from "../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../Core/Handlers/ControllerFlowHandler";
import { IApiService } from "./ApiService";
import { GridControllerBase2Ag } from "../../Core/Controllers/GridControllerBase2Ag";
import { EmployeeDeviationAfterEmploymentDTO } from "../Models/EmployeeDeviationAfterEmploymentDTO";

export class DeviationsGridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Lookups
    terms: { [index: string]: string; };

    // Data
    private deviations: EmployeeDeviationAfterEmploymentDTO[];

    // Permissions
    hasModifyPermission = false;

    // Flags
    private setupComplete: boolean;
    private activated = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private apiService: IApiService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Common.Api.Deviations", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.doLookup())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());

        this.onTabActivated(() => this.tabActivated());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        this.gridAg.options.enableFiltering = true;
        this.gridAg.options.setMinRowsToShow(20);
    }

    private tabActivated() {
        if (!this.activated) {
            this.flowHandler.start(this.getPermissions());
            this.activated = true;
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    private doLookup() {
        return this.$q.all([
            this.loadTerms(),
        ]).then(() => {
            this.setupComplete = true;
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.name",
            "common.api.deviation.dates",
            "core.delete",
            "common.api.employeenr",
            "common.api.employmentstop",
            "common.api.deviations.delete",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private getPermissions(): any[] {
        const features: any[] = [
            { feature: Feature.Time_Import_API, loadReadPermissions: true, loadModifyPermissions: true },
        ];
        return features;
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Import_API].readPermission;
        this.modifyPermission = response[Feature.Time_Import_API].modifyPermission;
    }

    public setupGrid() {
        this.gridAg.addColumnText("employeeNr", this.terms["common.api.employeenr"], 25, false);
        this.gridAg.addColumnText("name", this.terms["common.name"], 50, false);
        this.gridAg.addColumnDate("employmentStopDate", this.terms["common.api.employmentstop"], 25, false);
        this.gridAg.addColumnText("employeeDates.dateRangeText", this.terms["common.api.deviation.dates"], null, false);
        if (this.modifyPermission)
            this.gridAg.addColumnDelete(this.terms["core.delete"], this.deleteDeviations.bind(this));
        this.gridAg.finalizeInitGrid("common.api.deviations", true);
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });

    public loadGridData() {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.apiService.getEmployeeDeviationsAfterEmployment().then((deviations) => {
                this.deviations = deviations;
                this.setData(this.deviations);
            });
        }]);
    }

    private deleteDeviations(deviations) {
        this.progress.startDeleteProgress((completion) => {
            this.apiService.deleteEmployeeDeviationsAfterEmployment(deviations).then(result => {
                if (result.success) {
                    completion.completed(null);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, undefined, this.terms["common.api.deviations.delete"]
        ).then(() => {
            this.loadGridData();
        });
    }
}
