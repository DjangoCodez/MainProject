import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { EmailTemplateType } from '@shared/models/generated-interfaces/Enumerations';
import { IEmailTemplateDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteEmailTemplate,
  getEmailTemplate,
  getEmailTemplates,
  saveEmailTemplate,
} from '@shared/services/generated-service-endpoints/core/EmailTemplates.endpoints';
import { map, Observable, take, tap } from 'rxjs';
import { EmailTemplateDTO } from '../models/email-template.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class EmailTemplateService {
  constructor(
    private http: SoeHttpClient,
    private translate: TranslateService
  ) {}

  getGrid(id?: number): Observable<IEmailTemplateDTO[]> {
    return this.http
      .get<IEmailTemplateDTO[]>(getEmailTemplates(id))
      .pipe(tap(templates => this.mapEmailTemplateTypeToTypenames(templates)));
  }

  get(id: number): Observable<IEmailTemplateDTO> {
    return this.http.get<IEmailTemplateDTO>(getEmailTemplate(id));
  }

  save(model: EmailTemplateDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveEmailTemplate(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteEmailTemplate(id));
  }

  loadEmailTemplateTypes(): Observable<ISmallGenericType[]> {
    return this.translate
      .get([
        'billing.purchase.list.purchase',
        'common.customer.invoices.reminder',
        'common.salestypes',
      ])
      .pipe(
        take(1),
        map((terms: Record<string, string>) => {
          return [
            { id: EmailTemplateType.Invoice, name: terms['common.salestypes'] },
            {
              id: EmailTemplateType.Reminder,
              name: terms['common.customer.invoices.reminder'],
            },
            {
              id: EmailTemplateType.PurchaseOrder,
              name: terms['billing.purchase.list.purchase'],
            },
          ];
        })
      );
  }

  private mapEmailTemplateTypeToTypenames(
    templates: IEmailTemplateDTO[]
  ): IEmailTemplateDTO[] {
    let typeList: ISmallGenericType[] = [];
    this.loadEmailTemplateTypes().subscribe(types => (typeList = types));
    templates.forEach(template => {
      template.typename =
        typeList.find(type => type.id === template.type)?.name ?? '';
    });
    return templates;
  }
}
