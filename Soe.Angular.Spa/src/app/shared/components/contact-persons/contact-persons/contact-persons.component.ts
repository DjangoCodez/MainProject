import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { EditComponentDialogData } from '@ui/dialog/edit-component-dialog/edit-component-dialog.component';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, take, tap } from 'rxjs';
import { ContactPersonDTO } from '../models/contact-persons.model';
import { ContactPersonsService } from '../services/contact-persons.service';
import { ContactPersonForm } from '../models/contact-persons-form.model';
import { ContactPersonsEditComponent } from '@src/app/features/manage/contact-persons/components/contact-persons-edit/contact-persons-edit.component';
import { ValidationHandler } from '@shared/handlers';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-contact-persons',
  templateUrl: './contact-persons.component.html',
  styleUrls: ['./contact-persons.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ContactPersonsComponent
  extends GridBaseDirective<ContactPersonDTO>
  implements OnInit, OnChanges
{
  @Input({ required: true }) actorId!: number;
  @Input() form: SoeFormGroup | undefined;

  @Output() personsChange = new EventEmitter<ContactPersonDTO[]>();

  contactPersonService = inject(ContactPersonsService);
  readonly coreService = inject(CoreService);
  readonly dialogService = inject(DialogService);

  validationHandler = inject(ValidationHandler);

  persons: ContactPersonDTO[] = [];
  personRows = new BehaviorSubject<ContactPersonDTO[]>([]);
  availablePersons: ContactPersonDTO[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.None, 'Common.Directives.ContactPersons', {
      skipInitialLoad: true,
      lookups: [this.loadAvailablePersons()],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.actorId && this.actorId) {
      this.contactPersonService
        .getContactPersonsByActorId(this.actorId)
        .subscribe(rows => {
          this.persons = rows;
          this.setData(rows, false);
        });
    }
  }

  loadAvailablePersons() {
    return this.contactPersonService
      .getContactPersonsByActorId(SoeConfigUtil.actorCompanyId)
      .pipe(
        tap(res => {
          this.availablePersons = res;
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<ContactPersonDTO>) {
    super.onGridReadyToDefine(grid);

    const keys: string[] = [
      'common.contactperson.addcontactperson',
      'common.firstname',
      'common.lastname',
      'common.email',
      'common.phone',
      'core.edit',
      'core.remove',
      'common.contactperson.addcontactperson',
    ];
    this.translate
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('firstName', terms['common.firstname'], {
          flex: 1,
        });
        this.grid.addColumnText('lastName', terms['common.lastname'], {
          flex: 1,
        });
        this.grid.addColumnText('email', terms['common.email'], {
          flex: 1,
        });
        this.grid.addColumnText('phoneNumber', terms['common.phone'], {
          flex: 1,
        });
        this.grid.addColumnIconEdit({
          suppressFilter: true,
          onClick: row => this.showEditPerson(row),
        });
        this.grid.addColumnIconDelete({
          suppressFilter: true,
          onClick: row => this.deletePerson(row),
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid();
      });
  }

  addExistingContactPerson(person: ISmallGenericType) {
    if (!person || !person.id) return;
    if (!this.persons.some(x => x.actorContactPersonId === person.id)) {
      const contactPerson = this.availablePersons.find(
        x => x.actorContactPersonId === person.id
      );
      if (contactPerson) {
        this.persons = [...this.persons, contactPerson];
        this.setData(this.persons, true);
      }
    }
  }

  addNewContactPerson() {
    const dialogData: EditComponentDialogData<
      ContactPersonDTO,
      ContactPersonsService,
      ContactPersonForm
    > = {
      title: this.translate.instant('common.contactperson.addcontactperson'),
      size: 'xl',
      form: new ContactPersonForm({
        validationHandler: this.validationHandler,
        element: undefined,
        openInDialog: true,
      }),
      editComponent: ContactPersonsEditComponent,
    };
    this.dialogService
      .openEditComponent(dialogData)
      .afterClosed()
      .subscribe(
        (result: { response: BackendResponse; value: ContactPersonDTO }) => {
          if (!result) return;
          this.persons = [...this.persons, result.value];
          this.setData(this.persons, true);
        }
      );
  }

  showEditPerson(rowToUpdate: ContactPersonDTO) {
    const dialogData: EditComponentDialogData<
      ContactPersonDTO,
      ContactPersonsService,
      ContactPersonForm
    > = {
      title: this.translate.instant('common.contactperson.addcontactperson'),
      size: 'xl',
      form: new ContactPersonForm({
        validationHandler: this.validationHandler,
        element: rowToUpdate,
        openInDialog: true,
      }),
      editComponent: ContactPersonsEditComponent,
    };
    this.dialogService
      .openEditComponent(dialogData)
      .afterClosed()
      .subscribe(
        (result: { response: BackendResponse; value: ContactPersonDTO }) => {
          if (!result) return;
          this.persons = this.persons.map(x =>
            x.actorContactPersonId === rowToUpdate.actorContactPersonId
              ? result.value
              : x
          );
          this.setData(this.persons, false);
        }
      );
  }

  deletePerson(row: ContactPersonDTO) {
    this.persons = this.persons.filter(
      x => x.actorContactPersonId !== row.actorContactPersonId
    );
    this.setData(this.persons, true);
  }

  private setData(rows: ContactPersonDTO[], dirty: boolean) {
    this.rowData.next(rows);
    this.personsChange.emit(rows);
    if (dirty) {
      this.form?.markAsDirty();
    }
  }
}
