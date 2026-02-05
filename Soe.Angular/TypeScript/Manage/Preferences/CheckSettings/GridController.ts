import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IEditControllerFlowHandler } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbar } from "../../../Core/Handlers/Toolbar";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { IPreferencesService } from "../PreferencesService";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { lang } from "moment";
import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { Constants } from "../../../Util/Constants";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // terms
    terms: { [index: string]: string; };

    // Footer
    gridFooterComponentUrl: any;

    // Types
    resultTypes: any[];

    // Result rows
    result: any[];

    //@ngInject
    constructor(
        private preferencesService: IPreferencesService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private notificationService: INotificationService,
        private $timeout: ng.ITimeoutService,
        private $filter: ng.IFilterService,
        $uibModal,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,) {
        super(gridHandlerFactory, "Billing.Manage.Preferences.CheckSettings", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.beforeSetupGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_Preferences_CheckSettings, loadReadPermissions: true, loadModifyPermissions: true });

        this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("Manage/Preferences/CheckSettings/Views/gridFooter.html");
    }

    private beforeSetupGrid(): ng.IPromise<any> {
        return this.$q.all([this.loadTerms(), this.loadResultTypes(), this.loadAreas()]);
    }

    private loadTerms(): ng.IPromise<any> {
        var translationKeys: string[] = [
            "manage.preferences.checksettings.area",
            "common.value",
            "manage.preferences.checksettings.action",
            "common.connect.result",
            "common.dashboard.reload",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "common.connect.result",
            "manage.preferences.checksettings.settingsfor",
            "manage.preferences.checksettings.area",
            "manage.preferences.checksettings.verifying"
        ];

        return this.translationService.translateMany(translationKeys).then(terms => {
            this.terms = terms;
        });
    }

    private loadResultTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.CheckSettingsResultType, false, false).then(x => {
            this.resultTypes = x;
        });
    }

    private loadAreas(): ng.IPromise<any> {
        return this.preferencesService.getAreas().then((x) => {
            this.result = x;
            _.forEach(this.result, (area) => {
                area.areaName = this.terms["manage.preferences.checksettings.settingsfor"] + " " + area.areaName;
            });
        });
    }

    edit(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    private setupGrid() {
        this.gridAg.enableMasterDetail(true);
        this.gridAg.options.setDetailCellDataCallback((params) => {
            this.loadSettingsResult(params);

            this.$timeout(() => {
                this.gridAg.detailOptions.enableRowSelection = false;
                this.gridAg.detailOptions.sizeColumnToFit();
            });
        });

        // Master grid
        this.gridAg.addColumnText("areaName", this.terms["manage.preferences.checksettings.area"], null, false);
        this.gridAg.addColumnText("result", this.terms["common.connect.result"], null, false);
        this.gridAg.addColumnIcon("infoIconValue", null, null, { onClick: this.reloadDetailsGrid.bind(this), toolTip: this.terms["common.dashboard.reload"], icon: "fas fa-sync", showIcon: (row) => row['rowsLoaded'] });

        this.gridAg.options.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"]
        });

        // Detals grid
        this.gridAg.detailOptions.addColumnText("areaName", "", null);
        this.gridAg.detailOptions.addColumnText("setting", this.terms["manage.preferences.checksettings.area"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("description", this.terms["common.value"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("adjustment", this.terms["manage.preferences.checksettings.action"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnSelect("resultTypeName", this.terms["common.connect.result"], null, {
            populateFilterFromGrid: true,
            toolTipField: "resultTypeName", displayField: "resultTypeName", selectOptions: this.resultTypes, shape: Constants.SHAPE_CIRCLE, shapeValueField: "resultTypeColor", colorField: "resultTypeColor", enableHiding: true
        });
        this.gridAg.detailOptions.finalizeInitGrid();

        this.gridAg.addStandardMenuItems();
        this.gridAg.options.finalizeInitGrid();

        this.$timeout(() => {
            _.forEach(this.result, (y) => {
                y['expander'] = "";
            });
            this.setData(this.result);
        });
        
    }

    private reloadDetailsGrid(area: any) {
        this.progress.showProgressDialog();
        this.performCheckSettings(area, [area]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    private checkSettings() {
        var areasToCheck: any[] = [];
        _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
            areasToCheck.push(row);
        });

        if (areasToCheck.length === 0)
            return;

        this.progress.showProgressDialog();
        this.performCheckSettings(_.head(areasToCheck), areasToCheck);
    }

    private performCheckSettings(area: any, areasToCheck: any[]) {
        this.progress.updateProgressDialogMessage(this.terms["manage.preferences.checksettings.verifying"] + " " + area.areaName);
        this.preferencesService.checkSettings([area.area]).then((x) => {
            var noOfPassed: number = 0;
            var noOfWarnings: number = 0;
            var noOfErrors: number = 0;
            _.forEach(x, (y) => {
                var result = this.getResult(y.resultType);
                y['resultTypeName'] = result['name'];
                y['resultTypeColor'] = result['color'];
                noOfPassed += result['passed'];
                noOfWarnings += result['warnings'];
                noOfErrors += result['errors'];
            });

            area['result'] = noOfPassed + " godkända, " + noOfWarnings + " varningar, " + noOfErrors + " fel";

            // Remove current
            _.pull(areasToCheck, area);

            // Set values
            area['rows'] = x;
            area['rowsLoaded'] = true;
            this.gridAg.options.refreshRows(area);
            
            if (area['callback'])
                area['callback'](area['rows']);

            if (areasToCheck.length > 0) {
                this.performCheckSettings(_.head(areasToCheck), areasToCheck);
            }
            else {
                this.progress.hideProgressDialog();
            }
        });
    }

    private loadSettingsResult(params: any) {
        if (!params.data['rowsLoaded']) {
            this.progress.startLoadingProgress([() => {
                return this.preferencesService.checkSettings([params.data.area]).then((x) => {
                    var noOfPassed: number = 0;
                    var noOfWarnings: number = 0;
                    var noOfErrors: number = 0;
                    _.forEach(x, (y) => {
                        var result = this.getResult(y.resultType);
                        y['resultTypeName'] = result['name'];
                        y['resultTypeColor'] = result['color'];
                        noOfPassed += result['passed'];
                        noOfWarnings += result['warnings'];
                        noOfErrors += result['errors'];
                    });

                    params.data['result'] = noOfPassed + " godkända, " + noOfWarnings + " varningar, " + noOfErrors + " fel";

                    params.data['rows'] = x;
                    params.data['rowsLoaded'] = true;
                });
            }], this.terms["manage.preferences.checksettings.verifying"] + " " + params.data.areaName).then(() => {
                this.gridAg.options.refreshRows(params.data);
                params.data["callback"] = params.successCallback;
                params.successCallback(params.data['rows']);
            });

        }
        else {
            params.data["callback"] = params.successCallback;
            params.successCallback(params.data['rows']);
        }
    }

    private getResultType(resultType: number) {
        return _.find(this.resultTypes, { 'id': resultType }).name;
    }

    private getResult(resultType: number): any {
        var result = {};
        result['name'] = _.find(this.resultTypes, { 'id': resultType }).name;
        switch (resultType) {
            case 0:
                // Default
                result['passed'] = 0;
                result['warnings'] = 0;
                result['errors'] = 0;
                result['color'] = "#dfdfdf";
                break;
            case 1:
                // Passed
                result['passed'] = 1;
                result['warnings'] = 0;
                result['errors'] = 0;
                result['color'] = "#def1de";
                break;
            case 2:
                // Warning
                result['passed'] = 0;
                result['warnings'] = 1;
                result['errors'] = 0;
                result['color'] = "#fce3cc";
                break;
            case 3:
                // Error
                result['passed'] = 0;
                result['warnings'] = 0;
                result['errors'] = 1;
                result['color'] = "#f8d4d4";
                break;
        }
        return result;
    }

    private reloadData() {
        this.checkSettings();
    }
}
