import { Injectable } from '@angular/core';
import { IContactPersonGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service'
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, forkJoin, map, take } from 'rxjs';
import {
  ContactPersonGridDTO,
  ContactPersonDTO,
} from '../models/contact-persons.model';
import {
  deleteContactPerson,
  deleteContactPersons,
  getContactPerson,
  getContactPersonCategories,
  getContactPersonForExport,
  getContactPersons,
  getContactPersonsByActorId,
  getContactPersonsByActorIds,
  saveContactPerson,
} from '@shared/services/generated-service-endpoints/core/ContactPersons.endpoints';
import { TranslateService } from '@ngx-translate/core';
import { SoeCategoryType } from '@shared/models/generated-interfaces/Enumerations';

@Injectable({
  providedIn: 'root',
})
export class ContactPersonsService {
  constructor(
    private http: SoeHttpClient,
    private translate: TranslateService,
    private coreService: CoreService
  ) {}

  getGrid(id?: number): Observable<ContactPersonGridDTO[]> {
    const yesNoDict: string[] = [];
    this.translate
      .get(['core.yes', 'core.no'])
      .pipe(take(1))
      .subscribe(terms => {
        yesNoDict.push(terms['core.yes']);
        yesNoDict.push(terms['core.no']);
      });

    return this.http.get<IContactPersonGridDTO[]>(getContactPersons(id)).pipe(
      map(records =>
        records.map(x => {
          const result: ContactPersonGridDTO = {
            ...x,
            hasConsentId: +x.hasConsent,
          };
          return result;
        })
      )
    );
  }

  getContactPersonsByActorId(actorId: number): Observable<ContactPersonDTO[]> {
    return this.http.get<ContactPersonDTO[]>(
      getContactPersonsByActorId(actorId)
    );
  }

  getContactPersonsByActorIds(
    actorIds: string
  ): Observable<ContactPersonDTO[]> {
    return this.http.get<ContactPersonDTO[]>(
      getContactPersonsByActorIds(actorIds)
    );
  }

  get(id: number): Observable<ContactPersonDTO> {
    return forkJoin({
      contactPerson: this.http.get<ContactPersonDTO>(getContactPerson(id)),
      categoryRecord: this.getContactPersonCategories(id),
      categoryDict: this.coreService.getCategoriesDict(
        SoeCategoryType.ContactPerson,
        false
      ),
    }).pipe(
      map(
        ({ contactPerson, categoryRecord, categoryDict }): ContactPersonDTO => {
          let _categoryString = '';

          if (contactPerson) {
            contactPerson.categoryIds = categoryRecord;
            contactPerson.hasConsentId = +contactPerson.hasConsent;

            if (contactPerson.categoryIds.length > 0)
              contactPerson.categoryIds.forEach(element => {
                const catecoryName = categoryDict?.find((c: any) => {
                  return c.id == element;
                })?.name;
                _categoryString = _categoryString + catecoryName + ',';
              });
            contactPerson.categoryString = _categoryString?.slice(0, -1) + '';
          }
          return contactPerson;
        }
      )
    );
  }

  getContactPersonCategories(contactPersonId: number): Observable<any> {
    return this.http.get<any>(getContactPersonCategories(contactPersonId));
  }

  save(contactPerson: ContactPersonDTO): Observable<any> {
    contactPerson.categoryRecords = [];
    contactPerson.categoryIds.forEach(id => {
      contactPerson.categoryRecords.push({ categoryId: id, default: false });
    });
    return this.http.post<ContactPersonDTO>(saveContactPerson(), contactPerson);
  }

  getContactPersonForExport(actorId: number): Observable<any> {
    return this.http.get<any>(getContactPersonForExport(actorId));
  }

  deleteContactPersons(contactPersonIds: number[]): Observable<any> {
    return this.http.post<any>(deleteContactPersons(), contactPersonIds);
  }

  delete(contactPersonId: number): Observable<any> {
    return this.http.delete(deleteContactPerson(contactPersonId));
  }
}
