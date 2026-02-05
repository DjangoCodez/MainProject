import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ISoeBankerOnboardingDTO } from '@shared/models/generated-interfaces/BankIntegrationDTOs';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { finalize, take } from 'rxjs/operators';
import { BankintegrationOnboardingService } from '../../services/bankintegration-onboarding.service';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { ToolbarGridConfig } from '@ui/toolbar/models/toolbar';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-bankintegration-onboarding-grid',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  imports: [GridWrapperComponent],
  providers: [FlowHandlerService, ToolbarService],
})
export class BankintegrationOnboardingGridComponent
  extends GridBaseDirective<
    ISoeBankerOnboardingDTO,
    BankintegrationOnboardingService
  >
  implements OnInit
{
  service = inject(BankintegrationOnboardingService);
  messageBoxService = inject(MessageboxService);

  selectedRows = signal<ISoeBankerOnboardingDTO[]>([]);
  sendingAcknowledgement = signal(false);
  disableAcknowledgementButton = computed(
    () => this.selectedRows().length === 0 || this.sendingAcknowledgement()
  );

  ngOnInit(): void {
    this.startFlow(
      Feature.Manage_System_BankIntegration,
      'Manage.System.BankIntegration.Onboarding'
    );
  }

  override createGridToolbar(config?: Partial<ToolbarGridConfig>): void {
    super.createGridToolbar();
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton(
          'billing.invoices.markup.newcustomerdiscount',
          {
            iconName: signal('cloud-check'),
            caption: signal(
              'manage.system.bankintegration.onboarding.sendacknowledgement'
            ),
            tooltip: signal(
              'manage.system.bankintegration.onboarding.sendacknowledgementinfo'
            ),
            disabled: this.disableAcknowledgementButton,
            onAction: () => this.sendAcknowledgement(),
          }
        ),
      ],
    });
  }

  onGridReadyToDefine(grid: GridComponent<ISoeBankerOnboardingDTO>) {
    super.onGridReadyToDefine(grid);
    grid.selectionChanged.subscribe(rows => {
      this.selectedRows.set(rows);
    });

    this.translate
      .get([
        'manage.system.syscompany.sysbank',
        'manage.system.bankintegration.onboarding.signingtype',
        'common.status',
        'common.message',
        'common.created',
        'common.modified',
        'common.type',
        'common.accounts',
        'common.orgnrshort',
        'common.company',
        'common.email',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'bankName',
          terms['manage.system.syscompany.sysbank'],
          {
            flex: 90,
            tooltipField: 'bankName',
          }
        );
        this.grid.addColumnText('companyName', terms['common.company'], {
          flex: 30,
        });
        this.grid.addColumnText('companyOrgNr', terms['common.orgnrshort'], {
          flex: 30,
        });
        this.grid.addColumnText('companyMasterOrgNr', 'Company Master OrgNr', {
          flex: 30,
        });
        this.grid.addColumnText('bankAccounts', terms['common.accounts'], {
          flex: 90,
          tooltipField: 'bankAccounts',
        });
        this.grid.addColumnText('emails', terms['common.email'], {
          flex: 90,
          tooltipField: 'emails',
        });
        this.grid.addColumnText('regAction', 'Reg action', {
          minWidth: 30,
          maxWidth: 60,
        });
        this.grid.addColumnText('status', terms['common.status'], {
          minWidth: 30,
          maxWidth: 60,
        });
        this.grid.addColumnText(
          'signingTypeName',
          terms['manage.system.bankintegration.onboarding.signingtype'],
          {
            flex: 90,
            tooltipField: 'signingTypeName',
          }
        );
        this.grid.addColumnDateTime('created', terms['common.created'], {
          minWidth: 10,
          maxWidth: 120,
        });
        this.grid.addColumnDateTime('modified', terms['common.modified'], {
          minWidth: 10,
          maxWidth: 120,
        });

        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { type: number }
  ): Observable<ISoeBankerOnboardingDTO[]> {
    return super.loadData(id, { type: 10 });
  }

  sendAcknowledgement(): void {
    this.sendingAcknowledgement.set(true);
    const ids = this.selectedRows().map(r => r.onBoardingRequestId);
    this.service
      .sendAcknowledgement(ids)
      .pipe(
        take(1),
        finalize(() => this.sendingAcknowledgement.set(false))
      )
      .subscribe(res => {
        if (res.success) {
          this.messageBoxService.success(
            '',
            'manage.system.bankintegration.onboarding.acknowledgementsent'
          );
          this.refreshGrid();
        } else {
          this.messageBoxService.error(
            'core.error',
            ResponseUtil.getErrorMessage(res) ?? ''
          );
        }
      });
  }
}
