import { inject, Injectable } from '@angular/core';
import { forkJoin, map, Observable, take } from 'rxjs';
import {
  getBankintegrationRequestFiles,
  getBankintegrationRequestGrid,
  searchBankintegrationRequest,
} from '@shared/services/generated-service-endpoints/manage/System.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { ISoeBankerDownloadFileDTO } from '@shared/models/generated-interfaces/BankIntegrationDTOs';
import { SoeBankerRequestFilterDTO } from '../../../models/bankintegration.model';
import { ISoeBankerDownloadRequestGridDTO } from '../models/bankintegration-downloadrequest-grid.models';
import { TranslateService } from '@ngx-translate/core';
import { TermCollection } from '@shared/localization/term-types';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { orderBy } from 'lodash';

@Injectable({
  providedIn: 'root',
})
export class BankintegrationDownloadRequestService {
  private readonly http = inject(SoeHttpClient);
  private readonly translate = inject(TranslateService);

  errorMsgStatus: ISmallGenericType[] = [
    { id: 96, name: 'FileParseError' },
    { id: 98, name: 'AvaloError' },
    { id: 99, name: 'SoftoneError' },
  ];

  private statusWithOutGrodErrorMsg: ISmallGenericType[] = [
    { id: 1, name: 'Received' },
    { id: 5, name: 'Polling' },
    { id: 10, name: 'Transfered' },
    { id: 11, name: 'Downloaded' },
    { id: 20, name: 'Completed' },
    { id: 95, name: 'DownloadError' },
    { id: 97, name: 'BankError' },
  ];

  statuses: ISmallGenericType[] = orderBy(
    [...this.statusWithOutGrodErrorMsg, ...this.errorMsgStatus],
    ['id']
  );

  getGridAdditionalProps = {
    type: 0,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      type: number;
    }
  ): Observable<ISoeBankerDownloadRequestGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return forkJoin([
      this.getStatusMessages(),
      this.http.get<ISoeBankerDownloadRequestGridDTO[]>(
        getBankintegrationRequestGrid(this.getGridAdditionalProps.type)
      ),
    ]).pipe(
      map(
        ([statusMsgs, downloadRequests]): ISoeBankerDownloadRequestGridDTO[] =>
          this.mapStatusMessages(statusMsgs, downloadRequests) // Map status messages
      )
    );
  }

  getFiles(requestId: number): Observable<ISoeBankerDownloadFileDTO[]> {
    return this.http.get<ISoeBankerDownloadFileDTO[]>(
      getBankintegrationRequestFiles(requestId)
    );
  }

  search(
    filter: SoeBankerRequestFilterDTO
  ): Observable<ISoeBankerDownloadRequestGridDTO[]> {
    return forkJoin([
      this.getStatusMessages(),
      this.http.post<ISoeBankerDownloadRequestGridDTO[]>(
        searchBankintegrationRequest(),
        filter
      ),
    ]).pipe(
      map(
        ([statusMsgs, downloadRequests]): ISoeBankerDownloadRequestGridDTO[] =>
          this.mapStatusMessages(statusMsgs, downloadRequests) // Map status messages
      )
    );
  }

  public showStatusMessage(statusCode: number): boolean {
    return (
      // FileParseError, AvaloError, SoftoneError
      (statusCode === 96 || statusCode === 98 || statusCode === 99)
    );
  }

  private mapStatusMessages(
    statusMessages: TermCollection,
    downloadRequests: ISoeBankerDownloadRequestGridDTO[]
  ): ISoeBankerDownloadRequestGridDTO[] {
    downloadRequests.map(request => {
      switch (request.statusCode) {
        case 96: //FileParseError
          request.statusMessage =
            statusMessages[
              'manage.system.bankintegration.downloadrequest.fileparsererror.warningmsg'
            ];
          break;
        case 98: //AvaloError
          request.statusMessage =
            statusMessages[
              'manage.system.bankintegration.downloadrequest.avaloerror.warningmsg'
            ];
          break;
        case 99: //SoftoneError
          request.statusMessage =
            statusMessages[
              'manage.system.bankintegration.downloadrequest.softonerror.warningmsg'
            ];
          break;
      }
      return request;
    });
    return downloadRequests;
  }

  private getStatusMessages(): Observable<TermCollection> {
    return this.translate
      .get([
        'manage.system.bankintegration.downloadrequest.softonerror.warningmsg',
        'manage.system.bankintegration.downloadrequest.avaloerror.warningmsg',
        'manage.system.bankintegration.downloadrequest.fileparsererror.warningmsg',
      ])
      .pipe(take(1));
  }
}
