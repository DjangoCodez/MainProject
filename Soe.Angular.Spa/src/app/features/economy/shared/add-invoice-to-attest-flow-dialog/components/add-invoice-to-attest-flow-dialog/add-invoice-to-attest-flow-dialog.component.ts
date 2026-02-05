import { Component, inject, OnInit, signal, viewChildren } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin, Observable } from 'rxjs';
import { map, mergeMap, tap } from 'rxjs/operators';
import {
  IAttestWorkFlowHeadDTO,
  IAttestWorkFlowRowDTO,
  IAttestWorkFlowTemplateHeadDTO,
  IAttestWorkFlowTemplateRowDTO,
  IUserSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  CompanySettingType,
  SoeEntityState,
  SoeEntityType,
  TermGroup,
  TermGroup_AttestWorkFlowRowProcessType,
} from '@shared/models/generated-interfaces/Enumerations';
import { SupplierService } from '@features/economy/services/supplier.service';
import { CoreService } from '@shared/services/core.service';
import { AttestationGroupsService } from '@features/economy/attestation-groups/services/attestation-groups.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { UserSelectorForTemplateHeadRowComponent } from '../user-selector-for-template-head-row/user-selector-for-template-head-row.component';
import { AddInvoiceToAttestFlowDialogData } from '../../models/add-invoice-to-attest-flow-dialog-data.model';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { ValidationHandler } from '@shared/handlers';
import { ProgressService } from '@shared/services/progress/progress.service';
import { TranslateService } from '@ngx-translate/core';
import { Perform } from '@shared/util/perform.class';
import { AddInvoiceToAttestFlowForm } from '../../models/add-invoice-to-attest-flow-form.model';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { ReactiveFormsModule } from '@angular/forms';
import { NumberUtil } from '@shared/util/number-util';
import {
  InstructionComponent,
  InstructionType,
} from '@ui/instruction/instruction.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { CrudActionTypeEnum } from '@shared/enums/action.enum';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-add-invoice-to-attest-flow-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogComponent,
    SelectComponent,
    TextboxComponent,
    CheckboxComponent,
    ButtonComponent,
    SaveButtonComponent,
    UserSelectorForTemplateHeadRowComponent,
    InstructionComponent,
  ],
  templateUrl: './add-invoice-to-attest-flow-dialog.component.html',
  styleUrls: ['./add-invoice-to-attest-flow-dialog.component.scss'],
})
export class AddInvoiceToAttestFlowDialogComponent
  extends DialogComponent<AddInvoiceToAttestFlowDialogData>
  implements OnInit
{
  private readonly validationHandler = inject(ValidationHandler);
  private readonly progressService = inject(ProgressService);
  private readonly translateService = inject(TranslateService);
  private readonly supplierService = inject(SupplierService);
  private readonly coreService = inject(CoreService);
  private readonly attestationService = inject(AttestationGroupsService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly performLoadData = new Perform<{
    groups: ISmallGenericType[];
    templates: IAttestWorkFlowTemplateHeadDTO[];
    groupTypes: ISmallGenericType[];
    roleUser: ISmallGenericType[];
    companySettings: void;
  }>(this.progressService);
  private readonly performSave = new Perform<BackendResponse>(
    this.progressService
  );
  private readonly performGroupChange = new Perform<IAttestWorkFlowHeadDTO>(
    this.progressService
  );
  private readonly performTemplateLoad = new Perform<
    IAttestWorkFlowTemplateRowDTO[]
  >(this.progressService);
  private readonly performCheckExisting = new Perform<IAttestWorkFlowHeadDTO[]>(
    this.progressService
  );

  readonly userSelectors = viewChildren(
    UserSelectorForTemplateHeadRowComponent
  );

  protected readonly form: AddInvoiceToAttestFlowForm =
    new AddInvoiceToAttestFlowForm({
      validationHandler: this.validationHandler,
    });

  // Amount & user setting
  private requiredUserId: number = 0;
  private totalAmountWhenUserRequired: number = 0;
  private requiredUser: IUserSmallDTO | null = null;
  protected readonly userRequiredMessage = signal('');
  protected readonly userRequiredMessageType = signal<InstructionType>('info');
  protected readonly showUserRequiredWarning = signal(false);

  // Data
  protected readonly attestWorkFlowHead = signal<IAttestWorkFlowHeadDTO>(
    {} as IAttestWorkFlowHeadDTO
  );

  // Lookups
  protected readonly attestGroups = signal<ISmallGenericType[]>([]);
  protected readonly templates = signal<IAttestWorkFlowTemplateHeadDTO[]>([]);
  protected readonly attestGroupTypes = signal<ISmallGenericType[]>([]);
  protected readonly role_User = signal<ISmallGenericType[]>([]);
  protected readonly templateRows = signal<IAttestWorkFlowTemplateRowDTO[]>([]);
  protected readonly isLoaded = signal(false);

  // Flags
  protected readonly buttonOKClicked = signal(false);
  private defaultAttestGroupId: number = 0;
  private highestAmount: number = 0;
  private supplierInvoiceIds: number[] = [];

  ngOnInit(): void {
    this.setInputsFromData();
    this.form.numberOfInvoicesText.setValue(
      this.supplierInvoiceIds.length.toString()
    );

    // Subscribe to form control changes
    this.form.attestWorkFlowHeadId.valueChanges.subscribe(value => {
      // check type since when changing drowpdown, two events are fired,
      // one with string and one with number
      if (value && typeof value === 'number') {
        this.groupChanged(value);
      }
    });

    this.form.attestWorkFlowTemplateHeadId.valueChanges.subscribe(value => {
      // check type since when changing drowpdown, two events are fired,
      // one with string and one with number
      if (value && typeof value === 'number') {
        this.loadCompanyTemplateRows(value);
      }
    });

    this.loadData();
  }

  private setInputsFromData(): void {
    this.supplierInvoiceIds = this.data.supplierInvoices.map(i => i.invoiceId);
    this.highestAmount = Math.max(
      ...this.data.supplierInvoices.map(i => i.totalAmount || 0)
    );
  }

  private loadData(): void {
    this.performLoadData
      .load$(
        forkJoin({
          groups: this.loadAttestGroups(),
          templates: this.loadTemplateHeads(),
          groupTypes: this.loadAttestGroupTypes(),
          roleUser: this.loadRoleUserTypes(),
          companySettings: this.loadCompanySettings(),
        }),
        { showDialogDelay: 500 }
      )
      .subscribe(() => {
        this.isLoaded.set(true);
        this.setRequiredUserMessage();
        if (this.defaultAttestGroupId) {
          this.form.attestWorkFlowHeadId.setValue(this.defaultAttestGroupId);
        }
      });
  }

  private loadAttestGroups(): Observable<ISmallGenericType[]> {
    return this.supplierService.getAttestWorkFlowGroupsDict(true).pipe(
      tap(data => {
        this.attestGroups.set(data);
      })
    );
  }

  private loadTemplateHeads(): Observable<IAttestWorkFlowTemplateHeadDTO[]> {
    return this.supplierService
      .getAttestWorkFlowTemplateHeadsForCurrentCompany()
      .pipe(
        tap(data => {
          this.templates.set(data);
        })
      );
  }

  private loadAttestGroupTypes(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.AttestWorkFlowType, false, false)
      .pipe(
        tap(data => {
          this.attestGroupTypes.set(data);
        })
      );
  }

  private loadRoleUserTypes(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.AttestWorkFlowApproverType, false, false)
      .pipe(
        tap(data => {
          this.role_User.set(data);
        })
      );
  }

  private loadCompanySettings(): Observable<void> {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired,
        CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired,
        CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup,
      ])
      .pipe(
        tap(settings => {
          this.requiredUserId = SettingsUtil.getIntCompanySetting(
            settings,
            CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired
          );
          this.totalAmountWhenUserRequired = SettingsUtil.getIntCompanySetting(
            settings,
            CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired
          );
          this.defaultAttestGroupId = SettingsUtil.getIntCompanySetting(
            settings,
            CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup
          );
        }),
        mergeMap(() => {
          return this.coreService.getUser(this.requiredUserId).pipe(
            tap(user => {
              this.requiredUser = user;
            })
          );
        }),
        map(() => undefined)
      );
  }

  private setRequiredUserMessage(): void {
    if (
      this.requiredUserId > 0 &&
      this.totalAmountWhenUserRequired <= this.highestAmount
    ) {
      const message = this.translateService.instant(
        'economy.supplier.attestgroup.invoicerequiresspecificuser'
      );
      if (message) {
        const userName = this.requiredUser?.name ?? '';
        const amount: string = NumberUtil.formatDecimal(
          this.totalAmountWhenUserRequired
        );
        const userRequiredMessage = message.format(amount, userName);
        this.userRequiredMessage.set(userRequiredMessage);
        this.showUserRequiredWarning.set(true);
      }
    }
  }

  private groupChanged(attestWorkFlowHeadId: number): void {
    this.performGroupChange
      .load$(
        this.supplierService.getAttestWorkFlowHead(
          attestWorkFlowHeadId,
          false,
          true
        ),
        { showDialogDelay: 500 }
      )
      .subscribe(head => {
        this.attestWorkFlowHead.set({
          ...head,
          attestWorkFlowGroupId: head.attestWorkFlowHeadId,
        });
        this.form.sendMessage.setValue(head.sendMessage ?? false);
        if (head.attestWorkFlowTemplateHeadId) {
          this.form.attestWorkFlowTemplateHeadId.setValue(
            head.attestWorkFlowTemplateHeadId
          );
        }
      });
  }

  private loadCompanyTemplateRows(templateHeadId: number): void {
    this.performTemplateLoad
      .load$(
        this.supplierService.getAttestWorkFlowTemplateHeadRows(templateHeadId),
        { showDialogDelay: 500 }
      )
      .subscribe(data => {
        const head = this.attestWorkFlowHead();
        data.forEach((row: IAttestWorkFlowTemplateRowDTO) => {
          head.rows?.forEach(r => {
            if (r.attestTransitionId === row.attestTransitionId) {
              row.type = r.type!;
            }
          });

          if (row.type == null) {
            row.type = head.type;
          }
        });

        this.templateRows.set(data);
      });
  }

  protected buttonOkClick(): void {
    this.buttonOKClicked.set(true);

    this.performCheckExisting
      .load$(
        this.supplierService.getAttestWorkFlowHeadFromInvoiceIds(
          this.supplierInvoiceIds
        ),
        { showDialogDelay: 500 }
      )
      .subscribe((attestWorkFlowHeads: IAttestWorkFlowHeadDTO[]) => {
        const existingAttestFlows: Array<{
          invoiceId: number;
          attestWorkFlowHeadId: number;
        }> = [];

        attestWorkFlowHeads.forEach((attestWorkFlowHead, index) => {
          if (attestWorkFlowHead && attestWorkFlowHead.attestWorkFlowHeadId) {
            existingAttestFlows.push({
              invoiceId: this.supplierInvoiceIds[index],
              attestWorkFlowHeadId: attestWorkFlowHead.attestWorkFlowHeadId,
            });
          }
        });

        if (existingAttestFlows.length > 0) {
          this.messageboxService
            .question(
              this.translateService.instant('core.verifyquestion'),
              this.translateService.instant(
                'economy.supplier.invoice.existingattestflowmessage'
              ),
              { buttons: 'yesNo' }
            )
            .afterClosed()
            .subscribe((response: IMessageboxComponentResponse) => {
              if (response.result) {
                this.saveAttestFlow();
              } else {
                this.buttonOKClicked.set(false);
              }
            });
        } else {
          this.saveAttestFlow();
        }
      });
  }

  private saveAttestFlow(): void {
    const head: IAttestWorkFlowHeadDTO = this.attestWorkFlowHead();

    if (!head.rows) {
      head.rows = [];
    }

    head.attestWorkFlowTemplateHeadId =
      this.form.attestWorkFlowTemplateHeadId.value;
    head.sendMessage = this.form.sendMessage.value;
    head.adminInformation = this.form.adminText.value;
    head.state = SoeEntityState.Active;
    head.entity = SoeEntityType.SupplierInvoice;

    const rows: IAttestWorkFlowRowDTO[] = [];
    const regRow = head.rows.find(
      r =>
        r.userId === SoeConfigUtil.userId &&
        r.processType === TermGroup_AttestWorkFlowRowProcessType.Registered &&
        r.answer
    );

    if (regRow) {
      rows.push(regRow);
    } else {
      const firstSelector = this.userSelectors()[0];
      rows.push({
        attestTransitionId: firstSelector?.getAttestTransitionId() || 0,
        userId: SoeConfigUtil.userId,
        processType: TermGroup_AttestWorkFlowRowProcessType.Registered,
        answer: true,
        type: head.type,
      } as IAttestWorkFlowRowDTO);
    }

    let rowsValid = true;
    let requiredUserIsSelected = false;
    let i = 0;

    this.userSelectors().forEach(
      (us: UserSelectorForTemplateHeadRowComponent) => {
        i++;
        const urows = us.getRowsToSave();

        if (!urows || urows.length === 0) {
          rowsValid = false;
        }

        urows.forEach((r: IAttestWorkFlowRowDTO) => {
          requiredUserIsSelected =
            r.userId === this.requiredUserId ? true : requiredUserIsSelected;
          r.processType =
            i === 1
              ? TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess
              : TermGroup_AttestWorkFlowRowProcessType.LevelNotReached;
          rows.push(r);
        });
      }
    );

    if (
      requiredUserIsSelected === false &&
      this.requiredUserId > 0 &&
      this.totalAmountWhenUserRequired <= this.highestAmount
    ) {
      this.userRequiredMessageType.set('error');
      this.buttonOKClicked.set(false);
    } else if (!rowsValid) {
      this.messageboxService.error(
        this.translateService.instant('core.error'),
        this.translateService.instant(
          'economy.supplier.invoice.attesthasinvalidrows'
        ),
        { buttons: 'ok' }
      );
      this.buttonOKClicked.set(false);
    } else {
      head.rows = rows;
      this.performSave.crud(
        CrudActionTypeEnum.Save,
        this.attestationService
          .saveAttestWorkFlowMultiple(head, this.supplierInvoiceIds)
          .pipe(
            tap((result: BackendResponse) => {
              this.buttonOKClicked.set(false);
              if (result.success) {
                this.dialogRef.close({
                  success: true,
                  affectedInvoiceCount: ResponseUtil.getEntityId(result),
                });
              } else {
                this.dialogRef.close({ success: false });
              }
            })
          )
      );
    }
  }
}
