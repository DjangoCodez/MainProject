import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { RightMenuBaseComponent } from '../right-menu-base/right-menu-base.component';
import { AngularJsLegacyType } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, mergeMap, take, timer } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { InformationService } from '@shared/services/information.service';
import { DateUtil } from '@shared/util/date-util';
import { StorageService } from '@shared/services/storage.service';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'information-menu',
  templateUrl: './information-menu.component.html',
  styleUrls: ['./information-menu.component.scss'],
  standalone: false,
})
export class InformationMenuComponent
  extends RightMenuBaseComponent
  implements OnInit
{
  readonly translate = inject(TranslateService);
  readonly storageService = inject(StorageService);
  readonly informationService = inject(InformationService);
  destroyRef = inject(DestroyRef);

  nbrOfUnreadInformations = 0;
  hasSevereUnreadInformation = false;

  ngOnInit() {
    // Call service every 10 minutes
    timer(0, 60 * 1000 * 10)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        mergeMap(() => this.loadNbrOfUnreadInformations(true))
      )
      .subscribe();
  }

  private hasNewInformations(useCache: boolean): Observable<boolean> {
    return new Observable<boolean>(observer => {
      if (useCache) {
        // Get last time checked from local storage
        let time = this.storageService.get('hasNewInformations');
        if (!time) time = DateUtil.defaultDateTime().toDateTimeString();
        this.informationService
          .hasNewInformations(time)
          .pipe(take(1))
          .subscribe(result => {
            // Update last time checked in local storage
            this.storageService.set(
              'hasNewInformations',
              new Date().toDateTimeString()
            );
            observer.next(result);
          });
      } else {
        // Force check
        observer.next(true);
      }
    });
  }

  private loadNbrOfUnreadInformations(useCache: boolean): Observable<any> {
    const ret$ = new Observable<boolean>(observer => {
      this.hasNewInformations(useCache)
        .pipe(take(1))
        .subscribe(hasNewInformation => {
          // If new information exists, do not use cache to get number of unread informations
          this.informationService
            .getNbrOfUnreadInformations(!hasNewInformation)
            .pipe(take(1))
            .subscribe(data => {
              this.nbrOfUnreadInformations = data;

              // Set default tooltip
              this.setToggleTooltip('core.informationmenu.title');
              // If unread messages, add number to tooltip
              if (this.nbrOfUnreadInformations > 0) {
                this.checkSevereUnreadInformation(!hasNewInformation);
                const term = this.translate.instant(
                  'core.informationmenu.unread'
                );
                this.toggleTooltip += ` (${
                  this.nbrOfUnreadInformations
                } ${term.toLocaleLowerCase()})`;
              }
              observer.next(hasNewInformation);
            });
        });
    });
    return ret$;
  }

  private checkSevereUnreadInformation(useCache: boolean) {
    this.informationService
      .hasSevereUnreadInformation(useCache)
      .pipe(take(1))
      .subscribe(data => {
        this.hasSevereUnreadInformation = data;
        // if (this.hasSevereUnreadInformation && !this.showMenu)
        //   this.toggleShowMenu();
      });
  }

  openLegacyMenu(): void {
    super.openLegacyMenu(AngularJsLegacyType.RightMenu_Information);
  }
}
