import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    items: any[];
    terms: any;
    gridFooterComponentUrl: any;
    isDisabled: boolean = true;

    private inactivatePermission: boolean = false;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private accountingService: IAccountingService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Economy.Accounting.AccountDims", progressHandlerFactory, messagingHandlerFactory);

        this.gridFooterComponentUrl = urlHelperService.getViewUrl("gridFooter.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.loadModifyPermissions())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData(true))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))

        this.gridAg.options.subscribe([new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => {
            row.entity.isSelected = row.isSelected;
        })]);
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(false); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Accounting_AccountRoles, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];
        features.push(Feature.Economy_Accounting_AccountRoles_Inactivate);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.inactivatePermission = x[Feature.Economy_Accounting_AccountRoles_Inactivate];
        });
    }

    public edit(row) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission)) {
            this.messagingHandler.publishEditRow(row);
        }
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            var accountDimIdArray = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (item) => {
                accountDimIdArray.push(item.accountDimId);
            });

            this.accountingService.deleteAccountDims(accountDimIdArray).then((result) => {
                if (result.success) {
                    completion.completed(result, false, result.infoMessage);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.loadGridData(false);
        });
    }

    private setUpGrid() {
        var translationKeys: string[] = [
            "core.yes",
            "core.no",
            "common.active",
            "common.number",
            "common.name",
            "common.shortname",
            "core.edit",
            "core.warning",
            "economy.accounting.accountdim.deletewarning",
            "economy.accounting.siedim",
            "economy.accounting.account.childdim",
            "economy.accounting.useinscheduleplanning",
            "economy.accounting.excludeinaccountingexport",
            "economy.accounting.excludeinsalaryreport"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            if (this.inactivatePermission)
                this.gridAg.addColumnActive("isActive", terms["common.active"], 50);
            this.gridAg.addColumnText("accountDimNr", terms["common.number"], 40, true);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("shortName", terms["common.shortname"], null);
            this.gridAg.addColumnText("parentAccountDimName", terms["economy.accounting.account.childdim"], null);
            this.gridAg.addColumnText("sysSieDimNr", terms["economy.accounting.siedim"], 40);
            this.gridAg.addColumnBool("useInSchedulePlanning", terms["economy.accounting.useinscheduleplanning"], 20, false);
            this.gridAg.addColumnBool("excludeinAccountingExport", terms["economy.accounting.excludeinaccountingexport"], 20, false);
            this.gridAg.addColumnBool("excludeinSalaryReport", terms["economy.accounting.excludeinsalaryreport"], 20, false);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => {
                this.$timeout(() => {
                    this.isDisabled = this.gridAg.options.getSelectedCount() === 0;
                });
            }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row) => {
                this.$timeout(() => {
                    this.isDisabled = this.gridAg.options.getSelectedCount() === 0;
                });
            }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("economy.accounting.accountdims", true, undefined, this.inactivatePermission);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData(false));
    }

    private loadGridData(useCache: boolean) {
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getAccountDims(false, true, false, false, true, useCache).then((x) => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }
}
