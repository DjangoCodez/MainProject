import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { RightMenuBaseComponent } from '../right-menu-base/right-menu-base.component';
import { AngularJsLegacyType } from '@shared/models/generated-interfaces/Enumerations';
import { CommunicationService } from '@shared/services/communication.service';
import { mergeMap, timer } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
    // eslint-disable-next-line @angular-eslint/component-selector
    selector: 'message-menu',
    templateUrl: './message-menu.component.html',
    styleUrls: ['./message-menu.component.scss'],
    standalone: false
})
export class MessageMenuComponent
  extends RightMenuBaseComponent
  implements OnInit
{
  readonly translate = inject(TranslateService);
  readonly communicationService = inject(CommunicationService);
  destroyRef = inject(DestroyRef);

  nbrOfUnreadMessages = 0;

  ngOnInit() {
    this.loadNbrOfUnreadMessages();
  }

  private loadNbrOfUnreadMessages() {
    // Call service every 10 minutes
    timer(0, 60 * 1000 * 10)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        mergeMap(() => this.communicationService.getNbrOfUnreadMessages())
      )
      .subscribe(data => {
        this.nbrOfUnreadMessages = data;

        // Set default tooltip
        this.setToggleTooltip('common.messages.messages');
        // If unread messages, add number to tooltip
        if (this.nbrOfUnreadMessages > 0) {
          const term = this.translate.instant('common.messages.unread');
          this.toggleTooltip += ` (${
            this.nbrOfUnreadMessages
          } ${term.toLocaleLowerCase()})`;
        }
      });
  }

  openLegacyMenu(): void {
    super.openLegacyMenu(AngularJsLegacyType.RightMenu_Message);
  }
}
