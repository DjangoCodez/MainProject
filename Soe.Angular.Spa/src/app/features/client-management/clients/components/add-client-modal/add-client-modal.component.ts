import {
  Component,
  computed,
  inject,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { ButtonComponent } from '@ui/button/button/button.component';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';
import { CommonModule } from '@angular/common';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { AddClientForm } from '../../models/add-client.form';
import { ReactiveFormsModule } from '@angular/forms';
import { ClientsService } from '../../services/clients.service';
import { interval, Subject, switchMap, takeUntil, filter } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { IconModule } from '@ui/icon/icon.module';
import { ClipBoardService } from '@shared/services/clip-board.service';
import { formatDistanceToNow } from 'date-fns';
import { DateUtil } from '@shared/util/date-util';

export interface AddClientModalData extends DialogData {
  connectionRequest: AddClientForm;
}

@Component({
  selector: 'soe-cm-add-client-modal',
  standalone: true,
  imports: [
    CommonModule,
    DialogComponent,
    TextboxComponent,
    ButtonComponent,
    InstructionComponent,
    IconButtonComponent,
    ReactiveFormsModule,
    TranslatePipe,
    IconModule,
  ],
  templateUrl: './add-client-modal.component.html',
})
export class AddClientModalComponent
  extends DialogComponent<AddClientModalData>
  implements OnInit, OnDestroy
{
  private readonly clientsService = inject(ClientsService);
  private readonly clipboardService = inject(ClipBoardService);
  private readonly destroy$ = new Subject<void>();

  // Polling state
  isPolling = signal(false);
  isConnected = signal(false);
  connectedCompanyName = signal<string | undefined>(undefined);

  // Trigger for periodic expiration text refresh
  private readonly refreshTrigger = signal(0);
  get connectionRequest() {
    return this.data.connectionRequest;
  }

  // Expiration display
  expirationText = computed(() => {
    // Reference refreshTrigger to force recalculation
    this.refreshTrigger();

    const expiresAt = this.connectionRequest.expiresAtUTC;
    if (!expiresAt) return '';

    try {
      return formatDistanceToNow(this.connectionRequest.expiresAtUTC, {
        addSuffix: true,
        locale: DateUtil.getLocale(),
      });
    } catch (error) {
      return '';
    }
  });

  ngOnInit(): void {
    this.startPollingAfterDelay();
    this.startExpirationRefreshTimer();
  }

  private startExpirationRefreshTimer(): void {
    // Refresh expiration text every 5 seconds
    interval(5_000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.refreshTrigger.update(val => val + 1);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private startPollingAfterDelay(): void {
    this.isPolling.set(true);
    // Poll every 5 seconds
    interval(5_000)
      .pipe(
        takeUntil(this.destroy$),
        switchMap(() =>
          this.clientsService.checkConnectionRequestStatus(
            this.connectionRequest.connectionRequestId
          )
        ),
        filter(response => response !== null) // Filter out null responses
      )
      .subscribe(value => {
        this.isConnected.set(true);
        this.isPolling.set(false);
        this.connectedCompanyName.set(value.linkedCompanyName);
        // Stop polling
        this.destroy$.next();
      });
  }

  copyCode(): void {
    this.clipboardService.writeToClipboard(this.connectionRequest.code);
  }

  protected ok(): void {
    this.dialogRef.close(this.isConnected());
  }
}
