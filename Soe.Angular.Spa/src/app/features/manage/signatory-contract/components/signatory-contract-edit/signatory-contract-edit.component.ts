import {
  Component,
  inject,
  OnInit,
  viewChild,
  signal,
  computed,
} from '@angular/core';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SignatoryContractService } from '../../services/signatory-contract.service';
import { SignatoryContractDTO } from '../../models/signatory-contract-edit-dto.model';
import {
  SignatoryContractForm,
  subContractPermissionValidator,
} from '../../models/signatory-contract-form.model';
import {
  Feature,
  TermGroup,
  TermGroup_SignatoryContractPermissionType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ProgressOptions } from '@shared/services/progress';
import { SignatoryContractPermissionEditGridComponent } from '../signatory-contract-permission-edit-grid/signatory-contract-permission-edit-grid.component';
import { ISignatoryContractDTO } from '@shared/models/generated-interfaces/SignatoryContractDTO';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { ToolbarEditConfig } from '@ui/toolbar/models/toolbar';
import { TermCollection } from '@shared/localization/term-types';
import { SignatoryContractRevokeDialogComponent } from '../signatory-contract-revoke-dialog/signatory-contract-revoke-dialog.component';
import { CrudActionTypeEnum } from '@shared/enums';
import { SignatoryContractRevokeDTO } from '../../models/signatory-contract-revoke-dto';
import { SubSignatoryContractEditGridComponent } from '../sub-signatory-contract-edit-grid/sub-signatory-contract-edit-grid.component';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { SignatoryContractAuthDialogData } from '@shared/components/signatory-contract-auth-dialog/models/signatory-contract-auth-dialog-data.model';
import { SignatoryContractAuthDialogComponent } from '@shared/components/signatory-contract-auth-dialog/components/signatory-contract-auth-dialog/signatory-contract-auth-dialog.component';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-signatory-contract-edit',
  standalone: false,
  templateUrl: './signatory-contract-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class SignatoryContractEditComponent
  extends EditBaseDirective<
    SignatoryContractDTO,
    SignatoryContractService,
    SignatoryContractForm
  >
  implements OnInit
{
  readonly service = inject(SignatoryContractService);
  private readonly coreService = inject(CoreService);
  private readonly dialogService = inject(DialogService);
  protected readonly savingOptions: ProgressOptions = {
    callback: (val: BackendResponse) => {
      this.afterSave();
    },
  };
  private readonly permissionGrid =
    viewChild.required<SignatoryContractPermissionEditGridComponent>(
      'permissionEditGrid'
    );
  private readonly subSignatoryContractEditGrid =
    viewChild.required<SubSignatoryContractEditGridComponent>(
      'subSignatoryContractEditGrid'
    );
  private readonly supportUserId = SoeConfigUtil.supportUserId;
  private readonly userId = SoeConfigUtil.userId;

  protected users: ISmallGenericType[] = [];
  protected authenticationMethodTerms: ISmallGenericType[] = [];
  protected readonly isRevoked = signal<boolean>(false);
  private readonly isMainRecipientUser = signal<boolean>(false);
  private readonly canManageSubContracts = signal<boolean>(false);
  protected readonly isNew = signal<boolean>(false);

  protected readonly addPermission = computed(
    () => this.flowHandler.modifyPermission() && !!this.supportUserId
  );

  protected readonly modifyPermission = computed(
    () =>
      this.flowHandler.modifyPermission() &&
      this.isMainRecipientUser() &&
      !this.supportUserId &&
      this.canManageSubContracts()
  );

  protected readonly revokePermission = computed(
    () =>
      this.flowHandler.modifyPermission() &&
      (this.isMainRecipientUser() || !!this.supportUserId)
  );

  protected readonly savePermission = computed(
    () =>
      (this.isNew() && this.addPermission()) ||
      (!this.isNew() && this.modifyPermission())
  );

  override ngOnInit(): void {
    super.ngOnInit();
    this.isNew.set(this.form!.isNew);
    this.startFlow(Feature.Manage_Preferences_Registry_SignatoryContract_Edit, {
      lookups: [this.loadUsers(), this.loadAuthenticationMethodTerms()],
    });
  }

  override loadTerms(): Observable<TermCollection> {
    const translationsKeys: string[] = [
      'manage.registry.signatorycontract.revoke',
      'manage.registry.signatorycontract.error.subcontractinvalidpermission',
    ];

    return super.loadTerms(translationsKeys);
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form!.getIdControl()!.value).pipe(
        tap((value: SignatoryContractDTO) => {
          this.form!.reset(value);
          this.form!.customSubContractsPatchValues(
            <SignatoryContractDTO[]>value.subContracts
          );

          if (value.revokedAt) {
            this.isRevoked.set(true);
          }

          this.isMainRecipientUser.set(value.recipientUserId === this.userId);

          this.canManageSubContracts.set(
            value.permissionTypes.includes(
              TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts
            ) &&
              value.permissionTypes.some(
                p =>
                  p !==
                  TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts
              )
          );

          setTimeout(() => {
            this.resetSubSignatoryContractGrid();
            this.disableControlsByAdditionalPermission();
          }, 0);
        })
      ),
      { showDialogDelay: 1000 }
    );
  }

  override onFinished(): void {
    super.onFinished();
    this.form!.addValidators(
      subContractPermissionValidator(
        this.terms[
          'manage.registry.signatorycontract.error.subcontractinvalidpermission'
        ]
      )
    );

    this.isNew.set(this.form!.isNew);
    if (!this.isNew()) {
      this.disableFormFieldsForEdit();
    }

    this.form!.revokedAt.disable();
  }

  override createEditToolbar(): void {
    const config: Partial<ToolbarEditConfig> = super.getDefaultToolbarOptions();
    config.hideCopy = true;
    super.createEditToolbar(config);
  }

  private disableControlsByAdditionalPermission(): void {
    if (!this.modifyPermission()) {
      this.form!.disable();
    }
  }

  private disableFormFieldsForEdit(): void {
    this.form!.recipientUserId.disable();
    this.form!.requiredAuthenticationMethodType.disable();
  }

  private afterSave(): void {
    const wasNew = this.isNew();
    this.isNew.set(false);

    setTimeout(() => {
      if (wasNew) {
        this.disableFormFieldsForEdit();
        this.resetPermissionGrid();
      }

      this.refreshGrids();
    }, 0);
  }

  private afterRevoke(): void {
    this.loadData()
      .pipe(
        tap(() => {
          setTimeout(() => {
            this.form!.revokedAt.disable();
            this.resetSubSignatoryContractGrid();
            this.refreshGrids();
          }, 0);
        })
      )
      .subscribe();
  }

  private refreshGrids(): void {
    this.permissionGrid().refreshGrid();
    this.subSignatoryContractEditGrid().refreshGrid();
  }

  private resetSubSignatoryContractGrid(): void {
    this.subSignatoryContractEditGrid().resetGrid();
  }

  private resetPermissionGrid(): void {
    this.permissionGrid().resetGrid();
  }

  private loadUsers(): Observable<ISmallGenericType[]> {
    return this.coreService.getUsersDict(false, false, true, false, false).pipe(
      tap(x => {
        this.users = x;
      })
    );
  }

  private loadAuthenticationMethodTerms(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.SignatoryContractAuthenticationMethodType,
        false,
        false,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.authenticationMethodTerms = x;
        })
      );
  }

  protected permissionChanged(selectedPermissions: number[]): void {
    this.form!.permissionTypes.setValue(selectedPermissions);

    this.form!.permissionTypes.markAsDirty();
    this.form!.permissionTypes.markAsTouched();
  }

  protected subSignatoryContractsChanged(
    subSignatoryContracts: ISignatoryContractDTO[]
  ): void {
    this.form!.customSubContractsPatchValues(
      <SignatoryContractDTO[]>subSignatoryContracts
    );

    this.form!.subContracts.markAsDirty();
    this.form!.subContracts.markAsTouched();
  }

  protected showRevokeModal(): void {
    const dialogData: DialogData = {
      title: this.terms['manage.registry.signatorycontract.revoke'],
      size: 'md',
    };

    this.dialogService
      .open(SignatoryContractRevokeDialogComponent, dialogData)
      .afterClosed()
      .subscribe((value: string | boolean | undefined) => {
        if (value) {
          if (this.form!.subContracts.length) {
            const mb = this.messageboxService.warning(
              'manage.registry.signatorycontract.revokesubsignatorycontractwarningtitle',
              'manage.registry.signatorycontract.revokesubsignatorycontractwarningmessage'
            );
            mb.afterClosed().subscribe(
              (response: IMessageboxComponentResponse) => {
                if (response?.result) {
                  this.revoke(value as string);
                }
              }
            );
          } else {
            this.revoke(value as string);
          }
        }
      });
  }

  private showContractPermissionModal(): void {
    const dialogData: SignatoryContractAuthDialogData = {
      title: '',
      size: 'sm',
      permissionType:
        TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
      signatoryContractId: this.form!.getIdControl()!.value,
    };

    this.dialogService
      .open(SignatoryContractAuthDialogComponent, dialogData)
      .afterClosed()
      .subscribe((value: boolean) => {
        if (value) {
          this.save();
        }
      });
  }

  private revoke(revokedReason: string): void {
    const dto: SignatoryContractRevokeDTO = {
      signatoryContractId: this.form!.getIdControl()!.value,
      revokedReason: revokedReason,
    };
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.revoke(dto),
      (val: BackendResponse) => {
        this.afterRevoke();
      }
    );
  }

  override performSave(): void {
    if (this.isNew()) {
      this.save();
    } else {
      this.showContractPermissionModal();
    }
  }

  private save(): void {
    const savingOptions: ProgressOptions = {
      callback: (val: BackendResponse) => {
        this.afterSave();
      },
    };

    super.performSave(savingOptions);
  }
}
