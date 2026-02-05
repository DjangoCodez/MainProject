import { FormControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  TermGroup_ReportUserSelectionAccessType,
  UserSelectionType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IUserSelectionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

interface ISaveUserSelectionDialogForm {
  validationHandler: ValidationHandler;
  element: IUserSelectionDTO | undefined;
}
export class SaveUserSelectionDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISaveUserSelectionDialogForm) {
    super(validationHandler, {
      userSelectionId: new SoeNumberFormControl(
        element?.userSelectionId || undefined
      ),
      userId: new SoeNumberFormControl(element?.userId || undefined),
      type: new SoeNumberFormControl(
        element?.type || UserSelectionType.Unknown
      ),
      name: new SoeTextFormControl(
        element?.name,
        {
          required: true,
          maxLength: 100,
        },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description, {
        maxLength: 512,
      }),
      selections: new SoeTextFormControl(element?.selections),
      selection: new SoeTextFormControl(element?.selection),
      default: new SoeCheckboxFormControl(element?.default || false),
      accessType: new SoeSelectFormControl(
        TermGroup_ReportUserSelectionAccessType.Private,
        {
          required: true,
        }
      ),
      roleIds: new FormControl<number[]>([]),
      messageGroupIds: new FormControl<number[]>([]),
    });
  }

  get isNewSelection(): boolean {
    return !this.controls.userSelectionId.value;
  }

  get accessTypeIsPrivate(): boolean {
    return (
      this.controls.accessType.value ===
      TermGroup_ReportUserSelectionAccessType.Private
    );
  }

  get accessTypeIsPublic(): boolean {
    return (
      this.controls.accessType.value ===
      TermGroup_ReportUserSelectionAccessType.Public
    );
  }

  get accessTypeIsRole(): boolean {
    return (
      this.controls.accessType.value ===
      TermGroup_ReportUserSelectionAccessType.Role
    );
  }

  get accessTypeIsMessageGroup(): boolean {
    return (
      this.controls.accessType.value ===
      TermGroup_ReportUserSelectionAccessType.MessageGroup
    );
  }

  get isCurrentRoleSelected(): boolean {
    const roleIds = this.controls.roleIds.value as number[];
    return roleIds.includes(SoeConfigUtil.roleId);
  }

  customPatchValue(element: IUserSelectionDTO, isCopy: boolean = false) {
    this.patchValue({
      userSelectionId: isCopy ? undefined : element.userSelectionId,
      userId: element.userId,
      type: element.type,
      name: isCopy ? undefined : element.name,
      description: element.description,
      selections: element.selections,
      selection: element.selection,
      default: element.default || false,
    });
    this.customAccessTypePatchValue(element);
    this.customRoleIdsPatchValue(
      element?.access
        ?.filter(a => a.roleId !== undefined)
        .map(a => a.roleId!) || []
    );
    this.customMessageGroupIdsPatchValue(
      element?.access
        ?.filter(a => a.messageGroupId !== undefined)
        .map(a => a.messageGroupId!) || []
    );
  }

  customAccessTypePatchValue(element: IUserSelectionDTO) {
    // If selection has a userId, it is a private selection
    let accessType: TermGroup_ReportUserSelectionAccessType =
      TermGroup_ReportUserSelectionAccessType.Private;

    if (!element.userSelectionId) {
      // New selection - default to private and set userId
      this.controls.userId.setValue(SoeConfigUtil.userId);
    } else {
      if (!element.userId) {
        // Get access type from selection's first access entry
        // Access entries only exist for roles and message groups
        if (element.access?.length > 0) {
          switch (element.access[0].type) {
            case TermGroup_ReportUserSelectionAccessType.Role:
              accessType = TermGroup_ReportUserSelectionAccessType.Role;
              break;
            case TermGroup_ReportUserSelectionAccessType.MessageGroup:
              accessType = TermGroup_ReportUserSelectionAccessType.MessageGroup;
              break;
          }
        } else {
          // No access entries and no userId - must be public
          accessType = TermGroup_ReportUserSelectionAccessType.Public;
        }
      }
    }

    this.controls.accessType.setValue(accessType);
  }

  customRoleIdsPatchValue(roleIds: number[]) {
    this.controls.roleIds.setValue(roleIds);
  }

  customMessageGroupIdsPatchValue(messageGroupIds: number[]) {
    this.controls.messageGroupIds.setValue(messageGroupIds);
  }
}

export function createRolesValidator(errorMessage: string): ValidatorFn {
  // Validate that at least one role is selected if access type is Role
  return (form): ValidationErrors | null => {
    const accessType = form.get('accessType')
      ?.value as TermGroup_ReportUserSelectionAccessType;
    if (accessType !== TermGroup_ReportUserSelectionAccessType.Role) {
      return null;
    }

    const rolesNotSelected =
      (form.get('roleIds')?.value as number[]).length === 0;
    return rolesNotSelected ? { [errorMessage]: true } : null;
  };
}

export function createMessageGroupsValidator(
  errorMessage: string
): ValidatorFn {
  // Validate that at least one message group is selected if access type is MessageGroup
  return (form): ValidationErrors | null => {
    const accessType = form.get('accessType')
      ?.value as TermGroup_ReportUserSelectionAccessType;
    if (accessType !== TermGroup_ReportUserSelectionAccessType.MessageGroup) {
      return null;
    }

    const messageGroupsNotSelected =
      (form.get('messageGroupIds')?.value as number[]).length === 0;
    return messageGroupsNotSelected ? { [errorMessage]: true } : null;
  };
}
