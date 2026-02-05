import {
  AfterViewInit,
  Component,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ContactPersonsService } from '@shared/components/contact-persons/services/contact-persons.service';
import { ContactPersonDTO } from '@shared/components/contact-persons/models/contact-persons.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ExportUtil } from '@shared/util/export-util';
import { clearAndSetFormArray } from '@shared/util/form-util';
import { ContactPersonForm } from '@shared/components/contact-persons/models/contact-persons-form.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CategoryItem } from '@shared/components/categories/categories.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-contact-persons-edit',
  templateUrl: './contact-persons-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ContactPersonsEditComponent
  extends EditBaseDirective<
    ContactPersonDTO,
    ContactPersonsService,
    ContactPersonForm
  >
  implements OnInit, AfterViewInit
{
  service = inject(ContactPersonsService);
  coreService = inject(CoreService);
  position: SmallGenericType[] = [];

  get contactPersonId() {
    return this.form?.getIdControl()?.value;
  }

  private toolbarDownloadDisabled = signal(false);

  ngAfterViewInit(): void {
    this.enableConsentDate(this.form?.value.hasConsent || false);
  }

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Manage_ContactPersons_Edit, {
      lookups: [this.loadPositions()],
    });

    this.setDownloadDisabled(this.form?.isNew || this.form?.isCopy || false);

    this.form?.actorContactPersonId.valueChanges.subscribe(value => {
      if (value && value > 0) this.setDownloadDisabled(false);
      else this.setDownloadDisabled(true);
    });
  }

  override createEditToolbar(): void {
    if (!this.form?.openInDialog) {
      super.createEditToolbar({ hideCopy: true });

      this.toolbarService.createItemGroup({
        items: [
          this.toolbarService.createToolbarButton('download', {
            iconName: signal('download'),
            tooltip: signal('common.download'),
            disabled: this.toolbarDownloadDisabled,
            onAction: () => this.handleToolbarAction(),
          }),
        ],
      });
    }
  }

  private setDownloadDisabled(disabled: boolean) {
    this.toolbarDownloadDisabled.set(disabled);
  }

  handleToolbarAction() {
    this.onToolbarButtonClick().subscribe();
  }

  onToolbarButtonClick(): Observable<unknown> {
    return this.performLoadData.load$(
      this.service.getContactPersonForExport(this.contactPersonId).pipe(
        tap(contactPerson => {
          if (contactPerson) {
            ExportUtil.Export(contactPerson, 'contactperson.json');
          }
        })
      )
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.contactPersonId).pipe(
        tap(value => {
          this.form?.customPatchValue(value);
          this.enableConsentDate(value.hasConsent);
        })
      )
    );
  }

  loadPositions(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.ContactPersonPosition,
        true,
        false,
        false,
        true
      )
      .pipe(tap(x => (this.position = x)));
  }

  categoryChanged(selectedCategoryIds: CategoryItem[]) {
    clearAndSetFormArray(
      selectedCategoryIds.map(c => c.categoryId),
      this.form!.categoryIds,
      true
    );
  }

  consentChange(hasConsent: boolean) {
    if (hasConsent && !this.form?.value.consentDate) {
      this.form?.patchValue({
        consentDate: new Date(),
      });
    }
    this.enableConsentDate(hasConsent);
  }

  enableConsentDate(hasConsent: boolean) {
    const control = this.form?.consentDate;
    if (!control) return;

    hasConsent ? control.enable() : control.disable();
  }
}
