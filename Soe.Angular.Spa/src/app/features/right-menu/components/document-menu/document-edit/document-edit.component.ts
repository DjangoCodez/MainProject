import {
  Component,
  ElementRef,
  OnInit,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  DataStorageRecipientDTO,
  DocumentDTO,
} from '@shared/models/document.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DocumentService } from '@shared/services/document.service';
import { Perform } from '@shared/util/perform.class';
import { focusOnElement } from '@shared/util/focus-util';
import { Observable, Subject, tap } from 'rxjs';
import { DocumentForm } from '../models/document-form.model';
import { FileUploadDialogComponent } from '@shared/components/file-upload-dialog/file-upload-dialog.component';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { MessageGroupService } from '@features/time/time-schedule-events/services/message-group.service';

@Component({
  selector: 'soe-document-edit',
  templateUrl: './document-edit.component.html',
  styleUrls: ['./document-edit.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DocumentEditComponent
  extends EditBaseDirective<DocumentDTO, DocumentService, DocumentForm>
  implements OnInit
{
  service = inject(DocumentService);
  private readonly messageGroupService = inject(MessageGroupService);
  dialogService = inject(DialogService);

  performDocumentFolders = new Perform<string[]>(this.progressService);
  performMessageGroups = new Perform<ISmallGenericType[]>(this.progressService);
  performDocumentRecipientInfo = new Perform<DataStorageRecipientDTO[]>(
    this.progressService
  );

  setFileName: Subject<string> = new Subject();

  folders: SmallGenericType[] = [];
  messageGroups: SmallGenericType[] = [];
  recipientFilters: SmallGenericType[] = [];
  filteredRecipients = signal<DataStorageRecipientDTO[]>([]);

  @ViewChild('nameField') nameField!: ElementRef;

  // Flags
  private recipientsLoaded = false;

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Manage_Preferences_UploadedFiles, {
      lookups: [this.loadDocumentFolders(), this.loadMessageGroups()],
    });
  }

  // SERVICE CALLS

  loadDocumentFolders(): Observable<string[]> {
    return this.performDocumentFolders.load$(
      this.service.getDocumentFolders().pipe(
        tap(data => {
          let i = 0;
          this.folders.push(new SmallGenericType(i++, ''));
          data.forEach(folder => {
            this.folders.push(new SmallGenericType(i++, folder));
          });
        })
      )
    );
  }

  loadMessageGroups(): Observable<ISmallGenericType[]> {
    return this.performMessageGroups.load$(this.messageGroupService.getDict());
  }

  getDocumentRecipientInfo(): Observable<DataStorageRecipientDTO[]> {
    return this.performDocumentRecipientInfo.load$(
      this.service.getDocumentRecipientInfo(this.form?.getIdControl()?.value)
    );
  }

  // EVENTS

  openUploadFileDialog() {
    const fileDialog = this.dialogService.open(FileUploadDialogComponent, {
      size: 'lg',
      title: 'core.document.choosefile',
      asBinary: false,
      multipleFiles: false,
      closeOnAttach: true,
      hideFooter: true,
    });

    fileDialog.afterClosed().subscribe((file: AttachedFile) => {
      this.form?.patchValue({
        fileName: file.name,
        fileString: file.content,
        fileSize: file.size,
        extension: file.extension,
      });
      this.additionalSaveData = file.content;

      if (!this.form?.value.name && file.name) {
        const extensionLength = file.extension?.length || 0;
        this.form?.patchValue({
          name: file.name.substring(0, file.name.length - extensionLength),
        });
      }

      // Needed since filename is disabled
      this.form?.markAsDirty();

      focusOnElement((<any>this.nameField).inputER.nativeElement);
    });
  }

  downloadFile() {
    this.service
      .getDocumentUrl(this.form?.getIdControl()?.value)
      .subscribe(url => {
        if (url) window.location.assign(url);
        else
          console.log(
            'File not found for DataStorageId',
            this.form?.getIdControl()?.value
          );
      });
  }

  selectedFolderChanged(index: number) {
    this.form?.patchValue({
      folder: this.folders.find(f => f.id === index)?.name,
      selectedFolder: undefined,
    });
  }

  needsConfirmationChanged() {
    if (this.recipientsLoaded) this.setupRecipientFilter();
  }

  showRecipients() {
    if (!this.recipientsLoaded)
      this.getDocumentRecipientInfo().subscribe(() => {
        this.setupRecipientFilter();
        this.recipientsLoaded = true;
        this.setFilteredRecipients();
      });
  }

  filteredRecipientsChanged(selectedId: number) {
    this.form?.patchValue({
      selectedRecipientFilter: selectedId,
    });

    this.setFilteredRecipients();
  }

  // HELP-METHODS

  private setupRecipientFilter() {
    this.recipientFilters = [];

    this.recipientFilters.push({
      id: RecipientFilter.All,
      name: this.translate.instant('core.document.recipientfilter.all'),
    });
    this.recipientFilters.push({
      id: RecipientFilter.Unread,
      name: this.translate.instant('core.document.recipientfilter.unread'),
    });
    if (this.needsConfirmation) {
      this.recipientFilters.push({
        id: RecipientFilter.ReadNotConfirmed,
        name: this.translate.instant(
          'core.document.recipientfilter.readnotconfirmed'
        ),
      });
      this.recipientFilters.push({
        id: RecipientFilter.Confirmed,
        name: this.translate.instant('core.document.recipientfilter.confirmed'),
      });
    } else {
      this.recipientFilters.push({
        id: RecipientFilter.Read,
        name: this.translate.instant('core.document.recipientfilter.read'),
      });
    }

    this.form?.controls.selectedRecipientFilter.patchValue(
      this.recipientFilters.find(f => f.id === RecipientFilter.All)
    );
  }

  private setFilteredRecipients() {
    let records = this.performDocumentRecipientInfo.data || [];

    switch (this.form?.controls.selectedRecipientFilter.value) {
      case RecipientFilter.All:
        break;
      case RecipientFilter.Unread:
        records = records.filter(r => !r.readDate);
        break;
      case RecipientFilter.Read:
        records = records.filter(r => r.readDate);
        break;
      case RecipientFilter.ReadNotConfirmed:
        records = records.filter(r => r.readDate && !r.confirmedDate);
        break;
      case RecipientFilter.Confirmed:
        records = records.filter(r => r.readDate && r.confirmedDate);
    }

    this.filteredRecipients.set(records);
  }

  get needsConfirmation(): boolean {
    return this.form?.controls.needsConfirmation.value;
  }
}

export enum RecipientFilter {
  All = 0,
  Unread = 1,
  Read = 2,
  ReadNotConfirmed = 3,
  Confirmed = 4,
}
