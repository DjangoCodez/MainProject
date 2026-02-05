import { ICoreService } from "../../../Core/Services/CoreService";
import { IHttpService } from "../../../Core/Services/HttpService";
import { Constants } from "../../../Util/Constants";
import { IContactPersonDTO } from "../../../Scripts/TypeLite.Net4";
import { ContactPersonDTO } from "../../Models/ContactPersonDTOs";

export interface IContactPersonService {

    // GET
    getContactPersons(useCache: boolean): ng.IPromise<any>;
    getContactPersonsByActorIds(actorIds: number[]): ng.IPromise<any>;
    getContactPersonForExport(actorId: number): ng.IPromise<any>;
    getContactPersonCategories(actorId: number): ng.IPromise<any>;

    // POST
    saveContactPerson(contactPerson: IContactPersonDTO): ng.IPromise<any>;
    deleteContactPersons(contactPersonIds: number[]): ng.IPromise<any>;

    // DELETE
    deleteContactPerson(contactPersonId: number): ng.IPromise<any>;
}

export class ContactPersonService implements IContactPersonService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getContactPersons(useCache: boolean): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_PERSON_CONTACT_PERSONS, useCache);
    }

    getContactPersonsByActorIds(actorIds: number[]): ng.IPromise<any> {
        let actors = "";
        if (actorIds.length > 0) {
            actors = actorIds.join(",");
        }
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_PERSON_CONTACT_PERSONS_BY_ACTORS + actors, false);
    }

    getContactPersonForExport(actorId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_PERSON_EXPORT + actorId, false);
    }

    getContactPersonCategories(actorId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_PERSON_CATEGORIES + actorId, false);
    }

    // POST
    saveContactPerson(contactPerson: ContactPersonDTO): ng.IPromise<any> {
        contactPerson.categoryRecords = []
        contactPerson.categoryIds.forEach(id => {
            contactPerson.categoryRecords.push({ categoryId: id, default: false })
        })
        return this.httpService.post(Constants.WEBAPI_CORE_CONTACT_PERSON, contactPerson);
    }

    deleteContactPersons(contactPersonIds: number[]): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_CORE_CONTACT_PERSON_DELETE, contactPersonIds);
    }

    // DELETE
    deleteContactPerson(contactPersonId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_CORE_CONTACT_PERSON + contactPersonId);
    }

}
