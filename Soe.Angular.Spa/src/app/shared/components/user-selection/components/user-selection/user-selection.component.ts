import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  effect,
  inject,
  input,
  model,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { UserSelectionType } from '@shared/models/generated-interfaces/Enumerations';
import { IUserSelectionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SelectionCollection } from '@shared/models/selection.model';
import { SelectComponent } from '@ui/forms/select/select.component';
import { UserSelectionForm } from '../../models/user-selection-form.model';
import { ValidationHandler } from '@shared/handlers';
import { UserSelectionFormDTO } from '../../models/user-selection.model';
import { UserSelectionService } from '../../services/user-selection.service';
import { forkJoin, map, Observable, tap } from 'rxjs';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';
import { TranslateService } from '@ngx-translate/core';
import {
  SaveUserSelectionDialogComponent,
  SaveUserSelectionDialogData,
  SaveUserSelectionDialogResult,
} from '../save-user-selection-dialog/save-user-selection-dialog.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ResponseUtil } from '@shared/util/response-util';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { TermCollection } from '@shared/localization/term-types';

@Component({
  selector: 'user-selection',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    IconButtonComponent,
    SelectComponent,
  ],
  templateUrl: './user-selection.component.html',
  styleUrl: './user-selection.component.scss',
})
export class UserSelectionComponent implements OnInit {
  labelKey = input<string>('');
  selectionType = input<UserSelectionType>(UserSelectionType.Unknown);
  selections = input<SelectionCollection>();
  selectedUserSelectionId = model<number>(0);
  savePublicPermission = input<boolean>(false);

  onSelectionsLoaded = output<SmallGenericType[]>();
  onSelected = output<IUserSelectionDTO | undefined>();

  // Form
  validationHandler = inject(ValidationHandler);
  form: UserSelectionForm = new UserSelectionForm({
    validationHandler: this.validationHandler,
    element: new UserSelectionFormDTO(),
  });

  // Services
  private readonly service = inject(UserSelectionService);
  private readonly dialogService = inject(DialogService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly translate = inject(TranslateService);

  // Terms
  private terms: TermCollection = {};

  // Properties
  selectionIsPrivate = computed<boolean>(() => {
    return !!this.selectedUserSelection()?.userId;
  });

  canSaveSelection = computed<boolean>(() => {
    return (
      this.savePublicPermission() ||
      this.selectionIsPrivate() ||
      !this.selectedUserSelectionId()
    );
  });

  canCopySelection = computed<boolean>(() => {
    return this.selectedUserSelectionId() > 0;
  });

  canDeleteSelection = computed<boolean>(() => {
    return (
      this.selectedUserSelectionId() > 0 &&
      (this.savePublicPermission() || this.selectionIsPrivate())
    );
  });

  selectedUserSelection = signal<IUserSelectionDTO | undefined>(undefined);

  // Lookups
  userSelections: SmallGenericType[] = [];

  constructor() {
    // React to external input changes by patching the form
    effect(() => {
      const id = this.selectedUserSelectionId();
      this.form.patchValue(
        { selectedUserSelectionId: id },
        { emitEvent: true }
      );
      this.onSelectionChanged(id);
    });
  }

  ngOnInit(): void {
    // Set initial value
    this.form.patchValue({
      selectedUserSelectionId: this.selectedUserSelectionId(),
    });

    forkJoin([this.loadTerms(), this.loadUserSelections()]).subscribe();
  }

  private loadTerms(): Observable<TermCollection> {
    return this.translate
      .get([
        'core.warning',
        'core.deletewarning',
        'core.reportmenu.selection.delete.error',
        'core.reportmenu.selection.deletepublicwarning.title',
        'core.reportmenu.selection.deletepublicwarning.message',
      ])
      .pipe(
        tap(terms => {
          this.terms = terms;
        })
      );
  }

  private loadUserSelections(): Observable<SmallGenericType[]> {
    return this.service.getUserSelectionsDict(this.selectionType()).pipe(
      map(userSelections => {
        // Add empty row
        const empty = new SmallGenericType(
          0,
          this.translate.instant('core.reportmenu.selection.new')
        );

        return [empty, ...userSelections];
      }),
      tap(userSelections => {
        this.userSelections = userSelections;

        // Select the empty row as default
        this.selectedUserSelection.set(undefined);

        this.form.patchValue({
          selectedUserSelectionId: 0,
        });

        this.onSelectionsLoaded.emit(userSelections);
      })
    );
  }

  // EVENTS

  onSelectionChanged(value: number): void {
    this.selectedUserSelectionId.set(value);

    if (this.form.controls.selectedUserSelectionId.value !== value) {
      this.form.patchValue({
        selectedUserSelectionId: value,
      });
    }

    if (value > 0) {
      this.service.getUserSelection(value).subscribe(userSelection => {
        this.selectedUserSelection.set(userSelection);
        this.onSelected.emit(userSelection);
      });
    } else {
      this.selectedUserSelection.set(undefined);
      this.onSelected.emit(undefined);
    }
  }

  saveUserSelection(isCopy: boolean): void {
    const dialogData: SaveUserSelectionDialogData = {
      title: 'core.reportmenu.selection.save',
      size: 'md',
      disableContentScroll: true,
      selection: this.selectedUserSelection() || ({} as IUserSelectionDTO),
      isCopy: isCopy,
    };

    this.dialogService
      .open(SaveUserSelectionDialogComponent, dialogData)
      .afterClosed()
      .subscribe((result: SaveUserSelectionDialogResult) => {
        if (result?.modified && result.selection) {
          this.service
            .saveUserSelection(result.selection)
            .subscribe(response => {
              this.loadUserSelections().subscribe(() => {
                this.onSelectionChanged(ResponseUtil.getEntityId(response));
              });
            });
        }
      });
  }

  initDeleteUserSelection(): void {
    const mb = this.messageboxService.warning(
      this.selectionIsPrivate()
        ? this.terms['core.warning']
        : this.terms['core.reportmenu.selection.deletepublicwarning.title'],
      this.selectionIsPrivate()
        ? this.terms['core.deletewarning']
        : this.terms['core.reportmenu.selection.deletepublicwarning.message']
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) {
        this.deleteUserSelection();
      }
    });
  }

  deleteUserSelection(): void {
    this.service
      .deleteUserSelection(this.selectedUserSelectionId())
      .subscribe(response => {
        if (response.success) {
          this.loadUserSelections().subscribe(() => {
            this.onSelectionChanged(0);
          });
        } else {
          this.messageboxService.error(
            this.terms['core.reportmenu.selection.delete.error'],
            ResponseUtil.getErrorMessage(response)
          );
        }
      });
  }
}
