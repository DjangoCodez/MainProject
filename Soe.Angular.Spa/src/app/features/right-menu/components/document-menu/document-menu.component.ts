import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslateService } from '@ngx-translate/core';
import { ValidationHandler } from '@shared/handlers';
import { TermCollection } from '@shared/localization/term-types';
import { DocumentDTO, DocumentFolder } from '@shared/models/document.model';
import {
  AngularJsLegacyType,
  Feature,
  XEMailAnswerType,
  XEMailType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { DocumentService } from '@shared/services/document.service';
import { StorageService } from '@shared/services/storage.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { StringUtil } from '@shared/util/string-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { EditComponentDialogData } from '@ui/dialog/edit-component-dialog/edit-component-dialog.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { Observable, forkJoin, mergeMap, of, take, tap, timer } from 'rxjs';
import { RightMenuBaseComponent } from '../right-menu-base/right-menu-base.component';
import {
  DocumentMenuContextMenuItemSelected,
  DocumentMenuContextMenuOption,
} from './context-menu/context-menu.component';
import { DocumentEditComponent } from './document-edit/document-edit.component';
import { DocumentForm } from './models/document-form.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

export enum DocumentMenuTabType {
  General,
  Employee,
}

export type DocumentMenuTab = {
  type: DocumentMenuTabType;
  title: string;
  selected: boolean;
};

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'document-menu',
  templateUrl: './document-menu.component.html',
  styleUrls: ['./document-menu.component.scss'],
  standalone: false,
})
export class DocumentMenuComponent
  extends RightMenuBaseComponent
  implements OnInit
{
  readonly translate = inject(TranslateService);
  readonly storageService = inject(StorageService);
  readonly coreService = inject(CoreService);
  readonly documentService = inject(DocumentService);
  readonly messageboxService = inject(MessageboxService);
  readonly dialogService = inject(DialogService);

  destroyRef = inject(DestroyRef);
  validationHandler = inject(ValidationHandler);

  private terms: TermCollection = {};

  modifyPermission = signal(false);

  nbrOfUnreadCompanyDocuments = 0;
  folders: DocumentFolder[] = [];
  documents: DocumentDTO[] = [];
  selectedDocument: DocumentDTO | undefined;
  pdf: any;
  image: any;

  loadingDocuments = false;

  tabs: DocumentMenuTab[] = [];
  selectedTab: DocumentMenuTab | undefined;

  // INIT
  ngOnInit() {
    // Call service every 10 minutes
    timer(0, 60 * 1000 * 10)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        mergeMap(() => this.loadNbrOfUnreadDocuments(true))
      )
      .subscribe();
  }

  private init() {
    forkJoin([this.loadTerms(), this.loadModifyPermissions()]).subscribe(() => {
      this.setupTabs();
    });
  }

  // SETUP
  private setupTabs() {
    this.tabs = [];
    this.tabs.push({
      type: DocumentMenuTabType.General,
      title: this.terms['core.documentmenu.type.general'],
      selected: true,
    });
    this.tabs.push({
      type: DocumentMenuTabType.Employee,
      title: this.terms['core.documentmenu.type.employee'],
      selected: false,
    });
    this.selectTab(this.tabs[0], this.showMenu);
  }

  // SERVICE CALLS
  private loadTerms(): Observable<any> {
    return this.translate
      .get([
        'core.documentmenu.type.general',
        'core.documentmenu.type.employee',
        'core.document.view',
        'core.document.new',
        'core.document.edit',
        'core.document.downloadfile',
        'core.document.viewmessage',
        'core.documentmenu.nofolder',
        'common.messages.sendconfirmation',
      ])
      .pipe(
        tap(terms => {
          this.terms = terms;
        })
      );
  }

  private loadModifyPermissions(): Observable<any> {
    return this.coreService
      .hasModifyPermissions([Feature.Manage_Preferences_UploadedFiles])
      .pipe(
        tap(x => {
          this.modifyPermission.set(
            x[Feature.Manage_Preferences_UploadedFiles]
          );
          return of();
        })
      );
  }

  private hasNewDocuments(useCache: boolean): Observable<boolean> {
    return new Observable<boolean>(observer => {
      if (useCache) {
        // Get last time checked from local storage
        let time = this.storageService.get('hasNewDocuments');
        if (!time) time = DateUtil.defaultDateTime().toDateTimeString();
        this.documentService
          .hasNewDocuments(time)
          .pipe(take(1))
          .subscribe(result => {
            // Update last time checked in local storage
            this.storageService.set(
              'hasNewDocuments',
              new Date().toDateTimeString()
            );
            observer.next(result);
          });
      } else {
        // Force check
        observer.next(true);
      }
    });
  }

  private loadNbrOfUnreadDocuments(useCache: boolean) {
    const ret$ = new Observable<boolean>(observer => {
      this.hasNewDocuments(useCache)
        .pipe(take(1))
        .subscribe(hasNewDocument => {
          // If new information exists, do not use cache to get number of unread informations
          this.documentService
            .getNbrOfUnreadCompanyDocuments(!hasNewDocument)
            .pipe(take(1))
            .subscribe(data => {
              this.nbrOfUnreadCompanyDocuments = data;

              // Set default tooltip
              this.setToggleTooltip('core.documentmenu.title');
              // If unread messages, add number to tooltip
              if (this.nbrOfUnreadCompanyDocuments > 0) {
                const term = this.translate.instant('core.documentmenu.unread');
                this.toggleTooltip += ` (${
                  this.nbrOfUnreadCompanyDocuments
                } ${term.toLocaleLowerCase()})`;
              }
              observer.next(hasNewDocument);
            });
        });
    });
    return ret$;
  }

  loadDocuments() {
    if (this.loadingDocuments) return;

    this.loadingDocuments = true;

    switch (this.selectedTab?.type) {
      case DocumentMenuTabType.General:
        this.documentService
          .getCompanyDocuments()
          .pipe(take(1))
          .subscribe(x => {
            this.documents = x;

            this.documents.forEach(document => {
              const recipient = document.recipients.find(
                r => r.userId === SoeConfigUtil.userId
              );
              if (recipient) {
                document.readDate = recipient.readDate;
                document.answerDate = recipient.confirmedDate;
                document.answerType = document.answerDate
                  ? XEMailAnswerType.Yes
                  : XEMailAnswerType.No;
              }
            });

            this.documentsLoaded();
          });
        break;
      case DocumentMenuTabType.Employee:
        this.documentService
          .getMyDocuments()
          .pipe(take(1))
          .subscribe(x => {
            this.documents = x;
            this.documentsLoaded();
          });
        break;
      default:
        this.loadingDocuments = false;
    }
  }

  private setDocumentAsRead(document: DocumentDTO) {
    if (document.readDate) return;

    this.documentService
      .setDocumentAsRead(document.dataStorageId, false)
      .pipe(take(1))
      .subscribe(result => {
        if (result.success) {
          this.loadNbrOfUnreadDocuments(false);

          document.readDate = DateUtil.parseDateOrJson(result.dateTimeValue);
          const folder = this.folders.find(f => f.name === document.folder);
          if (folder && folder.nbrOfUnread > 0) folder.nbrOfUnread--;
        }
      });
  }

  private setDocumentAsConfirmed(document: DocumentDTO) {
    if (!document.readDate || document.answerDate) return;

    this.documentService
      .setDocumentAsRead(document.dataStorageId, true)
      .pipe(take(1))
      .subscribe(result => {
        if (result.success && this.isGeneralTab) {
          document.answerDate = DateUtil.parseDateOrJson(result.dateTimeValue);
          document.answerType = XEMailAnswerType.Yes;
        }
      });
  }

  // ACTIONS
  protected toggleShowMenu() {
    super.toggleShowMenu();

    if (this.showMenu && this.tabs.length === 0) this.init();
  }

  selectTab(tab: DocumentMenuTab, loadDocuments = true) {
    if (this.selectedTab?.type === tab.type || this.loadingDocuments) return;

    this.tabs.forEach(p => {
      p.selected = false;
    });
    tab.selected = true;

    this.selectedTab = tab;

    if (loadDocuments) this.loadDocuments();
  }

  toggleFolder(folder: DocumentFolder) {
    folder.expanded = !folder.expanded;
  }

  viewFile(document: DocumentDTO) {
    if (!document.canViewDocument) return;

    if (!this.fullscreen) this.toggleFullscreen();

    this.selectedDocument = document;

    this.documentService
      .getDocumentData(document.dataStorageId)
      .pipe(take(1))
      .subscribe(data => {
        if (data) this.setDocumentAsRead(document);

        this.pdf = document.isPdf ? data : undefined;
        this.image = document.isImage ? data : undefined;
      });
  }

  downloadFile(document: DocumentDTO) {
    this.documentService
      .getDocumentUrl(document.dataStorageId)
      .pipe(take(1))
      .subscribe(url => {
        if (url) {
          BrowserUtil.downloadFile(url);
          this.setDocumentAsRead(document);
        } else {
          this.messageboxService.error(
            'core.document.filenotfound.title',
            this.translate
              .instant('core.document.filenotfound.message')
              .format(document.displayName)
          );
        }
      });
  }

  viewMessage(document: DocumentDTO) {
    // TODO: Migrate
    // This will open the message menu in legacy mode and then open the message
    if (document) {
      super.openLegacyMenu(AngularJsLegacyType.RightMenu_Message, {
        type: XEMailType.Incoming,
        title: document.description,
        id: document.messageId,
      });
    }
  }

  edit(document?: DocumentDTO) {
    if (!this.modifyPermission) return;

    const dialogData: EditComponentDialogData<
      DocumentDTO,
      DocumentService,
      DocumentForm
    > = {
      title: this.translate.instant(
        document ? 'core.document.edit' : 'core.document.new'
      ),
      size: 'lg',
      noToolbar: true,
      hideFooter: true,
      form: new DocumentForm({
        validationHandler: this.validationHandler,
        element: document,
      }),
      editComponent: DocumentEditComponent,
    };
    this.dialogService
      .openEditComponent(dialogData)
      .afterClosed()
      .subscribe(
        (result: { response: BackendResponse; value: DocumentDTO }) => {
          if (result?.response?.success) this.loadDocuments();
        }
      );
  }

  onMenuSelected(event: DocumentMenuContextMenuItemSelected) {
    switch (event.option) {
      case DocumentMenuContextMenuOption.NewDocument:
        this.edit();
        break;
      case DocumentMenuContextMenuOption.EditDocument:
        this.edit(event.document);
        break;
      case DocumentMenuContextMenuOption.ViewDocument:
        this.viewFile(event.document);
        break;
      case DocumentMenuContextMenuOption.DownloadDocument:
        this.downloadFile(event.document);
        break;
      case DocumentMenuContextMenuOption.ViewMessage:
        this.viewMessage(event.document);
        break;
      case DocumentMenuContextMenuOption.SendConfirmation:
        this.setDocumentAsConfirmed(event.document);
        break;
    }
  }

  openLegacyMenu(): void {
    super.openLegacyMenu(AngularJsLegacyType.RightMenu_Document);
  }

  // HELP-METHODS

  private documentsLoaded() {
    this.createFolders();
    this.loadingDocuments = false;
  }

  private createFolders() {
    // Create distinct collection of folder names from loaded documents

    // Set folder name on documents without folder
    this.documents
      .filter(d => !d.folder)
      .forEach(d => (d.folder = this.terms['core.documentmenu.nofolder']));

    const folderNames = this.documents
      .map(d => d.folder)
      .filter(StringUtil.uniqueFilter)
      .sort(StringUtil.sortLocale);

    this.folders = [];
    folderNames.forEach(folderName => {
      const folder = new DocumentFolder(folderName);
      folder.expanded = this.documents.length < 11; // If folder contains max 10 documents, expand it as default
      folder.nbrOfUnread = this.nbrOfUnreadDocumentsInFolder(folderName);
      this.folders.push(folder);
    });
  }

  documentsInFolder(folderName: string): DocumentDTO[] {
    return this.documents.filter(f => f.folder === folderName);
  }

  nbrOfUnreadDocumentsInFolder(folderName: string): number {
    return this.documentsInFolder(folderName).filter(i => !i.readDate).length;
  }

  get isGeneralTab(): boolean {
    return this.selectedTab?.type === DocumentMenuTabType.General;
  }

  get isEmployeeType(): boolean {
    return this.selectedTab?.type === DocumentMenuTabType.Employee;
  }

  getDocumentToolTip(document: DocumentDTO): string {
    return document.created
      ? `${this.translate.instant(
          'common.created'
        )} ${document.created.toFormattedDate()}`
      : '';
  }

  getConfirmedTooltip(document: DocumentDTO): string {
    return document.answerDate
      ? `${this.translate.instant(
          'common.messages.confirmed'
        )} ${document.answerDate.toFormattedDateTime()}`
      : this.translate.instant('common.messages.notconfirmed');
  }
}
