import { Injector, inject, signal, effect } from '@angular/core';
import { SoeFormControl } from '@shared/extensions';
import {
  Feature,
  SoeDataStorageRecordType,
  SoeEntityType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import { FileService } from '@shared/services/file.service';
import { Perform } from '@shared/util/perform.class';
import { FileRecordDTO } from '@shared/models/file.model';
import { DownloadUtility } from '@shared/util/download-util';
import { FileUtility } from '@shared/util/file-util';
import { TranslateService } from '@ngx-translate/core';
import { CrudActionTypeEnum } from '@shared/enums';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import { ProgressService } from '@shared/services/progress';
import { IEmailDocumentsRequestDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class FilesHelper extends ValueAccessorDirective<any> {
  fileSaved = signal<AttachedFile>(new AttachedFile());
  loadingFiles = false;
  filesLoaded = signal(false);
  loadedForRecordId = 0;
  recordId = signal(0);
  nbrOfFilesStr = signal('*');

  fileRecords = new BehaviorSubject<FileRecordDTO[]>([]);
  get files() {
    return this.fileRecords.value;
  }

  get isReadOnly() {
    return this.entity === SoeEntityType.CustomerCentral; // Add project later
  }

  get useExtended() {
    return this.entity === SoeEntityType.CustomerCentral; // Add project later
  }

  coreService = inject(CoreService);
  fileService = inject(FileService);
  toasterService = inject(ToasterService);
  translate = inject(TranslateService);
  progressService = inject(ProgressService);

  constructor(
    private setDirtyOnUpload: boolean,
    private entity: SoeEntityType,
    private type: SoeDataStorageRecordType,
    private uploadPermission: Feature,
    private performLoad: Perform<any>,
    private defaultUploadTerm?: string,
    private window?: Window,
    private extendedPermissions?: any
  ) {
    super(inject(Injector));
    this.control = new SoeFormControl(0);

    this.fileRecords.subscribe(files => {
      if (this.filesLoaded() && files) {
        this.nbrOfFilesStr.set(files.length.toString());
      } else {
        this.nbrOfFilesStr.set('*');
      }
    });

    effect(() => {
      //Trigger automatic reload of files when recordId changes
      if (this.recordId() && this.filesLoaded()) {
        this.loadFiles().subscribe();
      }
    });
  }

  loadFilesOnOpen(opened: boolean) {
    if (opened) this.loadFiles(true, true).subscribe();
  }

  loadFiles(
    reload: boolean = false,
    showToasterLoaded: boolean = false
  ): Observable<any> {
    if (
      !reload &&
      this.filesLoaded() &&
      this.recordId() == this.loadedForRecordId
    ) {
      return of(null);
    }
    this.loadedForRecordId = this.recordId();

    if (this.filesLoaded() && reload) {
      this.filesLoaded.set(false);
    }

    this.loadingFiles = true;
    this.fileRecords.next([]);

    return this.performLoad.load$(
      this.fileService.getFileRecords(this.entity, this.recordId()).pipe(
        tap(records => {
          this.fileRecords.next(records);
          this.filesLoaded.set(true);
          this.loadingFiles = false;

          if (showToasterLoaded) {
            this.toasterService.success(
              this.translate.instant('common.document.loaded'),
              ''
            );
          }
        })
      )
    );
  }

  private toFileRecord(
    attachedFile: AttachedFile,
    saveResponse: BackendResponse
  ) {
    const fileRecord = new FileRecordDTO();
    fileRecord.fileName = attachedFile.name || '';
    fileRecord.description = attachedFile.name || '';
    fileRecord.created = new Date();
    fileRecord.extension = attachedFile.extension || '';
    fileRecord.isModified = true;
    fileRecord.fileSize = attachedFile.size;
    fileRecord.type = this.type;
    fileRecord.entity = this.entity;
    fileRecord.recordId = this.recordId();
    fileRecord.fileRecordId = ResponseUtil.getEntityId(saveResponse);
    fileRecord.fileId = ResponseUtil.getNumberValue(saveResponse);
    return fileRecord;
  }

  public saveFile(attachedFile: AttachedFile) {
    return this.fileService
      .uploadFileForRecord(
        this.entity,
        this.type,
        this.recordId(),
        attachedFile
      )
      .pipe(
        tap(result => {
          if (result.success) {
            this.fileRecords.next([
              ...this.fileRecords.value,
              this.toFileRecord(attachedFile, result),
            ]);
          }
        })
      );
  }

  public replaceFile(attachedFile: AttachedFile, dataStorageId: number) {
    return this.fileService
      .replaceFile(this.entity, this.type, dataStorageId, attachedFile)
      .pipe(
        tap(result => {
          if (result.success) {
            this.loadFiles(true).subscribe();
          }
        })
      );
  }

  public delete(fileRecord: FileRecordDTO) {
    this.performLoad.crud(
      CrudActionTypeEnum.Delete,
      this.fileService.deleteFileRecord(fileRecord.fileRecordId),
      () => {
        this.fileRecords.next(
          this.fileRecords.value.filter(
            x => x.fileRecordId !== fileRecord.fileRecordId
          )
        );
      },
      undefined,
      {
        showToastOnError: true,
        showToastOnComplete: true,
      }
    );
  }

  public update(fileRecord: FileRecordDTO, field: string) {
    this.setFileRecordAsModified(fileRecord);
    this.performLoad.crud(
      CrudActionTypeEnum.Save,
      this.fileService.updateFileRecord(fileRecord),
      undefined,
      undefined,
      {
        showToastOnError: true,
        showToastOnComplete: true,
      }
    );
  }

  public downloadFile(fileRecord: FileRecordDTO) {
    return this.fileService.getFileRecord(fileRecord.fileRecordId).pipe(
      map((fileRecord: any) => {
        return {
          type: FileUtility.getTypeFromExtension(fileRecord.extension),
          data: fileRecord.data,
          fileName: fileRecord.fileName,
        };
      })
    );
  }

  public download(fileRecord: FileRecordDTO) {
    this.performLoad
      .load$(
        this.downloadFile(fileRecord).pipe(
          map(file => {
            DownloadUtility.downloadFile(file.fileName, file.type, file.data);
          })
        )
      )
      .subscribe();
  }

  private setFileRecordAsModified(fileRecord: FileRecordDTO) {
    const fRecord = this.fileRecords.value.find(
      record => record.fileRecordId == fileRecord.fileRecordId
    );
    if (fRecord) {
      fRecord.isModified = true;
    }
  }

  public reset() {
    this.fileRecords.next([]);
    this.filesLoaded.set(false);
  }

  public getUploadPermission() {
    return this.uploadPermission;
  }


  public addExtendedPermission(permissions: Feature[]) {
    this.extendedPermissions = permissions;
  }

  public hasExtendedPermission(permission: Feature): boolean {
    return this.extendedPermissions?.includes(permission);
  }

  public downloadFilesAsZip(ids: number[], prefixName: string) {
    this.performLoad
      .load$(
        this.fileService.GetFileRecordsAsZip(ids, prefixName).pipe(
          map((res: IActionResult) => {
            if (res.success) {
              const base64Data =
                typeof res.value === 'string' ? res.value : res.value?.$value;
              DownloadUtility.downloadFile(
                res.stringValue,
                'application/zip',
                base64Data.trim()
              );
            } else if (!res.success) {
              this.progressService.saveError({
                title: 'core.error',
                message:
                  ResponseUtil.getErrorMessage(res as any) ||
                  'Download failed.',
              });
            }
          })
        )
      )
      .subscribe();
  }

  public sendFilesAsEmail(model: IEmailDocumentsRequestDTO) {
    return this.performLoad.load$(this.fileService.sendDocumentsAsEmail(model));
  }
}
