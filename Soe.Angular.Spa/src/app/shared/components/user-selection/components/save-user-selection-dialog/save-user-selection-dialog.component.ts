import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ValidationHandler } from '@shared/handlers';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import {
  createMessageGroupsValidator,
  createRolesValidator,
  SaveUserSelectionDialogForm,
} from '../../models/save-user-selection-dialog-form.model';
import { IUserSelectionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { forkJoin, Observable, tap } from 'rxjs';
import {
  Feature,
  TermGroup,
  TermGroup_ReportUserSelectionAccessType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { RoleService } from '@shared/services/role.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { MessageGroupService } from '@features/time/time-schedule-events/services/message-group.service';
import { SelectComponent } from '@ui/forms/select/select.component';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { TranslateService } from '@ngx-translate/core';
import { TermCollection } from '@shared/localization/term-types';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';

export class SaveUserSelectionDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  disableContentScroll?: boolean;
  selection: IUserSelectionDTO = {} as IUserSelectionDTO;
  isCopy?: boolean;
}

export class SaveUserSelectionDialogResult {
  selection: IUserSelectionDTO = {} as IUserSelectionDTO;
  modified: boolean = false;
}

@Component({
  selector: 'save-user-selection-dialog',
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    SaveButtonComponent,
    DialogComponent,
    MultiSelectComponent,
    SelectComponent,
    TextboxComponent,
  ],
  templateUrl: './save-user-selection-dialog.component.html',
  styleUrl: './save-user-selection-dialog.component.scss',
})
export class SaveUserSelectionDialogComponent
  extends DialogComponent<SaveUserSelectionDialogData>
  implements OnInit
{
  // Form
  validationHandler = inject(ValidationHandler);
  form: SaveUserSelectionDialogForm = new SaveUserSelectionDialogForm({
    validationHandler: this.validationHandler,
    element: undefined,
  });

  // Services
  private readonly coreService = inject(CoreService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly messageGroupService = inject(MessageGroupService);
  private readonly roleService = inject(RoleService);
  private readonly translate = inject(TranslateService);
  private readonly cdr = inject(ChangeDetectorRef);

  // Permissions
  roleEditPermission = signal<boolean>(false);
  messageGroupEditPermission = signal<boolean>(false);

  // Terms
  private terms: TermCollection = {};

  // Lookups
  accessTypes: ISmallGenericType[] = [];
  roles: ISmallGenericType[] = [];
  messageGroups: ISmallGenericType[] = [];

  // Flags
  private initiallyPrivate = false;

  ngOnInit(): void {
    forkJoin([this.loadModifyPermissions(), this.loadTerms()]).subscribe(() => {
      this.loadAccessTypes()
        .pipe(
          tap(() => {
            // Remember if selection was initially private
            // It's used to determine if we should show a warning when changing access type
            this.initiallyPrivate =
              !this.data.isCopy &&
              !!this.data.selection.userSelectionId &&
              !!this.data.selection.userId;

            this.form.customPatchValue(this.data.selection, this.data.isCopy);
            this.addFormValidators();
          })
        )
        .subscribe();
    });

    this.form.controls.accessType.valueChanges.subscribe(() => {
      this.onAccessTypeChanged();
    });
  }

  private loadTerms(): Observable<TermCollection> {
    return this.translate
      .get([
        'core.missingmandatoryfield',
        'core.reportmenu.selection.modifypublicwarning.title',
        'core.reportmenu.selection.modifypublicwarning.message',
        'core.warning',
        'common.user.roles',
        'manage.registry.receivergroups.receivergroups',
      ])
      .pipe(
        tap(terms => {
          this.terms = terms;
        })
      );
  }

  private addFormValidators() {
    this.form.addValidators([
      createRolesValidator(
        `${this.terms['core.missingmandatoryfield']} ${this.terms['common.user.roles']}`
      ),
      createMessageGroupsValidator(
        `${this.terms['core.missingmandatoryfield']} ${this.terms['manage.registry.receivergroups.receivergroups']}`
      ),
    ]);
  }

  private loadModifyPermissions(): Observable<Record<number, boolean>> {
    const featureIds: number[] = [];
    featureIds.push(Feature.Manage_Roles_Edit_Permission);
    featureIds.push(Feature.Manage_Preferences_Registry_EventReceiverGroups);

    return this.coreService.hasModifyPermissions(featureIds).pipe(
      tap(x => {
        this.roleEditPermission.set(x[Feature.Manage_Roles_Edit_Permission]);
        this.messageGroupEditPermission.set(
          x[Feature.Manage_Preferences_Registry_EventReceiverGroups]
        );
      })
    );
  }

  private loadAccessTypes(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.ReportUserSelectionAccessType,
        false,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.accessTypes = [];
          x.forEach(at => {
            // Exclude 'MessageGroup' access type if edit permission for it is missing
            if (
              at.id !== TermGroup_ReportUserSelectionAccessType.MessageGroup ||
              this.messageGroupEditPermission()
            ) {
              this.accessTypes.push(at);
            }
          });
        })
      );
  }

  private loadRoles(): Observable<ISmallGenericType[]> {
    if (this.roleEditPermission()) {
      return this.roleService.getRolesByCompanyAsDict(false, false).pipe(
        tap(x => {
          this.roles = x;
        })
      );
    } else {
      return this.roleService
        .getRolesByUserAsDict(SoeConfigUtil.actorCompanyId)
        .pipe(
          tap(x => {
            this.roles = x;
          })
        );
    }
  }

  private loadMessageGroups(): Observable<ISmallGenericType[]> {
    return this.messageGroupService.getDict().pipe(
      tap(x => {
        this.messageGroups = x;
      })
    );
  }

  onAccessTypeChanged(): void {
    // Load roles or message groups as needed
    if (this.form.accessTypeIsRole && this.roles.length === 0) {
      this.loadRoles().subscribe();
    } else if (
      this.form.accessTypeIsMessageGroup &&
      this.messageGroups.length === 0
    ) {
      this.loadMessageGroups().subscribe();
    }

    // Clear roleIds if access type is not Role
    if (!this.form.accessTypeIsRole) {
      this.form.controls.roleIds.setValue([]);
    }

    // Clear messageGroupIds if access type is not MessageGroup
    if (!this.form.accessTypeIsMessageGroup) {
      this.form.controls.messageGroupIds.setValue([]);
    }

    // Set userId based on access type
    if (this.form.accessTypeIsPrivate) {
      this.form.controls.userId.setValue(SoeConfigUtil.userId);
    } else {
      this.form.controls.userId.setValue(undefined);
    }

    // Set current role if access type is Role and no roles are selected
    if (
      this.form.accessTypeIsRole &&
      (this.form.controls.roleIds.value as number[]).length === 0
    ) {
      this.form.controls.roleIds.setValue([SoeConfigUtil.roleId]);
    }

    // Update validation
    this.form.controls.roleIds.updateValueAndValidity();
    this.form.controls.messageGroupIds.updateValueAndValidity();

    console.log('Access type changed, form:', this.form.value);
  }

  validateCurrentRoleIsSelected(): boolean {
    if (this.form.accessTypeIsRole) {
      return this.form.isCurrentRoleSelected;
    } else {
      return true;
    }
  }

  cancel() {
    this.dialogRef.close({ modified: false } as SaveUserSelectionDialogResult);
  }

  initSave() {
    if (!this.form.isNewSelection && !this.initiallyPrivate) {
      // User is editing an existing selection that is not private, show warning.
      const mb = this.messageboxService.warning(
        this.terms['core.reportmenu.selection.modifypublicwarning.title'],
        this.terms['core.reportmenu.selection.modifypublicwarning.message']
      );
      mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
        if (response?.result) {
          this.validate();
        }
      });
      return;
    } else {
      this.validate();
    }
  }

  private validate() {
    if (!this.validateCurrentRoleIsSelected()) {
      // TODO: New term
      const currentRole = this.roles.find(r => r.id === SoeConfigUtil.roleId);
      const mb = this.messageboxService.warning(
        this.terms['core.warning'],
        `Du har valt roller men inte din nuvarande roll (${currentRole?.name}).\nDet innebär att du inte kommer att ha tillgång till detta urval när du använder aktuell roll.\n\nVill du fortsätta?`
      );
      mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
        if (response?.result) {
          this.save();
        }
      });
    } else {
      this.save();
    }
  }

  private save() {
    this.dialogRef.close({
      selection: this.form.value,
      modified: true,
    } as SaveUserSelectionDialogResult);
  }
}
