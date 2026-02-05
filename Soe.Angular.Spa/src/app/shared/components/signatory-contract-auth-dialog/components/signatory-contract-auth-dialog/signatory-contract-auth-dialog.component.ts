import {
  Component,
  OnInit,
  WritableSignal,
  inject,
  signal,
} from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ValidationHandler } from '@shared/handlers';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SignatoryContractAuthService } from '../../services/signatory-contract-auth.service';
import { SignatoryContractAuthDialogData } from '../../models/signatory-contract-auth-dialog-data.model';
import { AuthenticationFormData } from '../../models/authentication-form-data.model';
import { SignatoryContractAuthForm } from '../../models/signatory-contract-auth-form.model';
import { IAuthenticationDetailsDTO } from '@shared/models/generated-interfaces/AuthenticationDetailsDTO';
import { IAuthenticationResponseDTO } from '@shared/models/generated-interfaces/AuthenticationResponseDTO';
import { SignatoryContractAuthenticationMethodType } from '@shared/models/generated-interfaces/Enumerations';
import { Perform } from '@shared/util/perform.class';
import { tap } from 'rxjs/operators';
import { CrudActionTypeEnum } from '@shared/enums';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { IAuthorizeRequestDTO } from '@shared/models/generated-interfaces/AuthorizeRequestDTO';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-signatory-contract-auth-dialog',
  templateUrl: './signatory-contract-auth-dialog.component.html',
  imports: [
    ButtonComponent,
    DialogComponent,
    TextboxComponent,
    InstructionComponent,
    ReactiveFormsModule,
    SaveButtonComponent,
  ],
  providers: [SignatoryContractAuthService],
})
export class SignatoryContractAuthDialogComponent
  extends DialogComponent<SignatoryContractAuthDialogData>
  implements OnInit
{
  private readonly validationHandler = inject(ValidationHandler);
  private readonly progressService = inject(ProgressService);
  private readonly translateService = inject(TranslateService);
  private readonly performLoadData = new Perform<any>(this.progressService);
  private readonly performAction = new Perform<any>(this.progressService);
  private readonly signatoryContractAuthService = inject(
    SignatoryContractAuthService
  );

  protected readonly form: SignatoryContractAuthForm =
    new SignatoryContractAuthForm({
      validationHandler: this.validationHandler,
      element: new AuthenticationFormData(),
    });

  protected hasPermission: WritableSignal<boolean> = signal(true);
  protected showPassword: WritableSignal<boolean> = signal(false);
  protected showCode: WritableSignal<boolean> = signal(false);
  protected couldNotAuth: WritableSignal<boolean> = signal(false);
  protected message: WritableSignal<string | null> = signal(null);
  protected isLoaded: WritableSignal<boolean> = signal(false);

  private authenticationDetails: IAuthenticationDetailsDTO | null = null;

  ngOnInit() {
    this.data.title = this.translateService.instant(
      'common.signatorycontract.authentication'
    );
    this.loadData();
  }

  private loadData(): void {
    const authorizeRequestDTO: IAuthorizeRequestDTO = {
      permissionType: this.data.permissionType,
      signatoryContractId: this.data.signatoryContractId,
    };
    this.performLoadData
      .load$(
        this.signatoryContractAuthService.authorize(authorizeRequestDTO).pipe(
          tap(result => {
            this.isLoaded.set(true);
            this.data.title = this.data.title + ` [${result.permissionLabel}]`;

            if (!result.hasPermission) {
              this.hasPermission.set(false);
              return;
            }

            if (result.isAuthorized) {
              this.onSuccess();
              return;
            }

            if (result.isAuthenticationRequired) {
              this.authenticationDetails = result.authenticationDetails;
              this.setupAuthenticate();
            }
          })
        ),
        { showDialogDelay: 1000 }
      )
      .subscribe();
  }

  private setupAuthenticate() {
    if (!this.authenticationDetails) return;

    switch (this.authenticationDetails.authenticationMethodType) {
      case SignatoryContractAuthenticationMethodType.Password:
        this.showPassword.set(true);
        break;
      case SignatoryContractAuthenticationMethodType.PasswordSMSCode:
        this.showPassword.set(true);
        this.showCode.set(true);
        break;
    }

    this.message.set(this.authenticationDetails.message);
    this.couldNotAuth.set(
      this.authenticationDetails.authenticationRequestId === 0
    );

    this.form.setCodeValidators(this.showCode());
  }

  protected ok() {
    if (!this.authenticationDetails) return;

    const auth: IAuthenticationResponseDTO = {
      signatoryContractAuthenticationRequestId:
        this.authenticationDetails.authenticationRequestId,
      username: this.form.username.value,
      password: this.form.password.value,
      code: this.form.code.value,
    };

    this.performAction.crud(
      CrudActionTypeEnum.Work,
      this.signatoryContractAuthService.authenticate(auth).pipe(
        tap(result => {
          if (result.success) {
            this.onSuccess();
          }

          this.message.set(ResponseUtil.getStringValue(result));
        })
      )
    );
  }

  private onSuccess() {
    this.dialogRef.close(true);
  }
}
