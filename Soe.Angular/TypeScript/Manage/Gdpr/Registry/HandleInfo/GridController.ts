import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { HtmlUtility } from "../../../../Util/HtmlUtility";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandler } from "../../../../Core/Handlers/MessagingHandler";
import { IGridControllerFlowHandler, IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";
import { IToolbar } from "../../../../Core/Handlers/Toolbar";
import { Feature, SoeActorType } from "../../../../Util/CommonEnumerations";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { GDPRService } from "../../GDPRService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        protected gdprService: GDPRService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private $filter: ng.IFilterService,
        private messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private notificationService: INotificationService,
        private $scope: ng.IScope,
        private $uibModal) {
        super(gridHandlerFactory, "Billing.Invoices.SupplierAgreements", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            //.onBeforeSetUpGrid(() => this.loadSuppliers())
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    public onInit(parameters: any) {
        this.flowHandler.start([
            { feature: Feature.Manage_GRPR_Registry_HandlePersonalInfo, loadReadPermissions: true, loadModifyPermissions: true }
        ]);

        this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("Manage/Gdpr/Registry/HandleInfo/Views/gridFooter.html"); //CalendarUtility.getDateToday();
        this.consentDate = CalendarUtility.getDateToday();
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_GRPR_Registry_HandlePersonalInfo].readPermission;
        this.modifyPermission = response[Feature.Manage_GRPR_Registry_HandlePersonalInfo].modifyPermission;
    }

    public edit(row: any) {
    };

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());

        //Setup toolbar
        /*this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("billing.invoices.supplieragreement.addagreement", "billing.invoices.supplieragreement.addagreement", IconLibrary.FontAwesome, "fa-plus",
            () => { this.initAddSupplierAgreement(); }
        )));*/
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.warning",
            "manage.gdpr.registry.invaliddeleteselection",
            "core.yes",
            "core.no",
            "common.contactperson.contactperson",
            "common.customer",
            "common.entitylogviewer.selection.registrys.supplier",
            "common.date",
            "common.type",
            "common.name",
            "common.email",
            "manage.gdpr.registry.deletequestion",
            "manage.gdpr.registry.hasconnectedinvoices",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.yesNoDict.push({ id: 1, name: this.terms["core.yes"] })
            this.yesNoDict.push({ id: 2, name: this.terms["core.no"] })

            this.typeDict.push({ id: 1, name: this.terms["common.customer"] })
            this.typeDict.push({ id: 2, name: this.terms["common.entitylogviewer.selection.registrys.supplier"] })
            this.typeDict.push({ id: 3, name: this.terms["common.contactperson.contactperson"] })
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.gdprService.getActorsWithoutConsent().then((x) => {
                _.forEach(x, (item) => {
                    item.actorTypeName = this.typeDict[item.actorType - 1].name;
                    item.hasConnectedInvoicesName = item.hasConnectedInvoices ? this.yesNoDict[0].name : this.yesNoDict[1].name;
                })
                this.setData(x);
            });
        }]);
    }

    private setUpGrid() {
        this.gridAg.addColumnSelect("actorTypeName", this.terms["common.type"], null, { displayField: "actorTypeName", selectOptions: this.typeDict, dropdownValueLabel: "name" });
        this.gridAg.addColumnText("actorName", this.terms["common.name"], null);
        this.gridAg.addColumnText("email", this.terms["common.email"], null);
        this.gridAg.addColumnSelect("hasConnectedInvoicesName", this.terms["manage.gdpr.registry.hasconnectedinvoices"], null, { displayField: "hasConnectedInvoicesName", selectOptions: this.yesNoDict, dropdownValueLabel: "name" });

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("manage.gdpr.registry.handleinfo", true);
    }

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            var noOfRows: number = 0;
            var invalidDelete: number = 0;
            _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                if (row.hasConnectedInvoices)
                    invalidDelete++;
                noOfRows++;
            });

            if (noOfRows > 0) {
                this.isOkToGiveConsent = true;
                if (invalidDelete > 0) {
                    this.isOkToDelete = false;
                    this.showInvalidButton = true;
                }
                else {
                    this.isOkToDelete = true;
                    this.showInvalidButton = false;
                }
            }
            else {
                this.isOkToGiveConsent = false;
                this.isOkToDelete = false;
                this.showInvalidButton = false;
            }
        });
    }

    private delete() {
        var modal = this.notificationService.showDialog(this.terms["core.verifyquestion"], this.terms["manage.gdpr.registry.deletequestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val != null && val === true) {
                this.$scope.$applyAsync(() => {
                    var customers: number[] = [];
                    var suppliers: number[] = [];
                    var contactPersons: number[] = [];
                    _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                        switch (row.actorType) {
                            case SoeActorType.Customer:
                                customers.push(row.actorId);
                                break;
                            case SoeActorType.Supplier:
                                suppliers.push(row.actorId);
                                break;
                            case SoeActorType.ContactPerson:
                                contactPersons.push(row.actorId);
                                break;
                        }
                    });

                    this.progress.startSaveProgress((completion) => {
                        this.gdprService.deleteActorsWithoutConsent(customers, suppliers, contactPersons).then((result) => {
                            if (result.success) {
                                this.loadGridData();
                                completion.completed();
                            } else {
                                completion.failed(result.errorMessage);
                            }
                        });
                    }, null);
                });
            };
        });
    }

    private giveConsent() {
        this.$scope.$applyAsync(() => {
            var customers: number[] = [];
            var suppliers: number[] = [];
            var contactPersons: number[] = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                switch (row.actorType) {
                    case SoeActorType.Customer:
                        customers.push(row.actorId);
                        break;
                    case SoeActorType.Supplier:
                        suppliers.push(row.actorId);
                        break;
                    case SoeActorType.ContactPerson:
                        contactPersons.push(row.actorId);
                        break;
                }
            });

            this.progress.startSaveProgress((completion) => {
                this.gdprService.giveConsent(this.consentDate, customers, suppliers, contactPersons).then((result) => {
                    if (result.success) {
                        this.loadGridData();
                        completion.completed();
                    } else {
                        completion.failed(result.errorMessage);
                    }
                });
            }, null);
        });
    }

    private showError() {
        this.notificationService.showDialog(this.terms["core.warning"], this.terms["manage.gdpr.registry.invaliddeleteselection"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
    }

    private terms: { [index: string]: string; };
    private gridFooterComponentUrl: any;
    private yesNoDict: any[] = [];
    private typeDict: any[] = [];
    private consentDate: Date;
    private isOkToGiveConsent: boolean;
    private isOkToDelete: boolean;
    private showInvalidButton: boolean = false;
}