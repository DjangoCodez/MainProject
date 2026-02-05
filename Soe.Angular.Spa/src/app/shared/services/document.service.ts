import { Injectable } from '@angular/core';
import { CACHE_EXPIRE_SHORT, SoeHttpClient } from './http.service';
import {
  deleteDocument,
  getCompanyDocuments,
  getDocument,
  getDocumentData,
  getDocumentFolders,
  getDocumentRecipientInfo,
  getDocumentUrl,
  getMyDocuments,
  getNbrOfUnreadCompanyDocuments,
  hasNewDocuments,
  saveDocument,
  setDocumentAsRead,
} from './generated-service-endpoints/core/Document.endpoints';
import { Observable, map } from 'rxjs';
import {
  DataStorageRecipientDTO,
  DocumentDTO,
  SaveDocumentModel,
} from '@shared/models/document.model';

@Injectable({
  providedIn: 'root',
})
export class DocumentService {
  constructor(private http: SoeHttpClient) {}

  // GET

  hasNewDocuments(time: string): Observable<boolean> {
    return this.http.get<boolean>(hasNewDocuments(time));
  }

  getCompanyDocuments(): Observable<DocumentDTO[]> {
    return this.http.get<DocumentDTO[]>(getCompanyDocuments()).pipe(
      map(docs =>
        docs.map(doc => {
          const obj = new DocumentDTO();
          Object.assign(obj, doc);
          return obj;
        })
      )
    );
  }

  getMyDocuments(): Observable<DocumentDTO[]> {
    return this.http.get<DocumentDTO[]>(getMyDocuments()).pipe(
      map(docs =>
        docs.map(doc => {
          const obj = new DocumentDTO();
          Object.assign(obj, doc);
          return obj;
        })
      )
    );
  }

  getNbrOfUnreadCompanyDocuments(useCache: boolean): Observable<number> {
    return this.http.get<number>(getNbrOfUnreadCompanyDocuments(), {
      useCache: useCache,
      cacheOptions: { expires: CACHE_EXPIRE_SHORT },
    });
  }

  get(id: number): Observable<DocumentDTO> {
    return this.http.get<DocumentDTO>(getDocument(id));
  }

  getDocumentUrl(dataStorageId: number): Observable<string> {
    return this.http.get<string>(getDocumentUrl(dataStorageId));
  }

  getDocumentData(dataStorageId: number): Observable<any> {
    return this.http.get<any>(getDocumentData(dataStorageId));
  }

  getDocumentFolders(): Observable<string[]> {
    return this.http.get<string[]>(getDocumentFolders());
  }

  getDocumentRecipientInfo(
    dataStorageId: number
  ): Observable<DataStorageRecipientDTO[]> {
    return this.http.get<DataStorageRecipientDTO[]>(
      getDocumentRecipientInfo(dataStorageId)
    );
  }

  // POST

  save(item: DocumentDTO, additionalData: string): Observable<any> {
    const model = new SaveDocumentModel(item, additionalData);
    return this.http.post<SaveDocumentModel>(saveDocument(), model);
  }

  setDocumentAsRead(
    dataStorageId: number,
    confirmed: boolean
  ): Observable<any> {
    return this.http.post(setDocumentAsRead(), {
      dataStorageId: dataStorageId,
      confirmed: confirmed,
    });
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteDocument(id));
  }
}
