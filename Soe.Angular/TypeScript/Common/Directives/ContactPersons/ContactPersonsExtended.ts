import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IContactPersonService } from "./ContactPersonService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Feature } from "../../../Util/CommonEnumerations";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { IContactPersonDTO } from "../../../Scripts/TypeLite.Net4";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { AddContactPersonController } from "../../../Common/Directives/ContactPersons/AddContactPersonController";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ExportUtility } from "../../../Util/ExportUtility";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { Constants } from "../../../Util/Constants";
import { Guid } from "../../../Util/StringUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    modal: any;

    private actorIds: number[];
    private terms: { [index: string]: string; };
    private yesNoDict: any[] = [];
    private rowsSelected = false;
    private isModal = false;

    get filteredByActors() {
        return this.actorIds && this.actorIds.length
    }

    //@ngInject
    constructor(
        private contactPersonService: IContactPersonService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private notificationService: INotificationService,
        private $scope: ng.IScope,
        private $uibModal) {
        super(gridHandlerFactory, "Manage.ContactPerson.ContactPersons", progressHandlerFactory, messagingHandlerFactory);

        this.$scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            if (parameters && parameters.sourceGuid === this.guid) {
                return;
            }
            if (parameters.actorIds && parameters.actorIds.length) {
                this.actorIds = _.uniq(parameters.actorIds);
            }
            parameters.guid = Guid.newGuid();
            this.modal = parameters.modal;

            this.onInit(parameters);
            this.isModal = true;
        });


        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadData());
    }

    public onInit(parameters: any) {
        this.flowHandler.start([
            { feature: Feature.Manage_ContactPersons, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Manage_ContactPersons_Edit, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_ContactPersons].readPermission;
        this.modifyPermission = response[Feature.Manage_ContactPersons_Edit].modifyPermission;
    }

    public edit(row: any) {
    }

    private closeModal() {
        this.modal.dismiss();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadData());
        if (!this.filteredByActors) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.contactperson.addcontactperson", "common.contactperson.addcontactperson", IconLibrary.FontAwesome, "fa-plus",
                () => { this.initCreateContactPerson(); }
            )));
        }

        if (!this.filteredByActors) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.delete", "core.delete", IconLibrary.FontAwesome, "fa-user-slash",
                () => { return this.delete() },
                () => { return !this.rowsSelected },
            )));
        }
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.verifyquestion",
            "core.yes",
            "core.no",
            "common.firstname",
            "common.lastname",
            "common.emailaddress",
            "common.telephonenumber",
            "common.download",
            "manage.gdpr.registry.deletequestion",
            "common.contactperson.contactperson",
            "common.contactperson.hasconsent",
            "common.contactperson.consentdate",
            "common.contactperson.deletequestion",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "common.contactperson.suppliername",
            "common.contactperson.suppliernumber",
            "common.report.selection.customernr",
            "common.report.selection.customername",
            "common.contactperson.position",
            "common.categories.category"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.yesNoDict.push({ id: 1, name: this.terms["core.yes"] })
            this.yesNoDict.push({ id: 2, name: this.terms["core.no"] })
        });
    }

    private loadData() {
        if (this.filteredByActors) {
            this.loadGridDataByActorids(this.actorIds)
        }
        else {
            this.loadGridDataAll()
        }
    }

    private loadGridDataByActorids(ids: number[]) {
        this.progress.startLoadingProgress([() => {
            return this.contactPersonService.getContactPersonsByActorIds(ids).then((x) => {
                this.setGridData(x);
            });
        }]);
    }

    private loadGridDataAll() {
        this.progress.startLoadingProgress([() => {
            return this.contactPersonService.getContactPersons(false).then((x) => {
                this.setGridData(x);
            });
        }]);
    }

    private setGridData(data: any[]) {
        data.forEach(item => {
            item.consentDate = CalendarUtility.convertToDate(item.consentDate);

            //https://www.ag-grid.com/documentation/vue/grouping/, to prevent unbalanced grouping...
            item.supplierName = item.supplierName || "";
            item.supplierNr = item.supplierNr || "";
            item.customerName = item.customerName || "";
            item.customerNr = item.customerNr || "";
            item.categoryString = item.categoryString || "";
            item.positionName = item.positionName || "";
            item.consentDate = item.consentDate || "";

            item.hasConsentName = item.hasConsent ? this.yesNoDict[0].name : this.yesNoDict[1].name;
        })
        this.setData(data);
    }

    private setUpGrid() {
        this.gridAg.options.setName("contactPersonsGrid");
        this.gridAg.addColumnText("firstName", this.terms["common.firstname"], null, true);
        this.gridAg.addColumnText("lastName", this.terms["common.lastname"], null, true);
        this.gridAg.addColumnText("email", this.terms["common.emailaddress"], null, true);
        this.gridAg.addColumnText("phoneNumber", this.terms["common.telephonenumber"], null, true);
        this.gridAg.addColumnSelect("hasConsentName", this.terms["common.contactperson.hasconsent"], null, { enableRowGrouping: true, enableHiding: true, displayField: "hasConsentName", selectOptions: this.yesNoDict, dropdownValueLabel: "name" });
        this.gridAg.addColumnDate("consentDate", this.terms["common.contactperson.consentdate"], null, true, null, { enableRowGrouping: true });
        this.gridAg.addColumnText("supplierNr", this.terms["common.contactperson.suppliernumber"], null, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("supplierName", this.terms["common.contactperson.suppliername"], null, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("customerNr", this.terms["common.report.selection.customernr"], null, true, { enableRowGrouping: true, enableHiding: true });
        this.gridAg.addColumnText("customerName", this.terms["common.report.selection.customername"], null, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("positionName", this.terms["common.contactperson.position"], null, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("categoryString", this.terms["common.categories.category"], null, true, { enableRowGrouping: true });
        this.gridAg.addColumnIcon(null, this.terms["common.edit"], null, { icon: "fal fa-pencil iconEdit", onClick: this.editContactPerson.bind(this) });
        this.gridAg.addColumnIcon(null, this.terms["common.download"], null, { icon: "fal fa-download iconEdit", onClick: this.exportContactPerson.bind(this) });

        const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        this.gridAg.options.subscribe(events);

        this.gridAg.options.useGrouping(false, false, { keepColumnsAfterGroup: false, selectChildren: true });
        this.gridAg.finalizeInitGrid("manage.contactperson.contactpersons.contactpersons", true);
    }

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.rowsSelected = this.gridAg.options.getSelectedRows().length > 0;
        });
    }

    private initCreateContactPerson() {
        var contactPerson = <IContactPersonDTO>{};
        // Fix names
        contactPerson.firstName = contactPerson.lastName = "";
        this.createEditContactPerson(contactPerson);
    }

    private editContactPerson(row: any) {
        this.createEditContactPerson(row);
    }

    private createEditContactPerson(contactPerson: IContactPersonDTO) {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getCommonDirectiveUrl("ContactPersons", "AddContactPerson.html"),
            controller: AddContactPersonController,
            backdrop: 'static',
            controllerAs: "ctrl",
            resolve: {
                contactPerson: () => { return contactPerson },
                title: () => { return this.terms["common.contactperson.contactperson"] },
            }
        });

        modal.result.then((editedContactPerson: IContactPersonDTO) => {
            if (editedContactPerson) {
                this.progress.startSaveProgress((completion) => {
                    this.contactPersonService.saveContactPerson(editedContactPerson).then((result) => {
                        if (result.success) {
                            this.loadData();
                            completion.completed(null, null, true);
                        }
                        else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null);
            }
        });
    }

    private deleteContactPerson(row: any) {
        if (!row)
            return;

        const modal = this.notificationService.showDialog(this.terms["core.verifyquestion"], this.terms["common.contactperson.deletequestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val != null && val === true) {
                this.progress.startSaveProgress((completion) => {
                    this.contactPersonService.deleteContactPerson(row.actorContactPersonId).then((result) => {
                        if (result.success) {
                            this.loadData();
                            completion.completed(null, null, true);
                        }
                        else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null);
            }
        });
    }

    private delete() {
        const modal = this.notificationService.showDialog(this.terms["core.verifyquestion"], this.terms["manage.gdpr.registry.deletequestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val != null && val === true) {
                this.$scope.$applyAsync(() => {
                    var contactPersons: number[] = [];
                    _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                        contactPersons.push(row.actorContactPersonId);
                    });

                    this.progress.startSaveProgress((completion) => {
                        this.contactPersonService.deleteContactPersons(contactPersons).then((result) => {
                            if (result.success) {
                                this.loadData();
                                completion.completed();
                            } else {
                                completion.failed(result.errorMessage);
                            }
                        });
                    }, null);
                });
            }
        });
    }

    private exportContactPerson(row: any) {
        this.progress.startLoadingProgress([() => {
            return this.contactPersonService.getContactPersonForExport(row.actorContactPersonId).then((contactPerson) => {
                if (contactPerson)
                    ExportUtility.Export(contactPerson, 'contactperson.json');
            });
        }]);
    }
}