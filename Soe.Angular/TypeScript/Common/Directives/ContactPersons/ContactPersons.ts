import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { InterfaceUtility } from "../../../Util/InterfaceUtility"
import { CoreUtility } from "../../../Util/CoreUtility";
import { Constants } from "../../../Util/Constants";
import { AddContactPersonController } from "./AddContactPersonController";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { ContactPersonDTO } from "../../Models/ContactPersonDTOs";

export class ContactPersonsController extends GridControllerBase2Ag {
    public contactPersons: ContactPersonDTO[];
    public actorId: number;
    public selected: ContactPersonDTO;
    private contactPersonsForActor: ContactPersonDTO[];
    public contactPersonsForActorIds: Array<number> = [];
    modal: angular.ui.bootstrap.IModalService;
    private addContactTitle: string;
    private gridFinalized: boolean = false;

    //@ngInject
    constructor($scope, $http,
        $templateCache,
        private $timeout: ng.ITimeoutService,
        private $uibModal: ng.ui.bootstrap.IModalService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService) {

        super(gridHandlerFactory, "common.contactperson.contactpersons", progressHandlerFactory, messagingHandlerFactory);

        this.modal = $uibModal;
    }

    public $onInit() {
        this.loadLookups();
    }

    public setupGrid(): void {
        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableFiltering = true;
        this.gridAg.options.setMinRowsToShow(10);

        const keys: string[] = ["common.contactperson.addcontactperson", "common.firstname", "common.lastname", "common.email", "common.phone", "core.edit", "core.remove", "common.contactperson.addcontactperson"];
        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText(InterfaceUtility.interfacePropertyToString((o: ContactPersonDTO) => o.firstName), terms["common.firstname"], null);
            this.gridAg.addColumnText(InterfaceUtility.interfacePropertyToString((o: ContactPersonDTO) => o.lastName), terms["common.lastname"], null);
            this.gridAg.addColumnText(InterfaceUtility.interfacePropertyToString((o: ContactPersonDTO) => o.email), terms["common.email"], null);
            this.gridAg.addColumnText(InterfaceUtility.interfacePropertyToString((o: ContactPersonDTO) => o.phoneNumber), terms["common.phone"], null);
            this.gridAg.addColumnIcon(null, " ", null, { icon: "fal fa-pencil iconEdit", toolTip: terms["common.edit"], onClick: this.openEdit.bind(this), suppressFilter: true });
            this.gridAg.addColumnIcon(null, " ", null, { icon: "fal fa-times iconDelete", toolTip: terms["common.remove"], onClick: this.deleteContactPerson.bind(this), suppressFilter: true });
            this.addContactTitle = terms["common.contactperson.addcontactperson"];

            this.gridAg.finalizeInitGrid("common.contactperson.contactpersons", true);

            this.$timeout(() => {
                this.gridAg.setData(this.contactPersonsForActor);
                this.gridFinalized = true;
            });
        });
    }

    public loadLookups() {
        this.$q.all([
            this.loadContactPersonsLookUp(true),
            this.loadContactPersons()]).then(() => {
                this.setupGrid();
            });
    }

    private loadContactPersons(): ng.IPromise<any> {
        if (!this.actorId) {
            this.contactPersonsForActor = [];
            return;
        }

        return this.coreService.getContactPersonsForActor(this.actorId, true, false).then((result: ContactPersonDTO[]) => {
            this.contactPersonsForActor = result;
            for (var i = 0; i < this.contactPersonsForActor.length; i++) {
                let contact = this.contactPersonsForActor[i];
                contact.consentDate = CalendarUtility.convertToDate(contact.consentDate);
            }

            this.gridAg.setData(this.contactPersonsForActor);
        });
    }

    private loadContactPersonsLookUp(useCache: boolean): ng.IPromise<any> {
        return this.coreService.getContactPersonsForActor(CoreUtility.actorCompanyId, true, useCache).then((result: ContactPersonDTO[]) => {
            this.contactPersons = _.sortBy(result, (c) => { return c.firstName.toLowerCase(); });
        });
    }

    protected openEdit(row: ContactPersonDTO) {
        this.openEditContactPerson(row);
    }

    protected deleteContactPerson(contactperson: ContactPersonDTO) {
        for (var i = 0; i < this.contactPersonsForActor.length; i++) {
            if (this.contactPersonsForActor[i] === contactperson) {
                this.contactPersonsForActor.splice(i, 1);
                this.gridAg.setData(this.contactPersonsForActor);
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid });
                break;
            }
        }
    }

    public addNewContactPerson() {
        var contactPerson = <ContactPersonDTO>{};
        // Fix names
        contactPerson.firstName = contactPerson.lastName = "";
        this.openEditContactPerson(contactPerson);
    }

    public addContactPersonChanged(id: number) {
        if (!id)
            return;

        let added: boolean = false;
        for (var i = 0; i < this.contactPersonsForActor.length; i++) {
            if (id === this.contactPersonsForActor[i].actorContactPersonId) {
                added = true;
            }
        }

        if(!added)
            this.contactPersonsForActor.push(this.contactPersons.filter((o: ContactPersonDTO) => { return o.actorContactPersonId === id })[0]);
        this.gridAg.setData(this.contactPersonsForActor);
    }

    public getcontactPersonsForActorIds() {
        if (this.contactPersonsForActor) {
            this.contactPersonsForActorIds = [];
            for (let i = 0; i < this.contactPersonsForActor.length; i++) {
                this.contactPersonsForActorIds.push(this.contactPersonsForActor[i].actorContactPersonId);
            }
        }
    }

    saveContactPerson(updatedContactPerson: ContactPersonDTO): void {
        this.progress.startSaveProgress((completion) => {
            this.coreService.saveContactPerson(updatedContactPerson).then((result) => {
                if (result.success) {
                    let exists = false;
                    for (let i = 0; i < this.contactPersonsForActor.length; i++) {
                        if (result.value.actorContactPersonId === this.contactPersonsForActor[i].actorContactPersonId) {
                            exists = true;
                        }
                    }
                    if (!exists) {
                        this.contactPersonsForActor.push(result.value);
                        this.loadContactPersonsLookUp(false);
                    }

                    result.value.consentDate = CalendarUtility.convertToDate(result.value.consentDate);
                    this.addContactPersonChanged(result.value.actorContactPersonId);
                    this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid });

                    completion.completed();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid);
    }

    private openEditContactPerson(contactPerson: ContactPersonDTO) {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getCommonDirectiveUrl("ContactPersons", "AddContactPerson.html"),
            controller: AddContactPersonController,
            backdrop: 'static',
            controllerAs: "ctrl",
            resolve: {
                contactPerson: () => {
                    return { actorContactPersonId: contactPerson.actorContactPersonId, email: contactPerson.email, firstName: contactPerson.firstName, lastName: contactPerson.lastName, phoneNumber: contactPerson.phoneNumber, hasConsent: contactPerson.hasConsent, consentDate: contactPerson.consentDate, position: contactPerson.position }
                },
                title: () => { return this.addContactTitle },
            }
        });

        modal.result.then((updatedContactPerson: ContactPersonDTO) => {
            if (updatedContactPerson && contactPerson) {
                contactPerson.firstName = updatedContactPerson.firstName;
                contactPerson.lastName = updatedContactPerson.lastName;
                contactPerson.email = updatedContactPerson.email;
                contactPerson.phoneNumber = updatedContactPerson.phoneNumber;
                contactPerson.hasConsent = updatedContactPerson.hasConsent;
                contactPerson.consentDate = updatedContactPerson.consentDate;
                contactPerson.categoryIds = updatedContactPerson.categoryIds;
                contactPerson.position = updatedContactPerson.position;
                this.saveContactPerson(contactPerson);
            }
        });
    }
}

//@ngInject
export function ContactPersonsDirecitve(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        restrict: "E",
        templateUrl: urlHelperService.getCommonDirectiveUrl("ContactPersons", "ContactPersons.html"),
        scope: {
            actorId: "=",
            contactPersonsForActorIds: "=",
            guid: "="

        },
        link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
            scope.$watch(() => (ngModelController.contactPersonsForActor), (newVAlue, oldValue, scope) => {
                ngModelController.getcontactPersonsForActorIds();
            }, true);

            scope.$watch(() => (ngModelController.actorId), (newValue, oldValue, scope) => {
                if (newValue && newValue > 0 && newValue !== oldValue) {
                    ngModelController.loadContactPersons(false, true);
                }
            }, true);
        },
        controller: ContactPersonsController,
        controllerAs: "ctrl",
        bindToController: true,
        replace: true
    }
}