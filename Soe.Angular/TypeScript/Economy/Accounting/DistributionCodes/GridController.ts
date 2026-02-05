import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Feature } from "../../../Util/CommonEnumerations";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { DistributionCodeGridDTO } from "../../../Common/Models/DistributionCodeHeadDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Collections
    private openingHours: SmallGenericType[] = [];

    // Terms
    private terms: { [index: string]: string; };

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Accounting.DistributionCodes", progressHandlerFactory, messagingHandlerFactory)

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onDoLookUp(() => this.loadLookups())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))

        super.onTabActivetedAndModified(() => {
            this.loadGridData();
        });
    }

    onInit(parameters: any) {
        this.guid = parameters.guid;
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start({ feature: Feature.Economy_Preferences_VoucherSettings_DistributionCodes_Edit, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadOpeningHours()]).then(() => {
                this.setupGrid();
            });
    }

    public loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.name",
            "common.validfrom",
            "economy.accounting.distributioncode.numberofperiods",
            "common.type",
            "core.edit",
            "common.accountdim",
            "economy.accounting.distributioncode.accountingbudget",
            "economy.accounting.distributioncode.staffbudget",
            "economy.accounting.distributioncode.salebudget",
            "economy.accounting.distributioncode.projectbudget",
            "economy.accounting.distributioncode.sublevel",
            "economy.accounting.distributioncode.subtype",
            "economy.accounting.distributioncode.openinghours",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    public loadOpeningHours(): ng.IPromise<any> {
        return this.accountingService.getOpeningHoursDict(false, false, true).then(x => {
            this.openingHours = x;
        });
    }

    public setupGrid() {
        // Columns
        this.gridAg.addColumnText(this.interfacePropertyToString((o: DistributionCodeGridDTO) => o.name), this.terms["common.name"], null, true);
        this.gridAg.addColumnText(this.interfacePropertyToString((o: DistributionCodeGridDTO) => o.noOfPeriods), this.terms["economy.accounting.distributioncode.numberofperiods"], null, true);
        this.gridAg.addColumnSelect(this.interfacePropertyToString((o: DistributionCodeGridDTO) => o.type), this.terms["common.type"], null, { displayField: "type", selectOptions: null, populateFilterFromGrid: true, enableHiding: true });
        this.gridAg.addColumnDate(this.interfacePropertyToString((o: DistributionCodeGridDTO) => o.fromDate), this.terms["common.validfrom"], null);
        this.gridAg.addColumnSelect(this.interfacePropertyToString((o: DistributionCodeGridDTO) => o.typeOfPeriod), this.terms["economy.accounting.distributioncode.subtype"], null, { displayField: "typeOfPeriod", selectOptions: null, populateFilterFromGrid: true, enableHiding: true });
        this.gridAg.addColumnText(this.interfacePropertyToString((o: DistributionCodeGridDTO) => o.subLevel), this.terms["economy.accounting.distributioncode.sublevel"], null, true);
        this.gridAg.addColumnText(this.interfacePropertyToString((o: DistributionCodeGridDTO) => o.accountDim), this.terms["common.accountdim"], null, true);
        if (this.openingHours.length > 1)
            this.gridAg.addColumnText(this.interfacePropertyToString((o: DistributionCodeGridDTO) => o.openingHour), this.terms["economy.accounting.distributioncode.openinghours"], null);
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        this.gridAg.finalizeInitGrid("economy.accounting.distributioncode.distributioncodes", true);
    }

    edit(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    public loadGridData() {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getDistributionCodesForGrid().then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    interfacePropertyToString = (property: (object: any) => void) => {
        var chaine = property.toString();
        var arr = chaine.match(/[\s\S]*{[\s\S]*\.([^\.; ]*)[ ;\n]*}/);
        return arr[1];
    };
}
