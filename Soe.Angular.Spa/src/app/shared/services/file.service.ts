import { Injectable } from '@angular/core';
import { SoeHttpClient } from './http.service';
import {
  deleteFileRecord,
  getFileRecords,
  uploadFileForRecord,
  getFileRecord,
  updateFileRecord,
  uploadFile,
  getFilesAsZip,
  sendDocumentsAsEmail,
  updateDataStorageFile,
} from './generated-service-endpoints/core/File.endpoints';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { FileRecordDTO } from '@shared/models/file.model';
import { DataStorageRecordDTO } from '@shared/models/data-storage.model';
import { FileUtility } from '@shared/util/file-util';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import { Observable } from 'rxjs';
import { IEmailDocumentsRequestDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Injectable({
  providedIn: 'root',
})
export class FileService {
  constructor(private http: SoeHttpClient) {}

  // GET

  getFileRecord(fileRecordId: number) {
    return this.http.get<DataStorageRecordDTO>(getFileRecord(fileRecordId));
  }

  getFileRecords(entity: number, recordId: number) {
    return this.http.get<FileRecordDTO[]>(getFileRecords(entity, recordId));
  }

  // POST

  uploadFileForRecord(
    entity: number,
    type: number,
    recordId: number,
    attachedFile: AttachedFile
  ) {
    return this.http.post<BackendResponse>(
      uploadFileForRecord(entity, type, recordId),
      this.getFormData(attachedFile)
    );
  }

  replaceFile(
    entity: number,
    type: number,
    dataStorageId: number,
    attachedFile: AttachedFile
  ) {
    return this.http.post<BackendResponse>(
      updateDataStorageFile(entity, type, dataStorageId),
      this.getFormData(attachedFile)
    );
  }

  uploadFile(entity: number, type: number, attachedFile: AttachedFile) {
    return this.http.post<BackendResponse>(
      uploadFile(entity, type),
      this.getFormData(attachedFile)
    );
  }

  updateFileRecord(fileRecord: FileRecordDTO) {
    return this.http.post<BackendResponse>(updateFileRecord(), fileRecord);
  }

  //   save(item: DocumentDTO, additionalData: string): Observable<any> {
  //     const model = new SaveDocumentModel(item, additionalData);
  //     return this.http.post<SaveDocumentModel>(saveDocument(), model);
  //   }

  deleteFileRecord(dataStorageRecordId: number) {
    return this.http.delete<BackendResponse>(
      deleteFileRecord(dataStorageRecordId)
    );
  }

  //Helpers
  private getFormData(file: AttachedFile): FormData {
    return FileUtility.createFormData(
      <string>file.name,
      <string>file.extension,
      <string>file.content,
      <Uint8Array>file.binaryContent
    );
  }

  //Get files as zip
  GetFileRecordsAsZip(
    ids: number[],
    prefixName: string
  ): Observable<IActionResult> {
    return this.http.post<IActionResult>(getFilesAsZip(), { ids, prefixName });
  }

  sendDocumentsAsEmail(model: IEmailDocumentsRequestDTO) {
    return this.http.post<IActionResult>(sendDocumentsAsEmail(), model)
  }
}
