import { Component, OnInit, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ContactPersonGridDTO } from '@shared/components/contact-persons/models/contact-persons.model';
import { ContactPersonsService } from '@shared/components/contact-persons/services/contact-persons.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ExportUtil } from '@shared/util/export-util';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { of } from 'rxjs';
import { take, tap } from 'rxjs/operators';

@Component({
  selector: 'soe-contact-persons-grid',
  templateUrl: './contact-persons-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ContactPersonsGridComponent
  extends GridBaseDirective<ContactPersonGridDTO, ContactPersonsService>
  implements OnInit
{
  service = inject(ContactPersonsService);
  messageboxService = inject(MessageboxService);
  performLoad = new Perform<unknown>(this.progressService);
  performAction = new Perform<ContactPersonsService>(this.progressService);

  private toolbarDeleteDisabled = signal(true);

  constructor(
    private translationService: TranslateService,
    public flowHandler: FlowHandlerService
  ) {
    super();

    this.startFlow(Feature.Manage_ContactPersons, 'Manage.ContactPersons');
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('delete', {
          iconName: signal('user-slash'),
          caption: signal('core.delete'),
          tooltip: signal('core.delete'),
          disabled: this.toolbarDeleteDisabled,
          onAction: () => this.onToolbarButtonClick(),
        }),
      ],
    });
  }

  private setDeleteDisabled(disabled: boolean) {
    this.toolbarDeleteDisabled.set(disabled);
  }

  onToolbarButtonClick(): void {
    const mb = this.messageboxService.warning(
      'core.verifyquestion',
      'common.contactperson.deletequestion'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.bulkDelete();
    });
  }

  override onGridReadyToDefine(grid: GridComponent<ContactPersonGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.grid.onSelectionChanged = this.selectionChanged.bind(this);

    this.translationService
      .get([
        'common.active',
        'core.yes',
        'core.no',
        'common.firstname',
        'common.lastname',
        'common.emailaddress',
        'common.telephonenumber',
        'common.contactperson.hasconsent',
        'common.contactperson.consentdate',
        'common.contactperson.suppliernumber',
        'common.contactperson.suppliername',
        'common.report.selection.customernr',
        'common.report.selection.customername',
        'common.contactperson.position',
        'common.categories.category',
        'core.edit',
        'common.download',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText('firstName', terms['common.firstname'], {
          flex: 10,
        });
        this.grid.addColumnText('lastName', terms['common.lastname'], {
          flex: 10,
        });
        this.grid.addColumnText('email', terms['common.emailaddress'], {
          flex: 6,
        });
        this.grid.addColumnText(
          'phoneNumber',
          terms['common.telephonenumber'],
          {
            flex: 6,
          }
        );
        this.grid.addColumnSelect(
          'hasConsentId',
          terms['common.contactperson.hasconsent'],
          [
            { id: 0, name: terms['core.no'] },
            { id: 1, name: terms['core.yes'] },
          ],
          null,
          {
            flex: 6,
            enableGrouping: true,
          }
        );
        this.grid.addColumnDate(
          'consentDate',
          terms['common.contactperson.consentdate'],
          {
            flex: 6,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'supplierNr',
          terms['common.contactperson.suppliernumber'],
          {
            flex: 6,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'supplierName',
          terms['common.contactperson.suppliername'],
          {
            flex: 6,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'customerNr',
          terms['common.report.selection.customernr'],
          {
            flex: 6,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'customerName',
          terms['common.report.selection.customername'],
          {
            flex: 10,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'positionName',
          terms['common.contactperson.position'],
          {
            flex: 10,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'categoryString',
          terms['common.categories.category'],
          {
            flex: 10,
            enableGrouping: true,
          }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        this.grid.addColumnIcon(null, '', {
          width: 22,
          iconName: 'download',
          pinned: 'right',
          tooltip: terms['common.download'],
          enableHiding: false,
          onClick: row => {
            this.downloadFile(row);
          },
        });

        this.grid.useGrouping({
          includeTotalFooter: false,
          includeFooter: false,
          keepColumnsAfterGroup: false,
          selectChildren: true,
        });

        super.finalizeInitGrid();
      });
  }

  selectionChanged() {
    this.setDeleteDisabled(this.grid.getSelectedRows().length == 0);
  }

  bulkDelete() {
    this.performAction.crud(
      CrudActionTypeEnum.Delete,
      this.service
        .deleteContactPersons(this.grid.getSelectedIds('actorContactPersonId'))
        .pipe(
          tap(result => {
            if (result.success) {
              this.performLoad.load(
                this.service.getGrid().pipe(
                  tap(x => {
                    this.grid.setData(x);
                  })
                )
              );
              this.setDeleteDisabled(true);
            }
          })
        )
    );
  }

  downloadFile(row: ContactPersonGridDTO) {
    return of(
      this.performLoad.load(
        this.service.getContactPersonForExport(row.actorContactPersonId).pipe(
          tap(contactPerson => {
            if (contactPerson) {
              ExportUtil.Export(contactPerson, 'contactperson.json');
            }
          })
        )
      )
    );
  }
}
