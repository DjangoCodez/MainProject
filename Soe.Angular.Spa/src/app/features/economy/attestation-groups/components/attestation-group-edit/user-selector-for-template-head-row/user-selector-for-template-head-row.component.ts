import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { FormControl } from '@angular/forms';
import { SupplierService } from '@features/economy/services/supplier.service';
import { TranslateService } from '@ngx-translate/core';
import { UserSmallDTO } from '@shared/components/billing/select-users-dialog/models/select-users-dialog.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  TermGroup_AttestWorkFlowRowProcessType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IAttestWorkFlowHeadDTO,
  IAttestWorkFlowRowDTO,
  IAttestWorkFlowTemplateRowDTO,
  IUserSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  BehaviorSubject,
  merge,
  Observable,
  of,
  Subject,
  take,
  takeLast,
  takeUntil,
  tap,
} from 'rxjs';

@Component({
  selector: 'soe-user-selector-for-template-head-row',
  templateUrl: './user-selector-for-template-head-row.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class UserSelectorForTemplateHeadRowComponent
  extends GridBaseDirective<Checkable<UserSmallDTO>>
  implements OnInit, OnDestroy
{
  destroy$ = new Subject<void>();

  translate = inject(TranslateService);
  coreService = inject(CoreService);
  supplierService = inject(SupplierService);

  @Input() row!: IAttestWorkFlowTemplateRowDTO;
  @Input() mode!: FormControl<number | null>;
  @Input() head!: IAttestWorkFlowHeadDTO;
  @Input() attestWorkFlowTypes!: ISmallGenericType[];
  @Output() rowChange = new EventEmitter<{
    rows: IAttestWorkFlowRowDTO[];
    gridId: string;
    changed: boolean;
  }>();

  gridRows = new BehaviorSubject<Checkable<IUserSmallDTO>[]>([]);
  selectedType = new FormControl<number>(0);
  relevantRows!: IAttestWorkFlowRowDTO[];
  checkableUsers: Checkable<IUserSmallDTO>[] = [];
  checkableRoles: Checkable<IUserSmallDTO>[] = [];
  currentMode: number = 0;
  loginNameColumn: string = 'entity.loginName';

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.None, 'NoName', { skipInitialLoad: true });

    this.selectedType.setValue(this.row.type ?? 0);
    this.setupWatchers();
  }

  private setupWatchers(): void {
    this.selectedType.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        tap(value => {
          this.row.type = value ?? 0;
          this.emitUpdatedRows(this.gridRows.getValue(), true);
        })
      )
      .subscribe();

    this.mode.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        tap(value => {
          this.currentMode = value ?? 0;
          this.updateGridData();
        })
      )
      .subscribe();
  }

  private loadGridData(): Observable<IUserSmallDTO[] | null> {
    let isNewTemplate = false;
    if (!this.row.attestTransitionId) return of(null);
    if (
      this.row.attestWorkFlowTemplateHeadId !==
      this.head.attestWorkFlowTemplateHeadId
    ) {
      isNewTemplate = true;
    }
    const userObservable = this.supplierService
      .getAttestWorkFlowUsersByAttestTransition(this.row.attestTransitionId)
      .pipe(
        takeLast(1),
        tap(users => {
          this.checkableUsers = users.map(user => new Checkable(user));

          if (!this.head.rows) return;

          this.relevantRows = this.head.rows.filter(
            row =>
              row.attestTransitionId === this.row.attestTransitionId &&
              row.processType !==
                TermGroup_AttestWorkFlowRowProcessType.Registered
          );

          if (isNewTemplate) return;
          this.relevantRows.forEach(row => {
            const user = this.checkableUsers.find(
              u => u.entity.userId === row.userId
            );
            if (user) user.checked = true;
          });
        })
      );

    const roleObservable = this.supplierService
      .getAttestWorkFlowAttestRolesByAttestTransition(
        this.row.attestTransitionId
      )
      .pipe(
        takeLast(1),
        tap(roles => {
          this.checkableRoles = roles.map(role => new Checkable(role));

          if (!this.head.rows) return;

          this.relevantRows = this.head.rows.filter(
            row =>
              row.attestTransitionId === this.row.attestTransitionId &&
              row.processType !==
                TermGroup_AttestWorkFlowRowProcessType.Registered
          );

          if (isNewTemplate) return;
          this.relevantRows.forEach(row => {
            const role = this.checkableRoles.find(
              r => r.entity.attestRoleId === row.attestRoleId
            );
            if (role) role.checked = true;
          });
        })
      );

    return merge(userObservable, roleObservable);
  }

  override onGridReadyToDefine(
    grid: GridComponent<Checkable<UserSmallDTO>>
  ): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get(['common.name', 'common.user', 'common.categories.selected'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.context.suppressGridMenu = true;

        this.grid.addColumnBool(
          'checked',
          terms['common.categories.selected'],
          {
            width: 100,
            suppressFilter: true,
            editable: true,
            alignCenter: true,
            suppressExport: true,
            suppressSizeToFit: true,
            sort: 'desc',
            onClick: (value, row) => this.updateChecked(row, value),
          }
        );
        this.grid.addColumnText('entity.name', terms['common.name'], {
          suppressFilter: true,
          suppressExport: true,
          flex: 1,
        });
        this.grid.addColumnText('entity.loginName', terms['common.user'], {
          suppressFilter: true,
          suppressExport: true,
          flex: 1,
        });
        super.finalizeInitGrid();
        this.loadGridData()
          .pipe(
            tap(() => {
              this.updateGridData();
              this.emitUpdatedRows(this.gridRows.getValue());
            })
          )
          .subscribe();
      });
  }

  updateChecked(row: Checkable<UserSmallDTO>, value: boolean): void {
    const rows = this.gridRows.getValue();
    if (this.currentMode == 0) {
      const user = rows.find(r => r.entity.userId === row.entity.userId);
      if (user) user.checked = value;
    } else {
      const role = rows.find(
        r => r.entity.attestRoleId === row.entity.attestRoleId
      );
      if (role) role.checked = value;
    }
    this.emitUpdatedRows(rows, true);
    this.updateGridData();
  }

  emitUpdatedRows(
    checkedRows: Checkable<IUserSmallDTO>[],
    changed: boolean = false
  ): void {
    const rows: IAttestWorkFlowRowDTO[] = [];
    let row: IAttestWorkFlowRowDTO | null;

    const attestUsers = checkedRows
      .filter(cu => cu.checked)
      .map(cu => cu.entity);

    attestUsers.forEach(user => {
      row = null;
      if (user.attestFlowRowId !== 0) {
        row = this.head.rows.find(r => r.userId === user.userId) ?? null;
      }

      if (!row) {
        row = <IAttestWorkFlowRowDTO>{};
      }
      row.attestTransitionId = this.row.attestTransitionId;
      row.attestWorkFlowRowId = user.attestFlowRowId;
      row.type = this.row.type;

      if (!user.userId) {
        row.attestRoleId = user.attestRoleId;
      } else {
        row.userId = user.userId;
      }

      rows.push(row);
    });

    this.rowChange.emit({ rows: rows, gridId: this.guid(), changed: changed });
  }

  updateGridData(): void {
    if (this.currentMode === 0) {
      //this.gridRows?.next(this.checkableUsers);
      this.gridRows = new BehaviorSubject<Checkable<IUserSmallDTO>[]>(
        this.checkableUsers || []
      );
      this.grid?.showColumns([this.loginNameColumn]);
    } else {
      //this.gridRows?.next(this.checkableRoles);
      this.gridRows = new BehaviorSubject<Checkable<IUserSmallDTO>[]>(
        this.checkableRoles || []
      );
      this.grid?.hideColumns([this.loginNameColumn]);
    }
    this.sortGrid();
  }

  sortGrid(): void {
    this.grid?.sort('checked', 'desc');
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

class Checkable<T> {
  public entity: T;
  public checked: boolean;

  constructor(entity: T) {
    this.entity = entity;
    this.checked = false;
  }
}
