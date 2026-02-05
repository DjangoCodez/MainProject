import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ExportUtility } from "../../../Util/ExportUtility";
import { TermGroup } from "../../../Util/CommonEnumerations";
import { ContactPersonDTO } from "../../Models/ContactPersonDTOs";

export class AddContactPersonController {
    public contactPerson: ContactPersonDTO;
    public title: string;
    private consentToolTip: string;
    private positions: ISmallGenericType[];

    set hasConsent(value: any) {
        this.contactPerson.hasConsent = value;
        if (this.contactPerson.hasConsent && !this.contactPerson.consentDate) {
            this.contactPerson.consentDate = CalendarUtility.getDateToday();
        }
    }

    get hasConsent() {
        return this.contactPerson.hasConsent;
    }

    get isExistingContactPerson() {
        return this.contactPerson && this.contactPerson.actorContactPersonId && this.contactPerson.actorContactPersonId > 0;
    }

    get validToSave() {
        return this.contactPerson && this.contactPerson.firstName && this.contactPerson.firstName.length > 0 && this.contactPerson.lastName && this.contactPerson.lastName.length > 0;
    }

    //@ngInject
    constructor(private $uibModalInstance,
        private $window: ng.IWindowService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        contactPerson: ContactPersonDTO, title) {
        this.contactPerson = contactPerson;
        this.title = title;
    }

    private $onInit() {
        this.setConsentToolTip();
        this.getCategories();
        this.getPositions();
    }

    public cancel() {
        console.log(this.contactPerson)
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close(this.contactPerson);
    }

    private getCategories() {
        this.contactPerson.categoryIds = []
        if (this.isExistingContactPerson) {
            this.coreService.getContactPersonCategories(this.contactPerson.actorContactPersonId).then(cats => this.contactPerson.categoryIds = cats || [])
        }
    }

    private getPositions() {
        this.coreService.getTermGroupContent(TermGroup.ContactPersonPosition, true, false, false).then(pos => this.positions = pos)
    }

    private setConsentToolTip() {

        var keys: string[] = [
            "common.consentdescr",
            "common.modifiedby"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.consentToolTip = terms["common.consentdescr"] + "\n"
            if (this.contactPerson.consentModifiedBy) {
                this.consentToolTip = this.consentToolTip + terms["common.modifiedby"] + ": " + this.contactPerson.consentModifiedBy + " " + CalendarUtility.toFormattedDate(this.contactPerson.consentModified);
            }
        });

    }

    public exportContactPerson() {
        return this.coreService.getContactPersonForExport(this.contactPerson.actorContactPersonId).then((contactPerson) => {
            if (contactPerson)
                ExportUtility.Export(contactPerson, 'contactperson.json');
        });
    }
}