import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  Output,
  signal,
  SimpleChanges,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, filter, Observable, take, tap } from 'rxjs';
import { Guid } from '@shared/util/string-util';
import {
  HouseholdTaxDeductionApplicantDialogData,
  HouseholdTaxDeductionApplicantDTO,
} from '@shared/components/household-tax-deduction-dialog/models/household-tax-deduction-Applicant.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { SoeFormGroup } from '@shared/extensions';
import { HouseholdTaxDeductionDialogComponent } from '@shared/components/household-tax-deduction-dialog/household-tax-deduction-dialog.component';
import { IHouseholdTaxDeductionApplicantDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TermCollection } from '@shared/localization/term-types';
import { ProgressService } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'soe-tax-deduction-contacts',
  templateUrl: './tax-deduction-contacts.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TaxDeductionContactsComponent
  extends GridBaseDirective<HouseholdTaxDeductionApplicantDTO>
  implements OnChanges
{
  readonly dialogService = inject(DialogService);
  readonly coreService = inject(CoreService);
  readonly handler = inject(FlowHandlerService);
  readonly progress = inject(ProgressService);
  readonly performLoad = new Perform<HouseholdTaxDeductionApplicantDTO[]>(
    this.progress
  );

  @Input() form: SoeFormGroup | undefined;
  @Input() customerId: number = 0;
  @Input() contacts: IHouseholdTaxDeductionApplicantDTO[] = [];
  @Output() contactChange = new EventEmitter<
    IHouseholdTaxDeductionApplicantDTO[]
  >();

  terms: any;
  contactRows = new BehaviorSubject<HouseholdTaxDeductionApplicantDTO[]>([]);
  parentGuid = Guid.newGuid();
  showAllApplicants = signal(false);

  ngOnInit(): void {
    super.ngOnInit();
    this.handler.execute({
      permission: Feature.None,
      skipInitialLoad: true,
      lookups: [this.loadTerms(), this.loadRotData()],
      setupGrid: this.setupGrid.bind(this),
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.showAllApplicants && changes.showAllApplicants.currentValue) {
    }
    if (changes.contacts && changes.contacts.currentValue) {
      this.setData();
    }
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'common.customer.customer.rot.socialsecnr',
      'common.customer.customer.rot.name',
      'common.customer.customer.rot.property',
      'common.customer.customer.rot.apartmentnr',
      'common.customer.customer.rot.cooperativeorgnr',
      'core.edit',
      'core.delete',
      'common.type',
      'common.invoicenr',
      'common.productnr',
      'common.name',
      'common.quantity',
      'common.amount',
      'common.purchaseprice',
      'common.price',
      'common.date',
      'common.customer.customer.marginalincome',
      'common.customer.customer.marginalincomeratioprocent',
      'common.customer.customer.rot.register',
    ]);
  }

  loadRotData() {
    return this.performLoad.load$(
      this.coreService
      .getHouseholdTaxDeductionRowsByCustomer(
        this.customerId,
        false,
        this.showAllApplicants(),
        false
      )
      .pipe(
        tap(x => {
            this.contacts = x;
            this.setData();
        })
        )
      );
  }

  setupGrid(grid: GridComponent<HouseholdTaxDeductionApplicantDTO>): void {
    super.setupGrid(grid, '');

    this.grid.addColumnText(
      'socialSecNr',
      this.terms['common.customer.customer.rot.socialsecnr'],
      { flex: 1 }
    );
    this.grid.addColumnText(
      'name',
      this.terms['common.customer.customer.rot.name'],
      { flex: 1 }
    );
    this.grid.addColumnText(
      'property',
      this.terms['common.customer.customer.rot.property'],
      { flex: 1 }
    );
    this.grid.addColumnText(
      'apartmentNr',
      this.terms['common.customer.customer.rot.apartmentnr'],
      { flex: 1 }
    );
    this.grid.addColumnText(
      'cooperativeOrgNr',
      this.terms['common.customer.customer.rot.cooperativeorgnr'],
      { flex: 1 }
    );
    this.grid.addColumnIconEdit({
      tooltip: this.terms['core.edit'],
      onClick: row => this.editContact(row),
      suppressFilter: true,
    });
    this.grid.addColumnIconDelete({
      onClick: row => this.deleteContact(row),
      suppressFilter: true,
    });

    this.grid.setNbrOfRowsToShow(5);
    this.grid.context.suppressGridMenu = true;

    super.finalizeInitGrid({ hidden: true });
  }

  deleteContact(row: HouseholdTaxDeductionApplicantDTO) {
    this.contactRows.pipe(take(1)).subscribe(rows => {
      const indexToRemove = rows.indexOf(row);
      this.contactChange.emit(
        this.contacts.filter((_, i) => i !== indexToRemove)
      );
      this.form?.markAsDirty();
    });
  }

  editContact(row: HouseholdTaxDeductionApplicantDTO) {
    this.addRot(row);
  }

  addRot(rowToUpdate?: HouseholdTaxDeductionApplicantDTO) {
    const dialogData: HouseholdTaxDeductionApplicantDialogData = {
      title: this.terms['common.customer.customer.rot.register'],
      size: 'lg',
      rowToUpdate,
    };
    this.dialogService
      .open(HouseholdTaxDeductionDialogComponent, dialogData)
      .afterClosed()
      .pipe(filter(value => !!value))
      .subscribe((value: IHouseholdTaxDeductionApplicantDTO) => {
        if (rowToUpdate) {
          this.contactRows.pipe(take(1)).subscribe(rows => {
            const indexToUpdate = rows.indexOf(rowToUpdate);
            this.contactChange.emit(
              this.contacts.map((item, i) =>
                i === indexToUpdate ? value : item
              )
            );
            this.form?.markAsDirty();
          });
        } else {
          this.contactChange.emit([...this.contacts, value]);
          this.form?.markAsDirty();
        }
      });
  }

  private setData(): void {
    this.contactRows.next(this.contacts);
    this.grid?.refreshCells();
  }
}
