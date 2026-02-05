import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { AttestationGroupForm } from '../../models/attestation-group-form.model';
import { AttestWorkFlowHeadDTO } from '../../models/attestation-groups.model';
import { AttestationGroupsService } from '../../services/attestation-groups.service';
import {
  Feature,
  SoeEntityState,
  TermGroup,
  TermGroup_AttestWorkFlowRowProcessType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  IAttestWorkFlowRowDTO,
  IAttestWorkFlowTemplateHeadDTO,
  IAttestWorkFlowTemplateRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SupplierService } from '@features/economy/services/supplier.service';
import { Observable, of, tap, concatMap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { FormControl } from '@angular/forms';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-attestation-group-edit',
  templateUrl: './attestation-group-edit.component.html',
  styleUrl: './attestation-group-edit.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AttestationGroupEditComponent
  extends EditBaseDirective<
    AttestWorkFlowHeadDTO,
    AttestationGroupsService,
    AttestationGroupForm
  >
  implements OnInit
{
  service = inject(AttestationGroupsService);
  coreService = inject(CoreService);
  supplierService = inject(SupplierService);

  templates: IAttestWorkFlowTemplateHeadDTO[] = [];
  role_User: ISmallGenericType[] = [];
  attestWorkFlowTypes: ISmallGenericType[] = [];

  attestGroup!: AttestWorkFlowHeadDTO;
  userSelectorRows: Record<string, IAttestWorkFlowRowDTO[]> = {};
  templateRows: IAttestWorkFlowTemplateRowDTO[] = [];
  roleOrUser = new FormControl<number>(0);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Preferences_SuppInvoiceSettings_AttestGroups,
      {
        lookups: [
          this.loadTemplates(),
          this.loadRoleUsers(),
          this.loadGroupTypes(),
        ],
      }
    );
  }

  loadTemplates(): Observable<IAttestWorkFlowTemplateHeadDTO[]> {
    return this.supplierService
      .getAttestWorkFlowTemplateHeadsForCurrentCompany()
      .pipe(tap(templates => (this.templates = templates)));
  }

  loadRoleUsers(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.AttestWorkFlowApproverType, false, false)
      .pipe(tap(role_User => (this.role_User = role_User)));
  }

  loadGroupTypes(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.AttestWorkFlowType, false, false)
      .pipe(tap(types => (this.attestWorkFlowTypes = types)));
  }

  doCompanyTemplateRowsLookups(): Observable<
    IAttestWorkFlowTemplateRowDTO[] | undefined
  > {
    if (!this.form?.value.attestWorkFlowTemplateHeadId) return of(undefined);
    return this.loadCompanyTemplateRows(
      this.form?.value.attestWorkFlowTemplateHeadId
    );
  }

  templateChanged(templateHeadId: number | undefined) {
    const template = this.templates.find(
      t => t.attestWorkFlowTemplateHeadId == templateHeadId
    );
    if (!template) return;
    if (!this.attestGroup) this.new();
    this.attestGroup.type = template?.type;
    this.attestGroup.rows = [];
    this.templateRows = [];
    this.userSelectorRows = {};
    if (!templateHeadId) return;
    this.performLoadData
      .load$(this.loadCompanyTemplateRows(templateHeadId, true))
      .subscribe();
  }

  private loadCompanyTemplateRows(
    templateHeadId: number,
    templateChanged: boolean = false
  ): Observable<IAttestWorkFlowTemplateRowDTO[]> {
    return this.supplierService
      .getAttestWorkFlowTemplateHeadRows(templateHeadId)
      .pipe(
        tap(data => {
          this.templateRows = data;

          this.templateRows.map(row => {
            if (row.type == null) row.type = this.form?.value.type;

            if (!templateChanged) {
              const groupRow = this.attestGroup.rows.find(
                r => r.attestTransitionId == row.attestTransitionId
              )?.type;
              if (groupRow != null) row.type = groupRow;
            }
            return row;
          });
        })
      );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.attestGroup = value;
          this.form?.reset(value);
          this.templateRows = [];
          if (!this.attestGroup) this.new();
        }),
        concatMap(() => this.doCompanyTemplateRowsLookups())
      )
    );
  }

  performSave(options?: ProgressOptions): void {
    if (this.attestGroup.rows == null) this.attestGroup.rows = [];

    this.attestGroup.state = SoeEntityState.Active;

    const rows = [];
    const regRow = this.attestGroup.rows.find(
      r =>
        r.userId === SoeConfigUtil.userId &&
        r.processType === TermGroup_AttestWorkFlowRowProcessType.Registered &&
        r.answer
    );

    if (regRow != null) {
      // Registration row exists
      rows.push(regRow);
    } else {
      if (this.templateRows.length <= 0) return;
      // Add user that registered the flow, just for logging
      rows.push({
        attestTransitionId: this.templateRows[0].attestTransitionId,
        userId: SoeConfigUtil.userId,
        processType: TermGroup_AttestWorkFlowRowProcessType.Registered,
        answer: true,
        type: this.templateRows[0].type,
      } as IAttestWorkFlowRowDTO);
    }

    let i = 0;
    const userSelectorIds = Object.keys(this.userSelectorRows) as string[];

    userSelectorIds.forEach(id => {
      i++;

      const uRows = this.userSelectorRows[id];

      uRows.forEach(row => {
        row.processType =
          i === 1
            ? TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess
            : TermGroup_AttestWorkFlowRowProcessType.LevelNotReached;
        rows.push(row);
      });
    });

    this.attestGroup.rows = rows;
    this.attestGroup.attestGroupCode = this.form?.value.attestGroupCode;
    this.attestGroup.attestGroupName = this.form?.value.attestGroupName;
    this.attestGroup.sendMessage = this.form?.value.sendMessage;
    this.attestGroup.attestWorkFlowTemplateHeadId =
      this.form?.value.attestWorkFlowTemplateHeadId;
    this.attestGroup.isAttestGroup = true;

    if (!this.form || this.form.invalid || !this.service) return;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(this.attestGroup).pipe(
        tap(res => {
          this.updateFormValueAndEmitChange(res);
          if (res.success) this.triggerCloseDialog(res);
        })
      ),
      undefined,
      undefined,
      options
    );
  }

  rowChanged(event: {
    rows: IAttestWorkFlowRowDTO[];
    gridId: string;
    changed: boolean;
  }) {
    const { gridId, rows, changed } = event;
    this.userSelectorRows[gridId] = rows;
    if (!changed) return;
    this.form?.markAsDirty();
  }

  private new() {
    this.attestGroup = <AttestWorkFlowHeadDTO>{}; //we just fake it
    this.attestGroup.sendMessage = true;
  }
}
