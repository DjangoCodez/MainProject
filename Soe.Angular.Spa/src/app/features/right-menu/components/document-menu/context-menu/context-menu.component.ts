import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DocumentDTO } from '@shared/models/document.model';

export enum DocumentMenuContextMenuOption {
  NewDocument,
  EditDocument,
  ViewDocument,
  DownloadDocument,
  ViewMessage,
  SendConfirmation,
}

export type DocumentMenuContextMenuItemSelected = {
  document: DocumentDTO;
  option: DocumentMenuContextMenuOption;
};

@Component({
  selector: 'document-menu-context-menu',
  templateUrl: './context-menu.component.html',
  styleUrls: [
    '../../../../../shared/styles/shared-styles/shared-context-menu-styles.scss',
    './context-menu.component.scss',
  ],
  standalone: false,
})
export class ContextMenuComponent {
  @Input() document!: DocumentDTO;
  @Output() menuSelected =
    new EventEmitter<DocumentMenuContextMenuItemSelected>();

  readonly DocumentMenuContextMenuOption = DocumentMenuContextMenuOption;

  onMenuSelected(option: DocumentMenuContextMenuOption) {
    this.menuSelected.emit({ document: this.document, option: option });
  }
}
