import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  OnInit,
  Output,
  signal,
  SimpleChanges,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import {
  Feature,
  SoeEntityState,
  SoeTimeCodeType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, filter, Observable, take, tap } from 'rxjs';
import { ProjectPersonDialogComponent } from '../project-person-dialog/project-person-dialog.component';
import { TermCollection } from '@shared/localization/term-types';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { TimeService } from '@features/time/services/time.service';
import { ProjectPersonsDialogData } from '@features/billing/project/models/project-persons.model';
import { ProjectService } from '@features/billing/project/services/project.service';
import { IProjectUserExDTO } from '@features/billing/project/models/project.model';

@Component({
  selector: 'soe-project-persons',
  templateUrl: './project-persons.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProjectPersonsComponent
  extends GridBaseDirective<IProjectUserExDTO>
  implements OnInit, OnChanges
{
  readonly dialogService = inject(DialogService);
  coreService = inject(CoreService);
  timeService = inject(TimeService);
  projectService = inject(ProjectService);

  @Input() form: SoeFormGroup | undefined;
  @Input({ required: true }) persons: IProjectUserExDTO[] = [];
  @Output() personsChange = new EventEmitter<IProjectUserExDTO[]>();

  personRows = new BehaviorSubject<IProjectUserExDTO[]>([]);
  calculatedCostPermission: boolean = false;
  terms: any;
  userTypes: ISmallGenericType[] = [];
  users: ISmallGenericType[] = [];
  timeCodes: ISmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Supplier_Invoice_Invoices_Edit,
      'Billing.Projects.Directives.ProjectPersons',
      {
        skipInitialLoad: true,
        additionalModifyPermissions: [
          Feature.Billing_Project_EmployeeCalculateCost,
        ],
        lookups: [this.loadUserTypes(), this.loadUsers(), this.loadTimeCodes()],
      }
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.persons && changes.persons.currentValue) {
      this.setData();
    }
  }

  override onPermissionsLoaded(): void {
    this.calculatedCostPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Project_EmployeeCalculateCost
    );
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms(['billing.projects.list.adduser']);
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideClearFilters: true,
      hideReload: true,
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton(
          'billing.projects.list.new_person',
          {
            iconName: signal('plus'),
            caption: signal('billing.projects.list.new_person'),
            tooltip: signal('billing.projects.list.new_person'),
            onAction: this.addOrEditPerson.bind(this, undefined),
          }
        ),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IProjectUserExDTO>): void {
    super.onGridReadyToDefine(grid);
    const keys: string[] = [
      'billing.projects.list.name',
      'billing.projects.list.participanttype',
      'billing.projects.list.starting',
      'billing.projects.list.ending',
      'billing.projects.list.timecodename',
      'core.edit',
      'core.delete',
      'core.aggrid.totals.filtered',
      'core.aggrid.totals.total',
      'billing.project.calculatedcost',
    ];
    this.translate
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');
        this.grid.addColumnSelect(
          'userId',
          terms['billing.projects.list.name'],
          this.users,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnSelect(
          'type',
          terms['billing.projects.list.participanttype'],
          this.userTypes,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnDate(
          'dateFrom',
          terms['billing.projects.list.starting'],
          { flex: 1 }
        );
        this.grid.addColumnDate(
          'dateTo',
          terms['billing.projects.list.ending'],
          { flex: 1 }
        );
        this.grid.addColumnSelect(
          'timeCodeId',
          terms['billing.projects.list.timecodename'],
          this.timeCodes,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 1,
            editable: false,
          }
        );

        if (this.calculatedCostPermission)
          this.grid.addColumnNumber(
            'employeeCalculatedCost',
            terms['billing.project.calculatedcost'],
            { flex: 1 }
          );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => this.addOrEditPerson(row),
          suppressFilter: true,
        });

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => this.deletePerson(row),
          suppressFilter: true,
        });

        this.grid.context.suppressGridMenu = true;

        super.finalizeInitGrid();
      });
  }

  private setData(): void {
    this.personRows.next(
      this.persons.filter(x => x.state !== SoeEntityState.Deleted)
    );
    this.grid?.refreshCells();
  }

  addOrEditPerson(rowToUpdate?: IProjectUserExDTO): void {
    const dialogData: ProjectPersonsDialogData = {
      title: this.terms['billing.projects.list.adduser'],
      size: 'md',
      rowToUpdate,
      calculatedCostPermission: this.calculatedCostPermission,
      userTypes: this.userTypes,
      users: this.users,
      timeCodes: this.timeCodes,
    };

    this.dialogService
      .open(ProjectPersonDialogComponent, dialogData)
      .afterClosed()
      .pipe(filter(value => !!value))
      .subscribe((value: IProjectUserExDTO) => {
        if (rowToUpdate) {
          this.personRows.pipe(take(1)).subscribe(rows => {
            const indexToUpdate = rows.indexOf(rowToUpdate);
            value.isModified = true;
            this.personsChange.emit(
              this.persons.map((item, i) =>
                i === indexToUpdate ? value : item
              )
            );
            this.form?.markAsDirty();
          });
        } else {
          value.isModified = true;
          this.personsChange.emit([...this.persons, value]);
          this.form?.markAsDirty();
        }
      });
  }

  deletePerson(row: IProjectUserExDTO): void {
    this.personRows.pipe(take(1)).subscribe(rows => {
      const indexToRemove = rows.indexOf(row);
      if (row.projectUserId === 0) {
        this.persons.splice(indexToRemove, 1);
      } else {
        this.persons[indexToRemove].state = SoeEntityState.Deleted;
        this.persons[indexToRemove].isModified = true;
      }
      this.personsChange.emit(this.persons);
      this.form?.markAsDirty();
    });
  }

  private loadUserTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.ProjectUserType, false, false)
      .pipe(
        tap(x => {
          this.userTypes = x;
        })
      );
  }

  private loadUsers() {
    return this.coreService.getUsersDict(true, false, true, false, false).pipe(
      tap(x => {
        this.users = x;
      })
    );
  }

  private loadTimeCodes() {
    return this.timeService
      .getTimeCodesDictByType(
        SoeTimeCodeType.WorkAndAbsense,
        true,
        false,
        false,
        false,
        false
      )
      .pipe(tap(x => (this.timeCodes = x)));
  }
}
