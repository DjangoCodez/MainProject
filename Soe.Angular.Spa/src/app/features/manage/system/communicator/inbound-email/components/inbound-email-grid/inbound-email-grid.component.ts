import { InboundEmailDialogData } from './../../models/inbound-email-dialog-data.model';
import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  IIncomingEmailFilterDTO,
  IIncomingEmailGridDTO,
} from '@shared/models/generated-interfaces/IncomingEmailDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { InboundEmailService } from '../../services/inbound-email.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { InboundEmailDetailDialogComponent } from '../inbound-email-detail-dialog/inbound-email-detail-dialog.component';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

@Component({
  selector: 'soe-inbound-email-grid',
  templateUrl: './inbound-email-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class InboundEmailGridComponent
  extends GridBaseDirective<IIncomingEmailGridDTO, InboundEmailService>
  implements OnInit
{
  readonly service = inject(InboundEmailService);
  private readonly dialogService = inject(DialogService);
  private readonly performLoadEmails = new Perform<IIncomingEmailGridDTO[]>(
    this.progressService
  );
  protected deliveryStatuses: ISmallGenericType[] = [];
  protected isSupportAdmin = signal(SoeConfigUtil.isSupportAdmin);
  private filter?: IIncomingEmailFilterDTO;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Manage_System_Communicator,
      'Manage.System.Communicator.InboundEmail',
      {
        skipInitialLoad: true,
        lookups: [this.loadDeliveryStatusOptions()],
      }
    );
  }

  loadDeliveryStatusOptions(): Observable<ISmallGenericType[]> {
    return this.performLoadData.load$(
      this.service.getDeliveryStatusOptions().pipe(
        tap((options: ISmallGenericType[]) => {
          this.deliveryStatuses = options;
        })
      )
    );
  }

  onGridReadyToDefine(grid: GridComponent<IIncomingEmailGridDTO>): void {
    super.onGridReadyToDefine(grid);

    this.grid.addColumnDate('date', 'Date', { width: 85 });
    this.grid.addColumnTime('date', 'Time', {
      width: 75,
      showSeconds: true,
      dateFormat: 'HH:mm:ss',
      alignLeft: true,
    });
    this.grid.addColumnText('senderEmail', 'Sender', { flex: 20 });
    this.grid.addColumnText('recipientEmails', 'Recipient', { flex: 40 });
    this.grid.addColumnText('attachementNames', 'Attachments', { flex: 20 });
    this.grid.addColumnSelect(
      'deliveryStatus',
      'Status',
      this.deliveryStatuses,
      undefined,
      {
        flex: 20,
        editable: false,
        cellClassRules: {
          'success-background-color': row =>
            !!(
              row.data?.deliveryStatus &&
              row.data?.deliveryStatus >= 10 &&
              row.data?.deliveryStatus < 20
            ),
          'error-background-color': row =>
            !!(
              row.data?.deliveryStatus &&
              row.data?.deliveryStatus >= 20 &&
              row.data?.deliveryStatus < 30
            ),
        },
      }
    );
    this.grid.addColumnIcon(null, '', {
      iconName: 'magnifying-glass',
      pinned: 'right',
      onClick: (row: IIncomingEmailGridDTO): void => {
        this.showEmailDetails(row);
      },
    });

    super.finalizeInitGrid();
  }

  protected triggerSearchEmails(_filter: IIncomingEmailFilterDTO): void {
    this.filter = _filter;

    this.loadEmails();
  }

  override loadData(): Observable<IIncomingEmailGridDTO[]> {
    return of([]).pipe(
      tap(() => {
        this.loadEmails();
      })
    );
  }

  private loadEmails(): void {
    if (this.filter) {
      this.performLoadEmails.load(
        this.service.getGrid(undefined, { filter: this.filter }).pipe(
          tap(emails => {
            this.grid.setData(emails);
          })
        )
      );
    } else {
      this.grid.setData([]);
    }
  }

  override edit(row: IIncomingEmailGridDTO): void {
    this.showEmailDetails(row);
  }

  private showEmailDetails(email: IIncomingEmailGridDTO): void {
    this.dialogService.open(InboundEmailDetailDialogComponent, <
      InboundEmailDialogData
    >{
      size: 'lg',
      inboundEmailId: email.incomingEmailId,
      title: 'Inbound Email',
    });
  }
}
