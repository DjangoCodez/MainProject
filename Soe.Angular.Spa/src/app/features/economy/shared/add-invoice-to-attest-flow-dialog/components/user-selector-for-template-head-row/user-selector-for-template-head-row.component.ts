import { Component, inject, signal, input, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import {
  IAttestWorkFlowHeadDTO,
  IAttestWorkFlowRowDTO,
  IAttestWorkFlowTemplateRowDTO,
  IUserSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SupplierService } from '@features/economy/services/supplier.service';
import { CoreService } from '@shared/services/core.service';
import {
  TermGroup,
  Feature,
  TermGroup_AttestWorkFlowRowProcessType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Checkable } from '../../models/checkable.model';
import { GridComponent } from '@ui/grid/grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TermCollection } from '@shared/localization/term-types';
import { UserSelectorForm } from '../../models/user-selector-form.model';
import { ValidationHandler } from '@shared/handlers/validation.handler';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { Perform } from '@shared/util/perform.class';
import { CellClickedEvent } from 'ag-grid-community';

@Component({
  selector: 'soe-user-selector-for-template-head-row',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    GridWrapperComponent,
    SelectComponent,
    InstructionComponent,
  ],
  templateUrl: './user-selector-for-template-head-row.component.html',
  styleUrls: ['./user-selector-for-template-head-row.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
})
export class UserSelectorForTemplateHeadRowComponent extends GridBaseDirective<
  Checkable<IUserSmallDTO>
> {
  row = input.required<IAttestWorkFlowTemplateRowDTO>();
  head = input.required<IAttestWorkFlowHeadDTO>();
  mode = input<number>(0); // 0 = Users, 1 = Roles

  private readonly supplierService = inject(SupplierService);
  private readonly coreService = inject(CoreService);
  validationHandler = inject(ValidationHandler);

  protected checkableUsers = signal<Checkable<IUserSmallDTO>[]>([]);
  protected checkableRoles = signal<Checkable<IUserSmallDTO>[]>([]);
  protected attestWorkFlowTypes = signal<ISmallGenericType[]>([]);

  protected readonly form: UserSelectorForm;

  gridName = `UserSelectorForTemplateHeadRow_${crypto.randomUUID()}`;

  private readonly performLoadTypes = new Perform<ISmallGenericType[]>(
    this.progressService
  );
  private readonly performLoadUsers = new Perform<IUserSmallDTO[]>(
    this.progressService
  );
  private readonly performLoadRoles = new Perform<IUserSmallDTO[]>(
    this.progressService
  );

  constructor() {
    super();

    this.form = new UserSelectorForm({
      validationHandler: this.validationHandler,
    });

    // Watch for mode changes and update grid
    effect(() => {
      const currentMode = this.mode();

      if (this.grid) {
        // Toggle loginName column visibility (show for Users, hide for Roles)
        if (currentMode === 0) {
          this.grid.showColumns(['entity.loginName']);
        } else {
          this.grid.hideColumns(['entity.loginName']);
        }

        this.refreshGrid();
      }
    });
  }

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Supplier_Invoice_AttestFlow, this.gridName, {
      lookups: [
        this.loadAttestWorkFlowTypes(),
        this.loadAttestWorkFlowUsers(),
        this.loadAttestWorkFlowAttestRoles(),
      ],
      skipInitialLoad: true,
      skipDefaultToolbar: true,
    });
  }

  override loadTerms(): Observable<TermCollection> {
    const translationsKeys = [
      'common.categories.selected',
      'common.name',
      'common.user',
      'economy.supplier.attestgroup.type',
    ];

    return super.loadTerms(translationsKeys);
  }

  override onFinished(): void {
    super.onFinished();
    this.form.type.setValue(this.row().type, { emitEvent: false });
    this.refreshGrid();
  }

  override loadData(): Observable<Checkable<IUserSmallDTO>[]> {
    const data =
      this.mode() === 0 ? this.checkableUsers() : this.checkableRoles();
    const sortedData = this.sortData(data);
    return of(sortedData);
  }

  private sortData(
    data: Checkable<IUserSmallDTO>[]
  ): Checkable<IUserSmallDTO>[] {
    return [...data].sort((a, b) => {
      if (a.checked !== b.checked) {
        return a.checked ? -1 : 1;
      }
      return (a.entity.name || '').localeCompare(b.entity.name || '');
    });
  }

  private loadAttestWorkFlowTypes(): Observable<ISmallGenericType[]> {
    return this.performLoadTypes.load$(
      this.coreService
        .getTermGroupContent(TermGroup.AttestWorkFlowType, false, false)
        .pipe(
          tap(data => {
            this.attestWorkFlowTypes.set(data);
          })
        ),
      { showDialogDelay: 500 }
    );
  }

  private loadAttestWorkFlowUsers(): Observable<IUserSmallDTO[]> {
    return this.performLoadUsers.load$(
      this.supplierService
        .getAttestWorkFlowUsersByAttestTransition(this.row().attestTransitionId)
        .pipe(
          tap((data: IUserSmallDTO[]) => {
            const checkables = data.map(u => new Checkable(u));

            if (this.head().rows) {
              const relevantRows = this.head().rows.filter(
                r =>
                  r.attestTransitionId === this.row().attestTransitionId &&
                  r.processType !==
                    TermGroup_AttestWorkFlowRowProcessType.Registered
              );

              relevantRows.forEach(rr => {
                const user = checkables.find(
                  u => u.entity.userId === rr.userId
                );
                if (user) {
                  user.checked = true;
                }
              });
            }

            this.checkableUsers.set(checkables);
          })
        ),
      { showDialogDelay: 500 }
    );
  }

  private loadAttestWorkFlowAttestRoles(): Observable<IUserSmallDTO[]> {
    return this.performLoadRoles.load$(
      this.supplierService
        .getAttestWorkFlowAttestRolesByAttestTransition(
          this.row().attestTransitionId
        )
        .pipe(
          tap((data: IUserSmallDTO[]) => {
            const checkables = data.map(r => new Checkable(r));

            if (this.head().rows) {
              const relevantRows = this.head().rows.filter(
                r =>
                  r.attestTransitionId === this.row().attestTransitionId &&
                  r.processType !==
                    TermGroup_AttestWorkFlowRowProcessType.Registered
              );

              relevantRows.forEach(rr => {
                const role = checkables.find(
                  u => u.entity.attestRoleId === rr.attestRoleId
                );
                if (role) {
                  role.checked = true;
                }
              });
            }

            this.checkableRoles.set(checkables);
          })
        ),
      { showDialogDelay: 500 }
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<Checkable<IUserSmallDTO>>
  ): void {
    super.onGridReadyToDefine(grid);
    this.setColumns();
    this.grid.context.suppressGridMenu = true;
    this.grid.context.suppressFiltering = true;
    super.finalizeInitGrid({ hidden: true });
  }

  private setColumns(): void {
    this.grid.addColumnBool(
      'checked',
      this.terms['common.categories.selected'],
      {
        flex: 0,
        width: 80,
        editable: true,
        onClick: () => {
          this.onCheckboxClicked();
        },
      }
    );

    this.grid.addColumnText('entity.name', this.terms['common.name'], {
      flex: 1,
    });

    this.grid.addColumnText('entity.loginName', this.terms['common.user'], {
      flex: 1,
      hide: this.mode() !== 0,
    });
  }

  private onCheckboxClicked(): void {
    queueMicrotask(() => {
      this.refreshGrid();
    });
  }

  protected onCellClicked(event: CellClickedEvent): void {
    if (event.colDef.field !== 'checked' && !event.data.checked) {
      event.data.checked = true;
      queueMicrotask(() => {
        this.refreshGrid();
      });
    }
  }

  public getRowsToSave(): IAttestWorkFlowRowDTO[] {
    const rows: IAttestWorkFlowRowDTO[] = [];

    const attestUsers = this.rowData.value
      .filter(cu => cu.checked)
      .map(cu => cu.entity);

    attestUsers.forEach(user => {
      let row: IAttestWorkFlowRowDTO | undefined;

      if (user.attestFlowRowId && user.attestFlowRowId !== 0) {
        row = this.head().rows?.find(
          r => r.attestWorkFlowRowId === user.attestFlowRowId
        );
      }

      if (!row) {
        row = {} as IAttestWorkFlowRowDTO;
      }

      row.attestTransitionId = this.row().attestTransitionId;
      row.attestWorkFlowRowId = user.attestFlowRowId;
      row.type = this.form.type.value;

      if (!user.userId) {
        row.attestRoleId = user.attestRoleId;
      } else {
        row.userId = user.userId;
      }

      rows.push(row);
    });

    return rows;
  }

  public getAttestTransitionId(): number {
    return this.row().attestTransitionId;
  }

}
