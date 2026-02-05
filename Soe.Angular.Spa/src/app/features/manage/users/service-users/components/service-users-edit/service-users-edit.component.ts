import {
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ServiceUserService } from '../../services/service-users.service';
import { AddServiceUserForm } from '../../models/add-service-user.form';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, tap } from 'rxjs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IServiceUserDTO } from '@shared/models/generated-interfaces/ServiceUserDTO';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ValidationHandler } from '@shared/handlers';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'soe-service-users-edit',
  templateUrl: './service-users-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ServiceUsersEditComponent
  extends EditBaseDirective<
    IServiceUserDTO,
    ServiceUserService,
    AddServiceUserForm
  >
  implements OnInit
{
  readonly service = inject(ServiceUserService);
  readonly validationHandler = inject(ValidationHandler);
  readonly destroyed$ = inject(DestroyRef);

  // Data for dropdowns
  protected roles = signal<SmallGenericType[]>([]);
  protected attestRoles = signal<SmallGenericType[]>([]);

  // Mode flags
  protected isEditMode = signal(false);
  protected isAddMode = computed(() => !this.isEditMode());

  override ngOnInit(): void {
    super.ngOnInit();
    this.form
      ?.getIdControl()
      ?.valueChanges.pipe(takeUntilDestroyed(this.destroyed$))
      .subscribe(id => {
        this.isEditMode.set(!!id);
      });
    this.startFlow(Feature.ClientManagement_Clients, {
      lookups: [this.loadRoles(), this.loadAttestRoles()],
    });
  }

  override onFinished(): void {
    super.onFinished();
  }

  override loadData(): Observable<void> {
    return super.loadData().pipe(
      tap(() => {
        if (this.form?.getIdControl()?.value) {
          this.form.get('userName')?.disable();
          this.form.get('roleId')?.disable();
          this.form.get('attestRoleIds')?.disable();
        }
      })
    );
  }

  private loadRoles() {
    return this.service.getRoles().pipe(
      tap(roles => {
        this.roles.set(roles.map(r => ({ id: r.roleId, name: r.name })));
      })
    );
  }

  private loadAttestRoles() {
    return this.service.getAttestRoles().pipe(
      tap(attestRoles => {
        this.attestRoles.set(
          attestRoles.map(r => ({ id: r.attestRoleId, name: r.name }))
        );
      })
    );
  }

  onSave(): void {
    // Only show confirmation for new connections (add mode)
    if (!this.isAddMode() || !this.form) return;
    const connectionCode = this.form.get('connectionCode')?.value;

    if (!connectionCode) {
      return;
    }

    // Fetch connection request details for confirmation
    this.service.getConnectionRequest(connectionCode).subscribe({
      next: connectionRequest => {
        // Build confirmation message with service provider name and initiator
        const serviceProviderName = connectionRequest.mcName;
        const initiatedBy = connectionRequest.createdBy;

        const message = this.translate
          .instant('manage.serviceuser.confirmationmessage')
          .replace('{0}', serviceProviderName)
          .replace('{1}', initiatedBy);

        this.messageboxService
          .question(this.translate.instant('core.verifyquestion'), message)
          .afterClosed()
          .subscribe(({ result }) => {
            if (result) {
              // User confirmed - proceed with save
              super.performSave();
            }
          });
      },
      error: () => {
        // Handle error - invalid code or connection request not found
        this.messageboxService.error(
          this.translate.instant('core.error'),
          this.translate.instant('manage.serviceuser.invalidconnectioncode')
        );
      },
    });
  }
}
