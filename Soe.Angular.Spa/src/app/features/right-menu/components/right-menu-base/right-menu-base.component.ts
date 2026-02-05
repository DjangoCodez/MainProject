import { Component, Input, SimpleChanges } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import {
  AngularJsLegacyType,
  Feature,
  SoeModule,
} from '@shared/models/generated-interfaces/Enumerations';
import { BrowserUtil } from '@shared/util/browser-util';
import { take } from 'rxjs/operators';

@Component({
    // eslint-disable-next-line @angular-eslint/component-selector
    selector: 'right-menu-base',
    templateUrl: './right-menu-base.component.html',
    styleUrls: ['./right-menu-base.component.scss'],
    standalone: false
})
export class RightMenuBaseComponent {
  @Input() positionIndex = 0;

  topPosition = 0;
  toggleTooltip = '';

  showMenu = false;
  fullscreen = false;

  constructor(protected translate: TranslateService) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes.positionIndex) this.setTopPosition();
  }

  private setTopPosition() {
    this.topPosition = 36 * (this.positionIndex + 1);
  }

  setToggleTooltip(translationKey: string) {
    this.translate
      .get(translationKey)
      .pipe(take(1))
      .subscribe(term => (this.toggleTooltip = term));
  }

  protected toggleShowMenu() {
    this.showMenu = !this.showMenu;
    this.triggerResize();
  }

  protected toggleFullscreen() {
    this.fullscreen = !this.fullscreen;
    this.triggerResize();
  }

  protected triggerResize() {
    // let eventObject = jQuery.Event("resize");
    //     $(window).trigger(eventObject);
  }

  openLegacyMenu(type: AngularJsLegacyType, data?: any) {
    const module: SoeModule = window.softOneSpa?.module;
    const feature: Feature = window.softOneSpa?.feature;

    let moduleName: string | undefined = undefined;
    switch (module) {
      case SoeModule.Manage:
        moduleName = 'manage';
        break;
      case SoeModule.Billing:
        moduleName = 'billing';
        break;
      case SoeModule.Economy:
        moduleName = 'economy';
        break;
      case SoeModule.Time:
        moduleName = 'time';
        break;
    }

    if (moduleName) {
      const win = BrowserUtil.openInNewTab(window, `/soe/${moduleName}/legacy`);

      win.ajsLegacy = {
        type: type,
        module: module,
        feature: feature,
        data: data,
      };
    }
  }
}
